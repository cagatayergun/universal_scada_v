// UI/Views/Ayarlar_Control.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Telemetry.Core;
using Telemetry.Properties;
using Telemetry.Repositories;
using Telemetry.Services;
using Telemetry.UIViews;
using MaterialSkin;          // YENİ EKLENDİ: MaterialSkin ana kütüphanesi
using MaterialSkin.Controls; // YENİ EKLENDİ: Material bileşen desteği

namespace Telemetry.UI.Views
{
    public partial class Ayarlar_Control : UserControl
    {
        public event System.EventHandler MachineListChanged;

        private readonly MachineSettings_Control _machineSettings;
        private readonly UserSettings_Control _userSettings;
        private readonly AlarmSettings_Control _alarmSettings;
        private readonly PlcOperatorSettings_Control _plcOperatorSettings;
        private readonly CostSettings_Control _costSettings;
        private readonly RecipeStepDesigner_Control _recipeStepDesigner;
        private readonly UtilitySettings_Control _utilitySettings;
        private readonly DowntimeSettings_Control _downtimeSettings;

        public Ayarlar_Control()
        {
            // Statik dil değişim olayına kayıt
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;

            InitializeComponent();

            // UX OPTMİZASYONU: Kontrolün arkasında çiğ renk blokları kalmaması için şeffaflığı açıyoruz
            this.BackColor = System.Drawing.Color.Transparent;
            this.DoubleBuffered = true; // Genel kırpışma koruması

            // SPEED OPTİMİZASYON: Tüm alt paneller yüklenirken arayüz yerleşim motorunu askıya alıyoruz
            this.SuspendLayout();

            try
            {
                // Alt kontrol birimlerinin örneklenmesi
                _machineSettings = new MachineSettings_Control();
                _userSettings = new UserSettings_Control();
                _alarmSettings = new AlarmSettings_Control();

                _downtimeSettings = new DowntimeSettings_Control();
                _downtimeSettings.Dock = DockStyle.Fill;
                tabPageDowntimeReasons.Controls.Add(_downtimeSettings);

                _plcOperatorSettings = new PlcOperatorSettings_Control();
                _costSettings = new CostSettings_Control();
                _recipeStepDesigner = new RecipeStepDesigner_Control();

                _machineSettings.MachineListChanged += (sender, args) => { MachineListChanged?.Invoke(this, args); };
                _utilitySettings = new UtilitySettings_Control();

                // Kontrollerin yuvalarına yerleştirilmesi (Docking ve Tab Kontrolü)
                _machineSettings.Dock = DockStyle.Fill;
                tabPageMachineSettings.Controls.Add(_machineSettings);

                _userSettings.Dock = DockStyle.Fill;
                tabPageUserSettings.Controls.Add(_userSettings);

                _alarmSettings.Dock = DockStyle.Fill;
                tabPageAlarmSettings.Controls.Add(_alarmSettings);

                _plcOperatorSettings.Dock = DockStyle.Fill;
                tabPagePlcOperators.Controls.Add(_plcOperatorSettings);

                _costSettings.Dock = DockStyle.Fill;
                // Örnek: tabPageCostSettings.Controls.Add(_costSettings);

                _utilitySettings.Dock = DockStyle.Fill;
                tabPageUtilitySettings.Controls.Add(_utilitySettings);

                _recipeStepDesigner.Dock = DockStyle.Fill;
                tabPageRecipeDesigner.Controls.Add(_recipeStepDesigner);

                ApplyPermissions();
            }
            finally
            {
                // Tüm alt paneller başarıyla eklendiğinde tek bir karede ekrana çizimi gerçekleştir
                this.ResumeLayout(true);
            }
        }

        public void RefreshUserRoles()
        {
            _userSettings.LoadAllRoles();
        }

        public void ApplyPermissions1()
        {
            ApplyPermissions();
        }

        private void ApplyPermissions()
        {
            if (this.IsDisposed) return;

            // Rol ve yetkilendirme mimarisine göre görünürlük / aktiflik kontrolleri
            _machineSettings.Visible = PermissionService.HasAnyPermission(new List<int> { 6 });
            _userSettings.Visible = PermissionService.HasAnyPermission(new List<int> { 7 });
            _alarmSettings.Enabled = PermissionService.HasAnyPermission(new List<int> { 8 });
            _costSettings.Enabled = PermissionService.HasAnyPermission(new List<int> { 9 });
            _plcOperatorSettings.Enabled = PermissionService.HasAnyPermission(new List<int> { 10 });
            _recipeStepDesigner.Visible = PermissionService.HasAnyPermission(new List<int> { 11 });
            _downtimeSettings.Enabled = PermissionService.HasAnyPermission(new List<int> { 8 });
            _utilitySettings.Visible = PermissionService.HasAnyPermission(new List<int> { 6, 1000 });
            _utilitySettings.Visible = false; // Mevcut orijinal mantık akışı korundu

            var master = PermissionService.HasAnyPermission(new List<int> { 1000 });
            if (master)
            {
                _machineSettings.Visible = PermissionService.HasAnyPermission(new List<int> { 1000 });
                _userSettings.Visible = PermissionService.HasAnyPermission(new List<int> { 1000 });
                _alarmSettings.Enabled = PermissionService.HasAnyPermission(new List<int> { 1000 });
                _downtimeSettings.Enabled = PermissionService.HasAnyPermission(new List<int> { 1000 });
                _costSettings.Enabled = PermissionService.HasAnyPermission(new List<int> { 1000 });
                _plcOperatorSettings.Enabled = PermissionService.HasAnyPermission(new List<int> { 1000 });
                _recipeStepDesigner.Visible = PermissionService.HasAnyPermission(new List<int> { 1000 });
                _utilitySettings.Visible = true;
                _utilitySettings.Visible = false;
            }
        }

        public void RefreshMachineSettingsView()
        {
            _machineSettings.RefreshMachineList();
        }

        public void InitializeControl(MachineRepository machineRepo,
                                      EfficiencyRepository efficiencyRepo,
                                      Dictionary<int, IPlcManager> plcManagers,
                                      PlcPollingService pollingService)
        {
            _plcOperatorSettings.InitializeControl(machineRepo, plcManagers);
            _downtimeSettings.InitializeControl(efficiencyRepo, pollingService);
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
        }

        public void ApplyLocalization()
        {
            tabPageMachineSettings.Text = Resources.MachineManagement;
            tabPageUserSettings.Text = Resources.UserManagement;
            tabPageAlarmSettings.Text = Resources.AlarmSettings;
            tabPagePlcOperators.Text = Resources.PlcOperatorManagement;
            tabPageRecipeDesigner.Text = Resources.recipedesigner;
            tabPageUtilitySettings.Text = "Line Usage Settings";
            tabPageDowntimeReasons.Text = "Down Time Settings";
        }

        // =========================================================================
        // BELLEK SIZINTISI KORUMASI: STATİK EVENT BAĞLANTI TEMİZLİĞİ
        // Kontrol kapatıldığında veya sekmeler değiştiğinde RAM'de asılı kalmasını önler.
        // =========================================================================
        protected override void OnHandleDestroyed(EventArgs e)
        {
            LanguageManager.LanguageChanged -= LanguageManager_LanguageChanged;
            base.OnHandleDestroyed(e);
        }
    }
}