namespace TekstilScada.UI.Views
{
    partial class GenelUretimRaporu_Control
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
            this.pnlFilters = new System.Windows.Forms.Panel();
            this.btnExportToExcel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioBuhar = new System.Windows.Forms.RadioButton();
            this.radioSu = new System.Windows.Forms.RadioButton();
            this.radioElektrik = new System.Windows.Forms.RadioButton();
            this.btnRaporOlustur = new System.Windows.Forms.Button();
            this.dtpEndTime = new System.Windows.Forms.DateTimePicker();
            this.dtpStartTime = new System.Windows.Forms.DateTimePicker();
            this.dgvReport = new System.Windows.Forms.DataGridView();
            this.flpMachineGroups = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlFilters.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReport)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlFilters
            // 
            this.pnlFilters.Controls.Add(this.btnExportToExcel);
            this.pnlFilters.Controls.Add(this.groupBox1);
            this.pnlFilters.Controls.Add(this.btnRaporOlustur);
            this.pnlFilters.Controls.Add(this.dtpEndTime);
            this.pnlFilters.Controls.Add(this.dtpStartTime);
            this.pnlFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilters.Location = new System.Drawing.Point(0, 0);
            this.pnlFilters.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlFilters.Name = "pnlFilters";
            this.pnlFilters.Size = new System.Drawing.Size(1050, 60);
            this.pnlFilters.TabIndex = 0;
            // 
            // btnExportToExcel
            // 
            this.btnExportToExcel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnExportToExcel.Location = new System.Drawing.Point(674, 16);
            this.btnExportToExcel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnExportToExcel.Name = "btnExportToExcel";
            this.btnExportToExcel.Size = new System.Drawing.Size(125, 30);
            this.btnExportToExcel.TabIndex = 8;
            this.btnExportToExcel.Text = "Export to Excel";
            this.btnExportToExcel.UseVisualStyleBackColor = true;
            this.btnExportToExcel.Click += new System.EventHandler(this.btnExportToExcel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioBuhar);
            this.groupBox1.Controls.Add(this.radioSu);
            this.groupBox1.Controls.Add(this.radioElektrik);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBox1.Location = new System.Drawing.Point(194, 4);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(283, 49);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Consumption Type";
            // 
            // radioBuhar
            // 
            this.radioBuhar.AutoSize = true;
            this.radioBuhar.Location = new System.Drawing.Point(205, 19);
            this.radioBuhar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.radioBuhar.Name = "radioBuhar";
            this.radioBuhar.Size = new System.Drawing.Size(61, 19);
            this.radioBuhar.TabIndex = 2;
            this.radioBuhar.Text = "Steam";
            this.radioBuhar.UseVisualStyleBackColor = true;
            this.radioBuhar.CheckedChanged += new System.EventHandler(this.radioConsumption_CheckedChanged);
            // 
            // radioSu
            // 
            this.radioSu.AutoSize = true;
            this.radioSu.Location = new System.Drawing.Point(127, 19);
            this.radioSu.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.radioSu.Name = "radioSu";
            this.radioSu.Size = new System.Drawing.Size(67, 19);
            this.radioSu.TabIndex = 1;
            this.radioSu.Text = "Water";
            this.radioSu.UseVisualStyleBackColor = true;
            this.radioSu.CheckedChanged += new System.EventHandler(this.radioConsumption_CheckedChanged);
            // 
            // radioElektrik
            // 
            this.radioElektrik.AutoSize = true;
            this.radioElektrik.Checked = true;
            this.radioElektrik.Location = new System.Drawing.Point(20, 19);
            this.radioElektrik.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.radioElektrik.Name = "radioElektrik";
            this.radioElektrik.Size = new System.Drawing.Size(66, 19);
            this.radioElektrik.TabIndex = 0;
            this.radioElektrik.TabStop = true;
            this.radioElektrik.Text = "Electric";
            this.radioElektrik.UseVisualStyleBackColor = true;
            this.radioElektrik.CheckedChanged += new System.EventHandler(this.radioConsumption_CheckedChanged);
            // 
            // btnRaporOlustur
            // 
            this.btnRaporOlustur.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.btnRaporOlustur.Location = new System.Drawing.Point(525, 15);
            this.btnRaporOlustur.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnRaporOlustur.Name = "btnRaporOlustur";
            this.btnRaporOlustur.Size = new System.Drawing.Size(131, 30);
            this.btnRaporOlustur.TabIndex = 2;
            this.btnRaporOlustur.Text = "Report";
            this.btnRaporOlustur.UseVisualStyleBackColor = true;
            this.btnRaporOlustur.Click += new System.EventHandler(this.btnRaporOlustur_Click);
            // 
            // dtpEndTime
            // 
            this.dtpEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpEndTime.Location = new System.Drawing.Point(13, 34);
            this.dtpEndTime.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dtpEndTime.Name = "dtpEndTime";
            this.dtpEndTime.Size = new System.Drawing.Size(120, 23);
            this.dtpEndTime.TabIndex = 1;
            // 
            // dtpStartTime
            // 
            this.dtpStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpStartTime.Location = new System.Drawing.Point(13, 9);
            this.dtpStartTime.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dtpStartTime.Name = "dtpStartTime";
            this.dtpStartTime.Size = new System.Drawing.Size(120, 23);
            this.dtpStartTime.TabIndex = 0;
            // 
            // dgvReport
            // 
            this.dgvReport.AllowUserToAddRows = false;
            this.dgvReport.AllowUserToDeleteRows = false;
            this.dgvReport.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvReport.Location = new System.Drawing.Point(230, 60);
            this.dgvReport.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvReport.Name = "dgvReport";
            this.dgvReport.ReadOnly = true;
            this.dgvReport.RowHeadersWidth = 51;
            this.dgvReport.RowTemplate.Height = 29;
            this.dgvReport.Size = new System.Drawing.Size(820, 465);
            this.dgvReport.TabIndex = 1;
            // 
            // flpMachineGroups
            // 
            this.flpMachineGroups.AutoScroll = true;
            this.flpMachineGroups.BackColor = System.Drawing.SystemColors.ControlLight;
            this.flpMachineGroups.Dock = System.Windows.Forms.DockStyle.Left;
            this.flpMachineGroups.Location = new System.Drawing.Point(0, 60);
            this.flpMachineGroups.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.flpMachineGroups.Name = "flpMachineGroups";
            this.flpMachineGroups.Padding = new System.Windows.Forms.Padding(5);
            this.flpMachineGroups.Size = new System.Drawing.Size(230, 465);
            this.flpMachineGroups.TabIndex = 2;
            // 
            // GenelUretimRaporu_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvReport);
            this.Controls.Add(this.flpMachineGroups);
            this.Controls.Add(this.pnlFilters);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "GenelUretimRaporu_Control";
            this.Size = new System.Drawing.Size(1050, 525);
            this.Load += new System.EventHandler(this.GenelUretimRaporu_Control_Load);
            this.pnlFilters.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReport)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlFilters;
        private System.Windows.Forms.Button btnRaporOlustur;
        private System.Windows.Forms.DateTimePicker dtpEndTime;
        private System.Windows.Forms.DateTimePicker dtpStartTime;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioBuhar;
        private System.Windows.Forms.RadioButton radioSu;
        private System.Windows.Forms.RadioButton radioElektrik;
        private System.Windows.Forms.DataGridView dgvReport;
        private System.Windows.Forms.Button btnExportToExcel;
        private System.Windows.Forms.FlowLayoutPanel flpMachineGroups;
    }
}