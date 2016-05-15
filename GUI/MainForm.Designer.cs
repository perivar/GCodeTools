/*
 * Created by SharpDevelop.
 * User: perivar.nerseth
 * Date: 07/04/2016
 * Time: 14:37
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace GCodeOptimizer
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Panel pnlTop;
		private System.Windows.Forms.Button btnStartStop;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel pnlViewer;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.RadioButton radScaleHalf;
		private System.Windows.Forms.RadioButton radScaleOne;
		private System.Windows.Forms.RadioButton radScaleTwo;
		private System.Windows.Forms.RadioButton radScaleFour;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Panel pnlBottom;
		
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
			this.pnlTop = new System.Windows.Forms.Panel();
			this.btnStartStop = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.pnlViewer = new System.Windows.Forms.Panel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.radScaleHalf = new System.Windows.Forms.RadioButton();
			this.radScaleOne = new System.Windows.Forms.RadioButton();
			this.radScaleTwo = new System.Windows.Forms.RadioButton();
			this.radScaleFour = new System.Windows.Forms.RadioButton();
			this.btnSave = new System.Windows.Forms.Button();
			this.pnlBottom = new System.Windows.Forms.Panel();
			this.pnlTop.SuspendLayout();
			this.pnlViewer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.pnlBottom.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlTop
			// 
			this.pnlTop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.pnlTop.Controls.Add(this.btnStartStop);
			this.pnlTop.Controls.Add(this.label1);
			this.pnlTop.Location = new System.Drawing.Point(1, 1);
			this.pnlTop.Name = "pnlTop";
			this.pnlTop.Size = new System.Drawing.Size(642, 30);
			this.pnlTop.TabIndex = 0;
			// 
			// btnStartStop
			// 
			this.btnStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStartStop.Location = new System.Drawing.Point(564, 3);
			this.btnStartStop.Name = "btnStartStop";
			this.btnStartStop.Size = new System.Drawing.Size(75, 23);
			this.btnStartStop.TabIndex = 1;
			this.btnStartStop.Text = "Start";
			this.btnStartStop.UseVisualStyleBackColor = true;
			this.btnStartStop.Click += new System.EventHandler(this.BtnStartStopClick);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(11, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(487, 23);
			this.label1.TabIndex = 0;
			this.label1.Text = "label1";
			// 
			// pnlViewer
			// 
			this.pnlViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.pnlViewer.AutoScroll = true;
			this.pnlViewer.Controls.Add(this.pictureBox1);
			this.pnlViewer.Location = new System.Drawing.Point(1, 30);
			this.pnlViewer.Name = "pnlViewer";
			this.pnlViewer.Size = new System.Drawing.Size(642, 311);
			this.pnlViewer.TabIndex = 1;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(3, 3);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(500, 200);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// radScaleHalf
			// 
			this.radScaleHalf.Location = new System.Drawing.Point(11, 2);
			this.radScaleHalf.Name = "radScaleHalf";
			this.radScaleHalf.Size = new System.Drawing.Size(47, 24);
			this.radScaleHalf.TabIndex = 2;
			this.radScaleHalf.Text = "0.5x";
			this.radScaleHalf.UseVisualStyleBackColor = true;
			this.radScaleHalf.CheckedChanged += new System.EventHandler(this.RadScaleCheckedChanged);
			// 
			// radScaleOne
			// 
			this.radScaleOne.Location = new System.Drawing.Point(64, 2);
			this.radScaleOne.Name = "radScaleOne";
			this.radScaleOne.Size = new System.Drawing.Size(36, 24);
			this.radScaleOne.TabIndex = 3;
			this.radScaleOne.Text = "1x";
			this.radScaleOne.UseVisualStyleBackColor = true;
			this.radScaleOne.CheckedChanged += new System.EventHandler(this.RadScaleCheckedChanged);
			// 
			// radScaleTwo
			// 
			this.radScaleTwo.Checked = true;
			this.radScaleTwo.Location = new System.Drawing.Point(106, 2);
			this.radScaleTwo.Name = "radScaleTwo";
			this.radScaleTwo.Size = new System.Drawing.Size(36, 24);
			this.radScaleTwo.TabIndex = 4;
			this.radScaleTwo.TabStop = true;
			this.radScaleTwo.Text = "2x";
			this.radScaleTwo.UseVisualStyleBackColor = true;
			this.radScaleTwo.CheckedChanged += new System.EventHandler(this.RadScaleCheckedChanged);
			// 
			// radScaleFour
			// 
			this.radScaleFour.Location = new System.Drawing.Point(148, 2);
			this.radScaleFour.Name = "radScaleFour";
			this.radScaleFour.Size = new System.Drawing.Size(36, 24);
			this.radScaleFour.TabIndex = 5;
			this.radScaleFour.Text = "4x";
			this.radScaleFour.UseVisualStyleBackColor = true;
			this.radScaleFour.CheckedChanged += new System.EventHandler(this.RadScaleCheckedChanged);
			// 
			// btnSave
			// 
			this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSave.Location = new System.Drawing.Point(559, 2);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(75, 23);
			this.btnSave.TabIndex = 6;
			this.btnSave.Text = "Save";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.BtnSaveClick);
			// 
			// pnlBottom
			// 
			this.pnlBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.pnlBottom.Controls.Add(this.radScaleHalf);
			this.pnlBottom.Controls.Add(this.btnSave);
			this.pnlBottom.Controls.Add(this.radScaleOne);
			this.pnlBottom.Controls.Add(this.radScaleFour);
			this.pnlBottom.Controls.Add(this.radScaleTwo);
			this.pnlBottom.Location = new System.Drawing.Point(1, 342);
			this.pnlBottom.Name = "pnlBottom";
			this.pnlBottom.Size = new System.Drawing.Size(639, 29);
			this.pnlBottom.TabIndex = 7;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(644, 374);
			this.Controls.Add(this.pnlBottom);
			this.Controls.Add(this.pnlViewer);
			this.Controls.Add(this.pnlTop);
			this.Name = "MainForm";
			this.Text = "GCodeOptimizer";
			this.pnlTop.ResumeLayout(false);
			this.pnlViewer.ResumeLayout(false);
			this.pnlViewer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.pnlBottom.ResumeLayout(false);
			this.ResumeLayout(false);

		}
	}
}
