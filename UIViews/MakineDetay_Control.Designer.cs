namespace TekstilScada.UI.Views
{
    partial class MakineDetay_Control
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
            pnlTop = new Panel();
            btnGeri = new Button();
            lblMakineAdi = new Label();
            pnlMainContent = new Panel();
            tableLayoutPanelMain = new TableLayoutPanel();
            pnlTopDashboard = new Panel();
            tableLayoutPanelTop = new TableLayoutPanel();
            pnlGaugesAndInfo = new Panel();
            pnlGauges = new Panel();
            label14 = new Label();
            label13 = new Label();
            label12 = new Label();
            gaugeRpm = new Label();
            humuditytxt = new Label();
            lblTempValue = new Label();
            label102 = new Label();
            label101 = new Label();
            label100 = new Label();
            pnlInfo = new Panel();
            lblMusteriNo = new Label();
            label4 = new Label();
            lblOperator = new Label();
            label3 = new Label();
            lblReceteAdi = new Label();
            label2 = new Label();
            label1 = new Label();
            pnlTimeline = new Panel();
            tblPlots = new TableLayoutPanel();
            formsPlotTemp = new ScottPlot.WinForms.FormsPlot();
            formsPlotRpm = new ScottPlot.WinForms.FormsPlot();
            formsPlotAir = new ScottPlot.WinForms.FormsPlot();
            pnlAlarmsAndSteps = new Panel();
            lstAlarmlar = new ListBox();
            label7 = new Label();
            pnlTop.SuspendLayout();
            pnlMainContent.SuspendLayout();
            tableLayoutPanelMain.SuspendLayout();
            pnlTopDashboard.SuspendLayout();
            tableLayoutPanelTop.SuspendLayout();
            pnlGaugesAndInfo.SuspendLayout();
            pnlGauges.SuspendLayout();
            pnlInfo.SuspendLayout();
            pnlTimeline.SuspendLayout();
            tblPlots.SuspendLayout();
            pnlAlarmsAndSteps.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(btnGeri);
            pnlTop.Controls.Add(lblMakineAdi);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Margin = new Padding(3, 2, 3, 2);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1358, 38);
            pnlTop.TabIndex = 1;
            // 
            // btnGeri
            // 
            btnGeri.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGeri.Location = new Point(9, 8);
            btnGeri.Margin = new Padding(3, 2, 3, 2);
            btnGeri.Name = "btnGeri";
            btnGeri.Size = new Size(82, 22);
            btnGeri.TabIndex = 0;
            btnGeri.Text = "< GERİ";
            btnGeri.UseVisualStyleBackColor = true;
            // 
            // lblMakineAdi
            // 
            lblMakineAdi.Dock = DockStyle.Fill;
            lblMakineAdi.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold);
            lblMakineAdi.ForeColor = Color.FromArgb(45, 52, 54);
            lblMakineAdi.Location = new Point(0, 0);
            lblMakineAdi.Name = "lblMakineAdi";
            lblMakineAdi.Size = new Size(1358, 38);
            lblMakineAdi.TabIndex = 1;
            lblMakineAdi.Text = "MAKİNE ADI";
            lblMakineAdi.TextAlign = ContentAlignment.MiddleCenter;
            lblMakineAdi.Click += lblMakineAdi_Click;
            // 
            // pnlMainContent
            // 
            pnlMainContent.Controls.Add(tableLayoutPanelMain);
            pnlMainContent.Controls.Add(pnlAlarmsAndSteps);
            pnlMainContent.Dock = DockStyle.Fill;
            pnlMainContent.Location = new Point(0, 38);
            pnlMainContent.Margin = new Padding(3, 2, 3, 2);
            pnlMainContent.Name = "pnlMainContent";
            pnlMainContent.Size = new Size(1358, 761);
            pnlMainContent.TabIndex = 2;
            // 
            // tableLayoutPanelMain
            // 
            tableLayoutPanelMain.ColumnCount = 1;
            tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanelMain.Controls.Add(pnlTopDashboard, 0, 0);
            tableLayoutPanelMain.Controls.Add(pnlTimeline, 0, 1);
            tableLayoutPanelMain.Dock = DockStyle.Fill;
            tableLayoutPanelMain.Location = new Point(0, 0);
            tableLayoutPanelMain.Margin = new Padding(3, 2, 3, 2);
            tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            tableLayoutPanelMain.RowCount = 2;
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 239F));
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanelMain.Size = new Size(969, 761);
            tableLayoutPanelMain.TabIndex = 0;
            // 
            // pnlTopDashboard
            // 
            pnlTopDashboard.Controls.Add(tableLayoutPanelTop);
            pnlTopDashboard.Dock = DockStyle.Fill;
            pnlTopDashboard.Location = new Point(3, 2);
            pnlTopDashboard.Margin = new Padding(3, 2, 3, 2);
            pnlTopDashboard.Name = "pnlTopDashboard";
            pnlTopDashboard.Size = new Size(963, 235);
            pnlTopDashboard.TabIndex = 0;
            // 
            // tableLayoutPanelTop
            // 
            tableLayoutPanelTop.ColumnCount = 1;
            tableLayoutPanelTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 93.04734F));
            tableLayoutPanelTop.Controls.Add(pnlGaugesAndInfo, 0, 0);
            tableLayoutPanelTop.Dock = DockStyle.Fill;
            tableLayoutPanelTop.Location = new Point(0, 0);
            tableLayoutPanelTop.Margin = new Padding(3, 2, 3, 2);
            tableLayoutPanelTop.Name = "tableLayoutPanelTop";
            tableLayoutPanelTop.RowCount = 1;
            tableLayoutPanelTop.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanelTop.Size = new Size(963, 235);
            tableLayoutPanelTop.TabIndex = 0;
            // 
            // pnlGaugesAndInfo
            // 
            pnlGaugesAndInfo.BackColor = Color.WhiteSmoke;
            pnlGaugesAndInfo.Controls.Add(pnlGauges);
            pnlGaugesAndInfo.Controls.Add(pnlInfo);
            pnlGaugesAndInfo.Dock = DockStyle.Fill;
            pnlGaugesAndInfo.Location = new Point(3, 2);
            pnlGaugesAndInfo.Margin = new Padding(3, 2, 3, 2);
            pnlGaugesAndInfo.Name = "pnlGaugesAndInfo";
            pnlGaugesAndInfo.Size = new Size(957, 231);
            pnlGaugesAndInfo.TabIndex = 2;
            // 
            // pnlGauges
            // 
            pnlGauges.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            pnlGauges.AutoSize = true;
            pnlGauges.Controls.Add(label14);
            pnlGauges.Controls.Add(label13);
            pnlGauges.Controls.Add(label12);
            pnlGauges.Controls.Add(gaugeRpm);
            pnlGauges.Controls.Add(humuditytxt);
            pnlGauges.Controls.Add(lblTempValue);
            pnlGauges.Controls.Add(label102);
            pnlGauges.Controls.Add(label101);
            pnlGauges.Controls.Add(label100);
            pnlGauges.Location = new Point(203, 0);
            pnlGauges.Margin = new Padding(3, 2, 3, 2);
            pnlGauges.Name = "pnlGauges";
            pnlGauges.Size = new Size(760, 231);
            pnlGauges.TabIndex = 1;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label14.Location = new Point(623, 156);
            label14.Name = "label14";
            label14.Size = new Size(22, 15);
            label14.TabIndex = 26;
            label14.Text = "Kg";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label13.Location = new Point(379, 156);
            label13.Name = "label13";
            label13.Size = new Size(25, 15);
            label13.TabIndex = 25;
            label13.Text = "m3";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label12.Location = new Point(163, 156);
            label12.Name = "label12";
            label12.Size = new Size(25, 15);
            label12.TabIndex = 24;
            label12.Text = "m3";
            // 
            // gaugeRpm
            // 
            gaugeRpm.AutoSize = true;
            gaugeRpm.Font = new Font("Segoe UI", 48F, FontStyle.Bold);
            gaugeRpm.Location = new Point(558, 99);
            gaugeRpm.Name = "gaugeRpm";
            gaugeRpm.Size = new Size(74, 86);
            gaugeRpm.TabIndex = 23;
            gaugeRpm.Text = "0";
            // 
            // humuditytxt
            // 
            humuditytxt.AutoSize = true;
            humuditytxt.Font = new Font("Segoe UI", 48F, FontStyle.Bold);
            humuditytxt.Location = new Point(317, 99);
            humuditytxt.Name = "humuditytxt";
            humuditytxt.Size = new Size(74, 86);
            humuditytxt.TabIndex = 22;
            humuditytxt.Text = "0";
            // 
            // lblTempValue
            // 
            lblTempValue.AutoSize = true;
            lblTempValue.Font = new Font("Segoe UI", 48F, FontStyle.Bold);
            lblTempValue.Location = new Point(100, 99);
            lblTempValue.Name = "lblTempValue";
            lblTempValue.Size = new Size(74, 86);
            lblTempValue.TabIndex = 21;
            lblTempValue.Text = "0";
            // 
            // label102
            // 
            label102.AutoSize = true;
            label102.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label102.Location = new Point(572, 37);
            label102.Name = "label102";
            label102.Size = new Size(46, 15);
            label102.TabIndex = 20;
            label102.Text = "STEAM";
            // 
            // label101
            // 
            label101.AutoSize = true;
            label101.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label101.Location = new Point(341, 37);
            label101.Name = "label101";
            label101.Size = new Size(27, 15);
            label101.TabIndex = 19;
            label101.Text = "AIR";
            // 
            // label100
            // 
            label100.AutoSize = true;
            label100.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label100.Location = new Point(101, 37);
            label100.Name = "label100";
            label100.Size = new Size(58, 15);
            label100.TabIndex = 18;
            label100.Text = "VACUUM";
            // 
            // pnlInfo
            // 
            pnlInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            pnlInfo.Controls.Add(lblMusteriNo);
            pnlInfo.Controls.Add(label4);
            pnlInfo.Controls.Add(lblOperator);
            pnlInfo.Controls.Add(label3);
            pnlInfo.Controls.Add(lblReceteAdi);
            pnlInfo.Controls.Add(label2);
            pnlInfo.Controls.Add(label1);
            pnlInfo.Location = new Point(0, 0);
            pnlInfo.Margin = new Padding(3, 2, 3, 2);
            pnlInfo.Name = "pnlInfo";
            pnlInfo.Size = new Size(360, 231);
            pnlInfo.TabIndex = 0;
            // 
            // lblMusteriNo
            // 
            lblMusteriNo.BackColor = Color.White;
            lblMusteriNo.BorderStyle = BorderStyle.FixedSingle;
            lblMusteriNo.Font = new Font("Segoe UI", 9F);
            lblMusteriNo.Location = new Point(128, 114);
            lblMusteriNo.Name = "lblMusteriNo";
            lblMusteriNo.Size = new Size(175, 19);
            lblMusteriNo.TabIndex = 17;
            lblMusteriNo.Text = "---";
            lblMusteriNo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label4.Location = new Point(10, 115);
            label4.Name = "label4";
            label4.Size = new Size(113, 15);
            label4.TabIndex = 16;
            label4.Text = "Customer Number:";
            // 
            // lblOperator
            // 
            lblOperator.BackColor = Color.White;
            lblOperator.BorderStyle = BorderStyle.FixedSingle;
            lblOperator.Font = new Font("Segoe UI", 9F);
            lblOperator.Location = new Point(128, 91);
            lblOperator.Name = "lblOperator";
            lblOperator.Size = new Size(175, 19);
            lblOperator.TabIndex = 15;
            lblOperator.Text = "---";
            lblOperator.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label3.Location = new Point(10, 92);
            label3.Name = "label3";
            label3.Size = new Size(61, 15);
            label3.TabIndex = 14;
            label3.Text = "Operator:";
            // 
            // lblReceteAdi
            // 
            lblReceteAdi.BackColor = Color.White;
            lblReceteAdi.BorderStyle = BorderStyle.FixedSingle;
            lblReceteAdi.Font = new Font("Segoe UI", 9F);
            lblReceteAdi.Location = new Point(128, 67);
            lblReceteAdi.Name = "lblReceteAdi";
            lblReceteAdi.Size = new Size(175, 19);
            lblReceteAdi.TabIndex = 13;
            lblReceteAdi.Text = "---";
            lblReceteAdi.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label2.Location = new Point(10, 69);
            label2.Name = "label2";
            label2.Size = new Size(84, 15);
            label2.TabIndex = 12;
            label2.Text = "Recipe Name:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label1.Location = new Point(7, 32);
            label1.Name = "label1";
            label1.Size = new Size(172, 21);
            label1.TabIndex = 0;
            label1.Text = "Machine Information";
            // 
            // pnlTimeline
            // 
            pnlTimeline.Controls.Add(tblPlots);
            pnlTimeline.Dock = DockStyle.Fill;
            pnlTimeline.Location = new Point(3, 241);
            pnlTimeline.Margin = new Padding(3, 2, 3, 2);
            pnlTimeline.Name = "pnlTimeline";
            pnlTimeline.Size = new Size(963, 518);
            pnlTimeline.TabIndex = 1;
            // 
            // tblPlots
            // 
            tblPlots.ColumnCount = 1;
            tblPlots.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblPlots.Controls.Add(formsPlotTemp, 0, 0);
            tblPlots.Controls.Add(formsPlotRpm, 0, 1);
            tblPlots.Controls.Add(formsPlotAir, 0, 2);
            tblPlots.Dock = DockStyle.Fill;
            tblPlots.Location = new Point(0, 0);
            tblPlots.Name = "tblPlots";
            tblPlots.RowCount = 3;
            tblPlots.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            tblPlots.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            tblPlots.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            tblPlots.Size = new Size(963, 518);
            tblPlots.TabIndex = 0;
            // 
            // formsPlotTemp
            // 
            formsPlotTemp.DisplayScale = 1F;
            formsPlotTemp.Dock = DockStyle.Fill;
            formsPlotTemp.Location = new Point(3, 3);
            formsPlotTemp.Name = "formsPlotTemp";
            formsPlotTemp.Size = new Size(957, 166);
            formsPlotTemp.TabIndex = 0;
            // 
            // formsPlotRpm
            // 
            formsPlotRpm.DisplayScale = 1F;
            formsPlotRpm.Dock = DockStyle.Fill;
            formsPlotRpm.Location = new Point(3, 175);
            formsPlotRpm.Name = "formsPlotRpm";
            formsPlotRpm.Size = new Size(957, 166);
            formsPlotRpm.TabIndex = 1;
            // 
            // formsPlotAir
            // 
            formsPlotAir.DisplayScale = 1F;
            formsPlotAir.Dock = DockStyle.Fill;
            formsPlotAir.Location = new Point(3, 347);
            formsPlotAir.Name = "formsPlotAir";
            formsPlotAir.Size = new Size(957, 168);
            formsPlotAir.TabIndex = 2;
            // 
            // pnlAlarmsAndSteps
            // 
            pnlAlarmsAndSteps.BackColor = Color.WhiteSmoke;
            pnlAlarmsAndSteps.Controls.Add(lstAlarmlar);
            pnlAlarmsAndSteps.Controls.Add(label7);
            pnlAlarmsAndSteps.Dock = DockStyle.Right;
            pnlAlarmsAndSteps.Location = new Point(969, 0);
            pnlAlarmsAndSteps.Margin = new Padding(3, 2, 3, 2);
            pnlAlarmsAndSteps.Name = "pnlAlarmsAndSteps";
            pnlAlarmsAndSteps.Size = new Size(389, 761);
            pnlAlarmsAndSteps.TabIndex = 5;
            pnlAlarmsAndSteps.TabStop = true;
            pnlAlarmsAndSteps.Paint += pnlAlarmsAndSteps_Paint;
            // 
            // lstAlarmlar
            // 
            lstAlarmlar.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lstAlarmlar.BackColor = Color.FromArgb(45, 52, 54);
            lstAlarmlar.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lstAlarmlar.ForeColor = Color.White;
            lstAlarmlar.FormattingEnabled = true;
            lstAlarmlar.ItemHeight = 15;
            lstAlarmlar.Location = new Point(49, 22);
            lstAlarmlar.Margin = new Padding(3, 2, 3, 2);
            lstAlarmlar.Name = "lstAlarmlar";
            lstAlarmlar.Size = new Size(291, 709);
            lstAlarmlar.TabIndex = 1;
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            label7.Location = new Point(135, 4);
            label7.Name = "label7";
            label7.Size = new Size(133, 19);
            label7.TabIndex = 0;
            label7.Text = "Intra-Party Alarms";
            label7.Click += label7_Click;
            // 
            // MakineDetay_Control
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(pnlMainContent);
            Controls.Add(pnlTop);
            Margin = new Padding(3, 2, 3, 2);
            Name = "MakineDetay_Control";
            Size = new Size(1358, 799);
            pnlTop.ResumeLayout(false);
            pnlMainContent.ResumeLayout(false);
            tableLayoutPanelMain.ResumeLayout(false);
            pnlTopDashboard.ResumeLayout(false);
            tableLayoutPanelTop.ResumeLayout(false);
            pnlGaugesAndInfo.ResumeLayout(false);
            pnlGaugesAndInfo.PerformLayout();
            pnlGauges.ResumeLayout(false);
            pnlGauges.PerformLayout();
            pnlInfo.ResumeLayout(false);
            pnlInfo.PerformLayout();
            pnlTimeline.ResumeLayout(false);
            tblPlots.ResumeLayout(false);
            pnlAlarmsAndSteps.ResumeLayout(false);
            pnlAlarmsAndSteps.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Button btnGeri;
        private System.Windows.Forms.Label lblMakineAdi;
        private System.Windows.Forms.Panel pnlMainContent;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.Panel pnlTopDashboard;
        private System.Windows.Forms.Panel pnlTimeline;

        // DEĞİŞEN KISIM: Tek formsPlot1 yerine tablo ve 3 grafik
        private System.Windows.Forms.TableLayoutPanel tblPlots;

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTop;
        private System.Windows.Forms.Panel pnlGaugesAndInfo;
        private System.Windows.Forms.Panel pnlInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pnlGauges;
        private System.Windows.Forms.Label lblMusteriNo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblOperator;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblReceteAdi;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel pnlAlarmsAndSteps;
        private System.Windows.Forms.ListBox lstAlarmlar;
        private System.Windows.Forms.Label label7;
        private ScottPlot.WinForms.FormsPlot formsPlotTemp;
        private ScottPlot.WinForms.FormsPlot formsPlotRpm;
        private ScottPlot.WinForms.FormsPlot formsPlotAir;
        private Label gaugeRpm;
        private Label humuditytxt;
        private Label lblTempValue;
        private Label label102;
        private Label label101;
        private Label label100;
        private Label label14;
        private Label label13;
        private Label label12;
    }
}