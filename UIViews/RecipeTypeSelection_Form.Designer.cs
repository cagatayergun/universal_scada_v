// UIViews/RecipeTypeSelection_Form.Designer.cs
namespace Telemetry.UIViews
{
    partial class RecipeTypeSelection_Form
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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.makinetip = new MaterialSkin.Controls.MaterialLabel();      // MaterialLabel yapıldı
            this.cmbRecipeType = new System.Windows.Forms.ComboBox();
            this.btnOk = new MaterialSkin.Controls.MaterialButton();        // MaterialButton yapıldı
            this.SuspendLayout();
            // 
            // makinetip
            // 
            this.makinetip.Depth = 0;
            this.makinetip.Font = new System.Drawing.Font("Roboto", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.makinetip.FontType = MaterialSkin.MaterialSkinManager.fontType.H5; // Modern başlık tipografisi
            this.makinetip.Location = new System.Drawing.Point(50, 25); // Tam yatay merkezleme koordinatı
            this.makinetip.MouseState = MaterialSkin.MouseState.HOVER;
            this.makinetip.Name = "makinetip";
            this.makinetip.Size = new System.Drawing.Size(362, 32);
            this.makinetip.TabIndex = 0;
            this.makinetip.Text = "Select Machine Type";
            this.makinetip.TextAlign = System.Drawing.ContentAlignment.MiddleCenter; // Yazı merkezlendi
            // 
            // cmbRecipeType
            // 
            this.cmbRecipeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRecipeType.Font = new System.Drawing.Font("Segoe UI", 11F); // Material stiline uygun yumuşak font boyutu
            this.cmbRecipeType.FormattingEnabled = true;
            this.cmbRecipeType.Location = new System.Drawing.Point(106, 80);
            this.cmbRecipeType.Name = "cmbRecipeType";
            this.cmbRecipeType.Size = new System.Drawing.Size(250, 28);
            this.cmbRecipeType.TabIndex = 1;
            // 
            // btnOk
            // 
            this.btnOk.AutoSize = false;
            this.btnOk.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnOk.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnOk.Depth = 0;
            this.btnOk.HighEmphasis = true; // Baskın aksiyon vurgusu aktif
            this.btnOk.Icon = null;
            this.btnOk.Location = new System.Drawing.Point(150, 135); // Yeni dikey eksen yerleşimi
            this.btnOk.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnOk.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnOk.Name = "btnOk";
            this.btnOk.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnOk.Size = new System.Drawing.Size(162, 36); // Yükseklik flat buton standardı olan 36px'e çekildi
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "OK";
            this.btnOk.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained; // Dolgulu baskın stil
            this.btnOk.UseAccentColor = true; // Dikkat çekici vurgu rengi
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // RecipeTypeSelection_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(462, 200); // 36px buton düzenine göre form yüksekliği dengelendi
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.cmbRecipeType);
            this.Controls.Add(this.makinetip);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RecipeTypeSelection_Form";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "RecipeTypeSelection_Form";
            this.ResumeLayout(false);

        }

        #endregion

        private MaterialSkin.Controls.MaterialLabel makinetip;       // Tür güncellendi
        private System.Windows.Forms.ComboBox cmbRecipeType;
        private MaterialSkin.Controls.MaterialButton btnOk;           // Tür güncellendi
    }
}