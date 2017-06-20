/**
 * Copied from the SimpleGcodeParser file
 * Copyright (c) David-John Miller AKA Anoyomouse 2014
 *
 * See LICENCE in the project directory for licence information
 * Modified by perivar@nerseth.com
 **/
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GCode
{
	public struct LinePoints
	{
		public float X1 { get; set; }
		public float Y1 { get; set; }
		public float Z1 { get; set; }
		public float X2 { get; set; }
		public float Y2 { get; set; }
		public float Z2 { get; set; }
		public PenColorList Pen { get; set; }
		
		public LinePoints(Point3D start, Point3D end, PenColorList pen)
			: this()
		{
			X1 = start.X;
			Y1 = start.Y;
			Z1 = start.Z;
			X2 = end.X;
			Y2 = end.Y;
			Z2 = end.Z;
			Pen = pen;
		}

		public void DrawSegment(Graphics g, int height, bool highlight = false, float multiplier = 1, bool renderG0 = true, int left = 0, int bottom = 0)
		{
			if (Pen == PenColorList.RapidMove && !renderG0) {
				return;
			}
			
			if (Pen == PenColorList.RapidMove && highlight) {
				var rapidPen = ColorHelper.GetPen(PenColorList.RapidMoveHilight);
				float[] dashValues = { 5, 2 };
				rapidPen.DashPattern = dashValues;
				g.DrawLine(rapidPen, X1 * multiplier + left, height - (Y1 * multiplier) - bottom, X2 * multiplier + left, height - (Y2 * multiplier) - bottom);
				return;
			}

			if (highlight) {
				g.DrawLine(ColorHelper.GetPen(PenColorList.LineHighlight), X1 * multiplier + left, height - (Y1 * multiplier) - bottom, X2 * multiplier + left, height - (Y2 * multiplier) - bottom);
			} else {
				if (Pen == PenColorList.RapidMove) {
					var rapidPen = ColorHelper.GetPen(Pen);
					float[] dashValues = { 5, 2 };
					rapidPen.DashPattern = dashValues;
					g.DrawLine(rapidPen, X1 * multiplier + left, height - (Y1 * multiplier) - bottom, X2 * multiplier + left, height - (Y2 * multiplier) - bottom);
				} else {
					g.DrawLine(ColorHelper.GetPen(Pen), X1 * multiplier + left, height - (Y1 * multiplier) - bottom, X2 * multiplier + left, height - (Y2 * multiplier) - bottom);
				}
			}
		}
		
		public override string ToString()
		{
			return string.Format("[{0:0.##}, {1:0.##}, {2:0.##}] - [{3:0.##}, {4:0.##}, {5:0.##}]", X1, Y1, Z1, X2, Y2, Z2);
		}
	}

	// A section of gcode instructions together is called a block
	// A block includes a number of LinePoints that is used to draw the plot
	public class Block
	{
		private float minX, minY, minZ;
		private float maxX, maxY, maxZ;
		
		public float MinX { get { return this.minX; } }
		public float MaxX { get { return this.maxX; } }
		public float MinY { get { return this.minY; } }
		public float MaxY { get { return this.maxY; } }
		public float MinZ { get { return this.minZ; } }
		public float MaxZ { get { return this.maxZ; } }
		
		public string Name { get; set; }
		
		private List<LinePoints> plotPoints = new List<LinePoints>();
		public List<LinePoints> PlotPoints { get { return plotPoints; } }

		private List<GCodeInstruction> gcodeInstructions = new List<GCodeInstruction>();
		public List<GCodeInstruction> GCodeInstructions { get { return gcodeInstructions; } }
		
		/// <summary>
		/// Calculate Min and Max values based on all included gcode instructions
		/// </summary>
		public void CalculateMinAndMax()
		{
			// why was this needed in the first place?
			/*
			var first = gcodeInstructions.First();
			if (first != null)
			{
				maxX = first.maxX;
				maxY = first.maxY;
				maxZ = first.maxZ;

				minX = first.StartPoint.X;
				minY = first.StartPoint.Y;
				minZ = first.StartPoint.Z;
			}
			 */

			foreach (var currentInstruction in gcodeInstructions)
			{
				minX = Math.Min(minX, currentInstruction.minX);
				minY = Math.Min(minY, currentInstruction.minY);
				minZ = Math.Min(minZ, currentInstruction.minZ);

				maxX = Math.Max(maxX, currentInstruction.maxX);
				maxY = Math.Max(maxY, currentInstruction.maxY);
				maxZ = Math.Max(maxZ, currentInstruction.maxZ);
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("{0}  --  {1} cmds -- Max X/Y : {2:0.####}, {3:0.####} -- ({4})", Name, GCodeInstructions.Count, maxX, maxY, PlotPoints != null ? PlotPoints.Count : 0);

			return sb.ToString();
		}

		public string BuildGCodeOutput(bool doPeckDrilling) {
			return BuildGCodeOutput(this.Name, this.GCodeInstructions, doPeckDrilling);
		}
		
		public static string BuildGCodeOutput(string name, List<GCodeInstruction> gCodeInstructions, bool doPeckDrilling)
		{
			var sb = new StringBuilder();

			sb.AppendFormat("(Start cutting path id: {0})", name).AppendLine();

			if (doPeckDrilling) {
				var data = QuickSettings.Get["ZDepths"];
				if (string.IsNullOrEmpty(data))
				{
					data = "-0.1,-0.15,-0.2";
				}

				string [] bits = null;
				if (data.Contains(',')) {
					bits = data.Split(',');
				} else {
					bits = new string[] { data };
				}

				string lastLine = "";
				foreach(var line in bits) {
					float f;
					if (float.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out f)) {
						sb.AppendFormat(CultureInfo.InvariantCulture, "(Start peck drilling: {0:0.####})", f).AppendLine();
						foreach (var gCodeLine in gCodeInstructions) {
							string newLine = gCodeLine.ToString(true, zOverride: f);
							
							// ensure we don't duplicate lines
							if (!newLine.Equals(lastLine)) {
								sb.AppendLine(newLine);
								lastLine = newLine;
							}
						}
						sb.AppendFormat(CultureInfo.InvariantCulture, "(End peck drilling: {0:0.####})", f).AppendLine();
					}
				}
			} else {
				foreach (var line in gCodeInstructions) {
					sb.AppendLine(line.ToString(false));
				}
			}

			sb.AppendFormat(CultureInfo.InvariantCulture, "(End cutting path id: {0})", name).AppendLine();

			return sb.ToString();
		}
	}
}
