using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using GeneticAlgorithm;
using System.IO;
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
		
		float _maxX = 0.0f;
		float _maxY = 0.0f;
		float _scale = 1.0f;
		
		CancellationTokenSource _cancelTokenSource = null;
		
		private DateTime previousTime = DateTime.Now;
		
		public MainForm(List<IPoint> points, float maxX, float maxY)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			_points = points;
			this._maxX = maxX;
			this._maxY = maxY;
			
			//_points = DataProvider.GetPoints(@"JavaScript\data.js", "data200");
			//this._width = 900;
			//this._height = 680;
			
			_alg = new GAAlgorithm(_points);
			
			DrawInitialPoints();
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
			} else if (radScaleEight.Checked) {
				_scale = 8.0f;
			}
			
			if (!_alg.Running) {
				DrawInitialPoints();
			}
		}
		
		void BtnSaveClick(object sender, EventArgs e)
		{
			// first sort by z-order
			//var sortedBestPath = GCodeUtils.SortBlocksByZDepth(_alg.BestPath, _points);

			string newFileName = Path.GetFileNameWithoutExtension(QuickSettings.Get["LastOpenedFile"]) + "_optimized.gcode";
			sfdSaveDialog.FileName = newFileName;
			var result = sfdSaveDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				
				// then save
				GCodeUtils.SaveGCode(_alg.BestPath, _points, sfdSaveDialog.FileName);
			}
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
			// only update if 50 ms has passed
			var timeNow = DateTime.Now;
			if ((DateTime.Now - previousTime).Milliseconds <= 50) return;
			
			DrawUpdatedPoints(alg);
			
			previousTime = timeNow;
		}
		
		#region Draw Methods
		static Bitmap GetImage(float maxX, float maxY, float scale, IList<IPoint> points, GAAlgorithm alg = null) {

			// constants
			const int radius = 4;

			var bitmap = new Bitmap((int)(maxX*scale), (int)(maxY*scale));
			Graphics gfx = Graphics.FromImage(bitmap);

			// set background
			gfx.Clear(Color.White);

			// define pens
			var straightPen = new Pen(Color.Red, 0.5f);
			var lastStraightPen = new Pen(Color.HotPink, 1.0f);
			var arcPen = new Pen(Color.Black, 0.5f);
			var arcBrush = new SolidBrush(Color.Black);
			
			IPoint firstLocation = null;
			IPoint previousLocation = null;
			IPoint currentLocation = null;
			
			int TotalCount = 0;
			if (alg != null) {
				TotalCount = alg.BestPath.Count;
			} else {
				TotalCount = points.Count;
			}
			
			for (int i = 0; i < TotalCount; i++) {
				
				if (alg != null) {
					currentLocation = (IPoint) points[alg.BestPath[i]];
				} else {
					currentLocation = (IPoint) points[i];
				}
				
				if (previousLocation != null) {
					
					// draw
					gfx.FillEllipse( arcBrush, currentLocation.X*scale-radius, (maxY-currentLocation.Y)*scale-radius, radius*2, radius*2 );
					
					gfx.DrawLine(straightPen,
					             previousLocation.X*scale,
					             (maxY-previousLocation.Y)*scale,
					             currentLocation.X*scale,
					             (maxY-currentLocation.Y)*scale);
				}

				previousLocation = currentLocation;
			}
			
			// last line and circle
			if (alg != null) {
				firstLocation = points[alg.BestPath[0]];
			} else {
				firstLocation = points[0];
			}
			
			// draw last circle
			gfx.FillEllipse( Brushes.Yellow, firstLocation.X*scale-radius, (maxY-firstLocation.Y)*scale-radius, radius*2, radius*2 );

			// draw last line
			gfx.DrawLine(lastStraightPen,
			             previousLocation.X*scale,
			             (maxY-previousLocation.Y)*scale,
			             firstLocation.X*scale,
			             (maxY-firstLocation.Y)*scale);

			gfx.Flush();
			gfx.Dispose();
			straightPen.Dispose();
			lastStraightPen.Dispose();
			arcPen.Dispose();
			arcBrush.Dispose();

			return bitmap;
		}
		
		void DrawUpdatedPoints(GAAlgorithm alg)
		{
			pictureBox1.Image = GetImage(_maxX, _maxY, _scale, _points, alg);
			
			// print calculated distance
			label1.Text = string.Format("There are {0} G0 points, the {1}th generation with {2} times of mutation. Best value: {3}",
			                            _points.Count, alg.CurrentGeneration, alg.MutationTimes, alg.BestValue);
			
		}
		
		void DrawInitialPoints() {
			pictureBox1.Image = GetImage(_maxX, _maxY, _scale, _points);

			// print initial distance
			label1.Text = string.Format("Initially there are {0} G0 points. Best value: {1}",
			                            _points.Count, _alg.BestValue);
		}

		#endregion
	}
}
