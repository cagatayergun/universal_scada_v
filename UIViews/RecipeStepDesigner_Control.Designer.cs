// UI/Views/RecipeStepDesigner_Control.Designer.cs
namespace Telemetry.UI.Views
{
    partial class RecipeStepDesigner_Control
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
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlDesignSurface = new System.Windows.Forms.Panel();
            this.pnlToolbox = new System.Windows.Forms.Panel();
            this.btnTextbox = new MaterialSkin.Controls.MaterialButton();     // MaterialButton yapıldı
            this.btnCheckbox = new MaterialSkin.Controls.MaterialButton();    // MaterialButton yapıldı
            this.btnNumeric = new MaterialSkin.Controls.MaterialButton();     // MaterialButton yapıldı
            this.btnLabel = new MaterialSkin.Controls.MaterialButton();       // MaterialButton yapıldı
            this.pnlProperties = new System.Windows.Forms.Panel();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnNewLayout = new MaterialSkin.Controls.MaterialButton();   // MaterialButton yapıldı
            this.btnSaveLayout = new MaterialSkin.Controls.MaterialButton();  // MaterialButton yapıldı
            this.cmbStepType = new System.Windows.Forms.ComboBox();
            this.lblStepType = new MaterialSkin.Controls.MaterialLabel();     // MaterialLabel yapıldı
            this.cmbMachineSubType = new System.Windows.Forms.ComboBox();
            this.lblMachineSubType = new MaterialSkin.Controls.MaterialLabel(); // MaterialLabel yapıldı
            this.pnlMain.SuspendLayout();
            this.pnlToolbox.SuspendLayout();
            this.pnlProperties.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.Transparent;
            this.pnlMain.Controls.Add(this.pnlDesignSurface);
            this.pnlMain.Controls.Add(this.pnlToolbox);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 60); // Üst panel genişlemesine göre senkronize edildi
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(550, 390);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlDesignSurface
            // 
            this.pnlDesignSurface.AllowDrop = true;
            this.pnlDesignSurface.BackColor = System.Drawing.Color.Transparent; // Çizim elemanlarının dark modu izlemesi için transparan yapıldı
            this.pnlDesignSurface.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDesignSurface.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDesignSurface.Location = new System.Drawing.Point(120, 0);
            this.pnlDesignSurface.Name = "pnlDesignSurface";
            this.pnlDesignSurface.Size = new System.Drawing.Size(430, 390);
            this.pnlDesignSurface.TabIndex = 1;
            // 
            // pnlToolbox
            // 
            this.pnlToolbox.BackColor = System.Drawing.Color.Transparent;
            this.pnlToolbox.Controls.Add(this.btnTextbox);
            this.pnlToolbox.Controls.Add(this.btnCheckbox);
            this.pnlToolbox.Controls.Add(this.btnNumeric);
            this.pnlToolbox.Controls.Add(this.btnLabel);
            this.pnlToolbox.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlToolbox.Location = new System.Drawing.Point(0, 0);
            this.pnlToolbox.Name = "pnlToolbox";
            this.pnlToolbox.Size = new System.Drawing.Size(120, 390);
            this.pnlToolbox.TabIndex = 0;
            // 
            // btnLabel
            // 
            this.btnLabel.AutoSize = false;
            this.btnLabel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnLabel.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnLabel.Depth = 0;
            this.btnLabel.HighEmphasis = false;
            this.btnLabel.Icon = null;
            this.btnLabel.Location = new System.Drawing.Point(15, 15);
            this.btnLabel.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnLabel.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnLabel.Name = "btnLabel";
            this.btnLabel.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnLabel.Size = new System.Drawing.Size(90, 36); // Yükseklik 36px standardına çekildi
            this.btnLabel.TabIndex = 0;
            this.btnLabel.Text = "Etiket";
            this.btnLabel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnLabel.UseAccentColor = false;
            this.btnLabel.UseVisualStyleBackColor = true;
            this.btnLabel.Tag = typeof(System.Windows.Forms.Label);
            // 
            // btnNumeric
            // 
            this.btnNumeric.AutoSize = false;
            this.btnNumeric.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnNumeric.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnNumeric.Depth = 0;
            this.btnNumeric.HighEmphasis = false;
            this.btnNumeric.Icon = null;
            this.btnNumeric.Location = new System.Drawing.Point(15, 60); // Dikey konum buton yüksekliklerine göre simetrikleştirildi
            this.btnNumeric.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnNumeric.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnNumeric.Name = "btnNumeric";
            this.btnNumeric.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnNumeric.Size = new System.Drawing.Size(90, 36);
            this.btnNumeric.TabIndex = 1;
            this.btnNumeric.Text = "Sayı Kutusu";
            this.btnNumeric.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnNumeric.UseAccentColor = false;
            this.btnNumeric.UseVisualStyleBackColor = true;
            this.btnNumeric.Tag = typeof(System.Windows.Forms.NumericUpDown);
            // 
            // btnCheckbox
            // 
            this.btnCheckbox.AutoSize = false;
            this.btnCheckbox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCheckbox.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnCheckbox.Depth = 0;
            this.btnCheckbox.HighEmphasis = false;
            this.btnCheckbox.Icon = null;
            this.btnCheckbox.Location = new System.Drawing.Point(15, 105);
            this.btnCheckbox.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnCheckbox.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnCheckbox.Name = "btnCheckbox";
            this.btnCheckbox.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnCheckbox.Size = new System.Drawing.Size(90, 36);
            this.btnCheckbox.TabIndex = 2;
            this.btnCheckbox.Text = "Onay Kutusu";
            this.btnCheckbox.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnCheckbox.UseAccentColor = false;
            this.btnCheckbox.UseVisualStyleBackColor = true;
            this.btnCheckbox.Tag = typeof(System.Windows.Forms.CheckBox);
            // 
            // btnTextbox
            // 
            this.btnTextbox.AutoSize = false;
            this.btnTextbox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnTextbox.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnTextbox.Depth = 0;
            this.btnTextbox.HighEmphasis = false;
            this.btnTextbox.Icon = null;
            this.btnTextbox.Location = new System.Drawing.Point(15, 150);
            this.btnTextbox.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnTextbox.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnTextbox.Name = "btnTextbox";
            this.btnTextbox.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnTextbox.Size = new System.Drawing.Size(90, 36);
            this.btnTextbox.TabIndex = 3;
            this.btnTextbox.Text = "Metin Kutusu";
            this.btnTextbox.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnTextbox.UseAccentColor = false;
            this.btnTextbox.UseVisualStyleBackColor = true;
            this.btnTextbox.Tag = typeof(System.Windows.Forms.TextBox);
            // 
            // pnlProperties
            // 
            this.pnlProperties.BackColor = System.Drawing.Color.Transparent;
            this.pnlProperties.Controls.Add(this.propertyGrid);
            this.pnlProperties.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlProperties.Location = new System.Drawing.Point(550, 60);
            this.pnlProperties.Name = "pnlProperties";
            this.pnlProperties.Size = new System.Drawing.Size(250, 390);
            this.pnlProperties.TabIndex = 1;
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(250, 390);
            this.propertyGrid.TabIndex = 0;
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.Transparent;
            this.pnlTop.Controls.Add(this.btnNewLayout);
            this.pnlTop.Controls.Add(this.btnSaveLayout);
            this.pnlTop.Controls.Add(this.cmbStepType);
            this.pnlTop.Controls.Add(this.lblStepType);
            this.pnlTop.Controls.Add(this.cmbMachineSubType);
            this.pnlTop.Controls.Add(this.lblMachineSubType);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(800, 60); // 36px material buton taşmasını önlemek için 60px'e esnetildi
            this.pnlTop.TabIndex = 2;
            // 
            // cmbMachineSubType
            // 
            this.cmbMachineSubType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMachineSubType.FormattingEnabled = true;
            this.cmbMachineSubType.Location = new System.Drawing.Point(63, 16); // Dikey satır hizalaması eşitlendi
            this.cmbMachineSubType.Name = "cmbMachineSubType";
            this.cmbMachineSubType.Size = new System.Drawing.Size(150, 28);
            this.cmbMachineSubType.TabIndex = 1;
            // 
            // lblMachineSubType
            // 
            this.lblMachineSubType.AutoSize = true;
            this.lblMachineSubType.Depth = 0;
            this.lblMachineSubType.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblMachineSubType.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.lblMachineSubType.Location = new System.Drawing.Point(12, 21);
            this.lblMachineSubType.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblMachineSubType.Name = "lblMachineSubType";
            this.lblMachineSubType.Size = new System.Drawing.Size(32, 17);
            this.lblMachineSubType.TabIndex = 0;
            this.lblMachineSubType.Text = "Mak.:";
            // 
            // cmbStepType
            // 
            this.cmbStepType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStepType.FormattingEnabled = true;
            this.cmbStepType.Location = new System.Drawing.Point(300, 16);
            this.cmbStepType.Name = "cmbStepType";
            this.cmbStepType.Size = new System.Drawing.Size(150, 28);
            this.cmbStepType.TabIndex = 3;
            // 
            // lblStepType
            // 
            this.lblStepType.AutoSize = true;
            this.lblStepType.Depth = 0;
            this.lblStepType.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblStepType.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.lblStepType.Location = new System.Drawing.Point(225, 21);
            this.lblStepType.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblStepType.Name = "lblStepType";
            this.lblStepType.Size = new System.Drawing.Size(61, 17);
            this.lblStepType.TabIndex = 2;
            this.lblStepType.Text = "Adım Tipi:";
            // 
            // btnNewLayout
            // 
            this.btnNewLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNewLayout.AutoSize = false;
            this.btnNewLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnNewLayout.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnNewLayout.Depth = 0;
            this.btnNewLayout.HighEmphasis = false;
            this.btnNewLayout.Icon = null;
            this.btnNewLayout.Location = new System.Drawing.Point(475, 12);
            this.btnNewLayout.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnNewLayout.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnNewLayout.Name = "btnNewLayout";
            this.btnNewLayout.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnNewLayout.Size = new System.Drawing.Size(150, 36);
            this.btnNewLayout.TabIndex = 5;
            this.btnNewLayout.Text = "Yeni Tasarım";
            this.btnNewLayout.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined; // Çizgili flat tasarım
            this.btnNewLayout.UseAccentColor = false;
            this.btnNewLayout.UseVisualStyleBackColor = true;
            // 
            // btnSaveLayout
            // 
            this.btnSaveLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveLayout.AutoSize = false;
            this.btnSaveLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaveLayout.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSaveLayout.Depth = 0;
            this.btnSaveLayout.HighEmphasis = true;
            this.btnSaveLayout.Icon = null;
            this.btnSaveLayout.Location = new System.Drawing.Point(635, 12);
            this.btnSaveLayout.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSaveLayout.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSaveLayout.Name = "btnSaveLayout";
            this.btnSaveLayout.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSaveLayout.Size = new System.Drawing.Size(150, 36);
            this.btnSaveLayout.TabIndex = 4;
            this.btnSaveLayout.Text = "Tasarımı Kaydet";
            this.btnSaveLayout.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained; // Dolgulu baskın stil
            this.btnSaveLayout.UseAccentColor = true; // Dikkat çekici aksan rengiaktif
            this.btnSaveLayout.UseVisualStyleBackColor = true;
            // 
            // RecipeStepDesigner_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlProperties);
            this.Controls.Add(this.pnlTop);
            this.Name = "RecipeStepDesigner_Control";
            this.Size = new System.Drawing.Size(800, 450);
            this.pnlMain.ResumeLayout(false);
            this.pnlToolbox.ResumeLayout(false);
            this.pnlProperties.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlDesignSurface;
        private System.Windows.Forms.Panel pnlToolbox;
        private MaterialSkin.Controls.MaterialButton btnCheckbox; // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnNumeric;  // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnLabel;    // Tür güncellendi
        private System.Windows.Forms.Panel pnlProperties;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.Panel pnlTop;
        private MaterialSkin.Controls.MaterialButton btnSaveLayout; // Tür güncellendi
        private System.Windows.Forms.ComboBox cmbStepType;
        private MaterialSkin.Controls.MaterialLabel lblStepType;     // Tür güncellendi
        private System.Windows.Forms.ComboBox cmbMachineSubType;
        private MaterialSkin.Controls.MaterialLabel lblMachineSubType; // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnNewLayout;  // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnTextbox;    // Tür güncellendi
    }
}