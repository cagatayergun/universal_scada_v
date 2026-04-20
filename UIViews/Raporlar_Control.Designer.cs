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
            tabPageAlarmReport = new TabPage();
            tabControlReports.SuspendLayout();
            SuspendLayout();
            // 
            // tabControlReports
            // 
            tabControlReports.Controls.Add(tabPageAlarmReport);
            tabControlReports.Dock = DockStyle.Fill;
            tabControlReports.Location = new Point(0, 0);
            tabControlReports.Margin = new Padding(3, 2, 3, 2);
            tabControlReports.Name = "tabControlReports";
            tabControlReports.SelectedIndex = 0;
            tabControlReports.Size = new Size(700, 450);
            tabControlReports.TabIndex = 0;
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
        private System.Windows.Forms.TabControl tabControlReports;
        private TabPage tabPageAlarmReport;
    }
}