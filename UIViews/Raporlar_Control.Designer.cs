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
            this.tabSelector = new MaterialSkin.Controls.MaterialTabSelector(); // YENİ: Sekme Gösterici
            this.tabControlReports = new MaterialSkin.Controls.MaterialTabControl();
            this.tabPageProductionReport = new System.Windows.Forms.TabPage();
            this.tabPageAlarmReport = new System.Windows.Forms.TabPage();
            this.tabPageEfficiency = new System.Windows.Forms.TabPage();
            this.tabPageTrendAnalysis = new System.Windows.Forms.TabPage();
            this.tabPageRecipeOptimization = new System.Windows.Forms.TabPage();
            this.tabPageManualReport = new System.Windows.Forms.TabPage();
            this.tabPageGenelUretim = new System.Windows.Forms.TabPage();
            this.tabPageActionLog = new System.Windows.Forms.TabPage();
            this.tabControlReports.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabSelector (KAYBOLAN SEKMELERİ GERİ GETİREN KONTROL)
            // 
            this.tabSelector.BaseTabControl = this.tabControlReports; // TAB KONTROLE BAĞLANDI
            this.tabSelector.CharacterCasing = MaterialSkin.Controls.MaterialTabSelector.CustomCharacterCasing.Normal;
            this.tabSelector.Depth = 0;
            this.tabSelector.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabSelector.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.tabSelector.Location = new System.Drawing.Point(0, 0);
            this.tabSelector.MouseState = MaterialSkin.MouseState.HOVER;
            this.tabSelector.Name = "tabSelector";
            this.tabSelector.Size = new System.Drawing.Size(700, 48); // Sekme yüksekliği
            this.tabSelector.TabIndex = 1;
            this.tabSelector.Text = "materialTabSelector1";
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
            this.tabControlReports.Location = new System.Drawing.Point(0, 48); // Selector'un altına hizalandı
            this.tabControlReports.MouseState = MaterialSkin.MouseState.HOVER;
            this.tabControlReports.Name = "tabControlReports";
            this.tabControlReports.SelectedIndex = 0;
            this.tabControlReports.Size = new System.Drawing.Size(700, 402);
            this.tabControlReports.TabIndex = 0;
            // 
            // tabPageProductionReport
            // 
            this.tabPageProductionReport.Location = new System.Drawing.Point(4, 24);
            this.tabPageProductionReport.Name = "tabPageProductionReport";
            this.tabPageProductionReport.Size = new System.Drawing.Size(692, 374);
            this.tabPageProductionReport.TabIndex = 1;
            this.tabPageProductionReport.Text = "Production Report";
            // 
            // tabPageAlarmReport
            // 
            this.tabPageAlarmReport.Location = new System.Drawing.Point(4, 24);
            this.tabPageAlarmReport.Name = "tabPageAlarmReport";
            this.tabPageAlarmReport.Size = new System.Drawing.Size(692, 374);
            this.tabPageAlarmReport.TabIndex = 0;
            this.tabPageAlarmReport.Text = "Past Alarms";
            // 
            // tabPageEfficiency
            // 
            this.tabPageEfficiency.Location = new System.Drawing.Point(4, 24);
            this.tabPageEfficiency.Name = "tabPageEfficiency";
            this.tabPageEfficiency.Size = new System.Drawing.Size(692, 374);
            this.tabPageEfficiency.TabIndex = 2;
            this.tabPageEfficiency.Text = "Verimlilik Analizi";
            // 
            // tabPageTrendAnalysis
            // 
            this.tabPageTrendAnalysis.Location = new System.Drawing.Point(4, 24);
            this.tabPageTrendAnalysis.Name = "tabPageTrendAnalysis";
            this.tabPageTrendAnalysis.Size = new System.Drawing.Size(692, 374);
            this.tabPageTrendAnalysis.TabIndex = 3;
            this.tabPageTrendAnalysis.Text = "Trend Analysis";
            // 
            // tabPageRecipeOptimization
            // 
            this.tabPageRecipeOptimization.Location = new System.Drawing.Point(4, 24);
            this.tabPageRecipeOptimization.Name = "tabPageRecipeOptimization";
            this.tabPageRecipeOptimization.Size = new System.Drawing.Size(692, 374);
            this.tabPageRecipeOptimization.TabIndex = 4;
            this.tabPageRecipeOptimization.Text = "Prescription Analysis";
            // 
            // tabPageManualReport
            // 
            this.tabPageManualReport.Location = new System.Drawing.Point(4, 24);
            this.tabPageManualReport.Name = "tabPageManualReport";
            this.tabPageManualReport.Size = new System.Drawing.Size(692, 374);
            this.tabPageManualReport.TabIndex = 5;
            this.tabPageManualReport.Text = "Manual Consumption";
            // 
            // tabPageGenelUretim
            // 
            this.tabPageGenelUretim.Location = new System.Drawing.Point(4, 24);
            this.tabPageGenelUretim.Name = "tabPageGenelUretim";
            this.tabPageGenelUretim.Size = new System.Drawing.Size(692, 374);
            this.tabPageGenelUretim.TabIndex = 6;
            this.tabPageGenelUretim.Text = "General Consumption";
            // 
            // tabPageActionLog
            // 
            this.tabPageActionLog.Location = new System.Drawing.Point(4, 24);
            this.tabPageActionLog.Name = "tabPageActionLog";
            this.tabPageActionLog.Size = new System.Drawing.Size(692, 374);
            this.tabPageActionLog.TabIndex = 7;
            this.tabPageActionLog.Text = "Action Records";
            // 
            // Raporlar_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControlReports);
            this.Controls.Add(this.tabSelector); // TAB SELECTOR FORMA EKLENDİ
            this.Name = "Raporlar_Control";
            this.Size = new System.Drawing.Size(700, 450);
            this.tabControlReports.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private MaterialSkin.Controls.MaterialTabSelector tabSelector; // Eklendi
        private MaterialSkin.Controls.MaterialTabControl tabControlReports;
        private System.Windows.Forms.TabPage tabPageProductionReport;
        private System.Windows.Forms.TabPage tabPageAlarmReport;
        private System.Windows.Forms.TabPage tabPageEfficiency;
        private System.Windows.Forms.TabPage tabPageTrendAnalysis;
        private System.Windows.Forms.TabPage tabPageRecipeOptimization;
        private System.Windows.Forms.TabPage tabPageManualReport;
        private System.Windows.Forms.TabPage tabPageGenelUretim;
        private System.Windows.Forms.TabPage tabPageActionLog;
    }
}