/**
 * Copyright (c) David-John Miller AKA Anoyomouse 2014
 *
 * See LICENCE in the project directory for licence information
 **/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GCodePlotter
{
	public static class SimpleGCodeParser
	{
		public static List<GCodeInstruction> ParseText(string text)
		{
			#region Code
			text = text.Replace("\r\n", "\n");
			text = text.Replace("\r", "\n");

			string[] lines = text.Split('\n');

			var parsed = new List<GCodeInstruction>();

			foreach (string s in lines)
			{
				parsed.Add(ParseLine(s));
			}

			return parsed;
			#endregion
		}

		private static GCodeInstruction ParseLine(string line)
		{
			#region Code
			line = line.Trim(' ', '\t', '\n');
			return new GCodeInstruction(line);
			#endregion
		}
	}

	public enum CommandList
	{
		RapidMove = 0,
		NormalMove = 1,
		CWArc = 2,
		CCWArc = 3,
		Dwell = 4,
		Other = 99
	}

	public enum PenColorList
	{
		RapidMove,
		NormalMove,
		CWArc,
		CCWArc,
		RapidMoveHilight,
		LineHighlight,
		Background,
		GridLines
	}

	public static class ColorHelper
	{
		private static IDictionary<PenColorList, Pen> _penList = new Dictionary<PenColorList, Pen>();
		private static IDictionary<PenColorList, Color> _colorList = new Dictionary<PenColorList, Color>();

		private static Color GetDefaultColor(PenColorList list)
		{
			if (list == PenColorList.RapidMove) return Color.Red;
			if (list == PenColorList.NormalMove) return Color.DodgerBlue;
			if (list == PenColorList.CWArc) return Color.Lime;
			if (list == PenColorList.CCWArc) return Color.Yellow;
			if (list == PenColorList.RapidMoveHilight) return Color.Salmon;
			if (list == PenColorList.LineHighlight) return Color.White;
			if (list == PenColorList.Background) return Color.FromArgb(0x20, 0x20, 0x20);
			if (list == PenColorList.GridLines) return Color.DimGray;
			return Color.White;
		}

		public static Color GetColor(PenColorList type)
		{
			#region Code
			if (!_colorList.ContainsKey(type))
			{
				var value = QuickSettings.Get[string.Format("Color{0}", type)];
				if (string.IsNullOrWhiteSpace(value))
				{
					_colorList[type] = GetDefaultColor(type);
				}
				else
				{
					try
					{
						if (value.Contains(','))
						{
							var bits = value.Split(',');
							var r = int.Parse(bits[0]);
							var g = int.Parse(bits[1]);
							var b = int.Parse(bits[2]);
							_colorList[type] = Color.FromArgb(r, g, b);
						}
						else
						{
							_colorList[type] = Color.FromName(value);
						}
					}
					catch (Exception)
					{
						_colorList[type] = GetDefaultColor(type);
					}
				}
			}

			return _colorList[type];
			#endregion
		}

		public static void SetColor(PenColorList type, Color newColor)
		{
			#region Code
			var value = Convert.ToString(newColor);
			if (_colorList.ContainsKey(type))
			{
				_colorList[type] = newColor;
				if (_penList.ContainsKey(type)) _penList[type] = new Pen(newColor, 1);
				else _penList.Add(type, new Pen(newColor, 1));
			}
			else
			{
				_colorList.Add(type, newColor);
				_penList.Add(type, new Pen(newColor, 1));
			}

			QuickSettings.Get[string.Format("Color{0}", type)] = string.Format("{0},{1},{2}", newColor.R, newColor.G, newColor.B);
			#endregion
		}

		public static Pen GetPen(PenColorList type)
		{
			#region Code
			if (!_penList.ContainsKey(type))
			{
				if (type == PenColorList.LineHighlight)
					_penList[type] = new Pen(GetColor(type), 2f);
				else
					_penList[type] = new Pen(GetColor(type), 1f);
			}

			return _penList[type];
			#endregion
		}
	}

	public class GCodeInstruction
	{
		public readonly IFormatProvider InvariantCulture = CultureInfo.InvariantCulture;

		public const float CurveSection = 1;

		public static bool AbsoluteMode = true;

		public GCodeInstruction(string line)
		{
			#region Code
			if (line.StartsWith("(") && line.EndsWith(")"))
			{
				this.Comment = line.Trim('(', ')');
				IsOnlyComment = true;
				return;
			}

			IsOnlyComment = false;
			if (line.Contains('('))
			{
				var fst = line.IndexOf('(');
				var nxt = line.IndexOf(')', fst);
				this.Comment = line.Substring(fst + 1, nxt - fst - 1);

				if (nxt + 1 < line.Length)
				{
					line = string.Format(InvariantCulture, "{0} {1}", line.Substring(0, fst - 1), line.Substring(nxt + 1));
				}
				else
				{
					line = line.Substring(0, fst - 1);
				}

				line = line.Trim(' ', '\t', '\n');
			}

			if (!line.Contains(' '))
			{
				this.Command = line;
			}

			string[] bits = line.Split(' ');
			this.Command = bits[0];
			for (int i = 1; i < bits.Length; i++)
			{
				char axis = bits[i][0];
				float? dist = null;

				if (bits[i].Length == 1) // Only Axis, so dist is in next field!
				{
					i++;
					if (i >= bits.Length)
					{
						throw new ParsingException(string.Format(InvariantCulture, "No distance specified for {0}", axis));
					}

					dist = float.Parse(bits[i], InvariantCulture);
				}
				else
				{
					dist = float.Parse(bits[i].Substring(1), InvariantCulture);
				}

				if (!dist.HasValue) throw new ParsingException(string.Format(InvariantCulture, "No distance specified for {0}", axis));

				if (dist.HasValue) { dist = dist.Value; } // * Multiplier

				switch (char.ToUpper(axis))
				{
						case 'X': this.X = dist; break;
						case 'Y': this.Y = dist; break;
						case 'Z': this.Z = dist; break;
						case 'F': this.F = dist; break;
						case 'I': this.I = dist; break;
						case 'J': this.J = dist; break;
						case 'P': this.P = dist; break;
				}
			}
			#endregion
		}

		public string Comment { get; set; }
		public string Command { get; set; }
		public float? X { get; set; }
		public float? Y { get; set; }
		public float? Z { get; set; }
		public float? F { get; set; }
		public float? I { get; set; }
		public float? J { get; set; }
		public float? P { get; set; }

		internal float minX, minY;
		internal float maxX, maxY;
		internal PointF StartPoint;
		internal PointF EndPoint;

		public string CommandType
		{
			get
			{
				#region Code
				switch (Command)
				{
					case "G0":
						case "G00": return "Rapid Move";
					case "G1":
						case "G01": return "Coordinated motion";
					case "G2":
						case "G02": return "Clockwise arc motion";
					case "G3":
						case "G03": return "Counter clockwise arc motion";
					case "G4":
						case "G04": return "Dwell";
						case "G90": return "Absolute Mode";
						case "G91": return "Relative Mode";
						case "G21": return "G21";
				}
				return "Unknown " + Command;
				#endregion
			}
		}

		public CommandList CommandEnum
		{
			get
			{
				#region Code
				switch (Command)
				{
					case "G0":
						case "G00": return CommandList.RapidMove;
					case "G1":
						case "G01": return CommandList.NormalMove;
					case "G2":
						case "G02": return CommandList.CWArc;
					case "G3":
						case "G03": return CommandList.CCWArc;
					case "G4":
						case "G04": return CommandList.Dwell;
				}
				return CommandList.Other;
				#endregion
			}
		}

		public override string ToString()
		{
			return this.ToString(false);
		}

		public string ToString(bool multiLayer, float? zoverride = null)
		{
			#region Code
			if (string.IsNullOrWhiteSpace(Command))
			{
				return string.Format("({0})", Comment);
			}

			var sb = new StringBuilder();
			sb.Append(this.Command);

			if (X.HasValue) sb.AppendFormat(" X {0:F4}", this.X);
			if (Y.HasValue) sb.AppendFormat(" Y {0:F4}", this.Y);

			if (Z.HasValue)
			{
				if (this.Z <= 0)
				{
					if (multiLayer && zoverride.HasValue)
						sb.AppendFormat(" Z {0:F4}", zoverride.Value);
					else
						sb.AppendFormat(" Z {0:F4}", this.Z);
				}
				else
					sb.AppendFormat(" Z {0:F4}", this.Z);
			}
			if (I.HasValue) sb.AppendFormat(" I {0:F4}", this.I);
			if (J.HasValue) sb.AppendFormat(" J {0:F4}", this.J);
			if (F.HasValue) sb.AppendFormat(" F {0:F4}", this.F);
			if (P.HasValue) sb.AppendFormat(" P {0:F4}", this.P);

			if (!string.IsNullOrWhiteSpace(Comment))
				sb.AppendFormat(" ({0})", Comment);

			return sb.ToString();
			#endregion
		}

		private StringBuilder GetXY(StringBuilder sb)
		{
			return sb.AppendFormat(" ({0:F4},{1:F4} - {2:F4},{3:F4})", StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y);
		}

		internal bool IsOnlyComment { get; private set; }

		internal bool CanRender()
		{
			#region Code
			if (CommandEnum == CommandList.NormalMove ||
			    CommandEnum == CommandList.RapidMove ||
			    CommandEnum == CommandList.CWArc ||
			    CommandEnum == CommandList.CCWArc) //||
				//CommandEnum == CommandList.Dwell)
				return true;
			return false;
			#endregion
		}

		public List<LinePoints> CachedLinePoints { get; set; }

		internal List<LinePoints> RenderCode(ref PointF currentPoint)
		{
			#region Code
			if (CommandEnum == CommandList.Other)
			{
				if (Command == "G90") { AbsoluteMode = true; }
				if (Command == "G91") { AbsoluteMode = false; }
				return null;
			}
			if (CommandEnum == CommandList.Dwell)
				return null;

			var pos = new PointF(currentPoint.X, currentPoint.Y);
			if (AbsoluteMode)
			{
				if (X.HasValue)
					pos.X = X.Value;
				if (Y.HasValue)
					pos.Y = Y.Value;
				//if (Z.HasValue)
				//	pos.Z = (int)(Z.Value * Multiplier);
			}
			// relative specifies a delta
			else
			{
				if (X.HasValue)
					pos.X += X.Value;
				if (Y.HasValue)
					pos.Y += Y.Value;
				//if (Z.HasValue)
				//	pos.Z += (int)(Z.Value * Multiplier);
			}

			if (CommandEnum == CommandList.RapidMove || CommandEnum == CommandList.NormalMove)
			{
				maxX = Math.Max(currentPoint.X, pos.X);
				maxY = Math.Max(currentPoint.Y, pos.Y);

				minX = Math.Min(currentPoint.X, pos.X);
				minY = Math.Min(currentPoint.Y, pos.Y);

				StartPoint = new PointF(currentPoint.X, currentPoint.Y);
				EndPoint = new PointF(pos.X, pos.Y);

				var line = new LinePoints(currentPoint, pos, CommandEnum == CommandList.RapidMove ? PenColorList.RapidMove : PenColorList.NormalMove);
				currentPoint.X = pos.X;
				currentPoint.Y = pos.Y;
				return new List<LinePoints>() { line };
			}

			if (CommandEnum == CommandList.CWArc || CommandEnum == CommandList.CCWArc)
			{
				var center = new PointF(0f, 0f);
				var current = new PointF(currentPoint.X, currentPoint.Y);
				center.X = current.X + this.I ?? 0;
				center.Y = current.Y + this.J ?? 0;

				minX = currentPoint.X;
				minY = currentPoint.Y;

				StartPoint = new PointF(current.X, current.Y);

				var arcPoints = RenderArc(center, pos, (CommandEnum == CommandList.CWArc), ref currentPoint);
				EndPoint = new PointF(currentPoint.X, currentPoint.Y);

				return arcPoints;
			}

			return null;
			#endregion
		}

		private List<LinePoints> RenderArc(PointF center, PointF endpoint, bool clockwise, ref PointF currentPosition)
		{
			#region Code
			// angle variables.
			double angleA;
			double angleB;
			double angle;
			double radius;
			double length;

			// delta variables.
			double aX;
			double aY;
			double bX;
			double bY;

			// figure out our deltas
			var current = new PointF(currentPosition.X, currentPosition.Y);
			aX = current.X - center.X;
			aY = current.Y - center.Y;
			bX = endpoint.X - center.X;
			bY = endpoint.Y - center.Y;

			// Clockwise
			if (clockwise)
			{
				angleA = Math.Atan2(bY, bX);
				angleB = Math.Atan2(aY, aX);
			}
			// Counterclockwise
			else
			{
				angleA = Math.Atan2(aY, aX);
				angleB = Math.Atan2(bY, bX);
			}

			// Make sure angleB is always greater than angleA
			// and if not add 2PI so that it is (this also takes
			// care of the special case of angleA == angleB,
			// ie we want a complete circle)
			if (angleB <= angleA)
				angleB += 2 * Math.PI;
			angle = angleB - angleA;
			// calculate a couple useful things.
			radius = Math.Sqrt(aX * aX + aY * aY);
			length = radius * angle;

			// for doing the actual move.
			int steps;
			int s;
			int step;

			// Maximum of either 2.4 times the angle in radians
			// or the length of the curve divided by the curve section constant
			steps = (int)Math.Ceiling(Math.Max(angle * 2.4, length / CurveSection));

			// this is the real draw action.
			var newPoint = new PointF(current.X, current.Y);
			var lastPoint = new PointF(current.X, current.Y);
			var output = new List<LinePoints>();
			var p = clockwise ? PenColorList.CWArc : PenColorList.CCWArc;
			for (s = 1; s <= steps; s++)
			{
				// Forwards for CCW, backwards for CW
				if (!clockwise)
					step = s;
				else
					step = steps - s;

				// calculate our waypoint.
				newPoint.X = (float)((center.X + radius * Math.Cos(angleA + angle * ((double)step / steps))));
				newPoint.Y = (float)((center.Y + radius * Math.Sin(angleA + angle * ((double)step / steps))));
				//newPoint.setZ(arcStartZ + (endpoint.z() - arcStartZ) * s / steps);
				
				maxX = Math.Max(maxX, newPoint.X);
				maxY = Math.Max(maxY, newPoint.Y);

				minX = Math.Min(minX, newPoint.X);
				minY = Math.Min(minY, newPoint.Y);

				output.Add(new LinePoints(currentPosition, newPoint, p));
				// start the move
				currentPosition.X = newPoint.X;
				currentPosition.Y = newPoint.Y;
			}
			return output;
			#endregion
		}
	}

	public struct LinePoints
	{
		public LinePoints(PointF start, PointF end, PenColorList pen)
			: this()
		{
			#region Code
			X1 = start.X;
			Y1 = start.Y;
			X2 = end.X;
			Y2 = end.Y;
			Pen = pen;
			#endregion
		}

		public LinePoints(float x1, float y1, float x2, float y2, PenColorList pen)
			: this()
		{
			#region Code
			X1 = x1;
			Y1 = y1;
			X2 = x2;
			Y2 = y2;
			Pen = pen;
			#endregion
		}

		public float X1 { get; set; }
		public float Y1 { get; set; }
		public float X2 { get; set; }
		public float Y2 { get; set; }
		public PenColorList Pen { get; set; }

		public void DrawSegment(Graphics g, int height, bool highlight = false, float Multiplier = 1, bool renderG0 = true, int left = 0, int top = 0)
		{
			#region Code
			if (Pen == PenColorList.RapidMove && !renderG0)
			{
				return;
			}
			else if (Pen == PenColorList.RapidMove && highlight)
			{
				g.DrawLine(ColorHelper.GetPen(PenColorList.RapidMoveHilight), (X1 - left) * Multiplier, height - ((Y1 - top) * Multiplier), (X2 - left) * Multiplier, height - ((Y2 - top) * Multiplier));
				return;
			}

			if (highlight)
			{
				g.DrawLine(ColorHelper.GetPen(PenColorList.LineHighlight), (X1 - left) * Multiplier, height - ((Y1 - top) * Multiplier), (X2 - left) * Multiplier, height - ((Y2 - top) * Multiplier));
			}
			else
			{
				g.DrawLine(ColorHelper.GetPen(Pen), (X1 - left) * Multiplier, height - ((Y1 - top) * Multiplier), (X2 - left) * Multiplier, height - ((Y2 - top) * Multiplier));
			}
			#endregion
		}
	}

	public class Plot
	{
		public string Name { get; set; }
		private List<LinePoints> plotPoints = new List<LinePoints>();
		public List<LinePoints> PlotPoints { get { return plotPoints; } }

		private List<GCodeInstruction> gcodeInstructions = new List<GCodeInstruction>();
		public List<GCodeInstruction> GCodeInstructions { get { return gcodeInstructions; } }

		public void FinalizePlot()
		{
			var first = gcodeInstructions.First();
			if (first != null)
			{
				maxX = first.maxX;
				maxY = first.maxY;

				minX = first.StartPoint.X;
				minY = first.StartPoint.Y;
			}

			foreach (var plot in gcodeInstructions)
			{
				minX = Math.Min(minX, plot.minX);
				minY = Math.Min(minY, plot.minY);

				maxX = Math.Max(maxX, plot.maxX);
				maxY = Math.Max(maxY, plot.maxY);
			}
		}

		public float minX, minY;
		public float maxX, maxY;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("{0}   --   {1} lines -- {2},{3}", Name, PlotPoints != null ? PlotPoints.Count : 0, maxX, maxY);

			return sb.ToString();
		}

		public void Replot(ref PointF currentPoint)
		{
			this.PlotPoints.Clear();
			foreach (var line in this.GCodeInstructions)
			{
				if (line.CanRender())
				{
					line.CachedLinePoints = line.RenderCode(ref currentPoint);
					this.PlotPoints.AddRange(line.CachedLinePoints);
				}
			}

			this.FinalizePlot();
		}

		public string BuildGCodeOutput(bool multilayer)
		{
			var sb = new StringBuilder();

			sb.AppendFormat("(Start cutting path id: {0})", this.Name).AppendLine();

			if (multilayer)
			{
				var data = QuickSettings.Get["ZDepths"];
				if (string.IsNullOrEmpty(data))
				{
					data = "-0.1,-0.15,-0.2";
				}

				string [] bits = null;
				if (data.Contains(','))
					bits = data.Split(',');
				else
					bits = new string[] { data };

				foreach(var line in bits)
				{
					float f;
					if (float.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out f))
					{
						sb.AppendFormat(CultureInfo.InvariantCulture, "(Start layer: {0:F4})", f).AppendLine();
						foreach (var gCodeLine in this.GCodeInstructions)
						{
							sb.AppendLine(gCodeLine.ToString(true, zoverride: f));
						}
						sb.AppendFormat(CultureInfo.InvariantCulture, "(End layer: {0:F4})", f).AppendLine();
					}
				}
			}
			else
			{
				foreach (var line in this.GCodeInstructions)
				{
					sb.AppendLine(line.ToString(false));
				}
			}

			sb.AppendFormat(CultureInfo.InvariantCulture, "(End cutting path id: {0})", this.Name).AppendLine();

			return sb.ToString();
		}
	}

	public class ParsingException : Exception
	{
		public ParsingException(string message)
			: base(message)
		{

		}

		public ParsingException(string message, Exception inner)
			: base(message, inner)
		{

		}
	}
}
