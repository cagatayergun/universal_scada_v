// UIViews/EfficiencyReport_Control.Designer.cs
namespace Telemetry.UIViews
{
    partial class EfficiencyReport_Control
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            this.panelFilter = new System.Windows.Forms.Panel();
            this.lblStart = new MaterialSkin.Controls.MaterialLabel();      // MaterialLabel yapıldı
            this.lblEnd = new MaterialSkin.Controls.MaterialLabel();        // MaterialLabel yapıldı
            this.lblMachine = new MaterialSkin.Controls.MaterialLabel();    // MaterialLabel yapıldı
            this.lblSubType = new MaterialSkin.Controls.MaterialLabel();    // MaterialLabel yapıldı
            this.lblQuickFilter = new MaterialSkin.Controls.MaterialLabel(); // MaterialLabel yapıldı
            this.dtpStart = new System.Windows.Forms.DateTimePicker();
            this.dtpEnd = new System.Windows.Forms.DateTimePicker();
            this.cmbMachine = new System.Windows.Forms.ComboBox();
            this.cmbSubType = new System.Windows.Forms.ComboBox();
            this.btnSearch = new MaterialSkin.Controls.MaterialButton();    // MaterialButton yapıldı
            this.txtQuickFilter = new System.Windows.Forms.TextBox();
            this.panelKpiCards = new System.Windows.Forms.FlowLayoutPanel();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.splitTop = new System.Windows.Forms.SplitContainer();
            this.pnlTimelineCard = new System.Windows.Forms.Panel();
            this.pnlPieCard = new System.Windows.Forms.Panel();
            this.pnlGridCard = new System.Windows.Forms.Panel();
            this.chartTimeline = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.pieChartLive = new LiveChartsCore.SkiaSharpView.WinForms.PieChart();
            this.dgvEfficiency = new System.Windows.Forms.DataGridView();
            this.panelSummary = new System.Windows.Forms.Panel();
            this.lblSummary = new MaterialSkin.Controls.MaterialLabel();    // MaterialLabel yapıldı
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitTop)).BeginInit();
            this.splitTop.Panel1.SuspendLayout();
            this.splitTop.Panel2.SuspendLayout();
            this.splitTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartTimeline)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEfficiency)).BeginInit();
            this.panelFilter.SuspendLayout();
            this.panelSummary.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelFilter
            // 
            this.panelFilter.BackColor = System.Drawing.Color.Transparent; // Dark Mode parlamasını engellemek için şeffaf yapıldı
            this.panelFilter.Controls.Add(this.lblStart);
            this.panelFilter.Controls.Add(this.dtpStart);
            this.panelFilter.Controls.Add(this.lblEnd);
            this.panelFilter.Controls.Add(this.dtpEnd);
            this.panelFilter.Controls.Add(this.lblMachine);
            this.panelFilter.Controls.Add(this.cmbMachine);
            this.panelFilter.Controls.Add(this.lblSubType);
            this.panelFilter.Controls.Add(this.cmbSubType);
            this.panelFilter.Controls.Add(this.btnSearch);
            this.panelFilter.Controls.Add(this.lblQuickFilter);
            this.panelFilter.Controls.Add(this.txtQuickFilter);
            this.panelFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFilter.Height = 68;
            this.panelFilter.Location = new System.Drawing.Point(0, 0);
            this.panelFilter.Name = "panelFilter";
            this.panelFilter.Padding = new System.Windows.Forms.Padding(12);
            this.panelFilter.Size = new System.Drawing.Size(1600, 68);
            this.panelFilter.TabIndex = 0;
            // 
            // lblStart
            // 
            this.lblStart.AutoSize = true;
            this.lblStart.Depth = 0;
            this.lblStart.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblStart.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.lblStart.Location = new System.Drawing.Point(12, 6);
            this.lblStart.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(57, 17);
            this.lblStart.TabIndex = 0;
            this.lblStart.Text = "Start Date";
            // 
            // dtpStart
            // 
            this.dtpStart.CustomFormat = "yyyy-MM-dd HH:mm";
            this.dtpStart.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStart.Location = new System.Drawing.Point(12, 26);
            this.dtpStart.Name = "dtpStart";
            this.dtpStart.Size = new System.Drawing.Size(165, 25);
            this.dtpStart.TabIndex = 1;
            // 
            // lblEnd
            // 
            this.lblEnd.AutoSize = true;
            this.lblEnd.Depth = 0;
            this.lblEnd.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblEnd.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.lblEnd.Location = new System.Drawing.Point(190, 6);
            this.lblEnd.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblEnd.Name = "lblEnd";
            this.lblEnd.Size = new System.Drawing.Size(53, 17);
            this.lblEnd.TabIndex = 2;
            this.lblEnd.Text = "End Date";
            // 
            // dtpEnd
            // 
            this.dtpEnd.CustomFormat = "yyyy-MM-dd HH:mm";
            this.dtpEnd.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEnd.Location = new System.Drawing.Point(190, 26);
            this.dtpEnd.Name = "dtpEnd";
            this.dtpEnd.Size = new System.Drawing.Size(165, 25);
            this.dtpEnd.TabIndex = 3;
            // 
            // lblMachine
            // 
            this.lblMachine.AutoSize = true;
            this.lblMachine.Depth = 0;
            this.lblMachine.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblMachine.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.lblMachine.Location = new System.Drawing.Point(370, 6);
            this.lblMachine.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblMachine.Name = "lblMachine";
            this.lblMachine.Size = new System.Drawing.Size(50, 17);
            this.lblMachine.TabIndex = 4;
            this.lblMachine.Text = "Machine";
            // 
            // cmbMachine
            // 
            this.cmbMachine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMachine.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbMachine.Location = new System.Drawing.Point(370, 26);
            this.cmbMachine.Name = "cmbMachine";
            this.cmbMachine.Size = new System.Drawing.Size(180, 25);
            this.cmbMachine.TabIndex = 5;
            // 
            // lblSubType
            // 
            this.lblSubType.AutoSize = true;
            this.lblSubType.Depth = 0;
            this.lblSubType.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblSubType.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.lblSubType.Location = new System.Drawing.Point(565, 6);
            this.lblSubType.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblSubType.Name = "lblSubType";
            this.lblSubType.Size = new System.Drawing.Size(81, 17);
            this.lblSubType.TabIndex = 6;
            this.lblSubType.Text = "Machine Type";
            // 
            // cmbSubType
            // 
            this.cmbSubType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSubType.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbSubType.Location = new System.Drawing.Point(565, 26);
            this.cmbSubType.Name = "cmbSubType";
            this.cmbSubType.Size = new System.Drawing.Size(180, 25);
            this.cmbSubType.TabIndex = 7;
            // 
            // btnSearch
            // 
            this.btnSearch.AutoSize = false;
            this.btnSearch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSearch.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSearch.Depth = 0;
            this.btnSearch.HighEmphasis = true;
            this.btnSearch.Icon = null;
            this.btnSearch.Location = new System.Drawing.Point(760, 21); // Dikey basamak yerleşimi ortalandı
            this.btnSearch.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSearch.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSearch.Size = new System.Drawing.Size(145, 34);
            this.btnSearch.TabIndex = 8;
            this.btnSearch.Text = "Fetch Report";
            this.btnSearch.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSearch.UseAccentColor = true;
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // lblQuickFilter
            // 
            this.lblQuickFilter.AutoSize = true;
            this.lblQuickFilter.Depth = 0;
            this.lblQuickFilter.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblQuickFilter.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.lblQuickFilter.Location = new System.Drawing.Point(925, 6);
            this.lblQuickFilter.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblQuickFilter.Name = "lblQuickFilter";
            this.lblQuickFilter.Size = new System.Drawing.Size(76, 17);
            this.lblQuickFilter.TabIndex = 9;
            this.lblQuickFilter.Text = "Quick Search";
            // 
            // txtQuickFilter
            // 
            this.txtQuickFilter.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtQuickFilter.Location = new System.Drawing.Point(925, 26);
            this.txtQuickFilter.Name = "txtQuickFilter";
            this.txtQuickFilter.PlaceholderText = "Machine / Status / Alarm...";
            this.txtQuickFilter.Size = new System.Drawing.Size(210, 25);
            this.txtQuickFilter.TabIndex = 10;
            this.txtQuickFilter.TextChanged += new System.EventHandler(this.txtQuickFilter_TextChanged);
            // 
            // panelKpiCards
            // 
            this.panelKpiCards.AutoScroll = true;
            this.panelKpiCards.BackColor = System.Drawing.Color.Transparent;
            this.panelKpiCards.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelKpiCards.Height = 96;
            this.panelKpiCards.Location = new System.Drawing.Point(0, 68);
            this.panelKpiCards.Name = "panelKpiCards";
            this.panelKpiCards.Padding = new System.Windows.Forms.Padding(10, 10, 10, 6);
            this.panelKpiCards.Size = new System.Drawing.Size(1600, 96);
            this.panelKpiCards.TabIndex = 1;
            this.panelKpiCards.WrapContents = false;
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 164);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.splitTop);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.pnlGridCard);
            this.splitMain.Panel2.Padding = new System.Windows.Forms.Padding(10, 5, 10, 10);
            this.splitMain.Size = new System.Drawing.Size(1600, 694);
            this.splitMain.SplitterDistance = 430;
            this.splitMain.TabIndex = 2;
            // 
            // splitTop
            // 
            this.splitTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitTop.Location = new System.Drawing.Point(0, 0);
            this.splitTop.Name = "splitTop";
            // 
            // splitTop.Panel1
            // 
            this.splitTop.Panel1.Controls.Add(this.pnlTimelineCard);
            this.splitTop.Panel1.Padding = new System.Windows.Forms.Padding(10, 10, 5, 5);
            // 
            // splitTop.Panel2
            // 
            this.splitTop.Panel2.Controls.Add(this.pnlPieCard);
            this.splitTop.Panel2.Padding = new System.Windows.Forms.Padding(5, 10, 10, 5);
            this.splitTop.Size = new System.Drawing.Size(1600, 430);
            this.splitTop.SplitterDistance = 860;
            this.splitTop.TabIndex = 0;
            // 
            // pnlTimelineCard
            // 
            this.pnlTimelineCard.BackColor = System.Drawing.Color.Transparent;
            this.pnlTimelineCard.Controls.Add(this.chartTimeline);
            this.pnlTimelineCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTimelineCard.Location = new System.Drawing.Point(10, 10);
            this.pnlTimelineCard.Name = "pnlTimelineCard";
            this.pnlTimelineCard.Padding = new System.Windows.Forms.Padding(12);
            this.pnlTimelineCard.Size = new System.Drawing.Size(845, 415);
            this.pnlTimelineCard.TabIndex = 0;
            // 
            // pnlPieCard
            // 
            this.pnlPieCard.BackColor = System.Drawing.Color.Transparent;
            this.pnlPieCard.Controls.Add(this.pieChartLive);
            this.pnlPieCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPieCard.Location = new System.Drawing.Point(5, 10);
            this.pnlPieCard.Name = "pnlPieCard";
            this.pnlPieCard.Padding = new System.Windows.Forms.Padding(12);
            this.pnlPieCard.Size = new System.Drawing.Size(726, 415);
            this.pnlPieCard.TabIndex = 0;
            // 
            // pnlGridCard
            // 
            this.pnlGridCard.BackColor = System.Drawing.Color.Transparent;
            this.pnlGridCard.Controls.Add(this.dgvEfficiency);
            this.pnlGridCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGridCard.Location = new System.Drawing.Point(10, 5);
            this.pnlGridCard.Name = "pnlGridCard";
            this.pnlGridCard.Padding = new System.Windows.Forms.Padding(12);
            this.pnlGridCard.Size = new System.Drawing.Size(1580, 245);
            this.pnlGridCard.TabIndex = 0;
            // 
            // chartTimeline
            // 
            chartArea1.AxisX.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
            chartArea1.AxisX.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            chartArea1.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
            chartArea1.AxisY.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
            chartArea1.AxisY.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            chartArea1.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
            chartArea1.AxisY.ScaleView.Zoomable = true;
            chartArea1.AxisY.ScrollBar.Enabled = true;
            chartArea1.BackColor = System.Drawing.Color.Transparent; // Kod arkası ile tam uyum için transparan yapıldı
            chartArea1.CursorY.IsUserEnabled = true;
            chartArea1.CursorY.IsUserSelectionEnabled = true;
            chartArea1.Name = "ChartArea1";
            this.chartTimeline.ChartAreas.Add(chartArea1);
            this.chartTimeline.BackColor = System.Drawing.Color.Transparent;
            this.chartTimeline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartTimeline.Location = new System.Drawing.Point(12, 12);
            this.chartTimeline.Name = "chartTimeline";
            this.chartTimeline.Size = new System.Drawing.Size(821, 391);
            this.chartTimeline.TabIndex = 0;
            // 
            // pieChartLive
            // 
            this.pieChartLive.BackColor = System.Drawing.Color.Transparent;
            this.pieChartLive.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pieChartLive.Location = new System.Drawing.Point(12, 12);
            this.pieChartLive.Name = "pieChartLive";
            this.pieChartLive.Size = new System.Drawing.Size(702, 391);
            this.pieChartLive.TabIndex = 0;
            // 
            // dgvEfficiency
            // 
            this.dgvEfficiency.AllowUserToAddRows = false;
            this.dgvEfficiency.AllowUserToDeleteRows = false;
            this.dgvEfficiency.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dgvEfficiency.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvEfficiency.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvEfficiency.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvEfficiency.EnableHeadersVisualStyles = false;
            this.dgvEfficiency.Location = new System.Drawing.Point(12, 12);
            this.dgvEfficiency.Name = "dgvEfficiency";
            this.dgvEfficiency.ReadOnly = true;
            this.dgvEfficiency.RowHeadersVisible = false;
            this.dgvEfficiency.RowTemplate.Height = 36;
            this.dgvEfficiency.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvEfficiency.Size = new System.Drawing.Size(1556, 221);
            this.dgvEfficiency.TabIndex = 0;
            this.dgvEfficiency.SelectionChanged += new System.EventHandler(this.dgvEfficiency_SelectionChanged);
            // 
            // panelSummary
            // 
            this.panelSummary.BackColor = System.Drawing.Color.Transparent; // Koyu mod bütünlüğü için şeffaflaştırıldı
            this.panelSummary.Controls.Add(this.lblSummary);
            this.panelSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelSummary.Height = 42;
            this.panelSummary.Location = new System.Drawing.Point(0, 858);
            this.panelSummary.Name = "panelSummary";
            this.panelSummary.Size = new System.Drawing.Size(1600, 42);
            this.panelSummary.TabIndex = 3;
            // 
            // lblSummary
            // 
            this.lblSummary.Depth = 0;
            this.lblSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSummary.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblSummary.FontType = MaterialSkin.MaterialSkinManager.fontType.Body1; // Alt özet bilgi tipografisi düzenlendi
            this.lblSummary.Location = new System.Drawing.Point(0, 0);
            this.lblSummary.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblSummary.Size = new System.Drawing.Size(1600, 42);
            this.lblSummary.TabIndex = 0;
            this.lblSummary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // EfficiencyReport_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent; // Kontrol bütünüyle şeffaf yapıldı
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.panelKpiCards);
            this.Controls.Add(this.panelFilter);
            this.Controls.Add(this.panelSummary);
            this.Name = "EfficiencyReport_Control";
            this.Size = new System.Drawing.Size(1600, 900);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.splitTop.Panel1.ResumeLayout(false);
            this.splitTop.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitTop)).EndInit();
            this.splitTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartTimeline)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEfficiency)).EndInit();
            this.panelFilter.ResumeLayout(false);
            this.panelFilter.PerformLayout();
            this.panelSummary.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelFilter;
        private MaterialSkin.Controls.MaterialLabel lblStart;
        private MaterialSkin.Controls.MaterialLabel lblEnd;
        private MaterialSkin.Controls.MaterialLabel lblMachine;
        private MaterialSkin.Controls.MaterialLabel lblSubType;
        private MaterialSkin.Controls.MaterialLabel lblQuickFilter;
        private System.Windows.Forms.DateTimePicker dtpStart;
        private System.Windows.Forms.DateTimePicker dtpEnd;
        private System.Windows.Forms.ComboBox cmbMachine;
        private System.Windows.Forms.ComboBox cmbSubType;
        private MaterialSkin.Controls.MaterialButton btnSearch;
        private System.Windows.Forms.TextBox txtQuickFilter;
        private System.Windows.Forms.FlowLayoutPanel panelKpiCards;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.SplitContainer splitTop;
        private System.Windows.Forms.Panel pnlTimelineCard;
        private System.Windows.Forms.Panel pnlPieCard;
        private System.Windows.Forms.Panel pnlGridCard;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartTimeline;
        private LiveChartsCore.SkiaSharpView.WinForms.PieChart pieChartLive;
        private System.Windows.Forms.DataGridView dgvEfficiency;
        private System.Windows.Forms.Panel panelSummary;
        private MaterialSkin.Controls.MaterialLabel lblSummary;
    }
}