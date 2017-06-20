using System;
using System.Drawing;
using System.Windows.Forms;
using GCode;

namespace GCodePlotter
{
	/// <summary>
	/// Description of frmOptions.
	/// </summary>
	public partial class frmOptions : Form
	{
		public frmOptions()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();

			// read in existing options
			var layerSetting = QuickSettings.Get["ZDepths"];
			if (!"".Equals(layerSetting)) {
				txtLayers.Text = layerSetting;
			} else {
				// add some default values
				txtLayers.Text = "-0.1,-0.15,-0.2";
			}
		}
		
		void BtnSaveClick(object sender, EventArgs e)
		{
			QuickSettings.Get["ZDepths"] = txtLayers.Text;
			this.DialogResult = DialogResult.OK;
			this.Dispose();
		}
		
		void BtnCancelClick(object sender, EventArgs e)
		{
			this.Dispose();
		}
	}
}
