namespace TekstilScada.UI.Views
{
    partial class GenelBakis_Control
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
            pnlHeader = new Panel();
            flpTopKpis = new FlowLayoutPanel();
            pnlMain = new Panel();
            tlpMainLayout = new TableLayoutPanel();
            flpMachineGroups = new FlowLayoutPanel();
            pnlSidebar = new Panel();
            tlpSidebarLayout = new TableLayoutPanel();
            gbTopAlarms = new GroupBox();
            formsPlotTopAlarms = new ScottPlot.WinForms.FormsPlot();
            gbHourlyOee = new GroupBox();
            formsPlotHourlyOee = new ScottPlot.WinForms.FormsPlot();
            flpUtilityStrip = new FlowLayoutPanel();
            pnlHeader.SuspendLayout();
            pnlMain.SuspendLayout();
            tlpMainLayout.SuspendLayout();
            pnlSidebar.SuspendLayout();
            tlpSidebarLayout.SuspendLayout();
            gbTopAlarms.SuspendLayout();
            gbHourlyOee.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(flpTopKpis);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(9, 8);
            pnlHeader.Margin = new Padding(3, 2, 3, 2);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(4);
            pnlHeader.Size = new Size(1032, 83);
            pnlHeader.TabIndex = 0;
            // 
            // flpTopKpis
            // 
            flpTopKpis.BackColor = Color.WhiteSmoke;
            flpTopKpis.Dock = DockStyle.Fill;
            flpTopKpis.Location = new Point(4, 4);
            flpTopKpis.Margin = new Padding(3, 2, 3, 2);
            flpTopKpis.Name = "flpTopKpis";
            flpTopKpis.Size = new Size(1024, 75);
            flpTopKpis.TabIndex = 0;
            // 
            // pnlMain
            // 
            pnlMain.Controls.Add(tlpMainLayout);
            pnlMain.Controls.Add(flpUtilityStrip);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(9, 91);
            pnlMain.Margin = new Padding(3, 2, 3, 2);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(1032, 501);
            pnlMain.TabIndex = 2;
            // 
            // tlpMainLayout
            // 
            tlpMainLayout.ColumnCount = 2;
            tlpMainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 306F));
            tlpMainLayout.Controls.Add(flpMachineGroups, 0, 0);
            tlpMainLayout.Controls.Add(pnlSidebar, 1, 0);
            tlpMainLayout.Dock = DockStyle.Fill;
            tlpMainLayout.Location = new Point(0, 135);
            tlpMainLayout.Margin = new Padding(3, 2, 3, 2);
            tlpMainLayout.Name = "tlpMainLayout";
            tlpMainLayout.RowCount = 1;
            tlpMainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMainLayout.Size = new Size(1032, 366);
            tlpMainLayout.TabIndex = 0;
            // 
            // flpMachineGroups
            // 
            flpMachineGroups.AutoScroll = true;
            flpMachineGroups.AutoSize = true;
            flpMachineGroups.BackColor = SystemColors.Control;
            flpMachineGroups.Dock = DockStyle.Fill;
            flpMachineGroups.FlowDirection = FlowDirection.TopDown;
            flpMachineGroups.Location = new Point(3, 2);
            flpMachineGroups.Margin = new Padding(3, 2, 3, 2);
            flpMachineGroups.Name = "flpMachineGroups";
            flpMachineGroups.Padding = new Padding(4);
            flpMachineGroups.Size = new Size(720, 362);
            flpMachineGroups.TabIndex = 1;
            flpMachineGroups.WrapContents = false;
            // 
            // pnlSidebar
            // 
            pnlSidebar.Controls.Add(tlpSidebarLayout);
            pnlSidebar.Dock = DockStyle.Fill;
            pnlSidebar.Location = new Point(729, 2);
            pnlSidebar.Margin = new Padding(3, 2, 3, 2);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(300, 362);
            pnlSidebar.TabIndex = 2;
            // 
            // tlpSidebarLayout
            // 
            tlpSidebarLayout.ColumnCount = 1;
            tlpSidebarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpSidebarLayout.Controls.Add(gbTopAlarms, 0, 3);
            tlpSidebarLayout.Controls.Add(gbHourlyOee, 0, 4);
            tlpSidebarLayout.Dock = DockStyle.Fill;
            tlpSidebarLayout.Location = new Point(0, 0);
            tlpSidebarLayout.Name = "tlpSidebarLayout";
            tlpSidebarLayout.RowCount = 5;
            tlpSidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 2.20994473F));
            tlpSidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 3.5911603F));
            tlpSidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 2.20994473F));
            tlpSidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 91.71271F));
            tlpSidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 0F));
            tlpSidebarLayout.Size = new Size(300, 362);
            tlpSidebarLayout.TabIndex = 0;
            // 
            // gbTopAlarms
            // 
            gbTopAlarms.Controls.Add(formsPlotTopAlarms);
            gbTopAlarms.Dock = DockStyle.Fill;
            gbTopAlarms.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            gbTopAlarms.Location = new Point(3, 31);
            gbTopAlarms.Margin = new Padding(3, 2, 3, 2);
            gbTopAlarms.Name = "gbTopAlarms";
            gbTopAlarms.Padding = new Padding(3, 2, 3, 2);
            gbTopAlarms.Size = new Size(294, 328);
            gbTopAlarms.TabIndex = 3;
            gbTopAlarms.TabStop = false;
            gbTopAlarms.Text = "Son 24 Saatin Popüler Alarmları";
            // 
            // formsPlotTopAlarms
            // 
            formsPlotTopAlarms.DisplayScale = 1F;
            formsPlotTopAlarms.Dock = DockStyle.Fill;
            formsPlotTopAlarms.Location = new Point(3, 20);
            formsPlotTopAlarms.Margin = new Padding(3, 2, 3, 2);
            formsPlotTopAlarms.Name = "formsPlotTopAlarms";
            formsPlotTopAlarms.Size = new Size(288, 306);
            formsPlotTopAlarms.TabIndex = 0;
            // 
            // gbHourlyOee
            // 
            gbHourlyOee.Controls.Add(formsPlotHourlyOee);
            gbHourlyOee.Dock = DockStyle.Fill;
            gbHourlyOee.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            gbHourlyOee.Location = new Point(3, 363);
            gbHourlyOee.Margin = new Padding(3, 2, 3, 2);
            gbHourlyOee.Name = "gbHourlyOee";
            gbHourlyOee.Padding = new Padding(3, 2, 3, 2);
            gbHourlyOee.Size = new Size(294, 1);
            gbHourlyOee.TabIndex = 4;
            gbHourlyOee.TabStop = false;
            gbHourlyOee.Text = "Son 24 Saatin OEE Ortalaması";
            gbHourlyOee.Visible = false;
            // 
            // formsPlotHourlyOee
            // 
            formsPlotHourlyOee.DisplayScale = 1F;
            formsPlotHourlyOee.Dock = DockStyle.Fill;
            formsPlotHourlyOee.Location = new Point(3, 20);
            formsPlotHourlyOee.Margin = new Padding(3, 2, 3, 2);
            formsPlotHourlyOee.Name = "formsPlotHourlyOee";
            formsPlotHourlyOee.Size = new Size(288, 0);
            formsPlotHourlyOee.TabIndex = 0;
            formsPlotHourlyOee.Visible = false;
            // 
            // flpUtilityStrip
            // 
            flpUtilityStrip.AutoScroll = true;
            flpUtilityStrip.BackColor = Color.WhiteSmoke;
            flpUtilityStrip.Dock = DockStyle.Top;
            flpUtilityStrip.Location = new Point(0, 0);
            flpUtilityStrip.Name = "flpUtilityStrip";
            flpUtilityStrip.Padding = new Padding(5);
            flpUtilityStrip.Size = new Size(1032, 135);
            flpUtilityStrip.TabIndex = 3;
            flpUtilityStrip.WrapContents = false;
            // 
            // GenelBakis_Control
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLight;
            Controls.Add(pnlMain);
            Controls.Add(pnlHeader);
            Margin = new Padding(9, 8, 9, 8);
            Name = "GenelBakis_Control";
            Padding = new Padding(9, 8, 9, 8);
            Size = new Size(1050, 600);
            Load += GenelBakis_Control_Load;
            pnlHeader.ResumeLayout(false);
            pnlMain.ResumeLayout(false);
            tlpMainLayout.ResumeLayout(false);
            tlpMainLayout.PerformLayout();
            pnlSidebar.ResumeLayout(false);
            tlpSidebarLayout.ResumeLayout(false);
            gbTopAlarms.ResumeLayout(false);
            gbHourlyOee.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.FlowLayoutPanel flpTopKpis;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.TableLayoutPanel tlpMainLayout;
        private System.Windows.Forms.FlowLayoutPanel flpMachineGroups;
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.GroupBox gbTopAlarms;
        private ScottPlot.WinForms.FormsPlot formsPlotTopAlarms;
        private System.Windows.Forms.TableLayoutPanel tlpSidebarLayout;
        private System.Windows.Forms.GroupBox gbHourlyOee;
        private ScottPlot.WinForms.FormsPlot formsPlotHourlyOee;
        private System.Windows.Forms.FlowLayoutPanel flpUtilityStrip;
    }
}