/**
 * Copyright (c) David-John Miller AKA Anoyomouse 2014
 *
 * See LICENCE in the project directory for licence information
 * Modified by perivar@nerseth.com
 **/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;

namespace GCodePlotter
{
	#region Point 3D
	public struct Point3D {
		
		private float x;
		private float y;
		private float z;
		
		public static readonly Point3D Empty;
		
		public bool IsEmpty {
			get {
				return this.x == 0f && this.y == 0f && this.z == 0f;
			}
		}

		public float X {
			get {
				return this.x;
			}
			set {
				this.x = value;
			}
		}

		public float Y {
			get {
				return this.y;
			}
			set {
				this.y = value;
			}
		}

		public float Z {
			get {
				return this.z;
			}
			set {
				this.z = value;
			}
		}

		public Point3D(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Point3D(float x, float y)
		{
			this.x = x;
			this.y = y;
			this.z = 0;
		}
		
		public static bool operator ==(Point3D left, Point3D right) {
			return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
		}
		
		public static bool operator !=(Point3D left, Point3D right) {
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Point3D)) {
				return false;
			}
			var point3D = (Point3D)obj;
			return point3D.X == this.X && point3D.Y == this.Y && point3D.Z == this.Z
				&& point3D.GetType().Equals(base.GetType());
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		
		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture,
			                     "{{X={0}, Y={1}, Z={2}}}", new object[] {
			                     	this.x,
			                     	this.y,
			                     	this.z
			                     });
		}
	}
	#endregion
	
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
			if (list == PenColorList.RapidMoveHilight) return Color.Pink;
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
		private static Regex GCodeSplitter = new Regex(@"([A-Z])(\-?\d+\.?\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		// pattern matchers.
		private static Regex parenPattern  = new Regex(@"\((.*)\)", RegexOptions.Compiled);
		private static Regex semiPattern = new Regex(@";(.*)", RegexOptions.Compiled);
		
		public readonly IFormatProvider InvariantCulture = CultureInfo.InvariantCulture;
		
		public const float CurveSection = 1;

		// G90 (absolute positioning)
		public bool AbsoluteMode = true;

		// G90.1 (absolute distance mode for arc centers)
		public bool AbsoluteArcCenterMode = false;
		
		// Metric or Inches?
		public bool Metric = true;
		
		public string Comment { get; set; }
		public string Command { get; set; }
		public float? X { get; set; }	// x coordinate
		public float? Y { get; set; }	// y coordinate
		public float? Z { get; set; }	// z coordinate
		public float? F { get; set; } 	// feedrate
		public float? I { get; set; }	// arc x coordinate
		public float? J { get; set; }	// arc y coordinate
		public float? P { get; set; }
		public int? T { get; set; }		// tool change

		internal float minX, minY, minZ;
		internal float maxX, maxY, maxZ;
		internal Point3D StartPoint;
		internal Point3D EndPoint;
		
		public List<LinePoints> CachedLinePoints { get; set; }

		internal bool IsOnlyComment { get; private set; }
		
		public static List<GCodeInstruction> GetInstructions(CommandList command, Point3D p1, Point3D p2, float feed, Point3D shift, int side, Point3D prevPoint, float zClearance) {
			
			var output = new List<GCodeInstruction>();
			
			bool skip = false;
			// if the command is a line and the previous X Y Z and the p1 X Y Z is the same
			if (prevPoint == p1) {
				skip = true;
			} else if (p1 == p2) {
				skip = true;
			}
			
			// add only if the x y (and z?) coordinates are different
			if (!skip) {
				
				// never add a rapid move that extend the available space
				if (side == 0) {
					// ensure X is less than shift.X
					if (p1.X > shift.X) p1.X = shift.X;
				} else {
					// side 1
					// ensure X is equal or more than shift.X
					if (p1.X < shift.X) p1.X = shift.X;
				}
				
				// make sure to add Z clearance
				p1.Z = zClearance;
				
				output.Add(new GCodeInstruction(CommandList.RapidMove, p1, feed));
			}
			output.Add(new GCodeInstruction(command, p2, feed));
			
			return output;
		}
		
		public static List<GCodeInstruction> GetInstructions(CommandList command, Point3D p1, Point3D p2, Point3D p3, float feed, Point3D shift, int side, Point3D prevPoint, float zClearance) {
			
			var output = new List<GCodeInstruction>();
			
			bool skip = false;
			// if the command is an arc and the previous X Y and the p1 X Y is the same
			if (prevPoint.X == p1.X && prevPoint.Y == p1.Y) {
				skip = true;
			} else if (p1 == p2) {
				skip = true;
			}
			
			// add only if the x y (and z?) coordinates are different
			if (!skip) {

				// never add a rapid move that extend the available space
				if (side == 0) {
					// ensure X is less than shift.X
					if (p1.X > shift.X) p1.X = shift.X;
				} else {
					// side 1
					// ensure X is equal or more than shift.X
					if (p1.X < shift.X) p1.X = shift.X;
				}
				
				// make sure to add Z clearance
				p1.Z = zClearance;
				
				output.Add(new GCodeInstruction(CommandList.RapidMove, p1, feed));
			}
			output.Add(new GCodeInstruction(command, p1, p2, p3, feed));
			
			return output;
		}
		
		#region Constructors
		public GCodeInstruction(CommandList command, Point3D point, float feed) {
			switch (command) {
				case CommandList.RapidMove:
					Command = "G0";
					break;
				case CommandList.NormalMove:
					Command = "G1";
					break;
				case CommandList.CWArc:
					Command = "G2";
					break;
				case CommandList.CCWArc:
					Command = "G3";
					break;
				case CommandList.Other:
					break;
			}
			
			this.X = point.X;
			this.Y = point.Y;
			this.Z = point.Z;
			if (command != CommandList.RapidMove) this.F = feed;
		}
		
		public GCodeInstruction(CommandList command, Point3D p1, Point3D p2, Point3D p3, float feed, bool absoluteArcCenterMode = false) {
			switch (command) {
				case CommandList.RapidMove:
					Command = "G0";
					break;
				case CommandList.NormalMove:
					Command = "G1";
					break;
				case CommandList.CWArc:
					Command = "G2";
					break;
				case CommandList.CCWArc:
					Command = "G3";
					break;
				case CommandList.Other:
					break;
			}
			
			
			this.X = p2.X;
			this.Y = p2.Y;
			if (command != CommandList.RapidMove) this.F = feed;

			if (absoluteArcCenterMode) {
				// if G90.1 (absolute distance mode for arc centers)
				AbsoluteArcCenterMode = true;
				this.I = p3.X;
				this.J = p3.Y;
			} else {
				// relative mode for arc centers
				AbsoluteArcCenterMode = false;
				this.I = p3.X-p1.X;
				this.J = p3.Y-p1.Y;
			}
		}
		
		public GCodeInstruction(string line)
		{
			// parse comments and return the remaining command if any
			string command = ParseComments(line).Trim();
			
			if (!string.IsNullOrEmpty(Comment) && string.IsNullOrEmpty(command)) {
				IsOnlyComment = true;
			} else {
				IsOnlyComment = false;
				
				// use regexp for the part that remains after removing the comment
				// or a line that hasn't a comment at all
				MatchCollection matches = GCodeSplitter.Matches(command.ToUpper());

				if (matches.Count == 1) {
					// only matched the command, store this
					this.Command = matches[0].Groups[0].Value;
				} else if (matches.Count > 1) {
					// matched the command and something more
					this.Command = matches[0].Groups[0].Value;
					
					for (int index = 0; index < matches.Count; index++)
					{
						float value = float.Parse(matches[index].Groups[2].Value, InvariantCulture);

						switch (matches[index].Groups[1].Value)
						{
								case "X": X = value; break; // x
								case "Y": Y = value; break; // y
								case "Z": Z = value; break; // z
								case "F": F = value; break; // feedrate
								case "I": I = value; break; // arc x
								case "J": J = value; break; // arc y
								case "P": P = value; break;
								case "T": T = (int) value; break;
						}
					}
				} else {
					// empty - ignore
				}
			}
		}
		#endregion
		
		private string ParseComments(string line)
		{
			string comment = "";
			string command = line;
			
			MatchCollection parenMatcher = parenPattern.Matches(line);
			MatchCollection semiMatcher = semiPattern.Matches(line);

			// Note that we only support one style of comments, and only one comment per row.
			if (parenMatcher.Count != 0) {
				comment = parenMatcher[0].Groups[1].Value;

				// remove the comments from the command string
				command = parenPattern.Replace(line, "");
			}

			if (semiMatcher.Count != 0) {
				comment = semiMatcher[0].Groups[1].Value;

				// remove the comments from the command string
				command = semiPattern.Replace(line, "");
			}
			
			if (!string.IsNullOrEmpty(comment)) {
				// clean up the comment
				comment = comment.Trim().Replace('|', '\n');

				// and store it
				Comment = comment;
			}
			
			return command;
		}
		
		public string CommandType
		{
			get
			{
				#region Code
				switch (Command)
				{
					case "G0":
					case "G00":
						return "Rapid Move";
					case "G1":
					case "G01":
						return "Coordinated motion";
					case "G2":
					case "G02":
						return "Clockwise arc motion";
					case "G3":
					case "G03":
						return "Counter clockwise arc motion";
					case "G4":
					case "G04":
						return "Dwell";
					case "G90":
						return "Absolute Mode";
					case "G91":
						return "Relative Mode";
					case "G20":
						return "Set Units to Inches";
					case "G21":
						return "Set Units to Millimeters";
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
					case "G00":
						return CommandList.RapidMove;
					case "G1":
					case "G01":
						return CommandList.NormalMove;
					case "G2":
					case "G02":
						return CommandList.CWArc;
					case "G3":
					case "G03":
						return CommandList.CCWArc;
					case "G4":
					case "G04":
						return CommandList.Dwell;
				}
				return CommandList.Other;
				#endregion
			}
		}

		public override string ToString()
		{
			return this.ToString(false);
		}

		public string ToString(bool doMultiLayer, float? zOverride = null)
		{
			#region Code
			if (string.IsNullOrWhiteSpace(Command))
			{
				if (!string.IsNullOrWhiteSpace(Comment)) {
					return string.Format("({0})", Comment);
				} else {
					return string.Empty;
				}
			}

			var sb = new StringBuilder();
			sb.Append(this.Command);

			if (X.HasValue) sb.AppendFormat(" X{0:0.####}", this.X);
			if (Y.HasValue) sb.AppendFormat(" Y{0:0.####}", this.Y);

			if (Z.HasValue) {
				if (this.Z <= 0) {
					if (doMultiLayer && zOverride.HasValue) {
						sb.AppendFormat(" Z{0:0.####}", zOverride.Value);
					} else {
						sb.AppendFormat(" Z{0:0.####}", this.Z);
					}
				} else {
					sb.AppendFormat(" Z{0:0.####}", this.Z);
				}
			}
			if (I.HasValue) sb.AppendFormat(" I{0:0.####}", this.I);
			if (J.HasValue) sb.AppendFormat(" J{0:0.####}", this.J);
			if (F.HasValue) sb.AppendFormat(" F{0:0.####}", this.F);
			if (P.HasValue) sb.AppendFormat(" P{0:0.####}", this.P);
			if (T.HasValue) sb.AppendFormat(" T{0}", this.T);

			if (!string.IsNullOrWhiteSpace(Comment)) {
				sb.AppendFormat(" ({0})", Comment);
			}

			return sb.ToString();
			#endregion
		}

		public StringBuilder GetXY(StringBuilder sb)
		{
			return sb.AppendFormat(" ({0:0.####},{1:0.####} - {2:0.####},{3:0.####})", StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y);
		}

		internal bool CanRender()
		{
			#region Code
			if (CommandEnum == CommandList.NormalMove ||
			    CommandEnum == CommandList.RapidMove ||
			    CommandEnum == CommandList.CWArc ||
			    CommandEnum == CommandList.CCWArc) {
				
				// TODO: If only Z is enabled, ignore
				//if (!X.HasValue && !Y.HasValue && Z.HasValue) {
				//	return false;
				//}
				return true;
			}
			return false;
			#endregion
		}

		internal List<LinePoints> RenderCode(ref Point3D currentPoint)
		{
			#region Code
			if (CommandEnum == CommandList.Other)
			{
				if (Command == "G90") { AbsoluteMode = true; }
				if (Command == "G90.1") { AbsoluteArcCenterMode = true; }
				if (Command == "G91") { AbsoluteMode = false; }
				if (Command == "G91.1") { AbsoluteArcCenterMode = false; }
				if (Command == "G21") { Metric = true; }
				if (Command == "G20") { Metric = false; }
				return null;
			}
			if (CommandEnum == CommandList.Dwell)
				return null;

			var pos = new Point3D(currentPoint.X, currentPoint.Y, currentPoint.Z);
			if (AbsoluteMode)
			{
				if (X.HasValue)
					pos.X = X.Value;
				if (Y.HasValue)
					pos.Y = Y.Value;
				if (Z.HasValue)
					pos.Z = Z.Value;
			}
			// relative specifies a delta
			else
			{
				if (X.HasValue)
					pos.X += X.Value;
				if (Y.HasValue)
					pos.Y += Y.Value;
				if (Z.HasValue)
					pos.Z += Z.Value;
			}

			if (!Metric) {
				pos.X *= 25.4f;
				pos.Y *= 25.4f;
				pos.Z *= 25.4f;
			}
			
			if (CommandEnum == CommandList.RapidMove || CommandEnum == CommandList.NormalMove)
			{
				maxX = Math.Max(currentPoint.X, pos.X);
				maxY = Math.Max(currentPoint.Y, pos.Y);
				maxZ = Math.Max(currentPoint.Z, pos.Z);

				minX = Math.Min(currentPoint.X, pos.X);
				minY = Math.Min(currentPoint.Y, pos.Y);
				minZ = Math.Min(currentPoint.Z, pos.Z);

				StartPoint = new Point3D(currentPoint.X, currentPoint.Y, currentPoint.Z);
				EndPoint = new Point3D(pos.X, pos.Y, pos.Z);

				var line = new LinePoints(currentPoint, pos, CommandEnum == CommandList.RapidMove ? PenColorList.RapidMove : PenColorList.NormalMove);
				currentPoint.X = pos.X;
				currentPoint.Y = pos.Y;
				currentPoint.Z = pos.Z;
				return new List<LinePoints>() { line };
			}

			if (CommandEnum == CommandList.CWArc || CommandEnum == CommandList.CCWArc)
			{
				var center = new Point3D(0f, 0f, 0f);
				var current = new Point3D(currentPoint.X, currentPoint.Y, currentPoint.Z);
				
				if (AbsoluteArcCenterMode) {
					center.X = this.I ?? 0;
					center.Y = this.J ?? 0;
				} else {
					center.X = current.X + this.I ?? 0;
					center.Y = current.Y + this.J ?? 0;
				}

				minX = currentPoint.X;
				minY = currentPoint.Y;
				minZ = currentPoint.Z;

				StartPoint = new Point3D(current.X, current.Y, current.Z);

				var arcPoints = RenderArc(center, pos, (CommandEnum == CommandList.CWArc), ref currentPoint);
				EndPoint = new Point3D(currentPoint.X, currentPoint.Y, currentPoint.Z);
				
				return arcPoints;
			}

			return null;
			#endregion
		}

		private List<LinePoints> RenderArc(Point3D center, Point3D endpoint, bool clockwise, ref Point3D currentPosition)
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
			var current = new Point3D(currentPosition.X, currentPosition.Y, currentPosition.Z);
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
			if (angleB <= angleA) {
				angleB += 2 * Math.PI;
			}
			
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
			var newPoint = new Point3D(current.X, current.Y, current.Z);
			var lastPoint = new Point3D(current.X, current.Y, current.Z);
			var output = new List<LinePoints>();
			var p = clockwise ? PenColorList.CWArc : PenColorList.CCWArc;
			for (s = 1; s <= steps; s++)
			{
				// Forwards for CCW, backwards for CW
				if (!clockwise) {
					step = s;
				} else {
					step = steps - s;
				}

				// calculate our waypoint.
				newPoint.X = (float)((center.X + radius * Math.Cos(angleA + angle * ((double)step / steps))));
				newPoint.Y = (float)((center.Y + radius * Math.Sin(angleA + angle * ((double)step / steps))));
				
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

		protected bool EqualsXYZ(GCodeInstruction other)
		{
			if (other == null) return false;
			
			if (X.HasValue && other.X.HasValue && X.Value == other.X.Value
			    && Y.HasValue && other.Y.HasValue && Y.Value == other.Y.Value
			    && Z.HasValue && other.Z.HasValue && Z.Value == other.Z.Value)
			{
				return true;
			}
			
			return false;
		}
		
		/*
		public override bool Equals(object obj)
		{
			var item = obj as GCodeInstruction;
			return Equals(item);
		}

		public override int GetHashCode()
		{
			// see http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
			unchecked // Overflow is fine, just wrap
			{
				int hashCode = (int) 2166136261;
				hashCode = (hashCode * 16777619) ^ X.GetHashCode();
				hashCode = (hashCode * 16777619) ^ Y.GetHashCode();
				hashCode = (hashCode * 16777619) ^ Z.GetHashCode();
				return hashCode;
			}
		}
		 */
	}

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
			#region Code
			X1 = start.X;
			Y1 = start.Y;
			Z1 = start.Z;
			X2 = end.X;
			Y2 = end.Y;
			Z2 = end.Z;
			Pen = pen;
			#endregion
		}

		public void DrawSegment(Graphics g, int height, bool highlight = false, float Multiplier = 1, bool renderG0 = true, int left = 0, int bottom = 0)
		{
			#region Code
			if (Pen == PenColorList.RapidMove && !renderG0)
			{
				return;
			}
			
			if (Pen == PenColorList.RapidMove && highlight)
			{
				g.DrawLine(ColorHelper.GetPen(PenColorList.RapidMoveHilight), X1 * Multiplier + left, height - (Y1 * Multiplier) - bottom, X2 * Multiplier + left, height - (Y2 * Multiplier) - bottom);
				return;
			}

			if (highlight)
			{
				g.DrawLine(ColorHelper.GetPen(PenColorList.LineHighlight), X1 * Multiplier + left, height - (Y1 * Multiplier) - bottom, X2 * Multiplier + left, height - (Y2 * Multiplier) - bottom);
			}
			else
			{
				g.DrawLine(ColorHelper.GetPen(Pen), X1 * Multiplier + left, height - (Y1 * Multiplier) - bottom, X2 * Multiplier + left, height - (Y2 * Multiplier) - bottom);
			}
			#endregion
		}
	}

	public class Plot
	{
		public float minX, minY, minZ;
		public float maxX, maxY, maxZ;

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
				maxZ = first.maxZ;

				minX = first.StartPoint.X;
				minY = first.StartPoint.Y;
				minZ = first.StartPoint.Z;
			}

			foreach (var plot in gcodeInstructions)
			{
				minX = Math.Min(minX, plot.minX);
				minY = Math.Min(minY, plot.minY);
				minZ = Math.Min(minZ, plot.minZ);

				maxX = Math.Max(maxX, plot.maxX);
				maxY = Math.Max(maxY, plot.maxY);
				maxZ = Math.Max(maxZ, plot.maxZ);
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("{0}   --   {1} lines -- {2},{3}", Name, PlotPoints != null ? PlotPoints.Count : 0, maxX, maxY);

			return sb.ToString();
		}

		public void Replot(ref Point3D currentPoint)
		{
			PlotPoints.Clear();
			foreach (var line in this.GCodeInstructions)
			{
				if (line.CanRender())
				{
					line.CachedLinePoints = line.RenderCode(ref currentPoint);
					PlotPoints.AddRange(line.CachedLinePoints);
				}
			}

			FinalizePlot();
		}

		public string BuildGCodeOutput(bool doMultiLayers) {
			return BuildGCodeOutput(this.Name, this.GCodeInstructions, doMultiLayers);
		}
		
		public static string BuildGCodeOutput(string name, List<GCodeInstruction> gCodeInstructions, bool doMultiLayers)
		{
			var sb = new StringBuilder();

			sb.AppendFormat("(Start cutting path id: {0})", name).AppendLine();

			if (doMultiLayers) {
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

				foreach(var line in bits) {
					float f;
					if (float.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out f)) {
						sb.AppendFormat(CultureInfo.InvariantCulture, "(Start layer: {0:0.####})", f).AppendLine();
						foreach (var gCodeLine in gCodeInstructions) {
							sb.AppendLine(gCodeLine.ToString(true, zOverride: f));
						}
						sb.AppendFormat(CultureInfo.InvariantCulture, "(End layer: {0:0.####})", f).AppendLine();
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
