// UI/Views/TrendAnaliz_Control.Designer.cs
namespace Telemetry.UI.Views
{
    partial class TrendAnaliz_Control
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
            this.btnGenerateChart = new MaterialSkin.Controls.MaterialButton(); // MaterialButton yapıldı
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkRpm = new System.Windows.Forms.CheckBox();
            this.chkWaterLevel = new System.Windows.Forms.CheckBox();
            this.chkTemperature = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.clbMachines = new System.Windows.Forms.CheckedListBox();
            this.dtpEndTime = new System.Windows.Forms.DateTimePicker();
            this.label2 = new MaterialSkin.Controls.MaterialLabel();              // MaterialLabel yapıldı
            this.dtpStartTime = new System.Windows.Forms.DateTimePicker();
            this.label1 = new MaterialSkin.Controls.MaterialLabel();              // MaterialLabel yapıldı
            this.formsPlot1 = new ScottPlot.WinForms.FormsPlot();
            this.pnlFilters.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlFilters
            // 
            this.pnlFilters.BackColor = System.Drawing.Color.Transparent; // Koyu mod bütünlüğü için şeffaflaştırıldı
            this.pnlFilters.Controls.Add(this.btnGenerateChart);
            this.pnlFilters.Controls.Add(this.groupBox2);
            this.pnlFilters.Controls.Add(this.groupBox1);
            this.pnlFilters.Controls.Add(this.dtpEndTime);
            this.pnlFilters.Controls.Add(this.label2);
            this.pnlFilters.Controls.Add(this.dtpStartTime);
            this.pnlFilters.Controls.Add(this.label1);
            this.pnlFilters.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlFilters.Location = new System.Drawing.Point(0, 0);
            this.pnlFilters.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlFilters.Name = "pnlFilters";
            this.pnlFilters.Padding = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.pnlFilters.Size = new System.Drawing.Size(219, 450);
            this.pnlFilters.TabIndex = 0;
            // 
            // btnGenerateChart
            // 
            this.btnGenerateChart.AutoSize = false;
            this.btnGenerateChart.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnGenerateChart.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnGenerateChart.Depth = 0;
            this.btnGenerateChart.HighEmphasis = true; // Dolgulu baskın vurgu aktif
            this.btnGenerateChart.Icon = null;
            this.btnGenerateChart.Location = new System.Drawing.Point(11, 338);
            this.btnGenerateChart.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnGenerateChart.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnGenerateChart.Name = "btnGenerateChart";
            this.btnGenerateChart.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnGenerateChart.Size = new System.Drawing.Size(196, 36); // Modern material standardı olan 36px mühürlendi
            this.btnGenerateChart.TabIndex = 6;
            this.btnGenerateChart.Text = "Create Chart";
            this.btnGenerateChart.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnGenerateChart.UseAccentColor = true; // Dikkat çekici aksan rengi tetiği
            this.btnGenerateChart.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Transparent;
            this.groupBox2.Controls.Add(this.chkRpm);
            this.groupBox2.Controls.Add(this.chkWaterLevel);
            this.groupBox2.Controls.Add(this.chkTemperature);
            this.groupBox2.ForeColor = System.Drawing.Color.Gray; // Pasif durumdaki sınır başlığı rengi dengelendi
            this.groupBox2.Location = new System.Drawing.Point(11, 240);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox2.Size = new System.Drawing.Size(196, 86);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Data to Display";
            // 
            // chkRpm
            // 
            this.chkRpm.AutoSize = true;
            this.chkRpm.Location = new System.Drawing.Point(13, 60);
            this.chkRpm.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chkRpm.Name = "chkRpm";
            this.chkRpm.Size = new System.Drawing.Size(51, 19);
            this.chkRpm.TabIndex = 2;
            this.chkRpm.Text = "RPM";
            this.chkRpm.UseVisualStyleBackColor = true;
            // 
            // chkWaterLevel
            // 
            this.chkWaterLevel.AutoSize = true;
            this.chkWaterLevel.Location = new System.Drawing.Point(13, 42);
            this.chkWaterLevel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chkWaterLevel.Name = "chkWaterLevel";
            this.chkWaterLevel.Size = new System.Drawing.Size(84, 19);
            this.chkWaterLevel.TabIndex = 1;
            this.chkWaterLevel.Text = "Water level";
            this.chkWaterLevel.UseVisualStyleBackColor = true;
            // 
            // chkTemperature
            // 
            this.chkTemperature.Checked = true;
            this.chkTemperature.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTemperature.Location = new System.Drawing.Point(13, 20);
            this.chkTemperature.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chkTemperature.Name = "chkTemperature";
            this.chkTemperature.Size = new System.Drawing.Size(158, 24);
            this.chkTemperature.TabIndex = 0;
            this.chkTemperature.Text = "Temperature";
            this.chkTemperature.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.clbMachines);
            this.groupBox1.ForeColor = System.Drawing.Color.Gray;
            this.groupBox1.Location = new System.Drawing.Point(11, 87);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(196, 150);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Machines";
            // 
            // clbMachines
            // 
            this.clbMachines.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clbMachines.FormattingEnabled = true;
            this.clbMachines.Location = new System.Drawing.Point(3, 18);
            this.clbMachines.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.clbMachines.Name = "clbMachines";
            this.clbMachines.Size = new System.Drawing.Size(190, 130);
            this.clbMachines.TabIndex = 0;
            // 
            // dtpEndTime
            // 
            this.dtpEndTime.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dtpEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEndTime.Location = new System.Drawing.Point(11, 61);
            this.dtpEndTime.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dtpEndTime.Name = "dtpEndTime";
            this.dtpEndTime.Size = new System.Drawing.Size(196, 23);
            this.dtpEndTime.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Depth = 0;
            this.label2.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label2.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label2.Location = new System.Drawing.Point(11, 44);
            this.label2.MouseState = MaterialSkin.MouseState.HOVER;
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "End Date:";
            // 
            // dtpStartTime
            // 
            this.dtpStartTime.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dtpStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStartTime.Location = new System.Drawing.Point(11, 18);
            this.dtpStartTime.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dtpStartTime.Name = "dtpStartTime";
            this.dtpStartTime.Size = new System.Drawing.Size(196, 23);
            this.dtpStartTime.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Depth = 0;
            this.label1.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label1.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label1.Location = new System.Drawing.Point(11, 1);
            this.label1.MouseState = MaterialSkin.MouseState.HOVER;
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Start Date:";
            // 
            // formsPlot1
            // 
            this.formsPlot1.DisplayScale = 1F;
            this.formsPlot1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsPlot1.Location = new System.Drawing.Point(219, 0);
            this.formsPlot1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.formsPlot1.Name = "formsPlot1";
            this.formsPlot1.Size = new System.Drawing.Size(481, 450);
            this.formsPlot1.TabIndex = 1;
            // 
            // TrendAnaliz_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent; // Panel transparanlığı aktif edildi
            this.Controls.Add(this.formsPlot1);
            this.Controls.Add(this.pnlFilters);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "TrendAnaliz_Control";
            this.Size = new System.Drawing.Size(700, 450);
            this.pnlFilters.ResumeLayout(false);
            this.pnlFilters.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlFilters;
        private MaterialSkin.Controls.MaterialButton btnGenerateChart;    // Tür güncellendi
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkRpm;
        private System.Windows.Forms.CheckBox chkWaterLevel;
        private System.Windows.Forms.CheckBox chkTemperature;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckedListBox clbMachines;
        private System.Windows.Forms.DateTimePicker dtpEndTime;
        private MaterialSkin.Controls.MaterialLabel label2;                // Tür güncellendi
        private System.Windows.Forms.DateTimePicker dtpStartTime;
        private MaterialSkin.Controls.MaterialLabel label1;                // Tür güncellendi
        private ScottPlot.WinForms.FormsPlot formsPlot1;
    }
}