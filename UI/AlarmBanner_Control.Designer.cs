// UI/Controls/AlarmBanner_Control.Designer.cs
namespace Telemetry.UI.Controls
{
    partial class AlarmBanner_Control
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) { components.Dispose(); }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        private void InitializeComponent()
        {
            this.lblAlarmText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblAlarmText
            // 
            this.lblAlarmText.Dock = System.Windows.Forms.DockStyle.Fill;
            // Modern flat tasarım için font Segoe UI Semibold yapıldı
            this.lblAlarmText.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblAlarmText.ForeColor = System.Drawing.Color.White;
            this.lblAlarmText.Location = new System.Drawing.Point(0, 0);
            this.lblAlarmText.Name = "lblAlarmText";
            this.lblAlarmText.Size = new System.Drawing.Size(800, 42);
            this.lblAlarmText.TabIndex = 0;
            this.lblAlarmText.Text = "[MAKINE] - ALARM #101: ACİL DURUM BUTONUNA BASILDI";
            this.lblAlarmText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AlarmBanner_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(57)))), ((int)(((byte)(53))))); // Varsayılan Material Red
            this.Controls.Add(this.lblAlarmText);
            this.Name = "AlarmBanner_Control";
            this.Size = new System.Drawing.Size(800, 42);
            this.ResumeLayout(false);
        }
        #endregion
        private System.Windows.Forms.Label lblAlarmText;
    }
}