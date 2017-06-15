// Most of this comes from the lasercam project made by Chris Yerga
// Modified by perivar@nerseth.com to support OpenScad SVGs
// Copyright (c) 2010 Chris Yerga
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
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;

using System.Text;
using System.Xml;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace SVG
{
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
				string[] styleNames = styleText.Split(new char[] { ' ', '\t' });

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
				xfs = xfs.Trim(new char[] { '(', ')' });
				string[] elements = xfs.Split(new char[] { ' ', '\t' });

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
	/// SVG Rectangle
	/// </summary>
	public class SVGRect : SVGShapeBase, ISVGElement
	{
		private List<PointF> points = new List<PointF>();

		public SVGRect(XmlTextReader reader, Dictionary<string, SVGStyle> styleDictionary)
			: base(reader, styleDictionary)
		{
			float x = 0;
			try
			{
				x = float.Parse(reader.GetAttribute("x"), CultureInfo.InvariantCulture);
			}
			catch (ArgumentNullException) { }

			float y = 0;
			try
			{
				y = float.Parse(reader.GetAttribute("y"), CultureInfo.InvariantCulture);
			}
			catch (ArgumentNullException) { }
			
			float w = float.Parse(reader.GetAttribute("width"), CultureInfo.InvariantCulture);
			float h = float.Parse(reader.GetAttribute("height"), CultureInfo.InvariantCulture);

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
			try
			{
				x = float.Parse(reader.GetAttribute("x"), CultureInfo.InvariantCulture);
			}
			catch { }

			try
			{
				y = float.Parse(reader.GetAttribute("y"), CultureInfo.InvariantCulture);
			}
			catch { }
			
			width = float.Parse(reader.GetAttribute("width"), CultureInfo.InvariantCulture);
			height = float.Parse(reader.GetAttribute("height"), CultureInfo.InvariantCulture);
			string path = reader.GetAttribute("xlink:href");

			string dir = Path.GetDirectoryName(baseDocPath);
			string bitspath = Path.Combine(dir, path);
			bits = Image.FromFile(bitspath);

			var pts = new PointF[2];
			pts[0].X = x;
			pts[0].Y = y;
			pts[1].X = x+width;
			pts[1].Y = y+height;
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
		private List<PointF> points = new List<PointF>();

		public SVGCircle(XmlTextReader reader, Dictionary<string, SVGStyle> styleDictionary)
			: base(reader, styleDictionary)
		{
			float cx = 0;
			try
			{
				cx = float.Parse(reader.GetAttribute("cx"), CultureInfo.InvariantCulture);
			}
			catch { }

			float cy = 0;
			try
			{
				cy = float.Parse(reader.GetAttribute("cy"), CultureInfo.InvariantCulture);
			}
			catch { }
			
			float r = float.Parse(reader.GetAttribute("r"), CultureInfo.InvariantCulture);

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
		private List<List<PointF>> contours = new List<List<PointF>>();
		private List<PointF> currentContour = new List<PointF>();

		public SVGPolygon(XmlTextReader reader, Dictionary<string, SVGStyle> styleDictionary)
			: base(reader, styleDictionary)
		{
			string data = reader.GetAttribute("points");
			string[] textPoints = data.Split(new char[] { ' ', '\t' });

			foreach (string textPoint in textPoints)
			{
				string[] ordinates = textPoint.Split(new char[] { ',' });
				if (ordinates.Length > 1)
				{
					currentContour.Add(new PointF(float.Parse(ordinates[0], CultureInfo.InvariantCulture), float.Parse(ordinates[1], CultureInfo.InvariantCulture)));
				}
			}

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
		private List<List<PointF>> contours = new List<List<PointF>>();
		private List<PointF> currentContour = new List<PointF>();

		enum ParseState
		{
			None,
			MoveToAbs,
			MoveToRel,
			CurveToAbs,
			CurveToRel,
			LineToAbs,
			LineToRel
		};
		ParseState state = ParseState.None;

		public float GetFloat(string data, ref int index)
		{
			var builder = new StringBuilder();
			bool isnum = true;

			while (isnum)
			{
				if (index >= data.Length)
				{
					isnum = false;
				}
				else
				{
					switch (data[index])
					{
							case '0': break;
							case '1': break;
							case '2': break;
							case '3': break;
							case '4': break;
							case '5': break;
							case '6': break;
							case '7': break;
							case '8': break;
							case '9': break;
							case '-': break;
							case '.': break;
							case 'e': break;
							case '+': break;

							default: isnum = false; break;
					}
				}

				if (isnum)
				{
					builder.Append(data[index]);
					++index;
				}
			}

			return float.Parse(builder.ToString(), CultureInfo.InvariantCulture);
		}

		public SVGPath(XmlTextReader reader, Dictionary<string, SVGStyle> styleDictionary)
			: base(reader, styleDictionary)
		{
			_path = new GraphicsPath();

			string data = reader.GetAttribute("d");
			if (data == null)
			{
				return;
			}

			int index = 0;
			bool done = false;

			while (index < data.Length)
			{
				SkipSpace(data, ref index);
				if (index == data.Length) break; // handle if very last character is a non ascii character

				char command = data[index];
				switch (command)
				{
						case 'M': state = ParseState.MoveToAbs; ++index; break;
						case 'm': state = ParseState.MoveToRel; ++index; break;
						case 'c': state = ParseState.CurveToRel; ++index; break;
						case 'l': state = ParseState.LineToRel; ++index; break;
						case 'L': state = ParseState.LineToAbs; ++index; break;
						case 'Z':
						case 'z': state = ParseState.None; ++index;
						
						// Close current contour and open a new one
						currentContour.Add(currentContour.First());
						currentContour = Transform(currentContour);
						contours.Add(currentContour);
						_path.AddPolygon(currentContour.ToArray());
						currentContour = new List<PointF>();
						continue;

						case '0': break;
						case '1': break;
						case '2': break;
						case '3': break;
						case '4': break;
						case '5': break;
						case '6': break;
						case '7': break;
						case '8': break;
						case '9': break;
						case '-': break;
						case ' ': break;

						default: throw new ApplicationException(string.Format("Unexpected input {0}", data[index]));
				}

				if (done)
				{
					break;
				}

				SkipSpace(data, ref index);

				if (state == ParseState.MoveToAbs)
				{
					float x = GetFloat(data, ref index);
					SkipSpaceOrComma(data, ref index);
					float y = GetFloat(data, ref index);

					currentContour.Add(new PointF(x, y));
				}
				else if (state == ParseState.MoveToRel)
				{
					float x = GetFloat(data, ref index);
					SkipSpaceOrComma(data, ref index);
					float y = GetFloat(data, ref index);

					currentContour.Add(new PointF(PreviousPoint().X + x, PreviousPoint().Y + y));
				}
				else if (state == ParseState.LineToRel)
				{
					float x = GetFloat(data, ref index);
					SkipSpaceOrComma(data, ref index);
					float y = GetFloat(data, ref index);

					currentContour.Add(new PointF(PreviousPoint().X + x, PreviousPoint().Y + y));
				}
				else if (state == ParseState.LineToAbs)
				{
					float x = GetFloat(data, ref index);
					SkipSpaceOrComma(data, ref index);
					float y = GetFloat(data, ref index);

					currentContour.Add(new PointF(x, y));
				}
				else if (state == ParseState.CurveToRel)
				{
					float cx1 = GetFloat(data, ref index);
					if (data[index] != ',')
					{
						throw new ApplicationException("Expected comma");
					}
					++index;
					float cy1 = GetFloat(data, ref index);

					if (data[index] != ' ')
					{
						throw new ApplicationException("Expected space");
					}
					++index;

					float cx2 = GetFloat(data, ref index);
					if (data[index] != ',')
					{
						throw new ApplicationException("Expected comma");
					}
					++index;
					float cy2 = GetFloat(data, ref index);

					if (data[index] != ' ')
					{
						throw new ApplicationException("Expected space");
					}
					++index;

					float x = GetFloat(data, ref index);
					if (data[index] != ',')
					{
						throw new ApplicationException("Expected comma");
					}
					++index;
					float y = GetFloat(data, ref index);

					float lx = PreviousPoint().X;
					float ly = PreviousPoint().Y;

					AddBezierPoints(lx, ly, lx + cx1, ly + cy1, lx + cx2, ly + cy2, lx + x, ly + y);
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

		private PointF PreviousPoint()
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

		private void AddBezierPoints(float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x3, float y3)
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
				float fW = 1 - t;
				float fA = fW * fW * fW;
				float fB = 3 * t * fW * fW;
				float fC = 3 * t * t * fW;
				float fD = t * t * t;

				float fX = fA * x1 + fB * cx1 + fC * cx2 + fD * x3;
				float fY = fA * y1 + fB * cy1 + fC * cy2 + fD * y3;

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

		public void SkipSpace(string data, ref int index)
		{
			// make sure that we don't skip past the string data length
			while (index < data.Length && char.IsWhiteSpace(data[index]))
			{
				++index;
			}
		}

		public void SkipSpaceOrComma(string data, ref int index)
		{
			// make sure that we don't skip past the string data length
			while (index < data.Length && char.IsWhiteSpace(data[index]) || data[index] == ',')
			{
				++index;
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

			style = style.Trim(new char[] { '{', '}' });
			string[] stylePairs = style.Split(new char[] { ':', ';' });

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
		private List<ISVGElement> shapes = new List<ISVGElement>();
		private float GLOBAL_DPI;

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
							splitLine = line.Split(new char[] { ' ', '\t' });

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
					else if (reader.Name == "rect")
					{
						if (reader.GetAttribute("class") != null)
						{
							doc.AddShape(new SVGRect(reader, styleDictionary));
						}
					}
					else if (reader.Name == "circle")
					{
						doc.AddShape(new SVGCircle(reader, styleDictionary));
					}
					else if (reader.Name == "polygon")
					{
						doc.AddShape(new SVGPolygon(reader, styleDictionary));
					}
					else if (reader.Name == "polyline")
					{
						doc.AddShape(new SVGPolygon(reader, styleDictionary));
					}
					else if (reader.Name == "path")
					{
						doc.AddShape(new SVGPath(reader, styleDictionary));
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
			
			string widthUnit;
			ParseNumberWithOptionalUnit(widthValue, out realW, out widthUnit);
			realW = ScaleValueWithUnit(realW, widthUnit);
			
			string heightUnit;
			ParseNumberWithOptionalUnit(heightValue, out realH, out heightUnit);
			realH = ScaleValueWithUnit(realH, heightUnit);

			Debug.WriteLine("Size in mm: {0}, {1}", realW, realH);
			
			// The 'ViewBox' is how we scale an mm to a pixel.
			// The default is 90dpi but it may not be.
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
							if (Distance(point, lastPoint) > threshhold)
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
		
		public static string GenerateGCode(IEnumerable<IEnumerable<PointF>>contours) {
			var sb = new StringBuilder();
			int contourCounter = 0;

			// Enumerate each contour in the document
			foreach (var contour in contours)
			{
				contourCounter++;
				
				sb.AppendFormat("Drill Contour Center {0}\n", contourCounter);
				
				var center = Center(contour);
				sb.AppendFormat(CultureInfo.InvariantCulture, "G0 X{0:0.##} Y{1:0.##}\n", center.X, center.Y);
				sb.AppendLine("G1 Z-1.5 F800");
				sb.AppendLine("G1 Z0 F800");
				sb.AppendLine("G1 Z-3 F800");
				sb.AppendLine("G1 Z0 F800");
				sb.AppendLine("G1 Z-4.5 F800");
				sb.AppendLine("G1 Z0 F800");
				sb.AppendLine("G1 Z-6 F800");
				sb.AppendLine("G1 Z0 F800");
				sb.AppendLine("G1 Z-7.5 F800");
				sb.AppendLine("G1 Z0 F800");
				sb.AppendLine("G1 Z-9 F800");
				sb.AppendLine("G1 Z0 F800");
				sb.AppendLine("G1 Z-10 F800");
				sb.AppendLine("G1 Z0 F800");
				sb.AppendLine("G0 Z2");
			}
			return sb.ToString();
		}
	}
}
