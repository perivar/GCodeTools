/**
 * Copyright (c) David-John Miller AKA Anoyomouse 2014
 *
 * See LICENCE in the project directory for licence information
 **/
namespace GCodePlotter
{
	partial class frmPlotter
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.btnParseData = new System.Windows.Forms.Button();
			this.btnRedraw = new System.Windows.Forms.Button();
			this.cbRenderG0 = new System.Windows.Forms.CheckBox();
			this.btnLoad = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.sfdSaveDialog = new System.Windows.Forms.SaveFileDialog();
			this.ofdLoadDialog = new System.Windows.Forms.OpenFileDialog();
			this.panel1 = new System.Windows.Forms.Panel();
			this.treeView = new System.Windows.Forms.TreeView();
			this.panel2 = new System.Windows.Forms.Panel();
			this.cbSoloSelect = new System.Windows.Forms.CheckBox();
			this.panel4 = new System.Windows.Forms.Panel();
			this.btnSaveSplit = new System.Windows.Forms.Button();
			this.lblZClearance = new System.Windows.Forms.Label();
			this.txtZClearance = new System.Windows.Forms.TextBox();
			this.radRight = new System.Windows.Forms.RadioButton();
			this.radLeft = new System.Windows.Forms.RadioButton();
			this.txtSplit = new System.Windows.Forms.TextBox();
			this.lblSplit = new System.Windows.Forms.Label();
			this.btnSplit = new System.Windows.Forms.Button();
			this.txtDimension = new System.Windows.Forms.TextBox();
			this.cmdSaveLayers = new System.Windows.Forms.Button();
			this.txtFile = new System.Windows.Forms.TextBox();
			this.panel5 = new System.Windows.Forms.Panel();
			this.radZoomSixteen = new System.Windows.Forms.RadioButton();
			this.radZoomEight = new System.Windows.Forms.RadioButton();
			this.radZoomFour = new System.Windows.Forms.RadioButton();
			this.radZoomTwo = new System.Windows.Forms.RadioButton();
			this.panel6 = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel4.SuspendLayout();
			this.panel5.SuspendLayout();
			this.panel6.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox1.Location = new System.Drawing.Point(0, 0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(363, 465);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBox1MouseDown);
			this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBox1MouseMove);
			// 
			// btnParseData
			// 
			this.btnParseData.Enabled = false;
			this.btnParseData.Location = new System.Drawing.Point(10, 47);
			this.btnParseData.Name = "btnParseData";
			this.btnParseData.Size = new System.Drawing.Size(102, 32);
			this.btnParseData.TabIndex = 2;
			this.btnParseData.Text = "Parse";
			this.btnParseData.UseVisualStyleBackColor = true;
			this.btnParseData.Click += new System.EventHandler(this.btnParseDataClick);
			// 
			// btnRedraw
			// 
			this.btnRedraw.Location = new System.Drawing.Point(10, 161);
			this.btnRedraw.Name = "btnRedraw";
			this.btnRedraw.Size = new System.Drawing.Size(102, 32);
			this.btnRedraw.TabIndex = 2;
			this.btnRedraw.Text = "Reset (redraw)";
			this.btnRedraw.UseVisualStyleBackColor = true;
			this.btnRedraw.Click += new System.EventHandler(this.btnRedrawClick);
			// 
			// cbRenderG0
			// 
			this.cbRenderG0.AutoSize = true;
			this.cbRenderG0.Checked = true;
			this.cbRenderG0.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbRenderG0.Location = new System.Drawing.Point(7, 8);
			this.cbRenderG0.Name = "cbRenderG0";
			this.cbRenderG0.Size = new System.Drawing.Size(83, 17);
			this.cbRenderG0.TabIndex = 5;
			this.cbRenderG0.Text = "Render G0s";
			this.cbRenderG0.UseVisualStyleBackColor = true;
			this.cbRenderG0.CheckedChanged += new System.EventHandler(this.cbRenderG0CheckedChanged);
			// 
			// btnLoad
			// 
			this.btnLoad.Location = new System.Drawing.Point(10, 9);
			this.btnLoad.Name = "btnLoad";
			this.btnLoad.Size = new System.Drawing.Size(102, 32);
			this.btnLoad.TabIndex = 9;
			this.btnLoad.Text = "Load Data";
			this.btnLoad.UseVisualStyleBackColor = true;
			this.btnLoad.Click += new System.EventHandler(this.btnLoadClick);
			// 
			// btnSave
			// 
			this.btnSave.Location = new System.Drawing.Point(10, 85);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(102, 32);
			this.btnSave.TabIndex = 9;
			this.btnSave.Text = "Save Data";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSaveClick);
			// 
			// sfdSaveDialog
			// 
			this.sfdSaveDialog.DefaultExt = "gcode";
			this.sfdSaveDialog.Filter = "GCode Files|*.gcode|All Files|*.*";
			// 
			// ofdLoadDialog
			// 
			this.ofdLoadDialog.DefaultExt = "gcode";
			this.ofdLoadDialog.Filter = "GCode Files|*.gcode;*.nc;*.ngc|All Files|*.*";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.treeView);
			this.panel1.Controls.Add(this.panel2);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel1.Location = new System.Drawing.Point(542, 5);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(300, 510);
			this.panel1.TabIndex = 10;
			// 
			// treeView
			// 
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.HideSelection = false;
			this.treeView.Location = new System.Drawing.Point(0, 0);
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(300, 477);
			this.treeView.TabIndex = 10;
			this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewAfterSelect);
			this.treeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeViewMouseDown);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.cbSoloSelect);
			this.panel2.Controls.Add(this.cbRenderG0);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(0, 477);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(300, 33);
			this.panel2.TabIndex = 9;
			// 
			// cbSoloSelect
			// 
			this.cbSoloSelect.AutoSize = true;
			this.cbSoloSelect.Location = new System.Drawing.Point(205, 8);
			this.cbSoloSelect.Name = "cbSoloSelect";
			this.cbSoloSelect.Size = new System.Drawing.Size(80, 17);
			this.cbSoloSelect.TabIndex = 6;
			this.cbSoloSelect.Text = "Solo Select";
			this.cbSoloSelect.UseVisualStyleBackColor = true;
			this.cbSoloSelect.CheckedChanged += new System.EventHandler(this.cbSoloSelectCheckedChanged);
			// 
			// panel4
			// 
			this.panel4.Controls.Add(this.btnSaveSplit);
			this.panel4.Controls.Add(this.lblZClearance);
			this.panel4.Controls.Add(this.txtZClearance);
			this.panel4.Controls.Add(this.radRight);
			this.panel4.Controls.Add(this.radLeft);
			this.panel4.Controls.Add(this.txtSplit);
			this.panel4.Controls.Add(this.lblSplit);
			this.panel4.Controls.Add(this.btnSplit);
			this.panel4.Controls.Add(this.txtDimension);
			this.panel4.Controls.Add(this.cmdSaveLayers);
			this.panel4.Controls.Add(this.btnSave);
			this.panel4.Controls.Add(this.btnLoad);
			this.panel4.Controls.Add(this.btnRedraw);
			this.panel4.Controls.Add(this.btnParseData);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel4.Location = new System.Drawing.Point(417, 5);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(125, 510);
			this.panel4.TabIndex = 11;
			// 
			// btnSaveSplit
			// 
			this.btnSaveSplit.Location = new System.Drawing.Point(10, 351);
			this.btnSaveSplit.Name = "btnSaveSplit";
			this.btnSaveSplit.Size = new System.Drawing.Size(102, 32);
			this.btnSaveSplit.TabIndex = 19;
			this.btnSaveSplit.Text = "Save Both";
			this.btnSaveSplit.UseVisualStyleBackColor = true;
			this.btnSaveSplit.Click += new System.EventHandler(this.btnSaveSplitClick);
			// 
			// lblZClearance
			// 
			this.lblZClearance.Location = new System.Drawing.Point(10, 261);
			this.lblZClearance.Name = "lblZClearance";
			this.lblZClearance.Size = new System.Drawing.Size(68, 19);
			this.lblZClearance.TabIndex = 18;
			this.lblZClearance.Text = "Z-Clearance:";
			// 
			// txtZClearance
			// 
			this.txtZClearance.Location = new System.Drawing.Point(80, 258);
			this.txtZClearance.Name = "txtZClearance";
			this.txtZClearance.Size = new System.Drawing.Size(29, 20);
			this.txtZClearance.TabIndex = 17;
			this.txtZClearance.Text = "2.0";
			// 
			// radRight
			// 
			this.radRight.Location = new System.Drawing.Point(61, 283);
			this.radRight.Name = "radRight";
			this.radRight.Size = new System.Drawing.Size(58, 24);
			this.radRight.TabIndex = 16;
			this.radRight.TabStop = true;
			this.radRight.Text = "Right";
			this.radRight.UseVisualStyleBackColor = true;
			// 
			// radLeft
			// 
			this.radLeft.Checked = true;
			this.radLeft.Location = new System.Drawing.Point(10, 283);
			this.radLeft.Name = "radLeft";
			this.radLeft.Size = new System.Drawing.Size(45, 24);
			this.radLeft.TabIndex = 15;
			this.radLeft.TabStop = true;
			this.radLeft.Text = "Left";
			this.radLeft.UseVisualStyleBackColor = true;
			// 
			// txtSplit
			// 
			this.txtSplit.Location = new System.Drawing.Point(45, 235);
			this.txtSplit.Name = "txtSplit";
			this.txtSplit.Size = new System.Drawing.Size(64, 20);
			this.txtSplit.TabIndex = 14;
			// 
			// lblSplit
			// 
			this.lblSplit.Location = new System.Drawing.Point(10, 238);
			this.lblSplit.Name = "lblSplit";
			this.lblSplit.Size = new System.Drawing.Size(100, 16);
			this.lblSplit.TabIndex = 13;
			this.lblSplit.Text = "Split:";
			// 
			// btnSplit
			// 
			this.btnSplit.Location = new System.Drawing.Point(10, 313);
			this.btnSplit.Name = "btnSplit";
			this.btnSplit.Size = new System.Drawing.Size(102, 32);
			this.btnSplit.TabIndex = 12;
			this.btnSplit.Text = "Split";
			this.btnSplit.UseVisualStyleBackColor = true;
			this.btnSplit.Click += new System.EventHandler(this.btnSplitClick);
			// 
			// txtDimension
			// 
			this.txtDimension.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.txtDimension.BackColor = System.Drawing.SystemColors.Control;
			this.txtDimension.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtDimension.Location = new System.Drawing.Point(3, 421);
			this.txtDimension.Multiline = true;
			this.txtDimension.Name = "txtDimension";
			this.txtDimension.ReadOnly = true;
			this.txtDimension.Size = new System.Drawing.Size(119, 86);
			this.txtDimension.TabIndex = 11;
			// 
			// cmdSaveLayers
			// 
			this.cmdSaveLayers.Location = new System.Drawing.Point(10, 123);
			this.cmdSaveLayers.Name = "cmdSaveLayers";
			this.cmdSaveLayers.Size = new System.Drawing.Size(102, 32);
			this.cmdSaveLayers.TabIndex = 9;
			this.cmdSaveLayers.Text = "Save Data (layers)";
			this.cmdSaveLayers.UseVisualStyleBackColor = true;
			this.cmdSaveLayers.Click += new System.EventHandler(this.btnSaveLayersClick);
			// 
			// txtFile
			// 
			this.txtFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.txtFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtFile.Location = new System.Drawing.Point(14, 11);
			this.txtFile.Name = "txtFile";
			this.txtFile.ReadOnly = true;
			this.txtFile.Size = new System.Drawing.Size(380, 20);
			this.txtFile.TabIndex = 10;
			// 
			// panel5
			// 
			this.panel5.Controls.Add(this.radZoomSixteen);
			this.panel5.Controls.Add(this.txtFile);
			this.panel5.Controls.Add(this.radZoomEight);
			this.panel5.Controls.Add(this.radZoomFour);
			this.panel5.Controls.Add(this.radZoomTwo);
			this.panel5.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel5.Location = new System.Drawing.Point(5, 477);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(412, 38);
			this.panel5.TabIndex = 12;
			// 
			// radZoomSixteen
			// 
			this.radZoomSixteen.AutoSize = true;
			this.radZoomSixteen.Location = new System.Drawing.Point(149, 12);
			this.radZoomSixteen.Name = "radZoomSixteen";
			this.radZoomSixteen.Size = new System.Drawing.Size(36, 17);
			this.radZoomSixteen.TabIndex = 0;
			this.radZoomSixteen.Text = "x4";
			this.radZoomSixteen.UseVisualStyleBackColor = true;
			this.radZoomSixteen.Visible = false;
			this.radZoomSixteen.CheckedChanged += new System.EventHandler(this.radScaleChange);
			// 
			// radZoomEight
			// 
			this.radZoomEight.AutoSize = true;
			this.radZoomEight.Location = new System.Drawing.Point(107, 12);
			this.radZoomEight.Name = "radZoomEight";
			this.radZoomEight.Size = new System.Drawing.Size(36, 17);
			this.radZoomEight.TabIndex = 0;
			this.radZoomEight.Text = "x2";
			this.radZoomEight.UseVisualStyleBackColor = true;
			this.radZoomEight.Visible = false;
			this.radZoomEight.CheckedChanged += new System.EventHandler(this.radScaleChange);
			// 
			// radZoomFour
			// 
			this.radZoomFour.AutoSize = true;
			this.radZoomFour.Checked = true;
			this.radZoomFour.Location = new System.Drawing.Point(65, 12);
			this.radZoomFour.Name = "radZoomFour";
			this.radZoomFour.Size = new System.Drawing.Size(36, 17);
			this.radZoomFour.TabIndex = 0;
			this.radZoomFour.TabStop = true;
			this.radZoomFour.Text = "x1";
			this.radZoomFour.UseVisualStyleBackColor = true;
			this.radZoomFour.Visible = false;
			this.radZoomFour.CheckedChanged += new System.EventHandler(this.radScaleChange);
			// 
			// radZoomTwo
			// 
			this.radZoomTwo.AutoSize = true;
			this.radZoomTwo.Location = new System.Drawing.Point(14, 12);
			this.radZoomTwo.Name = "radZoomTwo";
			this.radZoomTwo.Size = new System.Drawing.Size(45, 17);
			this.radZoomTwo.TabIndex = 0;
			this.radZoomTwo.Text = "x0.5";
			this.radZoomTwo.UseVisualStyleBackColor = true;
			this.radZoomTwo.Visible = false;
			this.radZoomTwo.CheckedChanged += new System.EventHandler(this.radScaleChange);
			// 
			// panel6
			// 
			this.panel6.Controls.Add(this.pictureBox1);
			this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel6.Location = new System.Drawing.Point(5, 5);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(412, 472);
			this.panel6.TabIndex = 13;
			// 
			// frmPlotter
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(847, 520);
			this.Controls.Add(this.panel6);
			this.Controls.Add(this.panel5);
			this.Controls.Add(this.panel4);
			this.Controls.Add(this.panel1);
			this.Name = "frmPlotter";
			this.Padding = new System.Windows.Forms.Padding(5);
			this.Text = "GCode Viewer";
			this.Load += new System.EventHandler(this.frmPlotterLoad);
			this.ResizeEnd += new System.EventHandler(this.frmPlotterResizeEnd);
			this.ClientSizeChanged += new System.EventHandler(this.frmPlotterResizeEnd);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panel4.ResumeLayout(false);
			this.panel4.PerformLayout();
			this.panel5.ResumeLayout(false);
			this.panel5.PerformLayout();
			this.panel6.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Button btnParseData;
		private System.Windows.Forms.Button btnRedraw;
		private System.Windows.Forms.CheckBox cbRenderG0;
		private System.Windows.Forms.Button btnLoad;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button cmdSaveLayers;
		private System.Windows.Forms.SaveFileDialog sfdSaveDialog;
		private System.Windows.Forms.OpenFileDialog ofdLoadDialog;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.TextBox txtFile;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.RadioButton radZoomFour;
		private System.Windows.Forms.RadioButton radZoomTwo;
		private System.Windows.Forms.RadioButton radZoomEight;
		private System.Windows.Forms.RadioButton radZoomSixteen;
		private System.Windows.Forms.Panel panel6;
		private System.Windows.Forms.TextBox txtDimension;
		private System.Windows.Forms.TreeView treeView;
		private System.Windows.Forms.Button btnSplit;
		private System.Windows.Forms.TextBox txtSplit;
		private System.Windows.Forms.Label lblSplit;
		private System.Windows.Forms.RadioButton radRight;
		private System.Windows.Forms.RadioButton radLeft;
		private System.Windows.Forms.Label lblZClearance;
		private System.Windows.Forms.TextBox txtZClearance;
		private System.Windows.Forms.Button btnSaveSplit;
		private System.Windows.Forms.CheckBox cbSoloSelect;
	}
}

