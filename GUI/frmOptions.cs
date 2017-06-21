using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Globalization;
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
		
		void BtnGeneratePeckClick(object sender, EventArgs e)
		{
			float f = 1.6f; // use as ceiling
			
			// only allow decimal and not minus
			NumberStyles style = NumberStyles.AllowDecimalPoint;
			float thickness = 0.0f;
			if (!float.TryParse(txtThickness.Text, style, CultureInfo.InvariantCulture, out thickness)) {
				txtThickness.Text = "0.0";
				thickness = 2.0f;
			} else {
				var layers = new List<float>();
				float fSum = 0.0f;
				int count = (int) Math.Ceiling(thickness/f);
				for (int i = 0; i < count; i++) {
					fSum += f;
					if (fSum > thickness) {
						layers.Add(-thickness);
						break;
					} else {
						layers.Add(-fSum);
					}
				}
				string result = String.Join(",", layers.Select(val => val.ToString(CultureInfo.InvariantCulture)));
				txtLayers.Text = result;
			}
			
		}
	}
}
