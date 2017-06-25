/**
 * Copied from the SimpleGcodeParser file
 * Copyright (c) David-John Miller AKA Anoyomouse 2014
 *
 * See LICENCE in the project directory for licence information
 * Modified by perivar@nerseth.com
 **/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;

namespace GCode
{
	public static class SimpleGCodeParser
	{
		public static List<GCodeInstruction> ParseText(string text)
		{
			text = text.Replace("\r\n", "\n");
			text = text.Replace("\r", "\n");

			string[] lines = text.Split('\n');

			var parsed = new List<GCodeInstruction>();

			foreach (string s in lines)
			{
				parsed.Add(ParseLine(s));
			}

			return parsed;
		}

		private static GCodeInstruction ParseLine(string line)
		{
			line = line.Trim(' ', '\t', '\n');
			return new GCodeInstruction(line);
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

	public class GCodeInstruction
	{
		// split line into groups.
		// Supports one letter, optional space, optional minus, at least one number, optional fraction and optional more digits
		// E.g.
		// G2 X 30.0000 Y 30.0000 I 20.0000 J 40.0000
		// or
		// G02 X71.871087 Y23.266043 Z-0.050000 I2198.689889 J-561.348455
		private static Regex GCodeSplitter = new Regex(@"([A-Z])\s*(\-?\d+\.?\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
		public int? M { get; set; }		// 

		internal float minX, minY, minZ;
		internal float maxX, maxY, maxZ;
		internal Point3D StartPoint;
		internal Point3D EndPoint;
		
		public List<LinePoints> CachedLinePoints { get; set; }

		internal bool IsOnlyComment { get; private set; }
		
		internal bool IsEmptyLine {
			get {
				if (string.IsNullOrEmpty(Comment) && string.IsNullOrEmpty(Command)) {
					return true;
				} else {
					return false;
				}
			}
		}
		
		// Lines
		public static List<GCodeInstruction> GetInstructions(CommandList command, Point3D p1, Point3D p2, float feed, Point3D shift, int side, Point3D prevPoint, float zClearance) {
			
			var output = new List<GCodeInstruction>();
			
			bool skipRapidMove = false;
			// if the command is a line and the previous X Y Z and the p1 X Y Z is the same
			//if (prevPoint == p1) {
			// if last command didn't include a Z-coordinate, like a G2 or G3 arc mode
			// only check whether X and Y are similar between prevPoint and p1
			if (prevPoint.X == p1.X && prevPoint.Y == p1.Y) {
				skipRapidMove = true;
			} else if (p1 == p2) {
				skipRapidMove = true;
			}
			
			// add only if the x y (and z?) coordinates are different
			if (!skipRapidMove) {
				
				// never add a rapid move that extend the available space
				if (side == 0) {
					// ensure X is less than shift.X
					if (p1.X > shift.X) p1.X = shift.X;
				} else {
					// side 1
					// ensure X is equal or more than shift.X
					if (p1.X < shift.X) p1.X = shift.X;
				}
				
				// first raise Z to Z clearance (= G0 Za)
				output.Add(new GCodeInstruction(CommandList.RapidMove, null, null, zClearance, null));
				
				// secondly move X and Y to right place  (= G0 Xa Yb)
				output.Add(new GCodeInstruction(CommandList.RapidMove, p1.X, p1.Y, null, null));
			}
			output.Add(new GCodeInstruction(command, p2, feed));
			
			return output;
		}
		
		// Arcs
		public static List<GCodeInstruction> GetInstructions(CommandList command, Point3D p1, Point3D p2, Point3D p3, float feed, Point3D shift, int side, Point3D prevPoint, float zClearance) {
			
			var output = new List<GCodeInstruction>();
			
			bool skipRapidMove = false;
			// if the command is an arc and the previous X Y and the p1 X Y is the same
			if (prevPoint.X == p1.X && prevPoint.Y == p1.Y) {
				skipRapidMove = true;
			} else if (p1 == p2) {
				skipRapidMove = true;
			}
			
			// add only if the x y (and z?) coordinates are different
			if (!skipRapidMove) {

				// never add a rapid move that extend the available space
				if (side == 0) {
					// ensure X is less than shift.X
					if (p1.X > shift.X) p1.X = shift.X;
				} else {
					// side 1
					// ensure X is equal or more than shift.X
					if (p1.X < shift.X) p1.X = shift.X;
				}
				
				// first raise Z to Z clearance (= G0 Za)
				output.Add(new GCodeInstruction(CommandList.RapidMove, null, null, zClearance, null));
				
				// secondly move X and Y to right place  (= G0 Xa Yb)
				output.Add(new GCodeInstruction(CommandList.RapidMove, p1.X, p1.Y, null, null));
			}
			output.Add(new GCodeInstruction(command, p1, p2, p3, feed));
			
			return output;
		}
		
		#region Constructors
		public GCodeInstruction(CommandList command, float? x, float? y, float? z, float? feed) {
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
			
			if (x.HasValue) this.X = x.Value;
			if (y.HasValue) this.Y = y.Value;
			if (z.HasValue) this.Z = z.Value;
			if (command != CommandList.RapidMove && feed.HasValue) this.F = feed.Value;
		}
		
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
								//case "T": T = (int) value; break;
								//case "M": M = (int) value; break;
						}
					}
				} else {
					// empty - ignore
				}
			}
		}
		#endregion
		
		#region Properties
		public bool CanRender
		{
			get {
				if (CommandEnum == CommandList.NormalMove ||
				    CommandEnum == CommandList.RapidMove ||
				    CommandEnum == CommandList.CWArc ||
				    CommandEnum == CommandList.CCWArc) {
					return true;
				}
				return false;
			}
		}
		
		public bool HasXY {
			get {
				return X.HasValue && Y.HasValue;
			}
		}
		
		public PointF PointF {
			get {
				if (HasXY) {
					return new PointF(X.Value, Y.Value);
				} else {
					return PointF.Empty;
				}
			}
		}
		#endregion
		
		/// <summary>
		/// Parse the comments out from the passed line
		/// and store the Comments, return the remaining command if it exists or the full line
		/// </summary>
		/// <param name="line">gcode line</param>
		/// <returns>return the remaining command or the full line</returns>
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
			}
		}

		public CommandList CommandEnum
		{
			get
			{
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
			}
		}

		public override string ToString()
		{
			return this.ToString(false);
		}

		public string ToString(bool doMultiLayer, float? zOverride = null)
		{
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

			if (X.HasValue) sb.AppendFormat(CultureInfo.InvariantCulture, " X{0:0.####}", this.X);
			if (Y.HasValue) sb.AppendFormat(CultureInfo.InvariantCulture, " Y{0:0.####}", this.Y);

			if (Z.HasValue) {
				if (this.Z <= 0) {
					if (doMultiLayer && zOverride.HasValue) {
						sb.AppendFormat(CultureInfo.InvariantCulture, " Z{0:0.####}", zOverride.Value);
					} else {
						sb.AppendFormat(CultureInfo.InvariantCulture, " Z{0:0.####}", this.Z);
					}
				} else {
					sb.AppendFormat(CultureInfo.InvariantCulture, " Z{0:0.####}", this.Z);
				}
			}
			if (I.HasValue) sb.AppendFormat(CultureInfo.InvariantCulture, " I{0:0.####}", this.I);
			if (J.HasValue) sb.AppendFormat(CultureInfo.InvariantCulture, " J{0:0.####}", this.J);
			if (F.HasValue) sb.AppendFormat(CultureInfo.InvariantCulture, " F{0:0.####}", this.F);
			if (P.HasValue) sb.AppendFormat(CultureInfo.InvariantCulture, " P{0:0.####}", this.P);
			if (T.HasValue) sb.AppendFormat(CultureInfo.InvariantCulture, " T{0}", this.T);
			if (M.HasValue) sb.AppendFormat(CultureInfo.InvariantCulture, " M{0}", this.M);

			if (!string.IsNullOrWhiteSpace(Comment)) {
				sb.AppendFormat(" ({0})", Comment);
			}

			return sb.ToString();
		}
		
		internal List<LinePoints> RenderCode(ref Point3D currentPoint)
		{
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
			if (CommandEnum == CommandList.Dwell) {
				// ignore
				return null;
			}

			var pos = new Point3D(currentPoint.X, currentPoint.Y, currentPoint.Z);
			if (AbsoluteMode) {
				if (X.HasValue)
					pos.X = X.Value;
				if (Y.HasValue)
					pos.Y = Y.Value;
				if (Z.HasValue)
					pos.Z = Z.Value;
			} else {
				// relative specifies a delta
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
		}

		private List<LinePoints> RenderArc(Point3D center, Point3D endpoint, bool clockwise, ref Point3D currentPosition)
		{
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
		}

		public StringBuilder GetXY(StringBuilder sb)
		{
			return sb.AppendFormat(" ({0:0.####},{1:0.####} - {2:0.####},{3:0.####})", StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y);
		}

		public bool EqualsXYZ(GCodeInstruction other)
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
	}
}
