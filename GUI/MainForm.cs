using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using GeneticAlgorithm;
using GCode;

namespace GCodeOptimizer
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		private GAAlgorithm _alg;
		private List<IPoint> _points;
		
		float _width = 0.0f;
		float _height = 0.0f;
		float _scale = 2.0f;
		
		CancellationTokenSource _cancelTokenSource = null;
		
		public MainForm(List<IPoint> points, float maxX, float maxY)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			_points = points;
			this._width = maxX;
			this._height = maxY;
			
			//_points = DataProvider.GetPoints(@"JavaScript\data.js", "data200");
			//this._width = 900;
			//this._height = 680;
			
			_alg = new GAAlgorithm(_points);
			
			DrawPoints();
		}
		
		void RadScaleCheckedChanged(object sender, EventArgs e)
		{
			// check scale
			if (radScaleHalf.Checked) {
				_scale = 0.5f;
			} else if (radScaleOne.Checked) {
				_scale = 1.0f;
			} else if (radScaleTwo.Checked) {
				_scale = 2.0f;
			} else if (radScaleFour.Checked) {
				_scale = 4.0f;
			}
			
			if (!_alg.Running) {
				DrawPoints();
			}
		}
		
		void BtnSaveClick(object sender, EventArgs e)
		{
			MessageBox.Show(string.Format("There are {0} G0 points, the {1}th generation with {2} times of mutation. Best value: {3}",
			                              _points.Count, _alg.CurrentGeneration, _alg.MutationTimes, _alg.BestValue));
		}
		
		async void BtnStartStopClick(object sender, EventArgs e)
		{
			if (_alg.Running) {
				_alg.Running = false;
				_cancelTokenSource.Cancel();
			} else {
				_alg.Running = true;
				btnStartStop.Text = "Stop";
				var progressIndicator = new Progress<GAAlgorithm>(ReportProgress);
				_cancelTokenSource = new CancellationTokenSource();
				try {
					await _alg.Run(progressIndicator, _cancelTokenSource.Token);
				} catch (OperationCanceledException) {
					// do nothing?
					
				} finally {
					btnStartStop.Text = "Start";
					_alg.Running = false;
					_cancelTokenSource = null;
				}
			}
		}
		
		// method that is called from the async GAAlgorithm long running task
		void ReportProgress(GAAlgorithm alg)
		{
			RedrawPreview(alg);
			
			// print calculated distance
			label1.Text = string.Format("There are {0} G0 points, the {1}th generation with {2} times of mutation. Best value: {3}",
			                            _points.Count, alg.CurrentGeneration, alg.MutationTimes, alg.BestValue);
			
		}
		
		#region Draw Methods
		void RedrawPreview(GAAlgorithm alg)
		{
			//var b = new Bitmap((int)(pictureBox1.Width), (int)(height));
			var b = new Bitmap((int)(_width*_scale), (int)(_height*_scale));

			Graphics gfx = Graphics.FromImage(b);

			gfx.Clear(Color.Snow);

			var straightPen = new Pen(Color.Red, 0.5f);
			var arcPen = new Pen(Color.Black, 0.5f);
			var arcBrush = new SolidBrush(Color.Black);
			
			// run through each location in the order specified in the chromosome
			var previousLocation = (IPoint) Point3D.Empty;
			const int radius = 4;
			
			for (int i = 0; i < alg.BestPath.Count; i++) {
				
				var currentLocation = (IPoint) _points[alg.BestPath[i]];
				
				if (!previousLocation.IsEmpty) {
					
					// draw
					gfx.FillEllipse( arcBrush, currentLocation.X*_scale-radius, (_height-currentLocation.Y)*_scale-radius, radius*2, radius*2 );
					
					gfx.DrawLine(straightPen,
					             previousLocation.X*_scale,
					             (_height-previousLocation.Y)*_scale,
					             currentLocation.X*_scale,
					             (_height-currentLocation.Y)*_scale);
				}

				previousLocation = currentLocation;
			}

			// last line and circle
			var firstLocation = _points[alg.BestPath[0]];

			// draw last circle
			gfx.FillEllipse( Brushes.Yellow, firstLocation.X*_scale-radius, (_height-firstLocation.Y)*_scale-radius, radius*2, radius*2 );

			// draw last line
			gfx.DrawLine(straightPen,
			             previousLocation.X*_scale,
			             (_height-previousLocation.Y)*_scale,
			             firstLocation.X*_scale,
			             (_height-firstLocation.Y)*_scale);

			gfx.Flush();
			gfx.Dispose();
			straightPen.Dispose();
			arcPen.Dispose();
			arcBrush.Dispose();

			pictureBox1.Image = b;
		}
		
		void DrawPoints() {
			//var b = new Bitmap((int)(pictureBox1.Width), (int)(height));
			var b = new Bitmap((int)(_width*_scale), (int)(_height*_scale));

			Graphics gfx = Graphics.FromImage(b);

			gfx.Clear(Color.Snow);

			var straightPen = new Pen(Color.Red, 0.5f);
			var arcPen = new Pen(Color.Black, 0.5f);
			var arcBrush = new SolidBrush(Color.Black);
			
			// run through each location in the order specified in the chromosome
			var previousLocation = (IPoint) Point3D.Empty;
			const int radius = 4;
			
			for (int i = 0; i < _points.Count; i++) {
				
				var currentLocation = (IPoint) _points[i];
				
				if (!previousLocation.IsEmpty) {
					// draw
					gfx.FillEllipse( arcBrush, currentLocation.X*_scale-radius, (_height-currentLocation.Y)*_scale-radius, radius*2, radius*2 );
					
					gfx.DrawLine(straightPen,
					             previousLocation.X*_scale,
					             (_height-previousLocation.Y)*_scale,
					             currentLocation.X*_scale,
					             (_height-currentLocation.Y)*_scale);
				}

				previousLocation = currentLocation;
			}

			// last line and circle
			var firstLocation = _points[0];

			// draw last circle
			gfx.FillEllipse( Brushes.Yellow, firstLocation.X*_scale-radius, (_height-firstLocation.Y)*_scale-radius, radius*2, radius*2 );

			// draw last line
			gfx.DrawLine(straightPen,
			             previousLocation.X*_scale,
			             (_height-previousLocation.Y)*_scale,
			             firstLocation.X*_scale,
			             (_height-firstLocation.Y)*_scale);

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
