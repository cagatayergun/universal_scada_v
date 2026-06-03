// UI/Views/MakineDetay_Control.Designer.cs
namespace Telemetry.UI.Views
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
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnGeri = new MaterialSkin.Controls.MaterialButton();       // MaterialButton yapıldı
            this.lblMakineAdi = new MaterialSkin.Controls.MaterialLabel();   // MaterialLabel yapıldı
            this.pnlMainContent = new System.Windows.Forms.Panel();
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlTopDashboard = new System.Windows.Forms.Panel();
            this.tableLayoutPanelTop = new System.Windows.Forms.TableLayoutPanel();
            this.pnlGaugesAndInfo = new System.Windows.Forms.Panel();
            this.pnlGauges = new System.Windows.Forms.Panel();
            this.humuditypanel = new System.Windows.Forms.Panel();
            this.label12 = new MaterialSkin.Controls.MaterialLabel();
            this.label13 = new MaterialSkin.Controls.MaterialLabel();
            this.humuditytxt = new System.Windows.Forms.Label();
            this.lblhumudity = new MaterialSkin.Controls.MaterialLabel();
            this.humuditybar = new System.Windows.Forms.Panel();
            this.waterTankGauge1 = new Telemetry.UI.Controls.WaterTankGauge();
            this.panelTemp = new System.Windows.Forms.Panel();
            this.label11 = new MaterialSkin.Controls.MaterialLabel();
            this.label9 = new MaterialSkin.Controls.MaterialLabel();
            this.lblTempValue = new System.Windows.Forms.Label();
            this.lblTempTitle = new MaterialSkin.Controls.MaterialLabel();
            this.progressTemp = new System.Windows.Forms.Panel();
            this.gaugeRpm = new CircularProgressBar.CircularProgressBar();
            this.pnlInfo = new System.Windows.Forms.Panel();
            this.lblSiparisNo = new MaterialSkin.Controls.MaterialLabel();   // MaterialLabel yapıldı
            this.label6 = new MaterialSkin.Controls.MaterialLabel();         // MaterialLabel yapıldı
            this.lblBatchNo = new MaterialSkin.Controls.MaterialLabel();     // MaterialLabel yapıldı
            this.label5 = new MaterialSkin.Controls.MaterialLabel();         // MaterialLabel yapıldı
            this.lblMusteriNo = new MaterialSkin.Controls.MaterialLabel();   // MaterialLabel yapıldı
            this.label4 = new MaterialSkin.Controls.MaterialLabel();         // MaterialLabel yapıldı
            this.lblOperator = new MaterialSkin.Controls.MaterialLabel();    // MaterialLabel yapıldı
            this.label3 = new MaterialSkin.Controls.MaterialLabel();         // MaterialLabel yapıldı
            this.lblReceteAdi = new MaterialSkin.Controls.MaterialLabel();   // MaterialLabel yapıldı
            this.label2 = new MaterialSkin.Controls.MaterialLabel();         // MaterialLabel yapıldı
            this.label1 = new MaterialSkin.Controls.MaterialLabel();         // MaterialLabel yapıldı
            this.pnlTimeline = new System.Windows.Forms.Panel();
            this.tblPlots = new System.Windows.Forms.TableLayoutPanel();
            this.formsPlotTemp = new ScottPlot.WinForms.FormsPlot();
            this.formsPlotRpm = new ScottPlot.WinForms.FormsPlot();
            this.formsPlotWater = new ScottPlot.WinForms.FormsPlot();
            this.pnlAlarmsAndSteps = new System.Windows.Forms.Panel();
            this.dgvAdimlar = new System.Windows.Forms.DataGridView();
            this.label10 = new MaterialSkin.Controls.MaterialLabel();        // MaterialLabel yapıldı
            this.lblCalisanAdim = new MaterialSkin.Controls.MaterialLabel();  // MaterialLabel yapıldı
            this.label8 = new MaterialSkin.Controls.MaterialLabel();         // MaterialLabel yapıldı
            this.lstAlarmlar = new System.Windows.Forms.ListBox();
            this.label7 = new MaterialSkin.Controls.MaterialLabel();         // MaterialLabel yapıldı
            this.pnlTop.SuspendLayout();
            this.pnlMainContent.SuspendLayout();
            this.tableLayoutPanelMain.SuspendLayout();
            this.pnlTopDashboard.SuspendLayout();
            this.tableLayoutPanelTop.SuspendLayout();
            this.pnlGaugesAndInfo.SuspendLayout();
            this.pnlGauges.SuspendLayout();
            this.humuditypanel.SuspendLayout();
            this.panelTemp.SuspendLayout();
            this.pnlInfo.SuspendLayout();
            this.pnlTimeline.SuspendLayout();
            this.tblPlots.SuspendLayout();
            this.pnlAlarmsAndSteps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAdimlar)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.Transparent;
            this.pnlTop.Controls.Add(this.btnGeri);
            this.pnlTop.Controls.Add(this.lblMakineAdi);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1358, 44); // 36px modern buton için yükseklik 44'e genişletildi
            this.pnlTop.TabIndex = 1;
            // 
            // btnGeri
            // 
            this.btnGeri.AutoSize = false;
            this.btnGeri.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnGeri.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnGeri.Depth = 0;
            this.btnGeri.HighEmphasis = false;
            this.btnGeri.Icon = null;
            this.btnGeri.Location = new System.Drawing.Point(9, 4); // Dikey eksende kusursuz ortalandı
            this.btnGeri.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnGeri.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnGeri.Name = "btnGeri";
            this.btnGeri.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnGeri.Size = new System.Drawing.Size(85, 36);
            this.btnGeri.TabIndex = 0;
            this.btnGeri.Text = "< Back";
            this.btnGeri.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined; // Çizgili flat stil
            this.btnGeri.UseAccentColor = false;
            this.btnGeri.UseVisualStyleBackColor = true;
            // 
            // lblMakineAdi
            // 
            this.lblMakineAdi.Depth = 0;
            this.lblMakineAdi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMakineAdi.Font = new System.Drawing.Font("Roboto", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblMakineAdi.FontType = MaterialSkin.MaterialSkinManager.fontType.H5; // Modern üst başlık hiyerarşisi
            this.lblMakineAdi.Location = new System.Drawing.Point(0, 0);
            this.lblMakineAdi.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblMakineAdi.Name = "lblMakineAdi";
            this.lblMakineAdi.Size = new System.Drawing.Size(1358, 44);
            this.lblMakineAdi.TabIndex = 1;
            this.lblMakineAdi.Text = "MACHINE NAME";
            this.lblMakineAdi.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlMainContent
            // 
            this.pnlMainContent.BackColor = System.Drawing.Color.Transparent;
            this.pnlMainContent.Controls.Add(this.tableLayoutPanelMain);
            this.pnlMainContent.Controls.Add(this.pnlAlarmsAndSteps);
            this.pnlMainContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainContent.Location = new System.Drawing.Point(0, 44);
            this.pnlMainContent.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlMainContent.Name = "pnlMainContent";
            this.pnlMainContent.Size = new System.Drawing.Size(1358, 755);
            this.pnlMainContent.TabIndex = 2;
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 1;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Controls.Add(this.pnlTopDashboard, 0, 0);
            this.tableLayoutPanelMain.Controls.Add(this.pnlTimeline, 0, 1);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 2;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 239F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(969, 755);
            this.tableLayoutPanelMain.TabIndex = 0;
            // 
            // pnlTopDashboard
            // 
            this.pnlTopDashboard.Controls.Add(this.tableLayoutPanelTop);
            this.pnlTopDashboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTopDashboard.Location = new System.Drawing.Point(3, 2);
            this.pnlTopDashboard.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlTopDashboard.Name = "pnlTopDashboard";
            this.pnlTopDashboard.Size = new System.Drawing.Size(963, 235);
            this.pnlTopDashboard.TabIndex = 0;
            // 
            // tableLayoutPanelTop
            // 
            this.tableLayoutPanelTop.ColumnCount = 1;
            this.tableLayoutPanelTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelTop.Controls.Add(this.pnlGaugesAndInfo, 0, 0);
            this.tableLayoutPanelTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelTop.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelTop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanelTop.Name = "tableLayoutPanelTop";
            this.tableLayoutPanelTop.RowCount = 1;
            this.tableLayoutPanelTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelTop.Size = new System.Drawing.Size(963, 235);
            this.tableLayoutPanelTop.TabIndex = 0;
            // 
            // pnlGaugesAndInfo
            // 
            this.pnlGaugesAndInfo.BackColor = System.Drawing.Color.Transparent; // Koyu grafit karta tam uyum sağlandı
            this.pnlGaugesAndInfo.Controls.Add(this.pnlGauges);
            this.pnlGaugesAndInfo.Controls.Add(this.pnlInfo);
            this.pnlGaugesAndInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGaugesAndInfo.Location = new System.Drawing.Point(3, 2);
            this.pnlGaugesAndInfo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlGaugesAndInfo.Name = "pnlGaugesAndInfo";
            this.pnlGaugesAndInfo.Size = new System.Drawing.Size(957, 231);
            this.pnlGaugesAndInfo.TabIndex = 2;
            // 
            // pnlGauges
            // 
            this.pnlGauges.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom;
            this.pnlGauges.AutoSize = true;
            this.pnlGauges.Controls.Add(this.humuditypanel);
            this.pnlGauges.Controls.Add(this.waterTankGauge1);
            this.pnlGauges.Controls.Add(this.panelTemp);
            this.pnlGauges.Controls.Add(this.gaugeRpm);
            this.pnlGauges.Location = new System.Drawing.Point(203, 0);
            this.pnlGauges.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlGauges.Name = "pnlGauges";
            this.pnlGauges.Size = new System.Drawing.Size(760, 231);
            this.pnlGauges.TabIndex = 1;
            // 
            // humuditypanel
            // 
            this.humuditypanel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.humuditypanel.Controls.Add(this.label12);
            this.humuditypanel.Controls.Add(this.label13);
            this.humuditypanel.Controls.Add(this.humuditytxt);
            this.humuditypanel.Controls.Add(this.lblhumudity);
            this.humuditypanel.Controls.Add(this.humuditybar);
            this.humuditypanel.Location = new System.Drawing.Point(565, 28);
            this.humuditypanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.humuditypanel.Name = "humuditypanel";
            this.humuditypanel.Size = new System.Drawing.Size(105, 176);
            this.humuditypanel.TabIndex = 4;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Depth = 0;
            this.label12.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label12.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label12.Location = new System.Drawing.Point(25, 118);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(7, 17);
            this.label12.TabIndex = 6;
            this.label12.Text = "0";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Depth = 0;
            this.label13.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label13.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label13.Location = new System.Drawing.Point(11, 17);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(21, 17);
            this.label13.TabIndex = 5;
            this.label13.Text = "100";
            // 
            // humuditytxt
            // 
            this.humuditytxt.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.humuditytxt.Font = new System.Drawing.Font("Segoe UI Black", 12F, System.Drawing.FontStyle.Bold);
            this.humuditytxt.Location = new System.Drawing.Point(0, 132);
            this.humuditytxt.Name = "humuditytxt";
            this.humuditytxt.Size = new System.Drawing.Size(105, 22);
            this.humuditytxt.TabIndex = 2;
            this.humuditytxt.Text = "0 Rh";
            this.humuditytxt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblhumudity
            // 
            this.lblhumudity.Depth = 0;
            this.lblhumudity.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblhumudity.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblhumudity.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.lblhumudity.Location = new System.Drawing.Point(0, 154);
            this.lblhumudity.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblhumudity.Name = "lblhumudity";
            this.lblhumudity.Size = new System.Drawing.Size(105, 22);
            this.lblhumudity.TabIndex = 1;
            this.lblhumudity.Text = "Humidity";
            this.lblhumudity.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // humuditybar
            // 
            this.humuditybar.Location = new System.Drawing.Point(39, 17);
            this.humuditybar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.humuditybar.Name = "humuditybar";
            this.humuditybar.Size = new System.Drawing.Size(26, 113);
            this.humuditybar.TabIndex = 0;
            // 
            // waterTankGauge1
            // 
            this.waterTankGauge1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.waterTankGauge1.AutoSize = true;
            this.waterTankGauge1.Location = new System.Drawing.Point(531, 18);
            this.waterTankGauge1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.waterTankGauge1.Maximum = 5000;
            this.waterTankGauge1.Name = "waterTankGauge1";
            this.waterTankGauge1.Size = new System.Drawing.Size(170, 197);
            this.waterTankGauge1.TabIndex = 4;
            this.waterTankGauge1.Value = 0;
            // 
            // panelTemp
            // 
            this.panelTemp.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.panelTemp.Controls.Add(this.label11);
            this.panelTemp.Controls.Add(this.label9);
            this.panelTemp.Controls.Add(this.lblTempValue);
            this.panelTemp.Controls.Add(this.lblTempTitle);
            this.panelTemp.Controls.Add(this.progressTemp);
            this.panelTemp.Location = new System.Drawing.Point(305, 28);
            this.panelTemp.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelTemp.Name = "panelTemp";
            this.panelTemp.Size = new System.Drawing.Size(105, 176);
            this.panelTemp.TabIndex = 3;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Depth = 0;
            this.label11.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label11.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label11.Location = new System.Drawing.Point(25, 118);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(7, 17);
            this.label11.TabIndex = 6;
            this.label11.Text = "0";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Depth = 0;
            this.label9.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label9.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label9.Location = new System.Drawing.Point(11, 17);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(21, 17);
            this.label9.TabIndex = 5;
            this.label9.Text = "100";
            // 
            // lblTempValue
            // 
            this.lblTempValue.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblTempValue.Font = new System.Drawing.Font("Segoe UI Black", 12F, System.Drawing.FontStyle.Bold);
            this.lblTempValue.Location = new System.Drawing.Point(0, 132);
            this.lblTempValue.Name = "lblTempValue";
            this.lblTempValue.Size = new System.Drawing.Size(105, 22);
            this.lblTempValue.TabIndex = 2;
            this.lblTempValue.Text = "0 °C";
            this.lblTempValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTempTitle
            // 
            this.lblTempTitle.Depth = 0;
            this.lblTempTitle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblTempTitle.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblTempTitle.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.lblTempTitle.Location = new System.Drawing.Point(0, 154);
            this.lblTempTitle.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblTempTitle.Name = "lblTempTitle";
            this.lblTempTitle.Size = new System.Drawing.Size(105, 22);
            this.lblTempTitle.TabIndex = 1;
            this.lblTempTitle.Text = "Temperature";
            this.lblTempTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressTemp
            // 
            this.progressTemp.Location = new System.Drawing.Point(39, 17);
            this.progressTemp.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.progressTemp.Name = "progressTemp";
            this.progressTemp.Size = new System.Drawing.Size(26, 113);
            this.progressTemp.TabIndex = 0;
            // 
            // gaugeRpm
            // 
            this.gaugeRpm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.gaugeRpm.AnimationFunction = WinFormAnimation.KnownAnimationFunctions.Liner;
            this.gaugeRpm.AnimationSpeed = 500;
            this.gaugeRpm.BackColor = System.Drawing.Color.Transparent;
            this.gaugeRpm.Font = new System.Drawing.Font("Segoe UI Black", 12F, System.Drawing.FontStyle.Bold);
            this.gaugeRpm.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.gaugeRpm.InnerColor = System.Drawing.Color.Transparent;
            this.gaugeRpm.InnerMargin = 2;
            this.gaugeRpm.InnerWidth = -1;
            this.gaugeRpm.Location = new System.Drawing.Point(24, 42);
            this.gaugeRpm.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gaugeRpm.MarqueeAnimationSpeed = 2000;
            this.gaugeRpm.Maximum = 500;
            this.gaugeRpm.Name = "gaugeRpm";
            this.gaugeRpm.OuterColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.gaugeRpm.OuterMargin = -25;
            this.gaugeRpm.OuterWidth = 26;
            this.gaugeRpm.ProgressColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.gaugeRpm.ProgressWidth = 18;
            this.gaugeRpm.SecondaryFont = new System.Drawing.Font("Segoe UI", 10F);
            this.gaugeRpm.Size = new System.Drawing.Size(159, 148);
            this.gaugeRpm.StartAngle = 135;
            this.gaugeRpm.SubscriptColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.gaugeRpm.SubscriptMargin = new System.Windows.Forms.Padding(-6, 0, 0, 0);
            this.gaugeRpm.SubscriptText = "RPM";
            this.gaugeRpm.SuperscriptColor = System.Drawing.Color.FromArgb(((int)(((byte)(166)))), ((int)(((byte)(166)))), ((int)(((byte)(166)))));
            this.gaugeRpm.SuperscriptMargin = new System.Windows.Forms.Padding(0, -35, 50, 0);
            this.gaugeRpm.SuperscriptText = "";
            this.gaugeRpm.TabIndex = 0;
            this.gaugeRpm.Text = "0";
            this.gaugeRpm.TextMargin = new System.Windows.Forms.Padding(7, 20, 0, 0);
            this.gaugeRpm.Value = 68;
            // 
            // pnlInfo
            // 
            this.pnlInfo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom;
            this.pnlInfo.Controls.Add(this.lblSiparisNo);
            this.pnlInfo.Controls.Add(this.label6);
            this.pnlInfo.Controls.Add(this.lblBatchNo);
            this.pnlInfo.Controls.Add(this.label5);
            this.pnlInfo.Controls.Add(this.lblMusteriNo);
            this.pnlInfo.Controls.Add(this.label4);
            this.pnlInfo.Controls.Add(this.lblOperator);
            this.pnlInfo.Controls.Add(this.label3);
            this.pnlInfo.Controls.Add(this.lblReceteAdi);
            this.pnlInfo.Controls.Add(this.label2);
            this.pnlInfo.Controls.Add(this.label1);
            this.pnlInfo.Location = new System.Drawing.Point(0, 0);
            this.pnlInfo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Size = new System.Drawing.Size(360, 231);
            this.pnlInfo.TabIndex = 0;
            // 
            // lblSiparisNo
            // 
            this.lblSiparisNo.Depth = 0;
            this.lblSiparisNo.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblSiparisNo.FontType = MaterialSkin.MaterialSkinManager.fontType.Body1;
            this.lblSiparisNo.Location = new System.Drawing.Point(128, 161);
            this.lblSiparisNo.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblSiparisNo.Name = "lblSiparisNo";
            this.lblSiparisNo.Size = new System.Drawing.Size(175, 19);
            this.lblSiparisNo.TabIndex = 21;
            this.lblSiparisNo.Text = "---";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Depth = 0;
            this.label6.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label6.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label6.Location = new System.Drawing.Point(10, 162);
            this.label6.MouseState = MaterialSkin.MouseState.HOVER;
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 17);
            this.label6.TabIndex = 20;
            this.label6.Text = "Order No:";
            // 
            // lblBatchNo
            // 
            this.lblBatchNo.Depth = 0;
            this.lblBatchNo.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblBatchNo.FontType = MaterialSkin.MaterialSkinManager.fontType.Body1;
            this.lblBatchNo.Location = new System.Drawing.Point(128, 137);
            this.lblBatchNo.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblBatchNo.Name = "lblBatchNo";
            this.lblBatchNo.Size = new System.Drawing.Size(175, 19);
            this.lblBatchNo.TabIndex = 19;
            this.lblBatchNo.Text = "---";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Depth = 0;
            this.label5.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label5.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label5.Location = new System.Drawing.Point(10, 139);
            this.label5.MouseState = MaterialSkin.MouseState.HOVER;
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 17);
            this.label5.TabIndex = 18;
            this.label5.Text = "Batch No:";
            // 
            // lblMusteriNo
            // 
            this.lblMusteriNo.Depth = 0;
            this.lblMusteriNo.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblMusteriNo.FontType = MaterialSkin.MaterialSkinManager.fontType.Body1;
            this.lblMusteriNo.Location = new System.Drawing.Point(128, 114);
            this.lblMusteriNo.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblMusteriNo.Name = "lblMusteriNo";
            this.lblMusteriNo.Size = new System.Drawing.Size(175, 19);
            this.lblMusteriNo.TabIndex = 17;
            this.lblMusteriNo.Text = "---";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Depth = 0;
            this.label4.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label4.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label4.Location = new System.Drawing.Point(10, 115);
            this.label4.MouseState = MaterialSkin.MouseState.HOVER;
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(117, 17);
            this.label4.TabIndex = 16;
            this.label4.Text = "Customer Number:";
            // 
            // lblOperator
            // 
            this.lblOperator.Depth = 0;
            this.lblOperator.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblOperator.FontType = MaterialSkin.MaterialSkinManager.fontType.Body1;
            this.lblOperator.Location = new System.Drawing.Point(128, 91);
            this.lblOperator.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblOperator.Name = "lblOperator";
            this.lblOperator.Size = new System.Drawing.Size(175, 19);
            this.lblOperator.TabIndex = 15;
            this.lblOperator.Text = "---";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Depth = 0;
            this.label3.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label3.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label3.Location = new System.Drawing.Point(10, 92);
            this.label3.MouseState = MaterialSkin.MouseState.HOVER;
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 17);
            this.label3.TabIndex = 14;
            this.label3.Text = "Operator:";
            // 
            // lblReceteAdi
            // 
            this.lblReceteAdi.Depth = 0;
            this.lblReceteAdi.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblReceteAdi.FontType = MaterialSkin.MaterialSkinManager.fontType.Body1;
            this.lblReceteAdi.Location = new System.Drawing.Point(128, 67);
            this.lblReceteAdi.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblReceteAdi.Name = "lblReceteAdi";
            this.lblReceteAdi.Size = new System.Drawing.Size(175, 19);
            this.lblReceteAdi.TabIndex = 13;
            this.lblReceteAdi.Text = "---";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Depth = 0;
            this.label2.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label2.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label2.Location = new System.Drawing.Point(10, 69);
            this.label2.MouseState = MaterialSkin.MouseState.HOVER;
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 17);
            this.label2.TabIndex = 12;
            this.label2.Text = "Recipe Name:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Depth = 0;
            this.label1.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.label1.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle1;
            this.label1.Location = new System.Drawing.Point(7, 32);
            this.label1.MouseState = MaterialSkin.MouseState.HOVER;
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(150, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Machine Information";
            // 
            // pnlTimeline
            // 
            this.pnlTimeline.Controls.Add(this.tblPlots);
            this.pnlTimeline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTimeline.Location = new System.Drawing.Point(3, 241);
            this.pnlTimeline.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlTimeline.Name = "pnlTimeline";
            this.pnlTimeline.Size = new System.Drawing.Size(963, 512);
            this.pnlTimeline.TabIndex = 1;
            // 
            // tblPlots
            // 
            this.tblPlots.ColumnCount = 1;
            this.tblPlots.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblPlots.Controls.Add(this.formsPlotTemp, 0, 0);
            this.tblPlots.Controls.Add(this.formsPlotRpm, 0, 1);
            this.tblPlots.Controls.Add(this.formsPlotWater, 0, 2);
            this.tblPlots.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblPlots.Location = new System.Drawing.Point(0, 0);
            this.tblPlots.Name = "tblPlots";
            this.tblPlots.RowCount = 3;
            this.tblPlots.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tblPlots.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tblPlots.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tblPlots.Size = new System.Drawing.Size(963, 512);
            this.tblPlots.TabIndex = 0;
            // 
            // formsPlotTemp
            // 
            this.formsPlotTemp.DisplayScale = 1F;
            this.formsPlotTemp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsPlotTemp.Location = new System.Drawing.Point(3, 3);
            this.formsPlotTemp.Name = "formsPlotTemp";
            this.formsPlotTemp.Size = new System.Drawing.Size(957, 164);
            this.formsPlotTemp.TabIndex = 0;
            // 
            // formsPlotRpm
            // 
            this.formsPlotRpm.DisplayScale = 1F;
            this.formsPlotRpm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsPlotRpm.Location = new System.Drawing.Point(3, 173);
            this.formsPlotRpm.Name = "formsPlotRpm";
            this.formsPlotRpm.Size = new System.Drawing.Size(957, 164);
            this.formsPlotRpm.TabIndex = 1;
            // 
            // formsPlotWater
            // 
            this.formsPlotWater.DisplayScale = 1F;
            this.formsPlotWater.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsPlotWater.Location = new System.Drawing.Point(3, 343);
            this.formsPlotWater.Name = "formsPlotWater";
            this.formsPlotWater.Size = new System.Drawing.Size(957, 166);
            this.formsPlotWater.TabIndex = 2;
            // 
            // pnlAlarmsAndSteps
            // 
            this.pnlAlarmsAndSteps.BackColor = System.Drawing.Color.Transparent;
            this.pnlAlarmsAndSteps.Controls.Add(this.dgvAdimlar);
            this.pnlAlarmsAndSteps.Controls.Add(this.label10);
            this.pnlAlarmsAndSteps.Controls.Add(this.lblCalisanAdim);
            this.pnlAlarmsAndSteps.Controls.Add(this.label8);
            this.pnlAlarmsAndSteps.Controls.Add(this.lstAlarmlar);
            this.pnlAlarmsAndSteps.Controls.Add(this.label7);
            this.pnlAlarmsAndSteps.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlAlarmsAndSteps.Location = new System.Drawing.Point(969, 0);
            this.pnlAlarmsAndSteps.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlAlarmsAndSteps.Name = "pnlAlarmsAndSteps";
            this.pnlAlarmsAndSteps.Size = new System.Drawing.Size(389, 755);
            this.pnlAlarmsAndSteps.TabIndex = 5;
            this.pnlAlarmsAndSteps.TabStop = true;
            // 
            // dgvAdimlar
            // 
            this.dgvAdimlar.AllowUserToAddRows = false;
            this.dgvAdimlar.AllowUserToDeleteRows = false;
            this.dgvAdimlar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvAdimlar.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAdimlar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAdimlar.Location = new System.Drawing.Point(49, 87);
            this.dgvAdimlar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvAdimlar.Name = "dgvAdimlar";
            this.dgvAdimlar.ReadOnly = true;
            this.dgvAdimlar.RowHeadersVisible = false;
            this.dgvAdimlar.RowHeadersWidth = 51;
            this.dgvAdimlar.RowTemplate.Height = 24;
            this.dgvAdimlar.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAdimlar.Size = new System.Drawing.Size(291, 305);
            this.dgvAdimlar.TabIndex = 5;
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.AutoSize = true;
            this.label10.Depth = 0;
            this.label10.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label10.FontType = MaterialSkin.MaterialSkinManager.fontType.Body1;
            this.label10.Location = new System.Drawing.Point(136, 67);
            this.label10.MouseState = MaterialSkin.MouseState.HOVER;
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(94, 19);
            this.label10.TabIndex = 4;
            this.label10.Text = "Recipe Steps";
            // 
            // lblCalisanAdim
            // 
            this.lblCalisanAdim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCalisanAdim.Depth = 0;
            this.lblCalisanAdim.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblCalisanAdim.FontType = MaterialSkin.MaterialSkinManager.fontType.Body1;
            this.lblCalisanAdim.Location = new System.Drawing.Point(49, 31);
            this.lblCalisanAdim.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblCalisanAdim.Name = "lblCalisanAdim";
            this.lblCalisanAdim.Size = new System.Drawing.Size(291, 24);
            this.lblCalisanAdim.TabIndex = 3;
            this.lblCalisanAdim.Text = "---";
            this.lblCalisanAdim.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Depth = 0;
            this.label8.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label8.FontType = MaterialSkin.MaterialSkinManager.fontType.Body1;
            this.label8.Location = new System.Drawing.Point(146, 11);
            this.label8.MouseState = MaterialSkin.MouseState.HOVER;
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(97, 19);
            this.label8.TabIndex = 2;
            this.label8.Text = "Running Step";
            // 
            // lstAlarmlar
            // 
            this.lstAlarmlar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lstAlarmlar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(52)))), ((int)(((byte)(64))))); // Mat koyu material tonu
            this.lstAlarmlar.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lstAlarmlar.ForeColor = System.Drawing.Color.White;
            this.lstAlarmlar.FormattingEnabled = true;
            this.lstAlarmlar.ItemHeight = 15;
            this.lstAlarmlar.Location = new System.Drawing.Point(49, 424);
            this.lstAlarmlar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lstAlarmlar.Name = "lstAlarmlar";
            this.lstAlarmlar.Size = new System.Drawing.Size(291, 304);
            this.lstAlarmlar.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Depth = 0;
            this.label7.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label7.FontType = MaterialSkin.MaterialSkinManager.fontType.Body1;
            this.label7.Location = new System.Drawing.Point(133, 404);
            this.label7.MouseState = MaterialSkin.MouseState.HOVER;
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(133, 19);
            this.label7.TabIndex = 0;
            this.label7.Text = "Intra-Party Alarms";
            // 
            // MakineDetay_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent; // Panel transparanlığı mühürlendi
            this.Controls.Add(this.pnlMainContent);
            this.Controls.Add(this.pnlTop);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MakineDetay_Control";
            this.Size = new System.Drawing.Size(1358, 799);
            this.pnlTop.ResumeLayout(false);
            this.pnlMainContent.ResumeLayout(false);
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.pnlTopDashboard.ResumeLayout(false);
            this.tableLayoutPanelTop.ResumeLayout(false);
            this.pnlGaugesAndInfo.ResumeLayout(false);
            this.pnlGaugesAndInfo.PerformLayout();
            this.pnlGauges.ResumeLayout(false);
            this.pnlGauges.PerformLayout();
            this.humuditypanel.ResumeLayout(false);
            this.humuditypanel.PerformLayout();
            this.panelTemp.ResumeLayout(false);
            this.panelTemp.PerformLayout();
            this.pnlInfo.ResumeLayout(false);
            this.pnlInfo.PerformLayout();
            this.pnlTimeline.ResumeLayout(false);
            this.tblPlots.ResumeLayout(false);
            this.pnlAlarmsAndSteps.ResumeLayout(false);
            this.pnlAlarmsAndSteps.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAdimlar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private MaterialSkin.Controls.MaterialButton btnGeri;                  // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel lblMakineAdi;              // Tür güncellendi
        private System.Windows.Forms.Panel pnlMainContent;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.Panel pnlTopDashboard;
        private System.Windows.Forms.Panel pnlTimeline;
        private System.Windows.Forms.TableLayoutPanel tblPlots;
        private ScottPlot.WinForms.FormsPlot formsPlotTemp;
        private ScottPlot.WinForms.FormsPlot formsPlotRpm;
        private ScottPlot.WinForms.FormsPlot formsPlotWater;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTop;
        private System.Windows.Forms.Panel pnlGaugesAndInfo;
        private System.Windows.Forms.Panel pnlInfo;
        private MaterialSkin.Controls.MaterialLabel label1;                     // Tür güncellendi
        private System.Windows.Forms.Panel pnlGauges;
        private MaterialSkin.Controls.MaterialLabel lblSiparisNo;               // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel label6;                     // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel lblBatchNo;                 // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel label5;                     // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel lblMusteriNo;               // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel label4;                     // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel lblOperator;                // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel label3;                     // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel lblReceteAdi;               // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel label2;                     // Tür güncellendi
        private CircularProgressBar.CircularProgressBar gaugeRpm;
        private System.Windows.Forms.Panel panelTemp;
        private System.Windows.Forms.Panel progressTemp;
        private System.Windows.Forms.Label lblTempValue;
        private MaterialSkin.Controls.MaterialLabel lblTempTitle;               // Tür güncellendi
        private UI.Controls.WaterTankGauge waterTankGauge1;
        private System.Windows.Forms.Panel pnlAlarmsAndSteps;
        private System.Windows.Forms.DataGridView dgvAdimlar;
        private MaterialSkin.Controls.MaterialLabel label10;                    // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel lblCalisanAdim;              // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel label8;                     // Tür güncellendi
        private System.Windows.Forms.ListBox lstAlarmlar;
        private MaterialSkin.Controls.MaterialLabel label7;                     // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel label11;                    // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel label9;                     // Tür güncellendi
        private System.Windows.Forms.Panel humuditypanel;
        private MaterialSkin.Controls.MaterialLabel label12;                    // Tür güncellendi
        private MaterialSkin.Controls.MaterialLabel label13;                    // Tür güncellendi
        private System.Windows.Forms.Label humuditytxt;
        private MaterialSkin.Controls.MaterialLabel lblhumudity;                // Tür güncellendi
        private System.Windows.Forms.Panel humuditybar;
    }
}