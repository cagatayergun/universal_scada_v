// UI/Views/AlarmSettings_Control.Designer.cs
namespace Telemetry.UI.Views
{
    partial class AlarmSettings_Control
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
            this.dgvAlarms = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnDelete = new MaterialSkin.Controls.MaterialButton(); // MaterialButton olarak güncellendi
            this.btnSave = new MaterialSkin.Controls.MaterialButton();   // MaterialButton olarak güncellendi
            this.btnNew = new MaterialSkin.Controls.MaterialButton();    // MaterialButton olarak güncellendi
            this.txtCategory = new System.Windows.Forms.TextBox();
            this.label4 = new MaterialSkin.Controls.MaterialLabel();      // MaterialLabel olarak güncellendi
            this.numSeverity = new System.Windows.Forms.NumericUpDown();
            this.label3 = new MaterialSkin.Controls.MaterialLabel();      // MaterialLabel olarak güncellendi
            this.txtAlarmText = new System.Windows.Forms.TextBox();
            this.label2 = new MaterialSkin.Controls.MaterialLabel();      // MaterialLabel olarak güncellendi
            this.numAlarmNo = new System.Windows.Forms.NumericUpDown();
            this.label1 = new MaterialSkin.Controls.MaterialLabel();      // MaterialLabel olarak güncellendi
            ((System.ComponentModel.ISupportInitialize)(this.dgvAlarms)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSeverity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAlarmNo)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvAlarms
            // 
            this.dgvAlarms.AllowUserToAddRows = false;
            this.dgvAlarms.AllowUserToDeleteRows = false;
            this.dgvAlarms.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAlarms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAlarms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAlarms.Location = new System.Drawing.Point(0, 0);
            this.dgvAlarms.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvAlarms.MultiSelect = false;
            this.dgvAlarms.Name = "dgvAlarms";
            this.dgvAlarms.ReadOnly = true;
            this.dgvAlarms.RowHeadersWidth = 51;
            this.dgvAlarms.RowTemplate.Height = 26;
            this.dgvAlarms.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAlarms.Size = new System.Drawing.Size(700, 450);
            this.dgvAlarms.TabIndex = 0;
            this.dgvAlarms.SelectionChanged += new System.EventHandler(this.dgvAlarms_SelectionChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnDelete);
            this.groupBox1.Controls.Add(this.btnSave);
            this.groupBox1.Controls.Add(this.btnNew);
            this.groupBox1.Controls.Add(this.txtCategory);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.numSeverity);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtAlarmText);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.numAlarmNo);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox1.Location = new System.Drawing.Point(0, 262);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(700, 188);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Alarm Details";
            // 
            // btnDelete
            // 
            this.btnDelete.AutoSize = false;
            this.btnDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnDelete.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnDelete.Depth = 0;
            this.btnDelete.HighEmphasis = false;
            this.btnDelete.Icon = null;
            this.btnDelete.Location = new System.Drawing.Point(600, 138); // Yükseklik artışı için Y koordinatı optimize edildi
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnDelete.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnDelete.Size = new System.Drawing.Size(85, 36); // Metin kırpılmasını önleyen ideal flat genişlik
            this.btnDelete.TabIndex = 10;
            this.btnDelete.Text = "Delete";
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
            this.btnSave.HighEmphasis = true; // Ana aksiyon (Kaydet) vurgulandı
            this.btnSave.Icon = null;
            this.btnSave.Location = new System.Drawing.Point(505, 138);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSave.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSave.Name = "btnSave";
            this.btnSave.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSave.Size = new System.Drawing.Size(85, 36);
            this.btnSave.TabIndex = 9;
            this.btnSave.Text = "Save";
            this.btnSave.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained; // Dolgulu baskın stil
            this.btnSave.UseAccentColor = true;
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
            this.btnNew.Location = new System.Drawing.Point(410, 138);
            this.btnNew.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnNew.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnNew.Name = "btnNew";
            this.btnNew.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnNew.Size = new System.Drawing.Size(85, 36);
            this.btnNew.TabIndex = 8;
            this.btnNew.Text = "New";
            this.btnNew.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined; // Çizgili flat stil
            this.btnNew.UseAccentColor = false;
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // txtCategory
            // 
            this.txtCategory.Location = new System.Drawing.Point(120, 112);
            this.txtCategory.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtCategory.Name = "txtCategory";
            this.txtCategory.Size = new System.Drawing.Size(219, 23);
            this.txtCategory.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Depth = 0;
            this.label4.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label4.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label4.Location = new System.Drawing.Point(18, 115);
            this.label4.MouseState = MaterialSkin.MouseState.HOVER;
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 17);
            this.label4.TabIndex = 6;
            this.label4.Text = "Category:";
            // 
            // numSeverity
            // 
            this.numSeverity.Location = new System.Drawing.Point(120, 82);
            this.numSeverity.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numSeverity.Maximum = new decimal(new int[] { 4, 0, 0, 0 });
            this.numSeverity.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numSeverity.Name = "numSeverity";
            this.numSeverity.Size = new System.Drawing.Size(131, 23);
            this.numSeverity.TabIndex = 5;
            this.numSeverity.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Depth = 0;
            this.label3.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label3.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label3.Location = new System.Drawing.Point(18, 84);
            this.label3.MouseState = MaterialSkin.MouseState.HOVER;
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Severity:";
            // 
            // txtAlarmText
            // 
            this.txtAlarmText.Location = new System.Drawing.Point(120, 52);
            this.txtAlarmText.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtAlarmText.Name = "txtAlarmText";
            this.txtAlarmText.Size = new System.Drawing.Size(438, 23);
            this.txtAlarmText.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Depth = 0;
            this.label2.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label2.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label2.Location = new System.Drawing.Point(18, 55);
            this.label2.MouseState = MaterialSkin.MouseState.HOVER;
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Alarm Text:";
            // 
            // numAlarmNo
            // 
            this.numAlarmNo.Location = new System.Drawing.Point(120, 22);
            this.numAlarmNo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numAlarmNo.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numAlarmNo.Name = "numAlarmNo";
            this.numAlarmNo.Size = new System.Drawing.Size(131, 23);
            this.numAlarmNo.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Depth = 0;
            this.label1.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label1.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label1.Location = new System.Drawing.Point(18, 24);
            this.label1.MouseState = MaterialSkin.MouseState.HOVER;
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Alarm Number:";
            // 
            // AlarmSettings_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent; // Arka plan yönetimi üst ebeveyne devredildi
            this.Controls.Add(this.dgvAlarms); // Kod arkasındaki yerleşim motorunun doğru çalışması için tablo önce eklenir
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "AlarmSettings_Control";
            this.Size = new System.Drawing.Size(700, 450);
            this.Load += new System.EventHandler(this.AlarmSettings_Control_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAlarms)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSeverity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAlarmNo)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion
        private System.Windows.Forms.DataGridView dgvAlarms;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown numAlarmNo;
        private MaterialSkin.Controls.MaterialLabel label1;         // Tür güncellendi
        private System.Windows.Forms.TextBox txtAlarmText;
        private MaterialSkin.Controls.MaterialLabel label2;         // Tür güncellendi
        private System.Windows.Forms.NumericUpDown numSeverity;
        private MaterialSkin.Controls.MaterialLabel label3;         // Tür güncellendi
        private System.Windows.Forms.TextBox txtCategory;
        private MaterialSkin.Controls.MaterialLabel label4;         // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnDelete;     // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnSave;       // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnNew;        // Tür güncellendi
    }
}