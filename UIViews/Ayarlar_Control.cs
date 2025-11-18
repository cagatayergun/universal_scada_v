// UI/Views/Ayarlar_Control.cs
using System.Collections.Generic;
using System.Windows.Forms;
using Universalscada.core;
using Universalscada.Core.Repositories;
using Universalscada.Properties;
using Universalscada.Repositories;
using Universalscada.Services; // Bu using artık gereksiz olabilir

namespace Universalscada.UI.Views
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
        private IMachineRepository _machineRepository;
        public Ayarlar_Control()
        {
            InitializeComponent();
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;
            _machineSettings = new MachineSettings_Control();
            _userSettings = new UserSettings_Control();
            _alarmSettings = new AlarmSettings_Control();
            _plcOperatorSettings = new PlcOperatorSettings_Control();
            _costSettings = new CostSettings_Control();
            _recipeStepDesigner = new RecipeStepDesigner_Control();
            _machineSettings.MachineListChanged += (sender, args) => { MachineListChanged?.Invoke(this, args); };

            _machineSettings.Dock = DockStyle.Fill;
            tabPageMachineSettings.Controls.Add(_machineSettings);

            _userSettings.Dock = DockStyle.Fill;
            tabPageUserSettings.Controls.Add(_userSettings);

            _alarmSettings.Dock = DockStyle.Fill;
            tabPageAlarmSettings.Controls.Add(_alarmSettings);

            _plcOperatorSettings.Dock = DockStyle.Fill;
            tabPagePlcOperators.Controls.Add(_plcOperatorSettings);

            _costSettings.Dock = DockStyle.Fill;
            tabPageCostSettings.Controls.Add(_costSettings);

            _recipeStepDesigner.Dock = DockStyle.Fill;
            tabPageRecipeDesigner.Controls.Add(_recipeStepDesigner);
            ApplyPermissions();
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
            // ... (Bu metotta hiçbir değişiklik yok) ...
            _machineSettings.Visible = PermissionService.HasAnyPermission(new List<int> { 6 });
            _userSettings.Visible = PermissionService.HasAnyPermission(new List<int> { 7 });
            _alarmSettings.Enabled = PermissionService.HasAnyPermission(new List<int> { 8 });
            _costSettings.Enabled = PermissionService.HasAnyPermission(new List<int> { 9 });
            _plcOperatorSettings.Enabled = PermissionService.HasAnyPermission(new List<int> { 10 });
            _recipeStepDesigner.Visible = PermissionService.HasAnyPermission(new List<int> { 11 });

            var master = PermissionService.HasAnyPermission(new List<int> { 1000 });
            if (master == true)
            {
                _machineSettings.Visible = PermissionService.HasAnyPermission(new List<int> { 1000 });
                _userSettings.Visible = PermissionService.HasAnyPermission(new List<int> { 1000 });
                _alarmSettings.Enabled = PermissionService.HasAnyPermission(new List<int> { 1000 });
                _costSettings.Enabled = PermissionService.HasAnyPermission(new List<int> { 1000 });
                _plcOperatorSettings.Enabled = PermissionService.HasAnyPermission(new List<int> { 1000 });
                _recipeStepDesigner.Visible = PermissionService.HasAnyPermission(new List<int> { 1000 });
            }
        }

        public void RefreshMachineSettingsView()
        {
            _machineSettings.RefreshMachineList();
        }

        // === DEĞİŞTİ: InitializeControl ===
        // 'plcManagers' parametresi kaldırıldı.
        public void InitializeControl(IMachineRepository machineRepository)
        {
            _machineRepository = machineRepository;
            // _plcOperatorSettings'e artık 'plcManagers' geçilmiyor.
            // BİR SONRAKİ ADIM: 'PlcOperatorSettings_Control.cs' dosyasını düzenlemek olacak.
            _plcOperatorSettings.InitializeControl(machineRepo);
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
        }

        public void ApplyLocalization()
        {
            // ... (Bu metotta hiçbir değişiklik yok) ...
            tabPageMachineSettings.Text = Resources.MachineManagementTitle;
            tabPageUserSettings.Text = Resources.UserManagementTitle;
            tabPageAlarmSettings.Text = Resources.AlarmDefinitionTitle;
            tabPageCostSettings.Text = Resources.CostParametersTitle;
            tabPagePlcOperators.Text = Resources.PlcOperatorManagementTitle;
            tabPageRecipeDesigner.Text = Resources.RecipeStepDesignerTitle;
        }
    }
}