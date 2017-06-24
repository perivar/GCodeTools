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
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.panelViewerAndCommands = new System.Windows.Forms.Panel();
			this.panelViewer = new System.Windows.Forms.Panel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.panelZoomFilename = new System.Windows.Forms.Panel();
			this.txtCoordinates = new System.Windows.Forms.TextBox();
			this.txtFile = new System.Windows.Forms.TextBox();
			this.panelCommands = new System.Windows.Forms.Panel();
			this.panelMove = new System.Windows.Forms.Panel();
			this.btnShift = new System.Windows.Forms.Button();
			this.txtShiftX = new System.Windows.Forms.TextBox();
			this.txtShiftY = new System.Windows.Forms.TextBox();
			this.txtShiftZ = new System.Windows.Forms.TextBox();
			this.panelSVG = new System.Windows.Forms.Panel();
			this.radSVGCenter = new System.Windows.Forms.RadioButton();
			this.btnLoadSVG = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.txtZDepth = new System.Windows.Forms.TextBox();
			this.radSVGAll = new System.Windows.Forms.RadioButton();
			this.panelSplitCmds = new System.Windows.Forms.Panel();
			this.btnSplit = new System.Windows.Forms.Button();
			this.radLeft = new System.Windows.Forms.RadioButton();
			this.radRight = new System.Windows.Forms.RadioButton();
			this.lblSplit = new System.Windows.Forms.Label();
			this.txtSplit = new System.Windows.Forms.TextBox();
			this.btnSaveSplit = new System.Windows.Forms.Button();
			this.btnOptimize = new System.Windows.Forms.Button();
			this.lblZClearance = new System.Windows.Forms.Label();
			this.txtZClearance = new System.Windows.Forms.TextBox();
			this.txtDimension = new System.Windows.Forms.TextBox();
			this.cmdSaveLayers = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnLoad = new System.Windows.Forms.Button();
			this.panelCode = new System.Windows.Forms.Panel();
			this.treeView = new System.Windows.Forms.TreeView();
			this.panelCheckboxes = new System.Windows.Forms.Panel();
			this.cbSoloSelect = new System.Windows.Forms.CheckBox();
			this.cbRenderG0 = new System.Windows.Forms.CheckBox();
			this.sfdSaveDialog = new System.Windows.Forms.SaveFileDialog();
			this.ofdLoadDialog = new System.Windows.Forms.OpenFileDialog();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.panelViewerAndCommands.SuspendLayout();
			this.panelViewer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.panelZoomFilename.SuspendLayout();
			this.panelCommands.SuspendLayout();
			this.panelMove.SuspendLayout();
			this.panelSVG.SuspendLayout();
			this.panelSplitCmds.SuspendLayout();
			this.panelCode.SuspendLayout();
			this.panelCheckboxes.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(8, 8);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.panelViewerAndCommands);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.panelCode);
			this.splitContainer1.Size = new System.Drawing.Size(1254, 784);
			this.splitContainer1.SplitterDistance = 921;
			this.splitContainer1.TabIndex = 0;
			// 
			// panelViewerAndCommands
			// 
			this.panelViewerAndCommands.Controls.Add(this.panelViewer);
			this.panelViewerAndCommands.Controls.Add(this.panelZoomFilename);
			this.panelViewerAndCommands.Controls.Add(this.panelCommands);
			this.panelViewerAndCommands.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelViewerAndCommands.Location = new System.Drawing.Point(0, 0);
			this.panelViewerAndCommands.Name = "panelViewerAndCommands";
			this.panelViewerAndCommands.Size = new System.Drawing.Size(921, 784);
			this.panelViewerAndCommands.TabIndex = 0;
			// 
			// panelViewer
			// 
			this.panelViewer.Controls.Add(this.pictureBox1);
			this.panelViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelViewer.Location = new System.Drawing.Point(0, 0);
			this.panelViewer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panelViewer.Name = "panelViewer";
			this.panelViewer.Size = new System.Drawing.Size(733, 726);
			this.panelViewer.TabIndex = 13;
			this.panelViewer.Scroll += new System.Windows.Forms.ScrollEventHandler(this.PanelViewerScroll);
			// 
			// pictureBox1
			// 
			this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox1.Location = new System.Drawing.Point(0, 0);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(615, 716);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBox1MouseDown);
			this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBox1MouseMove);
			// 
			// panelZoomFilename
			// 
			this.panelZoomFilename.Controls.Add(this.txtCoordinates);
			this.panelZoomFilename.Controls.Add(this.txtFile);
			this.panelZoomFilename.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelZoomFilename.Location = new System.Drawing.Point(0, 726);
			this.panelZoomFilename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panelZoomFilename.Name = "panelZoomFilename";
			this.panelZoomFilename.Size = new System.Drawing.Size(733, 58);
			this.panelZoomFilename.TabIndex = 12;
			// 
			// txtCoordinates
			// 
			this.txtCoordinates.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.txtCoordinates.BackColor = System.Drawing.SystemColors.Control;
			this.txtCoordinates.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtCoordinates.Location = new System.Drawing.Point(544, 17);
			this.txtCoordinates.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtCoordinates.Name = "txtCoordinates";
			this.txtCoordinates.Size = new System.Drawing.Size(181, 26);
			this.txtCoordinates.TabIndex = 11;
			// 
			// txtFile
			// 
			this.txtFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txtFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtFile.Location = new System.Drawing.Point(24, 17);
			this.txtFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtFile.Name = "txtFile";
			this.txtFile.ReadOnly = true;
			this.txtFile.Size = new System.Drawing.Size(395, 26);
			this.txtFile.TabIndex = 10;
			// 
			// panelCommands
			// 
			this.panelCommands.Controls.Add(this.panelMove);
			this.panelCommands.Controls.Add(this.panelSVG);
			this.panelCommands.Controls.Add(this.panelSplitCmds);
			this.panelCommands.Controls.Add(this.btnOptimize);
			this.panelCommands.Controls.Add(this.lblZClearance);
			this.panelCommands.Controls.Add(this.txtZClearance);
			this.panelCommands.Controls.Add(this.txtDimension);
			this.panelCommands.Controls.Add(this.cmdSaveLayers);
			this.panelCommands.Controls.Add(this.btnSave);
			this.panelCommands.Controls.Add(this.btnLoad);
			this.panelCommands.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelCommands.Location = new System.Drawing.Point(733, 0);
			this.panelCommands.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panelCommands.Name = "panelCommands";
			this.panelCommands.Size = new System.Drawing.Size(188, 784);
			this.panelCommands.TabIndex = 11;
			// 
			// panelMove
			// 
			this.panelMove.Controls.Add(this.btnShift);
			this.panelMove.Controls.Add(this.txtShiftX);
			this.panelMove.Controls.Add(this.txtShiftY);
			this.panelMove.Controls.Add(this.txtShiftZ);
			this.panelMove.Location = new System.Drawing.Point(10, 171);
			this.panelMove.Name = "panelMove";
			this.panelMove.Size = new System.Drawing.Size(171, 79);
			this.panelMove.TabIndex = 32;
			// 
			// btnShift
			// 
			this.btnShift.Location = new System.Drawing.Point(5, 0);
			this.btnShift.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnShift.Name = "btnShift";
			this.btnShift.Size = new System.Drawing.Size(159, 34);
			this.btnShift.TabIndex = 21;
			this.btnShift.Text = "Move";
			this.btnShift.UseVisualStyleBackColor = true;
			this.btnShift.Click += new System.EventHandler(this.BtnShiftClick);
			// 
			// txtShiftX
			// 
			this.txtShiftX.Location = new System.Drawing.Point(5, 41);
			this.txtShiftX.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtShiftX.Name = "txtShiftX";
			this.txtShiftX.Size = new System.Drawing.Size(44, 26);
			this.txtShiftX.TabIndex = 23;
			this.txtShiftX.Text = "0.0";
			// 
			// txtShiftY
			// 
			this.txtShiftY.Location = new System.Drawing.Point(64, 41);
			this.txtShiftY.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtShiftY.Name = "txtShiftY";
			this.txtShiftY.Size = new System.Drawing.Size(44, 26);
			this.txtShiftY.TabIndex = 24;
			this.txtShiftY.Text = "0.0";
			// 
			// txtShiftZ
			// 
			this.txtShiftZ.Location = new System.Drawing.Point(120, 41);
			this.txtShiftZ.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtShiftZ.Name = "txtShiftZ";
			this.txtShiftZ.Size = new System.Drawing.Size(44, 26);
			this.txtShiftZ.TabIndex = 25;
			this.txtShiftZ.Text = "0.0";
			// 
			// panelSVG
			// 
			this.panelSVG.Controls.Add(this.radSVGCenter);
			this.panelSVG.Controls.Add(this.btnLoadSVG);
			this.panelSVG.Controls.Add(this.label1);
			this.panelSVG.Controls.Add(this.txtZDepth);
			this.panelSVG.Controls.Add(this.radSVGAll);
			this.panelSVG.Location = new System.Drawing.Point(10, 250);
			this.panelSVG.Name = "panelSVG";
			this.panelSVG.Size = new System.Drawing.Size(178, 113);
			this.panelSVG.TabIndex = 31;
			// 
			// radSVGCenter
			// 
			this.radSVGCenter.Location = new System.Drawing.Point(73, 41);
			this.radSVGCenter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.radSVGCenter.Name = "radSVGCenter";
			this.radSVGCenter.Size = new System.Drawing.Size(93, 37);
			this.radSVGCenter.TabIndex = 27;
			this.radSVGCenter.TabStop = true;
			this.radSVGCenter.Text = "Center";
			this.radSVGCenter.UseVisualStyleBackColor = true;
			// 
			// btnLoadSVG
			// 
			this.btnLoadSVG.Location = new System.Drawing.Point(5, 5);
			this.btnLoadSVG.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnLoadSVG.Name = "btnLoadSVG";
			this.btnLoadSVG.Size = new System.Drawing.Size(159, 34);
			this.btnLoadSVG.TabIndex = 22;
			this.btnLoadSVG.Text = "SVG Load";
			this.btnLoadSVG.UseVisualStyleBackColor = true;
			this.btnLoadSVG.Click += new System.EventHandler(this.BtnSVGLoadClick);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(6, 81);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(81, 29);
			this.label1.TabIndex = 29;
			this.label1.Text = "Z-Depth:";
			// 
			// txtZDepth
			// 
			this.txtZDepth.Location = new System.Drawing.Point(119, 78);
			this.txtZDepth.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtZDepth.Name = "txtZDepth";
			this.txtZDepth.Size = new System.Drawing.Size(45, 26);
			this.txtZDepth.TabIndex = 28;
			this.txtZDepth.Text = "-0.1";
			// 
			// radSVGAll
			// 
			this.radSVGAll.Checked = true;
			this.radSVGAll.Location = new System.Drawing.Point(7, 41);
			this.radSVGAll.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.radSVGAll.Name = "radSVGAll";
			this.radSVGAll.Size = new System.Drawing.Size(68, 37);
			this.radSVGAll.TabIndex = 26;
			this.radSVGAll.TabStop = true;
			this.radSVGAll.Text = "All";
			this.radSVGAll.UseVisualStyleBackColor = true;
			// 
			// panelSplitCmds
			// 
			this.panelSplitCmds.Controls.Add(this.btnSplit);
			this.panelSplitCmds.Controls.Add(this.radLeft);
			this.panelSplitCmds.Controls.Add(this.radRight);
			this.panelSplitCmds.Controls.Add(this.lblSplit);
			this.panelSplitCmds.Controls.Add(this.txtSplit);
			this.panelSplitCmds.Controls.Add(this.btnSaveSplit);
			this.panelSplitCmds.Location = new System.Drawing.Point(10, 362);
			this.panelSplitCmds.Name = "panelSplitCmds";
			this.panelSplitCmds.Size = new System.Drawing.Size(178, 154);
			this.panelSplitCmds.TabIndex = 30;
			// 
			// btnSplit
			// 
			this.btnSplit.Location = new System.Drawing.Point(6, 5);
			this.btnSplit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnSplit.Name = "btnSplit";
			this.btnSplit.Size = new System.Drawing.Size(160, 34);
			this.btnSplit.TabIndex = 12;
			this.btnSplit.Text = "Split";
			this.btnSplit.UseVisualStyleBackColor = true;
			this.btnSplit.Click += new System.EventHandler(this.btnSplitClick);
			// 
			// radLeft
			// 
			this.radLeft.Checked = true;
			this.radLeft.Location = new System.Drawing.Point(6, 42);
			this.radLeft.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.radLeft.Name = "radLeft";
			this.radLeft.Size = new System.Drawing.Size(68, 37);
			this.radLeft.TabIndex = 15;
			this.radLeft.TabStop = true;
			this.radLeft.Text = "Left";
			this.radLeft.UseVisualStyleBackColor = true;
			// 
			// radRight
			// 
			this.radRight.Location = new System.Drawing.Point(82, 42);
			this.radRight.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.radRight.Name = "radRight";
			this.radRight.Size = new System.Drawing.Size(87, 37);
			this.radRight.TabIndex = 16;
			this.radRight.TabStop = true;
			this.radRight.Text = "Right";
			this.radRight.UseVisualStyleBackColor = true;
			// 
			// lblSplit
			// 
			this.lblSplit.Location = new System.Drawing.Point(11, 82);
			this.lblSplit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblSplit.Name = "lblSplit";
			this.lblSplit.Size = new System.Drawing.Size(52, 26);
			this.lblSplit.TabIndex = 13;
			this.lblSplit.Text = "Split:";
			// 
			// txtSplit
			// 
			this.txtSplit.Location = new System.Drawing.Point(105, 80);
			this.txtSplit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtSplit.Name = "txtSplit";
			this.txtSplit.Size = new System.Drawing.Size(59, 26);
			this.txtSplit.TabIndex = 14;
			this.txtSplit.Text = "0.0";
			// 
			// btnSaveSplit
			// 
			this.btnSaveSplit.Location = new System.Drawing.Point(9, 114);
			this.btnSaveSplit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnSaveSplit.Name = "btnSaveSplit";
			this.btnSaveSplit.Size = new System.Drawing.Size(160, 34);
			this.btnSaveSplit.TabIndex = 19;
			this.btnSaveSplit.Text = "Save Both";
			this.btnSaveSplit.UseVisualStyleBackColor = true;
			this.btnSaveSplit.Click += new System.EventHandler(this.btnSaveSplitClick);
			// 
			// btnOptimize
			// 
			this.btnOptimize.Location = new System.Drawing.Point(15, 131);
			this.btnOptimize.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnOptimize.Name = "btnOptimize";
			this.btnOptimize.Size = new System.Drawing.Size(159, 34);
			this.btnOptimize.TabIndex = 20;
			this.btnOptimize.Text = "Optimize";
			this.btnOptimize.UseVisualStyleBackColor = true;
			this.btnOptimize.Click += new System.EventHandler(this.BtnOptimizeClick);
			// 
			// lblZClearance
			// 
			this.lblZClearance.Location = new System.Drawing.Point(15, 519);
			this.lblZClearance.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblZClearance.Name = "lblZClearance";
			this.lblZClearance.Size = new System.Drawing.Size(114, 29);
			this.lblZClearance.TabIndex = 18;
			this.lblZClearance.Text = "Z-Safe Height:";
			// 
			// txtZClearance
			// 
			this.txtZClearance.Location = new System.Drawing.Point(130, 516);
			this.txtZClearance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtZClearance.Name = "txtZClearance";
			this.txtZClearance.Size = new System.Drawing.Size(46, 26);
			this.txtZClearance.TabIndex = 17;
			this.txtZClearance.Text = "2.0";
			// 
			// txtDimension
			// 
			this.txtDimension.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.txtDimension.BackColor = System.Drawing.SystemColors.Control;
			this.txtDimension.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtDimension.Location = new System.Drawing.Point(4, 651);
			this.txtDimension.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.txtDimension.Multiline = true;
			this.txtDimension.Name = "txtDimension";
			this.txtDimension.ReadOnly = true;
			this.txtDimension.Size = new System.Drawing.Size(178, 131);
			this.txtDimension.TabIndex = 11;
			// 
			// cmdSaveLayers
			// 
			this.cmdSaveLayers.Location = new System.Drawing.Point(15, 89);
			this.cmdSaveLayers.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cmdSaveLayers.Name = "cmdSaveLayers";
			this.cmdSaveLayers.Size = new System.Drawing.Size(159, 34);
			this.cmdSaveLayers.TabIndex = 9;
			this.cmdSaveLayers.Text = "Save (Peck Drill)";
			this.cmdSaveLayers.UseVisualStyleBackColor = true;
			this.cmdSaveLayers.Click += new System.EventHandler(this.btnSaveLayersClick);
			// 
			// btnSave
			// 
			this.btnSave.Location = new System.Drawing.Point(15, 47);
			this.btnSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(159, 34);
			this.btnSave.TabIndex = 9;
			this.btnSave.Text = "Save";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSaveClick);
			// 
			// btnLoad
			// 
			this.btnLoad.Location = new System.Drawing.Point(15, 5);
			this.btnLoad.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnLoad.Name = "btnLoad";
			this.btnLoad.Size = new System.Drawing.Size(159, 34);
			this.btnLoad.TabIndex = 9;
			this.btnLoad.Text = "Load";
			this.btnLoad.UseVisualStyleBackColor = true;
			this.btnLoad.Click += new System.EventHandler(this.btnLoadClick);
			// 
			// panelCode
			// 
			this.panelCode.Controls.Add(this.treeView);
			this.panelCode.Controls.Add(this.panelCheckboxes);
			this.panelCode.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelCode.Location = new System.Drawing.Point(0, 0);
			this.panelCode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panelCode.Name = "panelCode";
			this.panelCode.Size = new System.Drawing.Size(329, 784);
			this.panelCode.TabIndex = 10;
			// 
			// treeView
			// 
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.HideSelection = false;
			this.treeView.Location = new System.Drawing.Point(0, 0);
			this.treeView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(329, 733);
			this.treeView.TabIndex = 10;
			this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewAfterSelect);
			this.treeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TreeViewKeyDown);
			this.treeView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TreeViewMouseUp);
			// 
			// panelCheckboxes
			// 
			this.panelCheckboxes.Controls.Add(this.cbSoloSelect);
			this.panelCheckboxes.Controls.Add(this.cbRenderG0);
			this.panelCheckboxes.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelCheckboxes.Location = new System.Drawing.Point(0, 733);
			this.panelCheckboxes.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panelCheckboxes.Name = "panelCheckboxes";
			this.panelCheckboxes.Size = new System.Drawing.Size(329, 51);
			this.panelCheckboxes.TabIndex = 9;
			// 
			// cbSoloSelect
			// 
			this.cbSoloSelect.AutoSize = true;
			this.cbSoloSelect.Location = new System.Drawing.Point(203, 12);
			this.cbSoloSelect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cbSoloSelect.Name = "cbSoloSelect";
			this.cbSoloSelect.Size = new System.Drawing.Size(116, 24);
			this.cbSoloSelect.TabIndex = 6;
			this.cbSoloSelect.Text = "Solo Select";
			this.cbSoloSelect.UseVisualStyleBackColor = true;
			this.cbSoloSelect.CheckedChanged += new System.EventHandler(this.cbSoloSelectCheckedChanged);
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
			// frmPlotter
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1270, 800);
			this.Controls.Add(this.splitContainer1);
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "frmPlotter";
			this.Padding = new System.Windows.Forms.Padding(8);
			this.Text = "GCode Viewer";
			this.Load += new System.EventHandler(this.frmPlotterLoad);
			this.ResizeEnd += new System.EventHandler(this.frmPlotterResizeEnd);
			this.ClientSizeChanged += new System.EventHandler(this.frmPlotterResizeEnd);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.panelViewerAndCommands.ResumeLayout(false);
			this.panelViewer.ResumeLayout(false);
			this.panelViewer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.panelZoomFilename.ResumeLayout(false);
			this.panelZoomFilename.PerformLayout();
			this.panelCommands.ResumeLayout(false);
			this.panelCommands.PerformLayout();
			this.panelMove.ResumeLayout(false);
			this.panelMove.PerformLayout();
			this.panelSVG.ResumeLayout(false);
			this.panelSVG.PerformLayout();
			this.panelSplitCmds.ResumeLayout(false);
			this.panelSplitCmds.PerformLayout();
			this.panelCode.ResumeLayout(false);
			this.panelCheckboxes.ResumeLayout(false);
			this.panelCheckboxes.PerformLayout();
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.Panel panelViewerAndCommands;
		private System.Windows.Forms.Panel panelViewer;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Panel panelCode;
		private System.Windows.Forms.Panel panelCheckboxes;
		private System.Windows.Forms.Panel panelCommands;
		private System.Windows.Forms.Panel panelZoomFilename;
		private System.Windows.Forms.CheckBox cbRenderG0;
		private System.Windows.Forms.Button btnLoad;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button cmdSaveLayers;
		private System.Windows.Forms.SaveFileDialog sfdSaveDialog;
		private System.Windows.Forms.OpenFileDialog ofdLoadDialog;
		private System.Windows.Forms.TextBox txtFile;
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
		private System.Windows.Forms.TextBox txtShiftZ;
		private System.Windows.Forms.TextBox txtShiftY;
		private System.Windows.Forms.TextBox txtShiftX;
		private System.Windows.Forms.RadioButton radSVGCenter;
		private System.Windows.Forms.RadioButton radSVGAll;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtZDepth;
		private System.Windows.Forms.Panel panelSplitCmds;
		private System.Windows.Forms.Panel panelMove;
		private System.Windows.Forms.Panel panelSVG;
	}
}