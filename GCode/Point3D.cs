using System;
using System.Drawing;
using System.Collections.Generic;
using System.Globalization;

namespace GCode
{
	/// <summary>
	/// Struct to hold a 3D point (i.e. x, y and z coordinates)
	/// </summary>
	public struct Point3D : IPoint {

		private float x;
		private float y;
		private float z;
		
		public static readonly Point3D Empty;

		public PointF PointF {
			get {
				return new PointF(this.X, this.Y);
			}
		}
		
		public bool IsEmpty {
			get {
				return this.x == 0f && this.y == 0f && this.z == 0f;
			}
		}

		public float X {
			get {
				return this.x;
			}
			set {
				this.x = value;
			}
		}

		public float Y {
			get {
				return this.y;
			}
			set {
				this.y = value;
			}
		}

		public float Z {
			get {
				return this.z;
			}
			set {
				this.z = value;
			}
		}
		
		public Point3D(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Point3D(float x, float y)
		{
			this.x = x;
			this.y = y;
			this.z = 0;
		}
		
		public static bool operator ==(Point3D left, Point3D right) {
			return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
		}
		
		public static bool operator !=(Point3D left, Point3D right) {
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Point3D)) {
				return false;
			}
			var point3D = (Point3D)obj;
			return point3D.X == this.X && point3D.Y == this.Y && point3D.Z == this.Z
				&& point3D.GetType().Equals(base.GetType());
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		
		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture,
			                     "{{X={0}, Y={1}, Z={2}}}", new object[] {
			                     	this.x,
			                     	this.y,
			                     	this.z
			                     });
		}
	}
	
	/// <summary>
	/// Class to hold gcode-instructions connected to a 3D point (i.e. x, y and z coordinates)
	/// </summary>
	public class Point3DBlock : IPoint {

		private Point3D point;
		
		private List<GCodeInstruction> gcodeInstructions = new List<GCodeInstruction>();
		public List<GCodeInstruction> GCodeInstructions { get { return gcodeInstructions; } }

		public PointF PointF {
			get {
				return new PointF(point.X, point.Y);
			}
		}
		
		#region IPoint implementation
		public float X {
			get {
				return point.X;
			}
		}

		public float Y {
			get {
				return point.Y;
			}
		}
		
		public float Z {
			set {
				point.Z = value;
			}
			get {
				return point.Z;
			}
		}

		public int BestPathIndex { set; get; }
		
		public bool IsEmpty {
			get {
				return this.X == 0f && this.Y == 0f && this.Z == 0f;
			}
		}
		
		public bool EqualXYCoordinates(Point3DBlock other) {
			if (other != null && this.X == other.X && this.Y == other.Y) return true;
			return false;
		}
		
		public bool EqualCoordinates(Point3DBlock other) {
			if (other != null && this.X == other.X && this.Y == other.Y && this.Z == other.Z) return true;
			return false;
		}
		
		#endregion
		
		public Point3DBlock(float x, float y) {
			point.X = x;
			point.Y = y;
			point.Z = 0;
		}
		
		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture,
			                     "X={0}, Y={1}, Z={2}, Index: {3} - Count: {4}",
			                     this.X,
			                     this.Y,
			                     this.Z,
			                     this.BestPathIndex,
			                     this.gcodeInstructions.Count
			                    );
		}
	}
}
