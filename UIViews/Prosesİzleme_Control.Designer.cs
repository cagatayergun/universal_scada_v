// UI/Views/Prosesİzleme_Control.Designer.cs
namespace Telemetry.UI.Views
{
    partial class Prosesİzleme_Control
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
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.flpTopKpis = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.flowLayoutPanelMachines = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.Transparent; // Koyu mod parlamasını engelleyen şeffaflık
            this.pnlHeader.Controls.Add(this.flpTopKpis);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pnlHeader.Size = new System.Drawing.Size(896, 81);
            this.pnlHeader.TabIndex = 0;
            // 
            // flpTopKpis
            // 
            this.flpTopKpis.BackColor = System.Drawing.Color.Transparent; // KPI kart arka planları kartın kendisi tarafından yönetilir
            this.flpTopKpis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpTopKpis.Location = new System.Drawing.Point(4, 3);
            flpTopKpis.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.flpTopKpis.Name = "flpTopKpis";
            this.flpTopKpis.Size = new System.Drawing.Size(888, 75);
            this.flpTopKpis.TabIndex = 0;
            this.flpTopKpis.Paint += new System.Windows.Forms.PaintEventHandler(this.flpTopKpis_Paint);
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.Transparent;
            this.pnlMain.Controls.Add(this.flowLayoutPanelMachines);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 81);
            this.pnlMain.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(896, 495);
            this.pnlMain.TabIndex = 1;
            // 
            // flowLayoutPanelMachines
            // 
            this.flowLayoutPanelMachines.AutoScroll = true;
            this.flowLayoutPanelMachines.BackColor = System.Drawing.Color.Transparent; // Kart bütünlüğü için mat şeffaf zemin
            this.flowLayoutPanelMachines.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelMachines.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelMachines.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.flowLayoutPanelMachines.Name = "flowLayoutPanelMachines";
            this.flowLayoutPanelMachines.Padding = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.flowLayoutPanelMachines.Size = new System.Drawing.Size(896, 495);
            this.flowLayoutPanelMachines.TabIndex = 0;
            // 
            // Prosesİzleme_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent; // Formun genel rengini ebeveyn panel (Tema Motoru) yönetir
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlHeader);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Prosesİzleme_Control";
            this.Size = new System.Drawing.Size(896, 576);
            this.pnlHeader.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.FlowLayoutPanel flpTopKpis;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMachines;
    }
}