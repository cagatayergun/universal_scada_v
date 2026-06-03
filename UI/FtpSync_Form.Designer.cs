// UI/FtpSync_Form.Designer.cs
namespace Telemetry.UI
{
    partial class FtpSync_Form
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
            this.pnlTop = new System.Windows.Forms.Panel();
            this.clbMachines = new System.Windows.Forms.CheckedListBox();
            this.label1 = new MaterialSkin.Controls.MaterialLabel(); // MaterialLabel olarak güncellendi
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lstLocalRecipes = new System.Windows.Forms.ListBox();
            this.pnlMiddle = new System.Windows.Forms.Panel();
            this.btnReceive = new MaterialSkin.Controls.MaterialButton(); // MaterialButton olarak güncellendi
            this.btnSend = new MaterialSkin.Controls.MaterialButton();    // MaterialButton olarak güncellendi
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lstHmiRecipes = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnDeleteRecipes = new MaterialSkin.Controls.MaterialButton(); // MaterialButton olarak güncellendi
            this.btnRefreshHmi = new MaterialSkin.Controls.MaterialButton();    // MaterialButton olarak güncellendi
            this.dgvTransfers = new System.Windows.Forms.DataGridView();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageTransfers = new System.Windows.Forms.TabPage();
            this.tabPagePreview = new System.Windows.Forms.TabPage();
            this.pnlPreviewArea = new System.Windows.Forms.Panel();
            this.lblPreviewStatus = new MaterialSkin.Controls.MaterialLabel(); // MaterialLabel olarak güncellendi
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.pnlMiddle.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTransfers)).BeginInit();
            this.tabControlMain.SuspendLayout();
            this.tabPageTransfers.SuspendLayout();
            this.tabPagePreview.SuspendLayout();
            this.pnlPreviewArea.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.clbMachines);
            this.pnlTop.Controls.Add(this.label1);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlTop.Location = new System.Drawing.Point(896, 72); // MaterialHeader altına hizalandı
            this.pnlTop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(257, 570);
            this.pnlTop.TabIndex = 0;
            // 
            // clbMachines
            // 
            this.clbMachines.FormattingEnabled = true;
            this.clbMachines.Location = new System.Drawing.Point(4, 32);
            this.clbMachines.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.clbMachines.Name = "clbMachines";
            this.clbMachines.Size = new System.Drawing.Size(248, 526);
            this.clbMachines.TabIndex = 1;
            this.clbMachines.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbMachines_ItemCheck);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Depth = 0;
            this.label1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.label1.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle2;
            this.label1.Location = new System.Drawing.Point(74, 10);
            this.label1.MouseState = MaterialSkin.MouseState.HOVER;
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Target Machines";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitContainer1.Location = new System.Drawing.Point(9, 72); // MaterialHeader altına hizalandı
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pnlMiddle);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Size = new System.Drawing.Size(887, 280);
            this.splitContainer1.SplitterDistance = 387;
            this.splitContainer1.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lstLocalRecipes);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(387, 280);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "LOCAL RECIPE";
            // 
            // lstLocalRecipes
            // 
            this.lstLocalRecipes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLocalRecipes.FormattingEnabled = true;
            this.lstLocalRecipes.ItemHeight = 15;
            this.lstLocalRecipes.Location = new System.Drawing.Point(3, 18);
            this.lstLocalRecipes.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lstLocalRecipes.Name = "lstLocalRecipes";
            this.lstLocalRecipes.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstLocalRecipes.Size = new System.Drawing.Size(381, 260);
            this.lstLocalRecipes.TabIndex = 0;
            // 
            // pnlMiddle
            // 
            this.pnlMiddle.Controls.Add(this.btnReceive);
            this.pnlMiddle.Controls.Add(this.btnSend);
            this.pnlMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMiddle.Location = new System.Drawing.Point(0, 0);
            this.pnlMiddle.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlMiddle.Name = "pnlMiddle";
            this.pnlMiddle.Size = new System.Drawing.Size(128, 280);
            this.pnlMiddle.TabIndex = 1;
            // 
            // btnReceive
            // 
            this.btnReceive.AutoSize = false;
            this.btnReceive.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnReceive.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnReceive.Depth = 0;
            this.btnReceive.HighEmphasis = true;
            this.btnReceive.Icon = null;
            this.btnReceive.Location = new System.Drawing.Point(10, 110);
            this.btnReceive.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnReceive.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnReceive.Name = "btnReceive";
            this.btnReceive.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnReceive.Size = new System.Drawing.Size(108, 38);
            this.btnReceive.TabIndex = 1;
            this.btnReceive.Text = "<< Receive";
            this.btnReceive.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnReceive.UseAccentColor = false; // Standart tema rengi
            this.btnReceive.UseVisualStyleBackColor = true;
            this.btnReceive.Click += new System.EventHandler(this.btnReceive_Click);
            // 
            // btnSend
            // 
            this.btnSend.AutoSize = false;
            this.btnSend.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSend.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSend.Depth = 0;
            this.btnSend.HighEmphasis = true;
            this.btnSend.Icon = null;
            this.btnSend.Location = new System.Drawing.Point(10, 50);
            this.btnSend.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSend.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSend.Name = "btnSend";
            this.btnSend.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSend.Size = new System.Drawing.Size(108, 38);
            this.btnSend.TabIndex = 0;
            this.btnSend.Text = "Send >>";
            this.btnSend.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSend.UseAccentColor = true; // Vurgu rengi (Aksan)
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lstHmiRecipes);
            this.groupBox2.Controls.Add(this.panel1);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Right;
            this.groupBox2.Location = new System.Drawing.Point(128, 0);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox2.Size = new System.Drawing.Size(368, 280);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "REMOTE RECIPES";
            // 
            // lstHmiRecipes
            // 
            this.lstHmiRecipes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstHmiRecipes.FormattingEnabled = true;
            this.lstHmiRecipes.ItemHeight = 15;
            this.lstHmiRecipes.Location = new System.Drawing.Point(3, 54);
            this.lstHmiRecipes.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lstHmiRecipes.Name = "lstHmiRecipes";
            this.lstHmiRecipes.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstHmiRecipes.Size = new System.Drawing.Size(362, 224);
            this.lstHmiRecipes.TabIndex = 0;
            this.lstHmiRecipes.SelectedIndexChanged += new System.EventHandler(this.lstHmiRecipes_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnDeleteRecipes);
            this.panel1.Controls.Add(this.btnRefreshHmi);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 18);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(362, 36);
            this.panel1.TabIndex = 1;
            // 
            // btnDeleteRecipes
            // 
            this.btnDeleteRecipes.AutoSize = false;
            this.btnDeleteRecipes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnDeleteRecipes.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Dense;
            this.btnDeleteRecipes.Depth = 0;
            this.btnDeleteRecipes.HighEmphasis = false;
            this.btnDeleteRecipes.Icon = null;
            this.btnDeleteRecipes.Location = new System.Drawing.Point(245, 3);
            this.btnDeleteRecipes.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnDeleteRecipes.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnDeleteRecipes.Name = "btnDeleteRecipes";
            this.btnDeleteRecipes.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnDeleteRecipes.Size = new System.Drawing.Size(114, 28);
            this.btnDeleteRecipes.TabIndex = 1;
            this.btnDeleteRecipes.Text = "Recipe Delete";
            this.btnDeleteRecipes.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnDeleteRecipes.UseAccentColor = false;
            this.btnDeleteRecipes.UseVisualStyleBackColor = true;
            this.btnDeleteRecipes.Click += new System.EventHandler(this.btnDeleteRecipes_Click);
            // 
            // btnRefreshHmi
            // 
            this.btnRefreshHmi.AutoSize = false;
            this.btnRefreshHmi.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRefreshHmi.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Dense;
            this.btnRefreshHmi.Depth = 0;
            this.btnRefreshHmi.HighEmphasis = false;
            this.btnRefreshHmi.Icon = null;
            this.btnRefreshHmi.Location = new System.Drawing.Point(3, 3);
            this.btnRefreshHmi.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnRefreshHmi.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRefreshHmi.Name = "btnRefreshHmi";
            this.btnRefreshHmi.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnRefreshHmi.Size = new System.Drawing.Size(114, 28);
            this.btnRefreshHmi.TabIndex = 0;
            this.btnRefreshHmi.Text = "Refresh List";
            this.btnRefreshHmi.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnRefreshHmi.UseAccentColor = false;
            this.btnRefreshHmi.UseVisualStyleBackColor = true;
            this.btnRefreshHmi.Click += new System.EventHandler(this.btnRefreshHmi_Click);
            // 
            // dgvTransfers
            // 
            this.dgvTransfers.AllowUserToAddRows = false;
            this.dgvTransfers.AllowUserToDeleteRows = false;
            this.dgvTransfers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTransfers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTransfers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTransfers.Location = new System.Drawing.Point(3, 2);
            this.dgvTransfers.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvTransfers.Name = "dgvTransfers";
            this.dgvTransfers.ReadOnly = true;
            this.dgvTransfers.RowHeadersWidth = 51;
            this.dgvTransfers.RowTemplate.Height = 29;
            this.dgvTransfers.Size = new System.Drawing.Size(873, 258);
            this.dgvTransfers.TabIndex = 2;
            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tabPageTransfers);
            this.tabControlMain.Controls.Add(this.tabPagePreview);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlMain.Location = new System.Drawing.Point(9, 352); // SplitContainer altına hizalandı
            this.tabControlMain.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(887, 290);
            this.tabControlMain.TabIndex = 3;
            // 
            // tabPageTransfers
            // 
            this.tabPageTransfers.Controls.Add(this.dgvTransfers);
            this.tabPageTransfers.Location = new System.Drawing.Point(4, 24);
            this.tabPageTransfers.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageTransfers.Name = "tabPageTransfers";
            this.tabPageTransfers.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageTransfers.Size = new System.Drawing.Size(879, 262);
            this.tabPageTransfers.TabIndex = 0;
            this.tabPageTransfers.Text = "Transfer List";
            this.tabPageTransfers.UseVisualStyleBackColor = true;
            // 
            // tabPagePreview
            // 
            this.tabPagePreview.Controls.Add(this.pnlPreviewArea);
            this.tabPagePreview.Location = new System.Drawing.Point(4, 24);
            this.tabPagePreview.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPagePreview.Name = "tabPagePreview";
            this.tabPagePreview.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPagePreview.Size = new System.Drawing.Size(879, 262);
            this.tabPagePreview.TabIndex = 1;
            this.tabPagePreview.Text = "Recipe Preview";
            this.tabPagePreview.UseVisualStyleBackColor = true;
            // 
            // pnlPreviewArea
            // 
            this.pnlPreviewArea.Controls.Add(this.lblPreviewStatus);
            this.pnlPreviewArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPreviewArea.Location = new System.Drawing.Point(3, 2);
            this.pnlPreviewArea.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlPreviewArea.Name = "pnlPreviewArea";
            this.pnlPreviewArea.Size = new System.Drawing.Size(873, 258);
            this.pnlPreviewArea.TabIndex = 0;
            // 
            // lblPreviewStatus
            // 
            this.lblPreviewStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPreviewStatus.Depth = 0;
            this.lblPreviewStatus.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblPreviewStatus.FontType = MaterialSkin.MaterialSkinManager.fontType.Body1;
            this.lblPreviewStatus.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblPreviewStatus.Location = new System.Drawing.Point(0, 0);
            this.lblPreviewStatus.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblPreviewStatus.Name = "lblPreviewStatus";
            this.lblPreviewStatus.Size = new System.Drawing.Size(873, 258);
            this.lblPreviewStatus.TabIndex = 0;
            this.lblPreviewStatus.Text = "Select a prescription from the HMI list for preview.";
            this.lblPreviewStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FtpSync_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1162, 650); // MaterialHeader bar yüksekliği (64px) hesaba katılarak form dikey boyutu artırıldı
            this.Controls.Add(this.tabControlMain);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.pnlTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None; // Çerçeve çizim sorumluluğu tamamen MaterialSkin kütüphanesine devredildi
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FtpSync_Form";
            this.Padding = new System.Windows.Forms.Padding(9, 72, 9, 8); // Üst padding 72px yapılarak içeriklerin başlık barının altında ezilmesi önlendi
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "RECIPE SYNCHRONIZATION";
            this.Load += new System.EventHandler(this.FtpSync_Form_Load);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.pnlMiddle.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTransfers)).EndInit();
            this.tabControlMain.ResumeLayout(false);
            this.tabPageTransfers.ResumeLayout(false);
            this.tabPagePreview.ResumeLayout(false);
            this.pnlPreviewArea.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.CheckedListBox clbMachines;
        private MaterialSkin.Controls.MaterialLabel label1; // Sınıf türü MaterialLabel yapıldı
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox lstLocalRecipes;
        private System.Windows.Forms.Panel pnlMiddle;
        private MaterialSkin.Controls.MaterialButton btnReceive; // Sınıf türü MaterialButton yapıldı
        private MaterialSkin.Controls.MaterialButton btnSend;    // Sınıf türü MaterialButton yapıldı
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox lstHmiRecipes;
        private System.Windows.Forms.Panel panel1;
        private MaterialSkin.Controls.MaterialButton btnRefreshHmi; // Sınıf türü MaterialButton yapıldı
        private System.Windows.Forms.DataGridView dgvTransfers;
        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageTransfers;
        private System.Windows.Forms.TabPage tabPagePreview;
        private System.Windows.Forms.Panel pnlPreviewArea;
        private MaterialSkin.Controls.MaterialLabel lblPreviewStatus; // Sınıf türü MaterialLabel yapıldı
        private MaterialSkin.Controls.MaterialButton btnDeleteRecipes; // Sınıf türü MaterialButton yapıldı
    }
}