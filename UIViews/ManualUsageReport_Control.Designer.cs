// UI/Views/ManualUsageReport_Control.Designer.cs
namespace Telemetry.UI.Views
{
    partial class ManualUsageReport_Control
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
            this.btnGenerateReport = new MaterialSkin.Controls.MaterialButton(); // MaterialButton yapıldı
            this.dtpEndTime = new System.Windows.Forms.DateTimePicker();
            this.label2 = new MaterialSkin.Controls.MaterialLabel();              // MaterialLabel yapıldı
            this.dtpStartTime = new System.Windows.Forms.DateTimePicker();
            this.label1 = new MaterialSkin.Controls.MaterialLabel();              // MaterialLabel yapıldı
            this.btnExportToExcel = new MaterialSkin.Controls.MaterialButton();   // MaterialButton yapıldı
            this.dgvManualUsage = new System.Windows.Forms.DataGridView();
            this.flpMachineGroups = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvManualUsage)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlFilters
            // 
            this.pnlFilters.BackColor = System.Drawing.Color.Transparent; // Koyu mod parlamasını önlemek için şeffaflaştırıldı
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
            this.pnlFilters.Size = new System.Drawing.Size(788, 60); // 36px modern butonlar için dikey alan 60px'e genişletildi
            this.pnlFilters.TabIndex = 1;
            // 
            // btnGenerateReport
            // 
            this.btnGenerateReport.AutoSize = false;
            this.btnGenerateReport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnGenerateReport.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnGenerateReport.Depth = 0;
            this.btnGenerateReport.HighEmphasis = true; // Baskın ana işlem vurgusu aktif
            this.btnGenerateReport.Icon = null;
            this.btnGenerateReport.Location = new System.Drawing.Point(315, 15); // Dikey hizalama ortalaması sağlandı
            this.btnGenerateReport.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnGenerateReport.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnGenerateReport.Name = "btnGenerateReport";
            this.btnGenerateReport.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnGenerateReport.Size = new System.Drawing.Size(105, 36);
            this.btnGenerateReport.TabIndex = 4;
            this.btnGenerateReport.Text = "Report";
            this.btnGenerateReport.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained; // Dolgulu material tarzı
            this.btnGenerateReport.UseAccentColor = true; // Dikkat çekici aksan rengi
            this.btnGenerateReport.UseVisualStyleBackColor = true;
            this.btnGenerateReport.Click += new System.EventHandler(this.btnGenerateReport_Click);
            // 
            // dtpEndTime
            // 
            this.dtpEndTime.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dtpEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEndTime.Location = new System.Drawing.Point(167, 21); // Koordinatlar basamak hizalamasına eşitlendi
            this.dtpEndTime.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dtpEndTime.Name = "dtpEndTime";
            this.dtpEndTime.Size = new System.Drawing.Size(132, 23);
            this.dtpEndTime.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Depth = 0;
            this.label2.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label2.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label2.Location = new System.Drawing.Point(150, 24);
            this.label2.MouseState = MaterialSkin.MouseState.HOVER;
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(5, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "-";
            // 
            // dtpStartTime
            // 
            this.dtpStartTime.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dtpStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStartTime.Location = new System.Drawing.Point(12, 21);
            this.dtpStartTime.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dtpStartTime.Name = "dtpStartTime";
            this.dtpStartTime.Size = new System.Drawing.Size(132, 23);
            this.dtpStartTime.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Depth = 0;
            this.label1.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label1.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label1.Location = new System.Drawing.Point(12, 2);
            this.label1.MouseState = MaterialSkin.MouseState.HOVER;
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Date Range:";
            // 
            // btnExportToExcel
            // 
            this.btnExportToExcel.AutoSize = false;
            this.btnExportToExcel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnExportToExcel.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnExportToExcel.Depth = 0;
            this.btnExportToExcel.HighEmphasis = false;
            this.btnExportToExcel.Icon = null;
            this.btnExportToExcel.Location = new System.Drawing.Point(428, 15);
            this.btnExportToExcel.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnExportToExcel.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnExportToExcel.Name = "btnExportToExcel";
            this.btnExportToExcel.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnExportToExcel.Size = new System.Drawing.Size(130, 36); // Metin kırpılmasını önleyen genişlik payı esnetildi
            this.btnExportToExcel.TabIndex = 5;
            this.btnExportToExcel.Text = "Export to Excel";
            this.btnExportToExcel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined; // İkincil aksiyon çizgili flat stil
            this.btnExportToExcel.UseAccentColor = false;
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
            this.dgvManualUsage.Location = new System.Drawing.Point(230, 60); // Filtre paneli yüksekliğine göre Y başlangıcı güncellendi
            this.dgvManualUsage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvManualUsage.Name = "dgvManualUsage";
            this.dgvManualUsage.ReadOnly = true;
            this.dgvManualUsage.RowHeadersWidth = 51;
            this.dgvManualUsage.RowTemplate.Height = 36;
            this.dgvManualUsage.Size = new System.Drawing.Size(558, 390);
            this.dgvManualUsage.TabIndex = 3;
            // 
            // flpMachineGroups
            // 
            this.flpMachineGroups.AutoScroll = true;
            this.flpMachineGroups.BackColor = System.Drawing.Color.Transparent; // Koyu mod bütünlüğü için şeffaflaştırıldı
            this.flpMachineGroups.Dock = System.Windows.Forms.DockStyle.Left;
            this.flpMachineGroups.Location = new System.Drawing.Point(0, 60);
            this.flpMachineGroups.Name = "flpMachineGroups";
            this.flpMachineGroups.Padding = new System.Windows.Forms.Padding(5);
            this.flpMachineGroups.Size = new System.Drawing.Size(230, 390);
            this.flpMachineGroups.TabIndex = 2;
            // 
            // ManualUsageReport_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
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
        private MaterialSkin.Controls.MaterialButton btnGenerateReport; // Tür güncellendi
        private System.Windows.Forms.DateTimePicker dtpEndTime;
        private MaterialSkin.Controls.MaterialLabel label2;              // Tür güncellendi
        private System.Windows.Forms.DateTimePicker dtpStartTime;
        private MaterialSkin.Controls.MaterialLabel label1;              // Tür güncellendi
        private System.Windows.Forms.DataGridView dgvManualUsage;
        private MaterialSkin.Controls.MaterialButton btnExportToExcel;  // Tür güncellendi
        private System.Windows.Forms.FlowLayoutPanel flpMachineGroups;
    }
}