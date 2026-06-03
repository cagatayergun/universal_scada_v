// UI/Controls/MachineCard_Control.Designer.cs
namespace Telemetry.UI.Controls
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
            this.materialCard1 = new MaterialSkin.Controls.MaterialCard(); // YENİ: Ana modern konteyner paneli
            this.lblMachineIdValue = new System.Windows.Forms.Label();
            this.lblMachineNameValue = new System.Windows.Forms.Label();
            this.lblStepValue = new System.Windows.Forms.Label();
            this.lblOperatorValue = new System.Windows.Forms.Label();
            this.lblRecipeNameValue = new System.Windows.Forms.Label();
            this.lblMachineIdTitle = new System.Windows.Forms.Label();
            this.lblMachineNameTitle = new System.Windows.Forms.Label();
            this.lblStepTitle = new System.Windows.Forms.Label();
            this.lblOperatorTitle = new System.Windows.Forms.Label();
            this.lblRecipeNameTitle = new System.Windows.Forms.Label();
            this.lblPercentage = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProcessing = new System.Windows.Forms.Label();
            this.pnlIcons = new System.Windows.Forms.Panel();
            this.picConnection = new System.Windows.Forms.PictureBox();
            this.btnInfo = new System.Windows.Forms.Button();
            this.picAlarm = new System.Windows.Forms.PictureBox();
            this.picPause = new System.Windows.Forms.PictureBox();
            this.picPlay = new System.Windows.Forms.PictureBox();
            this.btnVnc = new System.Windows.Forms.Button();
            this.lblMachineNumber = new System.Windows.Forms.Label();
            this.materialCard1.SuspendLayout();
            this.pnlIcons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picConnection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAlarm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPause)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPlay)).BeginInit();
            this.SuspendLayout();
            // 
            // materialCard1
            // 
            this.materialCard1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.materialCard1.Controls.Add(this.lblMachineIdValue);
            this.materialCard1.Controls.Add(this.lblMachineNameValue);
            this.materialCard1.Controls.Add(this.lblStepValue);
            this.materialCard1.Controls.Add(this.lblOperatorValue);
            this.materialCard1.Controls.Add(this.lblRecipeNameValue);
            this.materialCard1.Controls.Add(this.lblMachineIdTitle);
            this.materialCard1.Controls.Add(this.lblMachineNameTitle);
            this.materialCard1.Controls.Add(this.lblStepTitle);
            this.materialCard1.Controls.Add(this.lblOperatorTitle);
            this.materialCard1.Controls.Add(this.lblRecipeNameTitle);
            this.materialCard1.Controls.Add(this.lblPercentage);
            this.materialCard1.Controls.Add(this.progressBar);
            this.materialCard1.Controls.Add(this.lblProcessing);
            this.materialCard1.Controls.Add(this.pnlIcons);
            this.materialCard1.Controls.Add(this.lblMachineNumber);
            this.materialCard1.Depth = 0;
            this.materialCard1.Dock = System.Windows.Forms.DockStyle.Fill; // Tüm kartı kapla
            this.materialCard1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialCard1.Location = new System.Drawing.Point(0, 0);
            this.materialCard1.Margin = new System.Windows.Forms.Padding(5);
            this.materialCard1.MouseState = MaterialSkin.MouseState.OUT;
            this.materialCard1.Name = "materialCard1";
            this.materialCard1.Padding = new System.Windows.Forms.Padding(10);
            this.materialCard1.Size = new System.Drawing.Size(280, 180);
            this.materialCard1.TabIndex = 0;
            // 
            // lblMachineIdValue
            // 
            this.lblMachineIdValue.BackColor = System.Drawing.Color.Transparent;
            this.lblMachineIdValue.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblMachineIdValue.Location = new System.Drawing.Point(115, 149);
            this.lblMachineIdValue.Name = "lblMachineIdValue";
            this.lblMachineIdValue.Size = new System.Drawing.Size(148, 19);
            this.lblMachineIdValue.TabIndex = 14;
            this.lblMachineIdValue.Text = "---";
            this.lblMachineIdValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMachineNameValue
            // 
            this.lblMachineNameValue.BackColor = System.Drawing.Color.Transparent;
            this.lblMachineNameValue.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblMachineNameValue.Location = new System.Drawing.Point(115, 127);
            this.lblMachineNameValue.Name = "lblMachineNameValue";
            this.lblMachineNameValue.Size = new System.Drawing.Size(148, 19);
            this.lblMachineNameValue.TabIndex = 13;
            this.lblMachineNameValue.Text = "---";
            this.lblMachineNameValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStepValue
            // 
            this.lblStepValue.BackColor = System.Drawing.Color.Transparent;
            this.lblStepValue.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblStepValue.Location = new System.Drawing.Point(115, 104);
            this.lblStepValue.Name = "lblStepValue";
            this.lblStepValue.Size = new System.Drawing.Size(148, 19);
            this.lblStepValue.TabIndex = 12;
            this.lblStepValue.Text = "---";
            this.lblStepValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblOperatorValue
            // 
            this.lblOperatorValue.BackColor = System.Drawing.Color.Transparent;
            this.lblOperatorValue.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblOperatorValue.Location = new System.Drawing.Point(115, 81);
            this.lblOperatorValue.Name = "lblOperatorValue";
            this.lblOperatorValue.Size = new System.Drawing.Size(148, 19);
            this.lblOperatorValue.TabIndex = 11;
            this.lblOperatorValue.Text = "---";
            this.lblOperatorValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRecipeNameValue
            // 
            this.lblRecipeNameValue.BackColor = System.Drawing.Color.Transparent;
            this.lblRecipeNameValue.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblRecipeNameValue.Location = new System.Drawing.Point(115, 59);
            this.lblRecipeNameValue.Name = "lblRecipeNameValue";
            this.lblRecipeNameValue.Size = new System.Drawing.Size(148, 19);
            this.lblRecipeNameValue.TabIndex = 10;
            this.lblRecipeNameValue.Text = "---";
            this.lblRecipeNameValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMachineIdTitle
            // 
            this.lblMachineIdTitle.AutoSize = true;
            this.lblMachineIdTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblMachineIdTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblMachineIdTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(190)))), ((int)(((byte)(197))))); // Soft Mavi Gri
            this.lblMachineIdTitle.Location = new System.Drawing.Point(10, 151);
            this.lblMachineIdTitle.Name = "lblMachineIdTitle";
            this.lblMachineIdTitle.Size = new System.Drawing.Size(79, 15);
            this.lblMachineIdTitle.TabIndex = 9;
            this.lblMachineIdTitle.Text = "MACHINE ID:";
            // 
            // lblMachineNameTitle
            // 
            this.lblMachineNameTitle.AutoSize = true;
            this.lblMachineNameTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblMachineNameTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblMachineNameTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(190)))), ((int)(((byte)(197)))));
            this.lblMachineNameTitle.Location = new System.Drawing.Point(10, 129);
            this.lblMachineNameTitle.Name = "lblMachineNameTitle";
            this.lblMachineNameTitle.Size = new System.Drawing.Size(100, 15);
            this.lblMachineNameTitle.TabIndex = 8;
            this.lblMachineNameTitle.Text = "MACHINE NAME:";
            // 
            // lblStepTitle
            // 
            this.lblStepTitle.AutoSize = true;
            this.lblStepTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblStepTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblStepTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(190)))), ((int)(((byte)(197)))));
            this.lblStepTitle.Location = new System.Drawing.Point(10, 106);
            this.lblStepTitle.Name = "lblStepTitle";
            this.lblStepTitle.Size = new System.Drawing.Size(36, 15);
            this.lblStepTitle.TabIndex = 7;
            this.lblStepTitle.Text = "STEP:";
            // 
            // lblOperatorTitle
            // 
            this.lblOperatorTitle.AutoSize = true;
            this.lblOperatorTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblOperatorTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblOperatorTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(190)))), ((int)(((byte)(197)))));
            this.lblOperatorTitle.Location = new System.Drawing.Point(10, 83);
            this.lblOperatorTitle.Name = "lblOperatorTitle";
            this.lblOperatorTitle.Size = new System.Drawing.Size(71, 15);
            this.lblOperatorTitle.TabIndex = 6;
            this.lblOperatorTitle.Text = "OPERATOR:";
            // 
            // lblRecipeNameTitle
            // 
            this.lblRecipeNameTitle.AutoSize = true;
            this.lblRecipeNameTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblRecipeNameTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblRecipeNameTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(190)))), ((int)(((byte)(197)))));
            this.lblRecipeNameTitle.Location = new System.Drawing.Point(10, 61);
            this.lblRecipeNameTitle.Name = "lblRecipeNameTitle";
            this.lblRecipeNameTitle.Size = new System.Drawing.Size(86, 15);
            this.lblRecipeNameTitle.TabIndex = 5;
            this.lblRecipeNameTitle.Text = "RECIPE NAME:";
            // 
            // lblPercentage
            // 
            this.lblPercentage.AutoSize = true;
            this.lblPercentage.BackColor = System.Drawing.Color.Transparent;
            this.lblPercentage.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblPercentage.Location = new System.Drawing.Point(232, 38);
            this.lblPercentage.Name = "lblPercentage";
            this.lblPercentage.Size = new System.Drawing.Size(27, 15);
            this.lblPercentage.TabIndex = 3;
            this.lblPercentage.Text = "0 %";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(115, 41);
            this.progressBar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(111, 8);
            this.progressBar.TabIndex = 2;
            // 
            // lblProcessing
            // 
            this.lblProcessing.AutoSize = true;
            this.lblProcessing.BackColor = System.Drawing.Color.Transparent;
            this.lblProcessing.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblProcessing.Location = new System.Drawing.Point(10, 38);
            this.lblProcessing.Name = "lblProcessing";
            this.lblProcessing.Size = new System.Drawing.Size(79, 15);
            this.lblProcessing.TabIndex = 1;
            this.lblProcessing.Text = "PROCESSING";
            // 
            // pnlIcons
            // 
            this.pnlIcons.BackColor = System.Drawing.Color.Transparent;
            this.pnlIcons.Controls.Add(this.picConnection);
            this.pnlIcons.Controls.Add(this.btnInfo);
            this.pnlIcons.Controls.Add(this.picAlarm);
            this.pnlIcons.Controls.Add(this.picPause);
            this.pnlIcons.Controls.Add(this.picPlay);
            this.pnlIcons.Controls.Add(this.btnVnc);
            this.pnlIcons.Location = new System.Drawing.Point(44, 4);
            this.pnlIcons.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlIcons.Name = "pnlIcons";
            this.pnlIcons.Size = new System.Drawing.Size(228, 30);
            this.pnlIcons.TabIndex = 15;
            // 
            // picConnection
            // 
            this.picConnection.BackColor = System.Drawing.Color.Transparent;
            this.picConnection.Image = global::Telemetry.Properties.Resource1.yilmak_baglanti_2;
            this.picConnection.InitialImage = global::Telemetry.Properties.Resource1.yilmak_baglanti;
            this.picConnection.Location = new System.Drawing.Point(149, 4);
            this.picConnection.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.picConnection.Name = "picConnection";
            this.picConnection.Size = new System.Drawing.Size(26, 22);
            this.picConnection.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picConnection.TabIndex = 6;
            this.picConnection.TabStop = false;
            // 
            // btnInfo
            // 
            this.btnInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(71)))), ((int)(((byte)(79))))); // Modern Material Blue Grey
            this.btnInfo.FlatAppearance.BorderSize = 0;
            this.btnInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat; // Modern flat görünüm
            this.btnInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnInfo.ForeColor = System.Drawing.Color.White;
            this.btnInfo.Location = new System.Drawing.Point(192, 4);
            this.btnInfo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnInfo.Name = "btnInfo";
            this.btnInfo.Size = new System.Drawing.Size(26, 22);
            this.btnInfo.TabIndex = 5;
            this.btnInfo.Text = "i";
            this.btnInfo.UseVisualStyleBackColor = false;
            this.btnInfo.Click += new System.EventHandler(this.btnInfo_Click);
            // 
            // picAlarm
            // 
            this.picAlarm.BackColor = System.Drawing.Color.Transparent;
            this.picAlarm.Image = global::Telemetry.Properties.Resource1.alarm;
            this.picAlarm.Location = new System.Drawing.Point(114, 4);
            this.picAlarm.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.picAlarm.Name = "picAlarm";
            this.picAlarm.Size = new System.Drawing.Size(26, 22);
            this.picAlarm.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picAlarm.TabIndex = 3;
            this.picAlarm.TabStop = false;
            // 
            // picPause
            // 
            this.picPause.BackColor = System.Drawing.Color.Transparent;
            this.picPause.Image = global::Telemetry.Properties.Resource1.pause;
            this.picPause.Location = new System.Drawing.Point(79, 4);
            this.picPause.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.picPause.Name = "picPause";
            this.picPause.Size = new System.Drawing.Size(26, 22);
            this.picPause.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picPause.TabIndex = 2;
            this.picPause.TabStop = false;
            // 
            // picPlay
            // 
            this.picPlay.BackColor = System.Drawing.Color.Transparent;
            this.picPlay.Image = global::Telemetry.Properties.Resource1.play;
            this.picPlay.Location = new System.Drawing.Point(44, 4);
            this.picPlay.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.picPlay.Name = "picPlay";
            this.picPlay.Size = new System.Drawing.Size(26, 22);
            this.picPlay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picPlay.TabIndex = 1;
            this.picPlay.TabStop = false;
            // 
            // btnVnc
            // 
            this.btnVnc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(71)))), ((int)(((byte)(79))))); // Modern Material Blue Grey
            this.btnVnc.FlatAppearance.BorderSize = 0;
            this.btnVnc.FlatStyle = System.Windows.Forms.FlatStyle.Flat; // Modern flat görünüm
            this.btnVnc.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnVnc.ForeColor = System.Drawing.Color.White;
            this.btnVnc.Location = new System.Drawing.Point(9, 4);
            this.btnVnc.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnVnc.Name = "btnVnc";
            this.btnVnc.Size = new System.Drawing.Size(26, 22);
            this.btnVnc.TabIndex = 0;
            this.btnVnc.Text = "M";
            this.btnVnc.UseVisualStyleBackColor = false;
            this.btnVnc.Click += new System.EventHandler(this.btnVnc_Click);
            // 
            // lblMachineNumber
            // 
            this.lblMachineNumber.AutoSize = true;
            this.lblMachineNumber.BackColor = System.Drawing.Color.Transparent;
            this.lblMachineNumber.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold);
            this.lblMachineNumber.Location = new System.Drawing.Point(10, 4);
            this.lblMachineNumber.Name = "lblMachineNumber";
            this.lblMachineNumber.Size = new System.Drawing.Size(32, 30);
            this.lblMachineNumber.TabIndex = 0;
            this.lblMachineNumber.Text = "1.";
            // 
            // MachineCard_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent; // Kenar ve arka plan çizim yetkisi bütünüyle MaterialCard kontrolünde
            this.Controls.Add(this.materialCard1);
            this.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.Name = "MachineCard_Control";
            this.Size = new System.Drawing.Size(280, 180);
            this.materialCard1.ResumeLayout(false);
            this.materialCard1.PerformLayout();
            this.pnlIcons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picConnection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAlarm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPause)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPlay)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private MaterialSkin.Controls.MaterialCard materialCard1;
        private System.Windows.Forms.Label lblMachineNumber;
        private System.Windows.Forms.Panel pnlIcons;
        private System.Windows.Forms.Button btnVnc;
        private System.Windows.Forms.PictureBox picPlay;
        private System.Windows.Forms.PictureBox picPause;
        private System.Windows.Forms.PictureBox picAlarm;
        private System.Windows.Forms.Button btnInfo;
        private System.Windows.Forms.Label lblProcessing;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblPercentage;
        private System.Windows.Forms.Label lblRecipeNameTitle;
        private System.Windows.Forms.Label lblOperatorTitle;
        private System.Windows.Forms.Label lblStepTitle;
        private System.Windows.Forms.Label lblMachineNameTitle;
        private System.Windows.Forms.Label lblMachineIdTitle;
        private System.Windows.Forms.Label lblRecipeNameValue;
        private System.Windows.Forms.Label lblOperatorValue;
        private System.Windows.Forms.Label lblStepValue;
        private System.Windows.Forms.Label lblMachineNameValue;
        private System.Windows.Forms.Label lblMachineIdValue;
        private System.Windows.Forms.PictureBox picConnection;
    }
}