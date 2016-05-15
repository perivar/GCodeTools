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
			this.panelCode = new System.Windows.Forms.Panel();
			this.treeView = new System.Windows.Forms.TreeView();
			this.panelCheckboxes = new System.Windows.Forms.Panel();
			this.cbSoloSelect = new System.Windows.Forms.CheckBox();
			this.panelCommands = new System.Windows.Forms.Panel();
			this.btnOptimize = new System.Windows.Forms.Button();
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
			this.panelZoomFilename = new System.Windows.Forms.Panel();
			this.txtCoordinates = new System.Windows.Forms.TextBox();
			this.panelViewer = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.panelCode.SuspendLayout();
			this.panelCheckboxes.SuspendLayout();
			this.panelCommands.SuspendLayout();
			this.panelZoomFilename.SuspendLayout();
			this.panelViewer.SuspendLayout();
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
			this.btnParseData.Location = new System.Drawing.Point(10, 37);
			this.btnParseData.Name = "btnParseData";
			this.btnParseData.Size = new System.Drawing.Size(102, 22);
			this.btnParseData.TabIndex = 2;
			this.btnParseData.Text = "Parse";
			this.btnParseData.UseVisualStyleBackColor = true;
			this.btnParseData.Click += new System.EventHandler(this.btnParseDataClick);
			// 
			// btnRedraw
			// 
			this.btnRedraw.Location = new System.Drawing.Point(10, 121);
			this.btnRedraw.Name = "btnRedraw";
			this.btnRedraw.Size = new System.Drawing.Size(102, 22);
			this.btnRedraw.TabIndex = 2;
			this.btnRedraw.Text = "Reset (Redraw)";
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
			this.btnLoad.Size = new System.Drawing.Size(102, 22);
			this.btnLoad.TabIndex = 9;
			this.btnLoad.Text = "Load";
			this.btnLoad.UseVisualStyleBackColor = true;
			this.btnLoad.Click += new System.EventHandler(this.btnLoadClick);
			// 
			// btnSave
			// 
			this.btnSave.Location = new System.Drawing.Point(10, 65);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(102, 22);
			this.btnSave.TabIndex = 9;
			this.btnSave.Text = "Save";
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
			// panelCode
			// 
			this.panelCode.Controls.Add(this.treeView);
			this.panelCode.Controls.Add(this.panelCheckboxes);
			this.panelCode.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelCode.Location = new System.Drawing.Point(542, 5);
			this.panelCode.Name = "panelCode";
			this.panelCode.Size = new System.Drawing.Size(300, 510);
			this.panelCode.TabIndex = 10;
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
			// panelCheckboxes
			// 
			this.panelCheckboxes.Controls.Add(this.cbSoloSelect);
			this.panelCheckboxes.Controls.Add(this.cbRenderG0);
			this.panelCheckboxes.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelCheckboxes.Location = new System.Drawing.Point(0, 477);
			this.panelCheckboxes.Name = "panelCheckboxes";
			this.panelCheckboxes.Size = new System.Drawing.Size(300, 33);
			this.panelCheckboxes.TabIndex = 9;
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
			// panelCommands
			// 
			this.panelCommands.Controls.Add(this.btnOptimize);
			this.panelCommands.Controls.Add(this.btnSaveSplit);
			this.panelCommands.Controls.Add(this.lblZClearance);
			this.panelCommands.Controls.Add(this.txtZClearance);
			this.panelCommands.Controls.Add(this.radRight);
			this.panelCommands.Controls.Add(this.radLeft);
			this.panelCommands.Controls.Add(this.txtSplit);
			this.panelCommands.Controls.Add(this.lblSplit);
			this.panelCommands.Controls.Add(this.btnSplit);
			this.panelCommands.Controls.Add(this.txtDimension);
			this.panelCommands.Controls.Add(this.cmdSaveLayers);
			this.panelCommands.Controls.Add(this.btnSave);
			this.panelCommands.Controls.Add(this.btnLoad);
			this.panelCommands.Controls.Add(this.btnRedraw);
			this.panelCommands.Controls.Add(this.btnParseData);
			this.panelCommands.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelCommands.Location = new System.Drawing.Point(417, 5);
			this.panelCommands.Name = "panelCommands";
			this.panelCommands.Size = new System.Drawing.Size(125, 510);
			this.panelCommands.TabIndex = 11;
			// 
			// btnOptimize
			// 
			this.btnOptimize.Location = new System.Drawing.Point(10, 149);
			this.btnOptimize.Name = "btnOptimize";
			this.btnOptimize.Size = new System.Drawing.Size(102, 22);
			this.btnOptimize.TabIndex = 20;
			this.btnOptimize.Text = "Optimize";
			this.btnOptimize.UseVisualStyleBackColor = true;
			this.btnOptimize.Click += new System.EventHandler(this.BtnOptimizeClick);
			// 
			// btnSaveSplit
			// 
			this.btnSaveSplit.Location = new System.Drawing.Point(10, 341);
			this.btnSaveSplit.Name = "btnSaveSplit";
			this.btnSaveSplit.Size = new System.Drawing.Size(102, 22);
			this.btnSaveSplit.TabIndex = 19;
			this.btnSaveSplit.Text = "Save Both";
			this.btnSaveSplit.UseVisualStyleBackColor = true;
			this.btnSaveSplit.Click += new System.EventHandler(this.btnSaveSplitClick);
			// 
			// lblZClearance
			// 
			this.lblZClearance.Location = new System.Drawing.Point(10, 261);
			this.lblZClearance.Name = "lblZClearance";
			this.lblZClearance.Size = new System.Drawing.Size(80, 19);
			this.lblZClearance.TabIndex = 18;
			this.lblZClearance.Text = "Z-Safe Height:";
			// 
			// txtZClearance
			// 
			this.txtZClearance.Location = new System.Drawing.Point(90, 258);
			this.txtZClearance.Name = "txtZClearance";
			this.txtZClearance.Size = new System.Drawing.Size(22, 20);
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
			this.txtSplit.Location = new System.Drawing.Point(48, 235);
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
			this.btnSplit.Size = new System.Drawing.Size(102, 22);
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
			this.cmdSaveLayers.Location = new System.Drawing.Point(10, 93);
			this.cmdSaveLayers.Name = "cmdSaveLayers";
			this.cmdSaveLayers.Size = new System.Drawing.Size(102, 22);
			this.cmdSaveLayers.TabIndex = 9;
			this.cmdSaveLayers.Text = "Save w/ Layers";
			this.cmdSaveLayers.UseVisualStyleBackColor = true;
			this.cmdSaveLayers.Click += new System.EventHandler(this.btnSaveLayersClick);
			// 
			// txtFile
			// 
			this.txtFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.txtFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtFile.Location = new System.Drawing.Point(16, 11);
			this.txtFile.Name = "txtFile";
			this.txtFile.ReadOnly = true;
			this.txtFile.Size = new System.Drawing.Size(266, 20);
			this.txtFile.TabIndex = 10;
			// 
			// panelZoomFilename
			// 
			this.panelZoomFilename.Controls.Add(this.txtCoordinates);
			this.panelZoomFilename.Controls.Add(this.txtFile);
			this.panelZoomFilename.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelZoomFilename.Location = new System.Drawing.Point(5, 477);
			this.panelZoomFilename.Name = "panelZoomFilename";
			this.panelZoomFilename.Size = new System.Drawing.Size(412, 38);
			this.panelZoomFilename.TabIndex = 12;
			// 
			// txtCoordinates
			// 
			this.txtCoordinates.BackColor = System.Drawing.SystemColors.Control;
			this.txtCoordinates.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtCoordinates.Location = new System.Drawing.Point(299, 11);
			this.txtCoordinates.Name = "txtCoordinates";
			this.txtCoordinates.Size = new System.Drawing.Size(107, 20);
			this.txtCoordinates.TabIndex = 11;
			// 
			// panelViewer
			// 
			this.panelViewer.AutoScroll = true;
			this.panelViewer.Controls.Add(this.pictureBox1);
			this.panelViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelViewer.Location = new System.Drawing.Point(5, 5);
			this.panelViewer.Name = "panelViewer";
			this.panelViewer.Size = new System.Drawing.Size(412, 472);
			this.panelViewer.TabIndex = 13;
			this.panelViewer.Scroll += new System.Windows.Forms.ScrollEventHandler(this.PanelViewerScroll);
			// 
			// frmPlotter
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(847, 520);
			this.Controls.Add(this.panelViewer);
			this.Controls.Add(this.panelZoomFilename);
			this.Controls.Add(this.panelCommands);
			this.Controls.Add(this.panelCode);
			this.Name = "frmPlotter";
			this.Padding = new System.Windows.Forms.Padding(5);
			this.Text = "GCode Viewer";
			this.Load += new System.EventHandler(this.frmPlotterLoad);
			this.ResizeEnd += new System.EventHandler(this.frmPlotterResizeEnd);
			this.ClientSizeChanged += new System.EventHandler(this.frmPlotterResizeEnd);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.panelCode.ResumeLayout(false);
			this.panelCheckboxes.ResumeLayout(false);
			this.panelCheckboxes.PerformLayout();
			this.panelCommands.ResumeLayout(false);
			this.panelCommands.PerformLayout();
			this.panelZoomFilename.ResumeLayout(false);
			this.panelZoomFilename.PerformLayout();
			this.panelViewer.ResumeLayout(false);
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
		private System.Windows.Forms.Panel panelCode;
		private System.Windows.Forms.Panel panelCheckboxes;
		private System.Windows.Forms.Panel panelCommands;
		private System.Windows.Forms.TextBox txtFile;
		private System.Windows.Forms.Panel panelZoomFilename;
		private System.Windows.Forms.Panel panelViewer;
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
		private System.Windows.Forms.Button btnOptimize;
		private System.Windows.Forms.TextBox txtCoordinates;
	}
}

