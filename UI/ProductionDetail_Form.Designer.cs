namespace TekstilScada.UI
{
    partial class ProductionDetail_Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            txtTheoreticalDuration = new TextBox();
            labelTheoretical = new Label();
            txtDurationDiff = new TextBox();
            labelDiff = new Label();
            pnlBottom = new Panel();
            btnExportToExcel = new Button();
            btnClose = new Button();
            gbProductionInfo = new GroupBox();
            txtSteam = new TextBox();
            label11 = new Label();
            txtWater = new TextBox();
            label10 = new Label();
            txtElectricity = new TextBox();
            label9 = new Label();
            pieChartControl = new System.Windows.Forms.DataVisualization.Charting.Chart();
            txtTotalDuration = new TextBox();
            label6 = new Label();
            txtStopTime = new TextBox();
            label5 = new Label();
            txtStartTime = new TextBox();
            label4 = new Label();
            txtCustomerNo = new TextBox();
            label8 = new Label();
            txtOrderNo = new TextBox();
            label7 = new Label();
            txtOperator = new TextBox();
            label3 = new Label();
            txtRecipeName = new TextBox();
            label2 = new Label();
            txtMachineName = new TextBox();
            label1 = new Label();
            pnlMainContent = new Panel();
            splitContainerMain = new SplitContainer();
            tabMainDetails = new TabControl();
            tabPageSteps = new TabPage();
            dgvStepDetails = new DataGridView();
            tabPageGraph = new TabPage();
            formsPlot1 = new ScottPlot.WinForms.FormsPlot();
            tabSubDetails = new TabControl();
            tabPageAlarms = new TabPage();
            dgvAlarms = new DataGridView();
            tabPageChemicals = new TabPage();
            dgvChemicals = new DataGridView();
            pnlBottom.SuspendLayout();
            gbProductionInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pieChartControl).BeginInit();
            pnlMainContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            tabMainDetails.SuspendLayout();
            tabPageSteps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvStepDetails).BeginInit();
            tabPageGraph.SuspendLayout();
            tabSubDetails.SuspendLayout();
            tabPageAlarms.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAlarms).BeginInit();
            tabPageChemicals.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvChemicals).BeginInit();
            SuspendLayout();
            // 
            // txtTheoreticalDuration
            // 
            txtTheoreticalDuration.Font = new Font("Segoe UI", 9F);
            txtTheoreticalDuration.Location = new Point(11, 420);
            txtTheoreticalDuration.Name = "txtTheoreticalDuration";
            txtTheoreticalDuration.ReadOnly = true;
            txtTheoreticalDuration.Size = new Size(284, 23);
            txtTheoreticalDuration.TabIndex = 17;
            // 
            // labelTheoretical
            // 
            labelTheoretical.AutoSize = true;
            labelTheoretical.Font = new Font("Segoe UI", 9F);
            labelTheoretical.Location = new Point(11, 401);
            labelTheoretical.Name = "labelTheoretical";
            labelTheoretical.Size = new Size(118, 15);
            labelTheoretical.TabIndex = 16;
            labelTheoretical.Text = "Theoretical Duration:";
            // 
            // txtDurationDiff
            // 
            txtDurationDiff.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtDurationDiff.Location = new Point(11, 466);
            txtDurationDiff.Name = "txtDurationDiff";
            txtDurationDiff.ReadOnly = true;
            txtDurationDiff.Size = new Size(284, 23);
            txtDurationDiff.TabIndex = 19;
            // 
            // labelDiff
            // 
            labelDiff.AutoSize = true;
            labelDiff.Font = new Font("Segoe UI", 9F);
            labelDiff.Location = new Point(11, 447);
            labelDiff.Name = "labelDiff";
            labelDiff.Size = new Size(125, 15);
            labelDiff.TabIndex = 18;
            labelDiff.Text = "Difference (Act-Theo):";
            // 
            // pnlBottom
            // 
            pnlBottom.Controls.Add(btnExportToExcel);
            pnlBottom.Controls.Add(btnClose);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(9, 840);
            pnlBottom.Margin = new Padding(3, 2, 3, 2);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(1088, 38);
            pnlBottom.TabIndex = 0;
            // 
            // btnExportToExcel
            // 
            btnExportToExcel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExportToExcel.Font = new Font("Segoe UI", 9F);
            btnExportToExcel.Location = new Point(866, 8);
            btnExportToExcel.Margin = new Padding(3, 2, 3, 2);
            btnExportToExcel.Name = "btnExportToExcel";
            btnExportToExcel.Size = new Size(105, 22);
            btnExportToExcel.TabIndex = 1;
            btnExportToExcel.Text = "Export to Excel";
            btnExportToExcel.UseVisualStyleBackColor = true;
            btnExportToExcel.Click += btnExportToExcel_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Font = new Font("Segoe UI", 9F);
            btnClose.Location = new Point(976, 8);
            btnClose.Margin = new Padding(3, 2, 3, 2);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(105, 22);
            btnClose.TabIndex = 0;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // gbProductionInfo
            // 
            gbProductionInfo.Controls.Add(txtSteam);
            gbProductionInfo.Controls.Add(label11);
            gbProductionInfo.Controls.Add(txtWater);
            gbProductionInfo.Controls.Add(label10);
            gbProductionInfo.Controls.Add(txtElectricity);
            gbProductionInfo.Controls.Add(label9);
            gbProductionInfo.Controls.Add(pieChartControl);
            gbProductionInfo.Controls.Add(txtTotalDuration);
            gbProductionInfo.Controls.Add(label6);
            gbProductionInfo.Controls.Add(txtStopTime);
            gbProductionInfo.Controls.Add(label5);
            gbProductionInfo.Controls.Add(txtStartTime);
            gbProductionInfo.Controls.Add(label4);
            gbProductionInfo.Controls.Add(txtCustomerNo);
            gbProductionInfo.Controls.Add(label8);
            gbProductionInfo.Controls.Add(txtOrderNo);
            gbProductionInfo.Controls.Add(label7);
            gbProductionInfo.Controls.Add(txtOperator);
            gbProductionInfo.Controls.Add(label3);
            gbProductionInfo.Controls.Add(txtRecipeName);
            gbProductionInfo.Controls.Add(label2);
            gbProductionInfo.Controls.Add(txtMachineName);
            gbProductionInfo.Controls.Add(label1);
            gbProductionInfo.Controls.Add(txtDurationDiff);
            gbProductionInfo.Controls.Add(labelDiff);
            gbProductionInfo.Controls.Add(txtTheoreticalDuration);
            gbProductionInfo.Controls.Add(labelTheoretical);
            gbProductionInfo.Dock = DockStyle.Left;
            gbProductionInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gbProductionInfo.Location = new Point(9, 8);
            gbProductionInfo.Margin = new Padding(3, 2, 3, 2);
            gbProductionInfo.Name = "gbProductionInfo";
            gbProductionInfo.Padding = new Padding(9, 8, 9, 8);
            gbProductionInfo.Size = new Size(306, 832);
            gbProductionInfo.TabIndex = 1;
            gbProductionInfo.TabStop = false;
            gbProductionInfo.Text = "Production Summary";
            // 
            // txtSteam
            // 
            txtSteam.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtSteam.Location = new Point(11, 604);
            txtSteam.Name = "txtSteam";
            txtSteam.ReadOnly = true;
            txtSteam.Size = new Size(284, 23);
            txtSteam.TabIndex = 25;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 9F);
            label11.Location = new Point(11, 585);
            label11.Name = "label11";
            label11.Size = new Size(145, 15);
            label11.TabIndex = 24;
            label11.Text = "Steam Consumption (m³):";
            // 
            // txtWater
            // 
            txtWater.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtWater.Location = new Point(11, 558);
            txtWater.Name = "txtWater";
            txtWater.ReadOnly = true;
            txtWater.Size = new Size(284, 23);
            txtWater.TabIndex = 23;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 9F);
            label10.Location = new Point(11, 539);
            label10.Name = "label10";
            label10.Size = new Size(143, 15);
            label10.TabIndex = 22;
            label10.Text = "Water Consumption (m³):";
            // 
            // txtElectricity
            // 
            txtElectricity.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtElectricity.Location = new Point(11, 512);
            txtElectricity.Name = "txtElectricity";
            txtElectricity.ReadOnly = true;
            txtElectricity.Size = new Size(284, 23);
            txtElectricity.TabIndex = 21;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 9F);
            label9.Location = new Point(11, 493);
            label9.Name = "label9";
            label9.Size = new Size(172, 15);
            label9.TabIndex = 20;
            label9.Text = "Electricity Consumption (kWh):";
            // 
            // pieChartControl
            // 
            chartArea1.Name = "ChartArea1";
            pieChartControl.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            pieChartControl.Legends.Add(legend1);
            pieChartControl.Location = new Point(28, 652);
            pieChartControl.Name = "pieChartControl";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            pieChartControl.Series.Add(series1);
            pieChartControl.Size = new Size(217, 149);
            pieChartControl.TabIndex = 16;
            pieChartControl.Text = "chart1";
            // 
            // txtTotalDuration
            // 
            txtTotalDuration.Font = new Font("Segoe UI", 9F);
            txtTotalDuration.Location = new Point(11, 374);
            txtTotalDuration.Margin = new Padding(3, 2, 3, 2);
            txtTotalDuration.Name = "txtTotalDuration";
            txtTotalDuration.ReadOnly = true;
            txtTotalDuration.Size = new Size(284, 23);
            txtTotalDuration.TabIndex = 15;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 9F);
            label6.Location = new Point(11, 355);
            label6.Name = "label6";
            label6.Size = new Size(85, 15);
            label6.TabIndex = 14;
            label6.Text = "Total Duration:";
            // 
            // txtStopTime
            // 
            txtStopTime.Font = new Font("Segoe UI", 9F);
            txtStopTime.Location = new Point(11, 328);
            txtStopTime.Margin = new Padding(3, 2, 3, 2);
            txtStopTime.Name = "txtStopTime";
            txtStopTime.ReadOnly = true;
            txtStopTime.Size = new Size(284, 23);
            txtStopTime.TabIndex = 13;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 9F);
            label5.Location = new Point(11, 309);
            label5.Name = "label5";
            label5.Size = new Size(57, 15);
            label5.TabIndex = 12;
            label5.Text = "End Date:";
            // 
            // txtStartTime
            // 
            txtStartTime.Font = new Font("Segoe UI", 9F);
            txtStartTime.Location = new Point(11, 282);
            txtStartTime.Margin = new Padding(3, 2, 3, 2);
            txtStartTime.Name = "txtStartTime";
            txtStartTime.ReadOnly = true;
            txtStartTime.Size = new Size(284, 23);
            txtStartTime.TabIndex = 11;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F);
            label4.Location = new Point(11, 263);
            label4.Name = "label4";
            label4.Size = new Size(61, 15);
            label4.TabIndex = 10;
            label4.Text = "Start Date:";
            // 
            // txtCustomerNo
            // 
            txtCustomerNo.Font = new Font("Segoe UI", 9F);
            txtCustomerNo.Location = new Point(11, 236);
            txtCustomerNo.Margin = new Padding(3, 2, 3, 2);
            txtCustomerNo.Name = "txtCustomerNo";
            txtCustomerNo.ReadOnly = true;
            txtCustomerNo.Size = new Size(284, 23);
            txtCustomerNo.TabIndex = 9;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 9F);
            label8.Location = new Point(11, 217);
            label8.Name = "label8";
            label8.Size = new Size(109, 15);
            label8.TabIndex = 8;
            label8.Text = "Customer Number:";
            // 
            // txtOrderNo
            // 
            txtOrderNo.Font = new Font("Segoe UI", 9F);
            txtOrderNo.Location = new Point(11, 190);
            txtOrderNo.Margin = new Padding(3, 2, 3, 2);
            txtOrderNo.Name = "txtOrderNo";
            txtOrderNo.ReadOnly = true;
            txtOrderNo.Size = new Size(284, 23);
            txtOrderNo.TabIndex = 7;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 9F);
            label7.Location = new Point(11, 171);
            label7.Name = "label7";
            label7.Size = new Size(59, 15);
            label7.TabIndex = 6;
            label7.Text = "Order No:";
            // 
            // txtOperator
            // 
            txtOperator.Font = new Font("Segoe UI", 9F);
            txtOperator.Location = new Point(11, 144);
            txtOperator.Margin = new Padding(3, 2, 3, 2);
            txtOperator.Name = "txtOperator";
            txtOperator.ReadOnly = true;
            txtOperator.Size = new Size(284, 23);
            txtOperator.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F);
            label3.Location = new Point(11, 125);
            label3.Name = "label3";
            label3.Size = new Size(57, 15);
            label3.TabIndex = 4;
            label3.Text = "Operator:";
            // 
            // txtRecipeName
            // 
            txtRecipeName.Font = new Font("Segoe UI", 9F);
            txtRecipeName.Location = new Point(11, 98);
            txtRecipeName.Margin = new Padding(3, 2, 3, 2);
            txtRecipeName.Name = "txtRecipeName";
            txtRecipeName.ReadOnly = true;
            txtRecipeName.Size = new Size(284, 23);
            txtRecipeName.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F);
            label2.Location = new Point(11, 79);
            label2.Name = "label2";
            label2.Size = new Size(80, 15);
            label2.TabIndex = 2;
            label2.Text = "Recipe Name:";
            // 
            // txtMachineName
            // 
            txtMachineName.Font = new Font("Segoe UI", 9F);
            txtMachineName.Location = new Point(11, 52);
            txtMachineName.Margin = new Padding(3, 2, 3, 2);
            txtMachineName.Name = "txtMachineName";
            txtMachineName.ReadOnly = true;
            txtMachineName.Size = new Size(284, 23);
            txtMachineName.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F);
            label1.Location = new Point(11, 33);
            label1.Name = "label1";
            label1.Size = new Size(91, 15);
            label1.TabIndex = 0;
            label1.Text = "Machine Name:";
            // 
            // pnlMainContent
            // 
            pnlMainContent.Controls.Add(splitContainerMain);
            pnlMainContent.Dock = DockStyle.Fill;
            pnlMainContent.Location = new Point(315, 8);
            pnlMainContent.Margin = new Padding(3, 2, 3, 2);
            pnlMainContent.Name = "pnlMainContent";
            pnlMainContent.Size = new Size(782, 832);
            pnlMainContent.TabIndex = 2;
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.Location = new Point(0, 0);
            splitContainerMain.Margin = new Padding(3, 2, 3, 2);
            splitContainerMain.Name = "splitContainerMain";
            splitContainerMain.Orientation = Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(tabMainDetails);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(tabSubDetails);
            splitContainerMain.Size = new Size(782, 832);
            splitContainerMain.SplitterDistance = 548;
            splitContainerMain.SplitterWidth = 3;
            splitContainerMain.TabIndex = 0;
            // 
            // tabMainDetails
            // 
            tabMainDetails.Controls.Add(tabPageSteps);
            tabMainDetails.Controls.Add(tabPageGraph);
            tabMainDetails.Dock = DockStyle.Fill;
            tabMainDetails.Location = new Point(0, 0);
            tabMainDetails.Margin = new Padding(3, 2, 3, 2);
            tabMainDetails.Name = "tabMainDetails";
            tabMainDetails.SelectedIndex = 0;
            tabMainDetails.Size = new Size(782, 548);
            tabMainDetails.TabIndex = 0;
            // 
            // tabPageSteps
            // 
            tabPageSteps.Controls.Add(dgvStepDetails);
            tabPageSteps.Location = new Point(4, 24);
            tabPageSteps.Margin = new Padding(3, 2, 3, 2);
            tabPageSteps.Name = "tabPageSteps";
            tabPageSteps.Padding = new Padding(3, 2, 3, 2);
            tabPageSteps.Size = new Size(774, 520);
            tabPageSteps.TabIndex = 0;
            tabPageSteps.Text = "Step Details";
            tabPageSteps.UseVisualStyleBackColor = true;
            // 
            // dgvStepDetails
            // 
            dgvStepDetails.AllowUserToAddRows = false;
            dgvStepDetails.AllowUserToDeleteRows = false;
            dgvStepDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvStepDetails.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvStepDetails.Dock = DockStyle.Fill;
            dgvStepDetails.Location = new Point(3, 2);
            dgvStepDetails.Margin = new Padding(3, 2, 3, 2);
            dgvStepDetails.Name = "dgvStepDetails";
            dgvStepDetails.ReadOnly = true;
            dgvStepDetails.RowHeadersWidth = 51;
            dgvStepDetails.RowTemplate.Height = 29;
            dgvStepDetails.Size = new Size(768, 516);
            dgvStepDetails.TabIndex = 0;
            // 
            // tabPageGraph
            // 
            tabPageGraph.Controls.Add(formsPlot1);
            tabPageGraph.Location = new Point(4, 24);
            tabPageGraph.Margin = new Padding(3, 2, 3, 2);
            tabPageGraph.Name = "tabPageGraph";
            tabPageGraph.Padding = new Padding(3, 2, 3, 2);
            tabPageGraph.Size = new Size(774, 520);
            tabPageGraph.TabIndex = 1;
            tabPageGraph.Text = "Process Chart";
            tabPageGraph.UseVisualStyleBackColor = true;
            // 
            // formsPlot1
            // 
            formsPlot1.DisplayScale = 1F;
            formsPlot1.Dock = DockStyle.Fill;
            formsPlot1.Location = new Point(3, 2);
            formsPlot1.Margin = new Padding(3, 2, 3, 2);
            formsPlot1.Name = "formsPlot1";
            formsPlot1.Size = new Size(768, 516);
            formsPlot1.TabIndex = 0;
            // 
            // tabSubDetails
            // 
            tabSubDetails.Controls.Add(tabPageAlarms);
            tabSubDetails.Controls.Add(tabPageChemicals);
            tabSubDetails.Dock = DockStyle.Fill;
            tabSubDetails.Location = new Point(0, 0);
            tabSubDetails.Margin = new Padding(3, 2, 3, 2);
            tabSubDetails.Name = "tabSubDetails";
            tabSubDetails.SelectedIndex = 0;
            tabSubDetails.Size = new Size(782, 281);
            tabSubDetails.TabIndex = 0;
            // 
            // tabPageAlarms
            // 
            tabPageAlarms.Controls.Add(dgvAlarms);
            tabPageAlarms.Location = new Point(4, 24);
            tabPageAlarms.Margin = new Padding(3, 2, 3, 2);
            tabPageAlarms.Name = "tabPageAlarms";
            tabPageAlarms.Padding = new Padding(3, 2, 3, 2);
            tabPageAlarms.Size = new Size(774, 253);
            tabPageAlarms.TabIndex = 0;
            tabPageAlarms.Text = "Process Alarms";
            tabPageAlarms.UseVisualStyleBackColor = true;
            // 
            // dgvAlarms
            // 
            dgvAlarms.AllowUserToAddRows = false;
            dgvAlarms.AllowUserToDeleteRows = false;
            dgvAlarms.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAlarms.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAlarms.Dock = DockStyle.Fill;
            dgvAlarms.Location = new Point(3, 2);
            dgvAlarms.Margin = new Padding(3, 2, 3, 2);
            dgvAlarms.Name = "dgvAlarms";
            dgvAlarms.ReadOnly = true;
            dgvAlarms.RowHeadersWidth = 51;
            dgvAlarms.RowTemplate.Height = 29;
            dgvAlarms.Size = new Size(768, 249);
            dgvAlarms.TabIndex = 0;
            // 
            // tabPageChemicals
            // 
            tabPageChemicals.Controls.Add(dgvChemicals);
            tabPageChemicals.Location = new Point(4, 24);
            tabPageChemicals.Margin = new Padding(3, 2, 3, 2);
            tabPageChemicals.Name = "tabPageChemicals";
            tabPageChemicals.Padding = new Padding(3, 2, 3, 2);
            tabPageChemicals.Size = new Size(774, 253);
            tabPageChemicals.TabIndex = 1;
            tabPageChemicals.Text = "Chemical Consumption";
            tabPageChemicals.UseVisualStyleBackColor = true;
            // 
            // dgvChemicals
            // 
            dgvChemicals.AllowUserToAddRows = false;
            dgvChemicals.AllowUserToDeleteRows = false;
            dgvChemicals.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvChemicals.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvChemicals.Dock = DockStyle.Fill;
            dgvChemicals.Location = new Point(3, 2);
            dgvChemicals.Margin = new Padding(3, 2, 3, 2);
            dgvChemicals.Name = "dgvChemicals";
            dgvChemicals.ReadOnly = true;
            dgvChemicals.RowHeadersWidth = 51;
            dgvChemicals.RowTemplate.Height = 29;
            dgvChemicals.Size = new Size(768, 249);
            dgvChemicals.TabIndex = 0;
            // 
            // ProductionDetail_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1106, 886);
            Controls.Add(pnlMainContent);
            Controls.Add(gbProductionInfo);
            Controls.Add(pnlBottom);
            Margin = new Padding(3, 2, 3, 2);
            MinimumSize = new Size(1122, 610);
            Name = "ProductionDetail_Form";
            Padding = new Padding(9, 8, 9, 8);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Production Report Detail";
            Load += ProductionDetail_Form_Load;
            pnlBottom.ResumeLayout(false);
            gbProductionInfo.ResumeLayout(false);
            gbProductionInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pieChartControl).EndInit();
            pnlMainContent.ResumeLayout(false);
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            tabMainDetails.ResumeLayout(false);
            tabPageSteps.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvStepDetails).EndInit();
            tabPageGraph.ResumeLayout(false);
            tabSubDetails.ResumeLayout(false);
            tabPageAlarms.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvAlarms).EndInit();
            tabPageChemicals.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvChemicals).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Button btnExportToExcel;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.GroupBox gbProductionInfo;
        private System.Windows.Forms.TextBox txtTotalDuration;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtStopTime;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtStartTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtCustomerNo;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtOrderNo;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtOperator;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtRecipeName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtMachineName;
        private System.Windows.Forms.Label label1;
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
        private System.Windows.Forms.TextBox txtTheoreticalDuration; // YENİ
        private System.Windows.Forms.Label labelTheoretical; // YENİ
        private System.Windows.Forms.TextBox txtDurationDiff; // YENİ
        private System.Windows.Forms.Label labelDiff; // YENİ
        private TextBox txtSteam;
        private Label label11;
        private TextBox txtWater;
        private Label label10;
        private TextBox txtElectricity;
        private Label label9;
    }
}


