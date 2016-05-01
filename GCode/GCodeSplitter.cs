using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GCodePlotter
{
	/// <summary>
	/// Description of GCodeSplitter.
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
		/// <returns>List of tiles</returns>
		/// <remarks>
		/// Copied from the G-Code_Ripper-0.12 Python App
		/// Method: def split_code(self,code2split,shift=[0,0,0],angle=0.0)
		/// </remarks>
		public static List<List<GCodeInstruction>> Split(List<GCodeInstruction> instructions, Point3D shift, float angle, float zClearance)
		{
			CommandList mvtype = CommandList.Other;  // G0 (Rapid), G1 (linear), G2 (clockwise arc) or G3 (counterclockwise arc).

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
			
			const float xsplit = 0.0f;
			var pos = Point3D.Empty;			// current position, shifted
			var pos_last = Point3D.Empty;		// last position, shifted
			var center = Point3D.Empty;			// center position converteed, shifted
			
			var cross = new List<Point3D>();	// number of intersections found
			
			int numInstructions = 1;
			foreach (var instruction in instructions)
			{
				// store move type
				mvtype = instruction.CommandEnum;
				
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
				
				if (mvtype == CommandList.NormalMove
				    || mvtype == CommandList.CWArc
				    || mvtype == CommandList.CCWArc) {
					
					pos = CoordOp(currentPos, shift, angle);
					pos_last = CoordOp(previousPos, shift, angle);
					
					// store center point
					if (instruction.I.HasValue && instruction.J.HasValue) {
						centerPos = new Point3D(previousPos.X+instruction.I.Value,
						                        previousPos.Y+instruction.J.Value,
						                        currentPos.Z);
						
						center = CoordOp(centerPos, shift, angle);
					}
					
					// determine what side the move belongs to
					if (pos_last.X > xsplit+SELF_ZERO) {
						flag_side = Position.R;
					} else if (pos_last.X < xsplit-SELF_ZERO) {
						flag_side = Position.L;
					} else {
						if (mvtype == CommandList.NormalMove) {
							if (pos.X >= xsplit) {
								flag_side = Position.R;
							} else {
								flag_side = Position.L;
							}
						} else if (mvtype == CommandList.CWArc) {
							if (Math.Abs(pos_last.Y-center.Y) < SELF_ZERO) {
								if (center.X > xsplit) {
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
								if (center.X > xsplit) {
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
					
					// Ignore rapid moves
					if (mvtype == CommandList.RapidMove) continue;
					
					if (mvtype == CommandList.NormalMove) {
						A = CoordUnop(pos_last, shift, angle);
						C = CoordUnop(pos, shift, angle);
						cross = GetLineIntersect(pos_last, pos, xsplit);

						if (cross.Count > 0) {
							// Line crosses boundary
							B = CoordUnop(cross[0], shift, angle);
							app[thisSide].AddRange(GCodeInstruction.GetInstructions(mvtype, A, B, currentFeedrate, shift, thisSide, GetPreviousPoint(app[thisSide]), zClearance));
							app[otherSide].AddRange(GCodeInstruction.GetInstructions(mvtype, B, C, currentFeedrate, shift, otherSide, GetPreviousPoint(app[otherSide]), zClearance));
						} else {
							// Lines doesn't intersect
							app[thisSide].AddRange(GCodeInstruction.GetInstructions(mvtype, A, C, currentFeedrate, shift, thisSide, GetPreviousPoint(app[thisSide]), zClearance));
						}
					}
					
					if (mvtype == CommandList.CWArc || mvtype == CommandList.CCWArc ) {
						A = CoordUnop(pos_last, shift, angle);
						C = CoordUnop(pos, shift, angle);
						D  = CoordUnop(center, shift, angle);
						cross = GetArcIntersects(pos_last, pos, xsplit, center, mvtype);

						if (cross.Count > 0) {
							// Arc crosses boundary at least once
							B = CoordUnop(cross[0], shift, angle);
							
							// Check length of arc before writing
							if (Distance(B, A) > SELF_ACCURACY) {
								app[thisSide].AddRange(GCodeInstruction.GetInstructions(mvtype, A, B, D, currentFeedrate, shift, thisSide, GetPreviousPoint(app[thisSide]), zClearance));
							}
							
							if (cross.Count == 1) { // Arc crosses boundary only once
								// Check length of arc before writing
								if (Distance(C, B) > SELF_ACCURACY) {
									app[otherSide].AddRange(GCodeInstruction.GetInstructions(mvtype, B, C, D, currentFeedrate, shift, otherSide, GetPreviousPoint(app[otherSide]), zClearance));
								}
							}
							
							if (cross.Count == 2) { // Arc crosses boundary twice
								E = CoordUnop(cross[1], shift, angle);
								
								// Check length of arc before writing
								if (Distance(E, B) > SELF_ACCURACY) {
									app[otherSide].AddRange(GCodeInstruction.GetInstructions(mvtype, B, E, D, currentFeedrate, shift, otherSide, GetPreviousPoint(app[otherSide]), zClearance));
								}
								
								// Check length of arc before writing
								if (Distance(C, E) > SELF_ACCURACY) {
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
					if (instruction.CommandEnum != CommandList.RapidMove) {
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

			// Save
			//DumpGCode("first.gcode", app[0]);
			//DumpGCode("second.gcode", app[1]);
			
			// clean up the mess with too many G0 commands
			//var app0 = CleanGCode(app[0]);
			//var app1 = CleanGCode(app[1]);
			//app.Clear();
			
			//app.Add(app0);
			//app.Add(app1);
			
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
			
			GCodeInstruction prevInstruction = null;
			bool foundLastMovement = false;
			int index = 1;
			while (!foundLastMovement) {
				prevInstruction = instructions[instructions.Count - index];
				
				if (prevInstruction.CommandEnum == CommandList.CWArc
				    || prevInstruction.CommandEnum == CommandList.CCWArc
				    || prevInstruction.CommandEnum == CommandList.NormalMove
				    || prevInstruction.CommandEnum == CommandList.RapidMove) {
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

		private static List<GCodeInstruction> CleanGCode(List<GCodeInstruction> instructions) {
			
			var cleanedList = new List<GCodeInstruction>();
			GCodeInstruction previousLine = null;
			Point3D currentPos = Point3D.Empty;
			float currentFeedrate = 0.0f;
			CommandList mvtype = CommandList.Other;
			
			foreach (GCodeInstruction currentInstruction in instructions) {
				
				if (currentInstruction.Equals(previousLine)) {
					continue;
				}

				// store move type
				mvtype = currentInstruction.CommandEnum;
				
				if (mvtype == CommandList.RapidMove
				    || mvtype == CommandList.NormalMove
				    || mvtype == CommandList.CWArc
				    || mvtype == CommandList.CCWArc) {
					
					// merge previous coordinates with newer ones to maintain correct point coordinates
					if ((currentInstruction.X.HasValue || currentInstruction.Y.HasValue || currentInstruction.Z.HasValue
					     || currentInstruction.F.HasValue)) {
						if (currentInstruction.X.HasValue && currentInstruction.X.Value != currentPos.X) {
							// X changed
							currentPos.X = currentInstruction.X.Value;
						}
						if (currentInstruction.Y.HasValue && currentInstruction.Y.Value != currentPos.Y) {
							// Y changed
							currentPos.Y = currentInstruction.Y.Value;
						}
						if (currentInstruction.Z.HasValue && currentInstruction.Z.Value != currentPos.Z) {
							// Z changed
							currentPos.Z = currentInstruction.Z.Value;
						}
						if (currentInstruction.F.HasValue && currentInstruction.F.Value != currentFeedrate) {
							// F changed
							currentFeedrate = currentInstruction.F.Value;
						}
					}
				}

				cleanedList.Add(currentInstruction);
				
				// store current position
				previousLine = currentInstruction;
			}

			return cleanedList;
		}
		
		private static void DumpGCode(string fileName, List<GCodeInstruction> instructions) {
			
			// create or overwrite a file
			using (FileStream f = File.Create(fileName)) {
				using (var s = new StreamWriter(f)) {
					foreach (var gCodeLine in instructions) {
						s.WriteLine(gCodeLine);
					}
				}
			}
		}
		
		public static List<Point3D> GetLineIntersect(Point3D p1, Point3D p2, float xsplit) {
			
			var output = new List<Point3D>();
			
			float dx = p2.X - p1.X;
			float dy = p2.Y - p1.Y;
			float dz = p2.Z - p1.Z;
			
			float xcross = xsplit;
			float ycross = 0.0f;
			float zcross = 0.0f;
			
			try {
				float my = dy/dx;
				float by = p1.Y- my * p1.X;
				ycross = my*xsplit + by;
			} catch (Exception) {
				ycross = p1.Y;
			}

			try {
				float mz = dz/dx;
				float bz = p1.Z - mz * p1.X;
				zcross = mz*xsplit + bz;
			} catch (Exception) {
				zcross = p1.Z;
			}

			if (xcross > Math.Min(p1.X,p2.X) + SELF_ZERO
			    && xcross < Math.Max(p1.X,p2.X) - SELF_ZERO) {
				
				var point = new Point3D() { X = xcross, Y = ycross, Z = zcross };
				output.Add(point);
			}
			
			return output;
		}
		
		public static List<Point3D> GetLineIntersect2(Point3D p1, Point3D p2, float xsplit) {
			
			var output = new List<Point3D>();

			var ps1 = new PointF(xsplit,10);
			var pe1 = new PointF(xsplit,20);
			
			var ps2 = new PointF(p1.X, p1.Y);
			var pe2 = new PointF(p2.X, p2.Y);
			
			var intersection = FindLineIntersectionPoint(ps1, pe1, ps2, pe2);
			
			if (!intersection.IsEmpty) {
				output.Add(new Point3D(intersection.X, intersection.Y));
			}
			
			return output;
		}
		
		public static List<Point3D> GetArcIntersects(Point3D p1, Point3D p2, float xsplit, Point3D cent, CommandList code) {
			
			var output = new List<Point3D>();
			
			float xcross1 = xsplit;
			float xcross2 = xsplit;
			float ycross1 = 0.0f;
			float ycross2 = 0.0f;
			float zcross1 = 0.0f;
			float zcross2 = 0.0f;
			double gamma1 = 0.0f;
			double gamma2 = 0.0f;
			
			double R = Distance(p1, cent);
			double Rt = Distance(p2, cent);
			
			if (Math.Abs(R-Rt) > SELF_ACCURACY) {
				Console.WriteLine("Radius Warning: R1={0} R2={0}", R, Rt);
			}

			double val =  Math.Pow(R,2) - Math.Pow(xsplit - cent.X,2);
			
			if (val >= 0.0) {
				double root = Math.Sqrt( val );
				ycross1 = (float) (cent.Y - root);
				ycross2 = (float) (cent.Y + root);
			} else {
				return output;
			}

			double theta = GetAngle(p1.X-cent.X,p1.Y-cent.Y); // Note! no code

			var betaTuple = Transform(p2.X-cent.X,p2.Y-cent.Y,DegreeToRadian(-theta));
			double xbeta = betaTuple.Item1;
			double ybeta = betaTuple.Item2;
			double beta = GetAngle(xbeta, ybeta, code);
			
			if (Math.Abs(beta) <= SELF_ZERO) {
				beta = 360.0;
			}

			var xyTransTuple = Transform(xsplit-cent.X,ycross1-cent.Y,DegreeToRadian(-theta));
			double xt = xyTransTuple.Item1;
			double yt = xyTransTuple.Item2;
			double gt1 = GetAngle(xt,yt,code);
			
			var xyTransTuple2 = Transform(xsplit-cent.X,ycross2-cent.Y,DegreeToRadian(-theta));
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
				output.Add(new Point3D() { X=xcross1,Y=ycross1,Z=zcross1 });
			
			if (gamma2 < beta && gamma1 > SELF_ZERO && gamma2 < beta-SELF_ZERO)
				output.Add(new Point3D() { X=xcross2,Y=ycross2,Z=zcross2 });

			/*
				#print(" start: x1 =%5.2f y1=%5.2f z1=%5.2f" %(p1[0],     p1[1],     p1[2]))
				#print("   end: x2 =%5.2f y2=%5.2f z2=%5.2f" %(p2[0],     p2[1],     p2[2]))
				#print("center: xc =%5.2f yc=%5.2f xsplit=%5.2f code=%s" %(cent[0],cent[1],xsplit,code))
				#print("R = %f" %(R))
				#print("theta =%5.2f" %(theta))
				#print("beta  =%5.2f gamma1=%5.2f gamma2=%5.2f\n" %(beta,gamma1,gamma2))
				#cnt=0
				#for line in output:
				#    cnt=cnt+1
				#    print("arc cross%d: %5.2f, %5.2f, %5.2f" %(cnt, line[0], line[1], line[2]))
				#print(output)
				#print("----------------------------------------------\n")
			 */
			return output;
		}

		public static List<Point3D> GetArcIntersects2(Point3D p1, Point3D p2, float xsplit, Point3D cent, CommandList code) {

			var output = new List<Point3D>();

			// find radius of circle
			double R = Distance(p1, cent);
			double Rt = Distance(p2, cent);
			
			if (Math.Abs(R-Rt) > SELF_ACCURACY) {
				Console.WriteLine("Radius Warning: R1={0} R2={0}", R, Rt);
			}
			
			var pp1 = new PointF(xsplit,10);
			var pp2 = new PointF(xsplit,20);
			
			var i1 = PointF.Empty;
			var i2 = PointF.Empty;
			int numIntersections = FindLineCircleIntersections(cent.X, cent.Y, (float) R, pp1, pp2, out i1, out i2);
			if (numIntersections > 0) {
				var ip1 = new Point3D() { X = i1.X, Y = i1.Y, Z = 0 };
				output.Add(ip1);
				
				if (numIntersections == 2) {
					var ip2 = new Point3D() { X = i2.X, Y = i2.Y, Z = 0 };
					output.Add(ip2);
				}
			}
			
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
		public static double GetAngle(double x, double y, CommandList code = CommandList.CCWArc)
		{
			double angle = 90.0 - RadianToDegree(Math.Atan2(x, y));
			if (angle < 0) {
				angle = 360 + angle;
			}
			if (code == CommandList.CWArc) {
				return (360.0 - angle);
			}
			return angle;
		}
		
		private static Point3D CoordOp(Point3D coords, Point3D offset, double rotate) {
			float x = coords.X;
			float y = coords.Y;
			float z = coords.Z;
			x = x - offset.X;
			y = y - offset.Y;
			z = z - offset.Z;
			var xy = Transform(x,y, DegreeToRadian(rotate) );
			return new Point3D() { X=(float)xy.Item1, Y=(float)xy.Item2, Z=z };
		}

		private static Point3D CoordUnop(Point3D coords, Point3D offset, double rotate) {
			float x = coords.X;
			float y = coords.Y;
			float z = coords.Z;
			var xy = Transform(x, y, DegreeToRadian(-rotate) );
			x = (float) xy.Item1 + offset.X;
			y = (float) xy.Item2 + offset.Y;
			z = z + offset.Z;
			return new Point3D() { X=x, Y=y, Z=z };
		}
		
		private static double DegreeToRadian(double angle)
		{
			return Math.PI * angle / 180.0;
		}
		
		private static double RadianToDegree(double angle)
		{
			return angle * (180.0 / Math.PI);
		}
		
		private static double Distance(Point3D p1, Point3D p2)
		{
			return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
		}
		
		/// <summary>
		/// Find the points of intersection between a circle and a line
		/// </summary>
		/// <param name="cx">x coordinate of center point of circle</param>
		/// <param name="cy">y coordinate of center point of circle</param>
		/// <param name="radius">radius of circle</param>
		/// <param name="point1">line point 1</param>
		/// <param name="point2">line point 2</param>
		/// <param name="intersection1">output coordinate of first intersection if it exists</param>
		/// <param name="intersection2">output coordinate of second intersection if it exists</param>
		/// <returns>number of found intersections</returns>
		/// <see cref="http://csharphelper.com/blog/2014/09/determine-where-a-line-intersects-a-circle-in-c/"/>
		private static int FindLineCircleIntersections(float cx, float cy, float radius,
		                                               PointF point1, PointF point2, out PointF intersection1, out PointF intersection2)
		{
			float dx, dy, A, B, C, det, t;

			dx = point2.X - point1.X;
			dy = point2.Y - point1.Y;

			A = dx * dx + dy * dy;
			B = 2 * (dx * (point1.X - cx) + dy * (point1.Y - cy));
			C = (point1.X - cx) * (point1.X - cx) + (point1.Y - cy) * (point1.Y - cy) - radius * radius;

			det = B * B - 4 * A * C;
			if ((A <= SELF_ZERO) || (det < 0))
			{
				// No real solutions.
				intersection1 = new PointF(float.NaN, float.NaN);
				intersection2 = new PointF(float.NaN, float.NaN);
				return 0;
			}
			else if (det == 0)
			{
				// One solution.
				t = -B / (2 * A);
				intersection1 = new PointF(point1.X + t * dx, point1.Y + t * dy);
				intersection2 = new PointF(float.NaN, float.NaN);
				return 1;
			}
			else
			{
				// Two solutions.
				t = (float)((-B + Math.Sqrt(det)) / (2 * A));
				intersection1 = new PointF(point1.X + t * dx, point1.Y + t * dy);
				t = (float)((-B - Math.Sqrt(det)) / (2 * A));
				intersection2 = new PointF(point1.X + t * dx, point1.Y + t * dy);
				return 2;
			}
		}
		
		/// <summary>
		/// Find the point of intersection between two lines
		/// </summary>
		/// <param name="ps1">start point of first line</param>
		/// <param name="pe1">end point of first line</param>
		/// <param name="ps2">start point of second line</param>
		/// <param name="pe2">end point of second line</param>
		/// <returns>Point of intersection</returns>
		/// <see cref="http://www.wyrmtale.com/blog/2013/115/2d-line-intersection-in-c"/>
		private static PointF FindLineIntersectionPoint(PointF ps1, PointF pe1,
		                                                PointF ps2, PointF pe2)
		{
			// Get A,B,C of first line - points : ps1 to pe1
			float A1 = pe1.Y-ps1.Y;
			float B1 = ps1.X-pe1.X;
			float C1 = A1*ps1.X+B1*ps1.Y;
			
			// Get A,B,C of second line - points : ps2 to pe2
			float A2 = pe2.Y-ps2.Y;
			float B2 = ps2.X-pe2.X;
			float C2 = A2*ps2.X+B2*ps2.Y;
			
			// Get delta and check if the lines are parallel
			float delta = A1*B2 - A2*B1;
			if(delta == 0) {
				// Lines are parallell
				return PointF.Empty;
			}
			
			// now return the intersection point
			return new PointF(
				(B2*C1 - B1*C2)/delta,
				(A1*C2 - A2*C1)/delta
			);
		}
	}
}
