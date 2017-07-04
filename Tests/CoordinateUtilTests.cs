﻿using System;
using System.Drawing;
using NUnit.Framework;
using GCode;

namespace GCodeOptimizer.Tests
{
	[TestFixture]
	public class CoordinateUtilTests
	{
		[Test]
		public void TestDistance() {

			var A = new Point3D(-2, 1);
			var B = new Point3D(1, 5);
			
			var distance = Transformation.Distance(A, B);
			Assert.AreEqual(5d, distance, 0.00000001);
		}
		
		[Test]
		public void TestAreaOfTriangle1()
		{
			// http://www.mathopenref.com/coordtrianglearea.html
			
			var A = new Point3D(15, 15);
			var B = new Point3D(23, 30);
			var C = new Point3D(50, 25);
			var area = Transformation.AreaOfTriangle(A, B, C);
			var area2 = Transformation.AreaOfTriangleFast(A, B, C);
			
			Assert.AreEqual(222.5f, area, 0.00000001);
			Assert.AreEqual(222.5f, area2, 0.00000001);
		}
		
		[Test]
		public void TestAreaOfTriangle2()
		{
			// http://www.mathopenref.com/coordtrianglearea.html
			
			var A = new Point3D(-6, 20);
			var B = new Point3D(11, 39);
			var C = new Point3D(15, -6);
			var area = Transformation.AreaOfTriangle(A, B, C);
			var area2 = Transformation.AreaOfTriangleFast(A, B, C);
			
			Assert.AreEqual(420.5f, area, 0.00000001);
			Assert.AreEqual(420.5f, area2, 0.00000001);
		}
		
		[Test]
		public void TestAreaOfTriangle3()
		{
			// http://keisan.casio.com/has10/SpecExec.cgi?id=system/2006/1223520411
			
			var A = new Point3D(-2, 3);
			var B = new Point3D(-3, -1);
			var C = new Point3D(3, -2);
			var area = Transformation.AreaOfTriangle(A, B, C);
			var area2 = Transformation.AreaOfTriangleFast(A, B, C);
			
			Assert.AreEqual(12.5f, area, 0.00000001);
			Assert.AreEqual(12.5f, area2, 0.00000001);
		}

		[Test]
		public void TestAreaOfRectangle1() {
			// http://www.mathopenref.com/coordrectangle.html
			
			var A = new Point3D(13, 19);
			var B = new Point3D(21, 34);
			var C = new Point3D(47, 19);
			var D = new Point3D(38, 5);
			
			var area = Transformation.AreaOfRectangle(A, B, C, D);
			
			Assert.AreEqual(510.2832f, area, 0.0001);
		}
		
		[Test]
		public void TestAreaOfRectangle2() {
			// http://www.mathopenref.com/coordrectangle.html
			
			var A = new Point3D(-10, 15);
			var B = new Point3D(-3, 26);
			var C = new Point3D(25, 9);
			var D = new Point3D(18, -2);
			
			var area = Transformation.AreaOfRectangle(A, B, C, D);
			
			Assert.AreEqual(427.0948f, area, 0.0001);
		}

		[Test]
		public void TestRectangleContains1() {

			// rotated rectangle
			var A = new Point3D(-10, 15);
			var B = new Point3D(-5, 25);
			var C = new Point3D(25, 7);
			var D = new Point3D(20, -3);
			
			var P1 = new Point3D(15, 10);
			var P2 = new Point3D(-5, 15);
			var P3 = new Point3D(20, -1);
			
			var P4 = new Point3D(0, 0);
			var P5 = new Point3D(20, 15);
			var P6 = new Point3D(-5, 10);
			
			Assert.IsTrue(Transformation.RectangleContains(A, B, C, D, P1));
			Assert.IsTrue(Transformation.RectangleContains(A, B, C, D, P2));
			Assert.IsTrue(Transformation.RectangleContains(A, B, C, D, P3));
			
			Assert.IsFalse(Transformation.RectangleContains(A, B, C, D, P4));
			Assert.IsFalse(Transformation.RectangleContains(A, B, C, D, P5));
			Assert.IsFalse(Transformation.RectangleContains(A, B, C, D, P6));
		}

		[Test]
		public void TestRectangleContains2() {

			// http://www.mathopenref.com/coordrectareaperim.html
			
			// straight rectangle
			var A = new Point3D(-10, -5);
			var B = new Point3D(-10, 10);
			var C = new Point3D(15, 10);
			var D = new Point3D(15, -5);
			
			var P1 = new Point3D(15, 10);
			var P2 = new Point3D(-5, 7.5f);
			var P3 = new Point3D(12.5f, -2.5f);
			
			var P4 = new Point3D(0, -10);
			var P5 = new Point3D(20, 15);
			var P6 = new Point3D(-15, 10);
			
			Assert.IsTrue(Transformation.RectangleContains(A, B, C, D, P1));
			Assert.IsTrue(Transformation.RectangleContains(A, B, C, D, P2));
			Assert.IsTrue(Transformation.RectangleContains(A, B, C, D, P3));
			
			Assert.IsFalse(Transformation.RectangleContains(A, B, C, D, P4));
			Assert.IsFalse(Transformation.RectangleContains(A, B, C, D, P5));
			Assert.IsFalse(Transformation.RectangleContains(A, B, C, D, P6));
		}
		
		[Test]
		public void TestRectangleContains3() {

			// http://www.mathopenref.com/coordrectareaperim.html
			
			// straight rectangle
			var rect = new RectangleF(-10, -5, 25, 15);
			
			var P1 = new Point3D(15, 10);
			var P2 = new Point3D(-5, 7.5f);
			var P3 = new Point3D(12.5f, -2.5f);
			
			var P4 = new Point3D(0, -10);
			var P5 = new Point3D(20, 15);
			var P6 = new Point3D(-15, 10);
			
			Assert.IsTrue(Transformation.RectangleContains(rect, P1));
			Assert.IsTrue(Transformation.RectangleContains(rect, P2));
			Assert.IsTrue(Transformation.RectangleContains(rect, P3));
			
			Assert.IsFalse(Transformation.RectangleContains(rect, P4));
			Assert.IsFalse(Transformation.RectangleContains(rect, P5));
			Assert.IsFalse(Transformation.RectangleContains(rect, P6));
		}
	}
}