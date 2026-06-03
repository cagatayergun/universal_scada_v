// UI/VncViewer_Form.Designer.cs
namespace Telemetry.UI
{
    partial class VncViewer_Form
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.remoteDesktop1 = new VncSharpCore.RemoteDesktop();
            this.SuspendLayout();
            // 
            // remoteDesktop1
            // 
            this.remoteDesktop1.AutoScroll = true;
            this.remoteDesktop1.AutoScrollMinSize = new System.Drawing.Size(608, 427);
            this.remoteDesktop1.Dock = System.Windows.Forms.DockStyle.Fill; // Form alanını tamamen kapla
            this.remoteDesktop1.Location = new System.Drawing.Point(3, 64); // Form Padding kuralına göre konumlanır
            this.remoteDesktop1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.remoteDesktop1.Name = "remoteDesktop1";
            this.remoteDesktop1.Size = new System.Drawing.Size(1194, 653); // Başlık barı payı çıkarılmış net boyut
            this.remoteDesktop1.TabIndex = 0;
            this.remoteDesktop1.ViewOnly = false;
            // 
            // VncViewer_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1200, 720); // Üst bar payı hesaba katılarak form boyutu hafifçe genişletildi
            this.Controls.Add(this.remoteDesktop1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None; // Çerçeve ve kenarlık çizimlerini Material kütüphanesi üstlenir
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "VncViewer_Form";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 3, 3); // Üst padding 64px yapılarak VNC ekranının başlık barı altında ezilmesi önlendi
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VNC Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VncViewer_Form_FormClosing);
            this.Load += new System.EventHandler(this.VncViewer_Form_Load);
            this.ResumeLayout(false);
        }

        #endregion

        private VncSharpCore.RemoteDesktop remoteDesktop1;
    }
}