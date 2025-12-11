namespace TekstilScada.UI.Views
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
            tabControlReports = new TabControl();
            tabPageProductionReport = new TabPage();
            tabPageAlarmReport = new TabPage();
            tabPageTrendAnalysis = new TabPage();
            tabPageRecipeOptimization = new TabPage();
            tabPageManualReport = new TabPage();
            tabPageGenelUretim = new TabPage();
            tabPageActionLog = new TabPage();
            tabControlReports.SuspendLayout();
            SuspendLayout();
            // 
            // tabControlReports
            // 
            tabControlReports.Controls.Add(tabPageProductionReport);
            tabControlReports.Controls.Add(tabPageAlarmReport);
            tabControlReports.Controls.Add(tabPageTrendAnalysis);
            tabControlReports.Controls.Add(tabPageRecipeOptimization);
            tabControlReports.Controls.Add(tabPageManualReport);
            tabControlReports.Controls.Add(tabPageGenelUretim);
            tabControlReports.Controls.Add(tabPageActionLog);
            tabControlReports.Dock = DockStyle.Fill;
            tabControlReports.Location = new Point(0, 0);
            tabControlReports.Margin = new Padding(3, 2, 3, 2);
            tabControlReports.Name = "tabControlReports";
            tabControlReports.SelectedIndex = 0;
            tabControlReports.Size = new Size(700, 450);
            tabControlReports.TabIndex = 0;
            // 
            // tabPageProductionReport
            // 
            tabPageProductionReport.Location = new Point(4, 24);
            tabPageProductionReport.Margin = new Padding(3, 2, 3, 2);
            tabPageProductionReport.Name = "tabPageProductionReport";
            tabPageProductionReport.Padding = new Padding(3, 2, 3, 2);
            tabPageProductionReport.Size = new Size(692, 422);
            tabPageProductionReport.TabIndex = 1;
            tabPageProductionReport.Text = "Production Report";
            tabPageProductionReport.UseVisualStyleBackColor = true;
            // 
            // tabPageAlarmReport
            // 
            tabPageAlarmReport.Location = new Point(4, 24);
            tabPageAlarmReport.Margin = new Padding(3, 2, 3, 2);
            tabPageAlarmReport.Name = "tabPageAlarmReport";
            tabPageAlarmReport.Padding = new Padding(3, 2, 3, 2);
            tabPageAlarmReport.Size = new Size(692, 422);
            tabPageAlarmReport.TabIndex = 0;
            tabPageAlarmReport.Text = "Past Alarms";
            tabPageAlarmReport.UseVisualStyleBackColor = true;
            // 
            // tabPageTrendAnalysis
            // 
            tabPageTrendAnalysis.Location = new Point(4, 24);
            tabPageTrendAnalysis.Margin = new Padding(3, 2, 3, 2);
            tabPageTrendAnalysis.Name = "tabPageTrendAnalysis";
            tabPageTrendAnalysis.Size = new Size(692, 422);
            tabPageTrendAnalysis.TabIndex = 3;
            tabPageTrendAnalysis.Text = "Trend Analysis";
            // 
            // tabPageRecipeOptimization
            // 
            tabPageRecipeOptimization.Location = new Point(4, 24);
            tabPageRecipeOptimization.Margin = new Padding(3, 2, 3, 2);
            tabPageRecipeOptimization.Name = "tabPageRecipeOptimization";
            tabPageRecipeOptimization.Size = new Size(692, 422);
            tabPageRecipeOptimization.TabIndex = 4;
            tabPageRecipeOptimization.Text = "Prescription Consumption Analysis";
            tabPageRecipeOptimization.UseVisualStyleBackColor = true;
            // 
            // tabPageManualReport
            // 
            tabPageManualReport.Location = new Point(4, 24);
            tabPageManualReport.Margin = new Padding(3, 2, 3, 2);
            tabPageManualReport.Name = "tabPageManualReport";
            tabPageManualReport.Padding = new Padding(3, 2, 3, 2);
            tabPageManualReport.Size = new Size(692, 422);
            tabPageManualReport.TabIndex = 5;
            tabPageManualReport.Text = "Manual Consumption";
            tabPageManualReport.UseVisualStyleBackColor = true;
            // 
            // tabPageGenelUretim
            // 
            tabPageGenelUretim.Location = new Point(4, 24);
            tabPageGenelUretim.Margin = new Padding(3, 2, 3, 2);
            tabPageGenelUretim.Name = "tabPageGenelUretim";
            tabPageGenelUretim.Padding = new Padding(3, 2, 3, 2);
            tabPageGenelUretim.Size = new Size(692, 422);
            tabPageGenelUretim.TabIndex = 6;
            tabPageGenelUretim.Text = "General Consumption Report";
            tabPageGenelUretim.UseVisualStyleBackColor = true;
            // 
            // tabPageActionLog
            // 
            tabPageActionLog.Location = new Point(4, 24);
            tabPageActionLog.Margin = new Padding(3, 2, 3, 2);
            tabPageActionLog.Name = "tabPageActionLog";
            tabPageActionLog.Padding = new Padding(3, 2, 3, 2);
            tabPageActionLog.Size = new Size(692, 422);
            tabPageActionLog.TabIndex = 4;
            tabPageActionLog.Text = "Action Records";
            tabPageActionLog.UseVisualStyleBackColor = true;
            // 
            // Raporlar_Control
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tabControlReports);
            Margin = new Padding(3, 2, 3, 2);
            Name = "Raporlar_Control";
            Size = new Size(700, 450);
            tabControlReports.ResumeLayout(false);
            ResumeLayout(false);
        }
        #endregion
        private System.Windows.Forms.TabPage tabPageActionLog;
        private System.Windows.Forms.TabControl tabControlReports;
        private System.Windows.Forms.TabPage tabPageAlarmReport;
        private System.Windows.Forms.TabPage tabPageProductionReport;
        private System.Windows.Forms.TabPage tabPageTrendAnalysis;
        private System.Windows.Forms.TabPage tabPageRecipeOptimization;
        private System.Windows.Forms.TabPage tabPageManualReport; // YENİ
        private System.Windows.Forms.TabPage tabPageGenelUretim; // YENİ
    }
}