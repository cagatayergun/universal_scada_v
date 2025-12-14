// UI/Views/Prosesİzleme_Control.Designer.cs
namespace TekstilScada.UI.Views
{
    partial class Prosesİzleme_Control
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                if (_pollingService != null)
                {
                    _pollingService.OnMachineDataRefreshed -= PollingService_OnMachineDataRefreshed;
                    _pollingService.OnMachineConnectionStateChanged -= PollingService_OnMachineConnectionStateChanged;
                }
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            flpTopKpis = new FlowLayoutPanel();
            pnlMain = new Panel();
            flowLayoutPanelMachines = new FlowLayoutPanel();
            pnlHeader.SuspendLayout();
            pnlMain.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(flpTopKpis);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(3, 2, 3, 2);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(4, 3, 4, 3);
            pnlHeader.Size = new Size(896, 81);
            pnlHeader.TabIndex = 0;
            // 
            // flpTopKpis
            // 
            flpTopKpis.BackColor = Color.DarkGray;
            flpTopKpis.Dock = DockStyle.Fill;
            flpTopKpis.Location = new Point(4, 3);
            flpTopKpis.Margin = new Padding(3, 2, 3, 2);
            flpTopKpis.Name = "flpTopKpis";
            flpTopKpis.Size = new Size(888, 75);
            flpTopKpis.TabIndex = 0;
            flpTopKpis.Paint += flpTopKpis_Paint;
            // 
            // pnlMain
            // 
            pnlMain.Controls.Add(flowLayoutPanelMachines);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 81);
            pnlMain.Margin = new Padding(3, 2, 3, 2);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(896, 495);
            pnlMain.TabIndex = 1;
            // 
            // flowLayoutPanelMachines
            // 
            flowLayoutPanelMachines.AutoScroll = true;
            flowLayoutPanelMachines.BackColor = SystemColors.ControlDark;
            flowLayoutPanelMachines.Dock = DockStyle.Fill;
            flowLayoutPanelMachines.Location = new Point(0, 0);
            flowLayoutPanelMachines.Margin = new Padding(3, 2, 3, 2);
            flowLayoutPanelMachines.Name = "flowLayoutPanelMachines";
            flowLayoutPanelMachines.Padding = new Padding(9, 8, 9, 8);
            flowLayoutPanelMachines.Size = new Size(896, 495);
            flowLayoutPanelMachines.TabIndex = 0;
            // 
            // Prosesİzleme_Control
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlMain);
            Controls.Add(pnlHeader);
            Margin = new Padding(3, 2, 3, 2);
            Name = "Prosesİzleme_Control";
            Size = new Size(896, 576);
            pnlHeader.ResumeLayout(false);
            pnlMain.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.FlowLayoutPanel flpTopKpis;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMachines;
    }
}