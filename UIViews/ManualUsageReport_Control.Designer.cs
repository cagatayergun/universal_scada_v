namespace TekstilScada.UI.Views
{
    partial class ManualUsageReport_Control
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlFilters = new System.Windows.Forms.Panel();
            this.btnGenerateReport = new System.Windows.Forms.Button();
            this.dtpEndTime = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpStartTime = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.btnExportToExcel = new System.Windows.Forms.Button();
            this.dgvManualUsage = new System.Windows.Forms.DataGridView();
            this.flpMachineGroups = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvManualUsage)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlFilters
            // 
            this.pnlFilters.Controls.Add(this.btnGenerateReport);
            this.pnlFilters.Controls.Add(this.dtpEndTime);
            this.pnlFilters.Controls.Add(this.label2);
            this.pnlFilters.Controls.Add(this.dtpStartTime);
            this.pnlFilters.Controls.Add(this.label1);
            this.pnlFilters.Controls.Add(this.btnExportToExcel);
            this.pnlFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilters.Location = new System.Drawing.Point(0, 0);
            this.pnlFilters.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlFilters.Name = "pnlFilters";
            this.pnlFilters.Size = new System.Drawing.Size(788, 48);
            this.pnlFilters.TabIndex = 1;
            // 
            // btnGenerateReport
            // 
            this.btnGenerateReport.Location = new System.Drawing.Point(320, 19);
            this.btnGenerateReport.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnGenerateReport.Name = "btnGenerateReport";
            this.btnGenerateReport.Size = new System.Drawing.Size(105, 22);
            this.btnGenerateReport.TabIndex = 4;
            this.btnGenerateReport.Text = "Report";
            this.btnGenerateReport.UseVisualStyleBackColor = true;
            this.btnGenerateReport.Click += new System.EventHandler(this.btnGenerateReport_Click);
            // 
            // dtpEndTime
            // 
            this.dtpEndTime.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dtpEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEndTime.Location = new System.Drawing.Point(167, 19);
            this.dtpEndTime.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dtpEndTime.Name = "dtpEndTime";
            this.dtpEndTime.Size = new System.Drawing.Size(132, 23);
            this.dtpEndTime.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(149, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(12, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "-";
            // 
            // dtpStartTime
            // 
            this.dtpStartTime.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dtpStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStartTime.Location = new System.Drawing.Point(12, 19);
            this.dtpStartTime.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dtpStartTime.Name = "dtpStartTime";
            this.dtpStartTime.Size = new System.Drawing.Size(132, 23);
            this.dtpStartTime.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Date Range:";
            // 
            // btnExportToExcel
            // 
            this.btnExportToExcel.Location = new System.Drawing.Point(431, 19);
            this.btnExportToExcel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnExportToExcel.Name = "btnExportToExcel";
            this.btnExportToExcel.Size = new System.Drawing.Size(106, 22);
            this.btnExportToExcel.TabIndex = 5;
            this.btnExportToExcel.Text = "Export to Excel";
            this.btnExportToExcel.UseVisualStyleBackColor = true;
            this.btnExportToExcel.Click += new System.EventHandler(this.btnExportToExcel_Click);
            // 
            // dgvManualUsage
            // 
            this.dgvManualUsage.AllowUserToAddRows = false;
            this.dgvManualUsage.AllowUserToDeleteRows = false;
            this.dgvManualUsage.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvManualUsage.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvManualUsage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvManualUsage.Location = new System.Drawing.Point(230, 48);
            this.dgvManualUsage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvManualUsage.Name = "dgvManualUsage";
            this.dgvManualUsage.ReadOnly = true;
            this.dgvManualUsage.RowHeadersWidth = 51;
            this.dgvManualUsage.RowTemplate.Height = 29;
            this.dgvManualUsage.Size = new System.Drawing.Size(558, 402);
            this.dgvManualUsage.TabIndex = 3;
            // 
            // flpMachineGroups
            // 
            this.flpMachineGroups.AutoScroll = true;
            this.flpMachineGroups.BackColor = System.Drawing.SystemColors.ControlLight;
            this.flpMachineGroups.Dock = System.Windows.Forms.DockStyle.Left;
            this.flpMachineGroups.Location = new System.Drawing.Point(0, 48);
            this.flpMachineGroups.Name = "flpMachineGroups";
            this.flpMachineGroups.Padding = new System.Windows.Forms.Padding(5);
            this.flpMachineGroups.Size = new System.Drawing.Size(230, 402);
            this.flpMachineGroups.TabIndex = 2;
            // 
            // ManualUsageReport_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvManualUsage);
            this.Controls.Add(this.flpMachineGroups);
            this.Controls.Add(this.pnlFilters);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "ManualUsageReport_Control";
            this.Size = new System.Drawing.Size(788, 450);
            this.Load += new System.EventHandler(this.ManualUsageReport_Control_Load);
            this.pnlFilters.ResumeLayout(false);
            this.pnlFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvManualUsage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlFilters;
        private System.Windows.Forms.Button btnGenerateReport;
        private System.Windows.Forms.DateTimePicker dtpEndTime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpStartTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgvManualUsage;
        private System.Windows.Forms.Button btnExportToExcel;
        private System.Windows.Forms.FlowLayoutPanel flpMachineGroups;
    }
}