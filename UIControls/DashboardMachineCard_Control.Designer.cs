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
            // DashboardMachineCard_Control
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Info;
            BorderStyle = BorderStyle.FixedSingle;
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
    }
}