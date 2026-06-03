// UI/ProductionDetail_Form.Designer.cs
namespace Telemetry.UI
{
    partial class ProductionDetail_Form
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.txtTheoreticalDuration = new System.Windows.Forms.TextBox();
            this.labelTheoretical = new MaterialSkin.Controls.MaterialLabel();
            this.txtDurationDiff = new System.Windows.Forms.TextBox();
            this.labelDiff = new MaterialSkin.Controls.MaterialLabel();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnExportToExcel = new MaterialSkin.Controls.MaterialButton();
            this.btnClose = new MaterialSkin.Controls.MaterialButton();
            this.gbProductionInfo = new System.Windows.Forms.GroupBox();
            this.txtSteam = new System.Windows.Forms.TextBox();
            this.label11 = new MaterialSkin.Controls.MaterialLabel();
            this.txtWater = new System.Windows.Forms.TextBox();
            this.label10 = new MaterialSkin.Controls.MaterialLabel();
            this.txtElectricity = new System.Windows.Forms.TextBox();
            this.label9 = new MaterialSkin.Controls.MaterialLabel();
            this.pieChartControl = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.txtTotalDuration = new System.Windows.Forms.TextBox();
            this.label6 = new MaterialSkin.Controls.MaterialLabel();
            this.txtStopTime = new System.Windows.Forms.TextBox();
            this.label5 = new MaterialSkin.Controls.MaterialLabel();
            this.txtStartTime = new System.Windows.Forms.TextBox();
            this.label4 = new MaterialSkin.Controls.MaterialLabel();
            this.txtCustomerNo = new System.Windows.Forms.TextBox();
            this.label8 = new MaterialSkin.Controls.MaterialLabel();
            this.txtOrderNo = new System.Windows.Forms.TextBox();
            this.label7 = new MaterialSkin.Controls.MaterialLabel();
            this.txtOperator = new System.Windows.Forms.TextBox();
            this.label3 = new MaterialSkin.Controls.MaterialLabel();
            this.txtRecipeName = new System.Windows.Forms.TextBox();
            this.label2 = new MaterialSkin.Controls.MaterialLabel();
            this.txtMachineName = new System.Windows.Forms.TextBox();
            this.label1 = new MaterialSkin.Controls.MaterialLabel();
            this.pnlMainContent = new System.Windows.Forms.Panel();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.tabMainDetails = new System.Windows.Forms.TabControl();
            this.tabPageSteps = new System.Windows.Forms.TabPage();
            this.dgvStepDetails = new System.Windows.Forms.DataGridView();
            this.tabPageGraph = new System.Windows.Forms.TabPage();
            this.formsPlot1 = new ScottPlot.WinForms.FormsPlot();
            this.tabSubDetails = new System.Windows.Forms.TabControl();
            this.tabPageAlarms = new System.Windows.Forms.TabPage();
            this.dgvAlarms = new System.Windows.Forms.DataGridView();
            this.tabPageChemicals = new System.Windows.Forms.TabPage();
            this.dgvChemicals = new System.Windows.Forms.DataGridView();
            this.pnlBottom.SuspendLayout();
            this.gbProductionInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pieChartControl)).BeginInit();
            this.pnlMainContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.tabMainDetails.SuspendLayout();
            this.tabPageSteps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStepDetails)).BeginInit();
            this.tabPageGraph.SuspendLayout();
            this.tabSubDetails.SuspendLayout();
            this.tabPageAlarms.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAlarms)).BeginInit();
            this.tabPageChemicals.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChemicals)).BeginInit();
            this.SuspendLayout();
            // 
            // txtTheoreticalDuration
            // 
            this.txtTheoreticalDuration.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtTheoreticalDuration.Location = new System.Drawing.Point(11, 420);
            this.txtTheoreticalDuration.Name = "txtTheoreticalDuration";
            this.txtTheoreticalDuration.ReadOnly = true;
            this.txtTheoreticalDuration.Size = new System.Drawing.Size(284, 23);
            this.txtTheoreticalDuration.TabIndex = 17;
            // 
            // labelTheoretical
            // 
            this.labelTheoretical.AutoSize = true;
            this.labelTheoretical.Depth = 0;
            this.labelTheoretical.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.labelTheoretical.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.labelTheoretical.Location = new System.Drawing.Point(11, 401);
            this.labelTheoretical.Name = "labelTheoretical";
            this.labelTheoretical.Size = new System.Drawing.Size(121, 17);
            this.labelTheoretical.TabIndex = 16;
            this.labelTheoretical.Text = "Theoretical Duration:";
            // 
            // txtDurationDiff
            // 
            this.txtDurationDiff.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.txtDurationDiff.Location = new System.Drawing.Point(11, 466);
            this.txtDurationDiff.Name = "txtDurationDiff";
            this.txtDurationDiff.ReadOnly = true;
            this.txtDurationDiff.Size = new System.Drawing.Size(284, 23);
            this.txtDurationDiff.TabIndex = 19;
            // 
            // labelDiff
            // 
            this.labelDiff.AutoSize = true;
            this.labelDiff.Depth = 0;
            this.labelDiff.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.labelDiff.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.labelDiff.Location = new System.Drawing.Point(11, 447);
            this.labelDiff.Name = "labelDiff";
            this.labelDiff.Size = new System.Drawing.Size(126, 17);
            this.labelDiff.TabIndex = 18;
            this.labelDiff.Text = "Difference (Act-Theo):";
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.btnExportToExcel);
            this.pnlBottom.Controls.Add(this.btnClose);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(9, 888); // Yükseklik genişletildiği için aşağı kaydırıldı
            this.pnlBottom.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(1088, 44);
            this.pnlBottom.TabIndex = 0;
            // 
            // btnExportToExcel
            // 
            this.btnExportToExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportToExcel.AutoSize = false;
            this.btnExportToExcel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnExportToExcel.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnExportToExcel.Depth = 0;
            this.btnExportToExcel.HighEmphasis = true;
            this.btnExportToExcel.Icon = null;
            this.btnExportToExcel.Location = new System.Drawing.Point(830, 4);
            this.btnExportToExcel.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnExportToExcel.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnExportToExcel.Name = "btnExportToExcel";
            this.btnExportToExcel.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnExportToExcel.Size = new System.Drawing.Size(130, 36);
            this.btnExportToExcel.TabIndex = 1;
            this.btnExportToExcel.Text = "Export to Excel";
            this.btnExportToExcel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnExportToExcel.UseAccentColor = true;
            this.btnExportToExcel.UseVisualStyleBackColor = true;
            this.btnExportToExcel.Click += new System.EventHandler(this.btnExportToExcel_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.AutoSize = false;
            this.btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnClose.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnClose.Depth = 0;
            this.btnClose.HighEmphasis = false;
            this.btnClose.Icon = null;
            this.btnClose.Location = new System.Drawing.Point(968, 4);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnClose.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnClose.Name = "btnClose";
            this.btnClose.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnClose.Size = new System.Drawing.Size(110, 36);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnClose.UseAccentColor = false;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // gbProductionInfo
            // 
            this.gbProductionInfo.Controls.Add(this.txtSteam);
            this.gbProductionInfo.Controls.Add(this.label11);
            this.gbProductionInfo.Controls.Add(this.txtWater);
            this.gbProductionInfo.Controls.Add(this.label10);
            this.gbProductionInfo.Controls.Add(this.txtElectricity);
            this.gbProductionInfo.Controls.Add(this.label9);
            this.gbProductionInfo.Controls.Add(this.pieChartControl);
            this.gbProductionInfo.Controls.Add(this.txtTotalDuration);
            this.gbProductionInfo.Controls.Add(this.label6);
            this.gbProductionInfo.Controls.Add(this.txtStopTime);
            this.gbProductionInfo.Controls.Add(this.label5);
            this.gbProductionInfo.Controls.Add(this.txtStartTime);
            this.gbProductionInfo.Controls.Add(this.label4);
            this.gbProductionInfo.Controls.Add(this.txtCustomerNo);
            this.gbProductionInfo.Controls.Add(this.label8);
            this.gbProductionInfo.Controls.Add(this.txtOrderNo);
            this.gbProductionInfo.Controls.Add(this.label7);
            this.gbProductionInfo.Controls.Add(this.txtOperator);
            this.gbProductionInfo.Controls.Add(this.label3);
            this.gbProductionInfo.Controls.Add(this.txtRecipeName);
            this.gbProductionInfo.Controls.Add(this.label2);
            this.gbProductionInfo.Controls.Add(this.txtMachineName);
            this.gbProductionInfo.Controls.Add(this.label1);
            this.gbProductionInfo.Controls.Add(this.txtDurationDiff);
            this.gbProductionInfo.Controls.Add(this.labelDiff);
            this.gbProductionInfo.Controls.Add(this.txtTheoreticalDuration);
            this.gbProductionInfo.Controls.Add(this.labelTheoretical);
            this.gbProductionInfo.Dock = System.Windows.Forms.DockStyle.Left;
            this.gbProductionInfo.Location = new System.Drawing.Point(9, 72); // MaterialHeader altına çekildi
            this.gbProductionInfo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gbProductionInfo.Name = "gbProductionInfo";
            this.gbProductionInfo.Padding = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.gbProductionInfo.Size = new Size(306, 816);
            this.gbProductionInfo.TabIndex = 1;
            this.gbProductionInfo.TabStop = false;
            this.gbProductionInfo.Text = "Production Summary";
            // 
            // txtSteam
            // 
            this.txtSteam.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.txtSteam.Location = new System.Drawing.Point(11, 604);
            this.txtSteam.Name = "txtSteam";
            this.txtSteam.ReadOnly = true;
            this.txtSteam.Size = new System.Drawing.Size(284, 23);
            this.txtSteam.TabIndex = 25;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Depth = 0;
            this.label11.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label11.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label11.Location = new System.Drawing.Point(11, 585);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(147, 17);
            this.label11.TabIndex = 24;
            this.label11.Text = "Steam Consumption (kg):";
            // 
            // txtWater
            // 
            this.txtWater.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.txtWater.Location = new System.Drawing.Point(11, 558);
            this.txtWater.Name = "txtWater";
            this.txtWater.ReadOnly = true;
            this.txtWater.Size = new System.Drawing.Size(284, 23);
            this.txtWater.TabIndex = 23;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Depth = 0;
            this.label10.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label10.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label10.Location = new System.Drawing.Point(11, 539);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(150, 17);
            this.label10.TabIndex = 22;
            this.label10.Text = "Water Consumption (m³):";
            // 
            // txtElectricity
            // 
            this.txtElectricity.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.txtElectricity.Location = new System.Drawing.Point(11, 512);
            this.txtElectricity.Name = "txtElectricity";
            this.txtElectricity.ReadOnly = true;
            this.txtElectricity.Size = new System.Drawing.Size(284, 23);
            this.txtElectricity.TabIndex = 21;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Depth = 0;
            this.label9.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label9.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label9.Location = new System.Drawing.Point(11, 493);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(182, 17);
            this.label9.TabIndex = 20;
            this.label9.Text = "Electricity Consumption (kWh):";
            // 
            // pieChartControl
            // 
            chartArea1.Name = "ChartArea1";
            this.pieChartControl.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.pieChartControl.Legends.Add(legend1);
            this.pieChartControl.Location = new System.Drawing.Point(11, 642); // Daha temiz yerleşim için sola hizalandı
            this.pieChartControl.Name = "pieChartControl";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.pieChartControl.Series.Add(series1);
            this.pieChartControl.Size = new System.Drawing.Size(284, 165); // Genişlik grup kutusuna tam uyacak şekilde artırıldı
            this.pieChartControl.TabIndex = 16;
            this.pieChartControl.Text = "chart1";
            // 
            // txtTotalDuration
            // 
            this.txtTotalDuration.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtTotalDuration.Location = new System.Drawing.Point(11, 374);
            this.txtTotalDuration.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtTotalDuration.Name = "txtTotalDuration";
            this.txtTotalDuration.ReadOnly = true;
            this.txtTotalDuration.Size = new System.Drawing.Size(284, 23);
            this.txtTotalDuration.TabIndex = 15;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Depth = 0;
            this.label6.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label6.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label6.Location = new System.Drawing.Point(11, 355);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(89, 17);
            this.label6.TabIndex = 14;
            this.label6.Text = "Total Duration:";
            // 
            // txtStopTime
            // 
            this.txtStopTime.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtStopTime.Location = new System.Drawing.Point(11, 328);
            this.txtStopTime.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtStopTime.Name = "txtStopTime";
            this.txtStopTime.ReadOnly = true;
            this.txtStopTime.Size = new System.Drawing.Size(284, 23);
            this.txtStopTime.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Depth = 0;
            this.label5.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label5.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label5.Location = new System.Drawing.Point(11, 309);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 17);
            this.label5.TabIndex = 12;
            this.label5.Text = "End Date:";
            // 
            // txtStartTime
            // 
            this.txtStartTime.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtStartTime.Location = new System.Drawing.Point(11, 282);
            this.txtStartTime.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtStartTime.Name = "txtStartTime";
            this.txtStartTime.ReadOnly = true;
            this.txtStartTime.Size = new System.Drawing.Size(284, 23);
            this.txtStartTime.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Depth = 0;
            this.label4.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label4.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label4.Location = new System.Drawing.Point(11, 263);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 17);
            this.label4.TabIndex = 10;
            this.label4.Text = "Start Date:";
            // 
            // txtCustomerNo
            // 
            this.txtCustomerNo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtCustomerNo.Location = new System.Drawing.Point(11, 236);
            this.txtCustomerNo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtCustomerNo.Name = "txtCustomerNo";
            this.txtCustomerNo.ReadOnly = true;
            this.txtCustomerNo.Size = new System.Drawing.Size(284, 23);
            this.txtCustomerNo.TabIndex = 9;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Depth = 0;
            this.label8.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label8.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label8.Location = new System.Drawing.Point(11, 217);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(120, 17);
            this.label8.TabIndex = 8;
            this.label8.Text = "Customer Number:";
            // 
            // txtOrderNo
            // 
            this.txtOrderNo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtOrderNo.Location = new System.Drawing.Point(11, 190);
            this.txtOrderNo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtOrderNo.Name = "txtOrderNo";
            this.txtOrderNo.ReadOnly = true;
            this.txtOrderNo.Size = new System.Drawing.Size(284, 23);
            this.txtOrderNo.TabIndex = 7;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Depth = 0;
            this.label7.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label7.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label7.Location = new System.Drawing.Point(11, 171);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 17);
            this.label7.TabIndex = 6;
            this.label7.Text = "Order No:";
            // 
            // txtOperator
            // 
            this.txtOperator.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtOperator.Location = new System.Drawing.Point(11, 144);
            this.txtOperator.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtOperator.Name = "txtOperator";
            this.txtOperator.ReadOnly = true;
            this.txtOperator.Size = new System.Drawing.Size(284, 23);
            this.txtOperator.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Depth = 0;
            this.label3.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label3.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label3.Location = new System.Drawing.Point(11, 125);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Operator:";
            // 
            // txtRecipeName
            // 
            this.txtRecipeName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtRecipeName.Location = new System.Drawing.Point(11, 98);
            this.txtRecipeName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtRecipeName.Name = "txtRecipeName";
            this.txtRecipeName.ReadOnly = true;
            this.txtRecipeName.Size = new System.Drawing.Size(284, 23);
            this.txtRecipeName.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Depth = 0;
            this.label2.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label2.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label2.Location = new System.Drawing.Point(11, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Recipe Name:";
            // 
            // txtMachineName
            // 
            this.txtMachineName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtMachineName.Location = new System.Drawing.Point(11, 52);
            this.txtMachineName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtMachineName.Name = "txtMachineName";
            this.txtMachineName.ReadOnly = true;
            this.txtMachineName.Size = new System.Drawing.Size(284, 23);
            this.txtMachineName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Depth = 0;
            this.label1.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label1.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label1.Location = new System.Drawing.Point(11, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Machine Name:";
            // 
            // pnlMainContent
            // 
            this.pnlMainContent.Controls.Add(this.splitContainerMain);
            this.pnlMainContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainContent.Location = new System.Drawing.Point(315, 72); // MaterialHeader altına çekildi
            this.pnlMainContent.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlMainContent.Name = "pnlMainContent";
            this.pnlMainContent.Size = new System.Drawing.Size(782, 816);
            this.pnlMainContent.TabIndex = 2;
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMain.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.tabMainDetails);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.tabSubDetails);
            this.splitContainerMain.Size = new System.Drawing.Size(782, 816);
            this.splitContainerMain.SplitterDistance = 530;
            this.splitContainerMain.SplitterWidth = 3;
            this.splitContainerMain.TabIndex = 0;
            // 
            // tabMainDetails
            // 
            this.tabMainDetails.Controls.Add(this.tabPageSteps);
            this.tabMainDetails.Controls.Add(this.tabPageGraph);
            this.tabMainDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMainDetails.Location = new System.Drawing.Point(0, 0);
            this.tabMainDetails.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabMainDetails.Name = "tabMainDetails";
            this.tabMainDetails.SelectedIndex = 0;
            this.tabMainDetails.Size = new System.Drawing.Size(782, 530);
            this.tabMainDetails.TabIndex = 0;
            // 
            // tabPageSteps
            // 
            this.tabPageSteps.Controls.Add(this.dgvStepDetails);
            this.tabPageSteps.Location = new System.Drawing.Point(4, 24);
            this.tabPageSteps.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageSteps.Name = "tabPageSteps";
            this.tabPageSteps.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageSteps.Size = new System.Drawing.Size(774, 502);
            this.tabPageSteps.TabIndex = 0;
            this.tabPageSteps.Text = "Step Details";
            this.tabPageSteps.UseVisualStyleBackColor = true;
            // 
            // dgvStepDetails
            // 
            this.dgvStepDetails.AllowUserToAddRows = false;
            this.dgvStepDetails.AllowUserToDeleteRows = false;
            this.dgvStepDetails.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvStepDetails.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStepDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvStepDetails.Location = new System.Drawing.Point(3, 2);
            this.dgvStepDetails.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvStepDetails.Name = "dgvStepDetails";
            this.dgvStepDetails.ReadOnly = true;
            this.dgvStepDetails.RowHeadersWidth = 51;
            this.dgvStepDetails.RowTemplate.Height = 29;
            this.dgvStepDetails.Size = new System.Drawing.Size(768, 498);
            this.dgvStepDetails.TabIndex = 0;
            // 
            // tabPageGraph
            // 
            this.tabPageGraph.Controls.Add(this.formsPlot1);
            this.tabPageGraph.Location = new System.Drawing.Point(4, 24);
            this.tabPageGraph.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageGraph.Name = "tabPageGraph";
            this.tabPageGraph.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageGraph.Size = new System.Drawing.Size(774, 502);
            this.tabPageGraph.TabIndex = 1;
            this.tabPageGraph.Text = "Process Chart";
            this.tabPageGraph.UseVisualStyleBackColor = true;
            // 
            // formsPlot1
            // 
            this.formsPlot1.DisplayScale = 1F;
            this.formsPlot1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsPlot1.Location = new System.Drawing.Point(3, 2);
            this.formsPlot1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.formsPlot1.Name = "formsPlot1";
            this.formsPlot1.Size = new System.Drawing.Size(768, 498);
            this.formsPlot1.TabIndex = 0;
            // 
            // tabSubDetails
            // 
            this.tabSubDetails.Controls.Add(this.tabPageAlarms);
            this.tabSubDetails.Controls.Add(this.tabPageChemicals);
            this.tabSubDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSubDetails.Location = new System.Drawing.Point(0, 0);
            this.tabSubDetails.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabSubDetails.Name = "tabSubDetails";
            this.tabSubDetails.SelectedIndex = 0;
            this.tabSubDetails.Size = new System.Drawing.Size(782, 283);
            this.tabSubDetails.TabIndex = 0;
            // 
            // tabPageAlarms
            // 
            this.tabPageAlarms.Controls.Add(this.dgvAlarms);
            this.tabPageAlarms.Location = new System.Drawing.Point(4, 24);
            this.tabPageAlarms.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageAlarms.Name = "tabPageAlarms";
            this.tabPageAlarms.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageAlarms.Size = new System.Drawing.Size(774, 255);
            this.tabPageAlarms.TabIndex = 0;
            this.tabPageAlarms.Text = "Process Alarms";
            this.tabPageAlarms.UseVisualStyleBackColor = true;
            // 
            // dgvAlarms
            // 
            this.dgvAlarms.AllowUserToAddRows = false;
            this.dgvAlarms.AllowUserToDeleteRows = false;
            this.dgvAlarms.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAlarms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAlarms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAlarms.Location = new System.Drawing.Point(3, 2);
            this.dgvAlarms.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvAlarms.Name = "dgvAlarms";
            this.dgvAlarms.ReadOnly = true;
            this.dgvAlarms.RowHeadersWidth = 51;
            this.dgvAlarms.RowTemplate.Height = 29;
            this.dgvAlarms.Size = new System.Drawing.Size(768, 251);
            this.dgvAlarms.TabIndex = 0;
            // 
            // tabPageChemicals
            // 
            this.tabPageChemicals.Controls.Add(this.dgvChemicals);
            this.tabPageChemicals.Location = new System.Drawing.Point(4, 24);
            this.tabPageChemicals.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageChemicals.Name = "tabPageChemicals";
            this.tabPageChemicals.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageChemicals.Size = new System.Drawing.Size(774, 255);
            this.tabPageChemicals.TabIndex = 1;
            this.tabPageChemicals.Text = "Chemical Consumption";
            this.tabPageChemicals.UseVisualStyleBackColor = true;
            // 
            // dgvChemicals
            // 
            this.dgvChemicals.AllowUserToAddRows = false;
            this.dgvChemicals.AllowUserToDeleteRows = false;
            this.dgvChemicals.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvChemicals.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvChemicals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvChemicals.Location = new System.Drawing.Point(3, 2);
            this.dgvChemicals.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvChemicals.Name = "dgvChemicals";
            this.dgvChemicals.ReadOnly = true;
            this.dgvChemicals.RowHeadersWidth = 51;
            this.dgvChemicals.RowTemplate.Height = 29;
            this.dgvChemicals.Size = new System.Drawing.Size(768, 251);
            this.dgvChemicals.TabIndex = 0;
            // 
            // ProductionDetail_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1106, 940); // Üst bar ve pasta grafik payı için dikey boyut 940'a çıkarıldı
            this.Controls.Add(this.pnlMainContent);
            this.Controls.Add(this.gbProductionInfo);
            this.Controls.Add(this.pnlBottom);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None; // Çerçeve sorumluluğu Material kütüphanesinde
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinimumSize = new System.Drawing.Size(1122, 610);
            this.Name = "ProductionDetail_Form";
            this.Padding = new System.Windows.Forms.Padding(9, 72, 9, 8); // Üst boşluk 72px yapılarak Header taşması önlendi
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Production Report Detail";
            this.Load += new System.EventHandler(this.ProductionDetail_Form_Load);
            this.pnlBottom.ResumeLayout(false);
            this.gbProductionInfo.ResumeLayout(false);
            this.gbProductionInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pieChartControl)).EndInit();
            this.pnlMainContent.ResumeLayout(false);
            this.splitContainerMain.Panel1.ResumeLayout(false);
           
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
          
            this.tabMainDetails.ResumeLayout(false);
            this.tabPageSteps.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStepDetails)).EndInit();
            this.tabPageGraph.ResumeLayout(false);
            this.tabSubDetails.ResumeLayout(false);
            
            this.tabPageAlarms.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAlarms)).EndInit();
            this.tabPageChemicals.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvChemicals)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlBottom;
        private MaterialSkin.Controls.MaterialButton btnExportToExcel; // Sınıf türü güncellendi
        private MaterialSkin.Controls.MaterialButton btnClose;         // Sınıf türü güncellendi
        private System.Windows.Forms.GroupBox gbProductionInfo;
        private System.Windows.Forms.TextBox txtTotalDuration;
        private MaterialSkin.Controls.MaterialLabel label6;            // Sınıf türü güncellendi
        private System.Windows.Forms.TextBox txtStopTime;
        private MaterialSkin.Controls.MaterialLabel label5;            // Sınıf türü güncellendi
        private System.Windows.Forms.TextBox txtStartTime;
        private MaterialSkin.Controls.MaterialLabel label4;            // Sınıf türü güncellendi
        private System.Windows.Forms.TextBox txtCustomerNo;
        private MaterialSkin.Controls.MaterialLabel label8;            // Sınıf türü güncellendi
        private System.Windows.Forms.TextBox txtOrderNo;
        private MaterialSkin.Controls.MaterialLabel label7;            // Sınıf türü güncellendi
        private System.Windows.Forms.TextBox txtOperator;
        private MaterialSkin.Controls.MaterialLabel label3;            // Sınıf türü güncellendi
        private System.Windows.Forms.TextBox txtRecipeName;
        private MaterialSkin.Controls.MaterialLabel label2;            // Sınıf türü güncellendi
        private System.Windows.Forms.TextBox txtMachineName;
        private MaterialSkin.Controls.MaterialLabel label1;            // Sınıf türü güncellendi
        private System.Windows.Forms.Panel pnlMainContent;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.TabControl tabMainDetails;
        private System.Windows.Forms.TabPage tabPageSteps;
        private System.Windows.Forms.TabPage tabPageGraph;
        private System.Windows.Forms.TabControl tabSubDetails;
        private System.Windows.Forms.TabPage tabPageAlarms;
        private System.Windows.Forms.TabPage tabPageChemicals;
        private System.Windows.Forms.DataGridView dgvStepDetails;
        private ScottPlot.WinForms.FormsPlot formsPlot1;
        private System.Windows.Forms.DataGridView dgvAlarms;
        private System.Windows.Forms.DataGridView dgvChemicals;
        private System.Windows.Forms.DataVisualization.Charting.Chart pieChartControl;
        private System.Windows.Forms.TextBox txtTheoreticalDuration;
        private MaterialSkin.Controls.MaterialLabel labelTheoretical; // Sınıf türü güncellendi
        private System.Windows.Forms.TextBox txtDurationDiff;
        private MaterialSkin.Controls.MaterialLabel labelDiff;        // Sınıf türü güncellendi
        private System.Windows.Forms.TextBox txtSteam;
        private MaterialSkin.Controls.MaterialLabel label11;          // Sınıf türü güncellendi
        private System.Windows.Forms.TextBox txtWater;
        private MaterialSkin.Controls.MaterialLabel label10;          // Sınıf türü güncellendi
        private System.Windows.Forms.TextBox txtElectricity;
        private MaterialSkin.Controls.MaterialLabel label9;           // Sınıf türü güncellendi
    }
}