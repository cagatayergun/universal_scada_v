// UI/Controls/RecipeStepEditors/StepEditor_Control.Designer.cs
namespace Telemetry.UI.Controls.RecipeStepEditors
{
    partial class StepEditor_Control
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
            this.pnlStepTypes = new System.Windows.Forms.Panel();
            this.chkSuAlma = new MaterialSkin.Controls.MaterialCheckbox();
            this.chkIsitma = new MaterialSkin.Controls.MaterialCheckbox();
            this.chkCalisma = new MaterialSkin.Controls.MaterialCheckbox();
            this.chkDozaj = new MaterialSkin.Controls.MaterialCheckbox();
            this.chkBosaltma = new MaterialSkin.Controls.MaterialCheckbox();
            this.chkSikma = new MaterialSkin.Controls.MaterialCheckbox();
            this.chknumune = new MaterialSkin.Controls.MaterialCheckbox();
            this.flpParameters = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlStepTypes.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlStepTypes
            // 
            this.pnlStepTypes.AutoScroll = true;
            this.pnlStepTypes.BackColor = System.Drawing.Color.Transparent;
            this.pnlStepTypes.BorderStyle = System.Windows.Forms.BorderStyle.None; // Flat görünüm için kenarlık kaldırıldı
            this.pnlStepTypes.Controls.Add(this.chkSikma);
            this.pnlStepTypes.Controls.Add(this.chknumune);
            this.pnlStepTypes.Controls.Add(this.chkBosaltma);
            this.pnlStepTypes.Controls.Add(this.chkDozaj);
            this.pnlStepTypes.Controls.Add(this.chkCalisma);
            this.pnlStepTypes.Controls.Add(this.chkIsitma);
            this.pnlStepTypes.Controls.Add(this.chkSuAlma);
            this.pnlStepTypes.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlStepTypes.Location = new System.Drawing.Point(0, 0);
            this.pnlStepTypes.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlStepTypes.Name = "pnlStepTypes";
            this.pnlStepTypes.Size = new System.Drawing.Size(206, 338);
            this.pnlStepTypes.TabIndex = 0;
            // 
            // chkSuAlma
            // 
            this.chkSuAlma.AutoSize = true;
            this.chkSuAlma.Depth = 0;
            this.chkSuAlma.Location = new System.Drawing.Point(9, 8); // Başlangıç noktası
            this.chkSuAlma.Margin = new System.Windows.Forms.Padding(0);
            this.chkSuAlma.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkSuAlma.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkSuAlma.Name = "chkSuAlma";
            this.chkSuAlma.ReadOnly = false;
            this.chkSuAlma.Ripple = true;
            this.chkSuAlma.Size = new System.Drawing.Size(126, 37);
            this.chkSuAlma.TabIndex = 0;
            this.chkSuAlma.Text = "TAKE WATER";
            this.chkSuAlma.UseVisualStyleBackColor = true;
            // 
            // chkIsitma
            // 
            this.chkIsitma.AutoSize = true;
            this.chkIsitma.Depth = 0;
            this.chkIsitma.Location = new System.Drawing.Point(9, 48); // Üst üste binmeyi önleyen 40px güvenli dikey aralık
            this.chkIsitma.Margin = new System.Windows.Forms.Padding(0);
            this.chkIsitma.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkIsitma.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkIsitma.Name = "chkIsitma";
            this.chkIsitma.ReadOnly = false;
            this.chkIsitma.Ripple = true;
            this.chkIsitma.Size = new System.Drawing.Size(100, 37);
            this.chkIsitma.TabIndex = 1;
            this.chkIsitma.Text = "HEATING";
            this.chkIsitma.UseVisualStyleBackColor = true;
            // 
            // chkCalisma
            // 
            this.chkCalisma.AutoSize = true;
            this.chkCalisma.Depth = 0;
            this.chkCalisma.Location = new System.Drawing.Point(9, 88);
            this.chkCalisma.Margin = new System.Windows.Forms.Padding(0);
            this.chkCalisma.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkCalisma.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkCalisma.Name = "chkCalisma";
            this.chkCalisma.ReadOnly = false;
            this.chkCalisma.Ripple = true;
            this.chkCalisma.Size = new System.Drawing.Size(109, 37);
            this.chkCalisma.TabIndex = 2;
            this.chkCalisma.Text = "WORKING";
            this.chkCalisma.UseVisualStyleBackColor = true;
            // 
            // chkDozaj
            // 
            this.chkDozaj.AutoSize = true;
            this.chkDozaj.Depth = 0;
            this.chkDozaj.Location = new System.Drawing.Point(9, 128);
            this.chkDozaj.Margin = new System.Windows.Forms.Padding(0);
            this.chkDozaj.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkDozaj.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkDozaj.Name = "chkDozaj";
            this.chkDozaj.ReadOnly = false;
            this.chkDozaj.Ripple = true;
            this.chkDozaj.Size = new System.Drawing.Size(97, 37);
            this.chkDozaj.TabIndex = 3;
            this.chkDozaj.Text = "DOSAGE";
            this.chkDozaj.UseVisualStyleBackColor = true;
            // 
            // chkBosaltma
            // 
            this.chkBosaltma.AutoSize = true;
            this.chkBosaltma.Depth = 0;
            this.chkBosaltma.Location = new System.Drawing.Point(9, 168);
            this.chkBosaltma.Margin = new System.Windows.Forms.Padding(0);
            this.chkBosaltma.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkBosaltma.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkBosaltma.Name = "chkBosaltma";
            this.chkBosaltma.ReadOnly = false;
            this.chkBosaltma.Ripple = true;
            this.chkBosaltma.Size = new System.Drawing.Size(124, 37);
            this.chkBosaltma.TabIndex = 4;
            this.chkBosaltma.Text = "UNLOADING";
            this.chkBosaltma.UseVisualStyleBackColor = true;
            // 
            // chkSikma
            // 
            this.chkSikma.AutoSize = true;
            this.chkSikma.Depth = 0;
            this.chkSikma.Location = new System.Drawing.Point(9, 208);
            this.chkSikma.Margin = new System.Windows.Forms.Padding(0);
            this.chkSikma.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkSikma.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkSikma.Name = "chkSikma";
            this.chkSikma.ReadOnly = false;
            this.chkSikma.Ripple = true;
            this.chkSikma.Size = new System.Drawing.Size(130, 37);
            this.chkSikma.TabIndex = 5;
            this.chkSikma.Text = "EXTRACTION";
            this.chkSikma.UseVisualStyleBackColor = true;
            // 
            // chknumune
            // 
            this.chknumune.AutoSize = true;
            this.chknumune.Depth = 0;
            this.chknumune.Location = new System.Drawing.Point(9, 248);
            this.chknumune.Margin = new System.Windows.Forms.Padding(0);
            this.chknumune.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chknumune.MouseState = MaterialSkin.MouseState.HOVER;
            this.chknumune.Name = "chknumune";
            this.chknumune.ReadOnly = false;
            this.chknumune.Ripple = true;
            this.chknumune.Size = new System.Drawing.Size(152, 37);
            this.chknumune.TabIndex = 6;
            this.chknumune.Text = "OPERATOR CALL";
            this.chknumune.UseVisualStyleBackColor = true;
            // 
            // flpParameters
            // 
            this.flpParameters.AutoScroll = true;
            this.flpParameters.BackColor = System.Drawing.Color.Transparent;
            this.flpParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpParameters.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpParameters.Location = new System.Drawing.Point(206, 0);
            this.flpParameters.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.flpParameters.Name = "flpParameters";
            this.flpParameters.Size = new System.Drawing.Size(144, 338);
            this.flpParameters.TabIndex = 7;
            this.flpParameters.WrapContents = false;
            // 
            // StepEditor_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.flpParameters);
            this.Controls.Add(this.pnlStepTypes);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "StepEditor_Control";
            this.Size = new System.Drawing.Size(350, 338);
            this.pnlStepTypes.ResumeLayout(false);
            this.pnlStepTypes.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlStepTypes;
        private MaterialSkin.Controls.MaterialCheckbox chkSuAlma;
        private MaterialSkin.Controls.MaterialCheckbox chkIsitma;
        private MaterialSkin.Controls.MaterialCheckbox chkCalisma;
        private MaterialSkin.Controls.MaterialCheckbox chkDozaj;
        private MaterialSkin.Controls.MaterialCheckbox chkBosaltma;
        private MaterialSkin.Controls.MaterialCheckbox chkSikma;
        private MaterialSkin.Controls.MaterialCheckbox chknumune;
        private System.Windows.Forms.FlowLayoutPanel flpParameters;
    }
}