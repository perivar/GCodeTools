using System;

namespace tsp
{
	/// <summary>
	/// Population.java
	/// Manages a population of candidate tours
	/// </summary>
	public class Population
	{
		// Holds population of tours
		internal Tour[] tours;

		// Construct a population
		public Population(int populationSize, bool initialise)
		{
			tours = new Tour[populationSize];
			// If we need to initialise a population of tours do so
			if (initialise)
			{
				// Loop and create individuals
				for (int i = 0; i < GetPopulationSize(); i++)
				{
					var newTour = new Tour();
					newTour.GenerateIndividual();
					SaveTour(i, newTour);
				}
			}
		}

		// Saves a tour
		public virtual void SaveTour(int index, Tour tour)
		{
			tours[index] = tour;
		}

		// Gets a tour from population
		public virtual Tour GetTour(int index)
		{
			return tours[index];
		}

		// Gets the best tour in the population
		public virtual Tour GetFittest()
		{
			Tour fittest = tours[0];
			// Loop through individuals to find fittest
			for (int i = 1; i < GetPopulationSize(); i++)
			{
				if (fittest.GetFitness() <= GetTour(i).GetFitness())
				{
					fittest = GetTour(i);
				}
			}
			return fittest;
		}

		// Gets population size
		public virtual int GetPopulationSize()
		{
			return tours.Length;
		}
	}
}