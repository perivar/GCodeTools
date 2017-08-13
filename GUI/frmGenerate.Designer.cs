/*
 * Created by SharpDevelop.
 * User: perivar.nerseth
 * Date: 07/04/2016
 * Time: 14:37
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace GCodePlotter
{
	partial class frmGenerate
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.NumericUpDown numericUpDownPointsX;
		private System.Windows.Forms.NumericUpDown numericUpDownGridSize;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown numericUpDownOffsetX;
		private System.Windows.Forms.NumericUpDown numericUpDownOffsetY;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button btnGenerateProbe;
		private System.Windows.Forms.Button btnGenerateGrid;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox textBoxPreviewX;
		private System.Windows.Forms.NumericUpDown numericUpDownPointsY;
		private System.Windows.Forms.TextBox textBoxPreviewY;
		private System.Windows.Forms.Button btnCancel;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.numericUpDownPointsX = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownPointsY = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownGridSize = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.numericUpDownOffsetX = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownOffsetY = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.textBoxPreviewX = new System.Windows.Forms.TextBox();
			this.textBoxPreviewY = new System.Windows.Forms.TextBox();
			this.btnGenerateProbe = new System.Windows.Forms.Button();
			this.btnGenerateGrid = new System.Windows.Forms.Button();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.btnCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownPointsX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownPointsY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownGridSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownOffsetX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownOffsetY)).BeginInit();
			this.SuspendLayout();
			// 
			// numericUpDownPointsX
			// 
			this.numericUpDownPointsX.Location = new System.Drawing.Point(92, 51);
			this.numericUpDownPointsX.Name = "numericUpDownPointsX";
			this.numericUpDownPointsX.Size = new System.Drawing.Size(65, 26);
			this.numericUpDownPointsX.TabIndex = 0;
			this.numericUpDownPointsX.Value = new decimal(new int[] {
			7,
			0,
			0,
			0});
			this.numericUpDownPointsX.ValueChanged += new System.EventHandler(this.NumericUpDownValueChanged);
			// 
			// numericUpDownPointsY
			// 
			this.numericUpDownPointsY.Location = new System.Drawing.Point(199, 51);
			this.numericUpDownPointsY.Name = "numericUpDownPointsY";
			this.numericUpDownPointsY.Size = new System.Drawing.Size(63, 26);
			this.numericUpDownPointsY.TabIndex = 1;
			this.numericUpDownPointsY.Value = new decimal(new int[] {
			5,
			0,
			0,
			0});
			this.numericUpDownPointsY.ValueChanged += new System.EventHandler(this.NumericUpDownValueChanged);
			// 
			// numericUpDownGridSize
			// 
			this.numericUpDownGridSize.DecimalPlaces = 1;
			this.numericUpDownGridSize.Location = new System.Drawing.Point(164, 95);
			this.numericUpDownGridSize.Name = "numericUpDownGridSize";
			this.numericUpDownGridSize.Size = new System.Drawing.Size(98, 26);
			this.numericUpDownGridSize.TabIndex = 2;
			this.numericUpDownGridSize.Value = new decimal(new int[] {
			50,
			0,
			0,
			0});
			this.numericUpDownGridSize.ValueChanged += new System.EventHandler(this.NumericUpDownValueChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 53);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(74, 23);
			this.label1.TabIndex = 3;
			this.label1.Text = "Points:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(173, 53);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(20, 23);
			this.label2.TabIndex = 4;
			this.label2.Text = "x";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(12, 97);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(104, 26);
			this.label3.TabIndex = 5;
			this.label3.Text = "Grid Size:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(12, 138);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(104, 26);
			this.label4.TabIndex = 6;
			this.label4.Text = "Offset:";
			// 
			// numericUpDownOffsetX
			// 
			this.numericUpDownOffsetX.DecimalPlaces = 1;
			this.numericUpDownOffsetX.Location = new System.Drawing.Point(92, 136);
			this.numericUpDownOffsetX.Name = "numericUpDownOffsetX";
			this.numericUpDownOffsetX.Size = new System.Drawing.Size(65, 26);
			this.numericUpDownOffsetX.TabIndex = 7;
			this.numericUpDownOffsetX.ValueChanged += new System.EventHandler(this.NumericUpDownValueChanged);
			// 
			// numericUpDownOffsetY
			// 
			this.numericUpDownOffsetY.DecimalPlaces = 1;
			this.numericUpDownOffsetY.Location = new System.Drawing.Point(199, 136);
			this.numericUpDownOffsetY.Name = "numericUpDownOffsetY";
			this.numericUpDownOffsetY.Size = new System.Drawing.Size(63, 26);
			this.numericUpDownOffsetY.TabIndex = 8;
			this.numericUpDownOffsetY.ValueChanged += new System.EventHandler(this.NumericUpDownValueChanged);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(173, 138);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(20, 23);
			this.label5.TabIndex = 9;
			this.label5.Text = ":";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(12, 183);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(145, 26);
			this.label6.TabIndex = 10;
			this.label6.Text = "Dimensions [X]:";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(12, 222);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(145, 26);
			this.label7.TabIndex = 11;
			this.label7.Text = "Dimensions [Y]:";
			// 
			// textBoxPreviewX
			// 
			this.textBoxPreviewX.Location = new System.Drawing.Point(149, 183);
			this.textBoxPreviewX.Name = "textBoxPreviewX";
			this.textBoxPreviewX.ReadOnly = true;
			this.textBoxPreviewX.Size = new System.Drawing.Size(113, 26);
			this.textBoxPreviewX.TabIndex = 12;
			// 
			// textBoxPreviewY
			// 
			this.textBoxPreviewY.Location = new System.Drawing.Point(149, 219);
			this.textBoxPreviewY.Name = "textBoxPreviewY";
			this.textBoxPreviewY.ReadOnly = true;
			this.textBoxPreviewY.Size = new System.Drawing.Size(113, 26);
			this.textBoxPreviewY.TabIndex = 13;
			// 
			// btnGenerateProbe
			// 
			this.btnGenerateProbe.Location = new System.Drawing.Point(12, 271);
			this.btnGenerateProbe.Name = "btnGenerateProbe";
			this.btnGenerateProbe.Size = new System.Drawing.Size(177, 35);
			this.btnGenerateProbe.TabIndex = 14;
			this.btnGenerateProbe.Text = "Generate Probe Grid";
			this.btnGenerateProbe.UseVisualStyleBackColor = true;
			this.btnGenerateProbe.Click += new System.EventHandler(this.BtnGenerateProbeClick);
			// 
			// btnGenerateGrid
			// 
			this.btnGenerateGrid.Location = new System.Drawing.Point(199, 271);
			this.btnGenerateGrid.Name = "btnGenerateGrid";
			this.btnGenerateGrid.Size = new System.Drawing.Size(169, 35);
			this.btnGenerateGrid.TabIndex = 15;
			this.btnGenerateGrid.Text = "Generate Drill Grid";
			this.btnGenerateGrid.UseVisualStyleBackColor = true;
			this.btnGenerateGrid.Click += new System.EventHandler(this.BtnGenerateGridClick);
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(278, 98);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(37, 23);
			this.label8.TabIndex = 16;
			this.label8.Text = "mm";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(278, 141);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(37, 23);
			this.label9.TabIndex = 17;
			this.label9.Text = "mm";
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(113, 314);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(169, 35);
			this.btnCancel.TabIndex = 18;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
			// 
			// frmGenerate
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(378, 361);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.btnGenerateGrid);
			this.Controls.Add(this.btnGenerateProbe);
			this.Controls.Add(this.textBoxPreviewY);
			this.Controls.Add(this.textBoxPreviewX);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.numericUpDownOffsetY);
			this.Controls.Add(this.numericUpDownOffsetX);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.numericUpDownGridSize);
			this.Controls.Add(this.numericUpDownPointsY);
			this.Controls.Add(this.numericUpDownPointsX);
			this.Name = "frmGenerate";
			this.Text = "Generate Grid";
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownPointsX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownPointsY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownGridSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownOffsetX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownOffsetY)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
