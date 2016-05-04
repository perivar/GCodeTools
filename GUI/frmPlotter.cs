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
		const int MAX_WIDTH = 10000;
		const int MAX_HEIGHT = 10000;
		
		List<GCodeInstruction> parsedInstructions = null;

		bool bDataLoaded = false;
		
		List<Plot> myPlots;

		Image renderImage = null;

		public frmPlotter()
		{
			InitializeComponent();
		}
		
		private void frmPlotter_Load(object sender, EventArgs e)
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
					cmdParseData.Enabled = true;
					cmdParseData.PerformClick();
				}
			}
		}

		private void cmdParseData_Click(object sender, EventArgs e)
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

		private DialogResult AskToLoadData()
		{
			//return MessageBox.Show("Doing this will load/reload data, are you sure you want to load this data deleting your old data?", "Question!", MessageBoxButtons.YesNo);
			return DialogResult.OK;
		}

		public void ParseText(string text)
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
			float absMaxX = 0.0f;
			float absMaxY = 0.0f;
			float absMaxZ = 0.0f;
			float absMinX = 0.0f;
			float absMinY = 0.0f;
			float absMinZ = 0.0f;
			
			foreach (Plot plotItem in myPlots)
			{
				plotItem.CalculateMinAndMax();
				
				absMaxX = Math.Max(absMaxX, plotItem.MaxX);
				absMaxY = Math.Max(absMaxY, plotItem.MaxY);
				absMaxZ = Math.Max(absMaxZ, plotItem.MaxZ);

				absMinX = Math.Min(absMinX, plotItem.MinX);
				absMinY = Math.Min(absMinY, plotItem.MinY);
				absMinZ = Math.Min(absMinZ, plotItem.MinZ);
				
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
			                                  absMaxX, absMinX, absMaxY, absMinY, absMaxZ, absMinZ);
			
			RenderPlots();
			bDataLoaded = true;
		}

		private void cmdRedraw_Click(object sender, EventArgs e)
		{
			RenderPlots();
		}

		void BtnSplitClick(object sender, EventArgs e)
		{
			if (radLeft.Checked) {
				ResetSplit(0);
			} else {
				ResetSplit(1);
			}
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
				
				cmdParseData.Enabled = true;
				cmdParseData.PerformClick();
				
				var splitPoint = new Point3D(xSplit, 0, 0);
				
				float zClearance = 2.0f;
				if (!float.TryParse(txtZClearance.Text, out zClearance)) {
					txtZClearance.Text = "2.0";
				}
				
				var split = GCodeSplitter.Split(parsedInstructions, splitPoint, 0.0f, zClearance);
				
				var gcodeSplitted = Plot.BuildGCodeOutput("Unnamed Plot", split[index], false);
				ParseText(gcodeSplitted);
			}
		}
		
		void TreeViewAfterSelect(object sender, TreeViewEventArgs e)
		{
			RenderPlots();
			pictureBox1.Refresh();
		}
		
		private void RenderPlots()
		{
			const int leftMargin = 20;
			const int bottomMargin = 20;
			
			var multiplier = 4f;
			if (radZoomTwo.Checked) {
				multiplier = 2;
			} else if (radZoomFour.Checked) {
				multiplier = 4;
			} else if (radZoomEight.Checked) {
				multiplier = 8;
			} else if (radZoomSixteen.Checked) {
				multiplier = 16;
			}

			var scale = (10 * multiplier);

			// determine the x and y sizes based on the maxX and Y from all the plots
			var absMaxX = 0f;
			var absMaxY = 0f;
			if (myPlots != null && myPlots.Count > 0)
			{
				foreach (Plot plotItem in myPlots)
				{
					absMaxX = Math.Max(absMaxX, plotItem.MaxX);
					absMaxY = Math.Max(absMaxY, plotItem.MaxY);
				}
			}

			absMaxX *= scale;
			absMaxY *= scale;

			// 10 mm per grid
			var intAbsMaxX = (int)(absMaxX + 1) / 10 + (int) (multiplier*leftMargin);
			var intAbsMaxY = (int)(absMaxY + 1) / 10 + (int) (multiplier*bottomMargin);
			
			// set max size in case the calculated dimensions are way off
			intAbsMaxX = (int) Math.Min(intAbsMaxX, MAX_WIDTH);
			intAbsMaxY = (int) Math.Min(intAbsMaxY, MAX_HEIGHT);

			if (renderImage == null || intAbsMaxX != renderImage.Width || intAbsMaxY != renderImage.Height)
			{
				if (renderImage != null) {
					renderImage.Dispose();
				}

				renderImage = new Bitmap(intAbsMaxX, intAbsMaxY);
				pictureBox1.Width = intAbsMaxX;
				pictureBox1.Height = intAbsMaxY;
				pictureBox1.Image = renderImage;
			}

			var graphics = Graphics.FromImage(renderImage);
			graphics.Clear(ColorHelper.GetColor(PenColorList.Background));
			
			// draw grid
			Pen gridPen = ColorHelper.GetPen(PenColorList.GridLines);
			for (var x = 0; x < pictureBox1.Width / scale; x++)
			{
				for (var y = 0; y < pictureBox1.Height / scale; y++)
				{
					graphics.DrawLine(gridPen, x * scale + leftMargin, 0, x * scale + leftMargin, pictureBox1.Height);
					graphics.DrawLine(gridPen, 0, pictureBox1.Height - (y * scale) - bottomMargin, pictureBox1.Width, pictureBox1.Height - (y * scale) - bottomMargin);
				}
			}

			// draw arrow grid
			using (var penZero = new Pen(Color.WhiteSmoke, 1)) {
				graphics.DrawLine(penZero, leftMargin, pictureBox1.Height-bottomMargin, pictureBox1.Width, pictureBox1.Height-bottomMargin);
				graphics.DrawLine(penZero, leftMargin, 0, leftMargin, pictureBox1.Height-bottomMargin);
			}
			using (var penX = new Pen(Color.Green, 3)) {
				penX.StartCap= LineCap.Flat;
				penX.EndCap = LineCap.ArrowAnchor;
				graphics.DrawLine(penX, leftMargin, pictureBox1.Height-bottomMargin, 100, pictureBox1.Height-bottomMargin);
			}
			using (var penY = new Pen(Color.Red, 3)) {
				penY.StartCap = LineCap.ArrowAnchor;
				penY.EndCap = LineCap.Flat;
				graphics.DrawLine(penY, leftMargin, pictureBox1.Height-100, leftMargin, pictureBox1.Height-bottomMargin);
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
								subLinePlots.DrawSegment(graphics, pictureBox1.Height, Multiplier: multiplier, renderG0: cbRenderG0.Checked, highlight: true, left: leftMargin, bottom: bottomMargin);
							}
						} else {
							foreach (var subLinePlots in instruction.CachedLinePoints) {
								subLinePlots.DrawSegment(graphics, pictureBox1.Height, Multiplier: multiplier, renderG0: cbRenderG0.Checked, left: leftMargin, bottom: bottomMargin);
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
								linePlots.DrawSegment(graphics, pictureBox1.Height, Multiplier: multiplier, renderG0: cbRenderG0.Checked, highlight: true, left: leftMargin, bottom: bottomMargin);
								
							} else {
								// nothing is selected, draw segment as normal
								if (treeView.SelectedNode == null || !cbSoloSelect.Checked) {
									linePlots.DrawSegment(graphics, pictureBox1.Height, Multiplier: multiplier, renderG0: cbRenderG0.Checked, left: leftMargin, bottom: bottomMargin);
								}
							}
						}
					}
				}
			}

			pictureBox1.Refresh();
		}

		private void cmdLoad_Click(object sender, EventArgs e)
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

		private void frmPlotter_ResizeEnd(object sender, EventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized)
			{
				return;
			}

			RenderPlots();
		}

		private void radScaleChange(object sender, EventArgs e)
		{
			RenderPlots();
		}

		private void cmdSave_Click(object sender, EventArgs e)
		{
			SaveGCodes(false);
		}

		private void cmdSaveLayers_Click(object sender, EventArgs e)
		{
			SaveGCodes(true);
		}

		private void SaveGCodes(bool doMultiLayer)
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
		
		void TreeViewMouseDown(object sender, MouseEventArgs e)
		{
			var me = (MouseEventArgs) e;
			if (me.Button == MouseButtons.Right) {
				treeView.SelectedNode = null;
				RenderPlots();
			}
		}
		
		void BtnSaveSplitClick(object sender, EventArgs e)
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
				
				cmdParseData.Enabled = true;
				cmdParseData.PerformClick();
				
				var splitPoint = new Point3D(xSplit, 0, 0);
				
				float zClearance = 2.0f;
				if (!float.TryParse(txtZClearance.Text, out zClearance)) {
					txtZClearance.Text = "2.0";
				}
				
				var split = GCodeSplitter.Split(parsedInstructions, splitPoint, 0.0f, zClearance);
				
				//var gcodeLeft = Plot.BuildGCodeOutput("Unnamed", split[0], false);
				//var gcodeRight = Plot.BuildGCodeOutput("Unnamed", split[1], false);
				
				GCodeSplitter.DumpGCode(txtFile.Text+"_left.gcode", split[0]);
				GCodeSplitter.DumpGCode(txtFile.Text+"_right.gcode", split[1]);
				
			}
		}
		
		void cbRenderG0_CheckedChanged(object sender, EventArgs e)
		{
			if (renderImage == null) {
				return;
			}

			RenderPlots();
		}
		
		void CbSoloSelectCheckedChanged(object sender, EventArgs e)
		{
			if (renderImage == null) {
				return;
			}

			RenderPlots();
		}
	}
}
