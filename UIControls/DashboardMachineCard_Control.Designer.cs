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
            pictureBox1 = new PictureBox();
            btnSendToPlc = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pnlStatusIndicator
            // 
            pnlStatusIndicator.BackColor = Color.SlateGray;
            pnlStatusIndicator.Dock = DockStyle.Left;
            pnlStatusIndicator.Location = new Point(0, 0);
            pnlStatusIndicator.Margin = new Padding(4, 3, 4, 3);
            pnlStatusIndicator.Name = "pnlStatusIndicator";
            pnlStatusIndicator.Size = new Size(12, 150);
            pnlStatusIndicator.TabIndex = 0;
            // 
            // lblMachineName
            // 
            lblMachineName.AutoSize = true;
            lblMachineName.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblMachineName.Location = new Point(23, 10);
            lblMachineName.Margin = new Padding(4, 0, 4, 0);
            lblMachineName.Name = "lblMachineName";
            lblMachineName.Size = new Size(97, 21);
            lblMachineName.TabIndex = 1;
            lblMachineName.Text = "Vinç No: 01";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Arial Black", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 162);
            lblStatus.Location = new Point(24, 112);
            lblStatus.Margin = new Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(96, 18);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "BEKLEMEDE";
            lblStatus.Click += lblStatus_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resource1.Gemini_Generated_Image_ldt511ldt511ldt5;
            pictureBox1.InitialImage = Properties.Resource1.Gemini_Generated_Image_ldt511ldt511ldt5;
            pictureBox1.Location = new Point(332, 44);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(83, 54);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 19;
            pictureBox1.TabStop = false;
            // 
            // btnSendToPlc
            // 
            btnSendToPlc.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 162);
            btnSendToPlc.Location = new Point(23, 44);
            btnSendToPlc.Name = "btnSendToPlc";
            btnSendToPlc.Size = new Size(303, 54);
            btnSendToPlc.TabIndex = 20;
            btnSendToPlc.Text = "SÜRÜCÜ ALARM\r\nRESET";
            btnSendToPlc.UseVisualStyleBackColor = true;
           
            // 
            // DashboardMachineCard_Control
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.WhiteSmoke;
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(btnSendToPlc);
            Controls.Add(pictureBox1);
            Controls.Add(lblStatus);
            Controls.Add(lblMachineName);
            Controls.Add(pnlStatusIndicator);
            Margin = new Padding(9);
            Name = "DashboardMachineCard_Control";
            Size = new Size(418, 150);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Panel pnlStatusIndicator;
        private System.Windows.Forms.Label lblMachineName;
        private System.Windows.Forms.Label lblStatus;
        private PictureBox pictureBox1;
        private Button btnSendToPlc;
    }
}