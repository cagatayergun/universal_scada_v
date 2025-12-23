using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks; // Task kullanýmý için gerekli
using TekstilScada.Core;
using TekstilScada.Localization;
using TekstilScada.Models;
using TekstilScada.Properties;
using TekstilScada.Repositories;
using TekstilScada.Services;
using TekstilScada.UI;
using TekstilScada.UI.Controls;

using TekstilScada.UI.Views;

namespace TekstilScada
{
    public partial class MainForm : Form
    {
        // --- REPOSITORY VE SERVÝSLER ---
        private readonly FtpTransferService _ftpTransferService;
        private readonly MachineRepository _machineRepository;
        private readonly RecipeRepository _recipeRepository;
        private readonly ProcessLogRepository _processLogRepository;
        private readonly AlarmRepository _alarmRepository;
        private readonly ProductionRepository _productionRepository;
        private readonly PlcPollingService _pollingService;
        private readonly DashboardRepository _dashboardRepository;
        private readonly CostRepository _costRepository;
        private readonly UserRepository _userRepository;

        // SignalR Gateway için gerekli ek Repository'ler
        private readonly RecipeConfigurationRepository _recipeConfigRepository;
        private readonly PlcOperatorRepository _plcOperatorRepository;

        // --- GATEWAY SERVÝSÝ (YENÝ) ---
        private SignalRGatewayService _gatewayService;

        // --- ARAYÜZ KONTROLLERÝ (VIEWS) ---
        private readonly ProsesÝzleme_Control _prosesIzlemeView;
        private readonly ProsesKontrol_Control _prosesKontrolView;
        private readonly Ayarlar_Control _ayarlarView;
        private readonly MakineDetay_Control _makineDetayView;
        private readonly Raporlar_Control _raporlarView;
        private readonly LiveEventPopup_Form _liveEventPopup;
        private readonly GenelBakis_Control _genelBakisView;

        private VncViewer_Form _activeVncViewerForm = null;
        private readonly UserSettings_Control _user_setting;
    
        private VncProxyServer _vncServer;
        string apiKey = LicenseManager.GenerateHardwareKey();
        public MainForm()
        {
            InitializeComponent();

            // 1. ADIM: Tüm nesneler burada oluþturulur.
            _machineRepository = new MachineRepository();
            _recipeRepository = new RecipeRepository();
            _processLogRepository = new ProcessLogRepository();
            _alarmRepository = new AlarmRepository();
            _productionRepository = new ProductionRepository();
            _costRepository = new CostRepository();
            _userRepository = new UserRepository();

            // Gateway için gerekli ek repo'larý oluþturuyoruz
            _recipeConfigRepository = new RecipeConfigurationRepository();
            _plcOperatorRepository = new PlcOperatorRepository();

            _dashboardRepository = new DashboardRepository(_recipeRepository);

            // PLC Servisi
            _pollingService = new PlcPollingService(
                _alarmRepository,
                _processLogRepository,
                _productionRepository,
                _recipeRepository,
                _machineRepository,
                new NullLogger<PlcPollingService>()
            );

            // FTP Servisi
            _ftpTransferService = new FtpTransferService(_pollingService);

            // 2. ADIM: View (Arayüz) Kontrolleri
            _prosesIzlemeView = new ProsesÝzleme_Control();
            _prosesKontrolView = new ProsesKontrol_Control(); // Gerekirse parametre ekleyin
            _ayarlarView = new Ayarlar_Control();
            _makineDetayView = new MakineDetay_Control();
            _raporlarView = new Raporlar_Control();
            _liveEventPopup = new LiveEventPopup_Form();
            _genelBakisView = new GenelBakis_Control();
            _user_setting = new UserSettings_Control();

            // Olay Abonelikleri
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;
            _ayarlarView.MachineListChanged += OnMachineListChanged;
            _pollingService.OnActiveAlarmStateChanged += OnActiveAlarmStateChanged;
            _prosesIzlemeView.MachineDetailsRequested += OnMachineDetailsRequested;
            _prosesIzlemeView.MachineVncRequested += OnMachineVncRequested;
            _makineDetayView.BackRequested += OnBackRequested;

            
        }

        // 'async' keyword'ü eklendi çünkü Gateway'i await ile baþlatacaðýz
        private async void MainForm_Load(object sender, EventArgs e)
        {
            // === LÝSANS DOÐRULAMA KODU ===
            var (isValid, message, licenseData) = LicenseManager.ValidateLicense();

            if (!isValid)
            {
                MessageBox.Show($"Lisans Hatasý: {message}", "Uygulama Lisansý", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            // Makine Sayýsý Kontrolü
            var machines = _machineRepository.GetAllMachines();
            if (machines.Count > licenseData.MachineLimit)
            {
                var dialogResult = MessageBox.Show(
                    $"Lisansýnýz {licenseData.MachineLimit} makine ile sýnýrlýdýr. Veritabanýnýzda {machines.Count} makine bulunmaktadýr.\nFazla makineler otomatik olarak silinecektir. Devam etmek istiyor musunuz?",
                    "Makine Sayýsý Limiti Aþýldý",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.Yes)
                {
                    for (int i = machines.Count - 1; i >= licenseData.MachineLimit; i--)
                    {
                        _machineRepository.DeleteMachine(machines[i].Id);
                    }
                    // Listeyi yenile
                    machines = _machineRepository.GetAllMachines();
                    MessageBox.Show("Fazla makineler silindi.", "Ýþlem Tamamlandý");
                }
                else
                {
                    this.Close();
                    return;
                }
            }

            // === SÝSTEM BAÞLATMA ===
            ApplyLocalization();
            UpdateUserInfoAndPermissions();
            ReloadSystem(_genelBakisView);
            LanguageManager.SetLanguage("en-US");

            // VNC Sunucusunu Baþlat
            try
            {
                _vncServer = new VncProxyServer(5901);
                _vncServer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VNC Server Hatasý: {ex.Message}");
            }
          

            string hardwareKey = LicenseManager.GenerateHardwareKey();

            if (string.IsNullOrEmpty(hardwareKey))
            {
                MessageBox.Show("Donaným kimliði alýnamadý!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Fabrika API Key: {hardwareKey}");

            // 2. Gateway Servisini Baþlat
            try
            {
                string hubUrl = "http://localhost:7039/scadaHub"; // API Adresiniz
                string jwtToken = null; // Gateway için token þu an null kalabilir

                _gatewayService = new SignalRGatewayService(
                    hubUrl,
                    jwtToken,
                    // hardwareKey, <--- BURADAKÝ FAZLALIK PARAMETREYÝ SÝLÝN (Service yapýnýza göre 3. sýrada yok)
                    _machineRepository,
                    _recipeRepository,
                    _userRepository,
                    _costRepository,
                    _alarmRepository,
                    _dashboardRepository,
                    _productionRepository,
                    _processLogRepository,
                    _recipeConfigRepository,
                    _plcOperatorRepository,
                    _pollingService,
                    _ftpTransferService,
                    hardwareKey // <--- DOÐRU YER: En sonda (Service yapýnýzla uyumlu)
                );

                _gatewayService.OnRemoteCommandReceived += CloudSyncService_OnRemoteCommandReceived;

                await _gatewayService.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gateway Hatasý: {ex.Message}", "Baðlantý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            // Cloud Sync Baþlatma (Eski sistem, eðer kullanýyorsanýz kalsýn)

        }

        private void ApplyPermissions()
        {
            // === ANA MENÜ YETKÝLENDÝRME ===
            // Not: List<int> { 1 } gibi roller veritabanýnýzdaki Role ID'lerine karþýlýk gelmeli.

            bool isMaster = PermissionService.HasAnyPermission(new List<int> { 1000 }); // Master Admin

            if (isMaster)
            {
                btnProsesKontrol.Visible = true;
                btnProsesKontrol.Enabled = true;
                btnRaporlar.Visible = true;
                btnRaporlar.Enabled = true;
                btnAyarlar.Visible = true;
                btnAyarlar.Enabled = true;
                btnProsesIzleme.Visible = true;
                btnProsesIzleme.Enabled = true;
            }
            else
            {
                // Normal Yetkilendirme
                btnProsesKontrol.Visible = PermissionService.HasAnyPermission(new List<int> { 1 }); // Operatör?
                btnProsesKontrol.Enabled = btnProsesKontrol.Visible;

                btnRaporlar.Visible = PermissionService.HasAnyPermission(new List<int> { 2 }); // Raporcu?
                btnRaporlar.Enabled = btnRaporlar.Visible;

                btnAyarlar.Visible = PermissionService.HasAnyPermission(new List<int> { 3 }); // Yönetici?
                btnAyarlar.Enabled = btnAyarlar.Visible;

                // Proses izleme genellikle herkese açýktýr veya en düþük yetki ister
                btnProsesIzleme.Visible = true;
                btnProsesIzleme.Enabled = true;
            }

            _ayarlarView.ApplyPermissions1();
            _user_setting.LoadAllRoles();
        }

        private void ReloadSystem(Control viewToShow)
        {
            _pollingService.Stop();

            // Makine Listesini Yenile
            List<Machine> machines = _machineRepository.GetAllEnabledMachines();
            if (machines == null)
            {
                MessageBox.Show(Resources.DatabaseConnectionFailed, Resources.CriticalError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Polling Servisini Baþlat
            _pollingService.Start(machines);
            var plcManagers = _pollingService.GetPlcManagers();

            // Kontrolleri Initialize Et
            _prosesIzlemeView.InitializeView(machines, _pollingService);
            _prosesKontrolView.InitializeControl(_recipeRepository, _machineRepository, plcManagers, _pollingService, _ftpTransferService);
            _ayarlarView.InitializeControl(_machineRepository, plcManagers);

            // CostRepository eklendi
            _raporlarView.InitializeControl(_machineRepository, _alarmRepository, _productionRepository, _dashboardRepository, _processLogRepository, _recipeRepository, _costRepository);

            _genelBakisView.InitializeControl(_pollingService, _machineRepository, _dashboardRepository, _alarmRepository, _processLogRepository, _productionRepository);

            _ayarlarView.RefreshMachineSettingsView();

            if (viewToShow != null && viewToShow != _genelBakisView)
            {
                ShowView(_ayarlarView);
            }
            else
            {
                ShowView(_genelBakisView);
            }
        }

        #region Arayüz ve Dil Yönetimi

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
            UpdateUserInfoAndPermissions();
        }

        private void ApplyLocalization()
        {
            this.Text = TekstilScada.Localization.Strings.ApplicationTitle;
            btnGenelBakis.Text = TekstilScada.Localization.Strings.MainMenu_GeneralOverview;
            btnProsesIzleme.Text = TekstilScada.Localization.Strings.MainMenu_ProcessMonitoring;
            btnProsesKontrol.Text = TekstilScada.Localization.Strings.MainMenu_ProcessControl;
            btnRaporlar.Text = TekstilScada.Localization.Strings.MainMenu_Reports;
            btnAyarlar.Text = TekstilScada.Localization.Strings.MainMenu_Settings;

            dilToolStripMenuItem.Text = Resources.Language;
            oturumToolStripMenuItem.Text = Resources.Session;
            çýkýþYapToolStripMenuItem.Text = Resources.Logout;
            lblStatusLiveEvents.Text = Resources.Livelogsee;
        }

        private void UpdateUserInfoAndPermissions()
        {
            if (CurrentUser.IsLoggedIn && CurrentUser.User != null)
            {
                lblStatusCurrentUser.Text = $"{Resources.Loggedin}: {CurrentUser.User.FullName}";
                try
                {
                    _userRepository.LogAction(CurrentUser.User.Id, "Log", "Session Login");
                }
                catch (Exception logEx)
                {
                    // Sessiz kalabilir veya debug yazabiliriz
                    System.Diagnostics.Debug.WriteLine($"Log Error: {logEx.Message}");
                }
            }
            else
            {
                lblStatusCurrentUser.Text = $"{Resources.Loggedin}: -";
            }

            _user_setting.LoadAllRoles();
            _ayarlarView.RefreshUserRoles();
            ApplyPermissions();
        }

        private void ShowView(UserControl view)
        {
            pnlContent.Controls.Clear();
            view.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(view);
        }

        #endregion

        #region Olay Yöneticileri (Event Handlers)

        private void OnMachineListChanged(object sender, EventArgs e) => ReloadSystem(_ayarlarView);
        private void OnBackRequested(object sender, EventArgs e) => ShowView(_prosesIzlemeView);
        private void btnGenelBakis_Click(object sender, EventArgs e) => ShowView(_genelBakisView);
        private void btnProsesIzleme_Click(object sender, EventArgs e) => ShowView(_prosesIzlemeView);
        private void btnProsesKontrol_Click(object sender, EventArgs e) => ShowView(_prosesKontrolView);
        private void btnRaporlar_Click(object sender, EventArgs e) => ShowView(_raporlarView);
        private void btnAyarlar_Click(object sender, EventArgs e) => ShowView(_ayarlarView);
        private void türkçeToolStripMenuItem_Click(object sender, EventArgs e) => LanguageManager.SetLanguage("tr-TR");
        private void englishToolStripMenuItem_Click(object sender, EventArgs e) => LanguageManager.SetLanguage("en-US");

        private void çýkýþYapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentUser.IsLoggedIn && CurrentUser.User != null)
            {
                try { _userRepository.LogAction(CurrentUser.User.Id, "Log", "Session Logout"); } catch { }
            }

            CurrentUser.User = null;
            UpdateUserInfoAndPermissions();
            ShowView(_genelBakisView);

            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    UpdateUserInfoAndPermissions();
                    ReloadSystem(_genelBakisView);
                }
            }
        }

        private void OnMachineDetailsRequested(object sender, int machineId)
        {
            var machine = _machineRepository.GetAllMachines().FirstOrDefault(m => m.Id == machineId);
            if (machine != null)
            {
                _makineDetayView.InitializeControl(machine, _pollingService, _processLogRepository, _alarmRepository, _recipeRepository, _productionRepository);
                ShowView(_makineDetayView);
            }
        }

        private void OnMachineVncRequested(object sender, int machineId)
        {
            if (_activeVncViewerForm != null && !_activeVncViewerForm.IsDisposed)
            {
                _activeVncViewerForm.Activate();
                MessageBox.Show(Resources.Vnccurrentclose, Resources.Warning, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var machine = _machineRepository.GetAllMachines().FirstOrDefault(m => m.Id == machineId);
            if (machine != null && !string.IsNullOrEmpty(machine.VncAddress))
            {
                try
                {
                    if (CurrentUser.IsLoggedIn && CurrentUser.User != null)
                    {
                        _userRepository.LogAction(CurrentUser.User.Id, "VNC Connection", $"{machine.MachineName} connected via VNC.");
                    }

                    var vncForm = new VncViewer_Form(machine.VncAddress, machine.VncPassword);
                    vncForm.Text = $"{machine.MachineName} - {Resources.VncConnectionTo}";
                    vncForm.FormClosed += (s, args) => { _activeVncViewerForm = null; };
                    _activeVncViewerForm = vncForm;
                    vncForm.Show();
                }
                catch (Exception ex)
                {
                    _activeVncViewerForm = null;
                    MessageBox.Show($"{Resources.Vncconnecterror} {ex.Message}", Resources.Error);
                }
            }
            else
            {
                MessageBox.Show(Resources.Vncnomachine, Resources.Information);
            }
        }

        private void OnActiveAlarmStateChanged(int machineId, FullMachineStatus status)
        {
            if (!this.IsHandleCreated || this.IsDisposed) return;
            this.Invoke(new Action(() =>
            {
                if (this.IsDisposed) return;

                var activeAlarms = _pollingService.MachineDataCache.Values.Where(s => s.HasActiveAlarm).ToList();

                if (activeAlarms.Any())
                {
                    var alarmToShow = activeAlarms
                        .Select(s => new { Status = s, Definition = _alarmRepository.GetAlarmDefinitionByNumber(s.ActiveAlarmNumber) })
                        .Where(ad => ad.Definition != null)
                        .OrderByDescending(ad => ad.Definition.Severity)
                        .FirstOrDefault();

                    if (alarmToShow != null)
                    {
                        lblStatusLiveEvents.Text = $"[{alarmToShow.Status.MachineName}] - ALARM: {alarmToShow.Definition.AlarmText}";
                        lblStatusLiveEvents.BackColor = Color.FromArgb(231, 76, 60);
                        lblStatusLiveEvents.ForeColor = Color.White;
                    }
                }
                else
                {
                    lblStatusLiveEvents.Text = Resources.Livelogsee;
                    lblStatusLiveEvents.BackColor = SystemColors.Control;
                    lblStatusLiveEvents.ForeColor = SystemColors.ControlText;
                }
            }));
        }

        private void lblStatusLiveEvents_Click(object sender, EventArgs e)
        {
            if (_liveEventPopup.Visible) _liveEventPopup.Hide();
            else _liveEventPopup.Show(this);
        }

        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            LanguageManager.LanguageChanged -= LanguageManager_LanguageChanged;
            _pollingService.Stop();

            if (_activeVncViewerForm != null && !_activeVncViewerForm.IsDisposed)
            {
                try { _activeVncViewerForm.Close(); } catch { }
            }

            if (_gatewayService != null)
            {
                // Gateway servisine StopAsync metodu eklediðinizi varsayýyoruz
                // await _gatewayService.StopAsync(); 

                // Veya en azýndan event aboneliðini kaldýrýn
                _gatewayService.OnRemoteCommandReceived -= CloudSyncService_OnRemoteCommandReceived;
            }
            // ------------------------------------------


            _vncServer?.Stop();
        }

        private void CloudSyncService_OnRemoteCommandReceived(int machineId, string command, string parameters)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => CloudSyncService_OnRemoteCommandReceived(machineId, command, parameters)));
                return;
            }

            AppendLog($"UZAK KOMUT ALINDI: Makine {machineId} -> {command}");

            var managers = _pollingService.GetPlcManagers();
            if (managers.TryGetValue(machineId, out var plcManager))
            {
                try
                {
                    switch (command.ToUpper())
                    {
                        case "STOP":
                            // await plcManager.StopMachineAsync();
                            MessageBox.Show($"Makine {machineId} için DURDURMA emri!", "Uzak Kontrol", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        case "START":
                            // await plcManager.StartMachineAsync();
                            MessageBox.Show($"Makine {machineId} için BAÞLATMA emri!", "Uzak Kontrol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        default:
                            AppendLog($"Bilinmeyen komut: {command}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    AppendLog($"Komut hatasý: {ex.Message}");
                }
            }
        }

        private void AppendLog(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        #endregion
    }
}