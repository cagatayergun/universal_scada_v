// UI/Controls/KurutmaReçete_Control.Designer.cs
namespace Telemetry.UI.Controls
{
    partial class KurutmaReçete_Control
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
            this.materialCard1 = new MaterialSkin.Controls.MaterialCard();
            this.label4 = new MaterialSkin.Controls.MaterialLabel();
            this.label1 = new MaterialSkin.Controls.MaterialLabel();
            this.numSicaklik = new System.Windows.Forms.NumericUpDown();
            this.label2 = new MaterialSkin.Controls.MaterialLabel();
            this.numNem = new System.Windows.Forms.NumericUpDown();
            this.chkNemAktif = new MaterialSkin.Controls.MaterialCheckbox();
            this.label3 = new MaterialSkin.Controls.MaterialLabel();
            this.numZaman = new System.Windows.Forms.NumericUpDown();
            this.chkZamanAktif = new MaterialSkin.Controls.MaterialCheckbox();
            this.label5 = new MaterialSkin.Controls.MaterialLabel();
            this.numCalismaDevri = new System.Windows.Forms.NumericUpDown();
            this.label6 = new MaterialSkin.Controls.MaterialLabel();
            this.numSogutmaZamani = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numSicaklik)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNem)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numZaman)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCalismaDevri)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSogutmaZamani)).BeginInit();
            this.materialCard1.SuspendLayout();
            this.SuspendLayout();
            // 
            // materialCard1
            // 
            this.materialCard1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.materialCard1.Controls.Add(this.chkZamanAktif);
            this.materialCard1.Controls.Add(this.chkNemAktif);
            this.materialCard1.Controls.Add(this.numSogutmaZamani);
            this.materialCard1.Controls.Add(this.label6);
            this.materialCard1.Controls.Add(this.numCalismaDevri);
            this.materialCard1.Controls.Add(this.label5);
            this.materialCard1.Controls.Add(this.label4);
            this.materialCard1.Controls.Add(this.numZaman);
            this.materialCard1.Controls.Add(this.label3);
            this.materialCard1.Controls.Add(this.numNem);
            this.materialCard1.Controls.Add(this.label2);
            this.materialCard1.Controls.Add(this.numSicaklik);
            this.materialCard1.Controls.Add(this.label1);
            this.materialCard1.Depth = 0;
            this.materialCard1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.materialCard1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialCard1.Location = new System.Drawing.Point(0, 0);
            this.materialCard1.Margin = new System.Windows.Forms.Padding(14);
            this.materialCard1.MouseState = MaterialSkin.MouseState.OUT;
            this.materialCard1.Name = "materialCard1";
            this.materialCard1.Padding = new System.Windows.Forms.Padding(14);
            this.materialCard1.Size = new System.Drawing.Size(390, 338);
            this.materialCard1.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Depth = 0;
            this.label4.Font = new System.Drawing.Font("Roboto", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.label4.FontType = MaterialSkin.MaterialSkinManager.fontType.H6;
            this.label4.Location = new System.Drawing.Point(18, 15);
            this.label4.MouseState = MaterialSkin.MouseState.HOVER;
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(193, 24);
            this.label4.TabIndex = 6;
            this.label4.Text = "Drying Parameters";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Depth = 0;
            this.label1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label1.FontType = MaterialSkin.MaterialSkinManager.fontType.Body1;
            this.label1.Location = new System.Drawing.Point(18, 59);
            this.label1.MouseState = MaterialSkin.MouseState.HOVER;
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Temperature (°C):";
            // 
            // numSicaklik
            // 
            this.numSicaklik.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.numSicaklik.Location = new System.Drawing.Point(165, 57);
            this.numSicaklik.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numSicaklik.Name = "numSicaklik";
            this.numSicaklik.Size = new System.Drawing.Size(110, 24);
            this.numSicaklik.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Depth = 0;
            this.label2.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label2.FontType = MaterialSkin.MaterialSkinManager.fontType.Body1;
            this.label2.Location = new System.Drawing.Point(18, 93);
            this.label2.MouseState = MaterialSkin.MouseState.HOVER;
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 19);
            this.label2.TabIndex = 2;
            this.label2.Text = "Humidity (%):";
            // 
            // numNem
            // 
            this.numNem.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.numNem.Location = new System.Drawing.Point(165, 91);
            this.numNem.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numNem.Name = "numNem";
            this.numNem.Size = new System.Drawing.Size(110, 24);
            this.numNem.TabIndex = 3;
            // 
            // chkNemAktif
            // 
            this.chkNemAktif.AutoSize = true;
            this.chkNemAktif.Depth = 0;
            // ÇÖZÜM 1: System.Windows.Forms.Point yerine dosdoğru System.Drawing.Point kullanıldı
            this.chkNemAktif.Location = new System.Drawing.Point(285, 84);
            this.chkNemAktif.Margin = new System.Windows.Forms.Padding(0);
            this.chkNemAktif.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkNemAktif.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkNemAktif.Name = "chkNemAktif";
            this.chkNemAktif.ReadOnly = false;
            this.chkNemAktif.Ripple = true;
            this.chkNemAktif.Size = new System.Drawing.Size(78, 37);
            this.chkNemAktif.TabIndex = 11;
            this.chkNemAktif.Text = "Active";
            // ÇÖZÜM 2: Hatalı olan Hata(etkin) UseAccentColor satırı tamamen temizlendi
            this.chkNemAktif.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Depth = 0;
            this.label3.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label3.FontType = MaterialSkin.MaterialSkinManager.fontType.Body1;
            this.label3.Location = new System.Drawing.Point(18, 127);
            this.label3.MouseState = MaterialSkin.MouseState.HOVER;
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 19);
            this.label3.TabIndex = 4;
            this.label3.Text = "Duration (min):";
            // 
            // numZaman
            // 
            this.numZaman.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.numZaman.Location = new System.Drawing.Point(165, 125);
            this.numZaman.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numZaman.Name = "numZaman";
            this.numZaman.Size = new System.Drawing.Size(110, 24);
            this.numZaman.TabIndex = 5;
            // 
            // chkZamanAktif
            // 
            this.chkZamanAktif.AutoSize = true;
            this.chkZamanAktif.Depth = 0;
            // ÇÖZÜM 3: Koordinat Point yapısı System.Drawing ad alanına çekildi
            this.chkZamanAktif.Location = new System.Drawing.Point(285, 118);
            this.chkZamanAktif.Margin = new System.Windows.Forms.Padding(0);
            this.chkZamanAktif.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkZamanAktif.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkZamanAktif.Name = "chkZamanAktif";
            this.chkZamanAktif.ReadOnly = false;
            this.chkZamanAktif.Ripple = true;
            this.chkZamanAktif.Size = new System.Drawing.Size(78, 37);
            this.chkZamanAktif.TabIndex = 12;
            this.chkZamanAktif.Text = "Active";
            // ÇÖZÜM 4: Hatalı UseAccentColor satırı kaldırıldı
            this.chkZamanAktif.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Depth = 0;
            this.label5.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label5.FontType = MaterialSkin.MaterialSkinManager.fontType.Body1;
            this.label5.Location = new System.Drawing.Point(18, 161);
            this.label5.MouseState = MaterialSkin.MouseState.HOVER;
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(123, 19);
            this.label5.TabIndex = 7;
            this.label5.Text = "Dry Speed (rpm):";
            // 
            // numCalismaDevri
            // 
            this.numCalismaDevri.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.numCalismaDevri.Location = new System.Drawing.Point(165, 159);
            this.numCalismaDevri.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numCalismaDevri.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            this.numCalismaDevri.Name = "numCalismaDevri";
            this.numCalismaDevri.Size = new System.Drawing.Size(110, 24);
            this.numCalismaDevri.TabIndex = 8;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Depth = 0;
            this.label6.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label6.FontType = MaterialSkin.MaterialSkinManager.fontType.Body1;
            this.label6.Location = new System.Drawing.Point(18, 195);
            this.label6.MouseState = MaterialSkin.MouseState.HOVER;
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(139, 19);
            this.label6.TabIndex = 9;
            this.label6.Text = "Cooling Time (min):";
            // 
            // numSogutmaZamani
            // 
            this.numSogutmaZamani.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.numSogutmaZamani.Location = new System.Drawing.Point(165, 193);
            this.numSogutmaZamani.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numSogutmaZamani.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            this.numSogutmaZamani.Name = "numSogutmaZamani";
            this.numSogutmaZamani.Size = new System.Drawing.Size(110, 24);
            this.numSogutmaZamani.TabIndex = 10;
            // 
            // KurutmaReçete_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.materialCard1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "KurutmaReçete_Control";
            this.Size = new System.Drawing.Size(390, 338);
            ((System.ComponentModel.ISupportInitialize)(this.numSicaklik)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNem)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numZaman)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCalismaDevri)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSogutmaZamani)).EndInit();
            this.materialCard1.ResumeLayout(false);
            this.materialCard1.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private MaterialSkin.Controls.MaterialCard materialCard1;
        private MaterialSkin.Controls.MaterialLabel label1;
        private System.Windows.Forms.NumericUpDown numSicaklik;
        private System.Windows.Forms.NumericUpDown numNem;
        private MaterialSkin.Controls.MaterialLabel label2;
        private System.Windows.Forms.NumericUpDown numZaman;
        private MaterialSkin.Controls.MaterialLabel label3;
        private MaterialSkin.Controls.MaterialLabel label4;
        private System.Windows.Forms.NumericUpDown numCalismaDevri;
        private MaterialSkin.Controls.MaterialLabel label5;
        private System.Windows.Forms.NumericUpDown numSogutmaZamani;
        private MaterialSkin.Controls.MaterialLabel label6;
        private MaterialSkin.Controls.MaterialCheckbox chkNemAktif;
        private MaterialSkin.Controls.MaterialCheckbox chkZamanAktif;
    }
}