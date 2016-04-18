using System;
using System.Collections.Generic;
using tsp;
using System.Windows.Forms;
using GCodePlotter;

namespace GCodeOptimizer
{
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	internal sealed class Program
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			//Application.Run(new MainForm());
			Application.Run(new frmPlotter());

			/*
			var points = TravellingSalesman.DataProvider.GetPoints(@"JavaScript\data.js", "data200");
			var alg = new GAAlgorithm(points);

			const int MAX_ITER = 10000;
			const int MAX_UNCHANGED_GENERATIONS = 400;
			for (int iter = 0; iter < MAX_ITER; iter++)
			{
				alg.GANextGeneration();
				
				Console.WriteLine("There are {0} G0 points, the {1}th generation with {2} times of mutation. Best value: {3}",
				                  points.Count, alg.CurrentGeneration, alg.MutationTimes, alg.BestValue);
				
				if (alg.UnchangedGenerations > MAX_UNCHANGED_GENERATIONS) break;
			}
			Console.ReadKey();
			 */
			
			/*
			var str1 = new List<char>("The morning is upon us.".ToCharArray());
			var str2 = str1.Slice(4, -2); // returns: morning is upon u
			var str3 = str1.Slice(-3, -1); // returns 'us'
			var str4 = str1.Slice(0, -1);  // returns 'The morning is upon us'
			 */
			
			//TSP_GA.Run();
			//JavaConverted.Population.Run();
			
			//Console.ReadLine();
		}
	}
}

