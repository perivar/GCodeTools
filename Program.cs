using System;
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
			Application.Run(args.Length == 0 ? new frmPlotter(string.Empty) : new frmPlotter(args[0]));
		}
	}
}

