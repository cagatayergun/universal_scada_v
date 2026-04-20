// UI/Controls/MachineCard_Control.Designer.cs
namespace TekstilScada.UI.Controls
{
    partial class MachineCard_Control
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
            pnlMain = new Panel();
            lblStatus = new Label();
            button1 = new Button();
            lblCraneNumber = new Label();
            picConnection = new PictureBox();
            btnInfo = new Button();
            picAlarm = new PictureBox();
            picPause = new PictureBox();
            picPlay = new PictureBox();
            btnVnc = new Button();
            pnlStatusIndicator = new Panel();
            pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picConnection).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picAlarm).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picPause).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picPlay).BeginInit();
            SuspendLayout();
            // 
            // pnlMain
            // 
            pnlMain.BackColor = Color.FromArgb(236, 240, 241);
            pnlMain.BorderStyle = BorderStyle.FixedSingle;
            pnlMain.Controls.Add(pnlStatusIndicator);
            pnlMain.Controls.Add(lblStatus);
            pnlMain.Controls.Add(button1);
            pnlMain.Controls.Add(lblCraneNumber);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 0);
            pnlMain.Margin = new Padding(3, 2, 3, 2);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(280, 180);
            pnlMain.TabIndex = 0;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Arial Black", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 162);
            lblStatus.Location = new Point(17, 135);
            lblStatus.Margin = new Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(96, 18);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "BEKLEMEDE";
            // 
            // button1
            // 
            button1.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 162);
            button1.Location = new Point(35, 66);
            button1.Name = "button1";
            button1.Size = new Size(209, 54);
            button1.TabIndex = 1;
            button1.Text = "SÜRÜCÜ ALARM\r\nRESET";
            button1.UseVisualStyleBackColor = true;
            // 
            // lblCraneNumber
            // 
            lblCraneNumber.AutoSize = true;
            lblCraneNumber.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold);
            lblCraneNumber.ForeColor = Color.FromArgb(44, 62, 80);
            lblCraneNumber.Location = new Point(17, 4);
            lblCraneNumber.Name = "lblCraneNumber";
            lblCraneNumber.Size = new Size(41, 30);
            lblCraneNumber.TabIndex = 0;
            lblCraneNumber.Text = "V1";
            // 
            // picConnection
            // 
            picConnection.Location = new Point(0, 0);
            picConnection.Name = "picConnection";
            picConnection.Size = new Size(100, 50);
            picConnection.TabIndex = 0;
            picConnection.TabStop = false;
            // 
            // btnInfo
            // 
            btnInfo.Location = new Point(0, 0);
            btnInfo.Name = "btnInfo";
            btnInfo.Size = new Size(75, 23);
            btnInfo.TabIndex = 0;
            // 
            // picAlarm
            // 
            picAlarm.Location = new Point(0, 0);
            picAlarm.Name = "picAlarm";
            picAlarm.Size = new Size(100, 50);
            picAlarm.TabIndex = 0;
            picAlarm.TabStop = false;
            // 
            // picPause
            // 
            picPause.Location = new Point(0, 0);
            picPause.Name = "picPause";
            picPause.Size = new Size(100, 50);
            picPause.TabIndex = 0;
            picPause.TabStop = false;
            // 
            // picPlay
            // 
            picPlay.Location = new Point(0, 0);
            picPlay.Name = "picPlay";
            picPlay.Size = new Size(100, 50);
            picPlay.TabIndex = 0;
            picPlay.TabStop = false;
            // 
            // btnVnc
            // 
            btnVnc.Location = new Point(0, 0);
            btnVnc.Name = "btnVnc";
            btnVnc.Size = new Size(75, 23);
            btnVnc.TabIndex = 0;
            // 
            // pnlStatusIndicator
            // 
            pnlStatusIndicator.BackColor = Color.SlateGray;
            pnlStatusIndicator.Dock = DockStyle.Left;
            pnlStatusIndicator.Location = new Point(0, 0);
            pnlStatusIndicator.Margin = new Padding(4, 3, 4, 3);
            pnlStatusIndicator.Name = "pnlStatusIndicator";
            pnlStatusIndicator.Size = new Size(12, 178);
            pnlStatusIndicator.TabIndex = 4;
            // 
            // MachineCard_Control
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlMain);
            Name = "MachineCard_Control";
            Size = new Size(280, 180);
            pnlMain.ResumeLayout(false);
            pnlMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picConnection).EndInit();
            ((System.ComponentModel.ISupportInitialize)picAlarm).EndInit();
            ((System.ComponentModel.ISupportInitialize)picPause).EndInit();
            ((System.ComponentModel.ISupportInitialize)picPlay).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Label lblCraneNumber;
        private System.Windows.Forms.Button btnVnc;
        private System.Windows.Forms.PictureBox picPlay;
        private System.Windows.Forms.PictureBox picPause;
        private System.Windows.Forms.PictureBox picAlarm;
        private System.Windows.Forms.Button btnInfo;
        private System.Windows.Forms.PictureBox picConnection;
        private Button button1;
        private Label lblStatus;
        private Panel pnlStatusIndicator;
    }
}