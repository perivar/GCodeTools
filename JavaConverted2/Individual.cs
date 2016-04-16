using System;

namespace JavaConverted
{
	public class Individual
	{
		public const int SIZE = 500;
		private int[] genes = new int[SIZE];
		private int fitnessValue;
		private static Random m_rand = new Random(); // random-number generator

		public Individual()
		{
		}

		public int GetFitnessValue()
		{
			return fitnessValue;
		}

		public void SetFitnessValue(int fitnessValue)
		{
			this.fitnessValue = fitnessValue;
		}

		public int GetGene(int index)
		{
			return genes[index];
		}

		public void SetGene(int index, int gene)
		{
			this.genes[index] = gene;
		}

		public void RandomGenes()
		{
			for(int i=0; i<SIZE; ++i)
			{
				this.SetGene(i, m_rand.Next(2));
			}
		}

		public void Mutate()
		{
			int index = m_rand.Next(SIZE);
			this.SetGene(index, 1-this.GetGene(index)); // flip
		}

		public int Evaluate()
		{
			int fitness = 0;
			for(int i=0; i<SIZE; ++i)
			{
				fitness += this.GetGene(i);
			}
			this.SetFitnessValue(fitness);

			return fitness;
		}
		
		public override string ToString()
		{
			return "Fitness: " + GetFitnessValue() + ", Num Genes: " + SIZE;
		}
	}
}