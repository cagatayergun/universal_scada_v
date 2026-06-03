// UIViews/UtilitySettings_Control.Designer.cs
namespace Telemetry.UIViews
{
    partial class UtilitySettings_Control
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lstLines = new System.Windows.Forms.ListBox();
            this.panelListHeader = new System.Windows.Forms.Panel();
            this.btnDeleteLine = new MaterialSkin.Controls.MaterialButton(); // MaterialButton yapıldı
            this.btnAddLine = new MaterialSkin.Controls.MaterialButton();    // MaterialButton yapıldı
            this.lblListHeader = new MaterialSkin.Controls.MaterialLabel();    // MaterialLabel yapıldı
            this.flowLayoutPanelSensors = new System.Windows.Forms.FlowLayoutPanel();
            this.grpGeneral = new System.Windows.Forms.GroupBox();
            this.txtSlaveId = new System.Windows.Forms.NumericUpDown();
            this.label4 = new MaterialSkin.Controls.MaterialLabel();         // MaterialLabel yapıldı
            this.txtPort = new System.Windows.Forms.NumericUpDown();
            this.label3 = new MaterialSkin.Controls.MaterialLabel();         // MaterialLabel yapıldı
            this.txtIpAddress = new System.Windows.Forms.TextBox();
            this.label2 = new MaterialSkin.Controls.MaterialLabel();         // MaterialLabel yapıldı
            this.txtLineName = new System.Windows.Forms.TextBox();
            this.label1 = new MaterialSkin.Controls.MaterialLabel();         // MaterialLabel yapıldı
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnSave = new MaterialSkin.Controls.MaterialButton();       // MaterialButton yapıldı
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelListHeader.SuspendLayout();
            this.grpGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSlaveId)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.Color.Transparent;
            this.splitContainer1.Panel1.Controls.Add(this.lstLines);
            this.splitContainer1.Panel1.Controls.Add(this.panelListHeader);
            this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(5);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.Transparent;
            this.splitContainer1.Panel2.Controls.Add(this.flowLayoutPanelSensors);
            this.splitContainer1.Panel2.Controls.Add(this.grpGeneral);
            this.splitContainer1.Panel2.Controls.Add(this.panelBottom);
            this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(5);
            this.splitContainer1.Size = new System.Drawing.Size(1000, 650);
            this.splitContainer1.SplitterDistance = 250;
            this.splitContainer1.TabIndex = 0;
            // 
            // lstLines
            // 
            this.lstLines.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLines.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lstLines.FormattingEnabled = true;
            this.lstLines.ItemHeight = 17;
            this.lstLines.Location = new System.Drawing.Point(5, 51); // Header genişlemesine göre koordinat kaydırıldı
            this.lstLines.Name = "lstLines";
            this.lstLines.Size = new System.Drawing.Size(240, 594);
            this.lstLines.TabIndex = 1;
            this.lstLines.SelectedIndexChanged += new System.EventHandler(this.lstLines_SelectedIndexChanged);
            // 
            // panelListHeader
            // 
            this.panelListHeader.BackColor = System.Drawing.Color.Transparent;
            this.panelListHeader.Controls.Add(this.btnDeleteLine);
            this.panelListHeader.Controls.Add(this.btnAddLine);
            this.panelListHeader.Controls.Add(this.lblListHeader);
            this.panelListHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelListHeader.Location = new System.Drawing.Point(5, 5);
            this.panelListHeader.Name = "panelListHeader";
            this.panelListHeader.Size = new System.Drawing.Size(240, 46); // 36px flat material butonlar için panel esnetildi
            this.panelListHeader.TabIndex = 0;
            // 
            // btnDeleteLine
            // 
            this.btnDeleteLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteLine.AutoSize = false;
            this.btnDeleteLine.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnDeleteLine.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnDeleteLine.Depth = 0;
            this.btnDeleteLine.HighEmphasis = false;
            this.btnDeleteLine.Icon = null;
            this.btnDeleteLine.Location = new System.Drawing.Point(202, 5); // Dikey hizalama kalibre edildi
            this.btnDeleteLine.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnDeleteLine.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnDeleteLine.Name = "btnDeleteLine";
            this.btnDeleteLine.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnDeleteLine.Size = new System.Drawing.Size(35, 36);
            this.btnDeleteLine.TabIndex = 2;
            this.btnDeleteLine.Text = "-";
            this.btnDeleteLine.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnDeleteLine.UseAccentColor = false;
            this.btnDeleteLine.UseVisualStyleBackColor = true;
            this.btnDeleteLine.Click += new System.EventHandler(this.btnDeleteLine_Click);
            // 
            // btnAddLine
            // 
            this.btnAddLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddLine.AutoSize = false;
            this.btnAddLine.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddLine.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnAddLine.Depth = 0;
            this.btnAddLine.HighEmphasis = false;
            this.btnAddLine.Icon = null;
            this.btnAddLine.Location = new System.Drawing.Point(160, 5);
            this.btnAddLine.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnAddLine.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnAddLine.Name = "btnAddLine";
            this.btnAddLine.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnAddLine.Size = new System.Drawing.Size(35, 36);
            this.btnAddLine.TabIndex = 1;
            this.btnAddLine.Text = "+";
            this.btnAddLine.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnAddLine.UseAccentColor = false;
            this.btnAddLine.UseVisualStyleBackColor = true;
            this.btnAddLine.Click += new System.EventHandler(this.btnAddLine_Click);
            // 
            // lblListHeader
            // 
            this.lblListHeader.AutoSize = true;
            this.lblListHeader.Depth = 0;
            this.lblListHeader.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblListHeader.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle2;
            this.lblListHeader.Location = new System.Drawing.Point(5, 14);
            this.lblListHeader.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblListHeader.Name = "lblListHeader";
            this.lblListHeader.Size = new System.Drawing.Size(89, 17);
            this.lblListHeader.TabIndex = 0;
            this.lblListHeader.Text = "Kayıtlı Hatlar";
            // 
            // grpGeneral
            // 
            this.grpGeneral.BackColor = System.Drawing.Color.Transparent;
            this.grpGeneral.Controls.Add(this.txtSlaveId);
            this.grpGeneral.Controls.Add(this.label4);
            this.grpGeneral.Controls.Add(this.txtPort);
            this.grpGeneral.Controls.Add(this.label3);
            this.grpGeneral.Controls.Add(this.txtIpAddress);
            this.grpGeneral.Controls.Add(this.label2);
            this.grpGeneral.Controls.Add(this.txtLineName);
            this.grpGeneral.Controls.Add(this.label1);
            this.grpGeneral.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpGeneral.ForeColor = System.Drawing.Color.Gray; // Sınır çizgisi rengi
            this.grpGeneral.Location = new System.Drawing.Point(5, 5);
            this.grpGeneral.Name = "grpGeneral";
            this.grpGeneral.Size = new System.Drawing.Size(736, 100);
            this.grpGeneral.TabIndex = 0;
            this.grpGeneral.TabStop = false;
            this.grpGeneral.Text = "Genel Bağlantı Ayarları";
            // 
            // txtSlaveId
            // 
            this.txtSlaveId.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSlaveId.Location = new System.Drawing.Point(620, 45);
            this.txtSlaveId.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            this.txtSlaveId.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.txtSlaveId.Name = "txtSlaveId";
            this.txtSlaveId.Size = new System.Drawing.Size(80, 23);
            this.txtSlaveId.TabIndex = 7;
            this.txtSlaveId.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Depth = 0;
            this.label4.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label4.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label4.Location = new System.Drawing.Point(617, 25);
            this.label4.MouseState = MaterialSkin.MouseState.HOVER;
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 17);
            this.label4.TabIndex = 6;
            this.label4.Text = "Slave ID";
            // 
            // txtPort
            // 
            this.txtPort.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPort.Location = new System.Drawing.Point(510, 45);
            this.txtPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(90, 23);
            this.txtPort.TabIndex = 5;
            this.txtPort.Value = new decimal(new int[] { 502, 0, 0, 0 });
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Depth = 0;
            this.label3.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label3.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label3.Location = new System.Drawing.Point(507, 25);
            this.label3.MouseState = MaterialSkin.MouseState.HOVER;
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Port";
            // 
            // txtIpAddress
            // 
            this.txtIpAddress.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtIpAddress.Location = new System.Drawing.Point(300, 45);
            this.txtIpAddress.Name = "txtIpAddress";
            this.txtIpAddress.Size = new System.Drawing.Size(190, 23);
            this.txtIpAddress.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Depth = 0;
            this.label2.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label2.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label2.Location = new System.Drawing.Point(297, 25);
            this.label2.MouseState = MaterialSkin.MouseState.HOVER;
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "IP Adresi";
            // 
            // txtLineName
            // 
            this.txtLineName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtLineName.Location = new System.Drawing.Point(20, 45);
            this.txtLineName.Name = "txtLineName";
            this.txtLineName.Size = new System.Drawing.Size(260, 23);
            this.txtLineName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Depth = 0;
            this.label1.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label1.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label1.Location = new System.Drawing.Point(17, 25);
            this.label1.MouseState = MaterialSkin.MouseState.HOVER;
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Hat İsmi";
            // 
            // flowLayoutPanelSensors
            // 
            this.flowLayoutPanelSensors.AutoScroll = true;
            this.flowLayoutPanelSensors.BackColor = System.Drawing.Color.Transparent;
            this.flowLayoutPanelSensors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelSensors.Location = new System.Drawing.Point(5, 105);
            this.flowLayoutPanelSensors.Name = "flowLayoutPanelSensors";
            this.flowLayoutPanelSensors.Size = new System.Drawing.Size(736, 490);
            this.flowLayoutPanelSensors.TabIndex = 1;
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = System.Drawing.Color.Transparent;
            this.panelBottom.Controls.Add(this.btnSave);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(5, 595);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(736, 50);
            this.panelBottom.TabIndex = 2;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.AutoSize = false;
            this.btnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSave.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSave.Depth = 0;
            this.btnSave.HighEmphasis = true; // Dolgulu baskın stil
            this.btnSave.Icon = null;
            this.btnSave.Location = new System.Drawing.Point(586, 7); // Dikey hizalama dengelendi
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSave.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSave.Name = "btnSave";
            this.btnSave.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSave.Size = new System.Drawing.Size(140, 36); // Yükseklik standardı 36px yapıldı
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "KAYDET";
            this.btnSave.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSave.UseAccentColor = true; // Dikkat çekici aksan rengiaktif
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // UtilitySettings_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent; // Ebeveyn forma tam entegre şeffaflık
            this.Controls.Add(this.splitContainer1);
            this.Name = "UtilitySettings_Control";
            this.Size = new System.Drawing.Size(1000, 650);
            this.Load += new System.EventHandler(this.UtilitySettings_Control_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelListHeader.ResumeLayout(false);
            this.panelListHeader.PerformLayout();
            this.grpGeneral.ResumeLayout(false);
            this.grpGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSlaveId)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox lstLines;
        private System.Windows.Forms.Panel panelListHeader;
        private MaterialSkin.Controls.MaterialButton btnDeleteLine; // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnAddLine;    // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel lblListHeader;    // Tür güncellendi
        private System.Windows.Forms.GroupBox grpGeneral;
        private System.Windows.Forms.NumericUpDown txtSlaveId;
        private MaterialSkin.Controls.MaterialLabel label4;         // Tür güncellendi
        private System.Windows.Forms.NumericUpDown txtPort;
        private MaterialSkin.Controls.MaterialLabel label3;         // Tür güncellendi
        private System.Windows.Forms.TextBox txtIpAddress;
        private MaterialSkin.Controls.MaterialLabel label2;         // Tür güncellendi
        private System.Windows.Forms.TextBox txtLineName;
        private MaterialSkin.Controls.MaterialLabel label1;         // Tür güncellendi
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelSensors;
        private System.Windows.Forms.Panel panelBottom;
        private MaterialSkin.Controls.MaterialButton btnSave;       // Tür güncellendi
    }
}