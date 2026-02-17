using System.Drawing;
using System.Windows.Forms;

namespace TekstilScada.UI.Views
{
    partial class RecipeStepDesigner_Control
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
            // --- BİLEŞENLERİN OLUŞTURULMASI ---
            this.toolStripMain = new System.Windows.Forms.ToolStrip();
            this.tsbNew = new System.Windows.Forms.ToolStripButton();
            this.tsbSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbCopy = new System.Windows.Forms.ToolStripButton();
            this.tsbPaste = new System.Windows.Forms.ToolStripButton();
            this.tsbDelete = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsLabelMachine = new System.Windows.Forms.ToolStripLabel();
            this.tsCmbMachineType = new System.Windows.Forms.ToolStripComboBox();
            this.tsLabelStep = new System.Windows.Forms.ToolStripLabel();
            this.tsCmbStepType = new System.Windows.Forms.ToolStripComboBox();

            this.statusStripBottom = new System.Windows.Forms.StatusStrip();
            this.lblStatusReady = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusPosition = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusSize = new System.Windows.Forms.ToolStripStatusLabel();

            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.splitContent = new System.Windows.Forms.SplitContainer();

            this.pnlToolbox = new System.Windows.Forms.Panel();
            this.lblToolboxHeader = new System.Windows.Forms.Label();
            this.btnLabel = new System.Windows.Forms.Button();
            this.btnNumeric = new System.Windows.Forms.Button();
            this.btnCheckbox = new System.Windows.Forms.Button();
            this.btnTextbox = new System.Windows.Forms.Button();
            this.btnButton = new System.Windows.Forms.Button();

            this.pnlDesignSurfaceWrapper = new System.Windows.Forms.Panel();
            this.pnlDesignSurface = new System.Windows.Forms.Panel();

            this.pnlProperties = new System.Windows.Forms.Panel();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.lblPropertiesHeader = new System.Windows.Forms.Label();

            // --- BAŞLATMA VE ASKIYA ALMA ---
            this.toolStripMain.SuspendLayout();
            this.statusStripBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContent)).BeginInit();
            this.splitContent.Panel1.SuspendLayout();
            this.splitContent.Panel2.SuspendLayout();
            this.splitContent.SuspendLayout();
            this.pnlToolbox.SuspendLayout();
            this.pnlDesignSurfaceWrapper.SuspendLayout();
            this.pnlProperties.SuspendLayout();
            this.SuspendLayout();

            // =================================================================================
            // 1. TOOLSTRIP (ÜST ARAÇ ÇUBUĞU)
            // =================================================================================
            this.toolStripMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripMain.BackColor = System.Drawing.Color.FromArgb(245, 246, 247);
            this.toolStripMain.Padding = new System.Windows.Forms.Padding(5);
            this.toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbNew,
            this.tsbSave,
            this.toolStripSeparator1,
            this.tsbCopy,
            this.tsbPaste,
            this.tsbDelete,
            this.toolStripSeparator2,
            this.tsLabelMachine,
            this.tsCmbMachineType,
            this.tsLabelStep,
            this.tsCmbStepType});
            this.toolStripMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolStripMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;

            ConfigureToolStripButton(this.tsbNew, "🗑️ Temizle", "Tasarımı Temizle");
            ConfigureToolStripButton(this.tsbSave, "💾 Kaydet", "Veritabanına Kaydet");
            ConfigureToolStripButton(this.tsbCopy, "📄 Kopyala", "Seçiliyi Kopyala (Ctrl+C)");
            ConfigureToolStripButton(this.tsbPaste, "📋 Yapıştır", "Yapıştır (Ctrl+V)");
            ConfigureToolStripButton(this.tsbDelete, "❌ Sil", "Seçiliyi Sil (Del)");

            this.tsLabelMachine.Text = "   Makine:";
            this.tsLabelMachine.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.tsLabelMachine.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);

            this.tsCmbMachineType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tsCmbMachineType.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.tsCmbMachineType.Size = new System.Drawing.Size(160, 25);
            this.tsCmbMachineType.BackColor = System.Drawing.Color.White;

            this.tsLabelStep.Text = "   Adım:";
            this.tsLabelStep.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.tsLabelStep.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);

            this.tsCmbStepType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tsCmbStepType.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.tsCmbStepType.Size = new System.Drawing.Size(160, 25);
            this.tsCmbStepType.BackColor = System.Drawing.Color.White;

            // =================================================================================
            // 2. STATUS STRIP (ALT BİLGİ ÇUBUĞU)
            // =================================================================================
            this.statusStripBottom.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.statusStripBottom.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatusReady,
            this.lblStatusPosition,
            this.lblStatusSize});
            this.statusStripBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusStripBottom.SizingGrip = false;

            this.lblStatusReady.ForeColor = System.Drawing.Color.White;
            this.lblStatusReady.Text = "Hazır";
            this.lblStatusReady.Spring = true;
            this.lblStatusReady.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblStatusPosition.ForeColor = System.Drawing.Color.White;
            this.lblStatusPosition.Text = "Konum: 0, 0";
            this.lblStatusPosition.Padding = new System.Windows.Forms.Padding(0, 0, 20, 0);

            this.lblStatusSize.ForeColor = System.Drawing.Color.White;
            this.lblStatusSize.Text = "Boyut: 0 x 0";

            // =================================================================================
            // 3. SPLIT CONTAINER (ANA) - Sol ve İçerik
            // =================================================================================
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitMain.Location = new System.Drawing.Point(0, 33);
            this.splitMain.Size = new System.Drawing.Size(1200, 645); // Önce Boyut Verildi!
            this.splitMain.SplitterDistance = 220; // HATA ÇÖZÜMÜ: Boyut verildikten sonra ayarlandı
            this.splitMain.SplitterWidth = 4;
            this.splitMain.BackColor = System.Drawing.Color.FromArgb(230, 230, 230);

            // Panel 1: Toolbox
            this.splitMain.Panel1.Controls.Add(this.pnlToolbox);
            this.splitMain.Panel1MinSize = 200;

            // Panel 2: İçerik
            this.splitMain.Panel2.Controls.Add(this.splitContent);
            this.splitMain.Panel2MinSize = 400;

            // =================================================================================
            // 4. SPLIT CONTAINER (İÇERİK) - Orta ve Sağ
            // =================================================================================
            this.splitContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContent.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            // HATA ÇÖZÜMÜ: Önce geçici boyut veriyoruz ki 650px mesafesi geçerli olsun
            this.splitContent.Size = new System.Drawing.Size(976, 645);
            this.splitContent.SplitterDistance = 650;
            this.splitContent.SplitterWidth = 4;
            this.splitContent.BackColor = System.Drawing.Color.FromArgb(230, 230, 230);

            // Panel 1: Tasarım Yüzeyi
            this.splitContent.Panel1.Controls.Add(this.pnlDesignSurfaceWrapper);
            this.splitContent.Panel1MinSize = 300;

            // Panel 2: Özellikler
            this.splitContent.Panel2.Controls.Add(this.pnlProperties);
            this.splitContent.Panel2MinSize = 250;

            // --- TOOLBOX İÇERİĞİ ---
            this.pnlToolbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlToolbox.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            this.pnlToolbox.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);

            this.lblToolboxHeader.Text = "ARAÇ KUTUSU";
            this.lblToolboxHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblToolboxHeader.Height = 35;
            this.lblToolboxHeader.ForeColor = System.Drawing.Color.FromArgb(241, 241, 241);
            this.lblToolboxHeader.BackColor = System.Drawing.Color.FromArgb(37, 37, 38);
            this.lblToolboxHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblToolboxHeader.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);

            this.pnlToolbox.Controls.Add(this.btnButton);
            this.pnlToolbox.Controls.Add(this.btnTextbox);
            this.pnlToolbox.Controls.Add(this.btnCheckbox);
            this.pnlToolbox.Controls.Add(this.btnNumeric);
            this.pnlToolbox.Controls.Add(this.btnLabel);
            this.pnlToolbox.Controls.Add(this.lblToolboxHeader);

            ConfigureToolboxButton(this.btnLabel, "   🏷️  Etiket", typeof(System.Windows.Forms.Label), 45);
            ConfigureToolboxButton(this.btnNumeric, "   🔢  Sayı Girişi", typeof(System.Windows.Forms.NumericUpDown), 90);
            ConfigureToolboxButton(this.btnCheckbox, "   ☑️  Onay Kutusu", typeof(System.Windows.Forms.CheckBox), 135);
            ConfigureToolboxButton(this.btnTextbox, "   📝  Metin Kutusu", typeof(System.Windows.Forms.TextBox), 180);
            ConfigureToolboxButton(this.btnButton, "   🖱️  Buton", typeof(System.Windows.Forms.Button), 225);

            // --- TASARIM ALANI ---
            this.pnlDesignSurfaceWrapper.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDesignSurfaceWrapper.BackColor = System.Drawing.Color.FromArgb(238, 241, 245);
            this.pnlDesignSurfaceWrapper.Padding = new System.Windows.Forms.Padding(20);
            this.pnlDesignSurfaceWrapper.Controls.Add(this.pnlDesignSurface);

            this.pnlDesignSurface.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDesignSurface.BackColor = System.Drawing.Color.White;
            this.pnlDesignSurface.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDesignSurface.AllowDrop = true;

            // --- ÖZELLİKLER PANELİ ---
            this.pnlProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlProperties.BackColor = System.Drawing.Color.White;

            this.lblPropertiesHeader.Text = "   ÖZELLİKLER";
            this.lblPropertiesHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPropertiesHeader.Height = 35;
            this.lblPropertiesHeader.BackColor = System.Drawing.Color.FromArgb(230, 231, 232);
            this.lblPropertiesHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPropertiesHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPropertiesHeader.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);

            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.ToolbarVisible = false;
            this.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.propertyGrid.BackColor = System.Drawing.Color.White;
            this.propertyGrid.ViewBackColor = System.Drawing.Color.White;
            this.propertyGrid.LineColor = System.Drawing.SystemColors.ControlLight;
            this.propertyGrid.HelpBackColor = System.Drawing.Color.WhiteSmoke;

            this.pnlProperties.Controls.Add(this.propertyGrid);
            this.pnlProperties.Controls.Add(this.lblPropertiesHeader);

            // --- FORM GENEL ---
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.toolStripMain);
            this.Controls.Add(this.statusStripBottom);
            this.Name = "RecipeStepDesigner_Control";
            this.Size = new System.Drawing.Size(1200, 700);

            this.toolStripMain.ResumeLayout(false);
            this.toolStripMain.PerformLayout();
            this.statusStripBottom.ResumeLayout(false);
            this.statusStripBottom.PerformLayout();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.splitContent.Panel1.ResumeLayout(false);
            this.splitContent.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContent)).EndInit();
            this.splitContent.ResumeLayout(false);
            this.pnlToolbox.ResumeLayout(false);
            this.pnlDesignSurfaceWrapper.ResumeLayout(false);
            this.pnlProperties.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ConfigureToolStripButton(ToolStripButton btn, string text, string tooltip)
        {
            btn.Text = text;
            btn.ToolTipText = tooltip;
            btn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btn.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            btn.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            btn.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
        }

        private void ConfigureToolboxButton(Button btn, string text, System.Type tagType, int topPos)
        {
            btn.Text = text;
            btn.Tag = tagType;
            btn.Top = topPos;
            btn.Left = 0;
            btn.Width = 180;
            btn.Height = 40;
            btn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(62, 62, 66);
            btn.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            btn.ForeColor = System.Drawing.Color.FromArgb(241, 241, 241);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
        }

        #endregion

        private ToolStrip toolStripMain;
        private StatusStrip statusStripBottom;
        private SplitContainer splitMain;
        private SplitContainer splitContent;

        private ToolStripButton tsbNew;
        private ToolStripButton tsbSave;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton tsbCopy;
        private ToolStripButton tsbPaste;
        private ToolStripButton tsbDelete;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripLabel tsLabelMachine;
        private ToolStripComboBox tsCmbMachineType;
        private ToolStripLabel tsLabelStep;
        private ToolStripComboBox tsCmbStepType;

        private ToolStripStatusLabel lblStatusReady;
        private ToolStripStatusLabel lblStatusPosition;
        private ToolStripStatusLabel lblStatusSize;

        private Panel pnlToolbox;
        private Panel pnlDesignSurfaceWrapper;
        private Panel pnlDesignSurface;
        private Panel pnlProperties;

        private Label lblToolboxHeader;
        private Button btnLabel;
        private Button btnNumeric;
        private Button btnCheckbox;
        private Button btnTextbox;
        private Button btnButton;

        private Label lblPropertiesHeader;
        private PropertyGrid propertyGrid;
    }
}