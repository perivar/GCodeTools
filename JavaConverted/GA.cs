using System;

namespace tsp
{
	/// <summary>
	/// GA.java
	/// Manages algorithms for evolving population
	/// </summary>
	public class GA
	{
		private static Random rng = new Random();

		// GA parameters
		private const double mutationRate = 0.001; // originally 0.015
		private const int tournamentSize = 5; // originally 5
		private const bool elitism = true;

		// Evolves a population over one generation
		public static Population EvolvePopulation(Population pop)
		{
			var newPopulation = new Population(pop.GetPopulationSize(), false);

			// Keep our best individual if elitism is enabled
			int elitismOffset = 0;
			if (elitism)
			{
				//newPopulation.SaveTour(0, pop.GetFittest());
				//elitismOffset = 1;

				var fittest = pop.GetFittest();
				newPopulation.SaveTour(0, fittest);
				//newPopulation.SaveTour(1, (Tour)fittest.Clone());
				//newPopulation.SaveTour(2, (Tour)fittest.Clone());
				//newPopulation.SaveTour(3, (Tour)fittest.Clone());
				elitismOffset = 1;
			}

			// Crossover population
			// Loop over the new population's size and create individuals from
			// Current population
			for (int i = elitismOffset; i < newPopulation.GetPopulationSize(); i++)
			{
				// Select parents
				Tour parent1 = TournamentSelection(pop);
				Tour parent2 = TournamentSelection(pop);
				
				// Crossover parents
				Tour child = Crossover(parent1, parent2);
				
				// Add child to new population
				newPopulation.SaveTour(i, child);
			}

			// Mutate the new population a bit to add some new genetic material
			for (int i = elitismOffset; i < newPopulation.GetPopulationSize(); i++)
			{
				Mutate(newPopulation.GetTour(i));
			}

			return newPopulation;
		}

		// Applies crossover to a set of parents and creates offspring
		public static Tour Crossover(Tour parent1, Tour parent2)
		{
			// Create new child tour
			var child = new Tour();

			// Get start and end sub tour positions for parent1's tour
			int startPos = (int)(rng.NextDouble() * parent1.TourSize());
			int endPos = (int)(rng.NextDouble() * parent1.TourSize());

			// Loop and add the sub tour from parent1 to our child
			for (int i = 0; i < child.TourSize(); i++)
			{
				// If our start position is less than the end position
				if (startPos < endPos && i > startPos && i < endPos)
				{
					child.SetCity(i, parent1.GetCity(i));
				} // If our start position is larger
				else if (startPos > endPos)
				{
					if (!(i < startPos && i > endPos))
					{
						child.SetCity(i, parent1.GetCity(i));
					}
				}
			}

			// Loop through parent2's city tour
			for (int i = 0; i < parent2.TourSize(); i++)
			{
				// If child doesn't have the city add it
				if (!child.ContainsCity(parent2.GetCity(i)))
				{
					// Loop to find a spare position in the child's tour
					for (int ii = 0; ii < child.TourSize(); ii++)
					{
						// Spare position found, add city
						if (child.GetCity(ii) == null)
						{
							child.SetCity(ii, parent2.GetCity(i));
							break;
						}
					}
				}
			}
			return child;
		}

		// Mutate a tour using swap mutation
		private static void Mutate(Tour tour)
		{
			// Loop through tour cities
			for(int tourPos1=0; tourPos1 < tour.TourSize(); tourPos1++)
			{
				// Apply mutation rate
				if(rng.NextDouble() < mutationRate)
				{
					// Get a second random position in the tour
					int tourPos2 = (int)(tour.TourSize() * rng.NextDouble());

					// Get the cities at target position in tour
					City city1 = tour.GetCity(tourPos1);
					City city2 = tour.GetCity(tourPos2);

					// Swap them around
					tour.SetCity(tourPos2, city1);
					tour.SetCity(tourPos1, city2);
				}
			}
		}

		// Selects candidate tour for crossover
		private static Tour TournamentSelection(Population pop)
		{
			// Create a tournament population
			var tournament = new Population(tournamentSize, false);
			// For each place in the tournament get a random candidate tour and
			// add it
			for (int i = 0; i < tournamentSize; i++)
			{
				int randomId = (int)(rng.NextDouble() * pop.GetPopulationSize());
				tournament.SaveTour(i, pop.GetTour(randomId));
			}
			// Get the fittest tour
			Tour fittest = tournament.GetFittest();
			return fittest;
		}
	}
}