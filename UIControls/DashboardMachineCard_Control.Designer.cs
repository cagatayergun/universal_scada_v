// UIControls/DashboardMachineCard_Control.Designer.cs
namespace TekstilScada.UI.Controls
{
    partial class DashboardMachineCard_Control
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && (components != null)) { components.Dispose(); } base.Dispose(disposing); }

        #region Component Designer generated code
        private void InitializeComponent()
        {
            pnlStatusIndicator = new Panel();
            lblMachineName = new Label();
            lblStatus = new Label();
            lblRecipeName = new Label();
            lblBatchId = new Label();
            lblTemperature = new Label();
            gaugeRpm = new CircularProgressBar.CircularProgressBar();
            progressBar = new ProgressBar();
            lblPercentage = new Label();
            lblProcessing = new Label();
            lblHumidity = new Label();
            lblhumudity = new Label();
            label2 = new Label();
            SuspendLayout();
            // 
            // pnlStatusIndicator
            // 
            pnlStatusIndicator.BackColor = Color.SlateGray;
            pnlStatusIndicator.Dock = DockStyle.Left;
            pnlStatusIndicator.Location = new Point(0, 0);
            pnlStatusIndicator.Margin = new Padding(4, 3, 4, 3);
            pnlStatusIndicator.Name = "pnlStatusIndicator";
            pnlStatusIndicator.Size = new Size(12, 197);
            pnlStatusIndicator.TabIndex = 0;
            // 
            // lblMachineName
            // 
            lblMachineName.AutoSize = true;
            lblMachineName.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblMachineName.Location = new Point(23, 5);
            lblMachineName.Margin = new Padding(4, 0, 4, 0);
            lblMachineName.Name = "lblMachineName";
            lblMachineName.Size = new Size(97, 21);
            lblMachineName.TabIndex = 1;
            lblMachineName.Text = "Makine Adı";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Arial Black", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 162);
            lblStatus.Location = new Point(23, 153);
            lblStatus.Margin = new Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(80, 18);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "DURUYOR";
            // 
            // lblRecipeName
            // 
            lblRecipeName.Font = new Font("Segoe UI Black", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 162);
            lblRecipeName.Location = new Point(23, 34);
            lblRecipeName.Margin = new Padding(4, 0, 4, 0);
            lblRecipeName.Name = "lblRecipeName";
            lblRecipeName.Size = new Size(131, 21);
            lblRecipeName.TabIndex = 4;
            lblRecipeName.Text = "Reçete: -";
            lblRecipeName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblBatchId
            // 
            lblBatchId.Font = new Font("Segoe UI Black", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 162);
            lblBatchId.Location = new Point(23, 53);
            lblBatchId.Margin = new Padding(4, 0, 4, 0);
            lblBatchId.Name = "lblBatchId";
            lblBatchId.Size = new Size(131, 21);
            lblBatchId.TabIndex = 8;
            lblBatchId.Text = "Parti: -";
            lblBatchId.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTemperature
            // 
            lblTemperature.Font = new Font("Arial Black", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTemperature.ForeColor = Color.FromArgb(192, 0, 0);
            lblTemperature.Location = new Point(100, 73);
            lblTemperature.Margin = new Padding(4, 0, 4, 0);
            lblTemperature.Name = "lblTemperature";
            lblTemperature.Size = new Size(98, 33);
            lblTemperature.TabIndex = 11;
            lblTemperature.Text = "25 °C";
            // 
            // gaugeRpm
            // 
            gaugeRpm.AnimationFunction = WinFormAnimation.KnownAnimationFunctions.Liner;
            gaugeRpm.AnimationSpeed = 500;
            gaugeRpm.BackColor = Color.Transparent;
            gaugeRpm.Font = new Font("Segoe UI Black", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 162);
            gaugeRpm.ForeColor = Color.FromArgb(64, 64, 64);
            gaugeRpm.InnerColor = Color.White;
            gaugeRpm.InnerMargin = 2;
            gaugeRpm.InnerWidth = -1;
            gaugeRpm.Location = new Point(198, 56);
            gaugeRpm.Margin = new Padding(3, 2, 3, 2);
            gaugeRpm.MarqueeAnimationSpeed = 2000;
            gaugeRpm.Maximum = 500;
            gaugeRpm.Name = "gaugeRpm";
            gaugeRpm.OuterColor = Color.FromArgb(224, 224, 224);
            gaugeRpm.OuterMargin = -25;
            gaugeRpm.OuterWidth = 26;
            gaugeRpm.ProgressColor = Color.FromArgb(46, 204, 113);
            gaugeRpm.ProgressWidth = 18;
            gaugeRpm.SecondaryFont = new Font("Segoe UI Black", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 162);
            gaugeRpm.Size = new Size(78, 68);
            gaugeRpm.StartAngle = 135;
            gaugeRpm.SubscriptColor = Color.FromArgb(100, 100, 100);
            gaugeRpm.SubscriptMargin = new Padding(-4, -37, 0, 0);
            gaugeRpm.SubscriptText = "RPM";
            gaugeRpm.SuperscriptColor = Color.FromArgb(166, 166, 166);
            gaugeRpm.SuperscriptMargin = new Padding(0, 0, 50, 0);
            gaugeRpm.SuperscriptText = "";
            gaugeRpm.TabIndex = 12;
            gaugeRpm.Text = "0";
            gaugeRpm.TextMargin = new Padding(7, 25, 0, 0);
            gaugeRpm.Value = 68;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(111, 180);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(135, 10);
            progressBar.TabIndex = 13;
            // 
            // lblPercentage
            // 
            lblPercentage.AutoSize = true;
            lblPercentage.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPercentage.Location = new Point(249, 177);
            lblPercentage.Name = "lblPercentage";
            lblPercentage.Size = new Size(27, 15);
            lblPercentage.TabIndex = 14;
            lblPercentage.Text = "0 %";
            // 
            // lblProcessing
            // 
            lblProcessing.AutoSize = true;
            lblProcessing.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblProcessing.ForeColor = Color.Black;
            lblProcessing.Location = new Point(23, 177);
            lblProcessing.Name = "lblProcessing";
            lblProcessing.Size = new Size(80, 15);
            lblProcessing.TabIndex = 15;
            lblProcessing.Text = "PROSESSING";
            // 
            // lblHumidity
            // 
            lblHumidity.AutoSize = true;
            lblHumidity.Font = new Font("Arial Black", 20.25F, FontStyle.Bold);
            lblHumidity.ForeColor = Color.CornflowerBlue;
            lblHumidity.Location = new Point(100, 107);
            lblHumidity.Name = "lblHumidity";
            lblHumidity.Size = new Size(89, 38);
            lblHumidity.TabIndex = 16;
            lblHumidity.Text = "25 %";
            lblHumidity.Visible = false;
            // 
            // lblhumudity
            // 
            lblhumudity.Font = new Font("Segoe UI Black", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 162);
            lblhumudity.Location = new Point(20, 116);
            lblhumudity.Margin = new Padding(4, 0, 4, 0);
            lblhumudity.Name = "lblhumudity";
            lblhumudity.Size = new Size(69, 21);
            lblhumudity.TabIndex = 18;
            lblhumudity.Text = "Humidity:";
            lblhumudity.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            label2.Font = new Font("Segoe UI Black", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 162);
            label2.Location = new Point(20, 82);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(83, 21);
            label2.TabIndex = 17;
            label2.Text = "Temprature:";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // DashboardMachineCard_Control
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Info;
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(lblhumudity);
            Controls.Add(label2);
            Controls.Add(lblHumidity);
            Controls.Add(lblProcessing);
            Controls.Add(lblPercentage);
            Controls.Add(progressBar);
            Controls.Add(gaugeRpm);
            Controls.Add(lblTemperature);
            Controls.Add(lblBatchId);
            Controls.Add(lblRecipeName);
            Controls.Add(lblStatus);
            Controls.Add(lblMachineName);
            Controls.Add(pnlStatusIndicator);
            Margin = new Padding(9);
            Name = "DashboardMachineCard_Control";
            Size = new Size(293, 197);
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion
        private System.Windows.Forms.Panel pnlStatusIndicator;
        private System.Windows.Forms.Label lblMachineName;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblRecipeName;
        private System.Windows.Forms.Label lblBatchId;
        private System.Windows.Forms.Label lblTemperature;
        private CircularProgressBar.CircularProgressBar gaugeRpm;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblPercentage;
        private Label lblProcessing;
        private Label lblHumidity; // YENİ
        private Label lblhumudity;
        private Label label2;
    }
}