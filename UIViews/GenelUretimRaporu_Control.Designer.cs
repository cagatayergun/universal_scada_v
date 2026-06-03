// UIViews/GenelUretimRaporu_Control.Designer.cs
namespace Telemetry.UI.Views
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
            this.btnExportToExcel = new MaterialSkin.Controls.MaterialButton(); // MaterialButton olarak güncellendi
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioBuhar = new MaterialSkin.Controls.MaterialRadioButton();   // MaterialRadioButton olarak güncellendi
            this.radioSu = new MaterialSkin.Controls.MaterialRadioButton();     // MaterialRadioButton olarak güncellendi
            this.radioElektrik = new MaterialSkin.Controls.MaterialRadioButton(); // MaterialRadioButton olarak güncellendi
            this.btnRaporOlustur = new MaterialSkin.Controls.MaterialButton();   // MaterialButton olarak güncellendi
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
            this.pnlFilters.BackColor = System.Drawing.Color.Transparent; // Arka plan parlamasını önlemek için transparan yapıldı
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
            this.btnExportToExcel.AutoSize = false;
            this.btnExportToExcel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnExportToExcel.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnExportToExcel.Depth = 0;
            this.btnExportToExcel.HighEmphasis = false;
            this.btnExportToExcel.Icon = null;
            this.btnExportToExcel.Location = new System.Drawing.Point(575, 12); // Buton dikey eksen ortalaması sağlandı
            this.btnExportToExcel.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnExportToExcel.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnExportToExcel.Name = "btnExportToExcel";
            this.btnExportToExcel.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnExportToExcel.Size = new System.Drawing.Size(135, 36); // Metin sığması için ideal genişlik esnetildi
            this.btnExportToExcel.TabIndex = 8;
            this.btnExportToExcel.Text = "Export to Excel";
            this.btnExportToExcel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined; // İkincil aksiyon çizgili flat stil
            this.btnExportToExcel.UseAccentColor = false;
            this.btnExportToExcel.UseVisualStyleBackColor = true;
            this.btnExportToExcel.Click += new System.EventHandler(this.btnExportToExcel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.radioBuhar);
            this.groupBox1.Controls.Add(this.radioSu);
            this.groupBox1.Controls.Add(this.radioElektrik);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.groupBox1.Location = new System.Drawing.Point(150, 3);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(285, 51);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Consumption Type";
            // 
            // radioBuhar
            // 
            this.radioBuhar.AutoSize = true;
            this.radioBuhar.Depth = 0;
            this.radioBuhar.Location = new System.Drawing.Point(192, 13); // Material radyo buton boşluk dengesi kuruldu
            this.radioBuhar.Margin = new System.Windows.Forms.Padding(0);
            this.radioBuhar.MouseLocation = new System.Drawing.Point(-1, -1);
            this.radioBuhar.MouseState = MaterialSkin.MouseState.HOVER;
            this.radioBuhar.Name = "radioBuhar";
            this.radioBuhar.Ripple = true;
            this.radioBuhar.Size = new System.Drawing.Size(80, 37);
            this.radioBuhar.TabIndex = 2;
            this.radioBuhar.Text = "Steam";
            this.radioBuhar.UseVisualStyleBackColor = true;
            this.radioBuhar.CheckedChanged += new System.EventHandler(this.radioConsumption_CheckedChanged);
            // 
            // radioSu
            // 
            this.radioSu.AutoSize = true;
            this.radioSu.Depth = 0;
            this.radioSu.Location = new System.Drawing.Point(105, 13);
            this.radioSu.Margin = new System.Windows.Forms.Padding(0);
            this.radioSu.MouseLocation = new System.Drawing.Point(-1, -1);
            this.radioSu.MouseState = MaterialSkin.MouseState.HOVER;
            this.radioSu.Name = "radioSu";
            this.radioSu.Ripple = true;
            this.radioSu.Size = new System.Drawing.Size(77, 37);
            this.radioSu.TabIndex = 1;
            this.radioSu.Text = "Water";
            this.radioSu.UseVisualStyleBackColor = true;
            this.radioSu.CheckedChanged += new System.EventHandler(this.radioConsumption_CheckedChanged);
            // 
            // radioElektrik
            // 
            this.radioElektrik.AutoSize = true;
            this.radioElektrik.Checked = true;
            this.radioElektrik.Depth = 0;
            this.radioElektrik.Location = new System.Drawing.Point(10, 13);
            this.radioElektrik.Margin = new System.Windows.Forms.Padding(0);
            this.radioElektrik.MouseLocation = new System.Drawing.Point(-1, -1);
            this.radioElektrik.MouseState = MaterialSkin.MouseState.HOVER;
            this.radioElektrik.Name = "radioElektrik";
            this.radioElektrik.Ripple = true;
            this.radioElektrik.Size = new System.Drawing.Size(85, 37);
            this.radioElektrik.TabIndex = 0;
            this.radioElektrik.TabStop = true;
            this.radioElektrik.Text = "Electric";
            this.radioElektrik.UseVisualStyleBackColor = true;
            this.radioElektrik.CheckedChanged += new System.EventHandler(this.radioConsumption_CheckedChanged);
            // 
            // btnRaporOlustur
            // 
            this.btnRaporOlustur.AutoSize = false;
            this.btnRaporOlustur.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRaporOlustur.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnRaporOlustur.Depth = 0;
            this.btnRaporOlustur.HighEmphasis = true; // Birincil öncelikli ana aksiyon olarak vurgulandı
            this.btnRaporOlustur.Icon = null;
            this.btnRaporOlustur.Location = new System.Drawing.Point(450, 12);
            this.btnRaporOlustur.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnRaporOlustur.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRaporOlustur.Name = "btnRaporOlustur";
            this.btnRaporOlustur.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnRaporOlustur.Size = new System.Drawing.Size(115, 36);
            this.btnRaporOlustur.TabIndex = 2;
            this.btnRaporOlustur.Text = "Report";
            this.btnRaporOlustur.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained; // Dolgulu material tarzı
            this.btnRaporOlustur.UseAccentColor = true; // Dikkat çekici aksan rengi
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
            this.dgvReport.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
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
            this.flpMachineGroups.BackColor = System.Drawing.Color.Transparent; // Koyu mod uyumu için panel şeffaflaştırıldı
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
            this.BackColor = System.Drawing.Color.Transparent;
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
        private MaterialSkin.Controls.MaterialButton btnRaporOlustur;   // Tür güncellendi
        private System.Windows.Forms.DateTimePicker dtpEndTime;
        private System.Windows.Forms.DateTimePicker dtpStartTime;
        private System.Windows.Forms.GroupBox groupBox1;
        private MaterialSkin.Controls.MaterialRadioButton radioBuhar;    // Tür güncellendi
        private MaterialSkin.Controls.MaterialRadioButton radioSu;       // Tür güncellendi
        private MaterialSkin.Controls.MaterialRadioButton radioElektrik; // Tür güncellendi
        private System.Windows.Forms.DataGridView dgvReport;
        private MaterialSkin.Controls.MaterialButton btnExportToExcel;  // Tür güncellendi
        private System.Windows.Forms.FlowLayoutPanel flpMachineGroups;
    }
}