using System;

namespace tsp
{
	/// <summary>
	/// City.java
	/// Models a city
	/// </summary>
	public class City : ICloneable
	{
		private static Random rng = new Random();

		#region Properties
		// Gets and sets the city's x coordinate
		public int X {
			get;
			set;
		}

		// Gets and sets the city's y coordinate
		public int Y {
			get;
			set;
		}
		#endregion
		
		// Constructs a randomly placed city
		public City()
		{
			this.X = (int)(rng.NextDouble()*200);
			this.Y = (int)(rng.NextDouble()*200);
		}

		// Constructs a city at chosen x, y location
		public City(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		// Gets the distance to given city
		public virtual double DistanceTo(City city)
		{
			int xDistance = Math.Abs(this.X - city.X);
			int yDistance = Math.Abs(this.Y - city.Y);
			double distance = Math.Sqrt((xDistance*xDistance) + (yDistance*yDistance));

			return distance;
		}

		public override string ToString()
		{
			return this.X+", "+this.Y;
		}

		#region ICloneable implementation
		public object Clone()
		{
			return new City(this.X, this.Y);
		}
		#endregion
	}
}