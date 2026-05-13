using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace TekstilScada.UIViews
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
            this.panelFilter = new Panel();

            this.lblStart = new Label();
            this.lblEnd = new Label();
            this.lblMachine = new Label();
            this.lblSubType = new Label();
            this.lblQuickFilter = new Label();

            this.dtpStart = new DateTimePicker();
            this.dtpEnd = new DateTimePicker();

            this.cmbMachine = new ComboBox();
            this.cmbSubType = new ComboBox();

            this.btnSearch = new Button();

            this.txtQuickFilter = new TextBox();

            this.panelKpiCards = new FlowLayoutPanel();

            this.splitMain = new SplitContainer();
            this.splitTop = new SplitContainer();

            this.pnlTimelineCard = new Panel();
            this.pnlPieCard = new Panel();
            this.pnlGridCard = new Panel();

            this.chartTimeline = new Chart();
            this.pieChartLive = new PieChart();
            this.dgvEfficiency = new DataGridView();

            this.panelSummary = new Panel();
            this.lblSummary = new Label();

            ChartArea chartArea1 = new ChartArea();

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

            this.SuspendLayout();

            // =====================================================
            // CONTROL
            // =====================================================

            this.BackColor = Color.FromArgb(241, 245, 249);

            this.Name = "EfficiencyReport_Control";

            this.Size = new Size(1600, 900);

            // =====================================================
            // panelFilter
            // =====================================================

            this.panelFilter.Dock = DockStyle.Top;

            this.panelFilter.Height = 68;

            this.panelFilter.Padding = new Padding(12);

            this.panelFilter.BackColor = Color.White;

            // =====================================================
            // lblStart
            // =====================================================

            this.lblStart.Text = "Start Date";

            this.lblStart.Font =
                new Font("Segoe UI Semibold", 9F);

            this.lblStart.ForeColor =
                Color.FromArgb(71, 85, 105);

            this.lblStart.Location = new Point(12, 6);

            this.lblStart.AutoSize = true;

            // =====================================================
            // dtpStart
            // =====================================================

            this.dtpStart.CustomFormat = "yyyy-MM-dd HH:mm";

            this.dtpStart.Format =
                DateTimePickerFormat.Custom;

            this.dtpStart.Font =
                new Font("Segoe UI", 10F);

            this.dtpStart.Size =
                new Size(165, 28);

            this.dtpStart.Location =
                new Point(12, 26);

            // =====================================================
            // lblEnd
            // =====================================================

            this.lblEnd.Text = "End Date";

            this.lblEnd.Font =
                new Font("Segoe UI Semibold", 9F);

            this.lblEnd.ForeColor =
                Color.FromArgb(71, 85, 105);

            this.lblEnd.Location =
                new Point(190, 6);

            this.lblEnd.AutoSize = true;

            // =====================================================
            // dtpEnd
            // =====================================================

            this.dtpEnd.CustomFormat = "yyyy-MM-dd HH:mm";

            this.dtpEnd.Format =
                DateTimePickerFormat.Custom;

            this.dtpEnd.Font =
                new Font("Segoe UI", 10F);

            this.dtpEnd.Size =
                new Size(165, 28);

            this.dtpEnd.Location =
                new Point(190, 26);

            // =====================================================
            // lblMachine
            // =====================================================

            this.lblMachine.Text = "Machine";

            this.lblMachine.Font =
                new Font("Segoe UI Semibold", 9F);

            this.lblMachine.ForeColor =
                Color.FromArgb(71, 85, 105);

            this.lblMachine.Location =
                new Point(370, 6);

            this.lblMachine.AutoSize = true;

            // =====================================================
            // cmbMachine
            // =====================================================

            this.cmbMachine.DropDownStyle =
                ComboBoxStyle.DropDownList;

            this.cmbMachine.Font =
                new Font("Segoe UI", 10F);

            this.cmbMachine.Size =
                new Size(180, 28);

            this.cmbMachine.Location =
                new Point(370, 26);

            // =====================================================
            // lblSubType
            // =====================================================

            this.lblSubType.Text = "Machine Type";

            this.lblSubType.Font =
                new Font("Segoe UI Semibold", 9F);

            this.lblSubType.ForeColor =
                Color.FromArgb(71, 85, 105);

            this.lblSubType.Location =
                new Point(565, 6);

            this.lblSubType.AutoSize = true;

            // =====================================================
            // cmbSubType
            // =====================================================

            this.cmbSubType.DropDownStyle =
                ComboBoxStyle.DropDownList;

            this.cmbSubType.Font =
                new Font("Segoe UI", 10F);

            this.cmbSubType.Size =
                new Size(180, 28);

            this.cmbSubType.Location =
                new Point(565, 26);

            // =====================================================
            // btnSearch
            // =====================================================

            this.btnSearch.Text = "Fetch Report";

            this.btnSearch.Font =
                new Font("Segoe UI Semibold", 10F, FontStyle.Bold);

            this.btnSearch.BackColor =
                Color.FromArgb(37, 99, 235);

            this.btnSearch.ForeColor =
                Color.White;

            this.btnSearch.FlatStyle =
                FlatStyle.Flat;

            this.btnSearch.FlatAppearance.BorderSize = 0;

            this.btnSearch.Cursor = Cursors.Hand;

            this.btnSearch.Size =
                new Size(145, 36);

            this.btnSearch.Location =
                new Point(760, 22);

            this.btnSearch.Click +=
                new EventHandler(this.btnSearch_Click);

            // =====================================================
            // lblQuickFilter
            // =====================================================

            this.lblQuickFilter.Text = "Quick Search";

            this.lblQuickFilter.Font =
                new Font("Segoe UI Semibold", 9F);

            this.lblQuickFilter.ForeColor =
                Color.FromArgb(71, 85, 105);

            this.lblQuickFilter.Location =
                new Point(925, 6);

            this.lblQuickFilter.AutoSize = true;

            // =====================================================
            // txtQuickFilter
            // =====================================================

            this.txtQuickFilter.Font =
                new Font("Segoe UI", 10F);

            this.txtQuickFilter.Size =
                new Size(210, 28);

            this.txtQuickFilter.Location =
                new Point(925, 26);

            this.txtQuickFilter.PlaceholderText =
                "Machine / Status / Alarm...";

            this.txtQuickFilter.TextChanged +=
                new EventHandler(this.txtQuickFilter_TextChanged);

            // =====================================================
            // KPI PANEL
            // =====================================================

            this.panelKpiCards.Dock =
                DockStyle.Top;

            this.panelKpiCards.Height = 96;

            this.panelKpiCards.Padding =
                new Padding(10, 10, 10, 6);

            this.panelKpiCards.BackColor =
                Color.FromArgb(241, 245, 249);

            this.panelKpiCards.WrapContents = false;

            this.panelKpiCards.AutoScroll = true;

            // =====================================================
            // splitMain
            // =====================================================

            this.splitMain.Dock = DockStyle.Fill;

            this.splitMain.Orientation =
                Orientation.Horizontal;

            this.splitMain.SplitterDistance = 430;

            // =====================================================
            // splitTop
            // =====================================================

            this.splitTop.Dock = DockStyle.Fill;

            this.splitTop.SplitterDistance = 860;

            // =====================================================
            // pnlTimelineCard
            // =====================================================

            this.pnlTimelineCard.Dock =
                DockStyle.Fill;

            this.pnlTimelineCard.BackColor =
                Color.White;

            this.pnlTimelineCard.Padding =
                new Padding(12);

            // =====================================================
            // pnlPieCard
            // =====================================================

            this.pnlPieCard.Dock =
                DockStyle.Fill;

            this.pnlPieCard.BackColor =
                Color.White;

            this.pnlPieCard.Padding =
                new Padding(12);

            // =====================================================
            // pnlGridCard
            // =====================================================

            this.pnlGridCard.Dock =
                DockStyle.Fill;

            this.pnlGridCard.BackColor =
                Color.White;

            this.pnlGridCard.Padding =
                new Padding(12);

            // =====================================================
            // chartTimeline
            // =====================================================

            chartArea1.Name = "ChartArea1";

            chartArea1.BackColor = Color.White;

            chartArea1.AxisX.LabelStyle.Font =
                new Font("Segoe UI", 9F);

            chartArea1.AxisY.LabelStyle.Font =
                new Font("Segoe UI", 9F);

            chartArea1.AxisX.LineColor =
                Color.FromArgb(220, 220, 220);

            chartArea1.AxisY.LineColor =
                Color.FromArgb(220, 220, 220);

            chartArea1.AxisX.MajorGrid.LineColor =
                Color.FromArgb(240, 240, 240);

            chartArea1.AxisY.MajorGrid.LineColor =
                Color.FromArgb(240, 240, 240);

            chartArea1.AxisX.MajorGrid.LineDashStyle =
                ChartDashStyle.Dash;

            chartArea1.AxisY.MajorGrid.LineDashStyle =
                ChartDashStyle.Dash;

            chartArea1.CursorY.IsUserEnabled = true;

            chartArea1.CursorY.IsUserSelectionEnabled = true;

            chartArea1.AxisY.ScaleView.Zoomable = true;

            chartArea1.AxisY.ScrollBar.Enabled = true;

            this.chartTimeline.ChartAreas.Add(chartArea1);

            this.chartTimeline.Dock = DockStyle.Fill;

            this.chartTimeline.BackColor = Color.White;

            // =====================================================
            // pieChartLive
            // =====================================================

            this.pieChartLive.Dock =
                DockStyle.Fill;

            this.pieChartLive.BackColor =
                Color.White;

            // =====================================================
            // dgvEfficiency
            // =====================================================

            this.dgvEfficiency.Dock =
                DockStyle.Fill;

            this.dgvEfficiency.BackgroundColor =
                Color.White;

            this.dgvEfficiency.BorderStyle =
                BorderStyle.None;

            this.dgvEfficiency.AllowUserToAddRows = false;

            this.dgvEfficiency.AllowUserToDeleteRows = false;

            this.dgvEfficiency.RowHeadersVisible = false;

            this.dgvEfficiency.EnableHeadersVisualStyles = false;

            this.dgvEfficiency.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            this.dgvEfficiency.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.DisplayedCells;

            this.dgvEfficiency.RowTemplate.Height = 30;

            this.dgvEfficiency.GridColor =
                Color.FromArgb(230, 230, 230);

            // =====================================================
            // panelSummary
            // =====================================================

            this.panelSummary.Dock =
                DockStyle.Bottom;

            this.panelSummary.Height = 42;

            this.panelSummary.BackColor =
                Color.FromArgb(15, 23, 42);

            // =====================================================
            // lblSummary
            // =====================================================

            this.lblSummary.Dock = DockStyle.Fill;

            this.lblSummary.Font =
                new Font("Segoe UI Semibold", 10F);

            this.lblSummary.ForeColor =
                Color.White;

            this.lblSummary.Padding =
                new Padding(12, 0, 0, 0);

            this.lblSummary.TextAlign =
                ContentAlignment.MiddleLeft;

            // =====================================================
            // FILTER CONTROLS
            // =====================================================

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

            // =====================================================
            // CARDS
            // =====================================================

            this.pnlTimelineCard.Controls.Add(this.chartTimeline);

            this.pnlPieCard.Controls.Add(this.pieChartLive);

            this.pnlGridCard.Controls.Add(this.dgvEfficiency);

            // =====================================================
            // SUMMARY
            // =====================================================

            this.panelSummary.Controls.Add(this.lblSummary);

            // =====================================================
            // SPLITS
            // =====================================================

            this.splitTop.Panel1.Padding =
                new Padding(10, 10, 5, 5);

            this.splitTop.Panel2.Padding =
                new Padding(5, 10, 10, 5);

            this.splitTop.Panel1.Controls.Add(this.pnlTimelineCard);

            this.splitTop.Panel2.Controls.Add(this.pnlPieCard);

            this.splitMain.Panel1.Controls.Add(this.splitTop);

            this.splitMain.Panel2.Padding =
                new Padding(10, 5, 10, 10);

            this.splitMain.Panel2.Controls.Add(this.pnlGridCard);

            // =====================================================
            // MAIN CONTROL
            // =====================================================

            this.Controls.Add(this.splitMain);

            this.Controls.Add(this.panelKpiCards);

            this.Controls.Add(this.panelFilter);

            this.Controls.Add(this.panelSummary);

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

            this.ResumeLayout(false);
        }

        #endregion

        private Panel panelFilter;

        private Label lblStart;
        private Label lblEnd;
        private Label lblMachine;
        private Label lblSubType;
        private Label lblQuickFilter;

        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;

        private ComboBox cmbMachine;
        private ComboBox cmbSubType;

        private Button btnSearch;

        private TextBox txtQuickFilter;

        private FlowLayoutPanel panelKpiCards;

        private SplitContainer splitMain;
        private SplitContainer splitTop;

        private Panel pnlTimelineCard;
        private Panel pnlPieCard;
        private Panel pnlGridCard;

        private Chart chartTimeline;
        private PieChart pieChartLive;
        private DataGridView dgvEfficiency;

        private Panel panelSummary;
        private Label lblSummary;
    }
}