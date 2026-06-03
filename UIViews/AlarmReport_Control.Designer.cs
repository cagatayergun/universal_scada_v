// UI/Views/AlarmReport_Control.Designer.cs
namespace Telemetry.UI.Views
{
    partial class AlarmReport_Control
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
            this.cmbMachines = new System.Windows.Forms.ComboBox();
            this.label3 = new MaterialSkin.Controls.MaterialLabel();            // MaterialLabel yapıldı
            this.dtpEndTime = new System.Windows.Forms.DateTimePicker();
            this.label2 = new MaterialSkin.Controls.MaterialLabel();
            this.dtpStartTime = new System.Windows.Forms.DateTimePicker();
            this.label1 = new MaterialSkin.Controls.MaterialLabel();
            this.btnExportToExcel = new MaterialSkin.Controls.MaterialButton(); // MaterialButton yapıldı
            this.dgvAlarmReport = new System.Windows.Forms.DataGridView();
            this.pnlPagination = new System.Windows.Forms.Panel();
            this.btnPrev = new MaterialSkin.Controls.MaterialButton();          // MaterialButton yapıldı
            this.lblPageInfo = new MaterialSkin.Controls.MaterialLabel();         // MaterialLabel yapıldı
            this.btnNext = new MaterialSkin.Controls.MaterialButton();          // MaterialButton yapıldı
            this.pnlFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAlarmReport)).BeginInit();
            this.pnlPagination.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlFilters
            // 
            this.pnlFilters.BackColor = System.Drawing.Color.Transparent;
            this.pnlFilters.Controls.Add(this.btnGenerateReport);
            this.pnlFilters.Controls.Add(this.cmbMachines);
            this.pnlFilters.Controls.Add(this.label3);
            this.pnlFilters.Controls.Add(this.dtpEndTime);
            this.pnlFilters.Controls.Add(this.label2);
            this.pnlFilters.Controls.Add(this.dtpStartTime);
            this.pnlFilters.Controls.Add(this.label1);
            this.pnlFilters.Controls.Add(this.btnExportToExcel);
            this.pnlFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilters.Location = new System.Drawing.Point(0, 0);
            this.pnlFilters.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlFilters.Name = "pnlFilters";
            this.pnlFilters.Size = new System.Drawing.Size(929, 45);
            this.pnlFilters.TabIndex = 0;
            // 
            // btnGenerateReport
            // 
            this.btnGenerateReport.AutoSize = false;
            this.btnGenerateReport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnGenerateReport.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnGenerateReport.Depth = 0;
            this.btnGenerateReport.HighEmphasis = true;
            this.btnGenerateReport.Icon = null;
            this.btnGenerateReport.Location = new System.Drawing.Point(620, 6); // Dikey basamak eşitlemesi yapıldı
            this.btnGenerateReport.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnGenerateReport.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnGenerateReport.Name = "btnGenerateReport";
            this.btnGenerateReport.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnGenerateReport.Size = new System.Drawing.Size(88, 34);
            this.btnGenerateReport.TabIndex = 6;
            this.btnGenerateReport.Text = "Report";
            this.btnGenerateReport.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained; // Dolgulu baskın stil
            this.btnGenerateReport.UseAccentColor = true; // Aksan rengi vurgusu
            this.btnGenerateReport.UseVisualStyleBackColor = true;
            this.btnGenerateReport.Click += new System.EventHandler(this.btnGenerateReport_Click);
            // 
            // cmbMachines
            // 
            this.cmbMachines.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMachines.FormattingEnabled = true;
            this.cmbMachines.Location = new System.Drawing.Point(445, 11);
            this.cmbMachines.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbMachines.Name = "cmbMachines";
            this.cmbMachines.Size = new System.Drawing.Size(165, 23);
            this.cmbMachines.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Depth = 0;
            this.label3.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label3.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label3.Location = new System.Drawing.Point(384, 15);
            this.label3.MouseState = MaterialSkin.MouseState.HOVER;
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Machine:";
            // 
            // dtpEndTime
            // 
            this.dtpEndTime.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dtpEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEndTime.Location = new System.Drawing.Point(242, 11);
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
            this.label2.Location = new System.Drawing.Point(224, 15);
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
            this.dtpStartTime.Location = new System.Drawing.Point(84, 11);
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
            this.label1.Location = new System.Drawing.Point(4, 15);
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
            this.btnExportToExcel.Location = new System.Drawing.Point(714, 6);
            this.btnExportToExcel.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnExportToExcel.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnExportToExcel.Name = "btnExportToExcel";
            this.btnExportToExcel.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnExportToExcel.Size = new System.Drawing.Size(130, 34);
            this.btnExportToExcel.TabIndex = 7;
            this.btnExportToExcel.Text = "Export to Excel";
            this.btnExportToExcel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined; // Çizgili ikincil aksiyon stili
            this.btnExportToExcel.UseAccentColor = false;
            this.btnExportToExcel.UseVisualStyleBackColor = true;
            this.btnExportToExcel.Click += new System.EventHandler(this.btnExportToExcel_Click);
            // 
            // dgvAlarmReport
            // 
            this.dgvAlarmReport.AllowUserToAddRows = false;
            this.dgvAlarmReport.AllowUserToDeleteRows = false;
            this.dgvAlarmReport.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAlarmReport.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAlarmReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAlarmReport.Location = new System.Drawing.Point(0, 45);
            this.dgvAlarmReport.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvAlarmReport.Name = "dgvAlarmReport";
            this.dgvAlarmReport.ReadOnly = true;
            this.dgvAlarmReport.RowHeadersWidth = 51;
            this.dgvAlarmReport.RowTemplate.Height = 26;
            this.dgvAlarmReport.Size = new System.Drawing.Size(929, 365);
            this.dgvAlarmReport.TabIndex = 1;
            // 
            // pnlPagination
            // 
            this.pnlPagination.BackColor = System.Drawing.Color.Transparent;
            this.pnlPagination.Controls.Add(this.btnPrev);
            this.pnlPagination.Controls.Add(this.lblPageInfo);
            this.pnlPagination.Controls.Add(this.btnNext);
            this.pnlPagination.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlPagination.Location = new System.Drawing.Point(0, 410);
            this.pnlPagination.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlPagination.Name = "pnlPagination";
            this.pnlPagination.Size = new System.Drawing.Size(929, 40);
            this.pnlPagination.TabIndex = 2;
            // 
            // btnPrev
            // 
            this.btnPrev.AutoSize = false;
            this.btnPrev.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnPrev.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnPrev.Depth = 0;
            this.btnPrev.HighEmphasis = false;
            this.btnPrev.Icon = null;
            this.btnPrev.Location = new System.Drawing.Point(6, 4);
            this.btnPrev.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnPrev.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnPrev.Size = new System.Drawing.Size(88, 32);
            this.btnPrev.TabIndex = 0;
            this.btnPrev.Text = "< Back";
            this.btnPrev.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnPrev.UseAccentColor = false;
            this.btnPrev.UseVisualStyleBackColor = true;
            this.btnPrev.Click += new System.EventHandler(this.btnPrev_Click);
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.AutoSize = true;
            this.lblPageInfo.Depth = 0;
            this.lblPageInfo.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblPageInfo.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.lblPageInfo.Location = new System.Drawing.Point(105, 12);
            this.lblPageInfo.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(130, 17);
            this.lblPageInfo.TabIndex = 1;
            this.lblPageInfo.Text = "Page: 1 / 1 (Total: 0)";
            // 
            // btnNext
            // 
            this.btnNext.AutoSize = false;
            this.btnNext.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnNext.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnNext.Depth = 0;
            this.btnNext.HighEmphasis = false;
            this.btnNext.Icon = null;
            this.btnNext.Location = new System.Drawing.Point(320, 4);
            this.btnNext.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnNext.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnNext.Name = "btnNext";
            this.btnNext.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnNext.Size = new System.Drawing.Size(88, 32);
            this.btnNext.TabIndex = 2;
            this.btnNext.Text = "Next >";
            this.btnNext.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnNext.UseAccentColor = false;
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // AlarmReport_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.dgvAlarmReport);
            this.Controls.Add(this.pnlPagination);
            this.Controls.Add(this.pnlFilters);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "AlarmReport_Control";
            this.Size = new System.Drawing.Size(929, 450);
            this.Load += new System.EventHandler(this.AlarmReport_Control_Load);
            this.pnlFilters.ResumeLayout(false);
            this.pnlFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAlarmReport)).EndInit();
            this.pnlPagination.ResumeLayout(false);
            this.pnlPagination.PerformLayout();
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.Panel pnlFilters;
        private MaterialSkin.Controls.MaterialButton btnGenerateReport; // Tür güncellendi
        private System.Windows.Forms.ComboBox cmbMachines;
        private MaterialSkin.Controls.MaterialLabel label3;            // Tür güncellendi
        private System.Windows.Forms.DateTimePicker dtpEndTime;
        private MaterialSkin.Controls.MaterialLabel label2;            // Tür güncellendi
        private System.Windows.Forms.DateTimePicker dtpStartTime;
        private MaterialSkin.Controls.MaterialLabel label1;            // Tür güncellendi
        private System.Windows.Forms.DataGridView dgvAlarmReport;
        private MaterialSkin.Controls.MaterialButton btnExportToExcel;  // Tür güncellendi
        private System.Windows.Forms.Panel pnlPagination;
        private MaterialSkin.Controls.MaterialButton btnPrev;           // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnNext;           // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel lblPageInfo;         // Tür güncellendi
    }
}