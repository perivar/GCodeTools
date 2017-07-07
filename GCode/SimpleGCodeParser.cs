/**
 * Copied from the SimpleGcodeParser file
 * Copyright (c) David-John Miller AKA Anoyomouse 2014
 * Heavily Modified by perivar@nerseth.com
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

			// keep an overview of the last command and position
			string lastCommand = string.Empty;
			float lastX = 0.0f;
			float lastY = 0.0f;
			float lastZ = 0.0f;
			float lastF = 0.0f;
			
			// try to parse multi-line instructions
			foreach (string s in lines) {
				var currentInstruction = ParseLine(s);
				
				// includes X, Y or Z but missing movement command
				// therefore use the last command instead
				if ((currentInstruction.X.HasValue || currentInstruction.Y.HasValue || currentInstruction.Z.HasValue)
				    && !currentInstruction.Command.StartsWith("G")) {
					//currentInstruction.Command = lastCommand;
					currentInstruction.Command = "G1";
				}

				// for arcs it's important that both I and J is used, even if they are zero
				if (currentInstruction.CommandType == CommandType.CCWArc
				    || currentInstruction.CommandType == CommandType.CWArc) {
					if (!currentInstruction.I.HasValue) currentInstruction.I = 0.0f;
					if (!currentInstruction.J.HasValue) currentInstruction.J = 0.0f;
				}

				// always add instruction
				parsed.Add(currentInstruction);
				
				// maintain correct point coordinates by storing last values
				if ((currentInstruction.X.HasValue || currentInstruction.Y.HasValue || currentInstruction.Z.HasValue
				     || currentInstruction.F.HasValue)) {
					
					if (currentInstruction.X.HasValue && currentInstruction.X.Value != lastX) {
						lastX = currentInstruction.X.Value;
					}
					if (currentInstruction.Y.HasValue && currentInstruction.Y.Value != lastY) {
						lastY = currentInstruction.Y.Value;
					}
					if (currentInstruction.Z.HasValue && currentInstruction.Z.Value != lastZ) {
						lastZ = currentInstruction.Z.Value;
					}
					if (currentInstruction.F.HasValue && currentInstruction.F.Value != lastF) {
						lastF = currentInstruction.F.Value;
					}
				}
				// maintain correct movement by storing last values
				if (currentInstruction.Command != null && !currentInstruction.Command.Equals(lastCommand)) {
					lastCommand = currentInstruction.Command;
				}
			}

			return parsed;
		}

		private static GCodeInstruction ParseLine(string line)
		{
			line = line.Trim(' ', '\t', '\n');
			return new GCodeInstruction(line);
		}
	}

	public enum CommandType
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
		// N46 G1 Z-.5 F10.
		//private static Regex GCodeSplitter = new Regex(@"([A-Z])\s*(\-?\d+\.?\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		//private static Regex GCodeSplitter = new Regex(@"([ABDEFGHIJKLMPQRSTXYZ])\s*(\-?\d+\.?\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		
		// https://github.com/cncgoko/Goko/blob/master/org.goko.gcode.rs274ngcv3/src/org/goko/core/gcode/rs274ngcv3/parser/GCodeTokenType.java
		// https://github.com/cncgoko/Goko/blob/master/org.goko.gcode.rs274ngcv3/src/org/goko/core/gcode/rs274ngcv3/parser/GCodeLexer.java
		private static Regex GCodeSplitter = new Regex(@"([A-M|O-Z])((\-|\+)?(([0-9]+(\.[0-9]*)?)|(\.[0-9]+)))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		
		// pattern matchers.
		private static Regex parenPattern  = new Regex(@"\((.*)\)", RegexOptions.Compiled);
		private static Regex semiPattern = new Regex(@";(.*)", RegexOptions.Compiled);
		
		public readonly IFormatProvider InvariantCulture = CultureInfo.InvariantCulture;
		
		// smooth factor that decides how many steps the arc curve will have
		// i.e. divide the length of the arc with this constant
		public const float CURVE_SECTION = 1.0f;

		// G90 (absolute positioning)
		public bool AbsoluteMode = true;

		// G90.1 (absolute distance mode for arc centers)
		public bool AbsoluteArcCenterMode = false;
		
		// Metric or Inches?
		public bool Metric = true;
		
		public string Comment { get; set; }
		public string Command { get; set; }
		public string NonMovementCommands { get; set; } // the remaining non movement commands
		
		// letter codes we understand
		// ABDEFGHIJKLMPQRSTXYZ
		
		// letter values we care about
		public float? X { get; set; }	// x coordinate
		public float? Y { get; set; }	// y coordinate
		public float? Z { get; set; }	// z coordinate
		public float? I { get; set; }	// arc x coordinate
		public float? J { get; set; }	// arc y coordinate
		public float? K { get; set; }	// arc z coordinate
		public float? F { get; set; } 	// feedrate
		
		internal float minX, minY, minZ;
		internal float maxX, maxY, maxZ;
		
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
		public static List<GCodeInstruction> GetInstructions(CommandType commandType, Point3D p1, Point3D p2, float feed, Point3D shift, int side, Point3D prevPoint, float zClearance) {
			
			var output = new List<GCodeInstruction>();
			
			bool skipRapidMove = false;
			// if the command is a line and the previous X Y and the p1 X Y is the same
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
				output.Add(new GCodeInstruction(CommandType.RapidMove, null, null, zClearance, null));
				
				// secondly move X and Y to right place  (= G0 Xa Yb)
				output.Add(new GCodeInstruction(CommandType.RapidMove, p1.X, p1.Y, null, null));
			}
			output.Add(new GCodeInstruction(commandType, p2, feed));
			
			return output;
		}
		
		// Arcs
		public static List<GCodeInstruction> GetInstructions(CommandType commandType, Point3D p1, Point3D p2, Point3D p3, float feed, Point3D shift, int side, Point3D prevPoint, float zClearance) {
			
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
				output.Add(new GCodeInstruction(CommandType.RapidMove, null, null, zClearance, null));
				
				// secondly move X and Y to right place  (= G0 Xa Yb)
				output.Add(new GCodeInstruction(CommandType.RapidMove, p1.X, p1.Y, null, null));
			}
			output.Add(new GCodeInstruction(commandType, p1, p2, p3, feed));
			
			return output;
		}
		
		#region Constructors
		public GCodeInstruction(CommandType commandType, float? x, float? y, float? z, float? feed) {
			SetCommand(commandType);
			
			if (x.HasValue) this.X = x.Value;
			if (y.HasValue) this.Y = y.Value;
			if (z.HasValue) this.Z = z.Value;
			if (commandType != CommandType.RapidMove && feed.HasValue) this.F = feed.Value;
		}
		
		public GCodeInstruction(CommandType commandType, Point3D point, float feed) {
			SetCommand(commandType);
			
			this.X = point.X;
			this.Y = point.Y;
			this.Z = point.Z;
			if (commandType != CommandType.RapidMove) this.F = feed;
		}
		
		public GCodeInstruction(CommandType commandType, Point3D p1, Point3D p2, Point3D p3, float feed, bool absoluteArcCenterMode = false) {
			SetCommand(commandType);
			
			this.X = p2.X;
			this.Y = p2.Y;
			if (commandType != CommandType.RapidMove) this.F = feed;

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
			Update(line);
		}
		#endregion
		
		#region Properties
		public bool IsArc {
			get {
				if (CommandType == CommandType.CWArc ||
				    CommandType == CommandType.CCWArc) {
					return true;
				}
				return false;
			}
		}
		
		public bool CanRender
		{
			get {
				if (CommandType == CommandType.NormalMove ||
				    CommandType == CommandType.RapidMove ||
				    CommandType == CommandType.CWArc ||
				    CommandType == CommandType.CCWArc) {
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
		
		#region Helper Methods
		public void SetCommand(CommandType commandType) {
			switch (commandType) {
				case CommandType.RapidMove:
					Command = "G0";
					break;
				case CommandType.NormalMove:
					Command = "G1";
					break;
				case CommandType.CWArc:
					Command = "G2";
					break;
				case CommandType.CCWArc:
					Command = "G3";
					break;
				case CommandType.Other:
					break;
			}
		}

		public string CommandDescription
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

		public CommandType CommandType
		{
			get {
				switch (Command) {
					case "G0":
					case "G00":
						return CommandType.RapidMove;
					case "G1":
					case "G01":
						return CommandType.NormalMove;
					case "G2":
					case "G02":
						return CommandType.CWArc;
					case "G3":
					case "G03":
						return CommandType.CCWArc;
					case "G4":
					case "G04":
						return CommandType.Dwell;
				}
				return CommandType.Other;
			}
		}

		public void Update(string line) {
			char firstChar = ' ';
			
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

					// get coordinate value
					float value = float.Parse(matches[0].Groups[2].Value, InvariantCulture);

					// get coordinate letter
					string letterCode = matches[0].Groups[1].Value;
					switch (letterCode) {
							case "X": X = value; break; // x
							case "Y": Y = value; break; // y
							case "Z": Z = value; break; // z
					}

				} else if (matches.Count > 1) {
					// matched the command and something more
					this.Command = matches[0].Groups[0].Value;
					
					// loop through the elements that have coordinate values and store them
					for (int index = 0; index < matches.Count; index++) {
						// get coordinate value
						float value = float.Parse(matches[index].Groups[2].Value, InvariantCulture);

						// get coordinate
						string letterCode = matches[index].Groups[1].Value;
						switch (letterCode) {
								case "X": X = value; break; // x
								case "Y": Y = value; break; // y
								case "Z": Z = value; break; // z
								case "I": I = value; break; // arc x
								case "J": J = value; break; // arc y
								case "K": K = value; break; // arc z
								case "F": F = value; break; // feedrate
							case "A":
							case "B":
							case "D":
							case "E":
							case "G":
							case "H":
							case "L":
							case "M": // machine command
							case "P":
							case "Q":
							case "R":
							case "S":
							case "T":
								// M3 (Start Spindle)
								// M7 (Flood Coolant On)
								// M6 T1 (Change Tool: Diameter: 1.0000 mm)
								// etc.
								if (NonMovementCommands==null) {
									// add new non movement command if this is not already the main command
									// match e.g. G1
									var tmpCommand1 = string.Format(CultureInfo.InvariantCulture, "{0}{1:0.##}", letterCode, value);
									// match e.g. G01
									var tmpCommand2 = string.Format(CultureInfo.InvariantCulture, "{0}{1:00.##}", letterCode, value);
									if (!tmpCommand1.Equals(this.Command) && !tmpCommand2.Equals(this.Command)) {
										this.NonMovementCommands = tmpCommand1;
									}
								} else {
									// concatinate non movement commands
									this.NonMovementCommands = string.Format(CultureInfo.InvariantCulture, "{0} {1}{2:0.##}", this.NonMovementCommands, letterCode, value);
								}
								break;
						}
					}
				} else {
					// empty - ignore
				}
				
				// check if the first letter is in fact a command
				// if not, replace the command with nothing
				if (this.Command != null) {
					firstChar = this.Command[0];
					if (firstChar == 'X' || firstChar == 'Y' || firstChar == 'Z') {
						// replace command with nothing
						this.Command = "(no cmd)";
					}
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
		
		public override string ToString()
		{
			return this.ToString(false);
		}

		public string ToString(bool doMultiLayer, float? zOverride = null)
		{
			if (string.IsNullOrWhiteSpace(Command)) {
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
			if (K.HasValue) sb.AppendFormat(CultureInfo.InvariantCulture, " K{0:0.####}", this.K);
			if (F.HasValue) sb.AppendFormat(CultureInfo.InvariantCulture, " F{0:0.####}", this.F);
			
			if (!string.IsNullOrWhiteSpace(NonMovementCommands)) {
				sb.AppendFormat(" {0}", NonMovementCommands);
			}
			
			if (!string.IsNullOrWhiteSpace(Comment)) {
				sb.AppendFormat(" ({0})", Comment);
			}

			return sb.ToString();
		}
		
		internal List<LinePoints> RenderCode(ref Point3D currentPosition)
		{
			if (CommandType == CommandType.Other) {
				if (Command == "G90") { AbsoluteMode = true; }
				if (Command == "G90.1") { AbsoluteArcCenterMode = true; }
				if (Command == "G91") { AbsoluteMode = false; }
				if (Command == "G91.1") { AbsoluteArcCenterMode = false; }
				if (Command == "G21") { Metric = true; }
				if (Command == "G20") { Metric = false; }
				return null;
			}
			if (CommandType == CommandType.Dwell) {
				// ignore
				return null;
			}

			var pos = new Point3D(currentPosition.X, currentPosition.Y, currentPosition.Z);
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
			
			if (CommandType == CommandType.RapidMove || CommandType == CommandType.NormalMove) {
				
				maxX = Math.Max(currentPosition.X, pos.X);
				maxY = Math.Max(currentPosition.Y, pos.Y);
				maxZ = Math.Max(currentPosition.Z, pos.Z);

				minX = Math.Min(currentPosition.X, pos.X);
				minY = Math.Min(currentPosition.Y, pos.Y);
				minZ = Math.Min(currentPosition.Z, pos.Z);

				var line = new LinePoints(currentPosition, pos, CommandType == CommandType.RapidMove ? PenColorList.RapidMove : PenColorList.NormalMove);
				currentPosition.X = pos.X;
				currentPosition.Y = pos.Y;
				currentPosition.Z = pos.Z;
				return new List<LinePoints>() { line };
			}

			if (CommandType == CommandType.CWArc || CommandType == CommandType.CCWArc) {
				var center = new Point3D(0f, 0f, 0f);
				var current = new Point3D(currentPosition.X, currentPosition.Y, currentPosition.Z);
				
				if (AbsoluteArcCenterMode) {
					center.X = this.I ?? 0;
					center.Y = this.J ?? 0;
				} else {
					center.X = current.X + this.I ?? 0;
					center.Y = current.Y + this.J ?? 0;
				}

				minX = currentPosition.X;
				minY = currentPosition.Y;
				minZ = currentPosition.Z;

				var arcPoints = RenderArc(center, pos, (CommandType == CommandType.CWArc), ref currentPosition);
				
				return arcPoints;
			}

			return null;
		}

		List<LinePoints> RenderArc(Point3D center, Point3D endpoint, bool clockwise, ref Point3D currentPosition)
		{
			// figure out our deltas
			var current = new Point3D(currentPosition.X, currentPosition.Y, currentPosition.Z);
			double aX = current.X - center.X;
			double aY = current.Y - center.Y;
			double bX = endpoint.X - center.X;
			double bY = endpoint.Y - center.Y;

			// angle variables.
			double angleA;
			double angleB;
			// Clockwise
			if (clockwise) {
				angleA = Math.Atan2(bY, bX);
				angleB = Math.Atan2(aY, aX);
			}
			// Counterclockwise
			else {
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
			
			double angle = angleB - angleA;
			
			// calculate a couple useful things.
			double radius = Math.Sqrt(aX * aX + aY * aY);
			double length = radius * angle;

			// Maximum of either 2.4 times the angle in radians
			// or the length of the curve divided by the curve section constant
			int steps = (int)Math.Ceiling(Math.Max(angle * 2.4, length / CURVE_SECTION));

			// this is the real draw action.
			var newPoint = new Point3D(current.X, current.Y, current.Z);
			var lastPoint = new Point3D(current.X, current.Y, current.Z);
			var output = new List<LinePoints>();
			var p = clockwise ? PenColorList.CWArc : PenColorList.CCWArc;
			
			int step;
			for (int s = 1; s <= steps; s++) {
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

		public bool EqualsXYZ(GCodeInstruction other)
		{
			if (other == null) return false;
			
			if (X.HasValue && other.X.HasValue && X.Value == other.X.Value
			    && Y.HasValue && other.Y.HasValue && Y.Value == other.Y.Value
			    && Z.HasValue && other.Z.HasValue && Z.Value == other.Z.Value) {
				return true;
			}
			
			return false;
		}
	}
}
