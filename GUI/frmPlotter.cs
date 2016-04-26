/**
 * Copyright (c) David-John Miller AKA Anoyomouse 2014
 *
 * See LICENCE in the project directory for licence information
 **/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GCodePlotter
{
	public partial class frmPlotter : Form
	{
		const int MAX_WIDTH = 10000;
		const int MAX_HEIGHT = 10000;
		
		List<GCodeInstruction> parsedPlots = null;

		public frmPlotter()
		{
			InitializeComponent();
		}

		bool bDataLoaded = false;
		private void frmPlotter_Load(object sender, EventArgs e)
		{
			#region Code
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
			#endregion
		}

		List<Plot> myPlots;
		Image renderImage = null;
		private void cmdParseData_Click(object sender, EventArgs e)
		{
			#region Code

			if (bDataLoaded)
			{
				if (AskToLoadData() == DialogResult.No)
				{
					return;
				}
			}

			var file = new FileInfo(txtFile.Tag.ToString());
			StreamReader tr = file.OpenText();
			string data = tr.ReadToEnd();
			tr.Close();

			ParseText(data);
			#endregion
		}

		private DialogResult AskToLoadData()
		{
			#region Code
			//return MessageBox.Show("Doing this will load/reload data, are you sure you want to load this data deleting your old data?", "Question!", MessageBoxButtons.YesNo);
			return DialogResult.OK;
			#endregion
		}

		public void ParseText(string text)
		{
			#region Code
			parsedPlots = SimpleGCodeParser.ParseText(text);
			var sb = new StringBuilder();

			treeView.Nodes.Clear();

			var currentPoint = new Point3D(0, 0, 0);

			myPlots = new List<Plot>();
			var currentPlot = new Plot();
			foreach (var line in parsedPlots)
			{
				sb.Append(line).AppendLine();
				if (line.IsOnlyComment)
				{
					if (line.Comment.StartsWith("Start cutting path id:") || line.Comment == "Footer")
					{
						if (currentPlot.PlotPoints.Count > 0)
						{
							var point = currentPlot.PlotPoints[currentPlot.PlotPoints.Count - 1];
							myPlots.Add(currentPlot);
							// New plot!
							currentPlot = new Plot();
						}

						if (line.Comment == "Footer")
						{
							currentPlot.Name = line.Comment;
						}
						else
						{
							if (line.Comment.Length > 23) {
								currentPlot.Name = line.Comment.Substring(23);
							}
						}
					}

					if (line.Comment.StartsWith("End cutting path id:"))
					{
						if (currentPlot.PlotPoints.Count > 0)
						{
							var point = currentPlot.PlotPoints[currentPlot.PlotPoints.Count - 1];
							myPlots.Add(currentPlot);
							// New plot!
							currentPlot = new Plot();
						}
					}
				}
				else if (line.CanRender())
				{
					var data = line.RenderCode(ref currentPoint);
					if (data != null)
					{
						currentPlot.PlotPoints.AddRange(data);
					}

					currentPlot.GCodeInstructions.Add(line);
				} else {
					// non renderable plot
				}
			}

			if (currentPlot.PlotPoints.Count > 0)
			{
				var point = currentPlot.PlotPoints[currentPlot.PlotPoints.Count - 1];
				myPlots.Add(currentPlot);
			}

			var footer = myPlots.Last();
			if (footer.Name == "Footer")
			{
				myPlots.Remove(footer);
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
				plotItem.FinalizePlot();
				
				// build node tree
				var node = new TreeNode(plotItem.ToString());
				foreach (var instruction in plotItem.GCodeInstructions) {
					node.Nodes.Add(instruction.ToString());
				}
				treeView.Nodes.Add(node);
				
				absMaxX = Math.Max(absMaxX, plotItem.maxX);
				absMaxY = Math.Max(absMaxY, plotItem.maxY);
				absMaxZ = Math.Max(absMaxZ, plotItem.maxZ);

				absMinX = Math.Min(absMinX, plotItem.minX);
				absMinY = Math.Min(absMinY, plotItem.minY);
				absMinZ = Math.Min(absMinZ, plotItem.minZ);
			}
			
			txtDimension.Text = String.Format("X max: {0:F2} mm \r\nX min: {1:F2} mm\r\nY max: {2:F2} mm \r\nY min: {3:F2} mm \r\nZ max: {4:F2} mm \r\nZ min: {5:F2} mm",
			                                  absMaxX, absMinX, absMaxY, absMinY, absMaxZ, absMinZ);
			
			RenderPlots();
			bDataLoaded = true;
			#endregion
		}

		private void cmdRedraw_Click(object sender, EventArgs e)
		{
			#region Code
			RenderPlots();
			#endregion
		}

		void BtnSplitClick(object sender, EventArgs e)
		{
			if ("".Equals(txtSplit.Text)) {
				MessageBox.Show("No split value entered!");
				return;
			}
			float xSplit = 0.0f;
			if (float.TryParse(txtSplit.Text, out xSplit)) {
				var splitPoint = new Point3D(xSplit, 0, 0);
				var split = GCodeSplitter.Split(parsedPlots, splitPoint, 0.0f);
				
				var gcodeTest = Plot.BuildGCodeOutput("noname", split[0], false);
				ParseText(gcodeTest);
			}
		}
		
		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			#region Code
			if (renderImage == null) {
				return;
			}

			RenderPlots();
			#endregion
		}

		void TreeViewAfterSelect(object sender, TreeViewEventArgs e)
		{
			RenderPlots();
			pictureBox1.Refresh();
		}
		
		private void RenderPlots()
		{
			#region Code
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

			var absMaxX = 0f;
			var absMaxY = 0f;

			if (myPlots != null && myPlots.Count > 0)
			{
				foreach (Plot plotItem in myPlots)
				{
					absMaxX = Math.Max(absMaxX, plotItem.maxX);
					absMaxY = Math.Max(absMaxY, plotItem.maxY);
				}
			}

			absMaxX *= scale;
			absMaxY *= scale;

			var intAbsMaxX = (int)(absMaxX + 1) / 10 + 10;
			var intAbsMaxY = (int)(absMaxY + 1) / 10 + 10;
			
			// set max size in case the calculated dimensions are way off
			intAbsMaxX = (int) Math.Min(intAbsMaxX, MAX_WIDTH);
			intAbsMaxY = (int) Math.Min(intAbsMaxY, MAX_HEIGHT);

			if (renderImage == null || intAbsMaxX != renderImage.Width || intAbsMaxY != renderImage.Height)
			{
				if (renderImage != null)
				{
					renderImage.Dispose();
				}

				renderImage = new Bitmap(intAbsMaxX, intAbsMaxY);
				pictureBox1.Width = intAbsMaxX;
				pictureBox1.Height = intAbsMaxY;
				pictureBox1.Image = renderImage;
			}

			var graphics = Graphics.FromImage(renderImage);
			graphics.Clear(ColorHelper.GetColor(PenColorList.Background));

			Pen gridPen = ColorHelper.GetPen(PenColorList.GridLines);
			for (var x = 1; x < pictureBox1.Width / scale; x++)
			{
				for (var y = 1; y < pictureBox1.Height / scale; y++)
				{
					graphics.DrawLine(gridPen, x * scale, 0, x * scale, pictureBox1.Height);
					graphics.DrawLine(gridPen, 0, pictureBox1.Height - (y * scale), pictureBox1.Width, pictureBox1.Height - (y * scale));
				}
			}

			if (myPlots != null && myPlots.Count > 0)
			{
				foreach (Plot plotItem in myPlots)
				{
					foreach (var data in plotItem.PlotPoints)
					{
						if (treeView.SelectedNode != null && treeView.SelectedNode.Text.Equals(plotItem.ToString()))
						{
							data.DrawSegment(graphics, pictureBox1.Height, Multiplier: multiplier, renderG0: checkBox1.Checked, highlight: true);
						}
						else
						{
							data.DrawSegment(graphics, pictureBox1.Height, Multiplier: multiplier, renderG0: checkBox1.Checked);
						}
					}
				}
			}

			pictureBox1.Refresh();
			#endregion
		}

		private void cmdLoad_Click(object sender, EventArgs e)
		{
			#region Code
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
			#endregion
		}

		private void frmPlotter_ResizeEnd(object sender, EventArgs e)
		{
			#region Code
			if (this.WindowState == FormWindowState.Minimized)
			{
				return;
			}

			RenderPlots();
			#endregion
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
			#region Code
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
				tw.WriteLine("G90");
				tw.WriteLine("G21 (All units in mm)");
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
			#endregion
		}
	}
}
