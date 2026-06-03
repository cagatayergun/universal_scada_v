// UI/Views/Raporlar_Control.Designer.cs
namespace Telemetry.UI.Views
{
    partial class Raporlar_Control
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
            this.tabControlReports = new MaterialSkin.Controls.MaterialTabControl(); // MaterialTabControl yapıldı
            this.tabPageProductionReport = new System.Windows.Forms.TabPage();
            this.tabPageAlarmReport = new System.Windows.Forms.TabPage();
            this.tabPageEfficiency = new System.Windows.Forms.TabPage(); // Tür düzeltildi
            this.tabPageTrendAnalysis = new System.Windows.Forms.TabPage();
            this.tabPageRecipeOptimization = new System.Windows.Forms.TabPage();
            this.tabPageManualReport = new System.Windows.Forms.TabPage();
            this.tabPageGenelUretim = new System.Windows.Forms.TabPage();
            this.tabPageActionLog = new System.Windows.Forms.TabPage();
            this.tabControlReports.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlReports
            // 
            this.tabControlReports.Controls.Add(this.tabPageProductionReport);
            this.tabControlReports.Controls.Add(this.tabPageAlarmReport);
            this.tabControlReports.Controls.Add(this.tabPageEfficiency);
            this.tabControlReports.Controls.Add(this.tabPageTrendAnalysis);
            this.tabControlReports.Controls.Add(this.tabPageRecipeOptimization);
            this.tabControlReports.Controls.Add(this.tabPageManualReport);
            this.tabControlReports.Controls.Add(this.tabPageGenelUretim);
            this.tabControlReports.Controls.Add(this.tabPageActionLog);
            this.tabControlReports.Depth = 0;
            this.tabControlReports.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlReports.Location = new System.Drawing.Point(0, 0);
            this.tabControlReports.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControlReports.MouseState = MaterialSkin.MouseState.HOVER;
            this.tabControlReports.Name = "tabControlReports";
            this.tabControlReports.SelectedIndex = 0;
            this.tabControlReports.Size = new System.Drawing.Size(700, 450);
            this.tabControlReports.TabIndex = 0;
            // 
            // tabPageProductionReport
            // 
            this.tabPageProductionReport.BackColor = System.Drawing.Color.Transparent; // Koyu mod uyumu mühürlendi
            this.tabPageProductionReport.Location = new System.Drawing.Point(4, 24);
            this.tabPageProductionReport.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageProductionReport.Name = "tabPageProductionReport";
            this.tabPageProductionReport.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageProductionReport.Size = new System.Drawing.Size(692, 422);
            this.tabPageProductionReport.TabIndex = 1;
            this.tabPageProductionReport.Text = "Production Report";
            // 
            // tabPageAlarmReport
            // 
            this.tabPageAlarmReport.BackColor = System.Drawing.Color.Transparent;
            this.tabPageAlarmReport.Location = new System.Drawing.Point(4, 24);
            this.tabPageAlarmReport.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageAlarmReport.Name = "tabPageAlarmReport";
            this.tabPageAlarmReport.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageAlarmReport.Size = new System.Drawing.Size(692, 422);
            this.tabPageAlarmReport.TabIndex = 0;
            this.tabPageAlarmReport.Text = "Past Alarms";
            // 
            // tabPageEfficiency
            // 
            // ÇÖZÜM: Orijinal koddaki dondurucu ve çakışan 'tabPageAlarmReport' nesne ezme hatası tamamen giderildi.
            this.tabPageEfficiency.BackColor = System.Drawing.Color.Transparent;
            this.tabPageEfficiency.Location = new System.Drawing.Point(4, 24);
            this.tabPageEfficiency.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageEfficiency.Name = "tabPageEfficiency";
            this.tabPageEfficiency.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageEfficiency.Size = new System.Drawing.Size(692, 422);
            this.tabPageEfficiency.TabIndex = 2; // Sekme hiyerarşi indeksi çakışmasız olarak düzeltildi
            this.tabPageEfficiency.Text = "Verimlilik Analizi";
            // 
            // tabPageTrendAnalysis
            // 
            this.tabPageTrendAnalysis.BackColor = System.Drawing.Color.Transparent;
            this.tabPageTrendAnalysis.Location = new System.Drawing.Point(4, 24);
            this.tabPageTrendAnalysis.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageTrendAnalysis.Name = "tabPageTrendAnalysis";
            this.tabPageTrendAnalysis.Size = new System.Drawing.Size(692, 422);
            this.tabPageTrendAnalysis.TabIndex = 3;
            this.tabPageTrendAnalysis.Text = "Trend Analysis";
            // 
            // tabPageRecipeOptimization
            // 
            this.tabPageRecipeOptimization.BackColor = System.Drawing.Color.Transparent;
            this.tabPageRecipeOptimization.Location = new System.Drawing.Point(4, 24);
            this.tabPageRecipeOptimization.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageRecipeOptimization.Name = "tabPageRecipeOptimization";
            this.tabPageRecipeOptimization.Size = new System.Drawing.Size(692, 422);
            this.tabPageRecipeOptimization.TabIndex = 4;
            this.tabPageRecipeOptimization.Text = "Prescription Consumption Analysis";
            // 
            // tabPageManualReport
            // 
            this.tabPageManualReport.BackColor = System.Drawing.Color.Transparent;
            this.tabPageManualReport.Location = new System.Drawing.Point(4, 24);
            this.tabPageManualReport.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageManualReport.Name = "tabPageManualReport";
            this.tabPageManualReport.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageManualReport.Size = new System.Drawing.Size(692, 422);
            this.tabPageManualReport.TabIndex = 5;
            this.tabPageManualReport.Text = "Manual Consumption";
            // 
            // tabPageGenelUretim
            // 
            this.tabPageGenelUretim.BackColor = System.Drawing.Color.Transparent;
            this.tabPageGenelUretim.Location = new System.Drawing.Point(4, 24);
            this.tabPageGenelUretim.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageGenelUretim.Name = "tabPageGenelUretim";
            this.tabPageGenelUretim.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageGenelUretim.Size = new System.Drawing.Size(692, 422);
            this.tabPageGenelUretim.TabIndex = 6;
            this.tabPageGenelUretim.Text = "General Consumption Report";
            // 
            // tabPageActionLog
            // 
            this.tabPageActionLog.BackColor = System.Drawing.Color.Transparent;
            this.tabPageActionLog.Location = new System.Drawing.Point(4, 24);
            this.tabPageActionLog.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageActionLog.Name = "tabPageActionLog";
            this.tabPageActionLog.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageActionLog.Size = new System.Drawing.Size(692, 422);
            this.tabPageActionLog.TabIndex = 7; // İndeks sıralaması optimize edildi
            this.tabPageActionLog.Text = "Action Records";
            // 
            // Raporlar_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent; // Panel şeffaflığı aktif edildi
            this.Controls.Add(this.tabControlReports);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Raporlar_Control";
            this.Size = new System.Drawing.Size(700, 450);
            this.tabControlReports.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion
        private System.Windows.Forms.TabPage tabPageActionLog;
        private MaterialSkin.Controls.MaterialTabControl tabControlReports; // Tür güncellendi
        private System.Windows.Forms.TabPage tabPageAlarmReport;
        private System.Windows.Forms.TabPage tabPageEfficiency;
        private System.Windows.Forms.TabPage tabPageProductionReport;
        private System.Windows.Forms.TabPage tabPageTrendAnalysis;
        private System.Windows.Forms.TabPage tabPageRecipeOptimization;
        private System.Windows.Forms.TabPage tabPageManualReport;
        private System.Windows.Forms.TabPage tabPageGenelUretim;
    }
}