using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using GCode;

namespace Util
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
			const double Deg2Rad = Math.PI / 180.0;
			return angle * Deg2Rad;
		}
		
		/// <summary>
		/// Radian to Degree
		/// </summary>
		/// <param name="angle">angle in radians</param>
		/// <returns>angle in degrees</returns>
		public static double RadianToDegree(double angle)
		{
			const double Rad2Deg = 180.0 / Math.PI;
			return angle * Rad2Deg;
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
			// From a Math point of view, the distance between two points in the same plane
			// is the square root of the sum from the power of two from each side in a triangle
			// distance = Math.Sqrt((x1-x2)*(x1-x2) + (y1-y2)*(y1-y2));
			// Or alternatively:
			// distance = Math.Sqrt(Math.Pow((x1-x2), 2) + Math.Pow((y1-y2), 2));
			
			return Math.Sqrt( Math.Pow(Math.Abs(p2.X - p1.X), 2) + Math.Pow(Math.Abs(p2.Y - p1.Y), 2) );
		}

		/// <summary>
		/// Return the euclidean distance between two points
		/// </summary>
		/// <param name="a">first point</param>
		/// <param name="b">second point</param>
		/// <returns>the euclidean distance between two points</returns>
		public static double Distance(PointF a, PointF b)
		{
			// From a Math point of view, the distance between two points in the same plane
			// is the square root of the sum from the power of two from each side in a triangle
			// distance = Math.sqrt((x1-x2)*(x1-x2) + (y1-y2)*(y1-y2));
			// Or alternatively:
			// distance = Math.sqrt(Math.pow((x1-x2), 2) + Math.pow((y1-y2), 2));

			double xd = Math.Abs(a.X - b.X);
			double yd = Math.Abs(a.Y - b.Y);

			xd = xd * xd;
			yd = yd * yd;

			return Math.Sqrt(xd + yd);
		}

		/// <summary>
		/// Return the bounding rectangle around a list of points
		/// </summary>
		/// <param name="points">points</param>
		/// <returns>the bounding rectangle</returns>
		public static RectangleF BoundingRect(IEnumerable<PointF> points)
		{
			var x_query = from PointF p in points select p.X;
			float xmin = x_query.Min();
			float xmax = x_query.Max();

			var y_query = from PointF p in points select p.Y;
			float ymin = y_query.Min();
			float ymax = y_query.Max();

			return new RectangleF(xmin, ymin, xmax - xmin, ymax - ymin);
		}
		
		/// <summary>
		/// Return the center point of the bounding rectangle for a list of points
		/// Note that this is not necceserily correct for skewed polygons
		/// </summary>
		/// <param name="points">list of points</param>
		/// <returns>the centerpoint of the bounding rectangle</returns>
		public static PointF Center(IEnumerable<PointF> points)
		{
			var rect = BoundingRect(points);
			
			return new PointF(rect.Left + rect.Width/2,
			                  rect.Top + rect.Height / 2);
		}
		
		/// <summary>
		/// Test if the passed list of points is a circle
		/// </summary>
		/// <param name="points">list of points</param>
		/// <returns>true if circle has been detected</returns>
		public static bool IsPolygonCircle(IEnumerable<PointF> points) {
			
			// A circle:
			// 1. Has more than 6 vertices.
			// 2. Has diameter of the same size in each direction.
			// 3. The area of the contour is ~πr2
			
			if (points.Count() < 6) return false;
			
			double area = PolygonArea(points);
			var r = BoundingRect(points);
			float radius = r.Width / 2;
			
			if (Math.Abs( 1 - ((double)r.Width / r.Height)) <= 0.2 &&
			    Math.Abs(1 - (area / (Math.PI * Math.Pow(radius, 2)))) <= 0.2)
			{
				// found circle
				return true;
			}
			return false;
		}
		
		/// <summary>
		/// If the polygon is a circle, this method can be used to
		/// return the center point and radiu
		/// </summary>
		/// <param name="points">list of points</param>
		/// <param name="center">out center point</param>
		/// <param name="radius">out radius</param>
		public static void GetCenterAndRadiusForPolygonCircle(IEnumerable<PointF> points, ref PointF center, out float radius) {

			center = new PointF {
				X = (float)(points.Average(p => p.X)),
				Y = (float)(points.Average(p => p.Y))
			};
			
			var radiuses = new List<double>();
			foreach (var point in points) {
				double rad = Distance(center, point);
				radiuses.Add(rad);
			}
			radius = (float)radiuses.Average();
		}
		
		/// <summary>
		/// Determine the area of the passed polygon
		/// </summary>
		/// <param name="points">contour</param>
		/// <returns>area</returns>
		public static double PolygonArea(IEnumerable<PointF> points) {
			
			int i,j;
			double area = 0;
			for (i=0; i < points.Count(); i++) {
				j = (i + 1) % points.Count();
				area += points.ElementAt(i).X * points.ElementAt(j).Y;
				area -= points.ElementAt(i).Y * points.ElementAt(j).X;
			}
			area /= 2;
			return (area < 0 ? -area : area);
		}

		/// <summary>
		/// Reflect the point 180 degrees around the origin
		/// </summary>
		/// <param name="ptReflect">point to reflect</param>
		/// <param name="ptOrigin">origin point</param>
		/// <returns>new reflected point</returns>
		public static PointF ReflectAbout(PointF ptReflect, PointF ptOrigin)
		{
			PointF tempReflectAbout = Point.Empty;
			
			// Reflect ptReflect 180 degrees around ptOrigin
			tempReflectAbout.X = (-(ptReflect.X - ptOrigin.X)) + ptOrigin.X;
			tempReflectAbout.Y = (-(ptReflect.Y - ptOrigin.Y)) + ptOrigin.Y;
			return tempReflectAbout;
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
			
			double theta = DegreeToRadian(angle);
			
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
			
			double theta = DegreeToRadian(angle);
			
			float newX = (float)(point.X * Math.Cos(theta) - point.Y * Math.Sin(theta));
			float newY = (float)(point.Y * Math.Cos(theta) + point.X * Math.Sin(theta));
			return new PointF(newX, newY);
		}
		
		/// <summary>
		/// Rotate point around a center point
		/// </summary>
		/// <param name="inPoint">point to rotate</param>
		/// <param name="theta">angle in radians</param>
		/// <param name="centerPoint">center point</param>
		/// <returns>rotated point</returns>
		/// <see cref="https://academo.org/demos/rotation-about-point/"/>
		public static PointF RotatePoint(PointF inPoint, PointF centerPoint, double theta)
		{
			// Imagine a point located at (x,y). If you wanted to rotate that point around the origin,
			// the coordinates of the new point would be located at (x',y').
			// x′= xcosθ − ysinθ
			// y′= ycosθ + xsinθ
			
			PointF tempRotatePoint = PointF.Empty;

			tempRotatePoint = inPoint;

			// first move point to origin
			tempRotatePoint.X = tempRotatePoint.X - centerPoint.X;
			tempRotatePoint.Y = tempRotatePoint.Y - centerPoint.Y;

			// rotate
			tempRotatePoint.X = (float)((Math.Cos(theta) * tempRotatePoint.X) + (-Math.Sin(theta) * tempRotatePoint.Y));
			tempRotatePoint.Y = (float)((Math.Sin(theta) * tempRotatePoint.X) + (Math.Cos(theta) * tempRotatePoint.Y));

			// then move point back to where it was originally
			tempRotatePoint.X = tempRotatePoint.X + centerPoint.X;
			tempRotatePoint.Y = tempRotatePoint.Y + centerPoint.Y;

			return tempRotatePoint;
		}
		
		/// <summary>
		/// Calculates angle in radians between two points and x-axis.
		/// Note this is not screen coordinates, but where Y axis is
		/// positive above X.
		/// </summary>
		/// <param name="centerPoint">Point we are rotating around.</param>
		/// <param name="targetPoint">Point we want to calcuate the angle to</param>
		/// <returns>angle in radians</returns>
		public static double GetAngleRadians(PointF centerPoint, PointF targetPoint)
		{
			double tempAngleFromPoint = 0;
			
			// Calculate the angle of a point relative to the center
			// Slope is rise over run
			double slope = 0;

			if (targetPoint.X == centerPoint.X) {
				// Either 90 or 270
				tempAngleFromPoint = ((targetPoint.Y > centerPoint.Y) ? Math.PI / 2 : -Math.PI / 2);

			} else if (targetPoint.X > centerPoint.X) {
				// 0 - 90 and 270 - 360
				slope = (targetPoint.Y - centerPoint.Y) / (targetPoint.X - centerPoint.X);
				tempAngleFromPoint = Math.Atan(slope);
			} else {
				// 180 - 270
				slope = (targetPoint.Y - centerPoint.Y) / (targetPoint.X - centerPoint.X);
				tempAngleFromPoint = Math.Atan(slope) + Math.PI;
			}

			if (tempAngleFromPoint < 0) {
				tempAngleFromPoint = tempAngleFromPoint + (Math.PI * 2);
			}
			
			return tempAngleFromPoint;
		}
		
		/// <summary>
		/// Calculates angle in degrees between two points and x-axis.
		/// Note this is not screen coordinates, but where Y axis is
		/// positive above X.
		/// </summary>
		/// <param name="centerPoint">Point we are rotating around.</param>
		/// <param name="targetPoint">Point we want to calcuate the angle to</param>
		/// <returns>angle in degrees</returns>
		public static double GetAngle(PointF centerPoint, PointF targetPoint) {
			
			// NOTE: Remember that most math has the Y axis as positive above the X.
			// However, for screens we have Y as positive below. For this reason,
			// the Y values can be inverted to get the expected results.
			// E.g.
			// double deltaY = (centerPoint.Y - targetPoint.Y);
			
			// calculate delta x and delta y between the two points
			double deltaY = (targetPoint.Y - centerPoint.Y);
			double deltaX = (targetPoint.X - centerPoint.X);
			
			// Calculate the angle theta from the deltaY and deltaX values
			// (atan2 returns radians values from [-PI,PI])
			// 0 currently points EAST.
			// NOTE: By preserving Y and X param order to atan2,  we are expecting
			// a CLOCKWISE angle direction.
			double theta = Math.Atan2(deltaY, deltaX);
			
			// Convert from radians to degrees
			double angle = RadianToDegree(theta);
			
			// rotate the theta angle clockwise by 90 degrees
			// (this makes 0 point NORTH)
			// NOTE: adding to an angle rotates it clockwise.
			// subtracting would rotate it counter-clockwise
			//angle += 90.0;
			
			// Convert to positive range [0-360)
			// since we want to prevent negative angles, adjust them now.
			// we can assume that atan2 will not return a negative value
			// greater than one partial rotation
			if (angle < 0) {
				angle += 360;
			}
			
			return angle;
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
			float A1 = pe1.Y - ps1.Y;
			float B1 = ps1.X - pe1.X;
			float C1 = A1*ps1.X + B1*ps1.Y;
			
			// Get A,B,C of second line - points : ps2 to pe2
			float A2 = pe2.Y - ps2.Y;
			float B2 = ps2.X - pe2.X;
			float C2 = A2*ps2.X + B2*ps2.Y;
			
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
		
		/// <summary>
		/// Determine Area of a triangle given by the three coordinate points
		/// </summary>
		/// <param name="A">point A</param>
		/// <param name="B">point B</param>
		/// <param name="C">point C</param>
		/// <returns>area</returns>
		public static double AreaOfTriangle(IPoint A, IPoint B, IPoint C)
		{
			// Heron's formula states that the area of a triangle whose sides have lengths
			// a, b, and c is:
			// A = sqrt ( s(s-a)(s-b)(s-c) )
			// where s is the semiperimeter of the triangle; that is,
			// s = (a + b + c) / 2;
			
			double a = Distance(A, B);
			double b = Distance(A, C);
			double c = Distance(B, C);
			double s = (a + b + c) / 2;
			return Math.Sqrt(s * (s-a) * (s-b) * (s-c));
		}

		/// <summary>
		/// Determine Area of a triangle given by the three coordinate points
		/// </summary>
		/// <param name="A">point A</param>
		/// <param name="B">point B</param>
		/// <param name="C">point C</param>
		/// <returns>area</returns>
		/// <seealso cref="http://www.mathopenref.com/coordtrianglearea.html"/>
		/// <seealso cref="https://stackoverflow.com/questions/17136084/checking-if-a-point-is-inside-a-rotated-rectangle"/>
		public static double AreaOfTriangleFast(IPoint A, IPoint B, IPoint C) {
			
			// Given the coordinates of the three vertices of any triangle,
			// the area of the triangle is given by:
			// area = abs (Ax(By−Cy) + Bx(Cy−Ay) + Cx(Ay−By)) / 2
			//
			// where Ax and Ay are the x and y coordinates of the point A etc..
			// i.e
			// area = Math.Abs( (Ax * By - Ax * Cy) + (Bx * Cy - Bx * Ay) + (Cx * Ay - Cx * By) ) / 2
			return Math.Abs( (A.X * B.Y - A.X * C.Y) + (B.X * C.Y - B.X * A.Y) + (C.X * A.Y - C.X * B.Y) ) / 2;
		}
		
		/// <summary>
		/// Determine Area of a rectangle given by the only three coordinate points
		/// </summary>
		/// <param name="A">point A</param>
		/// <param name="B">point B</param>
		/// <param name="C">point C</param>
		/// <param name="D">point D</param>
		/// <returns>area of rectangle</returns>
		/// <seealso cref="http://www.mathopenref.com/coordrectangle.html"/>
		public static double AreaOfRectangle(IPoint A, IPoint B, IPoint C, IPoint D) {
			
			double side1 = Distance(A, B);
			double side2 = Distance(B, C);
			double area = side1 * side2;
			return area;
		}
		
		/// <summary>
		/// Determine Area of a rectangle given by the only three coordinate points
		/// </summary>
		/// <param name="A">point A</param>
		/// <param name="B">point B</param>
		/// <param name="C">point C</param>
		/// <param name="D">point D</param>
		/// <returns>area of rectangle</returns>
		/// <seealso cref="http://www.mathopenref.com/coordrectangle.html"/>
		/// <seealso cref="http://www.mathopenref.com/coordpolygonarea.html"/>
		/// <seealso cref="https://martin-thoma.com/how-to-check-if-a-point-is-inside-a-rectangle/"/>
		public static double AreaOfRectangleFast(IPoint A, IPoint B, IPoint C, IPoint D) {

			// If you know the coordinates of the points, you can calculate the area of the rectangle like this:
			// A = 1/2 | ( Ay−Cy) * (Dx−Bx) + (By−Dy) * (Ax−Cx) |
			return Math.Abs( ((A.Y-C.Y) * (D.X-B.X)) + ((B.Y-D.Y) * (A.X-C.X)) ) / 2;
		}
		
		/// <summary>
		/// Check if a given point is within the rectangle
		/// (the rectangle can be both rotated or straight)
		/// </summary>
		/// <param name="A">rectangle point A</param>
		/// <param name="B">rectangle point B</param>
		/// <param name="C">rectangle point C</param>
		/// <param name="D">rectangle point D</param>
		/// <param name="P">point to check</param>
		/// <returns>true if point can be found within the rectangle</returns>
		/// <seealso cref="https://martin-thoma.com/how-to-check-if-a-point-is-inside-a-rectangle/"/>
		/// <seealso cref="https://math.stackexchange.com/questions/190111/how-to-check-if-a-point-is-inside-a-rectangle"/>
		public static bool RectangleContains(IPoint A, IPoint B, IPoint C, IPoint D, IPoint P) {

			double triangle1Area = AreaOfTriangleFast(A, B, P);
			double triangle2Area = AreaOfTriangleFast(B, C, P);
			double triangle3Area = AreaOfTriangleFast(C, D, P);
			double triangle4Area = AreaOfTriangleFast(D, A, P);

			double rectArea = AreaOfRectangleFast(A, B, C, D);

			double triangleAreaSum = (triangle1Area + triangle2Area + triangle3Area + triangle4Area);

			const int precision = 14;
			if(triangleAreaSum % (Math.Pow(10, precision)) >= 0.999999999999999)
			{
				triangleAreaSum = Math.Ceiling(triangleAreaSum);
			}
			
			if(triangleAreaSum == rectArea) {
				return true;
			} else {
				return false;
			}
		}

		/// <summary>
		/// Check if a given point is within the rectangle
		/// </summary>
		/// <param name="rect">rectangle</param>
		/// <param name="P">point to check</param>
		/// <returns>true if point can be found within the rectangle</returns>
		public static bool RectangleContains(RectangleF rect, IPoint P) {
			
			var A = new Point3D(rect.X, rect.Y);
			var B = new Point3D(rect.X, rect.Y+rect.Height);
			var C = new Point3D(rect.X+rect.Width, rect.Y+rect.Height);
			var D = new Point3D(rect.X+rect.Width, rect.Y);

			return RectangleContains(A, B, C, D, P);
		}
	}
}
