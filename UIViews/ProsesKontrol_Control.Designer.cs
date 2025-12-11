// UI/Views/ProsesKontrol_Control.Designer.cs
using TekstilScada.Services;

namespace TekstilScada.UI.Views
{
    partial class ProsesKontrol_Control
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                FtpTransferService.Instance.RecipeListChanged += OnRecipeListChanged;
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        private void InitializeComponent()
        {
            splitContainer1 = new SplitContainer();
            lstRecipes = new ListBox();
            panel1 = new Panel();
            yenile = new Button();
            btnDeleteRecipe = new Button();
            btnNewRecipe = new Button();
            label1 = new Label();
            pnlEditorArea = new Panel();
            panel2 = new Panel();
            btnFtpSync = new Button();
            cmbTargetMachine = new ComboBox();
            label4 = new Label();
            btnReadFromPlc = new Button();
            btnSendToPlc = new Button();
            btnSaveRecipe = new Button();
            txtRecipeName = new TextBox();
            label3 = new Label();
            btnCalculateCost = new Button();
            pnlCost = new Panel();
            lblTotalCost = new Label();
            lblCostTitle = new Label();
            this.pnlSearch = new System.Windows.Forms.Panel();
            this.txtSearchRecipe = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.pnlSort = new System.Windows.Forms.Panel();
            this.radioSortName = new System.Windows.Forms.RadioButton();
            this.radioSortDate = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            this.pnlSearch.SuspendLayout(); // YENİ
            this.pnlSort.SuspendLayout();   // YENİ
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            pnlCost.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(3, 2, 3, 2);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(lstRecipes);
            splitContainer1.Panel1.Controls.Add(panel1);
            this.splitContainer1.Panel1.Controls.Add(this.lstRecipes); // Fill (En altta kalacak, diğerleri üstüne binecek)
            this.splitContainer1.Panel1.Controls.Add(this.pnlSort);    // Top (Aramanın altında)
            this.splitContainer1.Panel1.Controls.Add(this.pnlSearch);  // Top (Başlığın altında)
            this.splitContainer1.Panel1.Controls.Add(this.label1);     // Top (En üstte)
            this.splitContainer1.Panel1.Controls.Add(this.panel1);     // Bottom (Butonlar)
            splitContainer1.Panel1.Controls.Add(label1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(pnlEditorArea);
            splitContainer1.Panel2.Controls.Add(panel2);
            splitContainer1.Size = new Size(939, 448);
            splitContainer1.SplitterDistance = 169;
            splitContainer1.TabIndex = 0;
            // 
            // lstRecipes
            // 
            this.lstRecipes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstRecipes.BringToFront(); // En öne getir ki diğer panellerin altında kalmasın (Fill mantığı)
            lstRecipes.FormattingEnabled = true;
            lstRecipes.ItemHeight = 15;
            lstRecipes.Location = new Point(0, 22);
            lstRecipes.Margin = new Padding(3, 2, 3, 2);
            lstRecipes.Name = "lstRecipes";
            lstRecipes.SelectionMode = SelectionMode.MultiExtended;
            lstRecipes.Size = new Size(169, 388);
            lstRecipes.TabIndex = 1;
            // 
            // panel1
            // 
            panel1.Controls.Add(yenile);
            panel1.Controls.Add(btnDeleteRecipe);
            panel1.Controls.Add(btnNewRecipe);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 410);
            panel1.Margin = new Padding(3, 2, 3, 2);
            panel1.Name = "panel1";
            panel1.Size = new Size(169, 38);
            panel1.TabIndex = 2;
            // 
            // yenile
            // 
            yenile.Dock = DockStyle.Left;
            yenile.Location = new Point(164, 0);
            yenile.Margin = new Padding(3, 2, 3, 2);
            yenile.Name = "yenile";
            yenile.Size = new Size(82, 38);
            yenile.TabIndex = 2;
            yenile.Text = "Refresh";
            yenile.UseVisualStyleBackColor = true;
            yenile.Click += yenile_Click;
            // 
            // pnlSearch (YENİ - Arama Paneli)
            // 
            this.pnlSearch.Controls.Add(this.txtSearchRecipe);
            this.pnlSearch.Controls.Add(this.lblSearch);
            this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearch.Location = new System.Drawing.Point(0, 22); // label1'in altı
            this.pnlSearch.Name = "pnlSearch";
            this.pnlSearch.Padding = new System.Windows.Forms.Padding(5);
            this.pnlSearch.Size = new System.Drawing.Size(169, 50);
            this.pnlSearch.TabIndex = 3;

            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSearch.Location = new System.Drawing.Point(5, 5);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(45, 15);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search:";

            // 
            // txtSearchRecipe
            // 
            this.txtSearchRecipe.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtSearchRecipe.Location = new System.Drawing.Point(5, 20);
            this.txtSearchRecipe.Name = "txtSearchRecipe";
            this.txtSearchRecipe.Size = new System.Drawing.Size(159, 23);
            this.txtSearchRecipe.TabIndex = 1;
            this.txtSearchRecipe.TextChanged += new System.EventHandler(this.txtSearchRecipe_TextChanged);

            // 
            // pnlSort (YENİ - Sıralama Paneli)
            // 
            this.pnlSort.Controls.Add(this.radioSortDate);
            this.pnlSort.Controls.Add(this.radioSortName);
            this.pnlSort.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSort.Location = new System.Drawing.Point(0, 72); // pnlSearch'in altı
            this.pnlSort.Name = "pnlSort";
            this.pnlSort.Size = new System.Drawing.Size(169, 30);
            this.pnlSort.TabIndex = 4;

            // 
            // radioSortName
            // 
            this.radioSortName.AutoSize = true;
            this.radioSortName.Checked = true; // Varsayılan: İsim
            this.radioSortName.Location = new System.Drawing.Point(5, 5);
            this.radioSortName.Name = "radioSortName";
            this.radioSortName.Size = new System.Drawing.Size(53, 19);
            this.radioSortName.TabIndex = 0;
            this.radioSortName.TabStop = true;
            this.radioSortName.Text = "A-Z";
            this.radioSortName.UseVisualStyleBackColor = true;
            this.radioSortName.CheckedChanged += new System.EventHandler(this.SortOption_CheckedChanged);

            // 
            // radioSortDate
            // 
            this.radioSortDate.AutoSize = true;
            this.radioSortDate.Location = new System.Drawing.Point(70, 5);
            this.radioSortDate.Name = "radioSortDate";
            this.radioSortDate.Size = new System.Drawing.Size(73, 19);
            this.radioSortDate.TabIndex = 1;
            this.radioSortDate.Text = "Newest"; // Veya "Date"
            this.radioSortDate.UseVisualStyleBackColor = true;
            this.radioSortDate.CheckedChanged += new System.EventHandler(this.SortOption_CheckedChanged);
            // 
            // btnDeleteRecipe
            // 
            btnDeleteRecipe.Dock = DockStyle.Left;
            btnDeleteRecipe.Location = new Point(82, 0);
            btnDeleteRecipe.Margin = new Padding(3, 2, 3, 2);
            btnDeleteRecipe.Name = "btnDeleteRecipe";
            btnDeleteRecipe.Size = new Size(82, 38);
            btnDeleteRecipe.TabIndex = 1;
            btnDeleteRecipe.Text = "Delete";
            btnDeleteRecipe.UseVisualStyleBackColor = true;
            // 
            // btnNewRecipe
            // 
            btnNewRecipe.Dock = DockStyle.Left;
            btnNewRecipe.Location = new Point(0, 0);
            btnNewRecipe.Margin = new Padding(3, 2, 3, 2);
            btnNewRecipe.Name = "btnNewRecipe";
            btnNewRecipe.Size = new Size(82, 38);
            btnNewRecipe.TabIndex = 0;
            btnNewRecipe.Text = "New";
            btnNewRecipe.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.Dock = DockStyle.Top;
            label1.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(169, 22);
            label1.TabIndex = 0;
            label1.Text = "Registered Recipes";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlEditorArea
            // 
            pnlEditorArea.Dock = DockStyle.Fill;
            pnlEditorArea.Location = new Point(0, 100);
            pnlEditorArea.Margin = new Padding(3, 2, 3, 2);
            pnlEditorArea.Name = "pnlEditorArea";
            pnlEditorArea.Size = new Size(766, 348);
            pnlEditorArea.TabIndex = 1;
            // 
            // panel2
            // 
            panel2.Controls.Add(btnFtpSync);
            panel2.Controls.Add(cmbTargetMachine);
            panel2.Controls.Add(label4);
            panel2.Controls.Add(btnReadFromPlc);
            panel2.Controls.Add(btnSendToPlc);
            panel2.Controls.Add(btnSaveRecipe);
            panel2.Controls.Add(txtRecipeName);
            panel2.Controls.Add(label3);
            panel2.Controls.Add(btnCalculateCost);
            panel2.Controls.Add(pnlCost);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Margin = new Padding(3, 2, 3, 2);
            panel2.Name = "panel2";
            panel2.Size = new Size(766, 100);
            panel2.TabIndex = 0;
            // 
            // btnFtpSync
            // 
            btnFtpSync.BackColor = Color.CornflowerBlue;
            btnFtpSync.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnFtpSync.ForeColor = Color.White;
            btnFtpSync.Location = new Point(359, 6);
            btnFtpSync.Margin = new Padding(3, 2, 3, 2);
            btnFtpSync.Name = "btnFtpSync";
            btnFtpSync.Size = new Size(386, 37);
            btnFtpSync.TabIndex = 7;
            btnFtpSync.Text = "REMOTE MACHINE OPERATIONS";
            btnFtpSync.UseVisualStyleBackColor = false;
            // 
            // cmbTargetMachine
            // 
            cmbTargetMachine.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTargetMachine.FormattingEnabled = true;
            cmbTargetMachine.Location = new Point(107, 13);
            cmbTargetMachine.Margin = new Padding(3, 2, 3, 2);
            cmbTargetMachine.Name = "cmbTargetMachine";
            cmbTargetMachine.Size = new Size(246, 23);
            cmbTargetMachine.TabIndex = 6;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label4.Location = new Point(10, 17);
            label4.Name = "label4";
            label4.Size = new Size(96, 15);
            label4.TabIndex = 5;
            label4.Text = "Target Machine:";
            // 
            // btnReadFromPlc
            // 
            btnReadFromPlc.Location = new Point(359, 47);
            btnReadFromPlc.Margin = new Padding(3, 2, 3, 2);
            btnReadFromPlc.Name = "btnReadFromPlc";
            btnReadFromPlc.Size = new Size(88, 47);
            btnReadFromPlc.TabIndex = 4;
            btnReadFromPlc.Text = "Read from PLC";
            btnReadFromPlc.UseVisualStyleBackColor = true;
            // 
            // btnSendToPlc
            // 
            btnSendToPlc.Location = new Point(657, 47);
            btnSendToPlc.Margin = new Padding(3, 2, 3, 2);
            btnSendToPlc.Name = "btnSendToPlc";
            btnSendToPlc.Size = new Size(88, 47);
            btnSendToPlc.TabIndex = 3;
            btnSendToPlc.Text = "Send to PLC";
            btnSendToPlc.UseVisualStyleBackColor = true;
            // 
            // btnSaveRecipe
            // 
            btnSaveRecipe.Location = new Point(511, 47);
            btnSaveRecipe.Margin = new Padding(3, 2, 3, 2);
            btnSaveRecipe.Name = "btnSaveRecipe";
            btnSaveRecipe.Size = new Size(82, 47);
            btnSaveRecipe.TabIndex = 2;
            btnSaveRecipe.Text = "Save";
            btnSaveRecipe.UseVisualStyleBackColor = true;
            // 
            // txtRecipeName
            // 
            txtRecipeName.Location = new Point(107, 59);
            txtRecipeName.Margin = new Padding(3, 2, 3, 2);
            txtRecipeName.Name = "txtRecipeName";
            txtRecipeName.Size = new Size(246, 23);
            txtRecipeName.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label3.Location = new Point(10, 63);
            label3.Name = "label3";
            label3.Size = new Size(84, 15);
            label3.TabIndex = 0;
            label3.Text = "Recipe Name:";
            // 
            // btnCalculateCost
            // 
            btnCalculateCost.BackColor = Color.DarkSlateGray;
            btnCalculateCost.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCalculateCost.ForeColor = Color.White;
            btnCalculateCost.Location = new Point(359, 64);
            btnCalculateCost.Name = "btnCalculateCost";
            btnCalculateCost.Size = new Size(280, 30);
            btnCalculateCost.TabIndex = 8;
            btnCalculateCost.Text = "Calculate Estimated Cost";
            btnCalculateCost.UseVisualStyleBackColor = false;
            btnCalculateCost.Visible = false;
            btnCalculateCost.Click += btnCalculateCost_Click;
            // 
            // pnlCost
            // 
            pnlCost.BackColor = SystemColors.Info;
            pnlCost.Controls.Add(lblTotalCost);
            pnlCost.Controls.Add(lblCostTitle);
            pnlCost.Location = new Point(645, 6);
            pnlCost.Name = "pnlCost";
            pnlCost.Size = new Size(100, 90);
            pnlCost.TabIndex = 9;
            pnlCost.Visible = false;
            // 
            // lblTotalCost
            // 
            lblTotalCost.Dock = DockStyle.Fill;
            lblTotalCost.Font = new Font("Segoe UI", 13.8F, FontStyle.Bold);
            lblTotalCost.ForeColor = Color.FromArgb(0, 64, 0);
            lblTotalCost.Location = new Point(0, 25);
            lblTotalCost.Name = "lblTotalCost";
            lblTotalCost.Size = new Size(100, 65);
            lblTotalCost.TabIndex = 1;
            lblTotalCost.Text = "0.00 $";
            lblTotalCost.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblCostTitle
            // 
            lblCostTitle.Dock = DockStyle.Top;
            lblCostTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCostTitle.Location = new Point(0, 0);
            lblCostTitle.Name = "lblCostTitle";
            lblCostTitle.Size = new Size(100, 25);
            lblCostTitle.TabIndex = 0;
            lblCostTitle.Text = "Estimated Cost";
            lblCostTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ProsesKontrol_Control
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Margin = new Padding(3, 2, 3, 2);
            Name = "ProsesKontrol_Control";
            Size = new Size(939, 448);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            pnlCost.ResumeLayout(false);
            ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox lstRecipes;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnDeleteRecipe;
        private System.Windows.Forms.Button btnNewRecipe;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnSendToPlc;
        private System.Windows.Forms.Button btnSaveRecipe;
        private System.Windows.Forms.TextBox txtRecipeName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnReadFromPlc;
        private System.Windows.Forms.Panel pnlEditorArea;
        private System.Windows.Forms.ComboBox cmbTargetMachine;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnFtpSync;
        private System.Windows.Forms.Panel pnlCost;
        private System.Windows.Forms.Label lblTotalCost;
        private System.Windows.Forms.Label lblCostTitle;
        private System.Windows.Forms.Button btnCalculateCost;
        private Button yenile;
        private System.Windows.Forms.Panel pnlSearch;
        private System.Windows.Forms.TextBox txtSearchRecipe;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.Panel pnlSort;
        private System.Windows.Forms.RadioButton radioSortName;
        private System.Windows.Forms.RadioButton radioSortDate;
    }
}
