namespace TekstilScada.UI
{
    partial class VncViewer_Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            // Eğer yönetilen kaynaklar serbest bırakılıyorsa
            if (disposing)
            {
                // components konteynerini Dispose et
                if (components != null)
                {
                    components.Dispose();
                }

                // remoteDesktop1 kontrolü de burada dispose edilecektir çünkü components tarafından yönetilir.
                // Ancak, emin olmak için veya özel durumlar için burada açıkça Dispose çağrılabilir.
                // RemoteDesktop sınıfı kendi içindeki vnc ve desktop kaynaklarını temizlemelidir.
                // if (this.remoteDesktop1 != null) // Zaten components.Dispose() tarafından halledildiğinden genellikle gereksiz.
                // {
                //    this.remoteDesktop1.Dispose();
                // }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            remoteDesktop1 = new VncSharpCore.RemoteDesktop();
            SuspendLayout();
            // 
            // remoteDesktop1
            // 
            remoteDesktop1.AutoScroll = true;
            remoteDesktop1.AutoScrollMinSize = new Size(608, 427);
            remoteDesktop1.Dock = DockStyle.Fill;
            remoteDesktop1.Location = new Point(0, 0);
            remoteDesktop1.Margin = new Padding(3, 2, 3, 2);
            remoteDesktop1.Name = "remoteDesktop1";
            remoteDesktop1.Size = new Size(1197, 673);
            remoteDesktop1.TabIndex = 0;
            remoteDesktop1.ViewOnly = false;
            // 
            // VncViewer_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(1197, 673);
            Controls.Add(remoteDesktop1);
            Margin = new Padding(3, 2, 3, 2);
            Name = "VncViewer_Form";
            Text = "VNC Görüntüleyici";
            FormClosing += VncViewer_Form_FormClosing;
            Load += VncViewer_Form_Load;
            ResumeLayout(false);

        }

        #endregion

        private VncSharpCore.RemoteDesktop remoteDesktop1;
    }
}