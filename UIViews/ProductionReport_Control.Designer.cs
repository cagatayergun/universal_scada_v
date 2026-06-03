// UI/Views/ProductionReport_Control.Designer.cs
namespace Telemetry.UI.Views
{
    partial class ProductionReport_Control
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
            this.pnlFilters = new System.Windows.Forms.Panel();
            this.btnExportToExcel = new MaterialSkin.Controls.MaterialButton(); // MaterialButton yapıldı
            this.txtOperator = new System.Windows.Forms.TextBox();
            this.label9 = new MaterialSkin.Controls.MaterialLabel();              // MaterialLabel yapıldı
            this.txtCustomerNo = new System.Windows.Forms.TextBox();
            this.label8 = new MaterialSkin.Controls.MaterialLabel();              // MaterialLabel yapıldı
            this.txtOrderNo = new System.Windows.Forms.TextBox();
            this.label7 = new MaterialSkin.Controls.MaterialLabel();              // MaterialLabel yapıldı
            this.txtRecipeName = new System.Windows.Forms.TextBox();
            this.label5 = new MaterialSkin.Controls.MaterialLabel();              // MaterialLabel yapıldı
            this.txtBatchNo = new System.Windows.Forms.TextBox();
            this.label4 = new MaterialSkin.Controls.MaterialLabel();              // MaterialLabel yapıldı
            this.btnGenerateReport = new MaterialSkin.Controls.MaterialButton(); // MaterialButton yapıldı
            this.cmbMachines = new System.Windows.Forms.ComboBox();
            this.label3 = new MaterialSkin.Controls.MaterialLabel();              // MaterialLabel yapıldı
            this.dtpEndTime = new System.Windows.Forms.DateTimePicker();
            this.label2 = new MaterialSkin.Controls.MaterialLabel();              // MaterialLabel yapıldı
            this.dtpStartTime = new System.Windows.Forms.DateTimePicker();
            this.label1 = new MaterialSkin.Controls.MaterialLabel();              // MaterialLabel yapıldı
            this.dgvProductionReport = new System.Windows.Forms.DataGridView();
            this.pnlFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductionReport)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlFilters
            // 
            this.pnlFilters.BackColor = System.Drawing.Color.Transparent; // Koyu mod parlamasını engelleyen şeffaflık
            this.pnlFilters.Controls.Add(this.btnExportToExcel);
            this.pnlFilters.Controls.Add(this.txtOperator);
            this.pnlFilters.Controls.Add(this.label9);
            this.pnlFilters.Controls.Add(this.txtCustomerNo);
            this.pnlFilters.Controls.Add(this.label8);
            this.pnlFilters.Controls.Add(this.txtOrderNo);
            this.pnlFilters.Controls.Add(this.label7);
            this.pnlFilters.Controls.Add(this.txtRecipeName);
            this.pnlFilters.Controls.Add(this.label5);
            this.pnlFilters.Controls.Add(this.txtBatchNo);
            this.pnlFilters.Controls.Add(this.label4);
            this.pnlFilters.Controls.Add(this.btnGenerateReport);
            this.pnlFilters.Controls.Add(this.cmbMachines);
            this.pnlFilters.Controls.Add(this.label3);
            this.pnlFilters.Controls.Add(this.dtpEndTime);
            this.pnlFilters.Controls.Add(this.label2);
            this.pnlFilters.Controls.Add(this.dtpStartTime);
            this.pnlFilters.Controls.Add(this.label1);
            this.pnlFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilters.Location = new System.Drawing.Point(0, 0);
            this.pnlFilters.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlFilters.Name = "pnlFilters";
            this.pnlFilters.Size = new System.Drawing.Size(970, 110); // 36px dikey material buton payı için 110px'e esnetildi
            this.pnlFilters.TabIndex = 0;
            // 
            // btnExportToExcel
            // 
            this.btnExportToExcel.AutoSize = false;
            this.btnExportToExcel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnExportToExcel.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnExportToExcel.Depth = 0;
            this.btnExportToExcel.HighEmphasis = false;
            this.btnExportToExcel.Icon = null;
            this.btnExportToExcel.Location = new System.Drawing.Point(835, 68); // Çizgili material buton konum hizalaması
            this.btnExportToExcel.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnExportToExcel.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnExportToExcel.Name = "btnExportToExcel";
            this.btnExportToExcel.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnExportToExcel.Size = new System.Drawing.Size(118, 36);
            this.btnExportToExcel.TabIndex = 17;
            this.btnExportToExcel.Text = "Export to Excel";
            this.btnExportToExcel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined; // Çizgili ikincil flat tasarım
            this.btnExportToExcel.UseAccentColor = false;
            this.btnExportToExcel.UseVisualStyleBackColor = true;
            this.btnExportToExcel.Click += new System.EventHandler(this.btnExportToExcel_Click);
            // 
            // txtOperator
            // 
            this.txtOperator.Location = new System.Drawing.Point(764, 36);
            this.txtOperator.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtOperator.Name = "txtOperator";
            this.txtOperator.Size = new System.Drawing.Size(189, 23);
            this.txtOperator.TabIndex = 16;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Depth = 0;
            this.label9.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label9.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label9.Location = new System.Drawing.Point(697, 39);
            this.label9.MouseState = MaterialSkin.MouseState.HOVER;
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(58, 17);
            this.label9.TabIndex = 15;
            this.label9.Text = "Operator:";
            // 
            // txtCustomerNo
            // 
            this.txtCustomerNo.Location = new System.Drawing.Point(480, 36);
            this.txtCustomerNo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtCustomerNo.Name = "txtCustomerNo";
            this.txtCustomerNo.Size = new System.Drawing.Size(178, 23);
            this.txtCustomerNo.TabIndex = 14;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Depth = 0;
            this.label8.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label8.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label8.Location = new System.Drawing.Point(359, 39);
            this.label8.MouseState = MaterialSkin.MouseState.HOVER;
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(117, 17);
            this.label8.TabIndex = 13;
            this.label8.Text = "Customer Number:";
            // 
            // txtOrderNo
            // 
            this.txtOrderNo.Location = new System.Drawing.Point(231, 36);
            this.txtOrderNo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtOrderNo.Name = "txtOrderNo";
            this.txtOrderNo.Size = new System.Drawing.Size(114, 23);
            this.txtOrderNo.TabIndex = 12;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Depth = 0;
            this.label7.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label7.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label7.Location = new System.Drawing.Point(158, 39);
            this.label7.MouseState = MaterialSkin.MouseState.HOVER;
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 17);
            this.label7.TabIndex = 11;
            this.label7.Text = "Order No:";
            // 
            // txtRecipeName
            // 
            this.txtRecipeName.Location = new System.Drawing.Point(764, 9);
            this.txtRecipeName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtRecipeName.Name = "txtRecipeName";
            this.txtRecipeName.Size = new System.Drawing.Size(189, 23);
            this.txtRecipeName.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Depth = 0;
            this.label5.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label5.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label5.Location = new System.Drawing.Point(686, 12);
            this.label5.MouseState = MaterialSkin.MouseState.HOVER;
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 17);
            this.label5.TabIndex = 9;
            this.label5.Text = "Recipe Name:";
            // 
            // txtBatchNo
            // 
            this.txtBatchNo.Location = new System.Drawing.Point(74, 36);
            this.txtBatchNo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtBatchNo.Name = "txtBatchNo";
            this.txtBatchNo.Size = new System.Drawing.Size(79, 23);
            this.txtBatchNo.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Depth = 0;
            this.label4.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label4.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label4.Location = new System.Drawing.Point(9, 39);
            this.label4.MouseState = MaterialSkin.MouseState.HOVER;
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "Batch No:";
            // 
            // btnGenerateReport
            // 
            this.btnGenerateReport.AutoSize = false;
            this.btnGenerateReport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnGenerateReport.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnGenerateReport.Depth = 0;
            this.btnGenerateReport.HighEmphasis = true; // Ana operasyon (Rapor Oluştur) vurgulandı
            this.btnGenerateReport.Icon = null;
            this.btnGenerateReport.Location = new System.Drawing.Point(732, 68);
            this.btnGenerateReport.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnGenerateReport.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnGenerateReport.Name = "btnGenerateReport";
            this.btnGenerateReport.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnGenerateReport.Size = new System.Drawing.Size(95, 36);
            this.btnGenerateReport.TabIndex = 6;
            this.btnGenerateReport.Text = "Report";
            this.btnGenerateReport.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained; // Dolgulu baskın tasarım
            this.btnGenerateReport.UseAccentColor = true; // Dikkat çekici aksan rengiaktif
            this.btnGenerateReport.UseVisualStyleBackColor = true;
            this.btnGenerateReport.Click += new System.EventHandler(this.btnGenerateReport_Click);
            // 
            // cmbMachines
            // 
            this.cmbMachines.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMachines.FormattingEnabled = true;
            this.cmbMachines.Location = new System.Drawing.Point(480, 9);
            this.cmbMachines.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbMachines.Name = "cmbMachines";
            this.cmbMachines.Size = new System.Drawing.Size(178, 23);
            this.cmbMachines.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Depth = 0;
            this.label3.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label3.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label3.Location = new System.Drawing.Point(412, 12);
            this.label3.MouseState = MaterialSkin.MouseState.HOVER;
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Machine:";
            // 
            // dtpEndTime
            // 
            this.dtpEndTime.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dtpEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEndTime.Location = new System.Drawing.Point(216, 9);
            this.dtpEndTime.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dtpEndTime.Name = "dtpEndTime";
            this.dtpEndTime.Size = new System.Drawing.Size(132, 23);
            this.dtpEndTime.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Depth = 0;
            this.label2.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.label2.FontType = MaterialSkin.MaterialSkinManager.fontType.Button;
            this.label2.Location = new System.Drawing.Point(198, 10);
            this.label2.MouseState = MaterialSkin.MouseState.HOVER;
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(6, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "-";
            // 
            // dtpStartTime
            // 
            this.dtpStartTime.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dtpStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStartTime.Location = new System.Drawing.Point(80, 9);
            this.dtpStartTime.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dtpStartTime.Name = "dtpStartTime";
            this.dtpStartTime.Size = new System.Drawing.Size(115, 23);
            this.dtpStartTime.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Depth = 0;
            this.label1.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label1.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label1.Location = new System.Drawing.Point(9, 12);
            this.label1.MouseState = MaterialSkin.MouseState.HOVER;
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Date Range:";
            // 
            // dgvProductionReport
            // 
            this.dgvProductionReport.AllowUserToAddRows = false;
            this.dgvProductionReport.AllowUserToDeleteRows = false;
            this.dgvProductionReport.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProductionReport.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProductionReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProductionReport.Location = new System.Drawing.Point(0, 110); // Panelin yeni yüksekliğine göre koordinat kaydırıldı
            this.dgvProductionReport.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvProductionReport.Name = "dgvProductionReport";
            this.dgvProductionReport.ReadOnly = true;
            this.dgvProductionReport.RowHeadersWidth = 51;
            this.dgvProductionReport.RowTemplate.Height = 36;
            this.dgvProductionReport.Size = new System.Drawing.Size(970, 340);
            this.dgvProductionReport.TabIndex = 1;
            // 
            // ProductionReport_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent; // Sayfa şeffaflığı aktif edildi
            this.Controls.Add(this.dgvProductionReport);
            this.Controls.Add(this.pnlFilters);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "ProductionReport_Control";
            this.Size = new System.Drawing.Size(970, 450);
            this.Load += new System.EventHandler(this.ProductionReport_Control_Load);
            this.pnlFilters.ResumeLayout(false);
            this.pnlFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductionReport)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlFilters;
        private MaterialSkin.Controls.MaterialButton btnGenerateReport; // Tür güncellendi
        private System.Windows.Forms.ComboBox cmbMachines;
        private MaterialSkin.Controls.MaterialLabel label3;             // Tür güncellendi
        private System.Windows.Forms.DateTimePicker dtpEndTime;
        private MaterialSkin.Controls.MaterialLabel label2;              // Tür güncellendi
        private System.Windows.Forms.DateTimePicker dtpStartTime;
        private MaterialSkin.Controls.MaterialLabel label1;              // Tür güncellendi
        private System.Windows.Forms.DataGridView dgvProductionReport;
        private System.Windows.Forms.TextBox txtBatchNo;
        private MaterialSkin.Controls.MaterialLabel label4;              // Tür güncellendi
        private System.Windows.Forms.TextBox txtRecipeName;
        private MaterialSkin.Controls.MaterialLabel label5;              // Tür güncellendi
        private System.Windows.Forms.TextBox txtOrderNo;
        private MaterialSkin.Controls.MaterialLabel label7;              // Tür güncellendi
        private System.Windows.Forms.TextBox txtCustomerNo;
        private MaterialSkin.Controls.MaterialLabel label8;              // Tür güncellendi
        private System.Windows.Forms.TextBox txtOperator;
        private MaterialSkin.Controls.MaterialLabel label9;              // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnExportToExcel;  // Tür güncellendi
    }
}