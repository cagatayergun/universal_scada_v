// UI/Views/Ayarlar_Control.Designer.cs
namespace Telemetry.UI.Views
{
    partial class Ayarlar_Control
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
            this.tabSelector = new MaterialSkin.Controls.MaterialTabSelector(); // YENİ: KAYBOLAN SEKMELERİ GERİ GETİREN MOTOR
            this.tabControlSettings = new MaterialSkin.Controls.MaterialTabControl();
            this.tabPageMachineSettings = new System.Windows.Forms.TabPage();
            this.tabPageUserSettings = new System.Windows.Forms.TabPage();
            this.tabPageAlarmSettings = new System.Windows.Forms.TabPage();
            this.tabPageDowntimeReasons = new System.Windows.Forms.TabPage();
            this.tabPagePlcOperators = new System.Windows.Forms.TabPage();
            this.tabPageUtilitySettings = new System.Windows.Forms.TabPage();
            this.tabPageRecipeDesigner = new System.Windows.Forms.TabPage();
            this.tabControlSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabSelector (SEKMELERİ GÖSTEREN ÜST PANEL)
            // 
            this.tabSelector.BaseTabControl = this.tabControlSettings; // TAB KONTROLE BAĞLANDI
            this.tabSelector.CharacterCasing = MaterialSkin.Controls.MaterialTabSelector.CustomCharacterCasing.Normal;
            this.tabSelector.Depth = 0;
            this.tabSelector.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabSelector.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.tabSelector.Location = new System.Drawing.Point(0, 0);
            this.tabSelector.MouseState = MaterialSkin.MouseState.HOVER;
            this.tabSelector.Name = "tabSelector";
            this.tabSelector.Size = new System.Drawing.Size(700, 48); // Sekmelerin yüksekliği material standardı
            this.tabSelector.TabIndex = 1;
            this.tabSelector.Text = "materialTabSelector1";
            // 
            // tabControlSettings
            // 
            this.tabControlSettings.Controls.Add(this.tabPageMachineSettings);
            this.tabControlSettings.Controls.Add(this.tabPageUserSettings);
            this.tabControlSettings.Controls.Add(this.tabPageAlarmSettings);
            this.tabControlSettings.Controls.Add(this.tabPageDowntimeReasons);
            this.tabControlSettings.Controls.Add(this.tabPagePlcOperators);
            this.tabControlSettings.Controls.Add(this.tabPageUtilitySettings);
            this.tabControlSettings.Controls.Add(this.tabPageRecipeDesigner);
            this.tabControlSettings.Depth = 0;
            this.tabControlSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlSettings.Location = new System.Drawing.Point(0, 48); // Selector'un altına bağlandı
            this.tabControlSettings.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControlSettings.MouseState = MaterialSkin.MouseState.HOVER;
            this.tabControlSettings.Multiline = true;
            this.tabControlSettings.Name = "tabControlSettings";
            this.tabControlSettings.SelectedIndex = 0;
            this.tabControlSettings.Size = new System.Drawing.Size(700, 402); // 450px - 48px Header
            this.tabControlSettings.TabIndex = 0;
            // 
            // tabPageMachineSettings
            // 
            this.tabPageMachineSettings.BackColor = System.Drawing.Color.Transparent;
            this.tabPageMachineSettings.Location = new System.Drawing.Point(4, 24);
            this.tabPageMachineSettings.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageMachineSettings.Name = "tabPageMachineSettings";
            this.tabPageMachineSettings.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageMachineSettings.Size = new System.Drawing.Size(692, 374);
            this.tabPageMachineSettings.TabIndex = 0;
            this.tabPageMachineSettings.Text = "Machine Management";
            // 
            // tabPageUserSettings
            // 
            this.tabPageUserSettings.BackColor = System.Drawing.Color.Transparent;
            this.tabPageUserSettings.Location = new System.Drawing.Point(4, 24);
            this.tabPageUserSettings.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageUserSettings.Name = "tabPageUserSettings";
            this.tabPageUserSettings.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageUserSettings.Size = new System.Drawing.Size(692, 374);
            this.tabPageUserSettings.TabIndex = 1;
            this.tabPageUserSettings.Text = "User Management";
            // 
            // tabPageAlarmSettings
            // 
            this.tabPageAlarmSettings.BackColor = System.Drawing.Color.Transparent;
            this.tabPageAlarmSettings.Location = new System.Drawing.Point(4, 24);
            this.tabPageAlarmSettings.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageAlarmSettings.Name = "tabPageAlarmSettings";
            this.tabPageAlarmSettings.Size = new System.Drawing.Size(692, 374);
            this.tabPageAlarmSettings.TabIndex = 2;
            this.tabPageAlarmSettings.Text = "Alarm Settings";
            // 
            // tabPageDowntimeReasons
            // 
            this.tabPageDowntimeReasons.BackColor = System.Drawing.Color.Transparent;
            this.tabPageDowntimeReasons.Location = new System.Drawing.Point(4, 24);
            this.tabPageDowntimeReasons.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageDowntimeReasons.Name = "tabPageDowntimeReasons";
            this.tabPageDowntimeReasons.Size = new System.Drawing.Size(692, 374);
            this.tabPageDowntimeReasons.TabIndex = 3;
            this.tabPageDowntimeReasons.Text = "Down Time Settings";
            // 
            // tabPagePlcOperators
            // 
            this.tabPagePlcOperators.BackColor = System.Drawing.Color.Transparent;
            this.tabPagePlcOperators.Location = new System.Drawing.Point(4, 24);
            this.tabPagePlcOperators.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPagePlcOperators.Name = "tabPagePlcOperators";
            this.tabPagePlcOperators.Size = new System.Drawing.Size(692, 374);
            this.tabPagePlcOperators.TabIndex = 4;
            this.tabPagePlcOperators.Text = "Plc Operator Management";
            // 
            // tabPageUtilitySettings
            // 
            this.tabPageUtilitySettings.BackColor = System.Drawing.Color.Transparent;
            this.tabPageUtilitySettings.Location = new System.Drawing.Point(4, 24);
            this.tabPageUtilitySettings.Name = "tabPageUtilitySettings";
            this.tabPageUtilitySettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageUtilitySettings.Size = new System.Drawing.Size(692, 374);
            this.tabPageUtilitySettings.TabIndex = 5;
            this.tabPageUtilitySettings.Text = "Line Usage Settings";
            // 
            // tabPageRecipeDesigner
            // 
            this.tabPageRecipeDesigner.BackColor = System.Drawing.Color.Transparent;
            this.tabPageRecipeDesigner.Location = new System.Drawing.Point(4, 24);
            this.tabPageRecipeDesigner.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageRecipeDesigner.Name = "tabPageRecipeDesigner";
            this.tabPageRecipeDesigner.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageRecipeDesigner.Size = new System.Drawing.Size(692, 374);
            this.tabPageRecipeDesigner.TabIndex = 6;
            this.tabPageRecipeDesigner.Text = "Recipe Designer";
            // 
            // Ayarlar_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.tabControlSettings);
            this.Controls.Add(this.tabSelector); // TAB SELECTOR FORMA EKLENDİ
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Ayarlar_Control";
            this.Size = new System.Drawing.Size(700, 450);
            this.tabControlSettings.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private MaterialSkin.Controls.MaterialTabSelector tabSelector; // Eklenen sekme barı nesnesi
        private MaterialSkin.Controls.MaterialTabControl tabControlSettings;
        private System.Windows.Forms.TabPage tabPageMachineSettings;
        private System.Windows.Forms.TabPage tabPageUserSettings;
        private System.Windows.Forms.TabPage tabPageAlarmSettings;
        private System.Windows.Forms.TabPage tabPagePlcOperators;
        private System.Windows.Forms.TabPage tabPageUtilitySettings;
        private System.Windows.Forms.TabPage tabPageRecipeDesigner;
        private System.Windows.Forms.TabPage tabPageDowntimeReasons;
    }
}