﻿/**
 * Copyright (c) David-John Miller AKA Anoyomouse 2014
 * Heaviliy Modified by perivar@nerseth.com
 *
 * See LICENCE in the project directory for licence information
 **/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Diagnostics;
using GCode;
using SVG;

namespace GCodePlotter
{
	public partial class frmPlotter : Form
	{
		// this tracks the transformation applied to the PictureBox's Graphics
		Matrix transform = new Matrix();
		const float mouseScrollValue = 0.5f;
		float zoomScale = 1.0f;

		const float DEFAULT_MULTIPLIER = 1.0f;
		float multiplier = DEFAULT_MULTIPLIER;
		float scale = 10.0f;		// draw scale, typically 10 times the multiplier

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

		Image offlineImage = null;
		
		Point MouseDownLocation = Point.Empty;
		
		// if app is called using a filepath (from "Open With") store this
		string filePathArgument = string.Empty;

		public frmPlotter(string filePath)
		{
			InitializeComponent();
			
			filePathArgument = filePath;
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

			string filePathToUse = string.Empty;
			if (filePathArgument != string.Empty) {
				filePathToUse = filePathArgument;
			} else {
				filePathToUse = QuickSettings.Get["LastOpenedFile"];
			}
			
			if (!string.IsNullOrWhiteSpace(filePathToUse))
			{
				// Load data here!
				var fileInfo = new FileInfo(filePathToUse);
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
		}
		
		void TreeViewMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right) {
				treeView.SelectedNode = null;
				RenderBlocks();
			}
		}

		void TreeViewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete) {
				var selectedNode = treeView.SelectedNode;
				if (selectedNode != null && selectedNode.Level == 0)
				{
					DeleteTreeNode(selectedNode);
				}
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

				ParseGCodeString(data);
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

			float xSplit = GetSplitValue();
			if (xSplit != 0) {
				
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
			if (offlineImage == null) {
				return;
			}

			RenderBlocks();
		}
		
		void cbSoloSelectCheckedChanged(object sender, EventArgs e)
		{
			if (offlineImage == null) {
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
				MouseDownLocation = GetRealCoordinate(e.Location);
			}
		}
		
		/// <summary>
		/// Back Track the Mouse to return accurate coordinates regardless of
		/// zoom or pan effects.
		/// <seealso cref="http://www.bobpowell.net/backtrack.htm"/>
		/// </summary>
		/// <param name="p">Point to backtrack</param>
		/// <returns>Backtracked point</returns>
		public Point GetRealCoordinate(Point p) {

			// Get the inverse of the view matrix so that we can transform the mouse point into the view
			Matrix inverseTransform = transform.Clone();
			inverseTransform.Invert();

			// Translate the point
			var pts = new [] { p };
			inverseTransform.TransformPoints(pts);
			return pts[0];
		}
		
		void PictureBox1MouseMove(object sender, MouseEventArgs e)
		{
			// Translate the mouse point
			Point mouseNowLocation = GetRealCoordinate(e.Location);
			
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
			{
				// the distance the mouse has travelled since the mouse was pressed
				int deltaX = mouseNowLocation.X - MouseDownLocation.X;
				int deltaY = mouseNowLocation.Y - MouseDownLocation.Y;

				// move (translat) the offline image the same distance
				transform.Translate(deltaX, deltaY);

				//UpdateAutoScrollMinSize();
				
				// Since we are only drawing the visable rectange of the picturebox
				// we have to ensure we update the drawing when we scroll
				RenderBlocks();
			}
			
			// output scaled coordinates
			float x = (mouseNowLocation.X - LEFT_MARGIN) / scale * 10;
			float y = (pictureBox1.Height - mouseNowLocation.Y - BOTTOM_MARGIN) / scale * 10;
			
			// round
			x = (float) Math.Round(x, 1, MidpointRounding.AwayFromZero);
			y = (float) Math.Round(y, 1, MidpointRounding.AwayFromZero);
			
			txtCoordinates.Text = string.Format(CultureInfo.InvariantCulture, "X: {0:0.##}, Y: {1:0.##}", x, y);
		}

		void OnMouseWheel(object sender, MouseEventArgs mea) {
			
			pictureBox1.Focus();
			if (pictureBox1.Focused && mea.Delta != 0)
			{
				ZoomInOut(mea.Location, mea.Delta > 0);
			}
			
			// set handled to true to disable scrolling the scrollbars using the mousewheel
			((HandledMouseEventArgs)mea).Handled = true;
		}
		
		void BtnOptimizeClick(object sender, EventArgs e)
		{
			//var points = DataProvider.GetPoints(@"JavaScript\data.js", "data200");
			var gcodeSplitObject = GCodeUtils.SplitGCodeInstructions(parsedInstructions);
			var points = gcodeSplitObject.AllG0Sections.ToList<IPoint>();
			
			// TODO: add the origin at 0,0
			//points.Add(new Point3D(0, 0, 0));
			
			new GCodeOptimizer.MainForm(this, points, maxX, maxY).Show();
		}
		
		void BtnShiftClick(object sender, EventArgs e)
		{
			float deltaX = GetMoveX();
			float deltaY = GetMoveY();
			float deltaZ = GetMoveZ();
			var gcodeInstructions = GCodeUtils.GetShiftedGCode(parsedInstructions, deltaX, deltaY, deltaZ);
			var gCode = GCodeUtils.GetGCode(gcodeInstructions);
			ParseGCodeString(gCode);
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
				ParseGCodeString(gCode);
			}
		}
		#endregion
		
		#region Zoom Methods
		void PanelViewerScroll(object sender, ScrollEventArgs e)
		{
			return;
			
			// Since we are only drawing the visable rectange of the picturebox
			// we have to ensure we update the drawing when we scroll
			//RenderBlocks();
			
			// Fix scrollbar position
			float zoom = transform.Elements[0];
			transform = new Matrix(zoom, 0, 0,
			                       zoom, 0, 0);
			transform.Translate(panelViewer.AutoScrollPosition.X / zoom,
			                    panelViewer.AutoScrollPosition.Y / zoom,
			                    MatrixOrder.Append);

			string transformString = string.Concat(transform.Elements.Select(i => string.Format("{0:0.##},", i)));
			txtDimension.Text = string.Format(CultureInfo.InvariantCulture, "{0}", transformString);

			RenderBlocks();

			//pictureBox1.Invalidate();
			//pictureBox1.Refresh();
		}

		void ZoomInOut(Point clickPoint, bool zoomIn) {
			
			// The best explanation of how zooming around a origin works is here:
			// https://stackoverflow.com/questions/27871711/zoom-in-on-a-fixed-point-using-matrices
			// I.e.
			// 1. Translate mouse point to origin (0,0) => ( -xpos, -ypos )
			// 2. Scale (e.g. 1.5 )
			// 3. Translate back to the pivot ( +xpos, +ypos )
			
			// A more comprehensive example can be found here:
			// https://stackoverflow.com/questions/44566229/how-to-zoom-at-a-point-in-picturebox-in-c
			
			// Figure out what the new scale will be and
			// ensure the scale factor remains at a sensible level
			float newZoomScale = Math.Min(Math.Max(zoomScale + (zoomIn ? mouseScrollValue : -mouseScrollValue), 0.25f), 20f);

			if (newZoomScale != zoomScale)
			{
				float zoomValue = newZoomScale / zoomScale;
				zoomScale = newZoomScale;
				
				// Translate mouse point to origin
				transform.Translate(-clickPoint.X, -clickPoint.Y, MatrixOrder.Append);

				// Scale the view
				transform.Scale(zoomValue, zoomValue, MatrixOrder.Append);

				// Translate origin back to original mouse point.
				transform.Translate(clickPoint.X, clickPoint.Y, MatrixOrder.Append);
				
				// Calculates the effective size of the image after zooming and updates the AutoScrollSize accordingly
				UpdateAutoScrollMinSize();
				
				// Update scrollbar position
				// UpdateScroll(clickPoint);
				//UpdateAutoScrollPosition(clickPoint, );
				
				// Render the GCode Blocks
				RenderBlocks();
			}
		}
		
		/// <summary>
		/// Calculates the effective size of the image
		/// after zooming and updates the AutoScrollSize accordingly
		/// </summary>
		void UpdateAutoScrollMinSize()
		{
			if(offlineImage == null) {
				panelViewer.AutoScrollMinSize = panelViewer.Size;
			} else {
				panelViewer.AutoScrollMinSize = new Size(
					(int)(offlineImage.Width * zoomScale + 0.5f),
					(int)(offlineImage.Height * zoomScale + 0.5f)
				);
			}
		}
		
		// TODO: DELETE?
		void UpdateScroll(Point origin) {
			if (offlineImage != null) {
				var scrollSize = new Size(
					(int)Math.Round(offlineImage.Width * transform.Elements[0]),
					(int)Math.Round(offlineImage.Height * transform.Elements[3]));
				
				var position = new Point((int)(-panelViewer.AutoScrollPosition.X - transform.Elements[4]),
				                         (int)(-panelViewer.AutoScrollPosition.Y - transform.Elements[5]));
				
				panelViewer.AutoScrollMinSize = scrollSize;
				//panelViewer.AutoScrollPosition = position;
			}
			else {
				//panelViewer.AutoScrollMargin = Size.Empty;
				panelViewer.AutoScrollMinSize = panelViewer.Size;
			}
		}
		
		/// <summary>
		/// Update the Scrollbar position
		/// </summary>
		/// <param name="clickPoint">position under the cursor which is to be retained</param>
		/// <param name="newMultiplier">zoom factor between 0.1 and 8.0 after it was updated</param>
		/// <param name="oldMultiplier">zoom factor between 0.1 and 8.0 before it was updated</param>
		void UpdateAutoScrollPosition(Point clickPoint, float newMultiplier, float oldMultiplier) {
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
				(int)Math.Round(newMultiplier * clickPoint.X / oldMultiplier) -
				(int)cursorOffset.X,
				(int)Math.Round(newMultiplier * clickPoint.Y / oldMultiplier) -
				(int)cursorOffset.Y );
			
			panelViewer.AutoScrollPosition = newScrollPosition;
		}
		
		Size GetDimensionsFromZoom() {

			// set scale variable
			scale = (10 * multiplier);

			// 10 mm per grid
			var width = (int)(maxX * scale + 1) / 10 + 2 * LEFT_MARGIN;
			var height = (int)(maxY * scale + 1) / 10 + 2 * BOTTOM_MARGIN;
			
			return new Size(width, height);
		}

		void CalculateOptimalZoomMultiplier() {
			// calculate optimal multiplier
			if (maxX > 0) {
				multiplier = (panelViewer.Width-50) / maxX;
			}
		}
		#endregion
		
		#region Get Block Methods
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
			//blocks.AddRange(GetBlockElements(gcodeSplitObject.PriorToFirstG0Section, "Top", ref currentPoint));

			// add main blocks
			blocks.AddRange(GetBlockElements(gcodeSplitObject.AllG0Sections, ref currentPoint));
			
			// last add footer
			//blocks.AddRange(GetBlockElements(gcodeSplitObject.AfterLastG0Section, "Bottom", ref currentPoint));
			
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
		#endregion
		
		#region Render Methods
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

		void CreateOfflineImage() {
			
			// create offline image
			var imageDimension = GetDimensionsFromZoom();
			int width = imageDimension.Width;
			int height = imageDimension.Height;
			
			// if anything has changed, reset image
			if (offlineImage == null || width != offlineImage.Width || height != offlineImage.Height)
			{
				if (offlineImage != null) {
					offlineImage.Dispose();
				}

				offlineImage = new Bitmap(width, height);
				//pictureBox1.ClientSize = imageDimension; // cannot set this if autosize=true
				
				try {
					pictureBox1.Image = offlineImage;
				} catch (OutOfMemoryException) {
					// could draw a red cross like here:
					// http://stackoverflow.com/questions/22163846/zooming-of-an-image-using-mousewheel
				}
			}
		}
		
		void RenderBlocks()
		{
			CreateOfflineImage();

			using (var offlineGraphics = Graphics.FromImage(offlineImage)) {
				
				// set background color
				offlineGraphics.Clear(ColorHelper.GetColor(PenColorList.Background));

				// draw smoothly.
				offlineGraphics.SmoothingMode = SmoothingMode.AntiAlias;

				// set the transformation (zoom, scale etc)
				offlineGraphics.Transform = transform;
				
				// The interpolation mode used to smooth the drawing
				offlineGraphics.InterpolationMode = InterpolationMode.High;
				
				// paint the code
				PaintGCode(offlineGraphics);
			}

			// when done with all drawing you can enforce the display update by calling
			// Invalidate or Refresh
			//pictureBox1.Invalidate();
			//pictureBox1.Refresh();
			//pictureBox1.Image = offlineImage;
			pictureBox1.Refresh();
		}
		
		void PaintGCode(Graphics g) {
			
			// draw grid
			Pen gridPen = ColorHelper.GetPen(PenColorList.GridLines);
			for (var x = 0; x < pictureBox1.Width / scale; x++)
			{
				for (var y = 0; y < pictureBox1.Height / scale; y++)
				{
					g.DrawLine(gridPen, x * scale + LEFT_MARGIN, 0, x * scale + LEFT_MARGIN, pictureBox1.Height);
					g.DrawLine(gridPen, 0, pictureBox1.Height - (y * scale) - BOTTOM_MARGIN, pictureBox1.Width, pictureBox1.Height - (y * scale) - BOTTOM_MARGIN);
				}
			}

			// draw arrow grid
			using (var penZero = new Pen(Color.WhiteSmoke, 1)) {
				g.DrawLine(penZero, LEFT_MARGIN, pictureBox1.Height-BOTTOM_MARGIN, pictureBox1.Width, pictureBox1.Height-BOTTOM_MARGIN);
				g.DrawLine(penZero, LEFT_MARGIN, 0, LEFT_MARGIN, pictureBox1.Height-BOTTOM_MARGIN);
			}
			using (var penX = new Pen(Color.Red, 3)) {
				penX.StartCap= LineCap.Flat;
				penX.EndCap = LineCap.ArrowAnchor;
				g.DrawLine(penX, LEFT_MARGIN, pictureBox1.Height-BOTTOM_MARGIN, 5 * scale + LEFT_MARGIN, pictureBox1.Height-BOTTOM_MARGIN);
			}
			using (var penY = new Pen(Color.Green, 3)) {
				penY.StartCap = LineCap.ArrowAnchor;
				penY.EndCap = LineCap.Flat;
				g.DrawLine(penY, LEFT_MARGIN, pictureBox1.Height - (5 * scale) - BOTTOM_MARGIN, LEFT_MARGIN, pictureBox1.Height-BOTTOM_MARGIN);
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
									subLinePlots.DrawSegment(g, pictureBox1.Height, true, multiplier, cbRenderG0.Checked, LEFT_MARGIN, BOTTOM_MARGIN);
								}
							}
						} else {
							if (instruction.CachedLinePoints != null) {
								foreach (var subLinePlots in instruction.CachedLinePoints) {
									subLinePlots.DrawSegment(g, pictureBox1.Height, false, multiplier, cbRenderG0.Checked, LEFT_MARGIN, BOTTOM_MARGIN);
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
									linePlots.DrawSegment(g, pictureBox1.Height, true, multiplier, cbRenderG0.Checked, LEFT_MARGIN, BOTTOM_MARGIN);
									
								} else {
									// nothing is selected, draw segment as normal
									if (treeView.SelectedNode == null || !cbSoloSelect.Checked) {
										linePlots.DrawSegment(g, pictureBox1.Height, false, multiplier, cbRenderG0.Checked, LEFT_MARGIN, BOTTOM_MARGIN);
									}
								}
							}
						}
					}
				}
			}
		}
		#endregion
		
		#region Getters for input text field values
		float GetSplitValue() {
			// only allow decimal and not minus
			NumberStyles style = NumberStyles.AllowDecimalPoint;
			float splitValue = 0.0f;
			if (!float.TryParse(txtSplit.Text, style, CultureInfo.InvariantCulture, out splitValue)) {
				txtSplit.Text = "0.0";
				splitValue = 0.0f;
			}
			return splitValue;
		}
		
		float GetFeedRateRapidMoves() {
			// allow leading sign (minus) and decimal fractions
			NumberStyles style = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
			
			var data = QuickSettings.Get["FeedRateRapidMoves"];
			if (string.IsNullOrEmpty(data))
			{
				data = "1016";
				QuickSettings.Get["FeedRateRapidMoves"] = data;
			}
			float f = 0.0f;
			if (float.TryParse(data, style, CultureInfo.InvariantCulture, out f)) {
				// succesfull
			}
			return f;
		}
		
		float GetFeedRatePlungeMoves() {
			// allow leading sign (minus) and decimal fractions
			NumberStyles style = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
			
			var data = QuickSettings.Get["FeedRatePlungeMoves"];
			if (string.IsNullOrEmpty(data))
			{
				data = "228.6";
				QuickSettings.Get["FeedRatePlungeMoves"] = data;
			}
			float f = 0.0f;
			if (float.TryParse(data, style, CultureInfo.InvariantCulture, out f)) {
				// succesfull
			}
			return f;
		}
		
		float GetZSafeHeight() {
			// only allow decimal and not minus
			NumberStyles style = NumberStyles.AllowDecimalPoint;
			float zSafeHeight = 2.0f;
			if (!float.TryParse(txtZClearance.Text, style, CultureInfo.InvariantCulture, out zSafeHeight)) {
				txtZClearance.Text = "2.0";
				zSafeHeight = 2.0f;
			}
			return zSafeHeight;
		}

		float GetZDepth() {
			// allow leading sign (minus) and decimal fractions
			NumberStyles style = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
			float zDepth = -0.1f;
			if (!float.TryParse(txtZDepth.Text, style, CultureInfo.InvariantCulture, out zDepth)) {
				txtZDepth.Text = "-0.1";
				zDepth = -0.1f;
			}
			return zDepth;
		}
		
		float GetMoveValue(TextBox txtBox) {
			NumberStyles style = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
			float shiftValue = 0.0f;
			if (!float.TryParse(txtBox.Text, style, CultureInfo.InvariantCulture, out shiftValue)) {
				txtBox.Text = "0.0";
				shiftValue = 0.0f;
			} else {
				txtBox.Text = "0.0";
			}
			return shiftValue;
		}
		
		float GetMoveX() {
			return GetMoveValue(txtShiftX);
		}

		float GetMoveY() {
			return GetMoveValue(txtShiftY);
		}

		float GetMoveZ() {
			return GetMoveValue(txtShiftZ);
		}
		#endregion
		
		#region Save Methods
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
			tw.WriteLine("G90.1 (set absolute distance mode for arc centers)");
			tw.WriteLine("G17 (set active plane to XY)");
			tw.WriteLine("G40 (turn cutter compensation off)");
			tw.WriteLine("G21 (set units to mm)");
			tw.WriteLine("G0 Z{0:0.####}", zSafeHeight);
			tw.WriteLine("M3 S4000 (start the spindle clockwise at the S speed)");
			tw.WriteLine("(Header end.)");
		}

		void WriteGCodeFooter(TextWriter tw, float zSafeHeight) {
			tw.WriteLine();
			tw.WriteLine("(Footer)");
			tw.WriteLine("G0 Z{0:0.####}", zSafeHeight);
			tw.WriteLine("M5 (stop the spindle)");
			tw.WriteLine("G0 X0 Y0");
			tw.WriteLine("G4 P1.0 (Dwell)");
			tw.WriteLine("(Footer end.)");
			tw.WriteLine();
		}
		#endregion
		
		#region Parse Methods
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
					ParseGCodeString(data);
				} else {
					string data = File.ReadAllText(fileInfo.FullName);
					ParseGCodeString(data);
				}
			}
		}
		
		public void ParseGCodeString(string text)
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
		#endregion

		void DeleteTreeNode(TreeNode selectedNode) {
			
			DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this node?",
			                                            "Delete Node?",
			                                            MessageBoxButtons.YesNo,
			                                            MessageBoxIcon.Question);
			if(dialogResult == DialogResult.Yes)
			{
				// get block
				var selectedBlock = selectedNode.Tag as Block;
				
				// get node position
				int selectedNodePos = treeView.Nodes.IndexOf(selectedNode);
				
				// try to get next node
				TreeNode nextTreeNode = null;
				if (selectedNodePos + 1 < treeView.Nodes.Count) {
					nextTreeNode = treeView.Nodes[selectedNodePos+1];
				} else {
					// get last node
					nextTreeNode = treeView.Nodes[treeView.Nodes.Count-1];
				}
				
				// remove from tree
				treeView.Nodes.Remove(selectedNode);
				
				// remove from block list
				myBlocks.Remove(selectedBlock);
				
				// and update
				treeView.SelectedNode = nextTreeNode;
				RenderBlocks();
			}
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

			float xSplit = GetSplitValue();
			if (xSplit != 0) {
				
				ParseData();
				
				var splitPoint = new Point3D(xSplit, 0, 0);
				
				float zClearance = GetZSafeHeight();
				
				var split = GCodeSplitter.Split(parsedInstructions, splitPoint, 0.0f, zClearance);
				
				// clean up the mess with too many G0 commands
				var cleaned = GCodeSplitter.CleanGCode(split[index]);
				
				var gcodeSplitted = Block.BuildGCodeOutput("Block_1", cleaned, false);
				ParseGCodeString(gcodeSplitted);
			}
		}
	}
}
