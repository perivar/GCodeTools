using System;
using System.Collections.Generic;
using System.Linq;
using GAF;
using GAF.Extensions;
using GAF.Operators;

namespace TravellingSalesman
{
	public class GAOperations
	{
		#region Fields
		private int _generationCount;
		private bool _randomize;
		#endregion
		
		#region Events

		public event GeneticAlgorithm.GenerationCompleteHandler OnGenerationComplete;
		public event GeneticAlgorithm.RunCompleteHandler OnRunComplete;

		#endregion
		
		#region Properties

		public int GenerationCount
		{
			get { return _generationCount; }
			set { _generationCount = value; }
		}

		public bool Randomize {
			get {
				return _randomize;
			}
			set {
				_randomize = value;
			}
		}
		#endregion
		
		#region Constructor

		public GAOperations(int generationCount, bool randomize = true)
		{
			_generationCount = generationCount;
			_randomize = randomize;
		}

		#endregion
		
		public void RunAlgorithm(List<Location> locations)
		{
			//const int populationSize = 100;
			const int populationSize = 30;
			//const int populationSize = 10;

			//get our locations
			//var locations = CreateLocations().ToList();

			//Each locationis an object the chromosome is a special case as it needs
			//to contain each location only once. Therefore, our chromosome will contain
			//all the locations with no duplicates

			//we can create an empty population as we will be creating the
			//initial solutions manually.
			var population = new Population();
			
			//create the chromosomes
			for (var p = 0; p < populationSize; p++)
			{
				var chromosome = new Chromosome();
				foreach (var location in locations)
				{
					chromosome.Genes.Add(new Gene(location));
				}

				if (_randomize) {
					var rnd = GAF.Threading.RandomProvider.GetThreadRandom();
					chromosome.Genes.ShuffleFast(rnd);
				}

				population.Solutions.Add(chromosome);
			}
			
			//create the elite operator
			var elite = new Elite(3); // originally 5%, new 3%?
			
			//create the crossover operator
			var crossover = new Crossover(0.9) // originally 0.8, new 0.9?
			{
				CrossoverType = CrossoverType.DoublePointOrdered
			};
			
			//create the mutation operator
			var mutate = new SwapMutate(0.01); // originally 0.02, new 0.01?

			//create the GA
			var ga = new GeneticAlgorithm(population, CalculateFitness);

			// subscribe to the generation and run complete events
			ga.OnGenerationComplete += (sender, e) =>
			{
				if (OnGenerationComplete != null)
					OnGenerationComplete(sender, e);
			};

			ga.OnRunComplete += (sender, e) =>
			{
				if (OnRunComplete != null)
					OnRunComplete(sender, e);
			};
			
			//add the operators
			ga.Operators.Add(elite);
			ga.Operators.Add(crossover);
			ga.Operators.Add(mutate);
			
			//run the GA
			ga.Run(Terminate);
		}

		#region Location
		public static IEnumerable<Location> CreateLocations() {
			return DataProvider.GetDestinations(@"data.js", "data200");
		}

		// The value returned by the fitness function should be set to a real number between 0 and 1.0
		// the higher the value, the fitter the solution.
		public static double CalculateFitness(Chromosome chromosome)
		{
			var distanceToTravel = CalculateDistance(chromosome);
			return 1.0 / (distanceToTravel / 10000);
		}

		public static double CalculateDistance(Chromosome chromosome)
		{
			var distanceToTravel = 0.0;
			Location previousLocation = null;

			//run through each location in the order specified in the chromosome
			foreach (var gene in chromosome.Genes)
			{
				var currentLocation = (Location)gene.ObjectValue;

				if (previousLocation != null)
				{
					var distance = previousLocation.GetDistance(currentLocation);
					distanceToTravel += distance;
				}

				previousLocation = currentLocation;
			}

			return distanceToTravel;
		}

		#endregion
		
		private bool Terminate(Population population, int currentGeneration, long currentEvaluation)
		{
			return currentGeneration > _generationCount;
		}
		
		
		public static Location[] GetFakeShortest(Location[] destinations)
		{
			var result = new Location[destinations.Length];

			var currentLocation = destinations[0];
			for(int fillingIndex=0; fillingIndex<destinations.Length; fillingIndex++)
			{
				int bestIndex = -1;
				double bestDistance = double.MaxValue;

				for(int evaluatingIndex=0; evaluatingIndex<destinations.Length; evaluatingIndex++)
				{
					var evaluatingItem = destinations[evaluatingIndex];
					if (evaluatingItem == null)
						continue;

					double distance = currentLocation.GetDistance(evaluatingItem);
					if (distance < bestDistance)
					{
						bestDistance = distance;
						bestIndex = evaluatingIndex;
					}
				}

				result[fillingIndex] = destinations[bestIndex];
				currentLocation = destinations[bestIndex];
				destinations[bestIndex] = null;
			}

			return result;
		}

	}
}