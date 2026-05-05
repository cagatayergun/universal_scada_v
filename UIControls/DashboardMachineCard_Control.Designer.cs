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
            pnlBitLampStatus_1 = new Panel();
            status_1 = new Label();
            pnlBitLampStatus_2 = new Panel();
            status_2 = new Label();
            pnlBitLampStatus_6 = new Panel();
            status_6 = new Label();
            pnlBitLampStatus_5 = new Panel();
            status_5 = new Label();
            status_3 = new Label();
            pnlBitLampStatus_3 = new Panel();
            status_4 = new Label();
            pnlBitLampStatus_4 = new Panel();
            pnlBitLampStatus_8 = new Panel();
            status_8 = new Label();
            pnlBitLampStatus_7 = new Panel();
            status_7 = new Label();
            pnlBitLampStatus_12 = new Panel();
            status_12 = new Label();
            pnlBitLampStatus_11 = new Panel();
            status_11 = new Label();
            pnlBitLampStatus_10 = new Panel();
            status_10 = new Label();
            pnlBitLampStatus_9 = new Panel();
            status_9 = new Label();
            pnlBitLampStatus_15 = new Panel();
            status_15 = new Label();
            pnlBitLampStatus_14 = new Panel();
            status_14 = new Label();
            pnlBitLampStatus_13 = new Panel();
            status_13 = new Label();
            pnlBitLampStatus_18 = new Panel();
            status_18 = new Label();
            pnlBitLampStatus_17 = new Panel();
            status_17 = new Label();
            pnlBitLampStatus_16 = new Panel();
            status_16 = new Label();
            pnlBitLampStatus_21 = new Panel();
            status_21 = new Label();
            pnlBitLampStatus_20 = new Panel();
            status_20 = new Label();
            pnlBitLampStatus_19 = new Panel();
            status_19 = new Label();
            pictureBox2 = new PictureBox();
            label1 = new Label();
            label2 = new Label();
            pictureBox3 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            SuspendLayout();
            // 
            // pnlStatusIndicator
            // 
            pnlStatusIndicator.BackColor = Color.SlateGray;
            pnlStatusIndicator.Dock = DockStyle.Left;
            pnlStatusIndicator.Location = new Point(0, 0);
            pnlStatusIndicator.Margin = new Padding(4, 3, 4, 3);
            pnlStatusIndicator.Name = "pnlStatusIndicator";
            pnlStatusIndicator.Size = new Size(12, 367);
            pnlStatusIndicator.TabIndex = 0;
            // 
            // lblMachineName
            // 
            lblMachineName.AutoSize = true;
            lblMachineName.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblMachineName.Location = new Point(23, 9);
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
            lblStatus.Location = new Point(22, 243);
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
            pictureBox1.Location = new Point(348, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(54, 31);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 19;
            pictureBox1.TabStop = false;
            // 
            // btnSendToPlc
            // 
            btnSendToPlc.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 162);
            btnSendToPlc.Location = new Point(127, 5);
            btnSendToPlc.Name = "btnSendToPlc";
            btnSendToPlc.Size = new Size(202, 28);
            btnSendToPlc.TabIndex = 20;
            btnSendToPlc.Text = "SÜRÜCÜ ALARM RESET";
            btnSendToPlc.UseVisualStyleBackColor = true;
            btnSendToPlc.Click += BtnSendToPlc_Click;
            // 
            // pnlBitLampStatus_1
            // 
            pnlBitLampStatus_1.BackColor = Color.Gray;
            pnlBitLampStatus_1.Location = new Point(28, 60);
            pnlBitLampStatus_1.Name = "pnlBitLampStatus_1";
            pnlBitLampStatus_1.Size = new Size(18, 18);
            pnlBitLampStatus_1.TabIndex = 23;
            // 
            // status_1
            // 
            status_1.AutoSize = true;
            status_1.BackColor = Color.WhiteSmoke;
            status_1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_1.Location = new Point(50, 62);
            status_1.Name = "status_1";
            status_1.Size = new Size(100, 15);
            status_1.TabIndex = 24;
            status_1.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_2
            // 
            pnlBitLampStatus_2.BackColor = Color.Gray;
            pnlBitLampStatus_2.Location = new Point(28, 81);
            pnlBitLampStatus_2.Name = "pnlBitLampStatus_2";
            pnlBitLampStatus_2.Size = new Size(18, 18);
            pnlBitLampStatus_2.TabIndex = 25;
            // 
            // status_2
            // 
            status_2.AutoSize = true;
            status_2.BackColor = Color.WhiteSmoke;
            status_2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_2.Location = new Point(50, 83);
            status_2.Name = "status_2";
            status_2.Size = new Size(100, 15);
            status_2.TabIndex = 26;
            status_2.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_6
            // 
            pnlBitLampStatus_6.BackColor = Color.Gray;
            pnlBitLampStatus_6.Location = new Point(154, 82);
            pnlBitLampStatus_6.Name = "pnlBitLampStatus_6";
            pnlBitLampStatus_6.Size = new Size(18, 18);
            pnlBitLampStatus_6.TabIndex = 29;
            // 
            // status_6
            // 
            status_6.AutoSize = true;
            status_6.BackColor = Color.WhiteSmoke;
            status_6.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_6.Location = new Point(176, 84);
            status_6.Name = "status_6";
            status_6.Size = new Size(100, 15);
            status_6.TabIndex = 30;
            status_6.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_5
            // 
            pnlBitLampStatus_5.BackColor = Color.Gray;
            pnlBitLampStatus_5.Location = new Point(154, 61);
            pnlBitLampStatus_5.Name = "pnlBitLampStatus_5";
            pnlBitLampStatus_5.Size = new Size(18, 18);
            pnlBitLampStatus_5.TabIndex = 27;
            // 
            // status_5
            // 
            status_5.AutoSize = true;
            status_5.BackColor = Color.WhiteSmoke;
            status_5.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_5.Location = new Point(176, 63);
            status_5.Name = "status_5";
            status_5.Size = new Size(100, 15);
            status_5.TabIndex = 28;
            status_5.Text = "PLC BAĞLANTISI";
            // 
            // status_3
            // 
            status_3.AutoSize = true;
            status_3.BackColor = Color.WhiteSmoke;
            status_3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_3.Location = new Point(50, 105);
            status_3.Name = "status_3";
            status_3.Size = new Size(100, 15);
            status_3.TabIndex = 28;
            status_3.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_3
            // 
            pnlBitLampStatus_3.BackColor = Color.Gray;
            pnlBitLampStatus_3.Location = new Point(28, 103);
            pnlBitLampStatus_3.Name = "pnlBitLampStatus_3";
            pnlBitLampStatus_3.Size = new Size(18, 18);
            pnlBitLampStatus_3.TabIndex = 27;
            // 
            // status_4
            // 
            status_4.AutoSize = true;
            status_4.BackColor = Color.WhiteSmoke;
            status_4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_4.Location = new Point(50, 126);
            status_4.Name = "status_4";
            status_4.Size = new Size(100, 15);
            status_4.TabIndex = 30;
            status_4.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_4
            // 
            pnlBitLampStatus_4.BackColor = Color.Gray;
            pnlBitLampStatus_4.Location = new Point(28, 124);
            pnlBitLampStatus_4.Name = "pnlBitLampStatus_4";
            pnlBitLampStatus_4.Size = new Size(18, 18);
            pnlBitLampStatus_4.TabIndex = 29;
            // 
            // pnlBitLampStatus_8
            // 
            pnlBitLampStatus_8.BackColor = Color.Gray;
            pnlBitLampStatus_8.Location = new Point(154, 124);
            pnlBitLampStatus_8.Name = "pnlBitLampStatus_8";
            pnlBitLampStatus_8.Size = new Size(18, 18);
            pnlBitLampStatus_8.TabIndex = 33;
            // 
            // status_8
            // 
            status_8.AutoSize = true;
            status_8.BackColor = Color.WhiteSmoke;
            status_8.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_8.Location = new Point(176, 126);
            status_8.Name = "status_8";
            status_8.Size = new Size(100, 15);
            status_8.TabIndex = 34;
            status_8.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_7
            // 
            pnlBitLampStatus_7.BackColor = Color.Gray;
            pnlBitLampStatus_7.Location = new Point(154, 103);
            pnlBitLampStatus_7.Name = "pnlBitLampStatus_7";
            pnlBitLampStatus_7.Size = new Size(18, 18);
            pnlBitLampStatus_7.TabIndex = 31;
            // 
            // status_7
            // 
            status_7.AutoSize = true;
            status_7.BackColor = Color.WhiteSmoke;
            status_7.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_7.Location = new Point(176, 105);
            status_7.Name = "status_7";
            status_7.Size = new Size(100, 15);
            status_7.TabIndex = 32;
            status_7.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_12
            // 
            pnlBitLampStatus_12.BackColor = Color.Gray;
            pnlBitLampStatus_12.Location = new Point(280, 123);
            pnlBitLampStatus_12.Name = "pnlBitLampStatus_12";
            pnlBitLampStatus_12.Size = new Size(18, 18);
            pnlBitLampStatus_12.TabIndex = 41;
            // 
            // status_12
            // 
            status_12.AutoSize = true;
            status_12.BackColor = Color.WhiteSmoke;
            status_12.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_12.Location = new Point(302, 125);
            status_12.Name = "status_12";
            status_12.Size = new Size(100, 15);
            status_12.TabIndex = 42;
            status_12.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_11
            // 
            pnlBitLampStatus_11.BackColor = Color.Gray;
            pnlBitLampStatus_11.Location = new Point(280, 102);
            pnlBitLampStatus_11.Name = "pnlBitLampStatus_11";
            pnlBitLampStatus_11.Size = new Size(18, 18);
            pnlBitLampStatus_11.TabIndex = 39;
            // 
            // status_11
            // 
            status_11.AutoSize = true;
            status_11.BackColor = Color.WhiteSmoke;
            status_11.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_11.Location = new Point(302, 104);
            status_11.Name = "status_11";
            status_11.Size = new Size(100, 15);
            status_11.TabIndex = 40;
            status_11.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_10
            // 
            pnlBitLampStatus_10.BackColor = Color.Gray;
            pnlBitLampStatus_10.Location = new Point(280, 81);
            pnlBitLampStatus_10.Name = "pnlBitLampStatus_10";
            pnlBitLampStatus_10.Size = new Size(18, 18);
            pnlBitLampStatus_10.TabIndex = 37;
            // 
            // status_10
            // 
            status_10.AutoSize = true;
            status_10.BackColor = Color.WhiteSmoke;
            status_10.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_10.Location = new Point(302, 83);
            status_10.Name = "status_10";
            status_10.Size = new Size(100, 15);
            status_10.TabIndex = 38;
            status_10.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_9
            // 
            pnlBitLampStatus_9.BackColor = Color.Gray;
            pnlBitLampStatus_9.Location = new Point(280, 60);
            pnlBitLampStatus_9.Name = "pnlBitLampStatus_9";
            pnlBitLampStatus_9.Size = new Size(18, 18);
            pnlBitLampStatus_9.TabIndex = 35;
            // 
            // status_9
            // 
            status_9.AutoSize = true;
            status_9.BackColor = Color.WhiteSmoke;
            status_9.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_9.Location = new Point(302, 62);
            status_9.Name = "status_9";
            status_9.Size = new Size(100, 15);
            status_9.TabIndex = 36;
            status_9.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_15
            // 
            pnlBitLampStatus_15.BackColor = Color.Gray;
            pnlBitLampStatus_15.Location = new Point(28, 214);
            pnlBitLampStatus_15.Name = "pnlBitLampStatus_15";
            pnlBitLampStatus_15.Size = new Size(18, 18);
            pnlBitLampStatus_15.TabIndex = 47;
            // 
            // status_15
            // 
            status_15.AutoSize = true;
            status_15.BackColor = Color.WhiteSmoke;
            status_15.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_15.Location = new Point(50, 216);
            status_15.Name = "status_15";
            status_15.Size = new Size(100, 15);
            status_15.TabIndex = 48;
            status_15.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_14
            // 
            pnlBitLampStatus_14.BackColor = Color.Gray;
            pnlBitLampStatus_14.Location = new Point(28, 193);
            pnlBitLampStatus_14.Name = "pnlBitLampStatus_14";
            pnlBitLampStatus_14.Size = new Size(18, 18);
            pnlBitLampStatus_14.TabIndex = 45;
            // 
            // status_14
            // 
            status_14.AutoSize = true;
            status_14.BackColor = Color.WhiteSmoke;
            status_14.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_14.Location = new Point(50, 195);
            status_14.Name = "status_14";
            status_14.Size = new Size(100, 15);
            status_14.TabIndex = 46;
            status_14.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_13
            // 
            pnlBitLampStatus_13.BackColor = Color.Gray;
            pnlBitLampStatus_13.Location = new Point(28, 172);
            pnlBitLampStatus_13.Name = "pnlBitLampStatus_13";
            pnlBitLampStatus_13.Size = new Size(18, 18);
            pnlBitLampStatus_13.TabIndex = 43;
            // 
            // status_13
            // 
            status_13.AutoSize = true;
            status_13.BackColor = Color.WhiteSmoke;
            status_13.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_13.Location = new Point(50, 174);
            status_13.Name = "status_13";
            status_13.Size = new Size(100, 15);
            status_13.TabIndex = 44;
            status_13.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_18
            // 
            pnlBitLampStatus_18.BackColor = Color.Gray;
            pnlBitLampStatus_18.Location = new Point(154, 214);
            pnlBitLampStatus_18.Name = "pnlBitLampStatus_18";
            pnlBitLampStatus_18.Size = new Size(18, 18);
            pnlBitLampStatus_18.TabIndex = 53;
            // 
            // status_18
            // 
            status_18.AutoSize = true;
            status_18.BackColor = Color.WhiteSmoke;
            status_18.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_18.Location = new Point(176, 216);
            status_18.Name = "status_18";
            status_18.Size = new Size(100, 15);
            status_18.TabIndex = 54;
            status_18.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_17
            // 
            pnlBitLampStatus_17.BackColor = Color.Gray;
            pnlBitLampStatus_17.Location = new Point(154, 193);
            pnlBitLampStatus_17.Name = "pnlBitLampStatus_17";
            pnlBitLampStatus_17.Size = new Size(18, 18);
            pnlBitLampStatus_17.TabIndex = 51;
            // 
            // status_17
            // 
            status_17.AutoSize = true;
            status_17.BackColor = Color.WhiteSmoke;
            status_17.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_17.Location = new Point(176, 195);
            status_17.Name = "status_17";
            status_17.Size = new Size(100, 15);
            status_17.TabIndex = 52;
            status_17.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_16
            // 
            pnlBitLampStatus_16.BackColor = Color.Gray;
            pnlBitLampStatus_16.Location = new Point(154, 172);
            pnlBitLampStatus_16.Name = "pnlBitLampStatus_16";
            pnlBitLampStatus_16.Size = new Size(18, 18);
            pnlBitLampStatus_16.TabIndex = 49;
            // 
            // status_16
            // 
            status_16.AutoSize = true;
            status_16.BackColor = Color.WhiteSmoke;
            status_16.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_16.Location = new Point(176, 174);
            status_16.Name = "status_16";
            status_16.Size = new Size(100, 15);
            status_16.TabIndex = 50;
            status_16.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_21
            // 
            pnlBitLampStatus_21.BackColor = Color.Gray;
            pnlBitLampStatus_21.Location = new Point(280, 214);
            pnlBitLampStatus_21.Name = "pnlBitLampStatus_21";
            pnlBitLampStatus_21.Size = new Size(18, 18);
            pnlBitLampStatus_21.TabIndex = 59;
            // 
            // status_21
            // 
            status_21.AutoSize = true;
            status_21.BackColor = Color.WhiteSmoke;
            status_21.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_21.Location = new Point(302, 216);
            status_21.Name = "status_21";
            status_21.Size = new Size(100, 15);
            status_21.TabIndex = 60;
            status_21.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_20
            // 
            pnlBitLampStatus_20.BackColor = Color.Gray;
            pnlBitLampStatus_20.Location = new Point(280, 193);
            pnlBitLampStatus_20.Name = "pnlBitLampStatus_20";
            pnlBitLampStatus_20.Size = new Size(18, 18);
            pnlBitLampStatus_20.TabIndex = 57;
            // 
            // status_20
            // 
            status_20.AutoSize = true;
            status_20.BackColor = Color.WhiteSmoke;
            status_20.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_20.Location = new Point(302, 195);
            status_20.Name = "status_20";
            status_20.Size = new Size(100, 15);
            status_20.TabIndex = 58;
            status_20.Text = "PLC BAĞLANTISI";
            // 
            // pnlBitLampStatus_19
            // 
            pnlBitLampStatus_19.BackColor = Color.Gray;
            pnlBitLampStatus_19.Location = new Point(280, 172);
            pnlBitLampStatus_19.Name = "pnlBitLampStatus_19";
            pnlBitLampStatus_19.Size = new Size(18, 18);
            pnlBitLampStatus_19.TabIndex = 55;
            // 
            // status_19
            // 
            status_19.AutoSize = true;
            status_19.BackColor = Color.WhiteSmoke;
            status_19.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            status_19.Location = new Point(302, 174);
            status_19.Name = "status_19";
            status_19.Size = new Size(100, 15);
            status_19.TabIndex = 56;
            status_19.Text = "PLC BAĞLANTISI";
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.WhiteSmoke;
            pictureBox2.BorderStyle = BorderStyle.FixedSingle;
            pictureBox2.Location = new Point(19, 39);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(393, 110);
            pictureBox2.TabIndex = 61;
            pictureBox2.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.WhiteSmoke;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label1.Location = new Point(20, 40);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(71, 21);
            label1.TabIndex = 62;
            label1.Text = "SWTICH";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.WhiteSmoke;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label2.Location = new Point(20, 152);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(65, 21);
            label2.TabIndex = 64;
            label2.Text = "BUTON";
            // 
            // pictureBox3
            // 
            pictureBox3.BackColor = Color.WhiteSmoke;
            pictureBox3.BorderStyle = BorderStyle.FixedSingle;
            pictureBox3.Location = new Point(19, 151);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(393, 90);
            pictureBox3.TabIndex = 63;
            pictureBox3.TabStop = false;
            // 
            // DashboardMachineCard_Control
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.WhiteSmoke;
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(pnlBitLampStatus_21);
            Controls.Add(status_21);
            Controls.Add(pnlBitLampStatus_20);
            Controls.Add(status_20);
            Controls.Add(pnlBitLampStatus_19);
            Controls.Add(status_19);
            Controls.Add(pnlBitLampStatus_18);
            Controls.Add(status_18);
            Controls.Add(pnlBitLampStatus_17);
            Controls.Add(status_17);
            Controls.Add(pnlBitLampStatus_16);
            Controls.Add(status_16);
            Controls.Add(pnlBitLampStatus_15);
            Controls.Add(status_15);
            Controls.Add(pnlBitLampStatus_14);
            Controls.Add(status_14);
            Controls.Add(pnlBitLampStatus_13);
            Controls.Add(status_13);
            Controls.Add(pnlBitLampStatus_12);
            Controls.Add(status_12);
            Controls.Add(pnlBitLampStatus_11);
            Controls.Add(status_11);
            Controls.Add(pnlBitLampStatus_10);
            Controls.Add(status_10);
            Controls.Add(pnlBitLampStatus_9);
            Controls.Add(status_9);
            Controls.Add(pnlBitLampStatus_8);
            Controls.Add(status_8);
            Controls.Add(pnlBitLampStatus_7);
            Controls.Add(status_7);
            Controls.Add(pnlBitLampStatus_4);
            Controls.Add(pnlBitLampStatus_6);
            Controls.Add(status_4);
            Controls.Add(status_6);
            Controls.Add(pnlBitLampStatus_3);
            Controls.Add(pnlBitLampStatus_5);
            Controls.Add(status_3);
            Controls.Add(status_5);
            Controls.Add(pnlBitLampStatus_2);
            Controls.Add(status_2);
            Controls.Add(pnlBitLampStatus_1);
            Controls.Add(status_1);
            Controls.Add(btnSendToPlc);
            Controls.Add(pictureBox1);
            Controls.Add(lblStatus);
            Controls.Add(lblMachineName);
            Controls.Add(pnlStatusIndicator);
            Controls.Add(pictureBox2);
            Controls.Add(pictureBox3);
            Margin = new Padding(9);
            Name = "DashboardMachineCard_Control";
            Size = new Size(424, 367);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Panel pnlStatusIndicator;
        private System.Windows.Forms.Label lblMachineName;
        private System.Windows.Forms.Label lblStatus;
        private PictureBox pictureBox1;
        private Button btnSendToPlc;
        private Panel pnlBitLampStatus_1;
        private Label status_1;
        private Panel pnlBitLampStatus_2;
        private Label status_2;
        private Panel pnlBitLampStatus_6;
        private Label status_6;
        private Panel pnlBitLampStatus_5;
        private Label status_5;
        private Label status_3;
        private Panel pnlBitLampStatus_3;
        private Label status_4;
        private Panel pnlBitLampStatus_4;
        private Panel pnlBitLampStatus_8;
        private Label status_8;
        private Panel pnlBitLampStatus_7;
        private Label status_7;
        private Panel pnlBitLampStatus_12;
        private Label status_12;
        private Panel pnlBitLampStatus_11;
        private Label status_11;
        private Panel pnlBitLampStatus_10;
        private Label status_10;
        private Panel pnlBitLampStatus_9;
        private Label status_9;
        private Panel pnlBitLampStatus_15;
        private Label status_15;
        private Panel pnlBitLampStatus_14;
        private Label status_14;
        private Panel pnlBitLampStatus_13;
        private Label status_13;
        private Panel pnlBitLampStatus_18;
        private Label status_18;
        private Panel pnlBitLampStatus_17;
        private Label status_17;
        private Panel pnlBitLampStatus_16;
        private Label status_16;
        private Panel pnlBitLampStatus_21;
        private Label status_21;
        private Panel pnlBitLampStatus_20;
        private Label status_20;
        private Panel pnlBitLampStatus_19;
        private Label status_19;
        private PictureBox pictureBox2;
        private Label label1;
        private Label label2;
        private PictureBox pictureBox3;
    }
}