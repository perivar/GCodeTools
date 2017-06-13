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
			this.btnLoadSVG = new System.Windows.Forms.Button();
			this.btnShift = new System.Windows.Forms.Button();
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
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(544, 714);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBox1MouseDown);
			this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBox1MouseMove);
			// 
			// btnParseData
			// 
			this.btnParseData.Enabled = false;
			this.btnParseData.Location = new System.Drawing.Point(15, 57);
			this.btnParseData.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnParseData.Name = "btnParseData";
			this.btnParseData.Size = new System.Drawing.Size(153, 34);
			this.btnParseData.TabIndex = 2;
			this.btnParseData.Text = "Parse";
			this.btnParseData.UseVisualStyleBackColor = true;
			this.btnParseData.Click += new System.EventHandler(this.btnParseDataClick);
			// 
			// btnRedraw
			// 
			this.btnRedraw.Location = new System.Drawing.Point(15, 186);
			this.btnRedraw.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnRedraw.Name = "btnRedraw";
			this.btnRedraw.Size = new System.Drawing.Size(153, 34);
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
			this.cbRenderG0.Location = new System.Drawing.Point(10, 12);
			this.cbRenderG0.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cbRenderG0.Name = "cbRenderG0";
			this.cbRenderG0.Size = new System.Drawing.Size(122, 24);
			this.cbRenderG0.TabIndex = 5;
			this.cbRenderG0.Text = "Render G0s";
			this.cbRenderG0.UseVisualStyleBackColor = true;
			this.cbRenderG0.CheckedChanged += new System.EventHandler(this.cbRenderG0CheckedChanged);
			// 
			// btnLoad
			// 
			this.btnLoad.Location = new System.Drawing.Point(15, 14);
			this.btnLoad.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnLoad.Name = "btnLoad";
			this.btnLoad.Size = new System.Drawing.Size(153, 34);
			this.btnLoad.TabIndex = 9;
			this.btnLoad.Text = "Load";
			this.btnLoad.UseVisualStyleBackColor = true;
			this.btnLoad.Click += new System.EventHandler(this.btnLoadClick);
			// 
			// btnSave
			// 
			this.btnSave.Location = new System.Drawing.Point(15, 100);
			this.btnSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(153, 34);
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
			this.panelCode.Location = new System.Drawing.Point(812, 8);
			this.panelCode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panelCode.Name = "panelCode";
			this.panelCode.Size = new System.Drawing.Size(450, 784);
			this.panelCode.TabIndex = 10;
			// 
			// treeView
			// 
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.HideSelection = false;
			this.treeView.Location = new System.Drawing.Point(0, 0);
			this.treeView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(450, 733);
			this.treeView.TabIndex = 10;
			this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewAfterSelect);
			this.treeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeViewMouseDown);
			// 
			// panelCheckboxes
			// 
			this.panelCheckboxes.Controls.Add(this.cbSoloSelect);
			this.panelCheckboxes.Controls.Add(this.cbRenderG0);
			this.panelCheckboxes.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelCheckboxes.Location = new System.Drawing.Point(0, 733);
			this.panelCheckboxes.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panelCheckboxes.Name = "panelCheckboxes";
			this.panelCheckboxes.Size = new System.Drawing.Size(450, 51);
			this.panelCheckboxes.TabIndex = 9;
			// 
			// cbSoloSelect
			// 
			this.cbSoloSelect.AutoSize = true;
			this.cbSoloSelect.Location = new System.Drawing.Point(308, 12);
			this.cbSoloSelect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cbSoloSelect.Name = "cbSoloSelect";
			this.cbSoloSelect.Size = new System.Drawing.Size(116, 24);
			this.cbSoloSelect.TabIndex = 6;
			this.cbSoloSelect.Text = "Solo Select";
			this.cbSoloSelect.UseVisualStyleBackColor = true;
			this.cbSoloSelect.CheckedChanged += new System.EventHandler(this.cbSoloSelectCheckedChanged);
			// 
			// panelCommands
			// 
			this.panelCommands.Controls.Add(this.btnLoadSVG);
			this.panelCommands.Controls.Add(this.btnShift);
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
			this.panelCommands.Location = new System.Drawing.Point(624, 8);
			this.panelCommands.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panelCommands.Name = "panelCommands";
			this.panelCommands.Size = new System.Drawing.Size(188, 784);
			this.panelCommands.TabIndex = 11;
			// 
			// btnLoadSVG
			// 
			this.btnLoadSVG.Location = new System.Drawing.Point(15, 317);
			this.btnLoadSVG.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnLoadSVG.Name = "btnLoadSVG";
			this.btnLoadSVG.Size = new System.Drawing.Size(153, 34);
			this.btnLoadSVG.TabIndex = 22;
			this.btnLoadSVG.Text = "SVG Load";
			this.btnLoadSVG.UseVisualStyleBackColor = true;
			this.btnLoadSVG.Click += new System.EventHandler(this.BtnSVGLoadClick);
			// 
			// btnShift
			// 
			this.btnShift.Location = new System.Drawing.Point(15, 273);
			this.btnShift.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnShift.Name = "btnShift";
			this.btnShift.Size = new System.Drawing.Size(153, 34);
			this.btnShift.TabIndex = 21;
			this.btnShift.Text = "Shift";
			this.btnShift.UseVisualStyleBackColor = true;
			this.btnShift.Click += new System.EventHandler(this.BtnShiftClick);
			// 
			// btnOptimize
			// 
			this.btnOptimize.Location = new System.Drawing.Point(15, 229);
			this.btnOptimize.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnOptimize.Name = "btnOptimize";
			this.btnOptimize.Size = new System.Drawing.Size(153, 34);
			this.btnOptimize.TabIndex = 20;
			this.btnOptimize.Text = "Optimize";
			this.btnOptimize.UseVisualStyleBackColor = true;
			this.btnOptimize.Click += new System.EventHandler(this.BtnOptimizeClick);
			// 
			// btnSaveSplit
			// 
			this.btnSaveSplit.Location = new System.Drawing.Point(15, 525);
			this.btnSaveSplit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnSaveSplit.Name = "btnSaveSplit";
			this.btnSaveSplit.Size = new System.Drawing.Size(153, 34);
			this.btnSaveSplit.TabIndex = 19;
			this.btnSaveSplit.Text = "Save Both";
			this.btnSaveSplit.UseVisualStyleBackColor = true;
			this.btnSaveSplit.Click += new System.EventHandler(this.btnSaveSplitClick);
			// 
			// lblZClearance
			// 
			this.lblZClearance.Location = new System.Drawing.Point(15, 402);
			this.lblZClearance.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblZClearance.Name = "lblZClearance";
			this.lblZClearance.Size = new System.Drawing.Size(120, 29);
			this.lblZClearance.TabIndex = 18;
			this.lblZClearance.Text = "Z-Safe Height:";
			// 
			// txtZClearance
			// 
			this.txtZClearance.Location = new System.Drawing.Point(135, 397);
			this.txtZClearance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtZClearance.Name = "txtZClearance";
			this.txtZClearance.Size = new System.Drawing.Size(31, 26);
			this.txtZClearance.TabIndex = 17;
			this.txtZClearance.Text = "2.0";
			// 
			// radRight
			// 
			this.radRight.Location = new System.Drawing.Point(92, 435);
			this.radRight.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.radRight.Name = "radRight";
			this.radRight.Size = new System.Drawing.Size(87, 37);
			this.radRight.TabIndex = 16;
			this.radRight.TabStop = true;
			this.radRight.Text = "Right";
			this.radRight.UseVisualStyleBackColor = true;
			// 
			// radLeft
			// 
			this.radLeft.Checked = true;
			this.radLeft.Location = new System.Drawing.Point(15, 435);
			this.radLeft.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.radLeft.Name = "radLeft";
			this.radLeft.Size = new System.Drawing.Size(68, 37);
			this.radLeft.TabIndex = 15;
			this.radLeft.TabStop = true;
			this.radLeft.Text = "Left";
			this.radLeft.UseVisualStyleBackColor = true;
			// 
			// txtSplit
			// 
			this.txtSplit.Location = new System.Drawing.Point(72, 362);
			this.txtSplit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtSplit.Name = "txtSplit";
			this.txtSplit.Size = new System.Drawing.Size(94, 26);
			this.txtSplit.TabIndex = 14;
			// 
			// lblSplit
			// 
			this.lblSplit.Location = new System.Drawing.Point(15, 366);
			this.lblSplit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblSplit.Name = "lblSplit";
			this.lblSplit.Size = new System.Drawing.Size(150, 25);
			this.lblSplit.TabIndex = 13;
			this.lblSplit.Text = "Split:";
			// 
			// btnSplit
			// 
			this.btnSplit.Location = new System.Drawing.Point(15, 482);
			this.btnSplit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnSplit.Name = "btnSplit";
			this.btnSplit.Size = new System.Drawing.Size(153, 34);
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
			this.txtDimension.Location = new System.Drawing.Point(4, 647);
			this.txtDimension.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtDimension.Multiline = true;
			this.txtDimension.Name = "txtDimension";
			this.txtDimension.ReadOnly = true;
			this.txtDimension.Size = new System.Drawing.Size(178, 131);
			this.txtDimension.TabIndex = 11;
			// 
			// cmdSaveLayers
			// 
			this.cmdSaveLayers.Location = new System.Drawing.Point(15, 143);
			this.cmdSaveLayers.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cmdSaveLayers.Name = "cmdSaveLayers";
			this.cmdSaveLayers.Size = new System.Drawing.Size(153, 34);
			this.cmdSaveLayers.TabIndex = 9;
			this.cmdSaveLayers.Text = "Peck Drilling Save";
			this.cmdSaveLayers.UseVisualStyleBackColor = true;
			this.cmdSaveLayers.Click += new System.EventHandler(this.btnSaveLayersClick);
			// 
			// txtFile
			// 
			this.txtFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.txtFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtFile.Location = new System.Drawing.Point(24, 17);
			this.txtFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtFile.Name = "txtFile";
			this.txtFile.ReadOnly = true;
			this.txtFile.Size = new System.Drawing.Size(396, 26);
			this.txtFile.TabIndex = 10;
			// 
			// panelZoomFilename
			// 
			this.panelZoomFilename.Controls.Add(this.txtCoordinates);
			this.panelZoomFilename.Controls.Add(this.txtFile);
			this.panelZoomFilename.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelZoomFilename.Location = new System.Drawing.Point(8, 734);
			this.panelZoomFilename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panelZoomFilename.Name = "panelZoomFilename";
			this.panelZoomFilename.Size = new System.Drawing.Size(616, 58);
			this.panelZoomFilename.TabIndex = 12;
			// 
			// txtCoordinates
			// 
			this.txtCoordinates.BackColor = System.Drawing.SystemColors.Control;
			this.txtCoordinates.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtCoordinates.Location = new System.Drawing.Point(448, 17);
			this.txtCoordinates.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtCoordinates.Name = "txtCoordinates";
			this.txtCoordinates.Size = new System.Drawing.Size(160, 26);
			this.txtCoordinates.TabIndex = 11;
			// 
			// panelViewer
			// 
			this.panelViewer.AutoScroll = true;
			this.panelViewer.Controls.Add(this.pictureBox1);
			this.panelViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelViewer.Location = new System.Drawing.Point(8, 8);
			this.panelViewer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panelViewer.Name = "panelViewer";
			this.panelViewer.Size = new System.Drawing.Size(616, 726);
			this.panelViewer.TabIndex = 13;
			this.panelViewer.Scroll += new System.Windows.Forms.ScrollEventHandler(this.PanelViewerScroll);
			// 
			// frmPlotter
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1270, 800);
			this.Controls.Add(this.panelViewer);
			this.Controls.Add(this.panelZoomFilename);
			this.Controls.Add(this.panelCommands);
			this.Controls.Add(this.panelCode);
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "frmPlotter";
			this.Padding = new System.Windows.Forms.Padding(8);
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
		private System.Windows.Forms.Button btnShift;
		private System.Windows.Forms.Button btnLoadSVG;
	}
}

