using System;
using System.Linq;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

using GCode;

namespace GeneticAlgorithm
{
	/// <summary>
	/// This whole algorithm is ported from the JavaScript version found at
	/// http://xyzbots.com/gcode-optimizer/
	/// https://github.com/andrewhodel/gcode-optimizer
	/// Based on https://github.com/parano/GeneticAlgorithm-TSP
	/// <seealso cref="http://geneticalgorithms.ai-depot.com/Tutorial/Overview.html"/>
	/// Ported by perivar@nerseth.com, 2016
	/// </summary>
	public class GAAlgorithm {
		
		private static Random rng = new Random();
		
		enum Direction {
			Next,
			Previous
		}
		
		class Best {
			public int BestPosition {get; set;}
			public int BestValue {get; set;}
			
			public override string ToString()
			{
				return string.Format("[BestPosition={0}, BestValue={1}]", BestPosition, BestValue);
			}
		}
		
		#region Private Fields
		List<IPoint> points;
		bool running = false;
		int POPULATION_SIZE;
		double CROSSOVER_PROBABILITY;
		double MUTATION_PROBABILITY;
		
		int unchangedGenerations;

		int mutationTimes;
		int[][] distances; // distances
		int bestValue;
		List<int> best;
		int currentGeneration;

		Best currentBest;
		double[] roulette;
		List<List<int>> population;
		int[] values;
		double[] fitnessValues;
		#endregion

		#region Public Getters
		
		/// <summary>
		/// Return the current generation number
		/// </summary>
		public int CurrentGeneration {
			get {
				return currentGeneration;
			}
		}

		/// <summary>
		/// Return number of mutations that has occured so far
		/// </summary>
		public int MutationTimes {
			get {
				return mutationTimes;
			}
		}

		/// <summary>
		/// Return the best value found so far
		/// </summary>
		public double BestValue {
			get {
				return bestValue;
			}
		}

		/// <summary>
		/// Return a list of the indexes of the best path found so far
		/// </summary>
		public List<int> BestPath {
			get {
				return best;
			}
		}

		/// <summary>
		/// Returns the number of unchanged generations
		/// The higher the number, the more generations has lived without improving the path
		/// </summary>
		public int UnchangedGenerations {
			get {
				return unchangedGenerations;
			}
		}
		#endregion
		
		#region Properties
		public bool Running {
			get {
				return running;
			}
			set {
				running = value;
			}
		}
		#endregion
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="points">list of points</param>
		public GAAlgorithm(List<IPoint> points) {
			this.points = points;
			
			InitData();
			GAInitialize();
		}

		/// <summary>
		/// Start the Genetic Algorithm and call progress event
		/// Use the Task-Based Asynchronous Pattern
		/// </summary>
		public async Task Run(IProgress<GAAlgorithm> progress, CancellationToken cancellationToken)
		{
			await Task.Run(() =>
			               {
			               	while (running) {
			               		GANextGeneration();
			               		if (progress != null) progress.Report(this);
			               		cancellationToken.ThrowIfCancellationRequested();
			               	}
			               });
		}
		
		/// <summary>
		/// Evolves a population over one generation
		/// Main Genetic Algorithm method that mutates and calculates new values.
		/// Is to be called repeatably until the wanted result is found
		/// </summary>
		public void GANextGeneration() {
			
			// build new population
			currentGeneration++;
			
			/* 3. elitism and roulette wheel selection */
			Selection();
			
			/* 4. crossover */
			Crossover();
			
			/* 5. mutation */
			Mutation();

			/* 6. re-evaluate current population */
			SetBestValue();
		}
		
		void InitData() {
			
			running = false;

			POPULATION_SIZE = 30; 			// 30
			CROSSOVER_PROBABILITY = 0.9;  	// 0.9 or 0.7
			MUTATION_PROBABILITY  = 0.01; 	// 0.01 or 0.05

			unchangedGenerations = 0;
			mutationTimes = 0;

			bestValue = 0;
			best = new List<int>();
			currentGeneration = 0;
			currentBest = null;
			
			population = new List<List<int>>();
			values = new int[POPULATION_SIZE];
			fitnessValues = new double[POPULATION_SIZE];
			roulette = new double[POPULATION_SIZE];
		}
		
		void GAInitialize() {
			CountDistances();
			
			/* 1. init population */
			for(int i=0; i<POPULATION_SIZE; i++) {
				population.Add(RandomIndividual(points.Count));
			}
			
			/* 2. evaluate current population */
			SetBestValue();
		}
		
		void Selection() {
			const int initnum = 4;

			// Keep our best individual if elitism is enabled
			var parents = new List<List<int>>();
			parents.Add(population[currentBest.BestPosition]);
			parents.Add(DoMutate(best.Clone()));
			parents.Add(AddMutate(best.Clone()));
			parents.Add(best.Clone());

			/* 3. roulette wheel selection */
			SetRoulette();
			for(int i=initnum; i<POPULATION_SIZE; i++) {
				parents.Add(population[WheelOut(rng.NextDouble())]);
			}
			population = parents;
		}

		void Crossover() {
			var queue= new List<int>();
			for(int i=0; i<POPULATION_SIZE; i++) {
				if(rng.NextDouble() < CROSSOVER_PROBABILITY ) {
					queue.Add(i);
				}
			}
			queue.Shuffle();
			for(int i=0, j=queue.Count-1; i<j; i+=2) {
				DoCrossover(queue[i], queue[i+1]);
			}
		}

		void DoCrossover(int x, int y) {
			
			var child1 = GetChild(Direction.Next, x, y);
			var child2 = GetChild(Direction.Previous, x, y);
			population[x] = child1;
			population[y] = child2;
		}

		List<int> GetChild(Direction dir, int x, int y) {
			var solution = new List<int>();
			var px = population[x].Clone();
			var py = population[y].Clone();
			int dx = 0;
			int dy = 0;
			int c = px[Utils.RandomNumber(px.Count)];
			solution.Add(c);
			
			while(px.Count > 1) {
				if (dir == Direction.Next) {
					dx = px.Next(px.IndexOf(c));
					dy = py.Next(py.IndexOf(c));
				}
				if (dir == Direction.Previous) {
					dx = px.Previous(px.IndexOf(c));
					dy = py.Previous(py.IndexOf(c));
				}
				
				px.DeleteByValue(c);
				py.DeleteByValue(c);
				
				c = distances[c][dx] < distances[c][dy] ? dx : dy;
				solution.Add(c);
			}
			
			return solution;
		}

		void Mutation() {
			for(int i=0; i<POPULATION_SIZE; i++) {
				if(rng.NextDouble() < MUTATION_PROBABILITY) {
					if(rng.NextDouble() > 0.5) {
						population[i] = AddMutate(population[i]);
					} else {
						population[i] = DoMutate(population[i]);
					}
					i--;
				}
			}
		}

		List<T> DoMutate<T>(List<T> seq){
			mutationTimes++;
			int m,n = 0;
			// m and n refers to the actual index in the array
			// m range from 0 to length-2, n range from 2...Length-m
			do {
				m = Utils.RandomNumber(seq.Count - 2);
				n = Utils.RandomNumber(seq.Count);
			} while (m>=n);

			for(int i=0, j=(n-m+1)>>1; i<j; i++) {
				seq.Swap(m+i, n-i);
			}
			return seq;
		}

		List<T> AddMutate<T>(List<T> seq){
			mutationTimes++;
			int m,n = 0;
			// m and n refers to the actual index in the array
			do {
				m = Utils.RandomNumber(seq.Count>>1);
				n = Utils.RandomNumber(seq.Count);
			} while (m>=n);
			
			var s1 = seq.Slice(0,m);
			var s2 = seq.Slice(m,n);
			var s3 = seq.Slice(n,seq.Count);
			return s2.Concat(s1).Concat(s3).ToList().Clone();
		}

		void SetBestValue() {
			/* 2. evaluate current population */
			for(int i=0; i<population.Count; i++) {
				values[i] = Evaluate(population[i].ToArray());
			}
			currentBest = GetCurrentBest();
			if(bestValue == 0 || bestValue > currentBest.BestValue) {
				best = population[currentBest.BestPosition].Clone();
				bestValue = currentBest.BestValue;
				unchangedGenerations = 0;
			} else {
				unchangedGenerations += 1;
			}
		}

		Best GetCurrentBest() {
			int bestP = 0;
			int currentBestValue = values[0];

			for(int i=1; i<population.Count; i++) {
				if(values[i] < currentBestValue) {
					currentBestValue = values[i];
					bestP = i;
				}
			}
			return new Best() {
				BestPosition = bestP,
				BestValue = currentBestValue,
			};
		}

		void SetRoulette() {
			
			//calculate all the fitness
			for(int i=0; i<values.Length; i++) { fitnessValues[i] = 1.0/values[i]; }
			
			//set the roulette
			double sum = 0;
			for(int i=0; i<fitnessValues.Length; i++) { sum += fitnessValues[i]; }
			for(int i=0; i<roulette.Length; i++) { roulette[i] = fitnessValues[i]/sum; }
			for(int i=1; i<roulette.Length; i++) { roulette[i] += roulette[i-1]; }
		}

		int WheelOut(double rand){
			int i;
			for(i=0; i<roulette.Length; i++) {
				if( rand <= roulette[i] ) {
					return i;
				}
			}
			return 0;
		}

		/// <summary>
		/// Return a list of numbers between 0 and n that has been shuffled
		/// </summary>
		/// <param name="n">upper bound</param>
		/// <returns>a shuffled list of numbers between 0 and n</returns>
		static List<int> RandomIndividual(int n){
			var a = new List<int>();
			for(int i=0; i<n; i++) {
				a.Add(i);
			}
			a.Shuffle();
			return a;
		}

		/// <summary>
		/// Calculate the total sum of the given distances
		/// </summary>
		/// <param name="indivial">array to evaluate</param>
		/// <returns>total sum</returns>
		int Evaluate(int[] indivial) {
			int sum = distances[ indivial[0] ][ indivial[indivial.Length - 1] ];
			for(int i=1; i<indivial.Length; i++) {
				sum += distances[ indivial[i] ][ indivial[i-1] ];
			}
			return sum;
		}

		void CountDistances() {
			int length = points.Count;
			distances = new int[length][];
			for(int i=0; i<length; i++) {
				distances[i] = new int[length];
				for(int j=0; j<length; j++) {
					distances[i][j] = (int) Math.Floor(Utils.Distance(points[i], points[j]));
				}
			}
		}
		
		public override string ToString()
		{
			return string.Format("[CurrentGeneration={0}, MutationTimes={1}, BestValue={2}]", currentGeneration, mutationTimes, bestValue);
		}
	}
}
