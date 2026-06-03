// UI/Views/MachineSettings_Control.Designer.cs
namespace Telemetry.UI.Views
{
    partial class MachineSettings_Control
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
            this.dgvMachines = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textdisplayname = new System.Windows.Forms.TextBox();
            this.label10 = new MaterialSkin.Controls.MaterialLabel();      // MaterialLabel yapıldı
            this.displaybox = new System.Windows.Forms.TextBox();
            this.displayno = new MaterialSkin.Controls.MaterialLabel();       // MaterialLabel yapıldı
            this.txtMachineSubType = new System.Windows.Forms.TextBox();
            this.label9 = new MaterialSkin.Controls.MaterialLabel();          // MaterialLabel yapıldı
            this.txtFtpPassword = new System.Windows.Forms.TextBox();
            this.label8 = new MaterialSkin.Controls.MaterialLabel();          // MaterialLabel yapıldı
            this.txtFtpUsername = new System.Windows.Forms.TextBox();
            this.label7 = new MaterialSkin.Controls.MaterialLabel();          // MaterialLabel yapıldı
            this.cmbMachineType = new System.Windows.Forms.ComboBox();
            this.label6 = new MaterialSkin.Controls.MaterialLabel();          // MaterialLabel yapıldı
            this.btnDelete = new MaterialSkin.Controls.MaterialButton();      // MaterialButton yapıldı
            this.btnSave = new MaterialSkin.Controls.MaterialButton();        // MaterialButton yapıldı
            this.btnNew = new MaterialSkin.Controls.MaterialButton();         // MaterialButton yapıldı
            this.chkIsEnabled = new MaterialSkin.Controls.MaterialCheckbox(); // MaterialCheckbox yapıldı
            this.txtVncAddress = new System.Windows.Forms.TextBox();
            this.label5 = new MaterialSkin.Controls.MaterialLabel();          // MaterialLabel yapıldı
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label4 = new MaterialSkin.Controls.MaterialLabel();          // MaterialLabel yapıldı
            this.txtIpAddress = new System.Windows.Forms.TextBox();
            this.label3 = new MaterialSkin.Controls.MaterialLabel();          // MaterialLabel yapıldı
            this.txtMachineName = new System.Windows.Forms.TextBox();
            this.label2 = new MaterialSkin.Controls.MaterialLabel();          // MaterialLabel yapıldı
            this.txtMachineId = new System.Windows.Forms.TextBox();
            this.label1 = new MaterialSkin.Controls.MaterialLabel();          // MaterialLabel yapıldı
            ((System.ComponentModel.ISupportInitialize)(this.dgvMachines)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvMachines
            // 
            this.dgvMachines.AllowUserToAddRows = false;
            this.dgvMachines.AllowUserToDeleteRows = false;
            this.dgvMachines.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMachines.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMachines.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMachines.Location = new System.Drawing.Point(0, 0);
            this.dgvMachines.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvMachines.MultiSelect = false;
            this.dgvMachines.Name = "dgvMachines";
            this.dgvMachines.ReadOnly = true;
            this.dgvMachines.RowHeadersWidth = 51;
            this.dgvMachines.RowTemplate.Height = 26;
            this.dgvMachines.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMachines.Size = new System.Drawing.Size(700, 412);
            this.dgvMachines.TabIndex = 0;
            this.dgvMachines.SelectionChanged += new System.EventHandler(this.dgvMachines_SelectionChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent; // Dark Mode zemin kırılmasını önler
            this.groupBox1.Controls.Add(this.textdisplayname);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.displaybox);
            this.groupBox1.Controls.Add(this.displayno);
            this.groupBox1.Controls.Add(this.txtMachineSubType);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.txtFtpPassword);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.txtFtpUsername);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.cmbMachineType);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.btnDelete);
            this.groupBox1.Controls.Add(this.btnSave);
            this.groupBox1.Controls.Add(this.btnNew);
            this.groupBox1.Controls.Add(this.chkIsEnabled);
            this.groupBox1.Controls.Add(this.txtVncAddress);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtPort);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtIpAddress);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtMachineName);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtMachineId);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox1.Location = new System.Drawing.Point(0, 224);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(700, 188);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Makine Bilgileri";
            // 
            // textdisplayname
            // 
            this.textdisplayname.Location = new System.Drawing.Point(478, 112);
            this.textdisplayname.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textdisplayname.Name = "textdisplayname";
            this.textdisplayname.Size = new System.Drawing.Size(205, 23);
            this.textdisplayname.TabIndex = 25;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Depth = 0;
            this.label10.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label10.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label10.Location = new System.Drawing.Point(362, 115);
            this.label10.MouseState = MaterialSkin.MouseState.HOVER;
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(107, 17);
            this.label10.TabIndex = 24;
            this.label10.Text = "Display Hall Name:";
            // 
            // displaybox
            // 
            this.displaybox.Location = new System.Drawing.Point(114, 112);
            this.displaybox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.displaybox.Name = "displaybox";
            this.displaybox.Size = new System.Drawing.Size(219, 23);
            this.displaybox.TabIndex = 23;
            // 
            // displayno
            // 
            this.displayno.AutoSize = true;
            this.displayno.Depth = 0;
            this.displayno.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.displayno.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.displayno.Location = new System.Drawing.Point(18, 115);
            this.displayno.MouseState = MaterialSkin.MouseState.HOVER;
            this.displayno.Name = "displayno";
            this.displayno.Size = new System.Drawing.Size(67, 17);
            this.displayno.TabIndex = 22;
            this.displayno.Text = "Display No:";
            // 
            // txtMachineSubType
            // 
            this.txtMachineSubType.Location = new System.Drawing.Point(478, 52);
            this.txtMachineSubType.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtMachineSubType.Name = "txtMachineSubType";
            this.txtMachineSubType.Size = new System.Drawing.Size(205, 23);
            this.txtMachineSubType.TabIndex = 21;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Depth = 0;
            this.label9.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label9.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label9.Location = new System.Drawing.Point(362, 55);
            this.label9.MouseState = MaterialSkin.MouseState.HOVER;
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(81, 17);
            this.label9.TabIndex = 20;
            this.label9.Text = "Makine Tipi 2:";
            // 
            // txtFtpPassword
            // 
            this.txtFtpPassword.Location = new System.Drawing.Point(478, 82);
            this.txtFtpPassword.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtFtpPassword.Name = "txtFtpPassword";
            this.txtFtpPassword.PasswordChar = '*';
            this.txtFtpPassword.Size = new System.Drawing.Size(205, 23);
            this.txtFtpPassword.TabIndex = 19;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Depth = 0;
            this.label8.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label8.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label8.Location = new System.Drawing.Point(362, 85);
            this.label8.MouseState = MaterialSkin.MouseState.HOVER;
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(56, 17);
            this.label8.TabIndex = 18;
            this.label8.Text = "FTP Şifre:";
            // 
            // txtFtpUsername
            // 
            this.txtFtpUsername.Location = new System.Drawing.Point(478, 22);
            this.txtFtpUsername.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtFtpUsername.Name = "txtFtpUsername";
            this.txtFtpUsername.Size = new System.Drawing.Size(205, 23);
            this.txtFtpUsername.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Depth = 0;
            this.label7.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label7.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label7.Location = new System.Drawing.Point(362, 25);
            this.label7.MouseState = MaterialSkin.MouseState.HOVER;
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(78, 17);
            this.label7.TabIndex = 16;
            this.label7.Text = "FTP Kullanıcı:";
            // 
            // cmbMachineType
            // 
            this.cmbMachineType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMachineType.FormattingEnabled = true;
            this.cmbMachineType.Location = new System.Drawing.Point(114, 82);
            this.cmbMachineType.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbMachineType.Name = "cmbMachineType";
            this.cmbMachineType.Size = new System.Drawing.Size(219, 23);
            this.cmbMachineType.TabIndex = 15;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Depth = 0;
            this.label6.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label6.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label6.Location = new System.Drawing.Point(18, 85);
            this.label6.MouseState = MaterialSkin.MouseState.HOVER;
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 17);
            this.label6.TabIndex = 14;
            this.label6.Text = "Makine Tipi:";
            // 
            // btnDelete
            // 
            this.btnDelete.AutoSize = false;
            this.btnDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnDelete.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnDelete.Depth = 0;
            this.btnDelete.HighEmphasis = false;
            this.btnDelete.Icon = null;
            this.btnDelete.Location = new System.Drawing.Point(600, 142); // 36px dikey eksen eşitlemesi sağlandı
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnDelete.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnDelete.Size = new System.Drawing.Size(85, 36);
            this.btnDelete.TabIndex = 13;
            this.btnDelete.Text = "Sil";
            this.btnDelete.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined; // Çizgili flat stil
            this.btnDelete.UseAccentColor = false;
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnSave
            // 
            this.btnSave.AutoSize = false;
            this.btnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSave.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSave.Depth = 0;
            this.btnSave.HighEmphasis = true; // Ana operasyon (Kaydet) baskınlaştırıldı
            this.btnSave.Icon = null;
            this.btnSave.Location = new System.Drawing.Point(505, 142);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSave.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSave.Name = "btnSave";
            this.btnSave.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSave.Size = new System.Drawing.Size(85, 36);
            this.btnSave.TabIndex = 12;
            this.btnSave.Text = "Kaydet";
            this.btnSave.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained; // Dolgulu material stil
            this.btnSave.UseAccentColor = true; // Dikkat çeken vurgu rengi
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnNew
            // 
            this.btnNew.AutoSize = false;
            this.btnNew.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnNew.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnNew.Depth = 0;
            this.btnNew.HighEmphasis = false;
            this.btnNew.Icon = null;
            this.btnNew.Location = new System.Drawing.Point(410, 142);
            this.btnNew.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnNew.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnNew.Name = "btnNew";
            this.btnNew.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnNew.Size = new System.Drawing.Size(85, 36);
            this.btnNew.TabIndex = 11;
            this.btnNew.Text = "Yeni";
            this.btnNew.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined; // Çizgili flat stil
            this.btnNew.UseAccentColor = false;
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // chkIsEnabled
            // 
            this.chkIsEnabled.AutoSize = true;
            this.chkIsEnabled.Depth = 0;
            this.chkIsEnabled.Location = new System.Drawing.Point(10, 142);
            this.chkIsEnabled.Margin = new System.Windows.Forms.Padding(0);
            this.chkIsEnabled.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkIsEnabled.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkIsEnabled.Name = "chkIsEnabled";
            this.chkIsEnabled.Ripple = true;
            this.chkIsEnabled.Size = new System.Drawing.Size(161, 37);
            this.chkIsEnabled.TabIndex = 10;
            this.chkIsEnabled.Text = "İzleme Aktif Mi?";
            this.chkIsEnabled.UseVisualStyleBackColor = true;
            // 
            // txtVncAddress
            // 
            this.txtVncAddress.Location = new System.Drawing.Point(114, 52);
            this.txtVncAddress.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtVncAddress.Name = "txtVncAddress";
            this.txtVncAddress.Size = new System.Drawing.Size(219, 23);
            this.txtVncAddress.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Depth = 0;
            this.label5.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label5.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label5.Location = new System.Drawing.Point(18, 55);
            this.label5.MouseState = MaterialSkin.MouseState.HOVER;
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 17);
            this.label5.TabIndex = 8;
            this.label5.Text = "VNC Adresi:";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(268, 22);
            this.txtPort.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(65, 23);
            this.txtPort.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Depth = 0;
            this.label4.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label4.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label4.Location = new System.Drawing.Point(232, 25);
            this.label4.MouseState = MaterialSkin.MouseState.HOVER;
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 17);
            this.label4.TabIndex = 6;
            this.label4.Text = "Port:";
            // 
            // txtIpAddress
            // 
            this.txtIpAddress.Location = new System.Drawing.Point(114, 22);
            this.txtIpAddress.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtIpAddress.Name = "txtIpAddress";
            this.txtIpAddress.Size = new System.Drawing.Size(110, 23);
            this.txtIpAddress.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Depth = 0;
            this.label3.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label3.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label3.Location = new System.Drawing.Point(18, 25);
            this.label3.MouseState = MaterialSkin.MouseState.HOVER;
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "IP Adresi:";
            // 
            // txtMachineName
            // 
            this.txtMachineName.Location = new System.Drawing.Point(478, -38); // Mevcut yerleşim dengesi korundu
            this.txtMachineName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtMachineName.Name = "txtMachineName";
            this.txtMachineName.Size = new System.Drawing.Size(205, 23);
            this.txtMachineName.TabIndex = 3;
            this.txtMachineName.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Depth = 0;
            this.label2.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label2.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label2.Location = new System.Drawing.Point(362, -35);
            this.label2.MouseState = MaterialSkin.MouseState.HOVER;
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Makine Adı:";
            this.label2.Visible = false;
            // 
            // txtMachineId
            // 
            this.txtMachineId.Location = new System.Drawing.Point(114, -38);
            this.txtMachineId.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtMachineId.Name = "txtMachineId";
            this.txtMachineId.Size = new System.Drawing.Size(219, 23);
            this.txtMachineId.TabIndex = 1;
            this.txtMachineId.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Depth = 0;
            this.label1.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label1.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label1.Location = new System.Drawing.Point(18, -35);
            this.label1.MouseState = MaterialSkin.MouseState.HOVER;
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Makine ID:";
            this.label1.Visible = false;
            // 
            // MachineSettings_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent; // Panel şeffaflığı aktif edildi
            this.Controls.Add(this.dgvMachines); // Kod arkasındaki yerleşim motorunun çalışması için dgv önce eklenir
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MachineSettings_Control";
            this.Size = new System.Drawing.Size(700, 412);
            this.Load += new System.EventHandler(this.MachineSettings_Control_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMachines)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvMachines;
        private System.Windows.Forms.GroupBox groupBox1;
        private MaterialSkin.Controls.MaterialButton btnDelete;      // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnSave;        // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnNew;         // Tür güncellendi
        private MaterialSkin.Controls.MaterialCheckbox chkIsEnabled; // Tür güncellendi
        private System.Windows.Forms.TextBox txtVncAddress;
        private MaterialSkin.Controls.MaterialLabel label5;          // Tür güncellendi
        private System.Windows.Forms.TextBox txtPort;
        private MaterialSkin.Controls.MaterialLabel label4;          // Tür güncellendi
        private System.Windows.Forms.TextBox txtIpAddress;
        private MaterialSkin.Controls.MaterialLabel label3;          // Tür güncellendi
        private System.Windows.Forms.TextBox txtMachineName;
        private MaterialSkin.Controls.MaterialLabel label2;          // Tür güncellendi
        private System.Windows.Forms.TextBox txtMachineId;
        private MaterialSkin.Controls.MaterialLabel label1;          // Tür güncellendi
        private System.Windows.Forms.ComboBox cmbMachineType;
        private MaterialSkin.Controls.MaterialLabel label6;          // Tür güncellendi
        private System.Windows.Forms.TextBox txtFtpPassword;
        private MaterialSkin.Controls.MaterialLabel label8;          // Tür güncellendi
        private System.Windows.Forms.TextBox txtFtpUsername;
        private MaterialSkin.Controls.MaterialLabel label7;          // Tür güncellendi
        private System.Windows.Forms.TextBox txtMachineSubType;
        private MaterialSkin.Controls.MaterialLabel label9;          // Tür güncellendi
        private System.Windows.Forms.TextBox displaybox;
        private MaterialSkin.Controls.MaterialLabel displayno;        // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel label10;         // Tür güncellendi
        private System.Windows.Forms.TextBox textdisplayname;
    }
}