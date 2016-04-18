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
		public frmPlotter()
		{
			InitializeComponent();
		}

		bool bDataLoaded = false;
		private void frmPlotter_Load(object sender, EventArgs e)
		{
			#region Code
			bDataLoaded = false;

			//var lastFile = @"Samples\dino-example.ngc";
			var lastFile = QuickSettings.Get["LastOpenedFile"];
			if (!string.IsNullOrWhiteSpace(lastFile))
			{
				// Load data here!
				var fil = new FileInfo(lastFile);
				if (fil.Exists)
				{
					txtFile.Text = fil.Name;
					txtFile.Tag = fil.FullName;
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
			return MessageBox.Show("Doing this will load/reload data, are you sure you want to load this data deleting your old data?", "Question!", MessageBoxButtons.YesNo);
			#endregion
		}

		public void ParseText(string text)
		{
			#region Code
			var parsedPlots = SimpleGCodeParser.ParseText(text);
			var sb = new StringBuilder();

			lstPlots.Items.Clear();

			var currentPoint = new PointF(0, 0);
			currentPoint.X = 0;
			currentPoint.Y = 0;

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
							currentPlot.Name = line.Comment.Substring(23);
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

			myPlots.ForEach(x => { x.FinalizePlot(); lstPlots.Items.Add(x); });

			RenderPlots();
			bDataLoaded = true;
			#endregion
		}

		private void CalculateGCodePlot()
		{
			#region Code
			var currentPoint = new PointF(0, 0);
			currentPoint.X = 0;
			currentPoint.Y = 0;

			foreach (var plot in lstPlots.Items.Cast<Plot>())
			{
				plot.Replot(ref currentPoint);
			}
			#endregion
		}

		private void cmdRedraw_Click(object sender, EventArgs e)
		{
			#region Code
			RenderPlots();
			#endregion
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			#region Code
			if (renderImage == null)
			{
				return;
			}

			RenderPlots();
			#endregion
		}

		private void cmdShiftDown_Click(object sender, EventArgs e)
		{
			#region Code
			var objs = lstPlots.SelectedItems.Cast<object>().ToList();
			var idx = lstPlots.SelectedIndex;
			if (idx >= lstPlots.Items.Count - objs.Count)
			{
				idx = lstPlots.Items.Count - objs.Count;
			}

			foreach (Plot i in objs)
			{
				myPlots.Remove(i);
				lstPlots.Items.Remove(i);
			}

			for(int i = 0; i < objs.Count; i++)
			{
				if ((idx + i) >= lstPlots.Items.Count)
				{
					lstPlots.Items.Add(objs[i]);
					myPlots.Add((Plot)objs[i]);
				}
				else
				{
					lstPlots.Items.Insert(idx + i + 1, objs[i]);
					myPlots.Insert(idx + i + 1, (Plot)objs[i]);
				}

				lstPlots.SelectedItems.Add(objs[i]);
			}

			CalculateGCodePlot();
			RenderPlots();
			#endregion
		}

		private void cmdToBottom_Click(object sender, EventArgs e)
		{
			#region Code
			var objs = lstPlots.SelectedItems.Cast<object>().ToList();
			var idx = lstPlots.SelectedIndex;
			foreach (Plot i in objs)
			{
				lstPlots.Items.Remove(i);
				myPlots.Remove(i);
			}

			for (int i = 0; i < objs.Count; i++)
			{
				lstPlots.Items.Add(objs[i]);
				myPlots.Add((Plot)objs[i]);
				lstPlots.SelectedItems.Add(objs[i]);
			}

			CalculateGCodePlot();
			RenderPlots();
			#endregion
		}

		private void cmdShiftUp_Click(object sender, EventArgs e)
		{
			#region Code
			var objs = lstPlots.SelectedItems.Cast<object>().ToList();
			var idx = lstPlots.SelectedIndex;
			if (idx < 1)
			{
				idx = 1;
			}

			foreach (Plot i in objs)
			{
				lstPlots.Items.Remove(i);
				myPlots.Remove(i);
			}

			for (int i = 0; i < objs.Count; i++)
			{
				lstPlots.Items.Insert(idx + i - 1, objs[i]);
				myPlots.Insert(idx + i - 1, (Plot)objs[i]);
				lstPlots.SelectedItems.Add(objs[i]);
			}

			CalculateGCodePlot();
			RenderPlots();
			#endregion
		}

		private void cmdToTop_Click(object sender, EventArgs e)
		{
			#region Code
			var objs = lstPlots.SelectedItems.Cast<object>().ToList();
			var idx = lstPlots.SelectedIndex;
			foreach (Plot i in objs)
			{
				lstPlots.Items.Remove(i);
				myPlots.Remove(i);
			}

			for (int i = 0; i < objs.Count; i++)
			{
				lstPlots.Items.Insert(i, objs[i]);
				myPlots.Insert(i, (Plot)objs[i]);
				lstPlots.SelectedItems.Add(objs[i]);
			}

			CalculateGCodePlot();
			RenderPlots();
			#endregion
		}

		private void lstPlots_SelectedIndexChanged(object sender, EventArgs e)
		{
			#region Code
			SelectPlot(lstPlots);
			#endregion
		}

		//Plot lastPlot = null;
		private void SelectPlot(ListBox box)
		{
			#region Code
			RenderPlots();

			pictureBox1.Refresh();
			#endregion
		}

		private void RenderPlots()
		{
			#region Code
			var multiplier = 4f;
			if (radZoomTwo.Checked) multiplier = 2;
			else if (radZoomFour.Checked) multiplier = 4;
			else if (radZoomEight.Checked) multiplier = 8;
			else if (radZoomSixteen.Checked) multiplier = 16;

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
						if (lstPlots.SelectedItems.Contains(plotItem))
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

		private void SaveGCodes(bool layers)
		{
			#region Code
			var result = sfdSaveDialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				var file = new FileInfo(sfdSaveDialog.FileName);
				if (!layers)
				{
					QuickSettings.Get["LastOpenedFile"] = file.FullName;
				}

				if (file.Exists)
					file.Delete();

				var tw = new StreamWriter(file.OpenWrite());

				tw.WriteLine("(File built with Jamakinmegcode)");
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
				                	tw.Write(x.BuildGCodeOutput(layers));
				                });
				tw.Flush();

				tw.WriteLine();
				tw.WriteLine("(Footer)");
				tw.WriteLine("G00 Z 5.00");
				tw.WriteLine("G00 X 0.00 Y 0.00");
				tw.WriteLine("(Footer end.)");
				tw.WriteLine();

				tw.Flush();

				tw.Close();
			}
			#endregion
		}
	}
}
