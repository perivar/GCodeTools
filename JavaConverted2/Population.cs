using System;

namespace JavaConverted
{
	public class Population
	{
		internal const int ELITISM_K = 5;
		internal const int POP_SIZE = 200 + ELITISM_K; // population size, 200 + ELITISM_K
		internal const int MAX_ITER = 50000; // max number of iterations
		internal const double MUTATION_RATE = 0.001; // probability of mutation, 0.05
		internal const double CROSSOVER_RATE = 0.9; // probability of crossover, 0.7

		private static Random m_rand = new Random(); // random-number generator
		private Individual[] m_population;
		private double totalFitness;

		public Population()
		{
			m_population = new Individual[POP_SIZE];

			// init population
			for (int i = 0; i < POP_SIZE; i++)
			{
				m_population[i] = new Individual();
				m_population[i].RandomGenes();
			}

			// evaluate current population
			this.Evaluate();
		}

		public void SetPopulation(Individual[] newPop)
		{
			System.Array.Copy(newPop, 0, this.m_population, 0, POP_SIZE);
		}

		public Individual[] GetPopulation()
		{
			return this.m_population;
		}

		public double Evaluate()
		{
			this.totalFitness = 0.0;
			for (int i = 0; i < POP_SIZE; i++)
			{
				this.totalFitness += m_population[i].Evaluate();
			}
			return this.totalFitness;
		}

		public Individual RouletteWheelSelection()
		{
			double randNum = m_rand.NextDouble() * this.totalFitness;
			int idx;
			for (idx=0; idx<POP_SIZE && randNum>0; ++idx)
			{
				randNum -= m_population[idx].GetFitnessValue();
			}
			return m_population[idx-1];
		}

		public Individual FindBestIndividual()
		{
			int idxMax = 0, idxMin = 0;
			double currentMax = 0.0;
			double currentMin = 1.0;
			double currentVal;

			for (int idx=0; idx<POP_SIZE; ++idx)
			{
				currentVal = m_population[idx].GetFitnessValue();
				if (currentMax < currentMin)
				{
					currentMax = currentMin = currentVal;
					idxMax = idxMin = idx;
				}
				if (currentVal > currentMax)
				{
					currentMax = currentVal;
					idxMax = idx;
				}
				if (currentVal < currentMin)
				{
					currentMin = currentVal;
					idxMin = idx;
				}
			}

			return m_population[idxMin];      // minimization
			//return m_population[idxMax]; // maximization
		}

		public static Individual[] Crossover(Individual indiv1, Individual indiv2)
		{
			var newIndiv = new Individual[2];
			newIndiv[0] = new Individual();
			newIndiv[1] = new Individual();

			int randPoint = m_rand.Next(Individual.SIZE);
			int i;
			for (i=0; i<randPoint; ++i)
			{
				newIndiv[0].SetGene(i, indiv1.GetGene(i));
				newIndiv[1].SetGene(i, indiv2.GetGene(i));
			}
			for (; i<Individual.SIZE; ++i)
			{
				newIndiv[0].SetGene(i, indiv2.GetGene(i));
				newIndiv[1].SetGene(i, indiv1.GetGene(i));
			}

			return newIndiv;
		}
		
		// Gets population size
		public int GetPopulationSize()
		{
			return m_population.Length;
		}

		public static void Run()
		{
			var pop = new Population();
			var newPop = new Individual[POP_SIZE];
			var indiv = new Individual[2];

			// current population
			Console.Write("Total Fitness = " + pop.totalFitness);
			Console.WriteLine(" ; Best Fitness = " + pop.FindBestIndividual().GetFitnessValue());

			// main loop
			int count;
			for (int iter = 0; iter < MAX_ITER; iter++)
			{
				count = 0;

				// Elitism
				for (int i=0; i<ELITISM_K; ++i)
				{
					newPop[count] = pop.FindBestIndividual();
					count++;
				}

				// build new Population
				while (count < POP_SIZE)
				{
					// Selection
					indiv[0] = pop.RouletteWheelSelection();
					indiv[1] = pop.RouletteWheelSelection();

					// Crossover
					if (m_rand.NextDouble() < CROSSOVER_RATE)
					{
						indiv = Crossover(indiv[0], indiv[1]);
					}

					// Mutation
					if (m_rand.NextDouble() < MUTATION_RATE)
					{
						indiv[0].Mutate();
					}
					if (m_rand.NextDouble() < MUTATION_RATE)
					{
						indiv[1].Mutate();
					}

					// add to new population
					newPop[count] = indiv[0];
					newPop[count+1] = indiv[1];
					count += 2;
				}
				pop.SetPopulation(newPop);

				// reevaluate current population
				pop.Evaluate();
				
				Console.WriteLine("[{0}/{1}] Total Fitness = {2}, Best Fitness = {3}",
				                  iter,
				                  MAX_ITER,
				                  pop.totalFitness,
				                  pop.FindBestIndividual().GetFitnessValue());
			}

			// best indiv
			Individual bestIndiv = pop.FindBestIndividual();
			Console.Write("Best Individual = " + bestIndiv);
		}
	}
}