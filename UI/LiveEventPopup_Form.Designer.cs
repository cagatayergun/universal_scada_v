// UI/LiveEventPopup_Form.Designer.cs
namespace Telemetry.UI
{
    partial class LiveEventPopup_Form
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) { components.Dispose(); }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.lstEvents = new System.Windows.Forms.ListView();
            this.colTime = new System.Windows.Forms.ColumnHeader();
            this.colSource = new System.Windows.Forms.ColumnHeader();
            this.colMessage = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // lstEvents
            // 
            // SPEED & LAYOUT OPTİMİZASYONU: Form boyutu değiştikçe listenin taşmasını önler ve Header altında hizalar
            this.lstEvents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstEvents.BorderStyle = System.Windows.Forms.BorderStyle.None; // Modern düz (flat) görünüm için kenarlık kaldırıldı
            this.lstEvents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colTime,
            this.colSource,
            this.colMessage});
            this.lstEvents.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lstEvents.FullRowSelect = true;
            this.lstEvents.HideSelection = false;
            this.lstEvents.Location = new System.Drawing.Point(3, 65); // MaterialHeader başlık çubuğunun altına yerleştirildi
            this.lstEvents.Name = "lstEvents";
            this.lstEvents.Size = new System.Drawing.Size(594, 296);
            this.lstEvents.TabIndex = 0;
            this.lstEvents.UseCompatibleStateImageBehavior = false;
            this.lstEvents.View = System.Windows.Forms.View.Details;
            // 
            // colTime
            // 
            this.colTime.Text = "Time";
            this.colTime.Width = 85;
            // 
            // colSource
            // 
            this.colSource.Text = "Source";
            this.colSource.Width = 125;
            // 
            // colMessage
            // 
            this.colMessage.Text = "Message";
            this.colMessage.Width = 360;
            // 
            // LiveEventPopup_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 364); // Toplam yükseklik üst bar payı kadar (64px) artırıldı
            this.Controls.Add(this.lstEvents);
            this.MaximizeBox = false; // SCADA akış pencerelerinde ekranın kaplanmasını önler
            this.Name = "LiveEventPopup_Form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Live Event Stream";
            this.TopMost = true; // Her zaman otomasyon ekranının üstünde kalmasını sağlar
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LiveEventPopup_Form_FormClosing);
            this.ResumeLayout(false);
        }
        #endregion
        private System.Windows.Forms.ListView lstEvents;
        private System.Windows.Forms.ColumnHeader colTime;
        private System.Windows.Forms.ColumnHeader colSource;
        private System.Windows.Forms.ColumnHeader colMessage;
    }
}