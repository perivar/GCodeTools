/**
 * Copyright (c) David-John Miller AKA Anoyomouse 2014
 *
 * See LICENCE in the project directory for licence information
 * Heaviliy Modified by perivar@nerseth.com
 **/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Globalization;
using GCode;
using SVG;

namespace GCodePlotter
{
	public partial class frmPlotter : Form
	{
		NumberStyles style = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
		CultureInfo culture = CultureInfo.InvariantCulture;
		
		const float DEFAULT_MULTIPLIER = 4.0f;

		float ZOOMFACTOR = 1.25f;   // = 25% smaller or larger
		int MINMAX = 8;             // Times bigger or smaller than the ctrl
		
		float scale = 1.0f;
		float multiplier = DEFAULT_MULTIPLIER;

		// calculated total min and max sizes
		float maxX = 0.0f;
		float maxY = 0.0f;
		float maxZ = 0.0f;
		float minX = 0.0f;
		float minY = 0.0f;
		float minZ = 0.0f;
		
		// margins to use within the gcode viewer
		const int LEFT_MARGIN = 20;
		const int BOTTOM_MARGIN = 20;
		
		List<GCodeInstruction> parsedInstructions = null;

		bool bDataLoaded = false;
		
		List<Block> myBlocks;

		Image renderImage = null;
		
		Point MouseDownLocation;

		public frmPlotter()
		{
			InitializeComponent();
		}
		
		DialogResult AskToLoadData()
		{
			//return MessageBox.Show("Doing this will load/reload data, are you sure you want to load this data deleting your old data?", "Question!", MessageBoxButtons.YesNo);
			return DialogResult.OK;
		}

		#region Events
		void frmPlotterLoad(object sender, EventArgs e)
		{
			bDataLoaded = false;

			var lastFile = QuickSettings.Get["LastOpenedFile"];
			if (!string.IsNullOrWhiteSpace(lastFile))
			{
				// Load data here!
				var fileInfo = new FileInfo(lastFile);
				if (fileInfo.Exists)
				{
					txtFile.Text = fileInfo.Name;
					txtFile.Tag = fileInfo.FullName;
					this.Text = fileInfo.Name;
					
					Application.DoEvents();
					ParseData();
				}
			}
			
			this.pictureBox1.MouseWheel += OnMouseWheel;
		}

		void frmPlotterResizeEnd(object sender, EventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized)
			{
				return;
			}
			
			// calculate optimal multiplier
			CalculateOptimalZoomMultiplier();
			
			RenderBlocks();
		}

		void TreeViewAfterSelect(object sender, TreeViewEventArgs e)
		{
			RenderBlocks();
			pictureBox1.Refresh();
		}

		void TreeViewMouseDown(object sender, MouseEventArgs e)
		{
			var me = (MouseEventArgs) e;
			if (me.Button == MouseButtons.Right) {
				treeView.SelectedNode = null;
				RenderBlocks();
			}
		}

		void btnLoadClick(object sender, EventArgs e)
		{
			if (AskToLoadData() == DialogResult.No) {
				return;
			}

			var result = ofdLoadDialog.ShowDialog();
			if (result == DialogResult.OK) {
				
				var fileInfo = new FileInfo(ofdLoadDialog.FileName);
				if (!fileInfo.Exists) {
					MessageBox.Show("Selected file does not exist, please select an existing file!");
					return;
				}

				QuickSettings.Get["LastOpenedFile"] = fileInfo.FullName;

				txtFile.Text = fileInfo.Name;
				txtFile.Tag = fileInfo.FullName;
				this.Text = fileInfo.Name;

				string data = File.ReadAllText(fileInfo.FullName);
				
				// reset multiplier
				multiplier = DEFAULT_MULTIPLIER;

				ParseText(data);
			}
		}

		void ParseData() {
			if (bDataLoaded) {
				if (AskToLoadData() == DialogResult.No) {
					return;
				}
			}

			if (txtFile.Tag != null) {
				var fileInfo = new FileInfo(txtFile.Tag.ToString());
				if (fileInfo.Extension.ToLower().Equals(".svg")) {
					var svg = SVGDocument.LoadFromFile(fileInfo.FullName);
					var contours = svg.GetScaledContours();
					//var contours = svg.GetContours();
					float zSafeHeight = GetZSafeHeight();
					float zDepth = GetZDepth();
					float feedRateRapid = GetFeedRateRapidMoves();
					float feedRatePlunge = GetFeedRatePlungeMoves();
					string data = "";
					if (radSVGCenter.Checked) {
						data = GCodeUtils.GetGCodeCenter(contours, zDepth, feedRateRapid, feedRatePlunge, zSafeHeight);
					} else {
						data = GCodeUtils.GetGCode(contours, zDepth, feedRateRapid, feedRatePlunge, zSafeHeight);
					}
					ParseText(data);
				} else {
					string data = File.ReadAllText(fileInfo.FullName);
					ParseText(data);
				}
			}
		}
		
		void btnSplitClick(object sender, EventArgs e)
		{
			if (radLeft.Checked) {
				ResetSplit(0);
			} else {
				ResetSplit(1);
			}
		}
		
		void btnSaveClick(object sender, EventArgs e)
		{
			SaveGCodes(false);
		}

		void btnSaveLayersClick(object sender, EventArgs e)
		{
			using (var options = new frmOptions()) {
				var result = options.ShowDialog();
				if (result == DialogResult.OK) {
					SaveGCodes(true);
				}
			}
		}
		
		void btnSaveSplitClick(object sender, EventArgs e)
		{
			if (parsedInstructions == null) {
				MessageBox.Show("No file loaded!");
				return;
			}
			
			if ("".Equals(txtSplit.Text)) {
				MessageBox.Show("No split value entered!");
				return;
			}

			float xSplit = 0.0f;
			if (float.TryParse(txtSplit.Text, style, culture, out xSplit)) {
				
				ParseData();
				
				var splitPoint = new Point3D(xSplit, 0, 0);
				
				float zClearance = GetZSafeHeight();
				
				var split = GCodeSplitter.Split(parsedInstructions, splitPoint, 0.0f, zClearance);
				
				SaveSplittedGCodes(split, splitPoint, (string)txtFile.Tag);
				
				MessageBox.Show("Saved splitted files to same directory as loaded file.\nExtensions are _first.gcode and _second.gcode!");
			}
		}
		
		void radScaleChange(object sender, EventArgs e)
		{
			RenderBlocks();
		}

		void cbRenderG0CheckedChanged(object sender, EventArgs e)
		{
			if (renderImage == null) {
				return;
			}

			RenderBlocks();
		}
		
		void cbSoloSelectCheckedChanged(object sender, EventArgs e)
		{
			if (renderImage == null) {
				return;
			}

			RenderBlocks();
		}
		
		void PictureBox1MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
			{
				// store mouse down for mouse drag support
				// i.e. change scroll bar position based when dragging
				MouseDownLocation = e.Location;
			}
		}
		
		void PictureBox1MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
			{
				// change scroll bar position based when dragging
				var changePoint = new Point(e.Location.X - MouseDownLocation.X,
				                            e.Location.Y - MouseDownLocation.Y);
				
				panelViewer.AutoScrollPosition = new Point(-panelViewer.AutoScrollPosition.X - changePoint.X,
				                                           -panelViewer.AutoScrollPosition.Y - changePoint.Y);
				
				// Since we are only drawing the visable rectange of the picturebox
				// we have to ensure we update the drawing when we scroll
				RenderBlocks();
			}
			
			// output scaled coordinates
			float x = (e.X - LEFT_MARGIN) / scale * 10;
			float y = (pictureBox1.Height - e.Y - BOTTOM_MARGIN) / scale * 10;
			
			txtCoordinates.Text = string.Format(CultureInfo.InvariantCulture, "X: {0:0.##}, Y: {1:0.##}", x, y);
		}

		void OnMouseWheel(object sender, MouseEventArgs mea) {
			
			// http://stackoverflow.com/questions/10694397/how-to-zoom-in-using-mouse-position-on-that-image
			
			if (mea.Delta < 0) {
				ZoomIn(mea.Location);
			} else {
				ZoomOut(mea.Location);
			}
			
			// set handled to true to disable scrolling the scrollbars using the mousewheel
			((HandledMouseEventArgs)mea).Handled = true;
		}
		
		void BtnOptimizeClick(object sender, EventArgs e)
		{
			//var points = DataProvider.GetPoints(@"JavaScript\data.js", "data200");
			var gcodeSplitObject = GCodeUtils.SplitGCodeInstructions(parsedInstructions);
			var points = gcodeSplitObject.AllG0Sections.ToList<IPoint>();
			
			// TODO: add an origin at 0,0
			//points.Add(new Point3D(0, 0, 0));
			
			new GCodeOptimizer.MainForm(this, points, maxX, maxY).Show();
		}
		
		void PanelViewerScroll(object sender, ScrollEventArgs e)
		{
			// Since we are only drawing the visable rectange of the picturebox
			// we have to ensure we update the drawing when we scroll
			RenderBlocks();
		}
		#endregion
		
		#region Methods
		Rectangle GetVisibleRectangle(Control c)
		{
			Rectangle rect = c.RectangleToScreen(c.ClientRectangle);
			while (c != null)
			{
				rect = Rectangle.Intersect(rect, c.RectangleToScreen(c.ClientRectangle));
				c = c.Parent;
			}
			rect = pictureBox1.RectangleToClient(rect);
			return rect;
		}
		
		void ZoomIn(Point clickPoint) {

			if ((pictureBox1.Width < (MINMAX * panelViewer.Width)) &&
			    (pictureBox1.Height < (MINMAX * panelViewer.Height)))
			{
				// store the multiplier to be used for scrollbar setting later
				float oldMultiplier = multiplier;
				
				// zoom the multiplier
				multiplier *= ZOOMFACTOR;
				
				UpdateScrollbar(clickPoint, oldMultiplier);
				RenderBlocks();
			}
		}

		void ZoomOut(Point clickPoint) {

			if ((pictureBox1.Width > (panelViewer.Width / MINMAX)) &&
			    (pictureBox1.Height > (panelViewer.Height / MINMAX )))
			{
				// store the multiplier to be used for scrollbar setting later
				float oldMultiplier = multiplier;
				
				// zoom the multiplier
				multiplier /= ZOOMFACTOR;

				UpdateScrollbar(clickPoint, oldMultiplier);
				RenderBlocks();
			}
		}
		
		/// <summary>
		/// Update the Scrollbar
		/// </summary>
		/// <param name="clickPoint">position under the cursor which is to be retained</param>
		/// <param name="oldMultiplier">zoom factor between 0.1 and 8.0 before it was updated</param>
		void UpdateScrollbar(Point clickPoint, float oldMultiplier) {
			// http://vilipetek.com/2013/09/07/105/
			
			var scrollPosition = panelViewer.AutoScrollPosition;
			var cursorOffset = new PointF(clickPoint.X + scrollPosition.X,
			                              clickPoint.Y + scrollPosition.Y);
			
			// AutoScrollPosition is quite cumbersome.
			// usually you get negative values when doing this:
			// Point p = this.AutoScrollPosition;
			// but when setting the scroll position you have to use positive values
			// ... so to restore the exact same scroll position you have to invert the negative numbers:
			// this.AutoScrollPosition = new Point(-p.X, -p.Y)
			
			// Calculate the new scroll position
			var newScrollPosition = new Point(
				(int)Math.Round(multiplier * clickPoint.X / oldMultiplier) -
				(int)cursorOffset.X,
				(int)Math.Round(multiplier * clickPoint.Y / oldMultiplier) -
				(int)cursorOffset.Y );
			
			panelViewer.AutoScrollPosition = newScrollPosition;
		}
		
		/// <summary>
		/// Turn the list of instruction into a list of blocks
		/// where the blocks are separated if "cutting path id" is found
		/// and when a rapid move up is found
		/// </summary>
		/// <param name="instructions">list of gcode instructions</param>
		/// <returns>list of blocks</returns>
		static List<Block> GetBlocks(List<GCodeInstruction> instructions) {

			var blocks = new List<Block>();
			var currentPoint = Point3D.Empty;

			// convert instructions into splitted gcode instructions
			var gcodeSplitObject = GCodeUtils.SplitGCodeInstructions(instructions);

			if (gcodeSplitObject == null) return blocks;
			
			// first add header
			blocks.AddRange(GetBlockElements(gcodeSplitObject.PriorToFirstG0Section, "Top", ref currentPoint));

			// add main blocks
			blocks.AddRange(GetBlockElements(gcodeSplitObject.AllG0Sections, ref currentPoint));
			
			// last add footer
			blocks.AddRange(GetBlockElements(gcodeSplitObject.AfterLastG0Section, "Bottom", ref currentPoint));
			
			return blocks;
		}
		
		static List<Block> GetBlockElements(List<Point3DBlock> point3DBlocks, ref Point3D currentPoint) {
			
			int blockCounter = 1;
			var blocks = new List<Block>();
			
			foreach (var currentPoint3D in point3DBlocks) {
				
				var currentBlock = new Block();
				currentBlock.Name = "Block_" + blockCounter++;
				
				foreach (var currentInstruction in currentPoint3D.GCodeInstructions) {
					// this is where the block is put together and where the linepoints is added
					var linePointsCollection = currentInstruction.RenderCode(ref currentPoint);
					if (linePointsCollection != null) {
						currentInstruction.CachedLinePoints = linePointsCollection;
						currentBlock.PlotPoints.AddRange(linePointsCollection);
					}

					// make sure to store the actual instruction as well
					//if (currentInstruction.CanRender) {
					// TODO: should we add all commands?
					if (!currentInstruction.IsEmptyLine) {
						currentBlock.GCodeInstructions.Add(currentInstruction);
					}
				}
				
				blocks.Add(currentBlock);
			}
			
			return blocks;
		}
		
		static List<Block> GetBlockElements(List<GCodeInstruction> instructions, string name, ref Point3D currentPoint) {
			
			var blocks = new List<Block>();
			
			var currentBlock = new Block();
			currentBlock.Name = name;
			foreach (var currentInstruction in instructions) {
				// this is where the block is put together and where the linepoints is added
				var linePointsCollection = currentInstruction.RenderCode(ref currentPoint);
				if (linePointsCollection != null) {
					currentInstruction.CachedLinePoints = linePointsCollection;
					currentBlock.PlotPoints.AddRange(linePointsCollection);
				}

				// make sure to store the actual instruction as well
				//if (currentInstruction.CanRender) {
				// TODO: should we add all commands?
				if (!currentInstruction.IsEmptyLine) {
					currentBlock.GCodeInstructions.Add(currentInstruction);
				}
			}
			if (currentBlock.GCodeInstructions.Count > 0) blocks.Add(currentBlock);
			
			return blocks;
		}
		
		void ResetSplit(int index) {
			
			if (parsedInstructions == null) {
				MessageBox.Show("No file loaded!");
				return;
			}
			
			if ("".Equals(txtSplit.Text) || "0.0".Equals(txtSplit.Text)) {
				MessageBox.Show("Please enter a positive split value!");
				return;
			}

			float xSplit = 0.0f;
			if (float.TryParse(txtSplit.Text, style, culture, out xSplit)) {
				
				ParseData();
				
				var splitPoint = new Point3D(xSplit, 0, 0);
				
				float zClearance = GetZSafeHeight();
				
				var split = GCodeSplitter.Split(parsedInstructions, splitPoint, 0.0f, zClearance);
				
				// clean up the mess with too many G0 commands
				var cleaned = GCodeSplitter.CleanGCode(split[index]);
				
				var gcodeSplitted = Block.BuildGCodeOutput("Block_1", cleaned, false);
				ParseText(gcodeSplitted);
			}
		}
		
		Size GetDimensionsFromZoom() {

			// set scale variable
			scale = (10 * multiplier);

			// 10 mm per grid
			var width = (int)(maxX * scale + 1) / 10 + 2 * LEFT_MARGIN;
			var height = (int)(maxY * scale + 1) / 10 + 2 * BOTTOM_MARGIN;
			
			return new Size(width, height);
		}
		
		void GetEmptyImage() {
			
			var imageDimension = GetDimensionsFromZoom();
			int width = imageDimension.Width;
			int height = imageDimension.Height;
			
			// if anything has changed, reset image
			if (renderImage == null || width != renderImage.Width || height != renderImage.Height)
			{
				if (renderImage != null) {
					renderImage.Dispose();
				}

				renderImage = new Bitmap(width, height);
				pictureBox1.Width = width;
				pictureBox1.Height = height;
				
				try {
					pictureBox1.Image = renderImage;
				} catch (OutOfMemoryException) {
					// could draw a red cross like here:
					// http://stackoverflow.com/questions/22163846/zooming-of-an-image-using-mousewheel
				}
			}
		}

		void RenderBlocks()
		{
			GetEmptyImage();
			
			var graphics = Graphics.FromImage(renderImage);
			graphics.Clear(ColorHelper.GetColor(PenColorList.Background));
			
			// limit the drawing area
			graphics.Clip = new Region(GetVisibleRectangle(this.pictureBox1));
			
			// draw grid
			Pen gridPen = ColorHelper.GetPen(PenColorList.GridLines);
			for (var x = 0; x < pictureBox1.Width / scale; x++)
			{
				for (var y = 0; y < pictureBox1.Height / scale; y++)
				{
					graphics.DrawLine(gridPen, x * scale + LEFT_MARGIN, 0, x * scale + LEFT_MARGIN, pictureBox1.Height);
					graphics.DrawLine(gridPen, 0, pictureBox1.Height - (y * scale) - BOTTOM_MARGIN, pictureBox1.Width, pictureBox1.Height - (y * scale) - BOTTOM_MARGIN);
				}
			}

			// draw arrow grid
			using (var penZero = new Pen(Color.WhiteSmoke, 1)) {
				graphics.DrawLine(penZero, LEFT_MARGIN, pictureBox1.Height-BOTTOM_MARGIN, pictureBox1.Width, pictureBox1.Height-BOTTOM_MARGIN);
				graphics.DrawLine(penZero, LEFT_MARGIN, 0, LEFT_MARGIN, pictureBox1.Height-BOTTOM_MARGIN);
			}
			using (var penX = new Pen(Color.Red, 3)) {
				penX.StartCap= LineCap.Flat;
				penX.EndCap = LineCap.ArrowAnchor;
				graphics.DrawLine(penX, LEFT_MARGIN, pictureBox1.Height-BOTTOM_MARGIN, 5 * scale + LEFT_MARGIN, pictureBox1.Height-BOTTOM_MARGIN);
			}
			using (var penY = new Pen(Color.Green, 3)) {
				penY.StartCap = LineCap.ArrowAnchor;
				penY.EndCap = LineCap.Flat;
				graphics.DrawLine(penY, LEFT_MARGIN, pictureBox1.Height - (5 * scale) - BOTTOM_MARGIN, LEFT_MARGIN, pictureBox1.Height-BOTTOM_MARGIN);
			}

			// draw gcode
			if (myBlocks != null && myBlocks.Count > 0)
			{
				if (treeView.SelectedNode != null
				    && treeView.SelectedNode.Level == 1) {
					// sub-level, i.e. the instruction level

					var selectedInstruction = (GCodeInstruction) treeView.SelectedNode.Tag;
					
					// find what block this instruction is a part of
					var parentBlock = (Block) treeView.SelectedNode.Parent.Tag;
					
					foreach (var instruction in parentBlock.GCodeInstructions) {
						
						if (instruction == selectedInstruction) {
							if (instruction.CachedLinePoints != null) {
								foreach (var subLinePlots in instruction.CachedLinePoints) {
									// draw correct instruction as selected
									subLinePlots.DrawSegment(graphics, pictureBox1.Height, true, multiplier, cbRenderG0.Checked, LEFT_MARGIN, BOTTOM_MARGIN);
								}
							}
						} else {
							if (instruction.CachedLinePoints != null) {
								foreach (var subLinePlots in instruction.CachedLinePoints) {
									subLinePlots.DrawSegment(graphics, pictureBox1.Height, false, multiplier, cbRenderG0.Checked, LEFT_MARGIN, BOTTOM_MARGIN);
								}
							}
						}
					}
				} else {
					// top level, i.e. the block level or if nothing is selected
					foreach (Block blockItem in myBlocks) {
						if (blockItem.PlotPoints != null) {
							foreach (var linePlots in blockItem.PlotPoints) {
								
								// check level first
								if (treeView.SelectedNode != null
								    && treeView.SelectedNode.Text.Equals(blockItem.ToString())) {

									// draw correct segment as selected
									linePlots.DrawSegment(graphics, pictureBox1.Height, true, multiplier, cbRenderG0.Checked, LEFT_MARGIN, BOTTOM_MARGIN);
									
								} else {
									// nothing is selected, draw segment as normal
									if (treeView.SelectedNode == null || !cbSoloSelect.Checked) {
										linePlots.DrawSegment(graphics, pictureBox1.Height, false, multiplier, cbRenderG0.Checked, LEFT_MARGIN, BOTTOM_MARGIN);
									}
								}
							}
						}
					}
				}
			}

			pictureBox1.Refresh();
		}

		float GetFeedRateRapidMoves() {
			var data = QuickSettings.Get["FeedRateRapidMoves"];
			if (string.IsNullOrEmpty(data))
			{
				data = "1016";
				QuickSettings.Get["FeedRateRapidMoves"] = data;
			}
			float f = 0.0f;
			if (float.TryParse(data, style, culture, out f)) {
				// succesfull
			}
			return f;
		}
		
		float GetFeedRatePlungeMoves() {
			var data = QuickSettings.Get["FeedRatePlungeMoves"];
			if (string.IsNullOrEmpty(data))
			{
				data = "228.6";
				QuickSettings.Get["FeedRatePlungeMoves"] = data;
			}
			float f = 0.0f;
			if (float.TryParse(data, style, culture, out f)) {
				// succesfull
			}
			return f;
		}
		
		float GetZSafeHeight() {
			style = NumberStyles.AllowDecimalPoint;
			float zSafeHeight = 2.0f;
			if (!float.TryParse(txtZClearance.Text, style, culture, out zSafeHeight)) {
				txtZClearance.Text = "2.0";
				zSafeHeight = 2.0f;
			}
			return zSafeHeight;
		}

		float GetZDepth() {
			style = NumberStyles.AllowDecimalPoint;
			float zDepth = 2.0f;
			if (!float.TryParse(txtZDepth.Text, style, culture, out zDepth)) {
				txtZDepth.Text = "-0.1";
				zDepth = -0.1f;
			}
			return zDepth;
		}
		
		float GetShiftValue(TextBox txtBox) {
			float shiftValue = 0.0f;
			if (!float.TryParse(txtBox.Text, style, culture, out shiftValue)) {
				txtBox.Text = "0.0";
				shiftValue = 0.0f;
			} else {
				txtBox.Text = "0.0";
			}
			return shiftValue;
		}
		
		float GetShiftX() {
			return GetShiftValue(txtShiftX);
		}

		float GetShiftY() {
			return GetShiftValue(txtShiftY);
		}

		float GetShiftZ() {
			return GetShiftValue(txtShiftZ);
		}
		
		void SaveGCodes(bool doPeckDrilling)
		{
			var result = sfdSaveDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				var file = new FileInfo(sfdSaveDialog.FileName);
				if (!doPeckDrilling)
				{
					QuickSettings.Get["LastOpenedFile"] = file.FullName;
				}

				if (file.Exists) {
					file.Delete();
				}

				float zSafeHeight = GetZSafeHeight();

				using (var tw = new StreamWriter(file.OpenWrite())) {
					WriteGCodeHeader(tw, zSafeHeight);
					myBlocks.ForEach(x =>
					                 {
					                 	tw.WriteLine();
					                 	tw.Write(x.BuildGCodeOutput(doPeckDrilling));
					                 });
					tw.Flush();
					WriteGCodeFooter(tw, zSafeHeight);
				}
			}
		}
		
		void SaveSplittedGCodes(List<List<GCodeInstruction>> split, Point3D splitPoint, string filePath) {
			
			var dirPath = Path.GetDirectoryName(filePath);
			var fileName = Path.GetFileNameWithoutExtension(filePath);
			
			var fileFirst = new FileInfo(dirPath + Path.DirectorySeparatorChar + fileName + "_first.gcode");
			var fileSecond = new FileInfo(dirPath + Path.DirectorySeparatorChar + fileName + "_second.gcode");
			
			// clean them
			var cleanedFirst = GCodeSplitter.CleanGCode(split[0]);
			var cleanedSecond = GCodeSplitter.CleanGCode(split[1]);
			
			SaveGCodes(cleanedFirst, Point3D.Empty, fileFirst);
			SaveGCodes(cleanedSecond, splitPoint, fileSecond);
		}

		void SaveGCodes(List<GCodeInstruction> instructions, Point3D splitPoint, FileInfo file)
		{
			List<Block> blocks = null;
			
			if (splitPoint.IsEmpty) {
				// turn the instructins into blocks
				blocks = GetBlocks(instructions);
			} else {
				// transform instructions
				var transformedInstructions = new List<GCodeInstruction>();
				
				foreach (var instruction in instructions) {
					if (instruction.CanRender) {
						// transform
						if (splitPoint.X > 0 && instruction.X.HasValue) {
							instruction.X = instruction.X - splitPoint.X;
						}
						if (splitPoint.Y > 0 && instruction.Y.HasValue) {
							instruction.Y = instruction.Y - splitPoint.Y;
						}
						if (splitPoint.Z > 0 && instruction.Z.HasValue) {
							instruction.Z = instruction.Z - splitPoint.Z;
						}
					}
					transformedInstructions.Add(instruction);
				}
				
				// turn the instructins into blocks
				blocks =  GetBlocks(transformedInstructions);
			}
			
			if (file.Exists) {
				file.Delete();
			}
			
			float zSafeHeight = GetZSafeHeight();
			
			using (var tw = new StreamWriter(file.OpenWrite())) {
				WriteGCodeHeader(tw, zSafeHeight);
				blocks.ForEach(x =>
				               {
				               	tw.WriteLine();
				               	tw.Write(x.BuildGCodeOutput(false));
				               });
				tw.Flush();
				WriteGCodeFooter(tw, zSafeHeight);
			}
		}
		
		void WriteGCodeHeader(TextWriter tw, float zSafeHeight) {
			tw.WriteLine("(File built with GCodeTools)");
			tw.WriteLine("(Generated on " + DateTime.Now + ")");
			tw.WriteLine();
			tw.WriteLine("(Header)");
			tw.WriteLine("G90 (set absolute distance mode)");
			//tw.WriteLine("G90.1 (set absolute distance mode for arc centers)");
			tw.WriteLine("G17 (set active plane to XY)");
			tw.WriteLine("G40 (turn cutter compensation off)");
			tw.WriteLine("G21 (set units to mm)");
			tw.WriteLine("G0 Z{0:0.####}", zSafeHeight);
			//tw.WriteLine("M3 (start the spindle clockwise at the S speed)");
			tw.WriteLine("(Header end.)");
		}

		void WriteGCodeFooter(TextWriter tw, float zSafeHeight) {
			tw.WriteLine();
			tw.WriteLine("(Footer)");
			tw.WriteLine("G0 Z{0:0.####}", zSafeHeight);
			//tw.WriteLine("M5 (stop the spindle)");
			//tw.WriteLine("M30 (program end)");
			tw.WriteLine("G0 X0 Y0");
			tw.WriteLine("(Footer end.)");
			tw.WriteLine();
		}
		
		void BtnShiftClick(object sender, EventArgs e)
		{
			float deltaX = GetShiftX();
			float deltaY = GetShiftY();
			float deltaZ = GetShiftZ();
			var gcodeInstructions = GCodeUtils.GetShiftedGCode(parsedInstructions, deltaX, deltaY, deltaZ);
			var gCode = GCodeUtils.GetGCode(gcodeInstructions);
			ParseText(gCode);
		}
		
		void BtnSVGLoadClick(object sender, EventArgs e)
		{
			
			var dialog = new OpenFileDialog();
			string svgFilePath = "";
			
			dialog.Filter = "SVG Drawing|*.svg";
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				svgFilePath = dialog.FileName;
				
				// store data
				var fileInfo = new FileInfo(svgFilePath);
				if (fileInfo.Exists)
				{
					txtFile.Text = fileInfo.Name;
					txtFile.Tag = fileInfo.FullName;
					this.Text = fileInfo.Name;
					
					// Cannot store with QuickSettings since the last opened file is
					// opened with another load method than SVGs
					//QuickSettings.Get["LastOpenedFile"] = fileInfo.FullName;
				}
				
				var svg = SVGDocument.LoadFromFile(svgFilePath);
				var contours = svg.GetScaledContours();
				//var contours = svg.GetContours();
				float zSafeHeight = GetZSafeHeight();
				float zDepth = GetZDepth();
				float feedRateRapid = GetFeedRateRapidMoves();
				float feedRatePlunge = GetFeedRatePlungeMoves();
				string gCode = "";
				if (radSVGCenter.Checked) {
					gCode = GCodeUtils.GetGCodeCenter(contours, zDepth, feedRateRapid, feedRatePlunge, zSafeHeight);
				} else {
					gCode = GCodeUtils.GetGCode(contours, zDepth, feedRateRapid, feedRatePlunge, zSafeHeight);
				}
				ParseText(gCode);
			}
		}
		#endregion
		
		public void ParseText(string text)
		{
			parsedInstructions = SimpleGCodeParser.ParseText(text);

			treeView.Nodes.Clear();

			// turn the instructions into blocks
			myBlocks = GetBlocks(parsedInstructions);
			
			// calculate max values for X, Y and Z
			// while finalizing the blocks and adding them to the lstPlot
			maxX = 0.0f;
			maxY = 0.0f;
			maxZ = 0.0f;
			minX = 0.0f;
			minY = 0.0f;
			minZ = 0.0f;
			foreach (Block block in myBlocks)
			{
				block.CalculateMinAndMax();
				
				maxX = Math.Max(maxX, block.MaxX);
				maxY = Math.Max(maxY, block.MaxY);
				maxZ = Math.Max(maxZ, block.MaxZ);

				minX = Math.Min(minX, block.MinX);
				minY = Math.Min(minY, block.MinY);
				minZ = Math.Min(minZ, block.MinZ);
				
				// build node tree
				var node = new TreeNode(block.ToString());
				node.Tag = block;
				foreach (var instruction in block.GCodeInstructions) {
					var childNode = new TreeNode();
					childNode.Text = instruction.ToString();
					childNode.Tag = instruction;
					node.Nodes.Add(childNode);
				}
				treeView.Nodes.Add(node);
			}
			
			txtDimension.Text = String.Format("X max: {0:F2} mm \r\nX min: {1:F2} mm\r\nY max: {2:F2} mm \r\nY min: {3:F2} mm \r\nZ max: {4:F2} mm \r\nZ min: {5:F2} mm",
			                                  maxX, minX, maxY, minY, maxZ, minZ);
			
			// calculate optimal multiplier
			CalculateOptimalZoomMultiplier();
			
			RenderBlocks();
			bDataLoaded = true;
		}
		
		void CalculateOptimalZoomMultiplier() {
			// calculate optimal multiplier
			if (maxX > 0) {
				multiplier = (panelViewer.Width-50) / maxX;
			}
		}
	}
}
