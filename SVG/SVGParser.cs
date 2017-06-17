// Most of this comes from the lasercam project made by Chris Yerga
// Copyright (c) 2010 Chris Yerga

// Modified by perivar@nerseth.com to support OpenScad SVGs
// Also leaned heavily on the SVG-to-GCode project https://github.com/avwuff/SVG-to-GCode
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Globalization;
using System.Diagnostics;

namespace SVG
{
	/// <summary>
	/// A collection of static SVG Util methods
	/// </summary>
	public static class SVGUtils {

		public static double Distance(PointF a, PointF b)
		{
			double xd = Math.Abs(a.X - b.X);
			double yd = Math.Abs(a.Y - b.Y);

			xd = xd * xd;
			yd = yd * yd;

			return Math.Sqrt(xd + yd);
		}

		public static RectangleF BoundingBox(IEnumerable<PointF> points)
		{
			var x_query = from PointF p in points select p.X;
			float xmin = x_query.Min();
			float xmax = x_query.Max();

			var y_query = from PointF p in points select p.Y;
			float ymin = y_query.Min();
			float ymax = y_query.Max();

			return new RectangleF(xmin, ymin, xmax - xmin, ymax - ymin);
		}
		
		public static PointF Center(IEnumerable<PointF> points)
		{
			var rect = BoundingBox(points);
			
			return new PointF(rect.Left + rect.Width/2,
			                  rect.Top + rect.Height / 2);
		}

		/// <summary>
		/// Read from the data string while a whitespace is found
		/// </summary>
		/// <param name="data">data string</param>
		/// <param name="index">starting position (incremented)</param>
		public static void SkipWhiteSpace(string data, ref int index)
		{
			// Skip any white space.
			while (index < data.Length) {
				char character = data[index];

				// List all white space characters here
				if (char.IsWhiteSpace(character) || character == ',')
				{
					// Continue
				} else {
					return;
				}
				++index;
			}
			return;
		}
		
		/// <summary>
		/// Read from the data string until a space or comma is found
		/// </summary>
		/// <param name="data">data strin</param>
		/// <param name="index">starting position (incremented)</param>
		/// <returns>the extracted token string</returns>
		public static string ExtractToken(string data, ref int index)
		{
			// Extract until we get a space or a comma
			var builder = new StringBuilder();

			int startIndex = 0;
			char character;
			bool seenMinus = false;
			bool seenE = false;

			startIndex = index;

			while (index < data.Length) {
				character = data[index];

				switch (character) {
						// Only accept numbers
					case '-':
						if (seenE) {
							builder.Append(character);
							++index;
						} else if (seenMinus || index > startIndex) {
							// We already saw a minus sign
							return builder.ToString();
						} else {
							seenMinus = true;
							builder.Append(character);
							++index;
						}
						break;
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
					case '.':
						builder.Append(character);
						++index;
						//,6.192 -10e-4,12.385
						break;
					case 'e': // Exponent
						seenE = true;
						builder.Append(character);
						++index;
						break;
					default:
						return builder.ToString();
				}
			}
			return builder.ToString();
		}
		
		/// <summary>
		/// Read a float value from the reader
		/// </summary>
		/// <param name="reader">reader</param>
		/// <param name="attributeName">name of attribute</param>
		/// <returns>a float value or zero</returns>
		public static float ReadFloat (XmlTextReader reader, string attributeName) {
			float value = 0;
			try
			{
				value = float.Parse(reader.GetAttribute(attributeName), CultureInfo.InvariantCulture);
			}
			catch (ArgumentNullException) { }
			return value;
		}
		
		/// <summary>
		/// Find out how meny segments to use for the bezier curves
		/// </summary>
		/// <param name="pStart"></param>
		/// <param name="pEnd"></param>
		/// <param name="globalDPI"></param>
		/// <returns></returns>
		public static double GetPinSegments(PointF pStart, PointF pEnd, float globalDPI)
		{
			double distance = SVGUtils.Distance(pStart, pEnd) / globalDPI;

			// with a resolution of 500 dpi, the curve should be split into 500 segments per inch. so a distance of 1 should be 500 segments, which is 0.002
			// subdivide the Bezier into 250 line segments
			double segments = 0;
			//segments = 250 * distance;
			segments = 1 * distance;
			
			return Math.Max(0.01, 1.0 / segments);
		}
		
		public static PointF ReflectAbout(PointF ptReflect, PointF ptOrigin)
		{
			PointF tempReflectAbout = Point.Empty;
			
			// Reflect ptReflect 180 degrees around ptOrigin
			tempReflectAbout.X = (-(ptReflect.X - ptOrigin.X)) + ptOrigin.X;
			tempReflectAbout.Y = (-(ptReflect.Y - ptOrigin.Y)) + ptOrigin.Y;
			return tempReflectAbout;
		}
		
		public static PointF RotatePoint(PointF inPoint, double Theta, PointF centerPoint)
		{
			PointF temprotatePoint = PointF.Empty;

			temprotatePoint = inPoint;

			temprotatePoint.X = temprotatePoint.X - centerPoint.X;
			temprotatePoint.Y = temprotatePoint.Y - centerPoint.Y;

			temprotatePoint.X = (float)((Math.Cos(Theta) * temprotatePoint.X) + (-Math.Sin(Theta) * temprotatePoint.Y));
			temprotatePoint.Y = (float)((Math.Sin(Theta) * temprotatePoint.X) + (Math.Cos(Theta) * temprotatePoint.Y));

			temprotatePoint.X = temprotatePoint.X + centerPoint.X;
			temprotatePoint.Y = temprotatePoint.Y + centerPoint.Y;

			return temprotatePoint;
		}
		
		public static double Rad2Deg(double inRad)
		{
			return inRad * (180 / Math.PI);
		}

		public static double Deg2Rad(double inDeg)
		{
			return inDeg / (180 / Math.PI);
		}
		
		public static double AngleFromVector(double vTop, double vBot, double diffX, double diffY)
		{
			double tempangleFromVect = 0;
			
			// Not sure if this working
			if (vBot == 0)
			{
				tempangleFromVect = ((vTop > 0) ? Math.PI / 2 : -Math.PI / 2);
			}
			else if (diffX >= 0)
			{
				tempangleFromVect = Math.Atan(vTop / vBot);
			}
			else
			{
				tempangleFromVect = Math.Atan(vTop / vBot) - Math.PI;
			}

			return tempangleFromVect;
		}

		public static double AngleFromPoint(PointF pCenter, PointF pPoint)
		{
			double tempangleFromPoint = 0;
			
			// Calculate the angle of a point relative to the center
			// Slope is rise over run
			double slope = 0;

			if (pPoint.X == pCenter.X)
			{
				// Either 90 or 270
				tempangleFromPoint = ((pPoint.Y > pCenter.Y) ? Math.PI / 2 : -Math.PI / 2);

			}
			else if (pPoint.X > pCenter.X)
			{
				// 0 - 90 and 270-360
				slope = (pPoint.Y - pCenter.Y) / (pPoint.X - pCenter.X);
				tempangleFromPoint = Math.Atan(slope);
			}
			else
			{
				// 180-270
				slope = (pPoint.Y - pCenter.Y) / (pPoint.X - pCenter.X);
				tempangleFromPoint = Math.Atan(slope) + Math.PI;
			}

			if (tempangleFromPoint < 0)
			{
				tempangleFromPoint = tempangleFromPoint + (Math.PI * 2);
			}
			
			return tempangleFromPoint;
		}
		
		public static List<PointF> ParseArcSegment(float RX, float RY, float rotAng, PointF P1, PointF P2, bool largeArcFlag, bool sweepFlag)
		{
			// Parse "A" command in SVG, which is segments of an arc
			// P1 is start point
			// P2 is end point

			var points = new List<PointF>();
			
			PointF centerPoint = PointF.Empty;
			double Theta = 0;
			PointF P1Prime = PointF.Empty;
			PointF P2Prime = PointF.Empty;

			PointF CPrime = PointF.Empty;
			double Q = 0;
			double qTop = 0;
			double qBot = 0;
			double c = 0;

			double startAng = 0;
			double endAng = 0;
			double Ang = 0;
			double AngStep = 0;

			PointF tempPoint = PointF.Empty;
			double tempAng = 0;
			double tempDist = 0;

			double Theta1 = 0;
			double ThetaDelta = 0;

			// Turn the degrees of rotation into radians
			Theta = Deg2Rad(rotAng);

			// Calculate P1Prime
			P1Prime.X = (float)((Math.Cos(Theta) * ((P1.X - P2.X) / 2)) + (Math.Sin(Theta) * ((P1.Y - P2.Y) / 2)));
			P1Prime.Y = (float)((-Math.Sin(Theta) * ((P1.X - P2.X) / 2)) + (Math.Cos(Theta) * ((P1.Y - P2.Y) / 2)));

			P2Prime.X = (float)((Math.Cos(Theta) * ((P2.X - P1.X) / 2)) + (Math.Sin(Theta) * ((P2.Y - P1.Y) / 2)));
			P2Prime.Y = (float)((-Math.Sin(Theta) * ((P2.X - P1.X) / 2)) + (Math.Cos(Theta) * ((P2.Y - P1.Y) / 2)));

			qTop = (((Math.Pow(RX, 2))) * ((Math.Pow(RY, 2)))) - (((Math.Pow(RX, 2))) * ((Math.Pow(P1Prime.Y, 2)))) - (((Math.Pow(RY, 2))) * ((Math.Pow(P1Prime.X, 2))));

			if (qTop < 0) // We've been given an invalid arc. Calculate the correct value.
			{
				c = Math.Sqrt((((Math.Pow(P1Prime.Y, 2))) / ((Math.Pow(RY, 2)))) + (((Math.Pow(P1Prime.X, 2))) / ((Math.Pow(RX, 2)))));

				RX = (float) (RX * c);
				RY = (float) (RY * c);

				qTop = 0;
			}

			qBot = (((Math.Pow(RX, 2))) * ((Math.Pow(P1Prime.Y, 2)))) + (((Math.Pow(RY, 2))) * ((Math.Pow(P1Prime.X, 2))));
			if (qBot != 0)
			{
				Q = Math.Sqrt((qTop) / (qBot));
			}
			else
			{
				Q = 0;
			}

			// Q is negative
			if (largeArcFlag == sweepFlag)
			{
				Q = -Q;
			}

			// Calculate Center Prime
			CPrime.X = 0;

			if (RY != 0)
			{
				CPrime.X = (float)(Q * ((RX * P1Prime.Y) / RY));
			}
			if (RX != 0)
			{
				CPrime.Y = (float)(Q * -((RY * P1Prime.X) / RX));
			}

			// Calculate center point
			centerPoint.X = (float)(((Math.Cos(Theta) * CPrime.X) - (Math.Sin(Theta) * CPrime.Y)) + ((P1.X + P2.X) / 2));
			centerPoint.Y = (float)(((Math.Sin(Theta) * CPrime.X) + (Math.Cos(Theta) * CPrime.Y)) + ((P1.Y + P2.Y) / 2));

			// Calculate Theta1
			Theta1 = AngleFromPoint(P1Prime, CPrime);
			ThetaDelta = AngleFromPoint(P2Prime, CPrime);

			Theta1 = Theta1 - Math.PI;
			ThetaDelta = ThetaDelta - Math.PI;

			if (sweepFlag) // Sweep is POSITIV
			{
				if (ThetaDelta < Theta1)
				{
					ThetaDelta = ThetaDelta + (Math.PI * 2);
				}
			}
			else // Sweep is NEGATIVE
			{
				if (ThetaDelta > Theta1)
				{
					ThetaDelta = ThetaDelta - (Math.PI * 2);
				}
			}

			startAng = Theta1;
			endAng = ThetaDelta;

			AngStep = (Math.PI / 180);
			if (! sweepFlag) // Sweep flag indicates a positive step
			{
				AngStep = -AngStep;
			}

			Debug.WriteLine("Start angle {0}, End angle {1}, Step {2}.", Rad2Deg(startAng), Rad2Deg(endAng), Rad2Deg(AngStep));

			Ang = startAng;
			do
			{
				tempPoint.X = (float)((RX * Math.Cos(Ang)) + centerPoint.X);
				tempPoint.Y = (float)((RY * Math.Sin(Ang)) + centerPoint.Y);

				tempAng = AngleFromPoint(centerPoint, tempPoint) + Theta;
				tempDist = Distance(centerPoint, tempPoint);

				tempPoint.X = (float)((tempDist * Math.Cos(tempAng)) + centerPoint.X);
				tempPoint.Y = (float)((tempDist * Math.Sin(tempAng)) + centerPoint.Y);

				points.Add(tempPoint);

				Ang = Ang + AngStep;
			} while ( ! ((Ang >= endAng && AngStep > 0) | (Ang <= endAng && AngStep < 0)));

			// Add the final point
			points.Add(P2);

			return points;
		}
	}
	
	/// <summary>
	/// Interface for a drawable object found in an SVG file. This
	/// is not a general-purpose SVG library -- it only does the
	/// bare minimum necessary to drive a CNC machine so this is
	/// mostly just to handle vectors and paths.
	/// </summary>
	public interface ISVGElement
	{
		// Retrieve the list of contours for this shape
		List<List<PointF>> GetContours();

		// System.Drawing Path
		GraphicsPath GetPath();

		// Fill and outline
		double OutlineWidth { get; }
		Color OutlineColor { get; }
		Color FillColor { get; }
	}

	/// <summary>
	/// A contour is a set of points describing a closed polygon.
	/// </summary>
	public class VectorContour
	{
		public double Brightness { get; set; }
		public Color Color { get; set; }
		public IEnumerable<PointF> Points { get; set; }
	}

	/// <summary>
	/// Base class for shapes found in SVG file. This handles common tasks
	/// such as parsing styles, applying transforms, etc.
	/// </summary>
	public class SVGShapeBase
	{
		/// <summary>
		/// Constructor for SVGShapeBase class. Called from derived class
		/// constructor.
		/// </summary>
		/// <param name="reader">XmlTextReader positioned at the XML element
		/// for the shape being constructed. This class uses it to look
		/// for style/transform attributes to apply to the shape</param>
		/// <param name="styleDictionary">Dictionary of named styles
		/// defined earlier in the SVG document, to be used should an
		/// XML style attribute with a name be encountered.</param>
		public SVGShapeBase(XmlTextReader reader, Dictionary<string, SVGStyle> styleDictionary)
		{
			string styleText = reader.GetAttribute("class");

			if (styleText != null)
			{
				string[] styleNames = styleText.Split(new [] { ' ', '\t' });

				foreach (string styleName in styleNames)
				{
					SVGStyle style = styleDictionary[styleName];

					if (style.FillColorPresent)
					{
						FillColor = style.FillColor;
					}
					if (style.OutlineColorPresent)
					{
						OutlineColor = style.OutlineColor;
					}
					if (style.OutlineWidthPresent)
					{
						OutlineWidth = style.OutlineWidth;
					}
				}
			}

			string xfs = reader.GetAttribute("transform");
			if (xfs != null)
			{
				if (xfs.StartsWith("matrix"))
				{
					xfs = xfs.Substring(6);
				}
				xfs = xfs.Trim(new [] { '(', ')' });
				var elements = xfs.Split(new [] { ' ', '\t', ',' } ).
					Select(tag => tag.Trim()).
					Where( tag => !string.IsNullOrEmpty(tag)).ToList();

				matrix = new Matrix(
					float.Parse(elements[0], CultureInfo.InvariantCulture),
					float.Parse(elements[1], CultureInfo.InvariantCulture),
					float.Parse(elements[2], CultureInfo.InvariantCulture),
					float.Parse(elements[3], CultureInfo.InvariantCulture),
					float.Parse(elements[4], CultureInfo.InvariantCulture),
					float.Parse(elements[5], CultureInfo.InvariantCulture));
			}
		}

		/// <summary>
		/// Transform the geometry of this shape as appropriate
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		public List<PointF> Transform(List<PointF> points)
		{
			PointF[] pts = points.ToArray();

			matrix.TransformPoints(pts);
			return new List<PointF>(pts);
		}

		internal Matrix matrix = new Matrix();
		internal GraphicsPath _path;
		public GraphicsPath GetPath() { return _path; }
		public double OutlineWidth { get; set; }
		public Color OutlineColor { get; set; }
		public Color FillColor { get; set; }
	}

	/// <summary>
	/// SVG Line
	/// </summary>
	public class SVGLine : SVGShapeBase, ISVGElement
	{
		List<PointF> points = new List<PointF>();

		public SVGLine(XmlTextReader reader, Dictionary<string, SVGStyle> styleDictionary)
			: base(reader, styleDictionary)
		{
			float x1 = SVGUtils.ReadFloat(reader, "x1");
			float y1 = SVGUtils.ReadFloat(reader, "y1");
			float x2 = SVGUtils.ReadFloat(reader, "x2");
			float y2 = SVGUtils.ReadFloat(reader, "y2");
			
			points.Add(new PointF(x1, y1));
			points.Add(new PointF(x2, y2));

			points = Transform(points);

			_path = new GraphicsPath();
			_path.AddLine(points[0], points[1]);
		}

		public List<List<PointF>> GetContours()
		{
			var result = new List<List<PointF>>();
			result.Add(points);

			return result;
		}
	}
	
	/// <summary>
	/// SVG Ellipse
	/// </summary>
	public class SVGEllipse : SVGShapeBase, ISVGElement
	{
		List<PointF> points = new List<PointF>();

		public SVGEllipse(XmlTextReader reader, Dictionary<string, SVGStyle> styleDictionary)
			: base(reader, styleDictionary)
		{

			//   cx = "245.46707"
			//   cy = "469.48389"
			//   rx = "13.131983"
			//   ry = "14.142136" />

			float cx = SVGUtils.ReadFloat(reader, "cx");
			float cy = SVGUtils.ReadFloat(reader, "cy");
			float rx = SVGUtils.ReadFloat(reader, "rx");
			float ry = SVGUtils.ReadFloat(reader, "ry");
			
			double A = 0;
			double x = 0;
			double y = 0;
			long rr = 0;

			rr = 2;
			if (rx > 100 | ry > 100)
			{
				rr = 1;
			}

			for (A = 0; A <= 360; A += rr)
			{
				x = Math.Cos(A * (Math.PI / 180)) * rx + cx;
				y = Math.Sin(A * (Math.PI / 180)) * ry + cy;

				points.Add(new PointF((float)x, (float)y));
			}
			
			points = Transform(points);

			_path = new GraphicsPath();
			_path.AddLine(points[0], points[1]);
		}

		public List<List<PointF>> GetContours()
		{
			var result = new List<List<PointF>>();
			result.Add(points);

			return result;
		}
	}

	/// <summary>
	/// SVG Rectangle
	/// </summary>
	public class SVGRect : SVGShapeBase, ISVGElement
	{
		List<PointF> points = new List<PointF>();

		public SVGRect(XmlTextReader reader, Dictionary<string, SVGStyle> styleDictionary)
			: base(reader, styleDictionary)
		{
			float x = SVGUtils.ReadFloat(reader, "x");
			float y = SVGUtils.ReadFloat(reader, "y");
			float w = SVGUtils.ReadFloat(reader, "width");
			float h = SVGUtils.ReadFloat(reader, "height");

			points.Add(new PointF(x, y));
			points.Add(new PointF(x + w, y));
			points.Add(new PointF(x + w, y + h));
			points.Add(new PointF(x, y + h));
			points.Add(new PointF(x, y));

			points = Transform(points);

			_path = new GraphicsPath();
			_path.AddPolygon(points.ToArray());
		}

		public List<List<PointF>> GetContours()
		{
			var result = new List<List<PointF>>();
			result.Add(points);

			return result;
		}
	}

	/// <summary>
	/// SVG Image. This is handled differently from the rest of the shapes
	/// as it cannot be represented as vector contours. It loads the bitmap
	/// and elsewhere encapsulation is broken willy-nilly and the client
	/// code reaches in and grabs said bits. If you don't like it, go on
	/// the internet and complain.
	/// </summary>
	public class SVGImage : SVGShapeBase, ISVGElement
	{
		float x = 0;
		float y = 0;
		float width, height;
		
		public Image bits;
		public RectangleF DestBounds { get; set; }

		public SVGImage(XmlTextReader reader, Dictionary<string, SVGStyle> styleDictionary, string baseDocPath)
			: base(reader, styleDictionary)
		{
			float x = SVGUtils.ReadFloat(reader, "x");
			float y = SVGUtils.ReadFloat(reader, "y");
			float w = SVGUtils.ReadFloat(reader, "width");
			float h = SVGUtils.ReadFloat(reader, "height");
			
			string path = reader.GetAttribute("xlink:href");

			string dir = Path.GetDirectoryName(baseDocPath);
			string bitspath = Path.Combine(dir, path);
			bits = Image.FromFile(bitspath);

			var pts = new PointF[2];
			pts[0].X = x;
			pts[0].Y = y;
			pts[1].X = x+w;
			pts[1].Y = y+h;
			matrix.TransformPoints(pts);

			DestBounds = new RectangleF(pts[0].X, pts[0].Y, pts[1].X - pts[0].X, pts[1].Y - pts[0].Y);
		}

		public List<List<PointF>> GetContours()
		{
			var result = new List<List<PointF>>();

			return result;
		}
	}

	/// <summary>
	/// SVG Circle. We like polygons, so we just turn it into one of them.
	/// </summary>
	public class SVGCircle : SVGShapeBase, ISVGElement
	{
		List<PointF> points = new List<PointF>();

		public SVGCircle(XmlTextReader reader, Dictionary<string, SVGStyle> styleDictionary)
			: base(reader, styleDictionary)
		{
			float cx = SVGUtils.ReadFloat(reader, "cx");
			float cy = SVGUtils.ReadFloat(reader, "cy");
			float r = SVGUtils.ReadFloat(reader, "r");

			for (double theta = 0.0; theta < 2.0*Math.PI; theta += Math.PI / 50.0)
			{
				double x = Math.Sin(theta) * r + cx;
				double y = Math.Cos(theta) * r + cy;

				points.Add(new PointF((float)x, (float)y));
			}
			points = Transform(points);

			_path = new GraphicsPath();
			_path.AddPolygon(points.ToArray());
		}

		public List<List<PointF>> GetContours()
		{
			var result = new List<List<PointF>>();
			result.Add(points);

			return result;
		}
	}

	/// <summary>
	/// SVG polygon. This maps directly to our canonical representation,
	/// so nothing fancy going on in here.
	/// </summary>
	public class SVGPolygon : SVGShapeBase, ISVGElement
	{
		List<List<PointF>> contours = new List<List<PointF>>();
		List<PointF> currentContour = new List<PointF>();

		public SVGPolygon(XmlTextReader reader, Dictionary<string, SVGStyle> styleDictionary)
			: base(reader, styleDictionary)
		{
			// Support
			// <polygon points="50 160 55 180 70 180 60 190 65 205 50 195 35 205 40 190 30 180 45 180" />
			// <polyline points="60 110, 65 120, 70 115, 75 130, 80 125, 85 140, 90 135, 95 150, 100 145"/>
			// <polygon points="850,75  958,137.5 958,262.5 850,325 742,262.6 742,137.5" />
			
			string data = reader.GetAttribute("points");
			
			// split the data string into elements
			// and remove empty strings
			var parts = data.Split(new [] { ' ', '\t', ',' } ).
				Select(tag => tag.Trim()).
				Where( tag => !string.IsNullOrEmpty(tag)).ToList();
			
			// take two and two elements
			for(int i = 0; i < parts.Count(); i += 2) {
				currentContour.Add(
					new PointF(float.Parse(parts[i], CultureInfo.InvariantCulture),
					           float.Parse(parts[i+1], CultureInfo.InvariantCulture)));
			}
			
			// Close the shape
			if (currentContour.Count > 2)
			{
				float deltaX = currentContour[0].X - currentContour[currentContour.Count - 1].X;
				float deltaY = currentContour[0].Y - currentContour[currentContour.Count - 1].Y;

				if (Math.Abs(deltaX) + Math.Abs(deltaY) > 0.001)
				{
					currentContour.Add(currentContour[0]);
				}
			}

			currentContour = Transform(currentContour);
			contours.Add(currentContour);

			_path = new GraphicsPath();
			if (currentContour.Count > 2)
			{
				_path.AddPolygon(currentContour.ToArray());
			}
		}

		public List<List<PointF>> GetContours()
		{
			return contours;
		}
	}

	/// <summary>
	/// SVG path. The XML mini-language is full-featured and complex, making
	/// the parsing here the bulk of the work. Also, for curved portions of
	/// the path we approximate with polygons.
	/// </summary>
	public class SVGPath : SVGShapeBase, ISVGElement
	{
		List<List<PointF>> contours = new List<List<PointF>>();
		List<PointF> currentContour = new List<PointF>();
		
		public SVGPath(XmlTextReader reader, Dictionary<string, SVGStyle> styleDictionary, float globalDPI)
			: base(reader, styleDictionary)
		{
			_path = new GraphicsPath();

			string data = reader.GetAttribute("d");
			if (data == null)
			{
				return;
			}

			// Parse an SVG path.
			int index = 0;
			string character = null;
			string lastCharacter = null;

			bool isRelative = false;
			bool gotFirstItem = false;

			float startX = 0;
			float startY = 0;
			float curX = 0;
			float curY = 0;

			PointF pt0 = PointF.Empty;
			PointF pt1 = PointF.Empty;
			PointF pt2 = PointF.Empty;
			PointF pt3 = PointF.Empty;
			PointF pt4 = PointF.Empty;
			PointF pt5 = PointF.Empty;
			
			string token1 = null;
			string token2 = null;
			string token3 = null;
			string token4 = null;
			string token5 = null;
			string token6 = null;
			string token7 = null;

			PointF prevPoint = PointF.Empty;
			bool hasPrevPoint = false;

			double pInSegments = 0;

			//M209.1,187.65c-0.3-0.2-0.7-0.4-1-0.4c-0.3,0-0.7,0.2-0.9,0.4c-0.3,0.3-0.4,0.6-0.4,0.9c0,0.4,0.1,0.7,0.4,1
			//c0.2,0.2,0.6,0.4,0.9,0.4c0.3,0,0.7-0.2,1-0.4c0.2-0.3,0.3-0.6,0.3-1C209.4,188.25,209.3,187.95,209.1,187.65z

			// Get rid of enter presses
			data = data.Replace("\r", " ");
			data = data.Replace("\n", " ");
			data = data.Replace("\t", " ");

			// Start parsing
			index = 0;
			while (index < data.Length) {
				character = "" + data[index];
				++index;
				isRelative = false;

				switch (character) {
					case "M":
					case "m":
					case "L":
					case "l":
					case "C":
					case "c":
					case "V":
					case "v":
					case "A":
					case "a":
					case "H":
					case "h":
					case "S":
					case "s":
					case "Z":
					case "z":
					case "Q":
					case "q":
					case "T":
					case "t":
						// Accepted character.
						lastCharacter = character;
						break;
					case " ":
						// Ignore whitespace
						break;
					default:
						// Not accepted, must be a continuation.
						if (lastCharacter != null) {
							character = lastCharacter;
							if (character == "m") { // Continuous moveto becomes lineto
								character = "l";
							}
							if (character == "M") { // Continuous moveto becomes lineto not relative
								character = "L";
							}
							index = index - 1;
						}
						break;
				}

				switch (character) {
					case " ": // Skip spaces
						break;
					case "M":
					case "m": // MOVE TO
						if (character.ToLower() == character) { // Lowercase means relative coordinates
							isRelative = true;
						}
						if (!gotFirstItem) { // Relative not valid for first item
							isRelative = false;
						}

						// Extract two coordinates
						SVGUtils.SkipWhiteSpace(data, ref index);
						token1 = SVGUtils.ExtractToken(data, ref index);
						SVGUtils.SkipWhiteSpace(data, ref index);
						token2 = SVGUtils.ExtractToken(data, ref index);

						// Set our "current" coordinates to this
						if (isRelative) {
							curX = curX + float.Parse(token1, CultureInfo.InvariantCulture);
							curY = curY + float.Parse(token2, CultureInfo.InvariantCulture);
						} else {
							curX = float.Parse(token1, CultureInfo.InvariantCulture);
							curY = float.Parse(token2, CultureInfo.InvariantCulture);
						}

						// Start a new line, since we moved
						//newLine currentLayer;

						// Add the start point to this line
						currentContour.Add(new PointF(curX, curY));
						//pData(currentLine).PathCode = pData(currentLine).PathCode + "Move to " + currX + ", " + currY + System.Environment.NewLine;

						startX = curX;
						startY = curY;
						gotFirstItem = true;
						hasPrevPoint = false;

						break;
					case "L":
					case "l": // LINE TO
						if (character.ToLower() == character) { // Lowercase means relative coordinates
							isRelative = true;
						}
						if (!gotFirstItem) { // Relative not valid for first item
							isRelative = false;
						}

						// Extract two coordinates
						SVGUtils.SkipWhiteSpace(data, ref index);
						token1 = SVGUtils.ExtractToken(data, ref index);
						SVGUtils.SkipWhiteSpace(data, ref index);
						token2 = SVGUtils.ExtractToken(data, ref index);

						// Set our "current" coordinates to this
						if (isRelative) {
							curX = curX + float.Parse(token1, CultureInfo.InvariantCulture);
							curY = curY + float.Parse(token2, CultureInfo.InvariantCulture);
						} else {
							curX = float.Parse(token1, CultureInfo.InvariantCulture);
							curY = float.Parse(token2, CultureInfo.InvariantCulture);
						}

						// Add this point to the line
						currentContour.Add(new PointF(curX, curY));
						//pData(currentLine).PathCode = pData(currentLine).PathCode + "Line to " + currX + ", " + currY + System.Environment.NewLine;

						if (!gotFirstItem) {
							startX = curX;
							startY = curY;
						}
						gotFirstItem = true;
						hasPrevPoint = false;

						break;
					case "V":
					case "v": // VERTICAL LINE TO
						if (character.ToLower() == character) { // Lowercase means relative coordinates
							isRelative = true;
						}
						if (!gotFirstItem) { // Relative not valid for first item
							isRelative = false;
						}

						// Extract one co-ordinate
						SVGUtils.SkipWhiteSpace(data, ref index);
						token1 = SVGUtils.ExtractToken(data, ref index);

						// Set our "current" coordinates to this
						if (isRelative) {
							curY = curY + float.Parse(token1, CultureInfo.InvariantCulture);
						} else {
							curY = float.Parse(token1, CultureInfo.InvariantCulture);
						}

						// Add this point to the line
						currentContour.Add(new PointF(curX, curY));
						//pData(currentLine).PathCode = pData(currentLine).PathCode + "Vertical to " + currX + ", " + currY + System.Environment.NewLine;

						if (!gotFirstItem) {
							startX = curX;
							startY = curY;
						}
						gotFirstItem = true;
						hasPrevPoint = false;

						break;
					case "H":
					case "h": // HORIZONTAL LINE TO
						if (character.ToLower() == character) { // Lowercase means relative coordinates
							isRelative = true;
						}
						if (!gotFirstItem) { // Relative not valid for first item
							isRelative = false;
						}

						// Extract one co-ordinate
						SVGUtils.SkipWhiteSpace(data, ref index);
						token1 = SVGUtils.ExtractToken(data, ref index);

						// Set our "current" coordinates to this
						if (isRelative) {
							curX = curX + float.Parse(token1, CultureInfo.InvariantCulture);
						} else {
							curX = float.Parse(token1, CultureInfo.InvariantCulture);
						}

						// Add this point to the line
						currentContour.Add(new PointF(curX, curY));
						//pData(currentLine).PathCode = pData(currentLine).PathCode + "Horiz to " + currX + ", " + currY + System.Environment.NewLine;

						if (!gotFirstItem) {
							startX = curX;
							startY = curY;
						}
						gotFirstItem = true;
						hasPrevPoint = false;

						break;
					case "A":
					case "a": // PARTIAL ARC TO
						if (character.ToLower() == character) { // Lowercase means relative coordinates
							isRelative = true;
						}
						if (!gotFirstItem) { // Relative not valid for first item
							isRelative = false;
						}

						// Radii X and Y
						SVGUtils.SkipWhiteSpace(data, ref index);
						token1 = SVGUtils.ExtractToken(data, ref index);
						SVGUtils.SkipWhiteSpace(data, ref index);
						token2 = SVGUtils.ExtractToken(data, ref index);

						// X axis rotation
						SVGUtils.SkipWhiteSpace(data, ref index);
						token3 = SVGUtils.ExtractToken(data, ref index);

						// Large arc flag
						SVGUtils.SkipWhiteSpace(data, ref index);
						token4 = SVGUtils.ExtractToken(data, ref index);

						// Sweep flag
						SVGUtils.SkipWhiteSpace(data, ref index);
						token5 = SVGUtils.ExtractToken(data, ref index);

						// X and Y
						SVGUtils.SkipWhiteSpace(data, ref index);
						token6 = SVGUtils.ExtractToken(data, ref index);
						SVGUtils.SkipWhiteSpace(data, ref index);
						token7 = SVGUtils.ExtractToken(data, ref index);

						// Start point
						pt0.X = curX;
						pt0.Y = curY;

						// Set our "current" coordinates to this
						if (isRelative) {
							curX = curX + float.Parse(token6, CultureInfo.InvariantCulture);
							curY = curY + float.Parse(token7, CultureInfo.InvariantCulture);
						} else {
							curX = float.Parse(token6, CultureInfo.InvariantCulture);
							curY = float.Parse(token7, CultureInfo.InvariantCulture);
						}

						pt1.X = curX;
						pt1.Y = curY;

						var points = SVGUtils.ParseArcSegment(
							float.Parse(token1, CultureInfo.InvariantCulture),
							float.Parse(token2, CultureInfo.InvariantCulture),
							float.Parse(token3, CultureInfo.InvariantCulture),
							pt0, pt1, (token4 == "1"), (token5 == "1")
						);
						currentContour.AddRange(points);
						//pData(currentLine).PathCode = pData(currentLine).PathCode + "Partial Arc to " + currX + ", " + currY + System.Environment.NewLine;

						if (!gotFirstItem) {
							startX = curX;
							startY = curY;
						}
						gotFirstItem = true;
						hasPrevPoint = false;

						break;
					case "C":
					case "c": // CURVE TO
						if (character.ToLower() == character) { // Lowercase means relative coordinates
							isRelative = true;
						}
						if (!gotFirstItem) { // Relative not valid for first item
							isRelative = false;
						}

						pt0.X = curX;
						pt0.Y = curY;

						// Extract two coordinates
						SVGUtils.SkipWhiteSpace(data, ref index);
						token1 = SVGUtils.ExtractToken(data, ref index);
						SVGUtils.SkipWhiteSpace(data, ref index);
						token2 = SVGUtils.ExtractToken(data, ref index);

						// Set into point 0
						pt1.X = (isRelative ? curX : 0) + float.Parse(token1, CultureInfo.InvariantCulture);
						pt1.Y = (isRelative ? curY : 0) + float.Parse(token2, CultureInfo.InvariantCulture);

						// Extract next two coordinates
						SVGUtils.SkipWhiteSpace(data, ref index);
						token1 = SVGUtils.ExtractToken(data, ref index);
						SVGUtils.SkipWhiteSpace(data, ref index);
						token2 = SVGUtils.ExtractToken(data, ref index);

						// Set into point 1
						pt2.X = (isRelative ? curX : 0) + float.Parse(token1, CultureInfo.InvariantCulture);
						pt2.Y = (isRelative ? curY : 0) + float.Parse(token2, CultureInfo.InvariantCulture);

						// Extract next two coordinates
						SVGUtils.SkipWhiteSpace(data, ref index);
						token1 = SVGUtils.ExtractToken(data, ref index);
						SVGUtils.SkipWhiteSpace(data, ref index);
						token2 = SVGUtils.ExtractToken(data, ref index);

						// Set into point 2
						curX = (isRelative ? curX : 0) + float.Parse(token1, CultureInfo.InvariantCulture);
						curY = (isRelative ? curY : 0) + float.Parse(token2, CultureInfo.InvariantCulture);
						pt3.X = curX;
						pt3.Y = curY;

						pInSegments = SVGUtils.GetPinSegments(pt0, pt3, globalDPI);

						// Run the bezier code
						currentContour.AddRange(Bezier.AddBezier((float)pInSegments, pt0, pt1, pt2, pt3));
						
						// Reflect this point
						prevPoint = SVGUtils.ReflectAbout(pt2, pt3);
						hasPrevPoint = true;

						//pData(currentLine).PathCode = pData(currentLine).PathCode + "Bezier to " + currX + ", " + currY + System.Environment.NewLine;

						if (!gotFirstItem) {
							startX = curX;
							startY = curY;
						}
						gotFirstItem = true;

						break;
					case "S":
					case "s": // CURVE TO with 3 points
						if (character.ToLower() == character) { // Lowercase means relative coordinates
							isRelative = true;
						}
						if (!gotFirstItem) { // Relative not valid for first item
							isRelative = false;
						}

						pt0.X = curX;
						pt0.Y = curY;

						// Extract two coordinates
						SVGUtils.SkipWhiteSpace(data, ref index);
						token1 = SVGUtils.ExtractToken(data, ref index);
						SVGUtils.SkipWhiteSpace(data, ref index);
						token2 = SVGUtils.ExtractToken(data, ref index);

						// Set into point 0
						pt1.X = (isRelative ? curX : 0) + float.Parse(token1, CultureInfo.InvariantCulture);
						pt1.Y = (isRelative ? curY : 0) + float.Parse(token2, CultureInfo.InvariantCulture);

						// Extract next two coordinates
						SVGUtils.SkipWhiteSpace(data, ref index);
						token1 = SVGUtils.ExtractToken(data, ref index);
						SVGUtils.SkipWhiteSpace(data, ref index);
						token2 = SVGUtils.ExtractToken(data, ref index);

						// Set into point 1
						curX = (isRelative ? curX : 0) + float.Parse(token1, CultureInfo.InvariantCulture);
						curY = (isRelative ? curY : 0) + float.Parse(token2, CultureInfo.InvariantCulture);
						pt2.X = curX;
						pt2.Y = curY;

						pInSegments = SVGUtils.GetPinSegments(pt0, pt2, globalDPI);

						if (!hasPrevPoint) {
							// Same as pt1
							prevPoint = pt1;
						}

						// Run the bezier code
						currentContour.AddRange(Bezier.AddBezier((float)pInSegments, pt0, prevPoint, pt1, pt2));

						// Reflect this point
						prevPoint = SVGUtils.ReflectAbout(pt1, pt2);
						hasPrevPoint = true;

						//pData(currentLine).PathCode = pData(currentLine).PathCode + "3Bezier to " + currX + ", " + currY + System.Environment.NewLine;

						if (!gotFirstItem) {
							startX = curX;
							startY = curY;
						}
						gotFirstItem = true;

						break;
					case "Q":
					case "q": // Quadratic Bezier TO with 3 points
						if (character.ToLower() == character) { // Lowercase means relative coordinates
							isRelative = true;
						}
						if (!gotFirstItem) { // Relative not valid for first item
							isRelative = false;
						}

						pt0.X = curX;
						pt0.Y = curY;

						// Extract two coordinates
						SVGUtils.SkipWhiteSpace(data, ref index);
						token1 = SVGUtils.ExtractToken(data, ref index);
						SVGUtils.SkipWhiteSpace(data, ref index);
						token2 = SVGUtils.ExtractToken(data, ref index);

						// Set into point 0
						pt1.X = (isRelative ? curX : 0) + float.Parse(token1, CultureInfo.InvariantCulture);
						pt1.Y = (isRelative ? curY : 0) + float.Parse(token2, CultureInfo.InvariantCulture);

						// Extract next two coordinates
						SVGUtils.SkipWhiteSpace(data, ref index);
						token1 = SVGUtils.ExtractToken(data, ref index);
						SVGUtils.SkipWhiteSpace(data, ref index);
						token2 = SVGUtils.ExtractToken(data, ref index);

						// Set into point 1
						curX = (isRelative ? curX : 0) + float.Parse(token1, CultureInfo.InvariantCulture);
						curY = (isRelative ? curY : 0) + float.Parse(token2, CultureInfo.InvariantCulture);
						pt2.X = curX;
						pt2.Y = curY;

						pInSegments = SVGUtils.GetPinSegments(pt0, pt2, globalDPI);

						// Run the bezier code
						currentContour.AddRange(Bezier.AddQuadBezier((float)pInSegments, pt0, pt1, pt2));
						
						// Reflect this point
						prevPoint = SVGUtils.ReflectAbout(pt1, pt2);

						hasPrevPoint = true;

						//pData(currentLine).PathCode = pData(currentLine).PathCode + "3Bezier to " + currX + ", " + currY + System.Environment.NewLine;

						if (!gotFirstItem) {
							startX = curX;
							startY = curY;
						}
						gotFirstItem = true;

						break;
					case "T":
					case "t": // Quadratic Bezier TO with 3 points, but use reflection of last
						if (character.ToLower() == character) { // Lowercase means relative coordinates
							isRelative = true;
						}
						if (!gotFirstItem) { // Relative not valid for first item
							isRelative = false;
						}

						pt0.X = curX;
						pt0.Y = curY;

						// Extract two coordinates
						SVGUtils.SkipWhiteSpace(data, ref index);
						token1 = SVGUtils.ExtractToken(data, ref index);
						SVGUtils.SkipWhiteSpace(data, ref index);
						token2 = SVGUtils.ExtractToken(data, ref index);

						// Set into point 0
						pt1.X = (isRelative ? curX : 0) + float.Parse(token1, CultureInfo.InvariantCulture);
						pt1.Y = (isRelative ? curY : 0) + float.Parse(token2, CultureInfo.InvariantCulture);

						pInSegments = SVGUtils.GetPinSegments(pt0, pt2, globalDPI);

						if (!hasPrevPoint) {
							// Same as pt1
							prevPoint = pt0; // SHOULD NEVER HAPPEN
						}

						// Run the bezier code
						currentContour.AddRange(Bezier.AddQuadBezier((float)pInSegments, pt0, prevPoint, pt1));
						
						// Reflect this point
						prevPoint = SVGUtils.ReflectAbout(prevPoint, pt1);
						hasPrevPoint = true;

						//pData(currentLine).PathCode = pData(currentLine).PathCode + "3Bezier to " + currX + ", " + currY + System.Environment.NewLine;

						if (!gotFirstItem) {
							startX = curX;
							startY = curY;
						}
						gotFirstItem = true;

						break;
					case "Z":
					case "z":
						hasPrevPoint = false;

						// z means end the shape
						// Draw a line back to start of shape
						//addPoint startX, startY;
						curX = startX;
						curY = startY;

						// Since this is a closed path, mark it as fillable.
						//pData(currentLine).Fillable = true;

						//pData(currentLine).PathCode = pData(currentLine).PathCode + "End Shape" + System.Environment.NewLine;

						// Close current contour and open a new one
						currentContour.Add(currentContour.First());
						currentContour = Transform(currentContour);
						contours.Add(currentContour);
						_path.AddPolygon(currentContour.ToArray());
						currentContour = new List<PointF>();
						
						break;
					default:
						Debug.WriteLine("UNSUPPORTED PATH CODE: {0}", character);
						break;
				}
			}
			
			if (currentContour.Count > 0)
			{
				if (currentContour.Count <= 2)
				{
					// Happens sometimes. This is either a point or
					// a line. Empty area, so just toss it.
				}
				else
				{
					currentContour.Add(currentContour.First());
					currentContour = Transform(currentContour);
					contours.Add(currentContour);
					_path.AddPolygon(currentContour.ToArray());
				}
			}
		}

		PointF PreviousPoint()
		{
			if (currentContour.Count > 0)
			{
				return currentContour.Last();
			}
			if (contours.Count > 0)
			{
				return contours.Last().Last();
			}
			return new PointF(0, 0);
		}

		void AddBezierPoints(float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x3, float y3)
		{
			var pointList = new List<PointF>();

			// First subdivide the Bezier into 250 line segments. This number is fairly arbitrary
			// and anything we pick is wrong because for small curves you'll have multiple segments
			// smaller than a pixel and for huge curves no number is large enough. We pick something
			// fairly big and then the polygon gets thinned in two separate stages afterwards. Many
			// of these get reduced to just a handful of vertices by the time we emit GCode.
			pointList.Add(new PointF(x1, y1));
			
			float stepDelta = 1.0f / 250.0f;
			for (float t = stepDelta; t < 1.0f; t += stepDelta) // Parametric value
			{
				float fX = Bezier.Cubic(t, x1, cx1, cx2, x3);
				float fY = Bezier.Cubic(t, y1, cy1, cy2, y3);
				
				pointList.Add(new PointF(fX, fY));
			}
			pointList.Add(new PointF(x3, y3));

			// Next thin the points based on a flatness test.
			bool done = true;
			do
			{
				done = true;
				int pointIndex = 0;
				do
				{
					PointF p1 = pointList[pointIndex];
					PointF p2 = pointList[pointIndex + 1];
					PointF p3 = pointList[pointIndex + 2];
					
					var pb = new PointF((p1.X + p3.X) / 2, (p1.Y + p3.Y) / 2);

					double err = Math.Sqrt(Math.Abs(p2.X - pb.X) * Math.Abs(p2.X - pb.X) +
					                       Math.Abs(p2.Y - pb.Y) * Math.Abs(p2.Y - pb.Y));
					double dist = Math.Sqrt(Math.Abs(p3.X - p1.X) * Math.Abs(p3.X - p1.X) +
					                        Math.Abs(p3.Y - p1.Y) * Math.Abs(p3.Y - p1.Y));
					double relativeErr = err / dist;

					// If the subdivided portion is within a pixel at 1000dpi
					// then it's flat enough to remove the intermediate vertex.
					if (relativeErr < 0.001)
					{
						pointList.RemoveAt(pointIndex + 1);
						done = false;
					}

					++pointIndex;
				} while (pointIndex < pointList.Count - 2);
			} while (!done);

			foreach (PointF point in pointList)
			{
				currentContour.Add(point);
			}
		}
		
		public List<List<PointF>> GetContours()
		{
			return contours;
		}
	}

	/// <summary>
	/// Styles contain colors, stroke widths, etc. We use them to differentiate
	/// vector vs. raster portions of the document.
	/// </summary>
	public class SVGStyle
	{
		public string Name { get; set; }

		public bool OutlineWidthPresent { get; set; }
		public double OutlineWidth { get; set; }

		public bool OutlineColorPresent { get; set; }
		public Color OutlineColor { get; set; }

		public bool FillColorPresent { get; set; }
		public Color FillColor { get; set; }

		static public Color ParseColor(string c)
		{
			Color result;

			if ( c.Length == 7 && c[0] == '#' )
			{
				string s1 = c.Substring(1, 2);
				string s2 = c.Substring(3, 2);
				string s3 = c.Substring(5, 2);

				byte r = 0;
				byte g = 0;
				byte b = 0;

				try
				{
					r = Convert.ToByte(s1, 16);
					g = Convert.ToByte(s2, 16);
					b = Convert.ToByte(s3, 16);
				}
				catch
				{
				}

				result = Color.FromArgb(r, g, b);
			}
			else
			{
				result = Color.FromName(c);
			}

			return result;

		}

		public SVGStyle(string name, string style)
		{
			Name = name;
			OutlineColorPresent = false;
			OutlineWidthPresent = false;
			FillColorPresent = false;

			style = style.Trim(new [] { '{', '}' });
			string[] stylePairs = style.Split(new [] { ':', ';' }, StringSplitOptions.RemoveEmptyEntries);

			// check we have pairs (can divide by 2)
			if ((stylePairs.Count() & 1) != 0)
			{
				throw new ArgumentException("Failed to parse style");
			}

			for (int index=0; index<stylePairs.Count(); index += 2)
			{
				switch (stylePairs[index])
				{
					case "stroke":
						OutlineColor = ParseColor(stylePairs[index+1]);
						OutlineColorPresent = true;
						break;
					case "stroke-width":
						OutlineWidth = double.Parse(stylePairs[index+1], CultureInfo.InvariantCulture);
						OutlineWidthPresent = true;
						break;
					case "fill":
						FillColor = ParseColor(stylePairs[index+1]);
						FillColorPresent = true;
						break;
						default: break;
				}
			}
		}

	}

	/// <summary>
	/// An SVG Document. Read from file and build in-memory representation.
	/// </summary>
	public class SVGDocument
	{
		List<ISVGElement> shapes = new List<ISVGElement>();
		float GLOBAL_DPI = 1.0f; // mm?

		public List<ISVGElement> Shapes {
			get {
				return shapes;
			}
		}

		public static SVGDocument LoadFromFile(string path)
		{
			DateTime start = DateTime.UtcNow;

			// Here begins the reading of the SVG file
			var reader = new XmlTextReader(path);
			var doc = new SVGDocument();
			var styleDictionary = new Dictionary<string, SVGStyle>();

			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.Name == "style")
					{
						// Inline style
						string styleData = reader.ReadElementContentAsString();
						var styleReader = new StringReader(styleData);
						string line;
						
						while ((line = styleReader.ReadLine()) != null)
						{
							string[] splitLine;

							line = line.Trim();
							if (line == "") continue;
							
							splitLine = line.Split(new [] { ' ', '\t', '{', '}' }, StringSplitOptions.RemoveEmptyEntries);

							string name = splitLine[0];
							if (name.StartsWith("."))
							{
								name = name.Substring(1);
							}
							if (splitLine.Count() == 2)
							{
								styleDictionary.Add(name, new SVGStyle(name, splitLine[1]));
							}

						};
					}
					else if (reader.Name == "svg")
					{
						string widthValue = reader.GetAttribute("width");
						string heightValue = reader.GetAttribute("height");
						string viewBoxValue = reader.GetAttribute("viewBox");
						doc.SetDPIFromSVGSizeParameters(widthValue, heightValue, viewBoxValue);
					}
					else if (reader.Name == "g") {
						// <g id="g3023" transform="translate(269.81467,-650.62904)">
						// <g id="Layer_x0020_1">
						string layerName = "";
						
						string inkscapeLabel = reader.GetAttribute("inkscape:label");
						if (inkscapeLabel != null) {
							layerName = inkscapeLabel;
						} else {
							string idValue = reader.GetAttribute("id");
							if (idValue != null) {
								if (idValue.ToLower().StartsWith("layer")) {
									layerName = idValue;
								}
							}
						}

						string transformValue = reader.GetAttribute("transform");
						// TODO: use the transform value for something!
					}
					else if (reader.Name == "rect")
					{
						doc.AddShape(new SVGRect(reader, styleDictionary));
					}
					else if (reader.Name == "path")
					{
						doc.AddShape(new SVGPath(reader, styleDictionary, doc.GLOBAL_DPI));
					}
					else if (reader.Name == "line")
					{
						doc.AddShape(new SVGLine(reader, styleDictionary));
					}
					else if (reader.Name == "polyline")
					{
						doc.AddShape(new SVGPolygon(reader, styleDictionary));
					}
					else if (reader.Name == "polygon")
					{
						doc.AddShape(new SVGPolygon(reader, styleDictionary));
					}
					else if (reader.Name == "circle")
					{
						doc.AddShape(new SVGCircle(reader, styleDictionary));
					}
					else if (reader.Name == "ellipse")
					{
						doc.AddShape(new SVGEllipse(reader, styleDictionary));
					}
					else if (reader.Name == "image")
					{
						doc.AddShape(new SVGImage(reader, styleDictionary, path));
					}
				}
			}

			TimeSpan duration = DateTime.UtcNow - start;
			Debug.WriteLine("### Load took {0}s", ((double)duration.TotalMilliseconds / 1000.0));

			return doc;
		}

		public void AddShape(ISVGElement shape)
		{
			shapes.Add(shape);
		}

		public IEnumerable<IEnumerable<PointF>> GetContours()
		{
			// Enumerate each shape in the document
			foreach (ISVGElement shape in shapes)
			{
				foreach (var contour in shape.GetContours())
				{
					yield return contour;
				}
			}
		}
		
		/// <summary>
		/// Get the points for all contours in all shapes.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<PointF> GetPoints()
		{
			// Enumerate each shape in the document
			foreach (ISVGElement shape in shapes)
			{
				foreach (var contour in shape.GetContours())
				{
					foreach (PointF point in contour)
					{
						yield return point;
					}
				}
			}
		}
		
		/// <summary>
		/// Scale the contours using the correct DPI,
		/// as well as the min x and min y coordinates.
		/// This fixes the issue where coordinates are negative
		/// </summary>
		/// <returns>a scaled contour list</returns>
		public IEnumerable<IEnumerable<PointF>> ScaledContours() {
			
			var contours = new List<List<PointF>>();
			
			// Calculate the extents for all contours
			var points = GetPoints();
			
			if (points.Count() == 0) return contours;
			
			float minX = points.Min(point => point.X);
			float minY = points.Min(point => point.Y);

			// Scale by the DPI
			// and
			// Fix the points by removing space at the left and top
			foreach (ISVGElement shape in shapes)
			{
				foreach (var contour in shape.GetContours())
				{
					var scaledPoints = new List<PointF>();
					foreach (PointF point in contour)
					{
						var scaledPoint = new PointF((point.X - minX)/GLOBAL_DPI, (point.Y - minY)/GLOBAL_DPI);
						scaledPoints.Add(scaledPoint);
					}
					contours.Add(scaledPoints);
				}
			}
			return contours;
		}

		static void ParseNumberWithOptionalUnit(string stringWithOptionalUnit, out float number, out string unit) {

			// Read numbers with optional units like this:
			// width="8.5in"
			// height="11in"
			
			var match = Regex.Match(stringWithOptionalUnit, @"([0-9]+(?:\.[0-9]+)?)(\w*)");
			if (match.Success) {
				string numberString = match.Groups[1].Value;
				string unitString = match.Groups[2].Value;
				
				number = float.Parse(numberString, CultureInfo.InvariantCulture);
				unit = unitString;
			} else {
				number = -1;
				unit = "FAIL";
			}
		}

		static float ScaleValueWithUnit(float value, string unit) {
			
			// Read the unit
			// default is mm
			switch (unit.ToLower())
			{
				case "in": // convert to mm
					value *= 25.4f;
					break;
				case "mm": // no conversion needed
				case "":
					break;
				case "cm": // convert from cm
					value *= 10.0f;
					break;
				case "pt": // 1 point = 1/72 in
					value = value * 25.4f / 72f;
					break;
				case "pc": // 1 pica = 1/6 in
					value = value * 25.4f / 6f;
					break;
			}
			
			return value;
		}
		
		void SetDPIFromSVGSizeParameters(string widthValue, string heightValue, string viewboxValue)
		{
			float realW = 0;
			float realH = 0;
			float realDPI = 0;

			// width="8.5in"
			// height="11in"
			// viewBox="0 0 765.00001 990.00002"

			// Read these numbers to determine the scale of the data inside the file.
			// width and height are the real-world widths and heights
			// viewbox is how we're going to scale the numbers in the file (expressed in pixels) to the native units of this program, which is mm
			
			// if we don't have a width value, we cannot set the DPI automatically
			if (widthValue == null) return;
			
			string widthUnit;
			ParseNumberWithOptionalUnit(widthValue, out realW, out widthUnit);
			realW = ScaleValueWithUnit(realW, widthUnit);
			
			string heightUnit;
			ParseNumberWithOptionalUnit(heightValue, out realH, out heightUnit);
			realH = ScaleValueWithUnit(realH, heightUnit);

			Debug.WriteLine("Size in mm: {0}, {1}", realW, realH);
			
			// The 'ViewBox' is how we scale an mm to a pixel.
			// The default is 90dpi but it may not be.
			if (viewboxValue != null) {
				var viewBoxArgs = viewboxValue.Split(' ').Where(t => !string.IsNullOrEmpty(t));
				float[] viewBoxFloatArgs = viewBoxArgs.Select(arg => float.Parse(arg, CultureInfo.InvariantCulture)).ToArray();
				
				if (viewBoxArgs.Count() == 4)
				{
					// Get the width in pixels
					if (realW == 0) {
						realDPI = 25.4f;
					} else {
						realDPI = viewBoxFloatArgs[2] / realW;
					}
				}

				// set the Global DPI variable
				GLOBAL_DPI = realDPI;
			}
		}
		
		public void Render(Graphics gc, bool rasterOnly)
		{
			foreach (ISVGElement shape in shapes)
			{
				if (shape is SVGImage)
				{
					// Polymorphism? What's that?
					var img = shape as SVGImage;

					gc.DrawImage(img.bits, img.DestBounds);
				}

				if (shape.OutlineWidth < .01 && shape.FillColor.A == 0 && rasterOnly)
				{
					continue;
				}

				GraphicsPath p = shape.GetPath();
				if (shape.FillColor.A > 0)
				{
					Brush b = new SolidBrush(shape.FillColor);
					gc.FillPath(b, p);
					b.Dispose();
				}
				if (shape.OutlineWidth > 0 && shape.OutlineColor.A > 0)
				{
					var pen = new Pen(shape.OutlineColor, (float)shape.OutlineWidth);
					gc.DrawPath(pen, p);
					pen.Dispose();
				}
			}
		}
		
		string GCodeHeader =
			@"(paperpixels SVG to GCode v0.1)
N30 G17 (active plane)
N35 G40 (turn compensation off)
N40 G20 (inch mode)
N45 G90 (Absolute mode, current coordinates)
N50 G61 (Exact stop mode for raster scanning)";

		string GCodeFooter =
			@"M5 (Laser Off)
E1P0 (Program End)
G0 X0 Y0 Z1 (Home and really turn off laser)
M30 (End)";

		/// <summary>
		/// This emits GCode suitable for driving a laser cutter to vector/raster
		/// the document. There are numerous assumptions made here so that it
		/// works exactly with my cheap Chinese laser cutter, my controller board
		/// and my Mach3 config. You may need to fiddle with things here to
		/// get the axes directions correct, etc.
		/// </summary>
		/// <param name="path">Output path for GCode file</param>
		/// <param name="raster">If true a raster path is emitted</param>
		/// <param name="rasterDpi">DPI resolution for raster</param>
		/// <param name="rasterFeedRate">IPS feed for raster scan</param>
		/// <param name="vector">If true a vector cut is emitted</param>
		/// <param name="vectorDpi">DPI resolution for vector cut</param>
		/// <param name="vectorFeedRate">IPS feed for vector cut</param>
		/// <param name="vectorCV">Use constant velocity mode? Smooths out
		/// discontinuities that occur at polygon vertices/corners using
		/// lookahead. Mach3 does all this, this simply emits a GCode to
		/// turn this on. This, plus sufficient lookahead configured in
		/// Mach3 made my laser perform much smoother.</param>
		/// <param name="progressDelegate">Callback for progress notification</param>
		public string EmitGCode(bool raster, int rasterDpi, int rasterFeedRate, bool vector, int vectorDpi, int vectorFeedRate, bool vectorCV, Action<double> progressDelegate)
		{
			// Open up file for writing
			var gcode = new StringWriter();

			// Emit header
			gcode.WriteLine(GCodeHeader);

			// BUGBUG: Should read size from SVG file header. But we usually are doing 11x11
			//         This always assumes an 11" x 11" document.
			double docWidth = 11.0;
			double docHeight = 11.0;
			double bandHeight = 0.5;
			double totalProgress = docHeight / bandHeight + 1.0;
			double progress = 0;

			// First pass is raster engraving
			if (raster)
			{
				// Create a bitmap image used for banding
				var band = new Bitmap(
					(int)(docWidth * rasterDpi),    // Band Width in px
					(int)(bandHeight * rasterDpi),  // Band Height in px
					PixelFormat.Format32bppArgb);
				Graphics gc = Graphics.FromImage(band);

				// Now render each band of the image
				for (double bandTop = 0.0; bandTop <= docHeight - bandHeight; bandTop += bandHeight)
				{
					// Call progress method once per band
					if (progressDelegate != null)
					{
						progressDelegate(progress / totalProgress);
					}

					// Set up the GC transform to render this band
					gc.ResetTransform();
					gc.FillRectangle(Brushes.White, 0, 0, 99999, 99999);
					gc.ScaleTransform(-rasterDpi, -rasterDpi);
					gc.TranslateTransform((float)-docWidth, (float)-bandTop);

					// Erase whatever was there before
					gc.FillRectangle(Brushes.White, -50, -50, 50, 50);

					// Render just the raster shapes into the band
					this.Render(gc, true);

					// Now scan the band and emit gcode. We access the bitmap data
					// directly for higher performance. The use of unsafe pointer
					// access here sped up perf significantly over GetPixel() which
					// is obvious but they don't teach PhD's this so I mention it here.
					unsafe
					{
						BitmapData lockedBits = band.LockBits(
							Rectangle.FromLTRB(0, 0, band.Width, band.Height),
							ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
						bool laserOn = false;
						int onStart = 0;
						for (int y = 0; y < band.Height; ++y)
						{
							if (laserOn)
							{
								throw new ApplicationException("Expected laser off");
							}

							// Get the bits for this scanline using something I call a "pointer"
							byte* pScanline = (byte*)((int)lockedBits.Scan0 + ((band.Height - 1) - y) * lockedBits.Stride);
							for (int x = 0; x < band.Width; ++x)
							{
								int b = *pScanline++;
								int g = *pScanline++;
								int r = *pScanline++;
								int a = *pScanline++;
								int luma = r + g + b;

								if (luma < 400)
								{
									if (!laserOn)
									{
										// Found an "on" edge
										onStart = x;
										laserOn = true;
									}
								}
								else
								{
									if (laserOn)
									{
										// Found an "off" edge
										double fx = (double)onStart / (double)rasterDpi;
										double fy = ((double)y / (double)rasterDpi) + bandTop;
										fy = docHeight - fy + bandHeight;
										fx = docWidth - fx;
										gcode.WriteLine(string.Format("G1 X{0:0.0000} Y{1:0.0000} F{2}", fx, fy, rasterFeedRate));
										fx = (double)x / (double)rasterDpi;
										fx = docWidth - fx;
										gcode.WriteLine("G1 Z0 (Laser On)");
										gcode.WriteLine(string.Format("X{0:0.0000}", fx));
										gcode.WriteLine("Z0.002 (Laser Off)");

										laserOn = false;
									}
								}
							}

							// If we get here and laser is still on then we
							// turn it off at the edge here.
							if (laserOn && false)
							{
								double fx = (double)onStart / (double)rasterDpi;
								double fy = ((double)y / (double)rasterDpi) + bandTop;
								gcode.WriteLine(string.Format("G1 X{0:0.0000} Y{1:0.0000}", fx, fy));
								fx = (double)band.Width / (double)rasterDpi;
								fx = docWidth - fx;
								gcode.WriteLine("G1 Z0 (Laser On)");
								gcode.WriteLine(string.Format("X{0:0.0000} F{1:0.0000}", fx, rasterFeedRate));
								gcode.WriteLine("Z0.002 (Laser Off)");

								laserOn = false;
							}
						}

						// Unlock band bits
						band.UnlockBits(lockedBits);

						// Increment progress
						progress += 1.0;
					}
				}
			}

			// Pause inbetween for the operator to adjust power
			// You need to create a M995 custom macro in Mach3 to
			// stick up a dialog that says "Adjust Power for Vector"
			gcode.WriteLine("(=================================================================================)");
			gcode.WriteLine("(Pause for operator power adjustment)");
			gcode.WriteLine("(Depends on macro M995 set up to prompt operator)");
			gcode.WriteLine("(=================================================================================)");
			gcode.WriteLine("M995");

			// Second pass is vector cuts
			double contourIncrement = 1.0 / GetVectorContours(100).Count();
			if (vectorCV)
			{
				gcode.WriteLine("G64 (Constant velocity mode for vector cuts)");
			}
			foreach (var contour in GetVectorContours(vectorDpi))
			{
				int contourFeed;

				if (vectorFeedRate > 0)
				{
					contourFeed = vectorFeedRate;
				}
				else
				{
					// This is trying to be overly cute and is probably
					// not useful. It maps colors to different speeds.
					contourFeed = (int)((1.0 - contour.Brightness) * 1000);
					if (contour.Color.Name == "Blue")
					{
						contourFeed = 50;
					}
					else if (contour.Color.Name == "Aqua")
					{
						contourFeed = 40;
					}
					else if (contour.Color.Name == "Lime")
					{
						contourFeed = 30;
					}
					else if (contour.Color.Name == "Yellow")
					{
						contourFeed = 20;
					}
					else if (contour.Color.Name == "Red")
					{
						contourFeed = 10;
					}
				}

				bool first = true;
				foreach (var point in contour.Points)
				{
					// Transform point to laser coordinate system
					double laserX = point.X;
					double laserY = 11.0 - point.Y;

					if (first)
					{
						// Rapid to the start of the contour
						gcode.WriteLine(string.Format("G0 X{0:0.0000} Y{1:0.0000}", laserX, laserY));
						gcode.WriteLine(string.Format("G1 Z-0.002 F{0} (Turn on laser. Set feed for this contour)", contourFeed));
						first = false;
					}
					else
					{
						// Next point in contour
						gcode.WriteLine(string.Format("X{0:0.0000} Y{1:0.0000}", laserX, laserY));
					}
				}
				gcode.WriteLine("Z0 (Turn off laser)");

				progress += contourIncrement;
				if (progressDelegate != null)
				{
					progressDelegate(progress / totalProgress);
				}
			}

			// Shut down
			gcode.WriteLine(GCodeFooter);
			gcode.Flush();
			gcode.Close();
			
			return gcode.ToString();
		}
		
		/// <summary>
		/// Given a specific DPI resolution, returns vectors to approximate
		/// the shapes in the document at that resolution. This allows us to
		/// thin out the tons of polygon edges for curved segments.
		/// </summary>
		/// <param name="dpi"></param>
		/// <returns></returns>
		public IEnumerable<VectorContour> GetVectorContours(double dpi)
		{
			double threshhold = 1.0/dpi;
			int total = 0;
			int thin = 0;

			foreach (ISVGElement shape in shapes)
			{
				if ((shape.OutlineWidth >= .01 && shape.OutlineColor.A == 255) || shape.FillColor.A == 255)
				{
					continue;
				}

				foreach (var contour in shape.GetContours())
				{
					var thinnedContour = new List<PointF>();
					PointF lastPoint = contour.First();
					bool first = true;

					foreach (PointF point in contour)
					{
						++total;

						if (first)
						{
							thinnedContour.Add(new PointF(point.X, point.Y));
							lastPoint = point;
							first = false;
						}
						else
						{
							if (SVGUtils.Distance(point, lastPoint) > threshhold)
							{
								++thin;
								thinnedContour.Add(point);
								lastPoint = point;
							}
						}
					}

					yield return new VectorContour()
					{
						Brightness = shape.OutlineColor.GetBrightness(),
						Color = shape.OutlineColor,
						Points = thinnedContour
					};
				}
			}

			Debug.WriteLine("Thinned contour ({0}/{1}) = {2}%", thin, total, (int)((double)thin / (double)total * 100.0));
		}
		
	}
}
