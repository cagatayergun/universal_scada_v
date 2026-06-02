namespace Telemetry.UIControls
{
    partial class UtilityDashboardCard_Control
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblLineName = new System.Windows.Forms.Label();
            this.pnlStatusIndicator = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblAirVal = new System.Windows.Forms.Label();
            this.lblSteamVal = new System.Windows.Forms.Label();
            this.lblWaterVal = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblElecVal = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.Gainsboro;
            this.pnlHeader.Controls.Add(this.lblLineName);
            this.pnlHeader.Controls.Add(this.pnlStatusIndicator);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(1, 1);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(5);
            this.pnlHeader.Size = new System.Drawing.Size(318, 30);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblLineName
            // 
            this.lblLineName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLineName.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblLineName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblLineName.Location = new System.Drawing.Point(5, 5);
            this.lblLineName.Name = "lblLineName";
            this.lblLineName.Size = new System.Drawing.Size(288, 20);
            this.lblLineName.TabIndex = 2;
            this.lblLineName.Text = "Hat İsmi";
            this.lblLineName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlStatusIndicator
            // 
            this.pnlStatusIndicator.BackColor = System.Drawing.Color.Silver;
            this.pnlStatusIndicator.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlStatusIndicator.Location = new System.Drawing.Point(293, 5);
            this.pnlStatusIndicator.Name = "pnlStatusIndicator";
            this.pnlStatusIndicator.Size = new System.Drawing.Size(20, 20);
            this.pnlStatusIndicator.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.lblAirVal, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblSteamVal, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblWaterVal, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblElecVal, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(1, 31);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(318, 68);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // lblAirVal
            // 
            this.lblAirVal.AutoSize = true;
            this.lblAirVal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAirVal.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblAirVal.ForeColor = System.Drawing.Color.Teal;
            this.lblAirVal.Location = new System.Drawing.Point(240, 27);
            this.lblAirVal.Name = "lblAirVal";
            this.lblAirVal.Size = new System.Drawing.Size(75, 41);
            this.lblAirVal.TabIndex = 7;
            this.lblAirVal.Text = "-\r\nm³";
            this.lblAirVal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSteamVal
            // 
            this.lblSteamVal.AutoSize = true;
            this.lblSteamVal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSteamVal.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSteamVal.ForeColor = System.Drawing.Color.DarkOrange;
            this.lblSteamVal.Location = new System.Drawing.Point(161, 27);
            this.lblSteamVal.Name = "lblSteamVal";
            this.lblSteamVal.Size = new System.Drawing.Size(73, 41);
            this.lblSteamVal.TabIndex = 6;
            this.lblSteamVal.Text = "-\r\nkg";
            this.lblSteamVal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblWaterVal
            // 
            this.lblWaterVal.AutoSize = true;
            this.lblWaterVal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWaterVal.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblWaterVal.ForeColor = System.Drawing.Color.RoyalBlue;
            this.lblWaterVal.Location = new System.Drawing.Point(82, 27);
            this.lblWaterVal.Name = "lblWaterVal";
            this.lblWaterVal.Size = new System.Drawing.Size(73, 41);
            this.lblWaterVal.TabIndex = 5;
            this.lblWaterVal.Text = "-\r\nm³";
            this.lblWaterVal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.label4.ForeColor = System.Drawing.Color.Gray;
            this.label4.Location = new System.Drawing.Point(240, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 27);
            this.label4.TabIndex = 3;
            this.label4.Text = "Hava";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.label3.ForeColor = System.Drawing.Color.Gray;
            this.label3.Location = new System.Drawing.Point(161, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 27);
            this.label3.TabIndex = 2;
            this.label3.Text = "Buhar";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.label2.ForeColor = System.Drawing.Color.Gray;
            this.label2.Location = new System.Drawing.Point(82, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 27);
            this.label2.TabIndex = 1;
            this.label2.Text = "Su";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.label1.ForeColor = System.Drawing.Color.Gray;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 27);
            this.label1.TabIndex = 0;
            this.label1.Text = "Elektrik";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblElecVal
            // 
            this.lblElecVal.AutoSize = true;
            this.lblElecVal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblElecVal.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblElecVal.ForeColor = System.Drawing.Color.Firebrick;
            this.lblElecVal.Location = new System.Drawing.Point(3, 27);
            this.lblElecVal.Name = "lblElecVal";
            this.lblElecVal.Size = new System.Drawing.Size(73, 41);
            this.lblElecVal.TabIndex = 4;
            this.lblElecVal.Text = "-\r\nkWh";
            this.lblElecVal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UtilityDashboardCard_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.pnlHeader);
            this.Name = "UtilityDashboardCard_Control";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.Size = new System.Drawing.Size(320, 100);
            this.pnlHeader.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblLineName;
        private System.Windows.Forms.Panel pnlStatusIndicator;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblAirVal;
        private System.Windows.Forms.Label lblSteamVal;
        private System.Windows.Forms.Label lblWaterVal;
        private System.Windows.Forms.Label lblElecVal;
    }
}