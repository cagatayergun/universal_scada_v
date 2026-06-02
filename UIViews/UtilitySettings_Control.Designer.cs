namespace Telemetry.UIViews
{
    partial class UtilitySettings_Control
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lstLines = new System.Windows.Forms.ListBox();
            this.panelListHeader = new System.Windows.Forms.Panel();
            this.btnDeleteLine = new System.Windows.Forms.Button();
            this.btnAddLine = new System.Windows.Forms.Button();
            this.lblListHeader = new System.Windows.Forms.Label();
            this.grpGeneral = new System.Windows.Forms.GroupBox();
            this.txtSlaveId = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.txtIpAddress = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtLineName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.flowLayoutPanelSensors = new System.Windows.Forms.FlowLayoutPanel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelListHeader.SuspendLayout();
            this.grpGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSlaveId)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lstLines);
            this.splitContainer1.Panel1.Controls.Add(this.panelListHeader);
            this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(5);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.flowLayoutPanelSensors);
            this.splitContainer1.Panel2.Controls.Add(this.grpGeneral);
            this.splitContainer1.Panel2.Controls.Add(this.panelBottom);
            this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(5);
            this.splitContainer1.Size = new System.Drawing.Size(1000, 650);
            this.splitContainer1.SplitterDistance = 250;
            this.splitContainer1.TabIndex = 0;
            // 
            // lstLines
            // 
            this.lstLines.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLines.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lstLines.FormattingEnabled = true;
            this.lstLines.ItemHeight = 17;
            this.lstLines.Location = new System.Drawing.Point(5, 45);
            this.lstLines.Name = "lstLines";
            this.lstLines.Size = new System.Drawing.Size(240, 600);
            this.lstLines.TabIndex = 1;
            this.lstLines.SelectedIndexChanged += new System.EventHandler(this.lstLines_SelectedIndexChanged);
            // 
            // panelListHeader
            // 
            this.panelListHeader.Controls.Add(this.btnDeleteLine);
            this.panelListHeader.Controls.Add(this.btnAddLine);
            this.panelListHeader.Controls.Add(this.lblListHeader);
            this.panelListHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelListHeader.Location = new System.Drawing.Point(5, 5);
            this.panelListHeader.Name = "panelListHeader";
            this.panelListHeader.Size = new System.Drawing.Size(240, 40);
            this.panelListHeader.TabIndex = 0;
            // 
            // btnDeleteLine
            // 
            this.btnDeleteLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteLine.BackColor = System.Drawing.Color.IndianRed;
            this.btnDeleteLine.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteLine.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.btnDeleteLine.ForeColor = System.Drawing.Color.White;
            this.btnDeleteLine.Location = new System.Drawing.Point(205, 8);
            this.btnDeleteLine.Name = "btnDeleteLine";
            this.btnDeleteLine.Size = new System.Drawing.Size(30, 25);
            this.btnDeleteLine.TabIndex = 2;
            this.btnDeleteLine.Text = "-";
            this.btnDeleteLine.UseVisualStyleBackColor = false;
            this.btnDeleteLine.Click += new System.EventHandler(this.btnDeleteLine_Click);
            // 
            // btnAddLine
            // 
            this.btnAddLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddLine.BackColor = System.Drawing.Color.SeaGreen;
            this.btnAddLine.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddLine.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.btnAddLine.ForeColor = System.Drawing.Color.White;
            this.btnAddLine.Location = new System.Drawing.Point(170, 8);
            this.btnAddLine.Name = "btnAddLine";
            this.btnAddLine.Size = new System.Drawing.Size(30, 25);
            this.btnAddLine.TabIndex = 1;
            this.btnAddLine.Text = "+";
            this.btnAddLine.UseVisualStyleBackColor = false;
            this.btnAddLine.Click += new System.EventHandler(this.btnAddLine_Click);
            // 
            // lblListHeader
            // 
            this.lblListHeader.AutoSize = true;
            this.lblListHeader.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblListHeader.Location = new System.Drawing.Point(5, 10);
            this.lblListHeader.Name = "lblListHeader";
            this.lblListHeader.Size = new System.Drawing.Size(95, 19);
            this.lblListHeader.TabIndex = 0;
            this.lblListHeader.Text = "Kayıtlı Hatlar";
            // 
            // grpGeneral
            // 
            this.grpGeneral.Controls.Add(this.txtSlaveId);
            this.grpGeneral.Controls.Add(this.label4);
            this.grpGeneral.Controls.Add(this.txtPort);
            this.grpGeneral.Controls.Add(this.label3);
            this.grpGeneral.Controls.Add(this.txtIpAddress);
            this.grpGeneral.Controls.Add(this.label2);
            this.grpGeneral.Controls.Add(this.txtLineName);
            this.grpGeneral.Controls.Add(this.label1);
            this.grpGeneral.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpGeneral.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpGeneral.Location = new System.Drawing.Point(5, 5);
            this.grpGeneral.Name = "grpGeneral";
            this.grpGeneral.Size = new System.Drawing.Size(736, 100);
            this.grpGeneral.TabIndex = 0;
            this.grpGeneral.TabStop = false;
            this.grpGeneral.Text = "Genel Bağlantı Ayarları";
            // 
            // txtSlaveId
            // 
            this.txtSlaveId.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSlaveId.Location = new System.Drawing.Point(620, 45);
            this.txtSlaveId.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.txtSlaveId.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtSlaveId.Name = "txtSlaveId";
            this.txtSlaveId.Size = new System.Drawing.Size(80, 23);
            this.txtSlaveId.TabIndex = 7;
            this.txtSlaveId.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label4.Location = new System.Drawing.Point(617, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "Slave ID";
            // 
            // txtPort
            // 
            this.txtPort.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPort.Location = new System.Drawing.Point(510, 45);
            this.txtPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(90, 23);
            this.txtPort.TabIndex = 5;
            this.txtPort.Value = new decimal(new int[] {
            502,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label3.Location = new System.Drawing.Point(507, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Port";
            // 
            // txtIpAddress
            // 
            this.txtIpAddress.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtIpAddress.Location = new System.Drawing.Point(300, 45);
            this.txtIpAddress.Name = "txtIpAddress";
            this.txtIpAddress.Size = new System.Drawing.Size(190, 23);
            this.txtIpAddress.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label2.Location = new System.Drawing.Point(297, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "IP Adresi";
            // 
            // txtLineName
            // 
            this.txtLineName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtLineName.Location = new System.Drawing.Point(20, 45);
            this.txtLineName.Name = "txtLineName";
            this.txtLineName.Size = new System.Drawing.Size(260, 23);
            this.txtLineName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label1.Location = new System.Drawing.Point(17, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Hat İsmi";
            // 
            // flowLayoutPanelSensors
            // 
            this.flowLayoutPanelSensors.AutoScroll = true;
            this.flowLayoutPanelSensors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelSensors.Location = new System.Drawing.Point(5, 105);
            this.flowLayoutPanelSensors.Name = "flowLayoutPanelSensors";
            this.flowLayoutPanelSensors.Size = new System.Drawing.Size(736, 490);
            this.flowLayoutPanelSensors.TabIndex = 1;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnSave);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(5, 595);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(736, 50);
            this.panelBottom.TabIndex = 2;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.BackColor = System.Drawing.Color.SteelBlue;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(586, 8);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(140, 35);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "KAYDET";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // UtilitySettings_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "UtilitySettings_Control";
            this.Size = new System.Drawing.Size(1000, 650);
            this.Load += new System.EventHandler(this.UtilitySettings_Control_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelListHeader.ResumeLayout(false);
            this.panelListHeader.PerformLayout();
            this.grpGeneral.ResumeLayout(false);
            this.grpGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSlaveId)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox lstLines;
        private System.Windows.Forms.Panel panelListHeader;
        private System.Windows.Forms.Button btnDeleteLine;
        private System.Windows.Forms.Button btnAddLine;
        private System.Windows.Forms.Label lblListHeader;
        private System.Windows.Forms.GroupBox grpGeneral;
        private System.Windows.Forms.NumericUpDown txtSlaveId;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown txtPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtIpAddress;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtLineName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelSensors;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnSave;
    }
}