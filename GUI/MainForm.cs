using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

using GeneticAlgorithm;
using GCode;

namespace GCodeOptimizer
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		private static BackgroundWorker _backgroundWorker;
		private GAAlgorithm _alg;
		private List<IPoint> _points;
		
		public MainForm(List<IPoint> points)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//_points = DataProvider.GetPoints(@"JavaScript\data.js", "data200");
			_points = points;
			_alg = new GAAlgorithm(_points);
			
			DrawPoints();
		}
		
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
			
			// hook up using the JavaScript ported code
			_alg.OnGenerationComplete += algGaOnGenerationComplete;
			_alg.OnRunComplete += algGaOnRunComplete;
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
		
		#region Events
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
		
		#region Event Handlers
		private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			_alg.Run();
		}
		
		private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			RedrawPreview((GAAlgorithm)e.UserState);
		}

		private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			// print calculated distance
			label1.Text = string.Format("There are {0} G0 points, the {1}th generation with {2} times of mutation. Best value: {3}",
			                            _points.Count, _alg.CurrentGeneration, _alg.MutationTimes, _alg.BestValue);

		}
		#endregion
		
		#region Draw Methods
		void RedrawPreview(GAAlgorithm alg)
		{
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
			var previousLocation = Point3D.Empty;
			const int radius = 4;
			
			for (int i = 0; i < alg.BestPath.Count; i++)
			{
				var currentLocation = (Point3D) _points[alg.BestPath[i]];
				
				if (previousLocation != Point3D.Empty)
				{
					// draw
					gfx.FillEllipse( arcBrush, currentLocation.X-radius, currentLocation.Y-radius, radius*2, radius*2 );
					
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
			
		}
		
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
			var previousLocation = Point3D.Empty;
			const int radius = 4;
			
			for (int i = 0; i < _points.Count; i++)
			{
				var currentLocation = (Point3D) _points[i];
				
				if (previousLocation != Point3D.Empty)
				{
					// draw
					gfx.FillEllipse( arcBrush, currentLocation.X-radius, currentLocation.Y-radius, radius*2, radius*2 );
					
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
			
		}
		#endregion
	}
}
