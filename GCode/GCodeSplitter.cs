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
		const float selfZero = 0.0000001f;
		const float selfAccuracy = 0.025f; // mm, inch= 0.01;
		
		enum Position {
			None = -1,
			L = 0,
			R = 1
		}
		
		//public static void Split(List<Plot> myPlots, float xsplit)
		public static void Split(List<GCodeInstruction> instructions, float xsplit)
		{
			CommandList mvtype = CommandList.Other;  // G0 (Rapid), G1 (linear), G2 (clockwise arc) or G3 (counterclockwise arc).

			var app = new List<List<GCodeInstruction>>();
			app.Add(new List<GCodeInstruction>());
			app.Add(new List<GCodeInstruction>());
			
			int thisSide = -1;
			int otherSide = -1;
			
			float currentFeedrate = 50.0f;
			
			var flag_side = Position.None;
			
			var currentPos = Point3D.Empty;
			var previousPos = Point3D.Empty;
			var center = Point3D.Empty;
			
			var A = Point3D.Empty;
			var B = Point3D.Empty;
			var C = Point3D.Empty;
			var D = Point3D.Empty;
			var E = Point3D.Empty;
			
			var cross = new List<Point3D>();
			
			int numInstructions = 1;
			Point3D currentPlot = Point3D.Empty;
			foreach (var instruction in instructions)
			{
				// merge previous coordinates with newer ones to maintain correct point coordinates
				if (instruction.CanRender() &&
				    (instruction.X.HasValue || instruction.Y.HasValue || instruction.Z.HasValue
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

				mvtype = instruction.CommandEnum;
				//if (previousPos.IsEmpty) continue;
				
				// ignore rapid moves
				if (mvtype == CommandList.RapidMove) {
					continue;
				}
				
				if (mvtype == CommandList.NormalMove
				    || mvtype == CommandList.CWArc
				    || mvtype == CommandList.CCWArc) {
					//pos      = self.coordop(POS,shift,angle)
					//pos_last = self.coordop(POS_LAST,shift,angle)
					//pos = instruction.StartPoint;
					//pos_last = instruction.EndPoint;
					
					if (instruction.I.HasValue && instruction.J.HasValue) {
						center = new Point3D(previousPos.X+instruction.I.Value,
						                     previousPos.Y+instruction.J.Value);
						
					}
					
					//if (CENTER[0] !="" && CENTER[1] !="") {
					//center = self.coordop(CENTER,shift,angle)
					//}
					
					if (currentPos.X > xsplit+selfZero) {
						flag_side = Position.R;
					} else if (currentPos.X < xsplit-selfZero) {
						flag_side = Position.L;
					} else {
						if (mvtype == CommandList.NormalMove) {
							if (previousPos.X >= xsplit) {
								flag_side = Position.R;
							} else {
								flag_side = Position.L;
							}
						} else if (mvtype == CommandList.CWArc) {
							if (Math.Abs(currentPos.Y-center.Y) < selfZero) {
								if (center.X > xsplit) {
									flag_side = Position.R;
								} else {
									flag_side = Position.L;
								}
							} else {
								if (currentPos.Y >= center.Y) {
									flag_side = Position.R;
								} else {
									flag_side = Position.L;
								}
							}
						} else { //(mvtype == 3) {
							if (Math.Abs(currentPos.Y-center.Y) < selfZero) {
								if (center.X > xsplit) {
									flag_side = Position.R;
								} else {
									flag_side = Position.L;
								}
							} else {
								if (currentPos.Y >= center.Y) {
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
					
					if (mvtype == CommandList.NormalMove) {
						//A  = self.coordunop(pos_last[:],shift,angle)
						//C  = self.coordunop(pos[:]     ,shift,angle)
						//cross = self.get_line_intersect(pos_last, pos, xsplit)
						A = previousPos;
						C = currentPos;
						cross = GetLineIntersect(previousPos, currentPos, xsplit);

						if (cross.Count > 0) { // Line crosses boundary
							//B  = self.coordunop(cross[0]   ,shift,angle)
							//app[this] ( [mvtype,A,B,feed] )
							//app[other]( [mvtype,B,C,feed] )
							B = cross[0];
							app[thisSide].Add(new GCodeInstruction(mvtype, A, B, currentFeedrate));
							app[otherSide].Add(new GCodeInstruction(mvtype, B, C, currentFeedrate));
						} else {
							//app[this] ( [mvtype,A,C,feed] )
							app[thisSide].Add(new GCodeInstruction(mvtype, A, C, currentFeedrate));
						}
					}
					
					if (mvtype == CommandList.CWArc || mvtype == CommandList.CCWArc) {
						//A  = self.coordunop(pos_last[:],shift,angle)
						//C  = self.coordunop(pos[:]     ,shift,angle)
						//D  = self.coordunop(center     ,shift,angle)
						A = previousPos;
						C = currentPos;
						D  = center;
						cross = GetArcIntersect2(previousPos, currentPos, xsplit, center, mvtype);

						if (cross.Count > 0) {
							// Arc crosses boundary at least once
							//B = self.coordunop(cross[0]   ,shift,angle)
							B = cross[0];
							
							// Check length of arc before writing
							if (Distance(B, A) > selfAccuracy) {
								//app[this]( [mvtype,A,B,D,feed])
								app[thisSide].Add(new GCodeInstruction(mvtype, A, B, D, currentFeedrate));
							}
							
							if (cross.Count == 1) { // Arc crosses boundary only once
								// Check length of arc before writing
								if (Distance(C, B) > selfAccuracy) {
									//app[other]([ mvtype,B,C,D, feed] )
									app[otherSide].Add(new GCodeInstruction(mvtype, B, C, D, currentFeedrate));
								}
							}
							
							if (cross.Count == 2) { // Arc crosses boundary twice
								//E  = self.coordunop(cross[1],shift,angle)
								E = cross[1];
								
								// Check length of arc before writing
								if (Distance(E, B) > selfAccuracy) {
									// app[other]([ mvtype,B,E,D, feed] )
									app[otherSide].Add(new GCodeInstruction(mvtype, B, E, D, currentFeedrate));
								}
								
								// Check length of arc before writing
								if (Distance(C, E) > selfAccuracy) {
									//app[this] ([ mvtype,E,C,D, feed] )
									app[thisSide].Add(new GCodeInstruction(mvtype, E, C, D, currentFeedrate));
								}
							}
						} else {
							// Arc does not cross boundary
							//app[this]([ mvtype,A,C,D, feed])
							app[thisSide].Add(new GCodeInstruction(mvtype, A, C, D, currentFeedrate));
						}
					}
				} else {
					app[0].Add(instruction);
					app[1].Add(instruction);
				}
				
				// store current position
				previousPos = currentPos;
				
				numInstructions++;
			}

			// Save
			DumpGCode("first.gcode", app[0]);
			DumpGCode("second.gcode", app[1]);
		}

		private static void DumpGCode(string fileName, List<GCodeInstruction> instructions) {
			
			var file = new FileInfo(fileName);
			using (TextWriter tw = new StreamWriter(file.Open(FileMode.Truncate))) {
				foreach (var gCodeLine in instructions) {
					tw.WriteLine(gCodeLine);
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

			if (xcross > Math.Min(p1.X,p2.X) + selfZero
			    && xcross < Math.Max(p1.X,p2.X) - selfZero) {
				
				var point = new Point3D() { X = xcross, Y = ycross, Z = zcross };
				output.Add(point);
			}
			
			return output;
		}
		public static List<Point3D> GetArcIntersect2(Point3D p1, Point3D p2, float xsplit, Point3D cent, CommandList code) {

			var output = new List<Point3D>();

			// find radius of circle
			double R = Distance(p1, cent);
			double Rt = Distance(p2, cent);
			
			if (Math.Abs(R-Rt) > selfAccuracy) {
				Console.WriteLine("Radius Warning: R1={0} R2={0}", R, Rt);
			}
			
			var pp1 = new PointF(10,10);
			var pp2 = new PointF(10,20);
			
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
		
		public static List<Point3D> GetArcIntersect(Point3D p1, Point3D p2, float xsplit, Point3D cent, CommandList code) {
			
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
			
			if (Math.Abs(R-Rt) > selfAccuracy) {
				Console.WriteLine("Radius Warning: R1={0} R2={0}", R, Rt);
			}

			double val =  Math.Pow(R,2) - Math.Pow(xsplit - cent.X,2);
			
			if (val >= 0.0) {
				double root = Math.Sqrt( val );
				ycross1 = (float) (cent.Y - root);
				ycross2 = (float) (cent.Y + root);
			} else {
				return null;
			}

			double theta = GetAngle2(p1.X-cent.X,p1.Y-cent.Y);

			var betaTuple = Transform(p2.X-cent.X,p2.Y-cent.Y,DegreeToRadian(-theta));
			double xbeta = betaTuple.Item1;
			double ybeta = betaTuple.Item2;
			double beta = GetAngle2(xbeta, ybeta, code);
			
			if (Math.Abs(beta) <= selfZero) {
				beta = 360.0;
			}

			var xyTransTuple = Transform(xsplit-cent.X,ycross1-cent.Y,DegreeToRadian(-theta));
			double xt = xyTransTuple.Item1;
			double yt = xyTransTuple.Item2;
			double gt1 = GetAngle2(xt,yt,code);
			
			var xyTransTuple2 = Transform(xsplit-cent.X,ycross2-cent.Y,DegreeToRadian(-theta));
			double xt2 = xyTransTuple2.Item1;
			double yt2 = xyTransTuple2.Item2;
			double gt2 = GetAngle2(xt2,yt2,code);

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
			
			if (gamma1 < beta && gamma1 > selfZero && gamma1 < beta-selfZero)
				output.Add(new Point3D() { X=xcross1,Y=ycross1,Z=zcross1 });
			
			if (gamma2 < beta && gamma1 > selfZero && gamma2 < beta-selfZero)
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

		/// <summary>
		/// routine takes an x and a y coords and does a cordinate transformation
		/// to a new coordinate system at angle from the initial coordinate system
		/// Returns new x,y tuple
		/// </summary>
		public static Tuple<double,double> Transform(double x, double y, double angle) {
			double newx = x * Math.Cos(angle) - y * Math.Sin(angle);
			double newy = x * Math.Sin(angle) + y * Math.Cos(angle);
			return new Tuple<double, double>(newx, newy);
		}

		/// <summary>
		/// routine takes an sin and cos and returns the angle (between 0 and 360)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="code"></param>
		public static double GetAngle2(double x, double y, string code = "") {
			double angle = 90.0 - RadianToDegree(Math.Atan2(x,y));
			if (angle < 0) {
				angle = 360 + angle;
				if (code == "G2") {
					return (360.0 - angle);
				}
			}
			return angle;
		}

		/// <summary>
		/// routine takes an sin and cos and returns the angle (between 0 and 360)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="code"></param>
		public static double GetAngle2(double x, double y, CommandList code) {
			double angle = 90.0 - RadianToDegree(Math.Atan2(x,y));
			if (angle < 0) {
				angle = 360 + angle;
				if (code == CommandList.CWArc) {
					return (360.0 - angle);
				}
			}
			return angle;
		}
		
		private static Point3D coordop(Point3D coords, Point3D offset, double rot) {
			float x = coords.X;
			float y = coords.Y;
			float z = coords.Z;
			x = x - offset.X;
			y = y - offset.Y;
			z = z - offset.Z;
			var xy = Transform(x,y, DegreeToRadian(rot) );
			return new Point3D() { X=(float)xy.Item1, Y=(float)xy.Item2, Z=z };
		}

		private static Point3D coordunop(Point3D coords, Point3D offset, double rot) {
			float x = coords.X;
			float y = coords.Y;
			float z = coords.Z;
			var xy = Transform(x, y, DegreeToRadian(-rot) );
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
		/// Find the points of intersection.
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
			if ((A <= 0.0000001) || (det < 0))
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
	}
}
