// UI/Views/PlcOperatorSettings_Control.Designer.cs
namespace Telemetry.UI.Views
{
    partial class PlcOperatorSettings_Control
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
            this.label1 = new MaterialSkin.Controls.MaterialLabel();      // MaterialLabel olarak güncellendi
            this.cmbMachines = new System.Windows.Forms.ComboBox();
            this.label2 = new MaterialSkin.Controls.MaterialLabel();      // MaterialLabel olarak güncellendi
            this.cmbSlot = new System.Windows.Forms.ComboBox();
            this.btnSend = new MaterialSkin.Controls.MaterialButton();    // MaterialButton olarak güncellendi
            this.btnRead = new MaterialSkin.Controls.MaterialButton();    // MaterialButton olarak güncellendi
            this.dgvOperators = new System.Windows.Forms.DataGridView();
            this.btnDelete = new MaterialSkin.Controls.MaterialButton();  // MaterialButton olarak güncellendi
            this.ekle = new MaterialSkin.Controls.MaterialButton();       // MaterialButton olarak güncellendi
            ((System.ComponentModel.ISupportInitialize)(this.dgvOperators)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Depth = 0;
            this.label1.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label1.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label1.Location = new System.Drawing.Point(18, 22); // Satır hizalaması için dikey eksen optimize edildi
            this.label1.MouseState = MaterialSkin.MouseState.HOVER;
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Target Machine:";
            // 
            // cmbMachines
            // 
            this.cmbMachines.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMachines.FormattingEnabled = true;
            this.cmbMachines.Location = new System.Drawing.Point(120, 18);
            this.cmbMachines.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbMachines.Name = "cmbMachines";
            this.cmbMachines.Size = new System.Drawing.Size(215, 23);
            this.cmbMachines.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Depth = 0;
            this.label2.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label2.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label2.Location = new System.Drawing.Point(355, 22);
            this.label2.MouseState = MaterialSkin.MouseState.HOVER;
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "User Order:";
            // 
            // cmbSlot
            // 
            this.cmbSlot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSlot.FormattingEnabled = true;
            this.cmbSlot.Location = new System.Drawing.Point(430, 18);
            this.cmbSlot.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbSlot.Name = "cmbSlot";
            this.cmbSlot.Size = new System.Drawing.Size(55, 23);
            this.cmbSlot.TabIndex = 3;
            // 
            // btnSend
            // 
            this.btnSend.AutoSize = false;
            this.btnSend.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSend.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSend.Depth = 0;
            this.btnSend.HighEmphasis = true; // Ana operasyon (PLC'ye Gönder) dolgulu stil ile belirginleştirildi
            this.btnSend.Icon = null;
            this.btnSend.Location = new System.Drawing.Point(505, 12); // Butonların üst üste binmesi yatay hizalamayla engellendi
            this.btnSend.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSend.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSend.Name = "btnSend";
            this.btnSend.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSend.Size = new System.Drawing.Size(230, 36); // Modern material yüksekliği olan 36px mühürlendi
            this.btnSend.TabIndex = 4;
            this.btnSend.Text = "Send Template to PLC ->";
            this.btnSend.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSend.UseAccentColor = true;
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // btnRead
            // 
            this.btnRead.AutoSize = false;
            this.btnRead.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRead.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnRead.Depth = 0;
            this.btnRead.HighEmphasis = false;
            this.btnRead.Icon = null;
            this.btnRead.Location = new System.Drawing.Point(745, 12);
            this.btnRead.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnRead.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRead.Name = "btnRead";
            this.btnRead.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnRead.Size = new System.Drawing.Size(230, 36);
            this.btnRead.TabIndex = 5;
            this.btnRead.Text = "<- Read Operator From PLC";
            this.btnRead.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined; // İkincil aksiyon çizgili flat stil
            this.btnRead.UseAccentColor = false;
            this.btnRead.UseVisualStyleBackColor = true;
            this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
            // 
            // dgvOperators
            // 
            this.dgvOperators.AllowUserToAddRows = false;
            this.dgvOperators.AllowUserToDeleteRows = false;
            this.dgvOperators.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvOperators.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOperators.Location = new System.Drawing.Point(18, 65);
            this.dgvOperators.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvOperators.Name = "dgvOperators";
            this.dgvOperators.RowHeadersWidth = 51;
            this.dgvOperators.RowTemplate.Height = 26;
            this.dgvOperators.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvOperators.Size = new System.Drawing.Size(957, 475);
            this.dgvOperators.TabIndex = 6;
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.AutoSize = false;
            this.btnDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnDelete.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnDelete.Depth = 0;
            this.btnDelete.HighEmphasis = false;
            this.btnDelete.Icon = null;
            this.btnDelete.Location = new System.Drawing.Point(775, 555); // 36px buton payı dikey koordinatı dengelendi
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnDelete.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnDelete.Size = new System.Drawing.Size(200, 36);
            this.btnDelete.TabIndex = 7;
            this.btnDelete.Text = "Delete Template";
            this.btnDelete.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnDelete.UseAccentColor = false;
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // ekle
            // 
            this.ekle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ekle.AutoSize = false;
            this.ekle.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ekle.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.ekle.Depth = 0;
            this.ekle.HighEmphasis = true;
            this.ekle.Icon = null;
            this.ekle.Location = new System.Drawing.Point(18, 555);
            this.ekle.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.ekle.MouseState = MaterialSkin.MouseState.HOVER;
            this.ekle.Name = "ekle";
            this.ekle.NoAccentTextColor = System.Drawing.Color.Empty;
            this.ekle.Size = new System.Drawing.Size(150, 36);
            this.ekle.TabIndex = 8;
            this.ekle.Text = "Add new User";
            this.ekle.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.ekle.UseAccentColor = false;
            this.ekle.UseVisualStyleBackColor = true;
            this.ekle.Click += new System.EventHandler(this.ekle_Click);
            // 
            // PlcOperatorSettings_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent; // Panel rengini ana forma (Dark/Light moda) bırakıyoruz
            this.Controls.Add(this.ekle);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.dgvOperators);
            this.Controls.Add(this.btnRead);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.cmbSlot);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbMachines);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "PlcOperatorSettings_Control";
            this.Size = new System.Drawing.Size(996, 610); // Butonların sıkışmaması için dikey boyut revize edildi
            this.Load += new System.EventHandler(this.PlcOperatorSettings_Control_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvOperators)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialSkin.Controls.MaterialLabel label1;
        private System.Windows.Forms.ComboBox cmbMachines;
        private MaterialSkin.Controls.MaterialLabel label2;
        private System.Windows.Forms.ComboBox cmbSlot;
        private MaterialSkin.Controls.MaterialButton btnSend;
        private MaterialSkin.Controls.MaterialButton btnRead;
        private System.Windows.Forms.DataGridView dgvOperators;
        private MaterialSkin.Controls.MaterialButton btnDelete;
        private MaterialSkin.Controls.MaterialButton ekle;
    }
}