using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using tsp;

using GAF;
using TravellingSalesman;

namespace GCodeOptimizer
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		private static BackgroundWorker _backgroundWorker;
		private static GAOperations _gaOperations;
		private static List<Location> _locations;
		
		private static TSP_GA _tspGa;
		
		private GAAlgorithm _alg;
		private List<Point> _points;
		
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			//_gaOperations = new GAOperations(400, true);
			//_locations = GAOperations.CreateLocations().ToList();
			//var tmpLocations = GAOperations.CreateLocations().ToArray();
			//_locations = GAOperations.GetFakeShortest(tmpLocations).ToList();
			
			//_tspGa = new TSP_GA(100000);
			
			_points = DataProvider.GetPoints(@"JavaScript\data.js", "data200");
			_alg = new GAAlgorithm(_points);
			
			DrawPoints();
		}
		
		#region Events
		void ga_OnGenerationComplete(object sender, GaEventArgs e)
		{
			if (!_backgroundWorker.CancellationPending)
			{
				var fittest = e.Population.GetTop(1)[0];
				var distanceToTravel = GAOperations.CalculateDistance(fittest);
				
				var holder = new ArgumentHolder();
				holder.Chromosome = fittest;
				holder.DistanceToTravel = distanceToTravel;
				holder.Generation = e.Generation;
				holder.TotalGenerationCount = _gaOperations.GenerationCount;
				holder.Fitness = fittest.Fitness;
				
				_backgroundWorker.ReportProgress(e.Generation * 100 / _gaOperations.GenerationCount,
				                                 holder);
				
				//Debug.WriteLine("Generation: {0}, Fitness: {1}, Distance: {2}", e.Generation, fittest.Fitness, distanceToTravel);
			}
		}

		void ga_OnRunComplete(object sender, GaEventArgs e)
		{
			/*
			var fittest = e.Population.GetTop(1)[0];
			foreach (var gene in fittest.Genes)
			{
				Debug.WriteLine(((Location)gene.ObjectValue));
			}
			 */
		}
		
		void tspGaOnGenerationComplete(object sender, MyEventArgs e)
		{
			if (!_backgroundWorker.CancellationPending)
			{
				_backgroundWorker.ReportProgress(e.Generation * 100 / e.TotalGenerationCount, e);
			}
		}
		
		void tspGaOnRunComplete(object sender, MyEventArgs e) {
			Debug.WriteLine("tspGaOnRunComplete");
		}

		void algGaOnGenerationComplete(GAAlgorithm sender)
		{
			if (!_backgroundWorker.CancellationPending)
			{
				_backgroundWorker.ReportProgress(sender.CurrentGeneration, sender);
			}
		}
		
		void algGaOnRunComplete(GAAlgorithm sender) {
			
		}
		#endregion
		
		void MainFormLoad(object sender, EventArgs e)
		{
			_backgroundWorker = new BackgroundWorker
			{
				WorkerSupportsCancellation = true,
				WorkerReportsProgress = true
			};
			_backgroundWorker.DoWork += backgroundWorker_DoWork;
			_backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
			_backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
			
			// hook up to some useful events
			//_gaOperations.OnGenerationComplete += ga_OnGenerationComplete;
			//_gaOperations.OnRunComplete += ga_OnRunComplete;
			
			// hook up using the Java ported code
			//_tspGa.OnGenerationComplete += tspGaOnGenerationComplete;
			//_tspGa.OnRunComplete += tspGaOnRunComplete;

			// hook up using the JavaScript ported code
			_alg.OnGenerationComplete += algGaOnGenerationComplete;
			_alg.OnRunComplete += algGaOnRunComplete;
		}
		
		#region Event Handlers
		private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			//_gaOperations.RunAlgorithm(_locations);
			//_tspGa.Run();
			_alg.Run();
		}
		
		private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			//RedrawPreview((Chromosome)e.UserState);
			//RedrawPreview((MyEventArgs)e.UserState);
			RedrawPreview((GAAlgorithm)e.UserState);
		}

		private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			// print calculated distance
			label1.Text = string.Format("There are {0} G0 points, the {1}th generation with {2} times of mutation. Best value: {3}",
			                            _points.Count, _alg.CurrentGeneration, _alg.MutationTimes, _alg.BestValue);

		}
		#endregion
		
		#region Redraw Preview Methods
		void RedrawPreview(ArgumentHolder holder)
		{
			//Stopwatch stopW = Stopwatch.StartNew();

			if (pictureBox1.Image != null)
			{
				pictureBox1.Image.Dispose();
				pictureBox1.Image = null;
			}

			var b = new Bitmap((int)(pictureBox1.Width), (int)(pictureBox1.Height));

			Graphics gfx = Graphics.FromImage(b);

			gfx.Clear(Color.Snow);

			var straightPen = new Pen(Color.Red, 0.5f);
			var arcPen = new Pen(Color.Black, 0.5f);
			var arcBrush = new SolidBrush(Color.Black);
			
			// run through each location in the order specified in the chromosome
			var distanceToTravel = 0.0;
			Location previousLocation = null;
			const int radius = 4;
			foreach (var gene in holder.Chromosome.Genes)
			{
				var currentLocation = (Location)gene.ObjectValue;

				if (previousLocation != null)
				{
					// calculate total distance
					var distance = previousLocation.GetDistance(currentLocation);
					distanceToTravel += distance;

					// draw
					gfx.FillEllipse( arcBrush, currentLocation.X-radius, currentLocation.Y-radius, radius*2, radius*2 );
					//gfx.DrawEllipse( arcPen, currentLocation.X-r, currentLocation.Y-r, r*2, r*2 );
					
					gfx.DrawLine(straightPen,
					             previousLocation.X,
					             previousLocation.Y,
					             currentLocation.X,
					             currentLocation.Y);
				}

				previousLocation = currentLocation;
			}
			
			gfx.Flush();

			gfx.Dispose();
			straightPen.Dispose();
			arcPen.Dispose();
			arcBrush.Dispose();

			pictureBox1.Image = b;

			// print calculated distance
			label1.Text = String.Format("Generation: {0}, Fitness: {1}, Distance: {2}", holder.Generation, holder.Fitness, distanceToTravel);
			
			//stopW.Stop();
			//Debug.WriteLine("Updating GCode Preview: {0} ms", stopW.Elapsed.TotalMilliseconds);
		}
		
		void RedrawPreview(MyEventArgs e)
		{
			//Stopwatch stopW = Stopwatch.StartNew();

			if (pictureBox1.Image != null)
			{
				pictureBox1.Image.Dispose();
				pictureBox1.Image = null;
			}

			var b = new Bitmap((int)(pictureBox1.Width), (int)(pictureBox1.Height));

			Graphics gfx = Graphics.FromImage(b);

			gfx.Clear(Color.Snow);

			var straightPen = new Pen(Color.Red, 0.5f);
			var arcPen = new Pen(Color.Black, 0.5f);
			var arcBrush = new SolidBrush(Color.Black);
			
			// run through each location in the order specified in the chromosome
			City previousLocation = null;
			const int radius = 4;

			for (int i = 0; i < e.BestTour.TourSize(); i++)
			{
				var currentLocation = e.BestTour.GetCity(i);

				if (previousLocation != null)
				{
					// draw
					gfx.FillEllipse( arcBrush, currentLocation.X-radius, currentLocation.Y-radius, radius*2, radius*2 );
					//gfx.DrawEllipse( arcPen, currentLocation.X-r, currentLocation.Y-r, r*2, r*2 );
					
					gfx.DrawLine(straightPen,
					             previousLocation.X,
					             previousLocation.Y,
					             currentLocation.X,
					             currentLocation.Y);
				}

				previousLocation = currentLocation;
			}
			
			gfx.Flush();

			gfx.Dispose();
			straightPen.Dispose();
			arcPen.Dispose();
			arcBrush.Dispose();

			pictureBox1.Image = b;

			// print calculated distance
			//label1.Text = "" + distanceToTravel;
			label1.Text = String.Format("Generation: {0}, Fitness: {1}, Distance: {2}", e.Generation, e.BestTour.GetFitness(), e.Distance);
			
			//stopW.Stop();
			//Debug.WriteLine("Updating GCode Preview: {0} ms", stopW.Elapsed.TotalMilliseconds);
		}
		
		void RedrawPreview(GAAlgorithm alg)
		{
			//Stopwatch stopW = Stopwatch.StartNew();

			if (pictureBox1.Image != null)
			{
				pictureBox1.Image.Dispose();
				pictureBox1.Image = null;
			}

			var b = new Bitmap((int)(pictureBox1.Width), (int)(pictureBox1.Height));

			Graphics gfx = Graphics.FromImage(b);

			gfx.Clear(Color.Snow);

			var straightPen = new Pen(Color.Red, 0.5f);
			var arcPen = new Pen(Color.Black, 0.5f);
			var arcBrush = new SolidBrush(Color.Black);
			
			// run through each location in the order specified in the chromosome
			var previousLocation = Point.Empty;
			const int radius = 4;
			
			for (int i = 0; i < alg.BestPath.Count; i++)
			{
				var currentLocation = _points[alg.BestPath[i]];
				
				if (previousLocation != Point.Empty)
				{
					// draw
					gfx.FillEllipse( arcBrush, currentLocation.X-radius, currentLocation.Y-radius, radius*2, radius*2 );
					//gfx.DrawEllipse( arcPen, currentLocation.X-r, currentLocation.Y-r, r*2, r*2 );
					
					gfx.DrawLine(straightPen,
					             previousLocation.X,
					             previousLocation.Y,
					             currentLocation.X,
					             currentLocation.Y);
				}

				previousLocation = currentLocation;
			}

			// last line and circle
			var firstLocation = _points[alg.BestPath[0]];

			// draw last circle
			gfx.FillEllipse( Brushes.Yellow, firstLocation.X-radius, firstLocation.Y-radius, radius*2, radius*2 );
			//gfx.DrawEllipse( arcPen, firstLocation.X-r, firstLocation.Y-r, r*2, r*2 );

			// draw last line
			gfx.DrawLine(straightPen,
			             previousLocation.X,
			             previousLocation.Y,
			             firstLocation.X,
			             firstLocation.Y);

			gfx.Flush();

			gfx.Dispose();
			straightPen.Dispose();
			arcPen.Dispose();
			arcBrush.Dispose();

			pictureBox1.Image = b;

			// print calculated distance
			label1.Text = string.Format("There are {0} G0 points, the {1}th generation with {2} times of mutation. Best value: {3}",
			                            _points.Count, alg.CurrentGeneration, alg.MutationTimes, alg.BestValue);
			
			
			//stopW.Stop();
			//Debug.WriteLine("Updating GCode Preview: {0} ms", stopW.Elapsed.TotalMilliseconds);
		}
		#endregion
		
		void DrawPoints() {
			if (pictureBox1.Image != null)
			{
				pictureBox1.Image.Dispose();
				pictureBox1.Image = null;
			}

			var b = new Bitmap((int)(pictureBox1.Width), (int)(pictureBox1.Height));

			Graphics gfx = Graphics.FromImage(b);

			gfx.Clear(Color.Snow);

			var straightPen = new Pen(Color.Red, 0.5f);
			var arcPen = new Pen(Color.Black, 0.5f);
			var arcBrush = new SolidBrush(Color.Black);
			
			// run through each location in the order specified in the chromosome
			var previousLocation = Point.Empty;
			const int radius = 4;
			
			for (int i = 0; i < _points.Count; i++)
			{
				var currentLocation = _points[i];
				
				if (previousLocation != Point.Empty)
				{
					// draw
					gfx.FillEllipse( arcBrush, currentLocation.X-radius, currentLocation.Y-radius, radius*2, radius*2 );
					//gfx.DrawEllipse( arcPen, currentLocation.X-r, currentLocation.Y-r, r*2, r*2 );
					
					gfx.DrawLine(straightPen,
					             previousLocation.X,
					             previousLocation.Y,
					             currentLocation.X,
					             currentLocation.Y);
				}

				previousLocation = currentLocation;
			}

			// last line and circle
			var firstLocation = _points[0];

			// draw last circle
			gfx.FillEllipse( Brushes.Yellow, firstLocation.X-radius, firstLocation.Y-radius, radius*2, radius*2 );
			//gfx.DrawEllipse( arcPen, firstLocation.X-r, firstLocation.Y-r, r*2, r*2 );

			// draw last line
			gfx.DrawLine(straightPen,
			             previousLocation.X,
			             previousLocation.Y,
			             firstLocation.X,
			             firstLocation.Y);

			gfx.Flush();

			gfx.Dispose();
			straightPen.Dispose();
			arcPen.Dispose();
			arcBrush.Dispose();

			pictureBox1.Image = b;

			// print calculated distance
			label1.Text = string.Format("There are {0} G0 points.",
			                            _points.Count);
			
			
			//stopW.Stop();
			//Debug.WriteLine("Updating GCode Preview: {0} ms", stopW.Elapsed.TotalMilliseconds);
			
		}
		
		void BtnStopClick(object sender, EventArgs e)
		{
			if (_alg.Running) {
				_alg.Running = false;
			} else {
				_alg.Running = true;
				_backgroundWorker.RunWorkerAsync();
			}
		}
	}
	
	#region Argument Holder
	class ArgumentHolder {
		private Chromosome chromosome;
		private double distanceToTravel;
		double fitness;
		int generation;
		int totalGenerationCount;

		public Chromosome Chromosome {
			get {
				return chromosome;
			}
			set {
				chromosome = value;
			}
		}

		public double DistanceToTravel {
			get {
				return distanceToTravel;
			}
			set {
				distanceToTravel = value;
			}
		}

		public int Generation {
			get {
				return generation;
			}
			set {
				generation = value;
			}
		}

		public int TotalGenerationCount {
			get {
				return totalGenerationCount;
			}
			set {
				totalGenerationCount = value;
			}
		}
		
		public double Fitness {
			get {
				return fitness;
			}
			set {
				fitness = value;
			}
		}
	}
	#endregion
}
