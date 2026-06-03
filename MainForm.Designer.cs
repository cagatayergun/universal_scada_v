// MainForm.Designer.cs
namespace Telemetry
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.pnlNavigation = new System.Windows.Forms.Panel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnAyarlar = new MaterialSkin.Controls.MaterialButton();       // MaterialButton yapıldı
            this.btnRaporlar = new MaterialSkin.Controls.MaterialButton();      // MaterialButton yapıldı
            this.btnProsesKontrol = new MaterialSkin.Controls.MaterialButton(); // MaterialButton yapıldı
            this.btnProsesIzleme = new MaterialSkin.Controls.MaterialButton();  // MaterialButton yapıldı
            this.btnGenelBakis = new MaterialSkin.Controls.MaterialButton();    // MaterialButton yapıldı
            this.pnlContent = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.dilToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.türkçeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.englishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.oturumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.çıkışYapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatusCurrentUser = new System.Windows.Forms.ToolStripStatusLabel();
            this.springLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusLiveEvents = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnlNavigation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlNavigation
            // 
            this.pnlNavigation.BackColor = System.Drawing.Color.Transparent; // Koyu mod panel parlamasını engelleyen şeffaflık
            this.pnlNavigation.Controls.Add(this.pictureBox2);
            this.pnlNavigation.Controls.Add(this.pictureBox1);
            this.pnlNavigation.Controls.Add(this.btnAyarlar);
            this.pnlNavigation.Controls.Add(this.btnRaporlar);
            this.pnlNavigation.Controls.Add(this.btnProsesKontrol);
            this.pnlNavigation.Controls.Add(this.btnProsesIzleme);
            this.pnlNavigation.Controls.Add(this.btnGenelBakis);
            this.pnlNavigation.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlNavigation.Location = new System.Drawing.Point(3, 88); // MaterialForm üst padding payı dengelendi
            this.pnlNavigation.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlNavigation.Name = "pnlNavigation";
            this.pnlNavigation.Size = new System.Drawing.Size(185, 305); // Buton genişlik sınırları için 185px yapıldı
            this.pnlNavigation.TabIndex = 0;
            // 
            // btnGenelBakis
            // 
            this.btnGenelBakis.AutoSize = false;
            this.btnGenelBakis.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnGenelBakis.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnGenelBakis.Depth = 0;
            this.btnGenelBakis.HighEmphasis = false;
            this.btnGenelBakis.Icon = null;
            this.btnGenelBakis.Location = new System.Drawing.Point(5, 5);
            this.btnGenelBakis.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnGenelBakis.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnGenelBakis.Name = "btnGenelBakis";
            this.btnGenelBakis.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnGenelBakis.Size = new System.Drawing.Size(175, 36); // Dikey 36px material standardına eşitlendi
            this.btnGenelBakis.TabIndex = 4;
            this.btnGenelBakis.Text = "Genel Bakış";
            this.btnGenelBakis.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Text; // Flat şeffaf stil
            this.btnGenelBakis.UseAccentColor = false;
            this.btnGenelBakis.UseVisualStyleBackColor = true;
            this.btnGenelBakis.Click += new System.EventHandler(this.btnGenelBakis_Click);
            // 
            // btnProsesIzleme
            // 
            this.btnProsesIzleme.AutoSize = false;
            this.btnProsesIzleme.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnProsesIzleme.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnProsesIzleme.Depth = 0;
            this.btnProsesIzleme.HighEmphasis = false;
            this.btnProsesIzleme.Icon = null;
            this.btnProsesIzleme.Location = new System.Drawing.Point(5, 46); // Buton konumları dikeyde simetrikleştirildi
            this.btnProsesIzleme.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnProsesIzleme.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnProsesIzleme.Name = "btnProsesIzleme";
            this.btnProsesIzleme.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnProsesIzleme.Size = new System.Drawing.Size(175, 36);
            this.btnProsesIzleme.TabIndex = 0;
            this.btnProsesIzleme.Text = "Proses İzleme";
            this.btnProsesIzleme.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Text;
            this.btnProsesIzleme.UseAccentColor = false;
            this.btnProsesIzleme.UseVisualStyleBackColor = true;
            this.btnProsesIzleme.Click += new System.EventHandler(this.btnProsesIzleme_Click);
            // 
            // btnProsesKontrol
            // 
            this.btnProsesKontrol.AutoSize = false;
            this.btnProsesKontrol.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnProsesKontrol.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnProsesKontrol.Depth = 0;
            this.btnProsesKontrol.HighEmphasis = false;
            this.btnProsesKontrol.Icon = null;
            this.btnProsesKontrol.Location = new System.Drawing.Point(5, 87);
            this.btnProsesKontrol.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnProsesKontrol.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnProsesKontrol.Name = "btnProsesKontrol";
            this.btnProsesKontrol.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnProsesKontrol.Size = new System.Drawing.Size(175, 36);
            this.btnProsesKontrol.TabIndex = 1;
            this.btnProsesKontrol.Text = "Proses Kontrol";
            this.btnProsesKontrol.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Text;
            this.btnProsesKontrol.UseAccentColor = false;
            this.btnProsesKontrol.UseVisualStyleBackColor = true;
            this.btnProsesKontrol.Click += new System.EventHandler(this.btnProsesKontrol_Click);
            // 
            // btnRaporlar
            // 
            this.btnRaporlar.AutoSize = false;
            this.btnRaporlar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRaporlar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnRaporlar.Depth = 0;
            this.btnRaporlar.HighEmphasis = false;
            this.btnRaporlar.Icon = null;
            this.btnRaporlar.Location = new System.Drawing.Point(5, 128);
            this.btnRaporlar.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnRaporlar.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRaporlar.Name = "btnRaporlar";
            this.btnRaporlar.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnRaporlar.Size = new System.Drawing.Size(175, 36);
            this.btnRaporlar.TabIndex = 2;
            this.btnRaporlar.Text = "Raporlar";
            this.btnRaporlar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Text;
            this.btnRaporlar.UseAccentColor = false;
            this.btnRaporlar.UseVisualStyleBackColor = true;
            this.btnRaporlar.Click += new System.EventHandler(this.btnRaporlar_Click);
            // 
            // btnAyarlar
            // 
            this.btnAyarlar.AutoSize = false;
            this.btnAyarlar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAyarlar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnAyarlar.Depth = 0;
            this.btnAyarlar.HighEmphasis = false;
            this.btnAyarlar.Icon = null;
            this.btnAyarlar.Location = new System.Drawing.Point(5, 169);
            this.btnAyarlar.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnAyarlar.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnAyarlar.Name = "btnAyarlar";
            this.btnAyarlar.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnAyarlar.Size = new System.Drawing.Size(175, 36);
            this.btnAyarlar.TabIndex = 3;
            this.btnAyarlar.Text = "Ayarlar";
            this.btnAyarlar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Text;
            this.btnAyarlar.UseAccentColor = false;
            this.btnAyarlar.UseVisualStyleBackColor = true;
            this.btnAyarlar.Click += new System.EventHandler(this.btnAyarlar_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.pictureBox1.Image = global::Telemetry.Properties.Resource1.yilmak2___Kopya;
            this.pictureBox1.Location = new System.Drawing.Point(61, 210);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(63, 55);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.pictureBox2.Image = global::Telemetry.Properties.Resource1.yilmak;
            this.pictureBox2.Location = new System.Drawing.Point(5, 265);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(175, 40);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 6;
            this.pictureBox2.TabStop = false;
            // 
            // pnlContent
            // 
            this.pnlContent.BackColor = System.Drawing.Color.Transparent;
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(188, 88);
            this.pnlContent.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(703, 305);
            this.pnlContent.TabIndex = 1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dilToolStripMenuItem,
            this.oturumToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(3, 64); // MaterialForm üst bar hizalaması
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(888, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // dilToolStripMenuItem
            // 
            this.dilToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.türkçeToolStripMenuItem,
            this.englishToolStripMenuItem});
            this.dilToolStripMenuItem.Name = "dilToolStripMenuItem";
            this.dilToolStripMenuItem.Size = new System.Drawing.Size(33, 20);
            this.dilToolStripMenuItem.Text = "Dil";
            // 
            // türkçeToolStripMenuItem
            // 
            this.türkçeToolStripMenuItem.Name = "türkçeToolStripMenuItem";
            this.türkçeToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.türkçeToolStripMenuItem.Text = "Türkçe";
            this.türkçeToolStripMenuItem.Click += new System.EventHandler(this.türkçeToolStripMenuItem_Click);
            // 
            // englishToolStripMenuItem
            // 
            this.englishToolStripMenuItem.Name = "englishToolStripMenuItem";
            this.englishToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.englishToolStripMenuItem.Text = "English";
            this.englishToolStripMenuItem.Click += new System.EventHandler(this.englishToolStripMenuItem_Click);
            // 
            // oturumToolStripMenuItem
            // 
            this.oturumToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.çıkışYapToolStripMenuItem});
            this.oturumToolStripMenuItem.Name = "oturumToolStripMenuItem";
            this.oturumToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.oturumToolStripMenuItem.Text = "Oturum";
            // 
            // çıkışYapToolStripMenuItem
            // 
            this.çıkışYapToolStripMenuItem.Name = "çıkışYapToolStripMenuItem";
            this.çıkışYapToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.çıkışYapToolStripMenuItem.Text = "Oturum";
            this.çıkışYapToolStripMenuItem.Click += new System.EventHandler(this.çıkışYapToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatusCurrentUser,
            this.springLabel,
            this.lblStatusLiveEvents});
            this.statusStrip1.Location = new System.Drawing.Point(3, 393);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 12, 0);
            this.statusStrip1.Size = new System.Drawing.Size(888, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatusCurrentUser
            // 
            this.lblStatusCurrentUser.Name = "lblStatusCurrentUser";
            this.lblStatusCurrentUser.Size = new System.Drawing.Size(76, 17);
            this.lblStatusCurrentUser.Text = "Giriş Yapan: -";
            // 
            // springLabel
            // 
            this.springLabel.Name = "springLabel";
            this.springLabel.Size = new System.Drawing.Size(673, 17);
            this.springLabel.Spring = true;
            // 
            // lblStatusLiveEvents
            // 
            this.lblStatusLiveEvents.IsLink = true;
            this.lblStatusLiveEvents.Name = "lblStatusLiveEvents";
            this.lblStatusLiveEvents.Size = new System.Drawing.Size(126, 17);
            this.lblStatusLiveEvents.Text = "Canlı Olay Akışı Göster";
            this.lblStatusLiveEvents.Click += new System.EventHandler(this.lblStatusLiveEvents_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(894, 418);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlNavigation);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MainForm";
            this.Text = "Telemetry SCADA Sistemi";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.pnlNavigation.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlNavigation;
        private MaterialSkin.Controls.MaterialButton btnAyarlar;         // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnRaporlar;        // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnProsesKontrol;   // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnProsesIzleme;    // Tür güncellendi
        private System.Windows.Forms.Panel pnlContent;
        private MaterialSkin.Controls.MaterialButton btnGenelBakis;      // Tür güncellendi
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem dilToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem türkçeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem englishToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusCurrentUser;
        private System.Windows.Forms.ToolStripStatusLabel springLabel;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusLiveEvents;
        private System.Windows.Forms.ToolStripMenuItem oturumToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem çıkışYapToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}