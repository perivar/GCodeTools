using System;
using System.Collections.Generic;
using System.Drawing;

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
		/// <param name="shift">Point3D describing the split coordinates</param>
		/// <param name="angle">Whether the split should happen in an angle</param>
		/// <param name="zClearance">z-height (clearance) to use in added rapid moves</param>
		/// <returns>List of tiles</returns>
		/// <remarks>
		/// Copied from the G-Code_Ripper-0.12 Python App
		/// Method: def split_code(self,code2split,shift=[0,0,0],angle=0.0)
		/// </remarks>
		public static List<List<GCodeInstruction>> Split(List<GCodeInstruction> instructions, Point3D shift, float angle, float zClearance)
		{
			// G0 (Rapid), G1 (linear), G2 (clockwise arc) or G3 (counterclockwise arc).
			CommandType mvtype = CommandType.Other;

			var app = new List<List<GCodeInstruction>>();
			app.Add(new List<GCodeInstruction>());
			app.Add(new List<GCodeInstruction>());
			
			var flag_side = Position.None;
			int thisSide = -1;
			int otherSide = -1;
			
			float currentFeedrate = 0.0f;
			
			var currentPos = Point3D.Empty;		// current position as read
			var previousPos = Point3D.Empty;	// current position as read
			var centerPos = Point3D.Empty;		// center position converted
			
			var A = Point3D.Empty;
			var B = Point3D.Empty;
			var C = Point3D.Empty;
			var D = Point3D.Empty;
			var E = Point3D.Empty;
			
			var pos = Point3D.Empty;			// current position, shifted
			var pos_last = Point3D.Empty;		// last position, shifted
			var center = Point3D.Empty;			// center position converteed, shifted
			
			var cross = new List<Point3D>();	// number of intersections found
			
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
				
				if (mvtype == CommandType.NormalMove
				    || mvtype == CommandType.CWArc
				    || mvtype == CommandType.CCWArc) {
					
					pos = SetOffsetAndRotation(currentPos, shift, angle);
					pos_last = SetOffsetAndRotation(previousPos, shift, angle);
					
					// store center point
					if (instruction.I.HasValue && instruction.J.HasValue) {
						centerPos = new Point3D(previousPos.X+instruction.I.Value,
						                        previousPos.Y+instruction.J.Value,
						                        currentPos.Z);
						
						center = SetOffsetAndRotation(centerPos, shift, angle);
					}
					
					// determine what side the move belongs to
					if (pos_last.X > SELF_ZERO) {
						flag_side = Position.R;
					} else if (pos_last.X < SELF_ZERO) {
						flag_side = Position.L;
					} else {
						if (mvtype == CommandType.NormalMove) {
							if (pos.X >= 0) {
								flag_side = Position.R;
							} else {
								flag_side = Position.L;
							}
						} else if (mvtype == CommandType.CWArc) {
							if (Math.Abs(pos_last.Y-center.Y) < SELF_ZERO) {
								if (center.X > 0) {
									flag_side = Position.R;
								} else {
									flag_side = Position.L;
								}
							} else {
								if (pos_last.Y >= center.Y) {
									flag_side = Position.R;
								} else {
									flag_side = Position.L;
								}
							}
						} else { //(mvtype == 3) {
							if (Math.Abs(pos_last.Y-center.Y) < SELF_ZERO) {
								if (center.X > 0) {
									flag_side = Position.R;
								} else {
									flag_side = Position.L;
								}
							} else {
								if (pos_last.Y >= center.Y) {
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
					if (mvtype == CommandType.NormalMove) {
						A = UnsetOffsetAndRotation(pos_last, shift, angle);
						C = UnsetOffsetAndRotation(pos, shift, angle);
						cross = GetLineIntersect(pos_last, pos);
						//cross = GetLineIntersect2(pos_last, pos);

						if (cross.Count > 0) {
							// Line crosses boundary
							B = UnsetOffsetAndRotation(cross[0], shift, angle);
							app[thisSide].AddRange(GCodeInstruction.GetInstructions(mvtype, A, B, currentFeedrate, shift, thisSide, GetPreviousPoint(app[thisSide]), zClearance));
							app[otherSide].AddRange(GCodeInstruction.GetInstructions(mvtype, B, C, currentFeedrate, shift, otherSide, GetPreviousPoint(app[otherSide]), zClearance));
						} else {
							// Lines doesn't intersect
							
							// check if this point is the same as centerpoint
							// if so add it to both sides
							// TODO: check if this works in all cases?
							if (currentPos.X == shift.X) {
								app[thisSide].AddRange(GCodeInstruction.GetInstructions(mvtype, A, C, currentFeedrate, shift, thisSide, GetPreviousPoint(app[thisSide]), zClearance));
								app[otherSide].AddRange(GCodeInstruction.GetInstructions(mvtype, A, C, currentFeedrate, shift, thisSide, GetPreviousPoint(app[otherSide]), zClearance));
							} else {
								app[thisSide].AddRange(GCodeInstruction.GetInstructions(mvtype, A, C, currentFeedrate, shift, thisSide, GetPreviousPoint(app[thisSide]), zClearance));
							}
						}
					}
					
					// Handle Arc moves
					if (mvtype == CommandType.CWArc || mvtype == CommandType.CCWArc ) {
						A = UnsetOffsetAndRotation(pos_last, shift, angle);
						C = UnsetOffsetAndRotation(pos, shift, angle);
						D  = UnsetOffsetAndRotation(center, shift, angle);
						cross = GetArcIntersects(pos_last, pos, center, mvtype);
						//cross = GetArcIntersects2(pos_last, pos, center, mvtype);

						if (cross.Count > 0) {
							// Arc crosses boundary at least once
							B = UnsetOffsetAndRotation(cross[0], shift, angle);
							
							// Check length of arc before writing
							if (Transformation.Distance(B, A) > SELF_ACCURACY) {
								app[thisSide].AddRange(GCodeInstruction.GetInstructions(mvtype, A, B, D, currentFeedrate, shift, thisSide, GetPreviousPoint(app[thisSide]), zClearance));
							}
							
							if (cross.Count == 1) { // Arc crosses boundary only once
								// Check length of arc before writing
								if (Transformation.Distance(C, B) > SELF_ACCURACY) {
									app[otherSide].AddRange(GCodeInstruction.GetInstructions(mvtype, B, C, D, currentFeedrate, shift, otherSide, GetPreviousPoint(app[otherSide]), zClearance));
								}
							}
							
							if (cross.Count == 2) { // Arc crosses boundary twice
								E = UnsetOffsetAndRotation(cross[1], shift, angle);
								
								// Check length of arc before writing
								if (Transformation.Distance(E, B) > SELF_ACCURACY) {
									app[otherSide].AddRange(GCodeInstruction.GetInstructions(mvtype, B, E, D, currentFeedrate, shift, otherSide, GetPreviousPoint(app[otherSide]), zClearance));
								}
								
								// Check length of arc before writing
								if (Transformation.Distance(C, E) > SELF_ACCURACY) {
									app[thisSide].AddRange(GCodeInstruction.GetInstructions(mvtype, E, C, D, currentFeedrate, shift, thisSide, GetPreviousPoint(app[thisSide]), zClearance));
								}
							}
						} else {
							// Arc does not cross boundary
							app[thisSide].AddRange(GCodeInstruction.GetInstructions(mvtype, A, C, D, currentFeedrate, shift, thisSide, GetPreviousPoint(app[thisSide]), zClearance));
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
				
				// Debug
				/*
				if (mvtype == CommandList.RapidMove) Console.WriteLine("{0} {1} {2}", mvtype, previousPos, currentPos);
				if (mvtype == CommandList.NormalMove) Console.WriteLine("{0} {1} {2} {3}", mvtype, previousPos, currentPos, currentFeedrate);
				if (mvtype == CommandList.CWArc || mvtype == CommandList.CCWArc)
					Console.WriteLine("{0} {1} {2} {3} {4}", mvtype, previousPos, currentPos, centerPos, currentFeedrate);
				 */
				
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
		
		/// <summary>
		/// traverse the instructions backwards until a movement is found,
		/// return as Point3D
		/// </summary>
		/// <param name="instructions">a list of gcode instructions</param>
		/// <returns>a point 3D or empty</returns>
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
		
		public static List<Point3D> GetLineIntersect(Point3D p1, Point3D p2) {
			
			var output = new List<Point3D>();
			
			float dx = p2.X - p1.X;
			float dy = p2.Y - p1.Y;
			float dz = p2.Z - p1.Z;
			
			// the coordinate system is shifted so that X is 0
			float xcross = 0.0f;
			float ycross = 0.0f;
			float zcross = 0.0f;
			
			try {
				float my = dy/dx;
				float by = p1.Y - my * p1.X;
				ycross = by;
			} catch (Exception) {
				ycross = p1.Y;
			}

			try {
				float mz = dz/dx;
				float bz = p1.Z - mz * p1.X;
				zcross = bz;
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
		
		public static List<Point3D> GetLineIntersect2(Point3D p1, Point3D p2) {
			
			var output = new List<Point3D>();

			// the coordinate system is shifted so that X is 0
			var ps1 = new PointF(0,10);
			var pe1 = new PointF(0,20);
			
			var ps2 = new PointF(p1.X, p1.Y);
			var pe2 = new PointF(p2.X, p2.Y);
			
			var intersection = Transformation.FindLineIntersectionPoint(ps1, pe1, ps2, pe2);
			
			if (!intersection.IsEmpty) {
				output.Add(new Point3D(intersection.X, intersection.Y));
			}
			
			return output;
		}
		
		public static List<Point3D> GetArcIntersects(Point3D p1, Point3D p2, Point3D cent, CommandType code) {
			
			var output = new List<Point3D>();
			
			// the coordinate system is shifted so that X is 0
			float xcross1 = 0.0f;
			float xcross2 = 0.0f;
			float ycross1 = 0.0f;
			float ycross2 = 0.0f;
			float zcross1 = 0.0f;
			float zcross2 = 0.0f;
			double gamma1 = 0.0f;
			double gamma2 = 0.0f;
			
			// find radius of circle
			double R = Transformation.Distance(p1, cent);
			double Rt = Transformation.Distance(p2, cent);
			
			if (Math.Abs(R-Rt) > SELF_ACCURACY) {
				Console.WriteLine("Radius Warning: R1={0} R2={0}", R, Rt);
			}

			double val =  Math.Pow(R,2) - Math.Pow(-cent.X,2);
			
			if (val >= 0.0) {
				double root = Math.Sqrt( val );
				ycross1 = (float) (cent.Y - root);
				ycross2 = (float) (cent.Y + root);
			} else {
				return output;
			}

			double theta = GetAngle(p1.X-cent.X,p1.Y-cent.Y); // Note! no code

			var betaTuple = Transform(p2.X-cent.X,p2.Y-cent.Y,Transformation.DegreeToRadian(-theta));
			double xbeta = betaTuple.Item1;
			double ybeta = betaTuple.Item2;
			double beta = GetAngle(xbeta, ybeta, code);
			
			if (Math.Abs(beta) <= SELF_ZERO) {
				beta = 360.0;
			}

			var xyTransTuple = Transform(-cent.X,ycross1-cent.Y,Transformation.DegreeToRadian(-theta));
			double xt = xyTransTuple.Item1;
			double yt = xyTransTuple.Item2;
			double gt1 = GetAngle(xt,yt,code);
			
			var xyTransTuple2 = Transform(-cent.X,ycross2-cent.Y,Transformation.DegreeToRadian(-theta));
			double xt2 = xyTransTuple2.Item1;
			double yt2 = xyTransTuple2.Item2;
			double gt2 = GetAngle(xt2,yt2,code);

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
			
			var dz = p2.Z - p1.Z;
			var da = beta;
			var mz = dz/da;
			zcross1 = (float) (p1.Z + gamma1 * mz);
			zcross2 = (float) (p1.Z + gamma2 * mz);
			
			if (gamma1 < beta && gamma1 > SELF_ZERO && gamma1 < beta-SELF_ZERO)
				output.Add(new Point3D(xcross1,ycross1,zcross1));
			
			if (gamma2 < beta && gamma1 > SELF_ZERO && gamma2 < beta-SELF_ZERO)
				output.Add(new Point3D(xcross2,ycross2,zcross2));

			return output;
		}

		/// <summary>
		/// Routine takes an x and a y coords and does a cordinate transformation
		/// to a new coordinate system at angle from the initial coordinate system
		/// Returns new x,y tuple
		/// </summary>
		public static Tuple<double,double> Transform(double x, double y, double angle) {
			double newx = x * Math.Cos(angle) - y * Math.Sin(angle);
			double newy = x * Math.Sin(angle) + y * Math.Cos(angle);
			return new Tuple<double, double>(newx, newy);
		}

		/// <summary>
		/// Routine takes an sin and cos and returns the angle (between 0 and 360)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="code">CommandList, type of arc</param>
		public static double GetAngle(double x, double y, CommandType code = CommandType.CCWArc)
		{
			double angle = 90.0 - Transformation.RadianToDegree(Math.Atan2(x, y));
			if (angle < 0) {
				angle = 360 + angle;
			}
			if (code == CommandType.CWArc) {
				return (360.0 - angle);
			}
			return angle;
		}
		
		private static Point3D SetOffsetAndRotation(Point3D coords, Point3D offset, double rotate) {
			float x = coords.X;
			float y = coords.Y;
			float z = coords.Z;
			x = x - offset.X;
			y = y - offset.Y;
			z = z - offset.Z;
			var xy = Transform(x,y, Transformation.DegreeToRadian(rotate) );
			return new Point3D((float)xy.Item1, (float)xy.Item2, z);
		}

		private static Point3D UnsetOffsetAndRotation(Point3D coords, Point3D offset, double rotate) {
			float x = coords.X;
			float y = coords.Y;
			float z = coords.Z;
			var xy = Transform(x, y, Transformation.DegreeToRadian(-rotate) );
			x = (float) xy.Item1 + offset.X;
			y = (float) xy.Item2 + offset.Y;
			z = z + offset.Z;
			return new Point3D(x, y, z);
		}
		
	}
}
