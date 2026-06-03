// UIViews/DowntimeSettings_Control.Designer.cs
namespace Telemetry.UIViews
{
    partial class DowntimeSettings_Control
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
            this.dgvDowntime = new System.Windows.Forms.DataGridView();
            this.colBitIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReasonText = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new MaterialSkin.Controls.MaterialLabel();      // MaterialLabel olarak güncellendi
            this.panelActions = new System.Windows.Forms.Panel();
            this.btnRefresh = new MaterialSkin.Controls.MaterialButton();   // MaterialButton olarak güncellendi
            this.btnAdd = new MaterialSkin.Controls.MaterialButton();       // MaterialButton olarak güncellendi
            this.btnDelete = new MaterialSkin.Controls.MaterialButton();    // MaterialButton olarak güncellendi
            this.btnSave = new MaterialSkin.Controls.MaterialButton();      // MaterialButton olarak güncellendi
            ((System.ComponentModel.ISupportInitialize)(this.dgvDowntime)).BeginInit();
            this.panelHeader.SuspendLayout();
            this.panelActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvDowntime
            // 
            this.dgvDowntime.AllowUserToAddRows = false;
            this.dgvDowntime.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDowntime.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvDowntime.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDowntime.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colBitIndex,
            this.colReasonText});
            this.dgvDowntime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDowntime.Location = new System.Drawing.Point(0, 50);
            this.dgvDowntime.MultiSelect = false;
            this.dgvDowntime.Name = "dgvDowntime";
            this.dgvDowntime.RowHeadersVisible = false;
            this.dgvDowntime.RowTemplate.Height = 26;
            this.dgvDowntime.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDowntime.Size = new System.Drawing.Size(650, 300);
            this.dgvDowntime.TabIndex = 0;
            // 
            // colBitIndex
            // 
            this.colBitIndex.FillWeight = 40F;
            this.colBitIndex.HeaderText = "PLC Bit Address (Index)";
            this.colBitIndex.Name = "colBitIndex";
            // 
            // colReasonText
            // 
            this.colReasonText.FillWeight = 150F;
            this.colReasonText.HeaderText = "Downtime Reason Description";
            this.colReasonText.Name = "colReasonText";
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.Transparent; // Ana formun Dark/Light mod temasına tam uyum
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(650, 50);
            this.panelHeader.TabIndex = 1;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Depth = 0;
            this.lblTitle.Font = new System.Drawing.Font("Roboto", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblTitle.FontType = MaterialSkin.MaterialSkinManager.fontType.H6; // Modern üst başlık font hiyerarşisi
            this.lblTitle.Location = new System.Drawing.Point(15, 14);
            this.lblTitle.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(262, 24);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "PLC Downtime / Wait Reasons";
            // 
            // panelActions
            // 
            this.panelActions.BackColor = System.Drawing.Color.Transparent;
            this.panelActions.Controls.Add(this.btnRefresh);
            this.panelActions.Controls.Add(this.btnAdd);
            this.panelActions.Controls.Add(this.btnDelete);
            this.panelActions.Controls.Add(this.btnSave);
            this.panelActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelActions.Location = new System.Drawing.Point(0, 350);
            this.panelActions.Name = "panelActions";
            this.panelActions.Size = new System.Drawing.Size(650, 60);
            this.panelActions.TabIndex = 2;
            // 
            // btnAdd
            // 
            this.btnAdd.AutoSize = false;
            this.btnAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAdd.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnAdd.Depth = 0;
            this.btnAdd.HighEmphasis = false;
            this.btnAdd.Icon = null;
            this.btnAdd.Location = new System.Drawing.Point(15, 12); // 36px yüksekliğe göre dikey eksende kusursuz ortalandı
            this.btnAdd.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnAdd.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnAdd.Size = new System.Drawing.Size(100, 36);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Text = "Add New";
            this.btnAdd.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined; // Temiz çizgili flat stil
            this.btnAdd.UseAccentColor = false;
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.AutoSize = false;
            this.btnDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnDelete.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnDelete.Depth = 0;
            this.btnDelete.HighEmphasis = false;
            this.btnDelete.Icon = null;
            this.btnDelete.Location = new System.Drawing.Point(123, 12);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnDelete.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnDelete.Size = new System.Drawing.Size(140, 36); // Metin taşmasını engelleyen genişlik esnetmesi
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "Delete Selected";
            this.btnDelete.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnDelete.UseAccentColor = false;
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.AutoSize = false;
            this.btnRefresh.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRefresh.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnRefresh.Depth = 0;
            this.btnRefresh.HighEmphasis = false;
            this.btnRefresh.Icon = null;
            this.btnRefresh.Location = new System.Drawing.Point(271, 12);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnRefresh.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnRefresh.Size = new System.Drawing.Size(90, 36);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnRefresh.UseAccentColor = false;
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.AutoSize = false;
            this.btnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSave.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSave.Depth = 0;
            this.btnSave.HighEmphasis = true; // Baskın ana aksiyon vurgusu aktif
            this.btnSave.Icon = null;
            this.btnSave.Location = new System.Drawing.Point(485, 12);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSave.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSave.Name = "btnSave";
            this.btnSave.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSave.Size = new System.Drawing.Size(150, 36);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save Changes";
            this.btnSave.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained; // Dolgulu material tarzı
            this.btnSave.UseAccentColor = true; // Dikkat çekici aksan rengi
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // DowntimeSettings_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent; // Arka plan yönetimi üst ebeveyne (Ayarlar paneline) devredildi
            this.Controls.Add(this.dgvDowntime);
            this.Controls.Add(this.panelActions);
            this.Controls.Add(this.panelHeader);
            this.Name = "DowntimeSettings_Control";
            this.Size = new System.Drawing.Size(650, 410);
            this.Load += new System.EventHandler(this.DowntimeSettings_Control_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDowntime)).EndInit();
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelActions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvDowntime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBitIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReasonText;
        private System.Windows.Forms.Panel panelHeader;
        private MaterialSkin.Controls.MaterialLabel lblTitle;               // Tür güncellendi
        private System.Windows.Forms.Panel panelActions;
        private MaterialSkin.Controls.MaterialButton btnAdd;               // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnDelete;            // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnSave;              // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnRefresh;           // Tür güncellendi
    }
}