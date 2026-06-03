// UI/Views/ProsesKontrol_Control.Designer.cs
using Telemetry.Services;

namespace Telemetry.UI.Views
{
    partial class ProsesKontrol_Control
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lstRecipes = new System.Windows.Forms.ListBox();
            this.lstRecipeHistory = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.yenile = new MaterialSkin.Controls.MaterialButton();       // MaterialButton yapıldı
            this.btnDeleteRecipe = new MaterialSkin.Controls.MaterialButton(); // MaterialButton yapıldı
            this.btnNewRecipe = new MaterialSkin.Controls.MaterialButton();    // MaterialButton yapıldı
            this.label1 = new MaterialSkin.Controls.MaterialLabel();          // MaterialLabel yapıldı
            this.pnlEditorArea = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnFtpSync = new MaterialSkin.Controls.MaterialButton();     // MaterialButton yapıldı
            this.cmbTargetMachine = new System.Windows.Forms.ComboBox();
            this.label4 = new MaterialSkin.Controls.MaterialLabel();          // MaterialLabel yapıldı
            this.btnReadFromPlc = new MaterialSkin.Controls.MaterialButton();  // MaterialButton yapıldı
            this.btnSendToPlc = new MaterialSkin.Controls.MaterialButton();    // MaterialButton yapıldı
            this.btnSaveRecipe = new MaterialSkin.Controls.MaterialButton();    // MaterialButton yapıldı
            this.txtRecipeName = new System.Windows.Forms.TextBox();
            this.label3 = new MaterialSkin.Controls.MaterialLabel();          // MaterialLabel yapıldı
            this.btnCalculateCost = new MaterialSkin.Controls.MaterialButton(); // MaterialButton yapıldı
            this.pnlCost = new System.Windows.Forms.Panel();
            this.lblTotalCost = new System.Windows.Forms.Label();
            this.lblCostTitle = new MaterialSkin.Controls.MaterialLabel();    // MaterialLabel yapıldı
            this.pnlSearch = new System.Windows.Forms.Panel();
            this.txtSearchRecipe = new System.Windows.Forms.TextBox();
            this.lblSearch = new MaterialSkin.Controls.MaterialLabel();       // MaterialLabel yapıldı
            this.pnlSort = new System.Windows.Forms.Panel();
            this.radioSortName = new MaterialSkin.Controls.MaterialRadioButton(); // MaterialRadioButton yapıldı
            this.radioSortDate = new MaterialSkin.Controls.MaterialRadioButton(); // MaterialRadioButton yapıldı
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.pnlCost.SuspendLayout();
            this.pnlSearch.SuspendLayout();
            this.pnlSort.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.Color.Transparent;
            this.splitContainer1.Panel1.Controls.Add(this.lstRecipes);
            this.splitContainer1.Panel1.Controls.Add(this.lstRecipeHistory);
            this.splitContainer1.Panel1.Controls.Add(this.pnlSort);
            this.splitContainer1.Panel1.Controls.Add(this.pnlSearch);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.Transparent;
            this.splitContainer1.Panel2.Controls.Add(this.pnlEditorArea);
            this.splitContainer1.Panel2.Controls.Add(this.panel2);
            this.splitContainer1.Size = new System.Drawing.Size(939, 448);
            this.splitContainer1.SplitterDistance = 210; // Genişlik material etiketlerin sığması için 210px'e esnetildi
            this.splitContainer1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Depth = 0;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.label1.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle2;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.MouseState = MaterialSkin.MouseState.HOVER;
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(210, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "Registered Recipes";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlSearch
            // 
            this.pnlSearch.BackColor = System.Drawing.Color.Transparent;
            this.pnlSearch.Controls.Add(this.txtSearchRecipe);
            this.pnlSearch.Controls.Add(this.lblSearch);
            this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearch.Location = new System.Drawing.Point(0, 22);
            this.pnlSearch.Name = "pnlSearch";
            this.pnlSearch.Padding = new System.Windows.Forms.Padding(5);
            this.pnlSearch.Size = new System.Drawing.Size(210, 52);
            this.pnlSearch.TabIndex = 3;
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Depth = 0;
            this.lblSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSearch.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblSearch.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.lblSearch.Location = new System.Drawing.Point(5, 5);
            this.lblSearch.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(41, 17);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search:";
            // 
            // txtSearchRecipe
            // 
            this.txtSearchRecipe.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtSearchRecipe.Location = new System.Drawing.Point(5, 22);
            this.txtSearchRecipe.Name = "txtSearchRecipe";
            this.txtSearchRecipe.Size = new System.Drawing.Size(200, 23);
            this.txtSearchRecipe.TabIndex = 1;
            // 
            // pnlSort
            // 
            this.pnlSort.BackColor = System.Drawing.Color.Transparent;
            this.pnlSort.Controls.Add(this.radioSortDate);
            this.pnlSort.Controls.Add(this.radioSortName);
            this.pnlSort.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSort.Location = new System.Drawing.Point(0, 74);
            this.pnlSort.Name = "pnlSort";
            this.pnlSort.Size = new System.Drawing.Size(210, 36);
            this.pnlSort.TabIndex = 4;
            // 
            // radioSortName
            // 
            this.radioSortName.AutoSize = true;
            this.radioSortName.Checked = true;
            this.radioSortName.Depth = 0;
            this.radioSortName.Location = new System.Drawing.Point(5, -1); // Dikey basamaklandırma dengelendi
            this.radioSortName.Margin = new System.Windows.Forms.Padding(0);
            this.radioSortName.MouseLocation = new System.Drawing.Point(-1, -1);
            this.radioSortName.MouseState = MaterialSkin.MouseState.HOVER;
            this.radioSortName.Name = "radioSortName";
            this.radioSortName.Ripple = true;
            this.radioSortName.Size = new System.Drawing.Size(64, 37);
            this.radioSortName.TabIndex = 0;
            this.radioSortName.TabStop = true;
            this.radioSortName.Text = "A-Z";
            this.radioSortName.UseVisualStyleBackColor = true;
            // 
            // radioSortDate
            // 
            this.radioSortDate.AutoSize = true;
            this.radioSortDate.Depth = 0;
            this.radioSortDate.Location = new System.Drawing.Point(85, -1);
            this.radioSortDate.Margin = new System.Windows.Forms.Padding(0);
            this.radioSortDate.MouseLocation = new System.Drawing.Point(-1, -1);
            this.radioSortDate.MouseState = MaterialSkin.MouseState.HOVER;
            this.radioSortDate.Name = "radioSortDate";
            this.radioSortDate.Ripple = true;
            this.radioSortDate.Size = new System.Drawing.Size(91, 37);
            this.radioSortDate.TabIndex = 1;
            this.radioSortDate.Text = "Newest";
            this.radioSortDate.UseVisualStyleBackColor = true;
            // 
            // lstRecipes
            // 
            this.lstRecipes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstRecipes.FormattingEnabled = true;
            this.lstRecipes.ItemHeight = 15;
            this.lstRecipes.Location = new System.Drawing.Point(0, 110);
            this.lstRecipes.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lstRecipes.Name = "lstRecipes";
            this.lstRecipes.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstRecipes.Size = new System.Drawing.Size(210, 192);
            this.lstRecipes.TabIndex = 1;
            // 
            // lstRecipeHistory
            // 
            this.lstRecipeHistory.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lstRecipeHistory.FormattingEnabled = true;
            this.lstRecipeHistory.ItemHeight = 15;
            this.lstRecipeHistory.Location = new System.Drawing.Point(0, 302);
            this.lstRecipeHistory.Name = "lstRecipeHistory";
            this.lstRecipeHistory.Size = new System.Drawing.Size(210, 100);
            this.lstRecipeHistory.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.yenile);
            this.panel1.Controls.Add(this.btnDeleteRecipe);
            this.panel1.Controls.Add(this.btnNewRecipe);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 402);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(210, 46); // 36px material butonlar için panel alanı 46px'e genişletildi
            this.panel1.TabIndex = 2;
            // 
            // btnNewRecipe
            // 
            this.btnNewRecipe.AutoSize = false;
            this.btnNewRecipe.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnNewRecipe.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnNewRecipe.Depth = 0;
            this.btnNewRecipe.HighEmphasis = false;
            this.btnNewRecipe.Icon = null;
            this.btnNewRecipe.Location = new System.Drawing.Point(3, 5); // Hizalama dengesi sağlandı
            this.btnNewRecipe.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnNewRecipe.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnNewRecipe.Name = "btnNewRecipe";
            this.btnNewRecipe.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnNewRecipe.Size = new System.Drawing.Size(65, 36);
            this.btnNewRecipe.TabIndex = 0;
            this.btnNewRecipe.Text = "New";
            this.btnNewRecipe.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnNewRecipe.UseAccentColor = false;
            this.btnNewRecipe.UseVisualStyleBackColor = true;
            // 
            // btnDeleteRecipe
            // 
            this.btnDeleteRecipe.AutoSize = false;
            this.btnDeleteRecipe.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnDeleteRecipe.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnDeleteRecipe.Depth = 0;
            this.btnDeleteRecipe.HighEmphasis = false;
            this.btnDeleteRecipe.Icon = null;
            this.btnDeleteRecipe.Location = new System.Drawing.Point(72, 5);
            this.btnDeleteRecipe.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnDeleteRecipe.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnDeleteRecipe.Name = "btnDeleteRecipe";
            this.btnDeleteRecipe.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnDeleteRecipe.Size = new System.Drawing.Size(65, 36);
            this.btnDeleteRecipe.TabIndex = 1;
            this.btnDeleteRecipe.Text = "Delete";
            this.btnDeleteRecipe.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnDeleteRecipe.UseAccentColor = false;
            this.btnDeleteRecipe.UseVisualStyleBackColor = true;
            // 
            // yenile
            // 
            this.yenile.AutoSize = false;
            this.yenile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.yenile.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.yenile.Depth = 0;
            this.yenile.HighEmphasis = false;
            this.yenile.Icon = null;
            this.yenile.Location = new System.Drawing.Point(141, 5);
            this.yenile.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.yenile.MouseState = MaterialSkin.MouseState.HOVER;
            this.yenile.Name = "yenile";
            this.yenile.NoAccentTextColor = System.Drawing.Color.Empty;
            this.yenile.Size = new System.Drawing.Size(65, 36);
            this.yenile.TabIndex = 2;
            this.yenile.Text = "Refresh";
            this.yenile.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.yenile.UseAccentColor = false;
            this.yenile.UseVisualStyleBackColor = true;
            // 
            // pnlEditorArea
            // 
            this.pnlEditorArea.BackColor = System.Drawing.Color.Transparent;
            this.pnlEditorArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEditorArea.Location = new System.Drawing.Point(0, 105);
            this.pnlEditorArea.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlEditorArea.Name = "pnlEditorArea";
            this.pnlEditorArea.Size = new System.Drawing.Size(725, 343);
            this.pnlEditorArea.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Transparent;
            this.panel2.Controls.Add(this.btnFtpSync);
            this.panel2.Controls.Add(this.cmbTargetMachine);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.btnReadFromPlc);
            this.panel2.Controls.Add(this.btnSendToPlc);
            this.panel2.Controls.Add(this.btnSaveRecipe);
            this.panel2.Controls.Add(this.txtRecipeName);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.btnCalculateCost);
            this.panel2.Controls.Add(this.pnlCost);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(725, 105);
            this.panel2.TabIndex = 0;
            // 
            // btnFtpSync
            // 
            this.btnFtpSync.AutoSize = false;
            this.btnFtpSync.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnFtpSync.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnFtpSync.Depth = 0;
            this.btnFtpSync.HighEmphasis = true;
            this.btnFtpSync.Icon = null;
            this.btnFtpSync.Location = new System.Drawing.Point(365, 8);
            this.btnFtpSync.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnFtpSync.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnFtpSync.Name = "btnFtpSync";
            this.btnFtpSync.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnFtpSync.Size = new System.Drawing.Size(260, 36);
            this.btnFtpSync.TabIndex = 7;
            this.btnFtpSync.Text = "REMOTE MACHINE OPERATIONS";
            this.btnFtpSync.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained; // Dolgulu material tarzı
            this.btnFtpSync.UseAccentColor = false;
            this.btnFtpSync.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Depth = 0;
            this.label4.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label4.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label4.Location = new System.Drawing.Point(10, 17);
            this.label4.MouseState = MaterialSkin.MouseState.HOVER;
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 17);
            this.label4.TabIndex = 5;
            this.label4.Text = "Target Machine:";
            // 
            // cmbTargetMachine
            // 
            this.cmbTargetMachine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTargetMachine.FormattingEnabled = true;
            this.cmbTargetMachine.Location = new System.Drawing.Point(107, 13);
            this.cmbTargetMachine.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbTargetMachine.Name = "cmbTargetMachine";
            this.cmbTargetMachine.Size = new System.Drawing.Size(246, 23);
            this.cmbTargetMachine.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Depth = 0;
            this.label3.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label3.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.label3.Location = new System.Drawing.Point(10, 63);
            this.label3.MouseState = MaterialSkin.MouseState.HOVER;
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 17);
            this.label3.TabIndex = 0;
            this.label3.Text = "Recipe Name:";
            // 
            // txtRecipeName
            // 
            this.txtRecipeName.Location = new System.Drawing.Point(107, 59);
            this.txtRecipeName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtRecipeName.Name = "txtRecipeName";
            this.txtRecipeName.Size = new System.Drawing.Size(246, 23);
            this.txtRecipeName.TabIndex = 1;
            // 
            // btnReadFromPlc
            // 
            this.btnReadFromPlc.AutoSize = false;
            this.btnReadFromPlc.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnReadFromPlc.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnReadFromPlc.Depth = 0;
            this.btnReadFromPlc.HighEmphasis = false;
            this.btnReadFromPlc.Icon = null;
            this.btnReadFromPlc.Location = new System.Drawing.Point(365, 53); // 36px material yüksekliğe göre dikey konum kalibre edildi
            this.btnReadFromPlc.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnReadFromPlc.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnReadFromPlc.Name = "btnReadFromPlc";
            this.btnReadFromPlc.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnReadFromPlc.Size = new System.Drawing.Size(82, 36);
            this.btnReadFromPlc.TabIndex = 4;
            this.btnReadFromPlc.Text = "Read PLC";
            this.btnReadFromPlc.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined; // Çizgili flat tarzı
            this.btnReadFromPlc.UseAccentColor = false;
            this.btnReadFromPlc.UseVisualStyleBackColor = true;
            // 
            // btnSaveRecipe
            // 
            this.btnSaveRecipe.AutoSize = false;
            this.btnSaveRecipe.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaveRecipe.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSaveRecipe.Depth = 0;
            this.btnSaveRecipe.HighEmphasis = true;
            this.btnSaveRecipe.Icon = null;
            this.btnSaveRecipe.Location = new System.Drawing.Point(453, 53);
            this.btnSaveRecipe.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSaveRecipe.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSaveRecipe.Name = "btnSaveRecipe";
            this.btnSaveRecipe.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSaveRecipe.Size = new System.Drawing.Size(82, 36);
            this.btnSaveRecipe.TabIndex = 2;
            this.btnSaveRecipe.Text = "Save";
            this.btnSaveRecipe.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained; // Dolgulu flat tarzı
            this.btnSaveRecipe.UseAccentColor = false;
            this.btnSaveRecipe.UseVisualStyleBackColor = true;
            // 
            // btnSendToPlc
            // 
            this.btnSendToPlc.AutoSize = false;
            this.btnSendToPlc.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSendToPlc.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSendToPlc.Depth = 0;
            this.btnSendToPlc.HighEmphasis = true;
            this.btnSendToPlc.Icon = null;
            this.btnSendToPlc.Location = new System.Drawing.Point(541, 53);
            this.btnSendToPlc.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSendToPlc.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSendToPlc.Name = "btnSendToPlc";
            this.btnSendToPlc.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSendToPlc.Size = new System.Drawing.Size(84, 36);
            this.btnSendToPlc.TabIndex = 3;
            this.btnSendToPlc.Text = "Send PLC";
            this.btnSendToPlc.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSendToPlc.UseAccentColor = true; // Dikkat çekici aksan fırçasıaktif
            this.btnSendToPlc.UseVisualStyleBackColor = true;
            // 
            // btnCalculateCost
            // 
            this.btnCalculateCost.AutoSize = false;
            this.btnCalculateCost.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCalculateCost.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnCalculateCost.Depth = 0;
            this.btnCalculateCost.HighEmphasis = false;
            this.btnCalculateCost.Icon = null;
            this.btnCalculateCost.Location = new System.Drawing.Point(365, 53);
            this.btnCalculateCost.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnCalculateCost.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnCalculateCost.Name = "btnCalculateCost";
            this.btnCalculateCost.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnCalculateCost.Size = new System.Drawing.Size(260, 36);
            this.btnCalculateCost.TabIndex = 8;
            this.btnCalculateCost.Text = "Calculate Estimated Cost";
            this.btnCalculateCost.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnCalculateCost.UseAccentColor = false;
            this.btnCalculateCost.UseVisualStyleBackColor = true;
            this.btnCalculateCost.Visible = false;
            // 
            // pnlCost
            // 
            this.pnlCost.BackColor = System.Drawing.Color.Transparent; // Koyu mod bütünlüğü için şeffaflaştırıldı
            this.pnlCost.Controls.Add(this.lblTotalCost);
            this.pnlCost.Controls.Add(this.lblCostTitle);
            this.pnlCost.Location = new System.Drawing.Point(635, 6);
            this.pnlCost.Name = "pnlCost";
            this.pnlCost.Size = new System.Drawing.Size(100, 90);
            this.pnlCost.TabIndex = 9;
            this.pnlCost.Visible = false;
            // 
            // lblCostTitle
            // 
            this.lblCostTitle.Depth = 0;
            this.lblCostTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCostTitle.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblCostTitle.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            this.lblCostTitle.Location = new System.Drawing.Point(0, 0);
            this.lblCostTitle.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblCostTitle.Name = "lblCostTitle";
            this.lblCostTitle.Size = new System.Drawing.Size(100, 25);
            this.lblCostTitle.TabIndex = 0;
            this.lblCostTitle.Text = "Estimated Cost";
            this.lblCostTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTotalCost
            // 
            this.lblTotalCost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalCost.Font = new System.Drawing.Font("Segoe UI Black", 12F, System.Drawing.FontStyle.Bold);
            this.lblTotalCost.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80))))); // Koyu mod uyumlu yumuşak yeşil fırça
            this.lblTotalCost.Location = new System.Drawing.Point(0, 25);
            this.lblTotalCost.Name = "lblTotalCost";
            this.lblTotalCost.Size = new System.Drawing.Size(100, 65);
            this.lblTotalCost.TabIndex = 1;
            this.lblTotalCost.Text = "0.00 $";
            this.lblTotalCost.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ProsesKontrol_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent; // Ebeveyn forma (Koyu/Açık temaya) tam şeffaf entegrasyon
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "ProsesKontrol_Control";
            this.Size = new System.Drawing.Size(939, 448);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.pnlCost.ResumeLayout(false);
            this.pnlSearch.ResumeLayout(false);
            this.pnlSearch.PerformLayout();
            this.pnlSort.ResumeLayout(false);
            this.pnlSort.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private MaterialSkin.Controls.MaterialLabel label1;                   // Tür güncellendi
        private System.Windows.Forms.ListBox lstRecipes;
        private System.Windows.Forms.ListBox lstRecipeHistory;
        private System.Windows.Forms.Panel panel1;
        private MaterialSkin.Controls.MaterialButton btnDeleteRecipe;         // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnNewRecipe;            // Tür güncellendi
        private System.Windows.Forms.Panel panel2;
        private MaterialSkin.Controls.MaterialButton btnSendToPlc;            // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnSaveRecipe;            // Tür güncellendi
        private System.Windows.Forms.TextBox txtRecipeName;
        private MaterialSkin.Controls.MaterialLabel label3;                   // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnReadFromPlc;          // Tür güncellendi
        private System.Windows.Forms.Panel pnlEditorArea;
        private System.Windows.Forms.ComboBox cmbTargetMachine;
        private MaterialSkin.Controls.MaterialLabel label4;                   // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnFtpSync;              // Tür güncellendi
        private System.Windows.Forms.Panel pnlCost;
        private System.Windows.Forms.Label lblTotalCost;
        private MaterialSkin.Controls.MaterialLabel lblCostTitle;             // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton btnCalculateCost;         // Tür güncellendi
        private MaterialSkin.Controls.MaterialButton yenile;                  // Tür güncellendi
        private System.Windows.Forms.Panel pnlSearch;
        private System.Windows.Forms.TextBox txtSearchRecipe;
        private MaterialSkin.Controls.MaterialLabel lblSearch;                // Tür güncellendi
        private System.Windows.Forms.Panel pnlSort;
        private MaterialSkin.Controls.MaterialRadioButton radioSortName;      // Tür güncellendi
        private MaterialSkin.Controls.MaterialRadioButton radioSortDate;      // Tür güncellendi
    }
}