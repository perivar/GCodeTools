using System;
using System.Drawing;

namespace GCode
{
	/// <summary>
	/// Transformation Utility Methods.
	/// </summary>
	public static class Transformation
	{
		const float SELF_ZERO = 0.0000001f;
		
		/// <summary>
		/// Degree to Radian
		/// </summary>
		/// <param name="angle">angle in degrees</param>
		/// <returns>angle in radians</returns>
		public static double DegreeToRadian(double angle)
		{
			return Math.PI * angle / 180.0;
		}
		
		/// <summary>
		/// Radian to Degree
		/// </summary>
		/// <param name="angle">angle in radians</param>
		/// <returns>angle in degrees</returns>
		public static double RadianToDegree(double angle)
		{
			return angle * (180.0 / Math.PI);
		}
		
		/// <summary>
		/// Calculate the distance between two points in 2D space (x and y)
		/// (Perform an euclidean calculation of two points)
		/// </summary>
		/// <param name="p1">first point</param>
		/// <param name="p2">second point</param>
		/// <returns>the euclidean distance between the two points</returns>
		public static double Distance(IPoint p1, IPoint p2)
		{
			return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
		}
		
		/// <summary>
		/// Rotate point through center with a certain angle
		/// (note that angle is negative for clockwise rotation)
		/// </summary>
		/// <param name="point">point to be rotated</param>
		/// <param name="center">center point</param>
		/// <param name="angle">angle in degrees is negative for clockwise rotation</param>
		/// <returns>rotated point</returns>
		public static PointF Rotate(PointF point, PointF center, float angle) {
			
			// If you want to rotate about arbitrary center (cx, cy)
			// then equations are:
			// x' = cx + (x-cx) * Cos(theta) - (y-cy) * Sin(theta)
			// y' = cy + (x-cx) * Sin(theta) + (y-cy) * Cos(theta)

			// https://stackoverflow.com/questions/12161277/how-to-rotate-a-vertex-around-a-certain-point
			// 1. A translation that brings point 1 to the origin
			// 2. Rotation around the origin by the required angle
			// 3. A translation that brings point 1 back to its original position
			
			double theta = Transformation.DegreeToRadian(angle);
			
			// Note that this makes the standard assumtion that the angle x is negative for clockwise rotation.
			// If that's not the case, then you would need to reverse the sign on the terms involving sin(x).
			float newX = (float)(center.X + (point.X-center.X)*Math.Cos(theta) - (point.Y-center.Y)*Math.Sin(theta));
			float newY = (float)(center.Y + (point.X-center.X)*Math.Sin(theta) + (point.Y-center.Y)*Math.Cos(theta));
			return new PointF(newX, newY);
		}
		
		/// <summary>
		/// Rotate point through origin (0,0) with a certain angle
		/// (note that angle is negative for clockwise rotation)
		/// </summary>
		/// <param name="point">point to be rotated</param>
		/// <param name="angle">angle in degrees is negative for clockwise rotation</param>
		/// <returns>rotated point</returns>
		public static PointF Rotate(PointF point, float angle) {
			
			// Example of a 2D rotation through an angle w where the coordinates
			// x, y go into x', y'.
			// Note that rotation is about the origin (0, 0).
			// x' = x * Cos(theta) - y * Sin(theta)
			// y' = y * Cos(theta) + x * Sin(theta)
			
			double theta = Transformation.DegreeToRadian(angle);
			
			float newX = (float)(point.X * Math.Cos(theta) - point.Y * Math.Sin(theta));
			float newY = (float)(point.Y * Math.Cos(theta) + point.X * Math.Sin(theta));
			return new PointF(newX, newY);
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
		public static int FindLineCircleIntersections(float cx, float cy, float radius,
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
		/// <see cref="https://www.topcoder.com/community/data-science/data-science-tutorials/geometry-concepts-line-intersection-and-its-applications/"/>
		public static PointF FindLineIntersectionPoint(PointF ps1, PointF pe1,
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
