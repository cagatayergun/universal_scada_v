// UI/Controls/WaterTankGauge.Designer.cs
namespace Telemetry.UI.Controls
{
    partial class WaterTankGauge
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
            this.SuspendLayout();
            // 
            // WaterTankGauge
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent; // Tasarım anında arkasında çirkin gri kutu oluşmasını önler
            this.Name = "WaterTankGauge";
            this.Size = new System.Drawing.Size(150, 190); // Alttaki KPI metinlerinin sığması için dikey boyutu 190'a genişletildi
            this.ResumeLayout(false);
        }

        #endregion
    }
}