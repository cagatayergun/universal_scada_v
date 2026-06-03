// UI/Views/RecipeOptimization_Control.Designer.cs
namespace Telemetry.UI.Views
{
    partial class RecipeOptimization_Control
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
            this.btnAnalyze = new MaterialSkin.Controls.MaterialButton();
            this.cmbRecipe2 = new System.Windows.Forms.ComboBox();
            this.label7 = new MaterialSkin.Controls.MaterialLabel();
            this.cmbRecipes = new System.Windows.Forms.ComboBox();
            this.label1 = new MaterialSkin.Controls.MaterialLabel();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.dgvHistory = new System.Windows.Forms.DataGridView();
            this.label2 = new MaterialSkin.Controls.MaterialLabel();
            this.pnlAverages = new System.Windows.Forms.Panel();
            this.pnlCompare = new System.Windows.Forms.Panel();
            this.lblAvgSteam2 = new System.Windows.Forms.Label();
            this.label8 = new MaterialSkin.Controls.MaterialLabel();
            this.lblAvgElectricity2 = new System.Windows.Forms.Label();
            this.label10 = new MaterialSkin.Controls.MaterialLabel();
            this.lblAvgCycleTime2 = new System.Windows.Forms.Label();
            this.label12 = new MaterialSkin.Controls.MaterialLabel();
            this.lblAvgWater2 = new System.Windows.Forms.Label();
            this.label14 = new MaterialSkin.Controls.MaterialLabel();
            this.pnlOriginal = new System.Windows.Forms.Panel();
            this.lblAvgSteam = new System.Windows.Forms.Label();
            this.label6 = new MaterialSkin.Controls.MaterialLabel();
            this.lblAvgElectricity = new System.Windows.Forms.Label();
            this.label5 = new MaterialSkin.Controls.MaterialLabel();
            this.lblAvgCycleTime = new System.Windows.Forms.Label();
            this.label4 = new MaterialSkin.Controls.MaterialLabel();
            this.lblAvgWater = new System.Windows.Forms.Label();
            this.label3 = new MaterialSkin.Controls.MaterialLabel();
            this.pnlFilters.SuspendLayout();
            this.pnlContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistory)).BeginInit();
            this.pnlAverages.SuspendLayout();
            this.pnlCompare.SuspendLayout();
            this.pnlOriginal.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlFilters
            // 
            this.pnlFilters.BackColor = System.Drawing.Color.Transparent;
            this.pnlFilters.Controls.Add(this.btnAnalyze);
            this.pnlFilters.Controls.Add(this.cmbRecipe2);
            this.pnlFilters.Controls.Add(this.label7);
            this.pnlFilters.Controls.Add(this.cmbRecipes);
            this.pnlFilters.Controls.Add(this.label1);
            this.pnlFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilters.Location = new System.Drawing.Point(0, 0);
            this.pnlFilters.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlFilters.Name = "pnlFilters";
            this.pnlFilters.Size = new System.Drawing.Size(788, 64);
            this.pnlFilters.TabIndex = 0;
            // 
            // btnAnalyze
            // 
            this.btnAnalyze.AutoSize = false;
            this.btnAnalyze.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAnalyze.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnAnalyze.Depth = 0;
            this.btnAnalyze.HighEmphasis = true;
            this.btnAnalyze.Icon = null;
            this.btnAnalyze.Location = new System.Drawing.Point(668, 14);
            this.btnAnalyze.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnAnalyze.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnAnalyze.Size = new System.Drawing.Size(105, 36);
            this.btnAnalyze.TabIndex = 2;
            this.btnAnalyze.Text = "Analyze";
            this.btnAnalyze.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnAnalyze.UseAccentColor = true;
            this.btnAnalyze.UseVisualStyleBackColor = true;
            this.btnAnalyze.Click += new System.EventHandler(this.btnAnalyze_Click);
            // 
            // cmbRecipe2
            // 
            this.cmbRecipe2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRecipe2.FormattingEnabled = true;
            this.cmbRecipe2.Location = new System.Drawing.Point(476, 20);
            this.cmbRecipe2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbRecipe2.Name = "cmbRecipe2";
            this.cmbRecipe2.Size = new System.Drawing.Size(176, 23);
            this.cmbRecipe2.TabIndex = 3;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Depth = 0;
            this.label7.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label7.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label7.Location = new System.Drawing.Point(371, 23);
            this.label7.MouseState = MaterialSkin.MouseState.HOVER;
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(76, 17);
            this.label7.TabIndex = 4;
            this.label7.Text = "To compare:";
            // 
            // cmbRecipes
            // 
            this.cmbRecipes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRecipes.FormattingEnabled = true;
            this.cmbRecipes.Location = new System.Drawing.Point(100, 20);
            this.cmbRecipes.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbRecipes.Name = "cmbRecipes";
            this.cmbRecipes.Size = new System.Drawing.Size(263, 23);
            this.cmbRecipes.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Depth = 0;
            this.label1.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label1.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label1.Location = new System.Drawing.Point(13, 23);
            this.label1.MouseState = MaterialSkin.MouseState.HOVER;
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Main Recipe:";
            // 
            // pnlContent
            // 
            this.pnlContent.BackColor = System.Drawing.Color.Transparent;
            this.pnlContent.Controls.Add(this.dgvHistory);
            this.pnlContent.Controls.Add(this.label2);
            this.pnlContent.Controls.Add(this.pnlAverages);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(0, 64);
            this.pnlContent.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(788, 386);
            this.pnlContent.TabIndex = 1;
            // 
            // dgvHistory
            // 
            this.dgvHistory.AllowUserToAddRows = false;
            this.dgvHistory.AllowUserToDeleteRows = false;
            this.dgvHistory.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvHistory.Location = new System.Drawing.Point(0, 128);
            this.dgvHistory.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvHistory.Name = "dgvHistory";
            this.dgvHistory.ReadOnly = true;
            this.dgvHistory.RowHeadersWidth = 51;
            this.dgvHistory.RowTemplate.Height = 36;
            this.dgvHistory.Size = new System.Drawing.Size(788, 258);
            this.dgvHistory.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Depth = 0;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.label2.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle2;
            this.label2.Location = new System.Drawing.Point(0, 94);
            this.label2.MouseState = MaterialSkin.MouseState.HOVER;
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(788, 34);
            this.label2.TabIndex = 1;
            this.label2.Text = "Geçmiş Üretimler";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlAverages
            // 
            this.pnlAverages.BackColor = System.Drawing.Color.Transparent;
            this.pnlAverages.Controls.Add(this.pnlCompare);
            this.pnlAverages.Controls.Add(this.pnlOriginal);
            this.pnlAverages.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAverages.Location = new System.Drawing.Point(0, 0);
            this.pnlAverages.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlAverages.Name = "pnlAverages";
            this.pnlAverages.Size = new System.Drawing.Size(788, 94);
            this.pnlAverages.TabIndex = 0;
            // 
            // pnlCompare
            // 
            this.pnlCompare.BackColor = System.Drawing.Color.Transparent;
            this.pnlCompare.Controls.Add(this.lblAvgSteam2);
            this.pnlCompare.Controls.Add(this.label8);
            this.pnlCompare.Controls.Add(this.lblAvgElectricity2);
            this.pnlCompare.Controls.Add(this.label10);
            this.pnlCompare.Controls.Add(this.lblAvgCycleTime2);
            this.pnlCompare.Controls.Add(this.label12);
            this.pnlCompare.Controls.Add(this.lblAvgWater2);
            this.pnlCompare.Controls.Add(this.label14);
            this.pnlCompare.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCompare.Location = new System.Drawing.Point(394, 0);
            this.pnlCompare.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlCompare.Name = "pnlCompare";
            this.pnlCompare.Size = new System.Drawing.Size(394, 94);
            this.pnlCompare.TabIndex = 1;
            // 
            // lblAvgSteam2
            // 
            this.lblAvgSteam2.Location = new System.Drawing.Point(197, 64);
            this.lblAvgSteam2.Name = "lblAvgSteam2";
            this.lblAvgSteam2.Size = new System.Drawing.Size(175, 30);
            this.lblAvgSteam2.TabIndex = 15;
            this.lblAvgSteam2.Text = "0 kg";
            this.lblAvgSteam2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.Depth = 0;
            this.label8.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label8.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label8.Location = new System.Drawing.Point(197, 45);
            this.label8.MouseState = MaterialSkin.MouseState.HOVER;
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(175, 19);
            this.label8.TabIndex = 14;
            this.label8.Text = "Average Steam Consumption";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAvgElectricity2
            // 
            this.lblAvgElectricity2.Location = new System.Drawing.Point(13, 64);
            this.lblAvgElectricity2.Name = "lblAvgElectricity2";
            this.lblAvgElectricity2.Size = new System.Drawing.Size(175, 30);
            this.lblAvgElectricity2.TabIndex = 13;
            this.lblAvgElectricity2.Text = "0 kW";
            this.lblAvgElectricity2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label10
            // 
            this.label10.Depth = 0;
            this.label10.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label10.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label10.Location = new System.Drawing.Point(13, 45);
            this.label10.MouseState = MaterialSkin.MouseState.HOVER;
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(175, 19);
            this.label10.TabIndex = 12;
            this.label10.Text = "Average Electricity Consumption";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAvgCycleTime2
            // 
            this.lblAvgCycleTime2.Location = new System.Drawing.Point(197, 19);
            this.lblAvgCycleTime2.Name = "lblAvgCycleTime2";
            this.lblAvgCycleTime2.Size = new System.Drawing.Size(175, 30);
            this.lblAvgCycleTime2.TabIndex = 11;
            this.lblAvgCycleTime2.Text = "00:00:00";
            this.lblAvgCycleTime2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label12
            // 
            this.label12.Depth = 0;
            this.label12.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label12.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label12.Location = new System.Drawing.Point(197, 0);
            this.label12.MouseState = MaterialSkin.MouseState.HOVER;
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(175, 19);
            this.label12.TabIndex = 10;
            this.label12.Text = "Average Cycle Time";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAvgWater2
            // 
            this.lblAvgWater2.Location = new System.Drawing.Point(13, 19);
            this.lblAvgWater2.Name = "lblAvgWater2";
            this.lblAvgWater2.Size = new System.Drawing.Size(175, 30);
            this.lblAvgWater2.TabIndex = 9;
            this.lblAvgWater2.Text = "0 L";
            this.lblAvgWater2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label14
            // 
            this.label14.Depth = 0;
            this.label14.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label14.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label14.Location = new System.Drawing.Point(13, 0);
            this.label14.MouseState = MaterialSkin.MouseState.HOVER;
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(175, 19);
            this.label14.TabIndex = 8;
            this.label14.Text = "Average Water Consumption";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlOriginal
            // 
            this.pnlOriginal.BackColor = System.Drawing.Color.Transparent;
            this.pnlOriginal.Controls.Add(this.lblAvgSteam);
            this.pnlOriginal.Controls.Add(this.label6);
            this.pnlOriginal.Controls.Add(this.lblAvgElectricity);
            this.pnlOriginal.Controls.Add(this.label5);
            this.pnlOriginal.Controls.Add(this.lblAvgCycleTime);
            this.pnlOriginal.Controls.Add(this.label4);
            this.pnlOriginal.Controls.Add(this.lblAvgWater);
            this.pnlOriginal.Controls.Add(this.label3);
            this.pnlOriginal.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlOriginal.Location = new System.Drawing.Point(0, 0);
            this.pnlOriginal.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlOriginal.Name = "pnlOriginal";
            // ÇÖZÜM: CS1660 hatasına yol açan hatalı lambda satırı tamamen kaldırıldı
            this.pnlOriginal.Size = new System.Drawing.Size(394, 94);
            this.pnlOriginal.TabIndex = 0;
            // 
            // lblAvgSteam
            // 
            this.lblAvgSteam.Location = new System.Drawing.Point(197, 64);
            this.lblAvgSteam.Name = "lblAvgSteam";
            this.lblAvgSteam.Size = new System.Drawing.Size(175, 30);
            this.lblAvgSteam.TabIndex = 7;
            this.lblAvgSteam.Text = "0 kg";
            this.lblAvgSteam.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.Depth = 0;
            this.label6.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label6.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label6.Location = new System.Drawing.Point(197, 45);
            this.label6.MouseState = MaterialSkin.MouseState.HOVER;
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(175, 19);
            this.label6.TabIndex = 6;
            this.label6.Text = "Average Steam Consumption";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAvgElectricity
            // 
            this.lblAvgElectricity.Location = new System.Drawing.Point(13, 64);
            this.lblAvgElectricity.Name = "lblAvgElectricity";
            this.lblAvgElectricity.Size = new System.Drawing.Size(175, 30);
            this.lblAvgElectricity.TabIndex = 5;
            this.lblAvgElectricity.Text = "0 kW";
            this.lblAvgElectricity.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.Depth = 0;
            this.label5.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label5.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label5.Location = new System.Drawing.Point(13, 45);
            this.label5.MouseState = MaterialSkin.MouseState.HOVER;
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(175, 19);
            this.label5.TabIndex = 4;
            this.label5.Text = "Average Electricity Consumption";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAvgCycleTime
            // 
            this.lblAvgCycleTime.Location = new System.Drawing.Point(197, 19);
            this.lblAvgCycleTime.Name = "lblAvgCycleTime";
            this.lblAvgCycleTime.Size = new System.Drawing.Size(175, 30);
            this.lblAvgCycleTime.TabIndex = 3;
            this.lblAvgCycleTime.Text = "00:00:00";
            this.lblAvgCycleTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Depth = 0;
            this.label4.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label4.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label4.Location = new System.Drawing.Point(197, 0);
            this.label4.MouseState = MaterialSkin.MouseState.HOVER;
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(175, 19);
            this.label4.TabIndex = 2;
            this.label4.Text = "Average Cycle Time";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAvgWater
            // 
            this.lblAvgWater.Location = new System.Drawing.Point(13, 19);
            this.lblAvgWater.Name = "lblAvgWater";
            this.lblAvgWater.Size = new System.Drawing.Size(175, 30);
            this.lblAvgWater.TabIndex = 1;
            this.lblAvgWater.Text = "0 L";
            this.lblAvgWater.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Depth = 0;
            this.label3.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label3.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label3.Location = new System.Drawing.Point(13, 0);
            this.label3.MouseState = MaterialSkin.MouseState.HOVER;
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(175, 19);
            this.label3.TabIndex = 0;
            this.label3.Text = "Average Water Consumption";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // RecipeOptimization_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlFilters);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "RecipeOptimization_Control";
            this.Size = new System.Drawing.Size(788, 450);
            this.Load += new System.EventHandler(this.RecipeOptimization_Control_Load);
            this.pnlFilters.ResumeLayout(false);
            this.pnlFilters.PerformLayout();
            this.pnlContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistory)).EndInit();
            this.pnlAverages.ResumeLayout(false);
            this.pnlCompare.ResumeLayout(false);
            this.pnlOriginal.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlFilters;
        private MaterialSkin.Controls.MaterialButton btnAnalyze;
        private System.Windows.Forms.ComboBox cmbRecipes;
        private MaterialSkin.Controls.MaterialLabel label1;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.DataGridView dgvHistory;
        private MaterialSkin.Controls.MaterialLabel label2;
        private System.Windows.Forms.Panel pnlAverages;
        private System.Windows.Forms.Label lblAvgCycleTime;
        private MaterialSkin.Controls.MaterialLabel label4;
        private System.Windows.Forms.Label lblAvgWater;
        private MaterialSkin.Controls.MaterialLabel label3;
        private System.Windows.Forms.Label lblAvgSteam;
        private MaterialSkin.Controls.MaterialLabel label6;
        private System.Windows.Forms.Label lblAvgElectricity;
        private MaterialSkin.Controls.MaterialLabel label5;
        private System.Windows.Forms.ComboBox cmbRecipe2;
        private MaterialSkin.Controls.MaterialLabel label7;
        private System.Windows.Forms.Panel pnlCompare;
        private System.Windows.Forms.Label lblAvgSteam2;
        private MaterialSkin.Controls.MaterialLabel label8;
        private System.Windows.Forms.Label lblAvgElectricity2;
        private MaterialSkin.Controls.MaterialLabel label10;
        private System.Windows.Forms.Label lblAvgCycleTime2;
        private MaterialSkin.Controls.MaterialLabel label12;
        private System.Windows.Forms.Label lblAvgWater2;
        private MaterialSkin.Controls.MaterialLabel label14;
        private System.Windows.Forms.Panel pnlOriginal;
    }
}