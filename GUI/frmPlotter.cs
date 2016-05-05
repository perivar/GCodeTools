/**
 * Copyright (c) David-John Miller AKA Anoyomouse 2014
 *
 * See LICENCE in the project directory for licence information
 * Modified by perivar@nerseth.com
 **/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using GCode;

namespace GCodePlotter
{
	public partial class frmPlotter : Form
	{
		private float ZOOMFACTOR = 1.25f;   // = 25% smaller or larger
		private int MINMAX = 5;             // 5 times bigger or smaller than the ctrl

		const int MAX_WIDTH = 8000;
		const int MAX_HEIGHT = 8000;

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

		float scale = 1.0f;
		float multiplier = 4.0f;
		
		List<GCodeInstruction> parsedInstructions = null;

		bool bDataLoaded = false;
		
		List<Plot> myPlots;

		Image renderImage = null;
		
		private Point MouseDownLocation;

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
					Application.DoEvents();
					btnParseData.Enabled = true;
					btnParseData.PerformClick();
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
			
			RenderPlots();
		}

		void TreeViewAfterSelect(object sender, TreeViewEventArgs e)
		{
			RenderPlots();
			pictureBox1.Refresh();
		}

		void TreeViewMouseDown(object sender, MouseEventArgs e)
		{
			var me = (MouseEventArgs) e;
			if (me.Button == MouseButtons.Right) {
				treeView.SelectedNode = null;
				RenderPlots();
			}
		}

		void btnLoadClick(object sender, EventArgs e)
		{
			if (AskToLoadData() == DialogResult.No)
			{
				return;
			}

			var result = ofdLoadDialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				var file = new FileInfo(ofdLoadDialog.FileName);
				if (!file.Exists)
				{
					MessageBox.Show("Selected file does not exist, please select an existing file!");
					return;
				}

				QuickSettings.Get["LastOpenedFile"] = file.FullName;

				StreamReader tr = file.OpenText();

				txtFile.Text = file.Name;
				txtFile.Tag = file.FullName;

				string data = tr.ReadToEnd();
				tr.Close();
				
				ParseText(data);
			}
		}

		void btnParseDataClick(object sender, EventArgs e)
		{
			if (bDataLoaded)
			{
				if (AskToLoadData() == DialogResult.No)
				{
					return;
				}
			}

			if (txtFile.Tag != null) {
				var file = new FileInfo(txtFile.Tag.ToString());
				StreamReader tr = file.OpenText();
				string data = tr.ReadToEnd();
				tr.Close();
				
				ParseText(data);}
		}

		void btnRedrawClick(object sender, EventArgs e)
		{
			// reset multiplier
			multiplier = 4.0f;
			
			pictureBox1.Left = 0;
			pictureBox1.Top = 0;
			
			RenderPlots();
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
			SaveGCodes(true);
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
			if (float.TryParse(txtSplit.Text, out xSplit)) {
				
				btnParseData.Enabled = true;
				btnParseData.PerformClick();
				
				var splitPoint = new Point3D(xSplit, 0, 0);
				
				float zClearance = 2.0f;
				if (!float.TryParse(txtZClearance.Text, out zClearance)) {
					txtZClearance.Text = "2.0";
					zClearance = 2.0f;
				}
				
				var split = GCodeSplitter.Split(parsedInstructions, splitPoint, 0.0f, zClearance);
				
				//var gcodeLeft = Plot.BuildGCodeOutput("Unnamed", split[0], false);
				//var gcodeRight = Plot.BuildGCodeOutput("Unnamed", split[1], false);
				
				GCodeSplitter.DumpGCode(txtFile.Text+"_left.gcode", split[0]);
				GCodeSplitter.DumpGCode(txtFile.Text+"_right.gcode", split[1]);
				
			}
		}
		
		void radScaleChange(object sender, EventArgs e)
		{
			RenderPlots();
		}

		void cbRenderG0CheckedChanged(object sender, EventArgs e)
		{
			if (renderImage == null) {
				return;
			}

			RenderPlots();
		}
		
		void cbSoloSelectCheckedChanged(object sender, EventArgs e)
		{
			if (renderImage == null) {
				return;
			}

			RenderPlots();
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
			}
		}

		void OnMouseWheel(object sender, MouseEventArgs mea) {
			
			// http://stackoverflow.com/questions/10694397/how-to-zoom-in-using-mouse-position-on-that-image
			
			if (mea.Delta < 0) {
				ZoomIn(mea.Location);
			} else {
				ZoomOut(mea.Location);
			}
		}
		
		void ZoomIn(Point clickPoint) {

			if ((pictureBox1.Width < (MINMAX * panelViewer.Width)) &&
			    (pictureBox1.Height < (MINMAX * panelViewer.Height)))
			{
				multiplier *= ZOOMFACTOR;
				
				// Formula to move the picturebox, to zoom in the point selected by the mouse cursor
				//pictureBox1.Top = (int)(clickPoint.Y - 1.25 * (clickPoint.Y - pictureBox1.Top));
				//pictureBox1.Left = (int)(clickPoint.X - 1.25 * (clickPoint.X - pictureBox1.Left));

				var x = (int)(clickPoint.X - 1.25 * (clickPoint.X - panelViewer.AutoScrollPosition.X));
				var y = (int)(clickPoint.Y - 1.25 * (clickPoint.Y - panelViewer.AutoScrollPosition.Y));
				var newScrollPoint = new Point(x, y);
				panelViewer.AutoScrollPosition = newScrollPoint;
				
				RenderPlots();
			}
		}

		void ZoomOut(Point clickPoint) {

			if ((pictureBox1.Width > (panelViewer.Width / MINMAX)) &&
			    (pictureBox1.Height > (panelViewer.Height / MINMAX )))
			{
				multiplier /= ZOOMFACTOR;

				// Formula to move the picturebox, to zoom in the point selected by the mouse cursor
				//pictureBox1.Top = (int)(clickPoint.Y - 0.80 * (clickPoint.Y - pictureBox1.Top));
				//pictureBox1.Left = (int)(clickPoint.X - 0.80 * (clickPoint.X - pictureBox1.Left));

				var x = (int)(clickPoint.X - 0.80 * (clickPoint.X - panelViewer.AutoScrollPosition.X));
				var y = (int)(clickPoint.Y - 0.80 * (clickPoint.Y - panelViewer.AutoScrollPosition.Y));
				var newScrollPoint = new Point(x, y);
				panelViewer.AutoScrollPosition = newScrollPoint;
				
				RenderPlots();
			}

		}
		#endregion
		
		#region Private Methods
		void ParseText(string text)
		{
			parsedInstructions = SimpleGCodeParser.ParseText(text);

			treeView.Nodes.Clear();

			var currentPoint = Point3D.Empty;

			myPlots = new List<Plot>();
			var currentPlot = new Plot();
			currentPlot.Name = "Unnamed Plot";
			foreach (var currentInstruction in parsedInstructions)
			{
				if (currentInstruction.IsOnlyComment) {
					
					if (currentInstruction.Comment.StartsWith("Start cutting path id:")
					    || currentInstruction.Comment == "Footer") {

						if (currentInstruction.Comment == "Footer") {
							currentPlot.Name = currentInstruction.Comment;
						} else {
							if (currentInstruction.Comment.Length > 23) {
								currentPlot.Name = currentInstruction.Comment.Substring(23);
							}
						}
					} else if (currentInstruction.Comment.StartsWith("End cutting path id:")) {
						if (currentPlot.PlotPoints.Count > 0) {
							
							myPlots.Add(currentPlot);
							
							// Reset plot, meaning add new
							currentPlot = new Plot();
							currentPlot.Name = "Unnamed Plot";
						}
					} else {
						// ignore all comments up to first "Start Cutting", i.e. header
						// TODO: Handle headers like (Circles) and (Square)
					}
					
				} else if (currentInstruction.CanRender()) {
					// this is where the plot is put together and where the linepoints is added
					var linePointsCollection = currentInstruction.RenderCode(ref currentPoint);
					if (linePointsCollection != null) {
						currentInstruction.CachedLinePoints = linePointsCollection;
						currentPlot.PlotPoints.AddRange(linePointsCollection);
					}

					// make sure to store the actual instruction as well
					currentPlot.GCodeInstructions.Add(currentInstruction);
				} else {
					// ignore everything that isn't a comment and or cannot be rendered
				}
			}

			if (currentPlot.PlotPoints.Count > 0) {
				myPlots.Add(currentPlot);
			}

			// remove footer if it exists
			if (myPlots.Count > 0) {
				var footer = myPlots.Last();
				if (footer.Name == "Footer") {
					myPlots.Remove(footer);
				}
			}
			
			// calculate max values for X, Y and Z
			// while finalizing the plots and adding them to the lstPlot
			maxX = 0.0f;
			maxY = 0.0f;
			maxZ = 0.0f;
			minX = 0.0f;
			minY = 0.0f;
			minZ = 0.0f;
			foreach (Plot plotItem in myPlots)
			{
				plotItem.CalculateMinAndMax();
				
				maxX = Math.Max(maxX, plotItem.MaxX);
				maxY = Math.Max(maxY, plotItem.MaxY);
				maxZ = Math.Max(maxZ, plotItem.MaxZ);

				minX = Math.Min(minX, plotItem.MinX);
				minY = Math.Min(minY, plotItem.MinY);
				minZ = Math.Min(minZ, plotItem.MinZ);
				
				// build node tree
				var node = new TreeNode(plotItem.ToString());
				node.Tag = plotItem;
				foreach (var instruction in plotItem.GCodeInstructions) {
					var childNode = new TreeNode();
					childNode.Text = instruction.ToString();
					childNode.Tag = instruction;
					node.Nodes.Add(childNode);
				}
				treeView.Nodes.Add(node);
			}
			
			txtDimension.Text = String.Format("X max: {0:F2} mm \r\nX min: {1:F2} mm\r\nY max: {2:F2} mm \r\nY min: {3:F2} mm \r\nZ max: {4:F2} mm \r\nZ min: {5:F2} mm",
			                                  maxX, minX, maxY, minY, maxZ, minZ);
			
			RenderPlots();
			bDataLoaded = true;
		}
		
		void ResetSplit(int index) {
			
			if (parsedInstructions == null) {
				MessageBox.Show("No file loaded!");
				return;
			}
			
			if ("".Equals(txtSplit.Text)) {
				MessageBox.Show("No split value entered!");
				return;
			}

			float xSplit = 0.0f;
			if (float.TryParse(txtSplit.Text, out xSplit)) {
				
				btnParseData.Enabled = true;
				btnParseData.PerformClick();
				
				var splitPoint = new Point3D(xSplit, 0, 0);
				
				float zClearance = 2.0f;
				if (!float.TryParse(txtZClearance.Text, out zClearance)) {
					txtZClearance.Text = "2.0";
					zClearance = 2.0f;
				}
				
				var split = GCodeSplitter.Split(parsedInstructions, splitPoint, 0.0f, zClearance);
				
				// clean up the mess with too many G0 commands
				var cleaned = GCodeSplitter.CleanGCode(split[index]);
				
				var gcodeSplitted = Plot.BuildGCodeOutput("Unnamed Plot", cleaned, false);
				ParseText(gcodeSplitted);
			}
		}
		
		Size GetDimensionsFromRadioButtons() {
			
			// set multiplier variable
			if (radZoomTwo.Checked) {
				multiplier = 2;
			} else if (radZoomFour.Checked) {
				multiplier = 4;
			} else if (radZoomEight.Checked) {
				multiplier = 8;
			} else if (radZoomSixteen.Checked) {
				multiplier = 16;
			}

			// set scale variable
			scale = (10 * multiplier);

			// 10 mm per grid
			var width = (int)(maxX * scale + 1) / 10 + 2 * LEFT_MARGIN;
			var height = (int)(maxY * scale + 1) / 10 + 2 * BOTTOM_MARGIN;
			
			// set max size in case the calculated dimensions are way off
			width = (int) Math.Min(width, MAX_WIDTH);
			height = (int) Math.Min(height, MAX_HEIGHT);
			
			return new Size(width, height);
		}
		
		Size GetDimensionsFromZoom() {

			// set scale variable
			scale = (10 * multiplier);

			// 10 mm per grid
			var width = (int)(maxX * scale + 1) / 10 + 2 * LEFT_MARGIN;
			var height = (int)(maxY * scale + 1) / 10 + 2 * BOTTOM_MARGIN;
			
			// set max size in case the calculated dimensions are way off
			//width = (int) Math.Min(width, MAX_WIDTH);
			//height = (int) Math.Min(height, MAX_HEIGHT);
			
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
				} catch (OutOfMemoryException ex) {
					// could draw a red cross like here:
					// http://stackoverflow.com/questions/22163846/zooming-of-an-image-using-mousewheel
				}
			}
		}

		void RenderPlots()
		{
			GetEmptyImage();
			
			var graphics = Graphics.FromImage(renderImage);
			graphics.Clear(ColorHelper.GetColor(PenColorList.Background));
			
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
			using (var penX = new Pen(Color.Green, 3)) {
				penX.StartCap= LineCap.Flat;
				penX.EndCap = LineCap.ArrowAnchor;
				graphics.DrawLine(penX, LEFT_MARGIN, pictureBox1.Height-BOTTOM_MARGIN, 100, pictureBox1.Height-BOTTOM_MARGIN);
			}
			using (var penY = new Pen(Color.Red, 3)) {
				penY.StartCap = LineCap.ArrowAnchor;
				penY.EndCap = LineCap.Flat;
				graphics.DrawLine(penY, LEFT_MARGIN, pictureBox1.Height-100, LEFT_MARGIN, pictureBox1.Height-BOTTOM_MARGIN);
			}

			// draw gcode
			if (myPlots != null && myPlots.Count > 0)
			{
				if (treeView.SelectedNode != null
				    && treeView.SelectedNode.Level == 1) {
					// sub-level, i.e. the instruction level

					var selectedInstruction = (GCodeInstruction) treeView.SelectedNode.Tag;
					
					// find what plot this instruction is a part of
					var parentPlot = (Plot) treeView.SelectedNode.Parent.Tag;
					
					foreach (var instruction in parentPlot.GCodeInstructions) {
						
						if (instruction == selectedInstruction) {
							foreach (var subLinePlots in instruction.CachedLinePoints) {
								// draw correct instruction as selected
								subLinePlots.DrawSegment(graphics, pictureBox1.Height, true, multiplier, cbRenderG0.Checked, LEFT_MARGIN, BOTTOM_MARGIN);
							}
						} else {
							foreach (var subLinePlots in instruction.CachedLinePoints) {
								subLinePlots.DrawSegment(graphics, pictureBox1.Height, false, multiplier, cbRenderG0.Checked, LEFT_MARGIN, BOTTOM_MARGIN);
							}
						}
					}
				} else {
					// top level, i.e. the plot level or if nothing is selected
					foreach (Plot plotItem in myPlots) {
						foreach (var linePlots in plotItem.PlotPoints) {
							
							// check level first
							if (treeView.SelectedNode != null
							    && treeView.SelectedNode.Text.Equals(plotItem.ToString())) {

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

			pictureBox1.Refresh();
		}
		
		void SaveGCodes(bool doMultiLayer)
		{
			var result = sfdSaveDialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				var file = new FileInfo(sfdSaveDialog.FileName);
				if (!doMultiLayer)
				{
					QuickSettings.Get["LastOpenedFile"] = file.FullName;
				}

				if (file.Exists) {
					file.Delete();
				}

				var tw = new StreamWriter(file.OpenWrite());

				tw.WriteLine("(File built with GCodeTools)");
				tw.WriteLine("(Generated on " + DateTime.Now.ToString() + ")");
				tw.WriteLine();
				tw.WriteLine("(Header)");
				tw.WriteLine("G90   (set absolute distance mode)");
				//tw.WriteLine("G90.1 (set absolute distance mode for arc centers)");
				tw.WriteLine("G17   (set active plane to XY)");
				tw.WriteLine("G21   (set units to mm)");
				tw.WriteLine("(Header end.)");
				tw.WriteLine();
				myPlots.ForEach(x =>
				                {
				                	tw.WriteLine();
				                	tw.Write(x.BuildGCodeOutput(doMultiLayer));
				                });
				tw.Flush();

				tw.WriteLine();
				tw.WriteLine("(Footer)");
				tw.WriteLine("G00 Z5");
				tw.WriteLine("G00 X0 Y0");
				tw.WriteLine("(Footer end.)");
				tw.WriteLine();

				tw.Flush();
				tw.Close();
			}
		}
		#endregion
	}
}
