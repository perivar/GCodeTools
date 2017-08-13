using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace GCodePlotter
{
	/// <summary>
	/// Description of frmGenerate.
	/// </summary>
	public partial class frmGenerate : Form
	{
		frmPlotter _plotter;
		
		public frmGenerate(frmPlotter plotter)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			this._plotter = plotter;
			CalculatePreview();
		}
		
		void NumericUpDownValueChanged(object sender, EventArgs e)
		{
			CalculatePreview();
		}
		
		private void CalculatePreview() {
			textBoxPreviewX.Text = string.Format("{0} - {1}mm", numericUpDownOffsetX.Value, numericUpDownGridSize.Value * (numericUpDownPointsX.Value - 1) + numericUpDownOffsetX.Value);
			textBoxPreviewY.Text = string.Format("{0} - {1}mm", numericUpDownOffsetY.Value, numericUpDownGridSize.Value * (numericUpDownPointsY.Value - 1) + numericUpDownOffsetY.Value);
		}
		
		void BtnGenerateProbeClick(object sender, EventArgs e)
		{
			// G0 Z3
			// G0 X0.000 Y0.000
			// G38.2 Z-20 F50

			float zSafeHeight = _plotter.GetZSafeHeight();
			
			var gridPoints = GenerateGrid();
			string gCode = null;
			using (var tw = new StringWriter()) {
				foreach (var gridPoint in gridPoints) {
					var coord = GetCoordinate(gridPoint);
					tw.WriteLine(string.Format(CultureInfo.InvariantCulture, "G0 Z{0:0.##}\n", zSafeHeight));
					tw.WriteLine(string.Format(CultureInfo.InvariantCulture, "G0 X{0:0.##} Y{1:0.##}", coord.X, coord.Y));
					tw.WriteLine("G38.2 Z-20 F50");
					tw.WriteLine();
				}
				gCode = tw.ToString();
			}
			
			this.DialogResult = DialogResult.OK;
			_plotter.ParseGCodeString(gCode);
			this.Dispose();
		}
		
		void BtnGenerateGridClick(object sender, EventArgs e)
		{
			// G0 X200.07 Y185
			// G1 Z-0.1 F228.6
			// G0 Z2

			float zSafeHeight = _plotter.GetZSafeHeight();
			float zDepth = _plotter.GetZDepth();
			float feedRateRapid = _plotter.GetFeedRateRapidMoves();
			float feedRatePlunge = _plotter.GetFeedRatePlungeMoves();
			
			var gridPoints = GenerateGrid();
			string gCode = null;
			using (var tw = new StringWriter()) {
				foreach (var gridPoint in gridPoints) {
					var coord = GetCoordinate(gridPoint);
					
					tw.WriteLine(string.Format(CultureInfo.InvariantCulture, "G0 X{0:0.##} Y{1:0.##}", coord.X, coord.Y));
					tw.WriteLine(string.Format(CultureInfo.InvariantCulture, "G1 Z{0:0.##} F{1:0.##}\n", zDepth, feedRatePlunge));
					tw.WriteLine(string.Format(CultureInfo.InvariantCulture, "G0 Z{0:0.##}\n", zSafeHeight));
					tw.WriteLine();
				}
				gCode = tw.ToString();
			}
			
			this.DialogResult = DialogResult.OK;
			_plotter.ParseGCodeString(gCode);
			this.Dispose();
		}
		
		void BtnCancelClick(object sender, EventArgs e)
		{
			this.Dispose();
		}
		
		public PointF GetCoordinate(Point point)
		{
			var gridSize = (float) this.numericUpDownGridSize.Value;
			var offsetX = (float) this.numericUpDownOffsetX.Value;
			var offsetY = (float) this.numericUpDownOffsetY.Value;

			return new PointF(point.X * gridSize + offsetX, point.Y * gridSize + offsetY);
		}

		public PointF GetCoordinate(int x, int y)
		{
			var gridSize = (float) this.numericUpDownGridSize.Value;
			var offsetX = (float) this.numericUpDownOffsetX.Value;
			var offsetY = (float) this.numericUpDownOffsetY.Value;

			return new PointF(x * gridSize + offsetX, y * gridSize + offsetY);
		}
		
		public List<Point> GenerateGrid() {
			// https://github.com/martin2250/GrblHeightProbe2/blob/master/HeightMap.cs
			
			var sizeX = (int) this.numericUpDownPointsX.Value;
			var sizeY = (int) this.numericUpDownPointsY.Value;
			var gridPoints = new List<Point>();

			// generate a sensible grid pattern
			int x = 0;
			while (x < sizeX)
			{
				for (int y = 0; y < sizeY; y++) {
					gridPoints.Add(new Point(x, y));
				}
				
				if (++x >= sizeX) {
					break;
				}
				
				for (int y = sizeY - 1; y >= 0; y--) {
					gridPoints.Add(new Point(x, y));
				}
				
				x++;
			}
			
			return gridPoints;
		}
	}
}
