using System;
using System.Collections.Generic;

namespace tsp
{
	/// <summary>
	/// TourManager.java
	/// Holds the cities of a tour
	/// </summary>
	public class TourManager
	{
		// Holds our cities
		private static List<City> destinationCities = new List<City>();

		// Adds a destination city
		public static void AddCity(City city)
		{
			destinationCities.Add(city);
		}

		// Get a city
		public static City GetCity(int index)
		{
			return (City)destinationCities[index];
		}

		// Get the number of destination cities
		public static int NumberOfCities()
		{
			return destinationCities.Count;
		}
	}
}