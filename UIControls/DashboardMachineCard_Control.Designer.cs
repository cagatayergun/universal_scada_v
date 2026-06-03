// UI/Controls/DashboardMachineCard_Control.Designer.cs
namespace Telemetry.UI.Controls
{
    partial class DashboardMachineCard_Control
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
            this.materialCard1 = new MaterialSkin.Controls.MaterialCard();
            this.pnlGaugeWrapper = new System.Windows.Forms.Panel(); // YENİ: KORUMA PANELİ (İZOLASYON)
            this.pnlStatusIndicator = new System.Windows.Forms.Panel();
            this.lblMachineName = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblRecipeName = new System.Windows.Forms.Label();
            this.lblBatchId = new System.Windows.Forms.Label();
            this.lblTemperature = new System.Windows.Forms.Label();
            this.gaugeRpm = new CircularProgressBar.CircularProgressBar();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblPercentage = new System.Windows.Forms.Label();
            this.lblProcessing = new System.Windows.Forms.Label();
            this.lblHumidity = new System.Windows.Forms.Label();
            this.lblhumudity = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.materialCard1.SuspendLayout();
            this.pnlGaugeWrapper.SuspendLayout();
            this.SuspendLayout();
            // 
            // materialCard1
            // 
            this.materialCard1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(52)))), ((int)(((byte)(64)))));
            this.materialCard1.Controls.Add(this.lblhumudity);
            this.materialCard1.Controls.Add(this.label2);
            this.materialCard1.Controls.Add(this.lblHumidity);
            this.materialCard1.Controls.Add(this.lblProcessing);
            this.materialCard1.Controls.Add(this.lblPercentage);
            this.materialCard1.Controls.Add(this.progressBar);
            this.materialCard1.Controls.Add(this.pnlGaugeWrapper); // gaugeRpm YERİNE KORUMA PANELİ EKLENDİ
            this.materialCard1.Controls.Add(this.lblTemperature);
            this.materialCard1.Controls.Add(this.lblBatchId);
            this.materialCard1.Controls.Add(this.lblRecipeName);
            this.materialCard1.Controls.Add(this.lblStatus);
            this.materialCard1.Controls.Add(this.lblMachineName);
            this.materialCard1.Controls.Add(this.pnlStatusIndicator);
            this.materialCard1.Depth = 1;
            this.materialCard1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.materialCard1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialCard1.Location = new System.Drawing.Point(0, 0);
            this.materialCard1.Margin = new System.Windows.Forms.Padding(14);
            this.materialCard1.MouseState = MaterialSkin.MouseState.OUT;
            this.materialCard1.Name = "materialCard1";
            this.materialCard1.Padding = new System.Windows.Forms.Padding(14);
            this.materialCard1.Size = new System.Drawing.Size(293, 197);
            this.materialCard1.TabIndex = 0;
            // 
            // pnlGaugeWrapper
            // 
            this.pnlGaugeWrapper.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(52)))), ((int)(((byte)(64)))));
            this.pnlGaugeWrapper.Controls.Add(this.gaugeRpm);
            this.pnlGaugeWrapper.Location = new System.Drawing.Point(202, 54);
            this.pnlGaugeWrapper.Name = "pnlGaugeWrapper";
            this.pnlGaugeWrapper.Size = new System.Drawing.Size(76, 72);
            this.pnlGaugeWrapper.TabIndex = 20;
            // 
            // pnlStatusIndicator
            // 
            this.pnlStatusIndicator.BackColor = System.Drawing.Color.SlateGray;
            this.pnlStatusIndicator.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlStatusIndicator.Location = new System.Drawing.Point(14, 14);
            this.pnlStatusIndicator.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pnlStatusIndicator.Name = "pnlStatusIndicator";
            this.pnlStatusIndicator.Size = new System.Drawing.Size(8, 169);
            this.pnlStatusIndicator.TabIndex = 0;
            // 
            // lblMachineName
            // 
            this.lblMachineName.AutoSize = true;
            this.lblMachineName.BackColor = System.Drawing.Color.Transparent;
            this.lblMachineName.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.lblMachineName.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblMachineName.Location = new System.Drawing.Point(32, 12);
            this.lblMachineName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMachineName.Name = "lblMachineName";
            this.lblMachineName.Size = new System.Drawing.Size(92, 21);
            this.lblMachineName.TabIndex = 1;
            this.lblMachineName.Text = "Machine";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI Black", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.LightGray;
            this.lblStatus.Location = new System.Drawing.Point(32, 148);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(50, 17);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "STOPS";
            // 
            // lblRecipeName
            // 
            this.lblRecipeName.BackColor = System.Drawing.Color.Transparent;
            this.lblRecipeName.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblRecipeName.ForeColor = System.Drawing.Color.Silver;
            this.lblRecipeName.Location = new System.Drawing.Point(32, 36);
            this.lblRecipeName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRecipeName.Name = "lblRecipeName";
            this.lblRecipeName.Size = new System.Drawing.Size(145, 21);
            this.lblRecipeName.TabIndex = 4;
            this.lblRecipeName.Text = "Recipe: -";
            this.lblRecipeName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBatchId
            // 
            this.lblBatchId.BackColor = System.Drawing.Color.Transparent;
            this.lblBatchId.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblBatchId.ForeColor = System.Drawing.Color.Silver;
            this.lblBatchId.Location = new System.Drawing.Point(32, 55);
            this.lblBatchId.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBatchId.Name = "lblBatchId";
            this.lblBatchId.Size = new System.Drawing.Size(145, 21);
            this.lblBatchId.TabIndex = 8;
            this.lblBatchId.Text = "Party: -";
            this.lblBatchId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTemperature
            // 
            this.lblTemperature.BackColor = System.Drawing.Color.Transparent;
            this.lblTemperature.Font = new System.Drawing.Font("Segoe UI Black", 18F, System.Drawing.FontStyle.Bold);
            this.lblTemperature.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(83)))), ((int)(((byte)(80)))));
            this.lblTemperature.Location = new System.Drawing.Point(103, 76);
            this.lblTemperature.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTemperature.Name = "lblTemperature";
            this.lblTemperature.Size = new System.Drawing.Size(95, 33);
            this.lblTemperature.TabIndex = 11;
            this.lblTemperature.Text = "0 °C";
            this.lblTemperature.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gaugeRpm
            // 
            this.gaugeRpm.AnimationFunction = WinFormAnimation.KnownAnimationFunctions.Liner;
            this.gaugeRpm.AnimationSpeed = 500;
            this.gaugeRpm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(52)))), ((int)(((byte)(64)))));
            this.gaugeRpm.Dock = System.Windows.Forms.DockStyle.Fill; // İzolasyon panelini doldur
            this.gaugeRpm.Font = new System.Drawing.Font("Segoe UI Black", 8F, System.Drawing.FontStyle.Bold);
            this.gaugeRpm.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.gaugeRpm.InnerColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(36)))), ((int)(((byte)(48)))));
            this.gaugeRpm.InnerMargin = 2;
            this.gaugeRpm.InnerWidth = -1;
            this.gaugeRpm.Location = new System.Drawing.Point(0, 0);
            this.gaugeRpm.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gaugeRpm.MarqueeAnimationSpeed = 2000;
            this.gaugeRpm.Maximum = 500;
            this.gaugeRpm.Name = "gaugeRpm";
            this.gaugeRpm.OuterColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(71)))), ((int)(((byte)(79)))));
            this.gaugeRpm.OuterMargin = -25;
            this.gaugeRpm.OuterWidth = 26;
            this.gaugeRpm.ProgressColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.gaugeRpm.ProgressWidth = 16;
            this.gaugeRpm.SecondaryFont = new System.Drawing.Font("Segoe UI Black", 8F, System.Drawing.FontStyle.Bold);
            this.gaugeRpm.Size = new System.Drawing.Size(76, 72);
            this.gaugeRpm.StartAngle = 135;
            this.gaugeRpm.SubscriptColor = System.Drawing.Color.FromArgb(((int)(((byte)(144)))), ((int)(((byte)(164)))), ((int)(((byte)(174)))));
            this.gaugeRpm.SubscriptMargin = new System.Windows.Forms.Padding(-2, -36, 0, 0);
            this.gaugeRpm.SubscriptText = "RPM";
            this.gaugeRpm.SuperscriptColor = System.Drawing.Color.FromArgb(((int)(((byte)(166)))), ((int)(((byte)(166)))), ((int)(((byte)(166)))));
            this.gaugeRpm.SuperscriptMargin = new System.Windows.Forms.Padding(0, 0, 50, 0);
            this.gaugeRpm.SuperscriptText = "";
            this.gaugeRpm.TabIndex = 12;
            this.gaugeRpm.Text = "0";
            this.gaugeRpm.TextMargin = new System.Windows.Forms.Padding(4, 24, 0, 0);
            this.gaugeRpm.Value = 0;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(111, 174);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(130, 8);
            this.progressBar.TabIndex = 13;
            // 
            // lblPercentage
            // 
            this.lblPercentage.AutoSize = true;
            this.lblPercentage.BackColor = System.Drawing.Color.Transparent;
            this.lblPercentage.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblPercentage.ForeColor = System.Drawing.Color.LightGray;
            this.lblPercentage.Location = new System.Drawing.Point(247, 170);
            this.lblPercentage.Name = "lblPercentage";
            this.lblPercentage.Size = new System.Drawing.Size(27, 15);
            this.lblPercentage.TabIndex = 14;
            this.lblPercentage.Text = "0 %";
            // 
            // lblProcessing
            // 
            this.lblProcessing.AutoSize = true;
            this.lblProcessing.BackColor = System.Drawing.Color.Transparent;
            this.lblProcessing.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblProcessing.ForeColor = System.Drawing.Color.Silver;
            this.lblProcessing.Location = new System.Drawing.Point(32, 170);
            this.lblProcessing.Name = "lblProcessing";
            this.lblProcessing.Size = new System.Drawing.Size(79, 15);
            this.lblProcessing.TabIndex = 15;
            this.lblProcessing.Text = "PROCESSING";
            // 
            // lblHumidity
            // 
            this.lblHumidity.BackColor = System.Drawing.Color.Transparent;
            this.lblHumidity.Font = new System.Drawing.Font("Segoe UI Black", 18F, System.Drawing.FontStyle.Bold);
            this.lblHumidity.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(181)))), ((int)(((byte)(246)))));
            this.lblHumidity.Location = new System.Drawing.Point(103, 110);
            this.lblHumidity.Name = "lblHumidity";
            this.lblHumidity.Size = new System.Drawing.Size(95, 33);
            this.lblHumidity.TabIndex = 16;
            this.lblHumidity.Text = "0 %";
            this.lblHumidity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblHumidity.Visible = false;
            // 
            // lblhumudity
            // 
            this.lblhumudity.BackColor = System.Drawing.Color.Transparent;
            this.lblhumudity.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblhumudity.ForeColor = System.Drawing.Color.Silver;
            this.lblhumudity.Location = new System.Drawing.Point(32, 116);
            this.lblhumudity.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblhumudity.Name = "lblhumudity";
            this.lblhumudity.Size = new System.Drawing.Size(69, 21);
            this.lblhumudity.TabIndex = 18;
            this.lblhumudity.Text = "Humidity:";
            this.lblhumudity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.Silver;
            this.label2.Location = new System.Drawing.Point(32, 82);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 21);
            this.label2.TabIndex = 17;
            this.label2.Text = "Temp:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // DashboardMachineCard_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(52)))), ((int)(((byte)(64)))));
            this.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Controls.Add(this.materialCard1);
            this.Margin = new System.Windows.Forms.Padding(9);
            this.Name = "DashboardMachineCard_Control";
            this.Size = new System.Drawing.Size(293, 197);
            this.materialCard1.ResumeLayout(false);
            this.materialCard1.PerformLayout();
            this.pnlGaugeWrapper.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private MaterialSkin.Controls.MaterialCard materialCard1;
        private System.Windows.Forms.Panel pnlGaugeWrapper; // YENİ
        private System.Windows.Forms.Panel pnlStatusIndicator;
        private System.Windows.Forms.Label lblMachineName;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblRecipeName;
        private System.Windows.Forms.Label lblBatchId;
        private System.Windows.Forms.Label lblTemperature;
        private CircularProgressBar.CircularProgressBar gaugeRpm;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblPercentage;
        private System.Windows.Forms.Label lblProcessing;
        private System.Windows.Forms.Label lblHumidity;
        private System.Windows.Forms.Label lblhumudity;
        private System.Windows.Forms.Label label2;
    }
}