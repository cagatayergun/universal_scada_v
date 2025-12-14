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
            gbHourlyConsumptionSteam = new GroupBox();
            formsPlotHourlySteam = new ScottPlot.WinForms.FormsPlot();
            gbHourlyConsumptionWater = new GroupBox();
            formsPlotHourlyWater = new ScottPlot.WinForms.FormsPlot();
            gbHourlyConsumption = new GroupBox();
            formsPlotHourly = new ScottPlot.WinForms.FormsPlot();
            gbHourlyOee = new GroupBox();
            formsPlotHourlyOee = new ScottPlot.WinForms.FormsPlot();
            pnlHeader.SuspendLayout();
            pnlMain.SuspendLayout();
            tlpMainLayout.SuspendLayout();
            pnlSidebar.SuspendLayout();
            tlpSidebarLayout.SuspendLayout();
            gbTopAlarms.SuspendLayout();
            gbHourlyConsumptionSteam.SuspendLayout();
            gbHourlyConsumptionWater.SuspendLayout();
            gbHourlyConsumption.SuspendLayout();
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
            tlpMainLayout.Location = new Point(0, 0);
            tlpMainLayout.Margin = new Padding(3, 2, 3, 2);
            tlpMainLayout.Name = "tlpMainLayout";
            tlpMainLayout.RowCount = 1;
            tlpMainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMainLayout.Size = new Size(1032, 501);
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
            flpMachineGroups.Size = new Size(720, 497);
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
            pnlSidebar.Size = new Size(300, 497);
            pnlSidebar.TabIndex = 2;
            // 
            // tlpSidebarLayout
            // 
            tlpSidebarLayout.ColumnCount = 1;
            tlpSidebarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpSidebarLayout.Controls.Add(gbTopAlarms, 0, 3);
            tlpSidebarLayout.Controls.Add(gbHourlyConsumptionSteam, 0, 2);
            tlpSidebarLayout.Controls.Add(gbHourlyConsumptionWater, 0, 1);
            tlpSidebarLayout.Controls.Add(gbHourlyConsumption, 0, 0);
            tlpSidebarLayout.Controls.Add(gbHourlyOee, 0, 4);
            tlpSidebarLayout.Dock = DockStyle.Fill;
            tlpSidebarLayout.Location = new Point(0, 0);
            tlpSidebarLayout.Name = "tlpSidebarLayout";
            tlpSidebarLayout.RowCount = 5;
            tlpSidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tlpSidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tlpSidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tlpSidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tlpSidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tlpSidebarLayout.Size = new Size(300, 497);
            tlpSidebarLayout.TabIndex = 0;
            // 
            // gbTopAlarms
            // 
            gbTopAlarms.Controls.Add(formsPlotTopAlarms);
            gbTopAlarms.Dock = DockStyle.Fill;
            gbTopAlarms.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            gbTopAlarms.Location = new Point(3, 299);
            gbTopAlarms.Margin = new Padding(3, 2, 3, 2);
            gbTopAlarms.Name = "gbTopAlarms";
            gbTopAlarms.Padding = new Padding(3, 2, 3, 2);
            gbTopAlarms.Size = new Size(294, 95);
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
            formsPlotTopAlarms.Size = new Size(288, 73);
            formsPlotTopAlarms.TabIndex = 0;
            // 
            // gbHourlyConsumptionSteam
            // 
            gbHourlyConsumptionSteam.Controls.Add(formsPlotHourlySteam);
            gbHourlyConsumptionSteam.Dock = DockStyle.Fill;
            gbHourlyConsumptionSteam.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            gbHourlyConsumptionSteam.Location = new Point(3, 200);
            gbHourlyConsumptionSteam.Margin = new Padding(3, 2, 3, 2);
            gbHourlyConsumptionSteam.Name = "gbHourlyConsumptionSteam";
            gbHourlyConsumptionSteam.Padding = new Padding(3, 2, 3, 2);
            gbHourlyConsumptionSteam.Size = new Size(294, 95);
            gbHourlyConsumptionSteam.TabIndex = 2;
            gbHourlyConsumptionSteam.TabStop = false;
            gbHourlyConsumptionSteam.Text = "Saatlik Buhar Tüketimi";
            // 
            // formsPlotHourlySteam
            // 
            formsPlotHourlySteam.DisplayScale = 1F;
            formsPlotHourlySteam.Dock = DockStyle.Fill;
            formsPlotHourlySteam.Location = new Point(3, 20);
            formsPlotHourlySteam.Margin = new Padding(3, 2, 3, 2);
            formsPlotHourlySteam.Name = "formsPlotHourlySteam";
            formsPlotHourlySteam.Size = new Size(288, 73);
            formsPlotHourlySteam.TabIndex = 0;
            // 
            // gbHourlyConsumptionWater
            // 
            gbHourlyConsumptionWater.Controls.Add(formsPlotHourlyWater);
            gbHourlyConsumptionWater.Dock = DockStyle.Fill;
            gbHourlyConsumptionWater.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            gbHourlyConsumptionWater.Location = new Point(3, 101);
            gbHourlyConsumptionWater.Margin = new Padding(3, 2, 3, 2);
            gbHourlyConsumptionWater.Name = "gbHourlyConsumptionWater";
            gbHourlyConsumptionWater.Padding = new Padding(3, 2, 3, 2);
            gbHourlyConsumptionWater.Size = new Size(294, 95);
            gbHourlyConsumptionWater.TabIndex = 1;
            gbHourlyConsumptionWater.TabStop = false;
            gbHourlyConsumptionWater.Text = "Saatlik Su Tüketimi";
            // 
            // formsPlotHourlyWater
            // 
            formsPlotHourlyWater.DisplayScale = 1F;
            formsPlotHourlyWater.Dock = DockStyle.Fill;
            formsPlotHourlyWater.Location = new Point(3, 20);
            formsPlotHourlyWater.Margin = new Padding(3, 2, 3, 2);
            formsPlotHourlyWater.Name = "formsPlotHourlyWater";
            formsPlotHourlyWater.Size = new Size(288, 73);
            formsPlotHourlyWater.TabIndex = 0;
            // 
            // gbHourlyConsumption
            // 
            gbHourlyConsumption.Controls.Add(formsPlotHourly);
            gbHourlyConsumption.Dock = DockStyle.Fill;
            gbHourlyConsumption.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            gbHourlyConsumption.Location = new Point(3, 2);
            gbHourlyConsumption.Margin = new Padding(3, 2, 3, 2);
            gbHourlyConsumption.Name = "gbHourlyConsumption";
            gbHourlyConsumption.Padding = new Padding(3, 2, 3, 2);
            gbHourlyConsumption.Size = new Size(294, 95);
            gbHourlyConsumption.TabIndex = 0;
            gbHourlyConsumption.TabStop = false;
            gbHourlyConsumption.Text = "Saatlik Elektrik Tüketimi";
            // 
            // formsPlotHourly
            // 
            formsPlotHourly.DisplayScale = 1F;
            formsPlotHourly.Dock = DockStyle.Fill;
            formsPlotHourly.Location = new Point(3, 20);
            formsPlotHourly.Margin = new Padding(3, 2, 3, 2);
            formsPlotHourly.Name = "formsPlotHourly";
            formsPlotHourly.Size = new Size(288, 73);
            formsPlotHourly.TabIndex = 0;
            // 
            // gbHourlyOee
            // 
            gbHourlyOee.Controls.Add(formsPlotHourlyOee);
            gbHourlyOee.Dock = DockStyle.Fill;
            gbHourlyOee.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            gbHourlyOee.Location = new Point(3, 398);
            gbHourlyOee.Margin = new Padding(3, 2, 3, 2);
            gbHourlyOee.Name = "gbHourlyOee";
            gbHourlyOee.Padding = new Padding(3, 2, 3, 2);
            gbHourlyOee.Size = new Size(294, 97);
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
            formsPlotHourlyOee.Size = new Size(288, 75);
            formsPlotHourlyOee.TabIndex = 0;
            formsPlotHourlyOee.Visible = false;
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
            gbHourlyConsumptionSteam.ResumeLayout(false);
            gbHourlyConsumptionWater.ResumeLayout(false);
            gbHourlyConsumption.ResumeLayout(false);
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
        private System.Windows.Forms.GroupBox gbHourlyConsumption;
        private ScottPlot.WinForms.FormsPlot formsPlotHourly;
        private System.Windows.Forms.GroupBox gbHourlyConsumptionWater;
        private ScottPlot.WinForms.FormsPlot formsPlotHourlyWater;
        private System.Windows.Forms.GroupBox gbHourlyConsumptionSteam;
        private ScottPlot.WinForms.FormsPlot formsPlotHourlySteam;
        private System.Windows.Forms.TableLayoutPanel tlpSidebarLayout;
        private System.Windows.Forms.GroupBox gbHourlyOee;
        private ScottPlot.WinForms.FormsPlot formsPlotHourlyOee;
    }
}