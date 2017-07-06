using System;
using System.Collections.Generic;
using System.Drawing;

namespace SVG
{
	/// <summary>
	/// Description of Bezier.
	/// http://devmag.org.za/2011/04/05/bzier-curves-a-tutorial/
	/// https://gist.github.com/valryon/b782bb76f543354f11f7
	/// </summary>
	public static class Bezier
	{
		/// <summary>
		/// Cubic curves (or degree-three curves)
		/// </summary>
		/// <param name="t">Time, between 0 and 1</param>
		/// <param name="p0">Source</param>
		/// <param name="p1">Control point 1</param>
		/// <param name="p2">Control point 1</param>
		/// <param name="p3">Destination</param>
		/// <returns>cubic value</returns>
		public static float Cubic(float t, float p0, float p1, float p2, float p3) {
			
			// [x,y] = (1–t)^3 * P0
			// + 3(1–t)^2 * t * P1
			// + 3(1–t)t^2 * P2
			// + t^3 * P3
			
			float fW = 1 - t;
			float fA = fW * fW * fW;
			float fB = 3 * fW * fW * t;
			float fC = 3 * fW * t * t;
			float fD = t * t * t;

			float f = fA * p0 + fB * p1 + fC * p2 + fD * p3;
			
			return f;
		}
		
		/// <summary>
		/// Quadratic curves (or degree-two curves)
		/// </summary>
		/// <param name="t">Time, between 0 and 1</param>
		/// <param name="p0">Source</param>
		/// <param name="p1">Control point</param>
		/// <param name="p2">Destination</param>
		/// <returns></returns>
		public static float Quadratic(float t, float p0, float p1, float p2)
		{
			// [x,y] = (1–t)^2 * P0
			// + 2(1–t) * t * P1
			// + t^2 * P2

			float fW = 1 - t;
			float fA = fW * fW;
			float fB = 2 * fW * t;
			float fC = t * t;

			float f = fA * p0 + fB * p1 + fC * p2;
			
			return f;
		}
		
		//  Draw the Bezier curve.
		public static void DrawBezier(Graphics g, float dt, PointF pt0, PointF pt1, PointF pt2, PointF pt3) {
			
			//  Draw the control lines.
			g.Clear(Color.White);
			
			g.DrawLine(Pens.Black, pt0.X, pt0.Y, pt1.X, pt1.Y);
			g.DrawLine(Pens.Black, pt1.X, pt1.Y, pt2.X, pt2.Y);
			g.DrawLine(Pens.Black, pt2.X, pt2.Y, pt3.X, pt3.Y);
			
			//  Draw the curve.
			float t;
			float x0;
			float y0;
			float x1;
			float y1;
			
			t = 0;
			x1 = Cubic(t, pt0.X, pt1.X, pt2.X, pt3.X);
			y1 = Cubic(t, pt0.Y, pt1.Y, pt2.Y, pt3.Y);
			
			t = (t + dt);
			while ((t < 1)) {
				x0 = x1;
				y0 = y1;
				x1 = Cubic(t, pt0.X, pt1.X, pt2.X, pt3.X);
				y1 = Cubic(t, pt0.Y, pt1.Y, pt2.Y, pt3.Y);
				
				g.DrawLine(Pens.Orange, x0, y0, x1, y1);

				t = (t + dt);
			}
			
			//  Connect to the final point.
			t = 1;
			x0 = x1;
			y0 = y1;
			
			x1 = Cubic(t, pt0.X, pt1.X, pt2.X, pt3.X);
			y1 = Cubic(t, pt0.Y, pt1.Y, pt2.Y, pt3.Y);
			
			g.DrawLine(Pens.Orange, x0, y0, x1, y1);
		}
		
		/// <summary>
		/// Add Cubic Bezier Points (Degree-three curves)
		/// </summary>
		/// <param name="dt">delta step (1.0 / number of segments)</param>
		/// <param name="pt0">point</param>
		/// <param name="pt1">point</param>
		/// <param name="pt2">point</param>
		/// <param name="pt3">point</param>
		/// <returns>list of segmented bezier curves</returns>
		public static List<PointF> AddBezier(float dt, PointF pt0, PointF pt1, PointF pt2, PointF pt3) {
			
			var points = new List<PointF>();
			
			float t;
			float x0;
			float y0;
			float x1;
			float y1;
			
			t = 0;
			x1 = Cubic(t, pt0.X, pt1.X, pt2.X, pt3.X);
			y1 = Cubic(t, pt0.Y, pt1.Y, pt2.Y, pt3.Y);
			t = (t + dt);
			
			// Add very first point
			points.Add(new PointF(x1, y1));
			
			while ((t < 1)) {
				x0 = x1;
				y0 = y1;
				x1 = Cubic(t, pt0.X, pt1.X, pt2.X, pt3.X);
				y1 = Cubic(t, pt0.Y, pt1.Y, pt2.Y, pt3.Y);
				
				points.Add(new PointF(x1, y1));
				t = (t + dt);
			}
			
			// Connect to the final point.
			// One more at t = 1
			t = 1;
			x0 = x1;
			y0 = y1;
			x1 = Cubic(t, pt0.X, pt1.X, pt2.X, pt3.X);
			y1 = Cubic(t, pt0.Y, pt1.Y, pt2.Y, pt3.Y);

			points.Add(new PointF(x1, y1));
			
			return points;
		}
		
		/// <summary>
		/// Add Quadratic Bezier Points (Degree-two curves)
		/// </summary>
		/// <param name="dt">delta step (1.0 / number of segments)</param>
		/// <param name="pt0">point</param>
		/// <param name="pt1">point</param>
		/// <param name="pt2">point</param>
		/// <returns></returns>
		public static List<PointF> AddQuadBezier(float dt, PointF pt0, PointF pt1, PointF pt2) {

			var points = new List<PointF>();

			float t;
			float x0;
			float y0;
			float x1;
			float y1;

			t = 0;
			x1 = Quadratic(t, pt0.X, pt1.X, pt2.X);
			y1 = Quadratic(t, pt0.Y, pt1.Y, pt2.Y);
			t = (t + dt);
			
			// Add very first point
			points.Add(new PointF(x1, y1));

			while ((t < 1)) {
				x0 = x1;
				y0 = y1;
				x1 = Quadratic(t, pt0.X, pt1.X, pt2.X);
				y1 = Quadratic(t, pt0.Y, pt1.Y, pt2.Y);
				
				points.Add(new PointF(x1, y1));
				t = (t + dt);
			}
			
			// Connect to the final point.
			// One more at t = 1
			t = 1;
			x0 = x1;
			y0 = y1;
			x1 = Quadratic(t, pt0.X, pt1.X, pt2.X);
			y1 = Quadratic(t, pt0.Y, pt1.Y, pt2.Y);

			points.Add(new PointF(x1, y1));
			
			return points;
		}
	}
}
