using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

using Util;

namespace GCode
{
	/// <summary>
	/// Description of GCodeSplitter.
	/// Most of it ported and copied from the G-Code_Ripper-0.12 Python App
	/// </summary>
	public static class GCodeSplitter
	{
		const float SELF_ZERO = 0.0000001f;
		const float SELF_ACCURACY = 0.025f; // mm, inch= 0.01;
		
		enum Position {
			None = -1,
			L = 0,
			R = 1
		}
		
		/// <summary>
		/// Split a gcode file into tiles
		/// </summary>
		/// <param name="instructions">list of gcode instructions</param>
		/// <param name="splitPoint">Point3D describing the split coordinates</param>
		/// <param name="angle">Whether the split should happen in an angle</param>
		/// <param name="zClearance">z-height (clearance) to use in added rapid moves</param>
		/// <returns>List of tiles</returns>
		/// <remarks>
		/// Copied from the G-Code_Ripper-0.12 Python App
		/// Method: def split_code(self,code2split,shift=[0,0,0],angle=0.0)
		/// </remarks>
		public static List<List<GCodeInstruction>> Split(List<GCodeInstruction> instructions, Point3D splitPoint, float angle, float zClearance)
		{
			// G0 (Rapid), G1 (linear), G2 (clockwise arc) or G3 (counterclockwise arc).
			CommandType command = CommandType.Other;

			const float xsplit = 0.0f; // xsplit is always zero, because the whole coordinate system is shifted to origin
			var app = new List<List<GCodeInstruction>>();
			app.Add(new List<GCodeInstruction>());
			app.Add(new List<GCodeInstruction>());
			
			var flag_side = Position.None;
			int thisSide = -1;
			int otherSide = -1;
			
			float currentFeedrate = 0.0f;
			
			var currentPos = Point3D.Empty;		// current position as read
			var previousPos = Point3D.Empty;	// current position as read
			var centerPos = Point3D.Empty;		// center position as read
			
			var currentPosAtOrigin = Point3D.Empty;		// current position, shifted
			var previousPosAtOrigin = Point3D.Empty;	// last position, shifted
			var centerPosAtOrigin = Point3D.Empty;		// center position, shifted

			var A = Point3D.Empty;
			var B = Point3D.Empty;
			var C = Point3D.Empty;
			var D = Point3D.Empty;
			var E = Point3D.Empty;
			
			var cross = new List<Point3D>();	// number of intersections found
			
			#if DEBUG
			Debug.WriteLine("Split point: {0}, angle: {1} z-clearance: {2}", splitPoint, angle, zClearance);
			#endif

			int numInstructions = 1;
			foreach (var instruction in instructions)
			{
				// store move type
				command = instruction.CommandType;
				
				// merge previous coordinates with newer ones to maintain correct point coordinates
				if ((instruction.X.HasValue || instruction.Y.HasValue || instruction.Z.HasValue
				     || instruction.F.HasValue)) {
					if (instruction.X.HasValue && instruction.X.Value != currentPos.X) {
						currentPos.X = instruction.X.Value;
					}
					if (instruction.Y.HasValue && instruction.Y.Value != currentPos.Y) {
						currentPos.Y = instruction.Y.Value;
					}
					if (instruction.Z.HasValue && instruction.Z.Value != currentPos.Z) {
						currentPos.Z = instruction.Z.Value;
					}
					if (instruction.F.HasValue && instruction.F.Value != currentFeedrate) {
						currentFeedrate = instruction.F.Value;
					}
				}
				
				if (command == CommandType.NormalMove
				    || command == CommandType.CWArc
				    || command == CommandType.CCWArc) {
					
					// shift and rotate so that we can work with the coordinate system at origin (0, 0)
					currentPosAtOrigin = SetOffsetAndRotation(currentPos, splitPoint, angle);
					previousPosAtOrigin = SetOffsetAndRotation(previousPos, splitPoint, angle);
					
					// store center point
					if (instruction.I.HasValue && instruction.J.HasValue) {
						centerPos = new Point3D(previousPos.X+instruction.I.Value,
						                        previousPos.Y+instruction.J.Value,
						                        currentPos.Z);
						
						centerPosAtOrigin = SetOffsetAndRotation(centerPos, splitPoint, angle);
					}
					
					// determine what side the move belongs to
					if (previousPosAtOrigin.X > xsplit+SELF_ZERO) {
						flag_side = Position.R;
					} else if (previousPosAtOrigin.X < xsplit-SELF_ZERO) {
						flag_side = Position.L;
					} else {
						if (command == CommandType.NormalMove) {
							if (currentPosAtOrigin.X >= xsplit) {
								flag_side = Position.R;
							} else {
								flag_side = Position.L;
							}
						} else if (command == CommandType.CWArc) {
							if (Math.Abs(previousPosAtOrigin.Y - centerPosAtOrigin.Y) < SELF_ZERO) {
								if (centerPosAtOrigin.X > xsplit) {
									flag_side = Position.R;
								} else {
									flag_side = Position.L;
								}
							} else {
								if (previousPosAtOrigin.Y >= centerPosAtOrigin.Y) {
									flag_side = Position.R;
								} else {
									flag_side = Position.L;
								}
							}
						} else { //(mvtype == 3) {
							if (Math.Abs(previousPosAtOrigin.Y - centerPosAtOrigin.Y) < SELF_ZERO) {
								if (centerPosAtOrigin.X > xsplit) {
									flag_side = Position.R;
								} else {
									flag_side = Position.L;
								}
							} else {
								if (previousPosAtOrigin.Y >= centerPosAtOrigin.Y) {
									flag_side = Position.L;
								} else {
									flag_side = Position.R;
								}
							}
						}
					}
					
					if (flag_side == Position.R) {
						thisSide  = 1;
						otherSide = 0;
					} else {
						thisSide  = 0;
						otherSide = 1;
					}
					
					// Handle normal moves
					if (command == CommandType.NormalMove) {
						A = UnsetOffsetAndRotation(previousPosAtOrigin, splitPoint, angle);
						C = UnsetOffsetAndRotation(currentPosAtOrigin, splitPoint, angle);
						cross = GetLineIntersect(previousPosAtOrigin, currentPosAtOrigin, xsplit);

						if (cross.Count > 0) {
							// Line crosses boundary
							B = UnsetOffsetAndRotation(cross[0], splitPoint, angle);
							app[thisSide].AddRange(GCodeInstruction.GetInstructions(command, A, B, currentFeedrate, splitPoint, thisSide, GetPreviousPoint(app[thisSide]), zClearance));
							app[otherSide].AddRange(GCodeInstruction.GetInstructions(command, B, C, currentFeedrate, splitPoint, otherSide, GetPreviousPoint(app[otherSide]), zClearance));
						} else {
							// Lines doesn't intersect
							
							// check if this point is the same as centerpoint
							// if so add it to both sides
							// TODO: check if this works in all cases?
							if (currentPos.X == splitPoint.X) {
								app[thisSide].AddRange(GCodeInstruction.GetInstructions(command, A, C, currentFeedrate, splitPoint, thisSide, GetPreviousPoint(app[thisSide]), zClearance));
								app[otherSide].AddRange(GCodeInstruction.GetInstructions(command, A, C, currentFeedrate, splitPoint, otherSide, GetPreviousPoint(app[otherSide]), zClearance));
							} else {
								app[thisSide].AddRange(GCodeInstruction.GetInstructions(command, A, C, currentFeedrate, splitPoint, thisSide, GetPreviousPoint(app[thisSide]), zClearance));
							}
						}
					}
					
					// Handle Arc moves
					if (command == CommandType.CWArc || command == CommandType.CCWArc ) {
						A = UnsetOffsetAndRotation(previousPosAtOrigin, splitPoint, angle);
						C = UnsetOffsetAndRotation(currentPosAtOrigin, splitPoint, angle);
						D  = UnsetOffsetAndRotation(centerPosAtOrigin, splitPoint, angle);
						cross = GetArcIntersects(previousPosAtOrigin, currentPosAtOrigin, centerPosAtOrigin, xsplit, command);

						if (cross.Count > 0) {
							// Arc crosses boundary at least once
							B = UnsetOffsetAndRotation(cross[0], splitPoint, angle);
							
							// Check length of arc before writing
							if (Transformation.Distance(B, A) > SELF_ACCURACY) {
								app[thisSide].AddRange(GCodeInstruction.GetInstructions(command, A, B, D, currentFeedrate, splitPoint, thisSide, GetPreviousPoint(app[thisSide]), zClearance));
							}
							
							if (cross.Count == 1) { // Arc crosses boundary only once
								// Check length of arc before writing
								if (Transformation.Distance(C, B) > SELF_ACCURACY) {
									app[otherSide].AddRange(GCodeInstruction.GetInstructions(command, B, C, D, currentFeedrate, splitPoint, otherSide, GetPreviousPoint(app[otherSide]), zClearance));
								}
							}
							
							if (cross.Count == 2) { // Arc crosses boundary twice
								E = UnsetOffsetAndRotation(cross[1], splitPoint, angle);
								
								// Check length of arc before writing
								if (Transformation.Distance(E, B) > SELF_ACCURACY) {
									app[otherSide].AddRange(GCodeInstruction.GetInstructions(command, B, E, D, currentFeedrate, splitPoint, otherSide, GetPreviousPoint(app[otherSide]), zClearance));
								}
								
								// Check length of arc before writing
								if (Transformation.Distance(C, E) > SELF_ACCURACY) {
									app[thisSide].AddRange(GCodeInstruction.GetInstructions(command, E, C, D, currentFeedrate, splitPoint, thisSide, GetPreviousPoint(app[thisSide]), zClearance));
								}
							}
						} else {
							// Arc does not cross boundary
							app[thisSide].AddRange(GCodeInstruction.GetInstructions(command, A, C, D, currentFeedrate, splitPoint, thisSide, GetPreviousPoint(app[thisSide]), zClearance));
						}
					}
					
				} else {
					
					// if not any normal or arc moves, store the instruction in both lists
					// rapid moves are also handled here
					if (instruction.CommandType != CommandType.RapidMove) {
						app[0].Add(instruction);
						app[1].Add(instruction);
					}
				}
				
				#if DEBUG
				if (command == CommandType.RapidMove) Debug.WriteLine("{0} [{1}={2}], [{3}={4}]", command, previousPos, previousPosAtOrigin, currentPos, currentPosAtOrigin);
				if (command == CommandType.NormalMove) Debug.WriteLine("{0} [{1}={2}], [{3}={4}] {5}", command, previousPos, previousPosAtOrigin, currentPos, currentPosAtOrigin, currentFeedrate);
				if (command  == CommandType.CWArc || command == CommandType.CCWArc)
					Debug.WriteLine("{0} [{1}={2}], [{3}={4}] [{5}={6}] {7}", command, previousPos, previousPosAtOrigin, currentPos, currentPosAtOrigin, centerPos, centerPosAtOrigin, currentFeedrate);
				#endif
				
				// store current position
				previousPos = currentPos;
				
				// count number of instructions processed
				numInstructions++;
			}
			
			// add a last raise Z
			// raise Z to Z clearance (= G0 Za)
			var lastRaiseBitInstruction = new GCodeInstruction(CommandType.RapidMove, null, null, zClearance, null);
			app[0].Add(lastRaiseBitInstruction);
			app[1].Add(lastRaiseBitInstruction);
			
			return app;
		}
		
		public static List<List<GCodeInstruction>> Split(List<GCodeInstruction> instructions, Point3D splitPoint, float angle, float zClearance, float minX, float maxX, float minY, float maxY) {
			
			return Split(instructions, splitPoint, angle, zClearance);
			
			// G0 (Rapid), G1 (linear), G2 (clockwise arc) or G3 (counterclockwise arc).
			CommandType mvtype = CommandType.Other;
			
			var app = new List<List<GCodeInstruction>>();
			app.Add(new List<GCodeInstruction>());
			app.Add(new List<GCodeInstruction>());
			
			var currentPos = Point3D.Empty;		// current position as read
			var previousPos = Point3D.Empty;	// current position as read
			var centerPos = Point3D.Empty;		// center position converted
			float currentFeedrate = 0.0f;
			
			// original rectangle
			var origRect = new RectangleF(minX, minY, Math.Abs(maxX-minX), Math.Abs(maxY-minY));
			
			int numTiles = 2; //9;

			// split into equal sized rectangles (tiles)
			int numColumns = (int) (Math.Ceiling(Math.Sqrt(numTiles)));
			int numRows = (int) Math.Ceiling(numTiles / (double)numColumns);
			
			float width =  origRect.Width / numColumns;
			float height = origRect.Height / numRows;
			
			var allTileRectangles = new Dictionary<RectangleF, List<GCodeInstruction>>();
			for (int y = 0; y < numRows; ++y) {
				for (int x = 0; x < numColumns; ++x) {
					allTileRectangles.Add(new RectangleF(x * width, y * height, width, height), new List<GCodeInstruction>());
				}
			}
			
			// for each tile, add corresponding gcode instructions
			int numInstructions = 1;
			foreach (var instruction in instructions)
			{
				// store move type
				mvtype = instruction.CommandType;
				
				// merge previous coordinates with newer ones to maintain correct point coordinates
				if ((instruction.X.HasValue || instruction.Y.HasValue || instruction.Z.HasValue
				     || instruction.F.HasValue)) {
					if (instruction.X.HasValue && instruction.X.Value != currentPos.X) {
						currentPos.X = instruction.X.Value;
					}
					if (instruction.Y.HasValue && instruction.Y.Value != currentPos.Y) {
						currentPos.Y = instruction.Y.Value;
					}
					if (instruction.Z.HasValue && instruction.Z.Value != currentPos.Z) {
						currentPos.Z = instruction.Z.Value;
					}
					if (instruction.F.HasValue && instruction.F.Value != currentFeedrate) {
						currentFeedrate = instruction.F.Value;
					}
				}
				
				// figure out what tile(s) this instruction belongs to
				var tileRectangles = GetImpactedTileRectangles(allTileRectangles, instruction, previousPos, currentPos);

				if (mvtype == CommandType.NormalMove
				    || mvtype == CommandType.CWArc
				    || mvtype == CommandType.CCWArc) {
					
					// store center point
					if (instruction.I.HasValue && instruction.J.HasValue) {
						centerPos = new Point3D(previousPos.X+instruction.I.Value,
						                        previousPos.Y+instruction.J.Value,
						                        currentPos.Z);
					}

					// Handle normal moves
					if (mvtype == CommandType.NormalMove) {
						
					}
					
					// Handle Arc moves
					if (mvtype == CommandType.CWArc || mvtype == CommandType.CCWArc ) {
					}
					
				} else {
					// if not any normal or arc moves, store the instruction in all tiles
					// rapid moves are also handled here
				}
				
				// store current position
				previousPos = currentPos;
				
				// count number of instructions processed
				numInstructions++;
			}
			
			return app;
		}
		
		static List<RectangleF> GetImpactedTileRectangles(Dictionary<RectangleF, List<GCodeInstruction>> tileRects, GCodeInstruction instruction, Point3D previousPos, Point3D currentPos) {
			var rectangles = new List<RectangleF>();
			
			foreach (var rect in tileRects.Keys) {
				if (Transformation.RectangleContains(rect, currentPos)) {
					rectangles.Add(rect);
				}
			}
			return rectangles;
		}
		
		/// <summary>
		/// Traverse the instructions backwards until a movement is found,
		/// return as Point3D
		/// </summary>
		/// <param name="instructions">a list of gcode instructions</param>
		/// <returns>previous point as a Point3D or Empty</returns>
		private static Point3D GetPreviousPoint(IList<GCodeInstruction> instructions) {
			var prevPoint = Point3D.Empty;
			
			if (instructions.Count == 0) return prevPoint;
			
			GCodeInstruction prevInstruction = null;
			bool foundLastMovement = false;
			int index = 1;
			while (!foundLastMovement) {
				prevInstruction = instructions[instructions.Count - index];
				
				if (prevInstruction.CommandType == CommandType.CWArc
				    || prevInstruction.CommandType == CommandType.CCWArc
				    || prevInstruction.CommandType == CommandType.NormalMove
				    || prevInstruction.CommandType == CommandType.RapidMove) {
					foundLastMovement = true;
				}
				
				if (index == instructions.Count) {
					// warning, no previous movements found
					break;
				}
				
				index++;
			}
			
			// merge previous coordinates with newer ones to maintain correct point coordinates
			if (prevInstruction.X.HasValue || prevInstruction.Y.HasValue || prevInstruction.Z.HasValue) {
				if (prevInstruction.X.HasValue) {
					prevPoint.X = prevInstruction.X.Value;
				}
				if (prevInstruction.Y.HasValue) {
					prevPoint.Y = prevInstruction.Y.Value;
				}
				if (prevInstruction.Z.HasValue) {
					prevPoint.Z = prevInstruction.Z.Value;
				}
			}

			return prevPoint;
		}
		
		/// <summary>
		/// Find line intersect at origin
		/// </summary>
		/// <remarks>Ported from Python: def get_line_intersect(self,p1, p2, xsplit)</remarks>
		/// <seealso cref="http://www.scorchworks.com/Gcoderipper/gcoderipper.html#download"/>
		/// <param name="p1">start point</param>
		/// <param name="p2">end point</param>
		/// <param name="xsplit">x split (always zero)</param>
		/// <returns>a list with at most one intersect point</returns>
		public static List<Point3D> GetLineIntersect(Point3D p1, Point3D p2, float xsplit) {
			
			var output = new List<Point3D>();
			
			// the coordinate system is shifted so that X is 0
			float xcross = xsplit;
			float ycross = 0.0f;
			float zcross = 0.0f;

			// if a line is on the form: y = a*x + b
			// then:
			// a1 = (y2-y1)/(x2-x1)
			// b1 = y1 - a1*x1
			
			float dx = p2.X - p1.X;
			float dy = p2.Y - p1.Y;
			float dz = p2.Z - p1.Z;
			
			// x and y plane
			try {
				float my = dy / dx;
				float by = p1.Y - my * p1.X;
				ycross = my * xsplit + by;
			} catch (Exception) {
				ycross = p1.Y;
			}

			// x and z plane
			try {
				float mz = dz / dx;
				float bz = p1.Z - mz * p1.X;
				zcross = mz * xsplit + bz;
			} catch (Exception) {
				zcross = p1.Z;
			}

			if (xcross > Math.Min(p1.X,p2.X) + SELF_ZERO
			    && xcross < Math.Max(p1.X,p2.X) - SELF_ZERO) {
				
				var point = new Point3D(xcross, ycross, zcross);
				output.Add(point);
			}
			
			return output;
		}
		
		/// <summary>
		/// Find arc intersects at origin
		/// </summary>
		/// <param name="p1">start point</param>
		/// <param name="p2">end point</param>
		/// <param name="cent">center point</param>
		/// <param name="xsplit">x split (always zero)</param>
		/// <param name="code">direction of arc (clock wise or counter clock wise)</param>
		/// <returns>a list of intersect points</returns>
		public static List<Point3D> GetArcIntersects(Point3D p1, Point3D p2, Point3D cent, float xsplit, CommandType code) {
			
			var output = new List<Point3D>();
			
			// the coordinate system is shifted so that X is 0
			float xcross1 = xsplit;
			float xcross2 = xsplit;
			
			// variables to find
			float ycross1 = 0.0f;
			float ycross2 = 0.0f;
			float zcross1 = 0.0f;
			float zcross2 = 0.0f;
			double gamma1 = 0.0f;
			double gamma2 = 0.0f;
			
			// find radius of circle
			double R = Transformation.Distance(p1, cent);
			double Rt = Transformation.Distance(p2, cent);
			
			// check that the radiuses are equal
			if (Math.Abs(R-Rt) > SELF_ACCURACY) {
				Debug.WriteLine("Radius Warning: R1={0} R2={1}", R, Rt);
			}

			// Calculate where the split line crosses the arc in the y-dimension
			// c is the centerpoint, r is radius and v is the length we want to find.
			// cx, v and r forms a right angled triangle.
			// The square of the hypotenuse (r) is equal to
			// the sum of the squares of the other two sides.
			// cX^2 + v^2 = r^2
			// v^2 = r^2 - cX^2
			double vSquared =  Math.Pow(R,2) - Math.Pow(cent.X,2);
			
			if (vSquared >= 0.0) {
				double v = Math.Sqrt(vSquared);
				ycross1 = (float) (cent.Y - v);
				ycross2 = (float) (cent.Y + v);
			} else {
				// if v^2 is less than zero, no intersections at all
				return output;
			}

			// Other sources:
			// https://stackoverflow.com/questions/22472427/get-the-centerpoint-of-an-arc-g-code-conversion
			// https://stackoverflow.com/questions/30006155/calculate-intersect-point-between-arc-and-line
			// https://gamedev.stackexchange.com/questions/31218/how-to-move-an-object-along-a-circumference-of-another-object
			
			// Find the relative vector from our center position to the p1 position
			float deltaP1X = p1.X - cent.X;
			float deltaP1Y = p1.Y - cent.Y;
			
			// From our relative vector we can find the precise angle relative to the X-axis with:
			// curTheta = atan2(deltaX, deltaY);
			// GetAngle calculates angle in degrees between the p1 relative vector
			// (line between p1 and the center point) and the X-axis.
			// Note! no code, defaults to CommandType.CCWArc
			// Theta1=angle of the position of the start point relative to the X axis
			float theta1 = GetAngle(deltaP1X, deltaP1Y);

			// Find the relative vector from our center position to the p2 position
			float deltaP2X = p2.X - cent.X;
			float deltaP2Y = p2.Y - cent.Y;
				
			// Theta2=angle of the position of the end point relative to the X axis
			float theta2 = GetAngle(deltaP2X, deltaP2Y);
			
			// Rotate the p2 vector (between p2 and the center point) by -theta
			var betaP = Rotate(deltaP2X, deltaP2Y, -theta1);
			
			// And get the new angle after rotation
			// beta = angle between the beta point and the X axis
			float beta = GetAngle(betaP.X, betaP.Y, code);
			
			if (Math.Abs(beta) <= SELF_ZERO) {
				beta = 360.0f;
			}
			
			// Rotate the cross1 vector (between cross1 and the center point) by -theta
			var t1 = Rotate(xsplit-cent.X, ycross1-cent.Y, -theta1);
			
			// And get the new angle after rotation
			// gt1 = angle between the t1 point and the X axis
			float gt1 = GetAngle(t1.X, t1.Y, code);
			
			// Rotate the cross2 vector (between cross2 and the center point) by -theta
			var t2 = Rotate(xsplit-cent.X, ycross2-cent.Y, -theta1);

			// And get the new angle after rotation
			// gt1 = angle between the t1 point and the X axis
			float gt2 = GetAngle(t2.X, t2.Y, code);

			// determine gamma
			if (gt1 < gt2) {
				gamma1 = gt1;
				gamma2 = gt2;
			} else {
				gamma1 = gt2;
				gamma2 = gt1;
				var temp = ycross1;
				ycross1 = ycross2;
				ycross2 = temp;
			}
			
			// if the z values for the two points are different
			// we need to interpolate between them to find the z points for
			// the cross1 and cross2 points
			var deltaZ = p2.Z - p1.Z;
			var deltaAngle = beta;
			var mz = deltaZ / deltaAngle;
			zcross1 = (float) (p1.Z + gamma1 * mz);
			zcross2 = (float) (p1.Z + gamma2 * mz);
			
			// check if the angles of the intersection points and the X-axis is within the
			// angles of the beta point and the X-axis.
			// if not the cross points are outside the arc and shouldn't be included
			if (gamma1 < beta && gamma1 > SELF_ZERO && gamma1 < beta-SELF_ZERO)
				output.Add(new Point3D(xcross1, ycross1, zcross1));
			
			if (gamma2 < beta && gamma1 > SELF_ZERO && gamma2 < beta-SELF_ZERO)
				output.Add(new Point3D(xcross2, ycross2, zcross2));

			#if DEBUG
			Debug.WriteLine(" start: x1 ={0:0.####} y1={1:0.####} z1={2:0.####}", p1.X, p1.Y, p1.Z);
			Debug.WriteLine("   end: x2 ={0:0.####} y2={1:0.####} z2={2:0.####}", p2.X, p2.Y, p2.Z);
			Debug.WriteLine("center: xc ={0:0.####} yc={1:0.####} xsplit={2:0.####} code={3}", cent.X, cent.Y, xsplit, code);
			Debug.WriteLine("R = {0:0.####}", R);
			Debug.WriteLine("theta ={0:0.####}", theta1);
			Debug.WriteLine("beta  ={0:0.####} gamma1={1:0.####} gamma2={2:0.####}", beta, gamma1, gamma2);
			int cnt = 1;
			foreach (var line in output) {
				Debug.WriteLine("arc cross {0}: {1:0.####}, {2:0.####}, {3:0.####}", cnt, line.X, line.Y, line.Z);
				cnt++;
			}
			Debug.WriteLine("----------------------------------------------");
			#endif
			
			return output;
		}

		/// <summary>
		/// Routine takes an x and a y coords and does a cordinate transformation
		/// to a new coordinate system at angle from the initial coordinate system
		/// Returns new x,y point
		/// Note! this rotates through origin (0,0)
		/// </summary>
		/// <param name="x">x</param>
		/// <param name="y">y</param>
		/// <param name="angle">angle in degrees</param>
		/// <returns>rotated coordinates</returns>
		public static Point3D Rotate(float x, float y, float angle) {
			var point = new PointF(x, y);
			var newPoint = Transformation.Rotate(point, angle);
			return new Point3D(newPoint);
		}
		
		/// <summary>
		/// Routine takes an x and y coordinate and returns the angle in degrees (between 0 and 360)
		/// I.e. angle of the position of the point relative to the X axis
		/// </summary>
		/// <param name="x">x</param>
		/// <param name="y">y</param>
		/// <param name="code">CommandType, type of arc</param>
		/// <returns>angle in degrees (0 - 360)</returns>
		public static float GetAngle(float x, float y, CommandType code = CommandType.CCWArc)
		{
			float angle = 90.0f - (float) Transformation.RadianToDegree(Math.Atan2(x, y));
			if (angle < 0) {
				angle = 360 + angle;
			}
			if (code == CommandType.CWArc) {
				return (360.0f - angle);
			}
			return angle;
		}
		
		private static Point3D SetOffsetAndRotation(Point3D point, Point3D center, float degrees) {
			
			// How to properly rotate point around another point
			// 1. A translation that brings point 1 to the origin (0,0)
			// 2. Rotation around the origin by the required angle
			// 3. A translation that brings point 1 back to its original position

			// bring point to origin
			float x = point.X - center.X;
			float y = point.Y - center.Y;
			float z = point.Z - center.Z;
			
			// rotate
			var newPoint = Rotate(x, y, degrees);
			return new Point3D(newPoint.X, newPoint.Y, z);
		}

		private static Point3D UnsetOffsetAndRotation(Point3D point, Point3D center, float degrees) {

			// rotate back
			var newPoint = Rotate(point.X, point.Y, -degrees);
			
			// translate point back to it's original position
			float x = newPoint.X + center.X;
			float y = newPoint.Y + center.Y;
			float z = point.Z + center.Z;
			return new Point3D(x, y, z);
			
		}
	}
}
