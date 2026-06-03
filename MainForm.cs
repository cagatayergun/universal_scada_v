using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection; // Kapsam ve DI yönetimi için
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telemetry;
using Telemetry.Localization1;
using Telemetry.Properties;
using Telemetry.Core;
using Telemetry.Models;
using Telemetry.Repositories;
using Telemetry.Services;
using Telemetry.UI;
using Telemetry.UI.Controls;
using Telemetry.UI.Views;
using MaterialSkin;          // YENÝ EKLENDÝ: MaterialSkin ana kütüphanesi
using MaterialSkin.Controls; // YENÝ EKLENDÝ: MaterialForm ve bileþenler için

namespace Telemetry
{
    // Form yerine MaterialForm'dan türetiyoruz
    public partial class MainForm : MaterialForm
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
        private readonly EfficiencyRepository _efficiencyRepository;
        private AutoBackupService _backupService;

        // SignalR Gateway için gerekli ek Repository'ler
        private readonly RecipeConfigurationRepository _recipeConfigRepository;
        private readonly PlcOperatorRepository _plcOperatorRepository;

        // --- YENÝ EKLENDÝ: UTILITY (SENSÖR) REPOSITORY & SERVICE ---
        private readonly UtilityRepository _utilityRepository;
        private readonly UtilityPollingService _utilityPollingService;

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
        private System.Windows.Forms.Timer _trialUsageTimer;
        // OPTÝMÝZASYON: UI darboðazýný (Kilitlenmeyi) önleyecek Alarm Timer'ý
        private System.Windows.Forms.Timer _alarmUpdateTimer;

        // YENÝ: Makine ID'sine göre çalýþan sunucularý tutar
        private Dictionary<int, VncProxyServer> _activeVncServers = new Dictionary<int, VncProxyServer>();
        string apiKey = LicenseManager.GenerateHardwareKey();

        public MainForm()
        {
            InitializeComponent();

            // =========================================================================
            // MATERIALSKIN TEMALANDIRMA MOTORU BAÞLANGICI
            // SCADA sistemlerine uygun göz yormayan Dark (Karanlýk) tema konfigürasyonu
            // =========================================================================
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK; // Karanlýk Tema
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.BlueGrey800,   // Ana Renk (Üst Baþlýk Barý)
                Primary.BlueGrey900,   // Koyu Ana Renk
                Primary.BlueGrey500,   // Açýk Ana Renk
                Accent.LightBlue200,   // Vurgu Rengi (Switch/Checkbox öðeleri için)
                TextShade.WHITE        // Yazý Rengi
            );

            // Arayüz render hýzýný artýrmak ve kýrpýþmayý önlemek için DoubleBuffering aktif
            this.DoubleBuffered = true;

            // =========================================================================
            // 1. ADIM: WINFORMS ÝÇÝN LOKAL DEPENDENCY INJECTION (DI) KURULUMU
            // Arka planda çalýþan Thread'lerin (PlcPollingService) MySQL baðlantý havuzunu 
            // týkamasýný önlemek için Repository'leri Transient (Geçici) olarak kaydediyoruz.
            // =========================================================================
            var services = new ServiceCollection();

            services.AddTransient<MachineRepository>();
            services.AddTransient<RecipeRepository>();
            services.AddTransient<ProcessLogRepository>();
            services.AddTransient<AlarmRepository>();
            services.AddTransient<ProductionRepository>();
            services.AddTransient<CostRepository>();
            services.AddTransient<UserRepository>();
            services.AddTransient<RecipeConfigurationRepository>();
            services.AddTransient<PlcOperatorRepository>();
            services.AddTransient<EfficiencyRepository>();
            var serviceProvider = services.BuildServiceProvider();
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            // =========================================================================
            // 2. ADIM: NESNELERÝN OLUÞTURULMASI (DI Provider Üzerinden)
            // =========================================================================
            _machineRepository = serviceProvider.GetRequiredService<MachineRepository>();
            _recipeRepository = serviceProvider.GetRequiredService<RecipeRepository>();
            _processLogRepository = serviceProvider.GetRequiredService<ProcessLogRepository>();
            _alarmRepository = serviceProvider.GetRequiredService<AlarmRepository>();
            _productionRepository = serviceProvider.GetRequiredService<ProductionRepository>();
            _costRepository = serviceProvider.GetRequiredService<CostRepository>();
            _efficiencyRepository = serviceProvider.GetRequiredService<EfficiencyRepository>();
            _userRepository = serviceProvider.GetRequiredService<UserRepository>();

            _recipeConfigRepository = serviceProvider.GetRequiredService<RecipeConfigurationRepository>();
            _plcOperatorRepository = serviceProvider.GetRequiredService<PlcOperatorRepository>();

            _dashboardRepository = new DashboardRepository(_recipeRepository);
            try
            {
                _efficiencyRepository.EnsureDowntimeDefinitionsTableCreated();
            }
            catch { /* Hata yönetimi */ }
            // --- UTILITY NESNELERÝNÝ OLUÞTURMA ---
            try
            {
                // 1. Repository oluþtur
                _utilityRepository = new UtilityRepository();

                // 2. Servisi oluþtur (Logger olmadýðý için NullLogger kullanýyoruz)
                _utilityPollingService = new UtilityPollingService(
                    _utilityRepository,
                    new NullLogger<UtilityPollingService>()
                );

                // 3. Servisi Baþlat (Arka planda okumaya baþlar)
                _utilityPollingService.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sensör servisi baþlatýlamadý: {ex.Message}");
            }

            // =========================================================================
            // 3. ADIM: PLC SERVÝSÝ VE FTP SERVÝSÝ
            // PlcPollingService'e scopeFactory parametresini yolluyoruz
            // =========================================================================
            _pollingService = new PlcPollingService(
                _alarmRepository,
                _processLogRepository,
                _productionRepository,
                _recipeRepository,
                _machineRepository,
                new NullLogger<PlcPollingService>(),
                scopeFactory
            );

            // FTP Servisi
            _ftpTransferService = new FtpTransferService(_pollingService);

            // =========================================================================
            // 4. ADIM: ARAYÜZ (VIEWS) KONTROLLERÝ VE ABONELÝKLER
            // =========================================================================
            _prosesIzlemeView = new ProsesÝzleme_Control();
            _prosesKontrolView = new ProsesKontrol_Control();
            _ayarlarView = new Ayarlar_Control();
            _makineDetayView = new MakineDetay_Control();
            _raporlarView = new Raporlar_Control();
            _liveEventPopup = new LiveEventPopup_Form();
            _genelBakisView = new GenelBakis_Control();
            _user_setting = new UserSettings_Control();

            // OPTÝMÝZASYON: UI kilitlenmesini engellemek için Alarm Timer'ý ayarlanýyor
            _alarmUpdateTimer = new System.Windows.Forms.Timer();
            _alarmUpdateTimer.Interval = 1000; // Saniyede sadece 1 kere çalýþýr
            _alarmUpdateTimer.Tick += AlarmUpdateTimer_Tick;

            // Olay Abonelikleri
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;
            _ayarlarView.MachineListChanged += OnMachineListChanged;
            _prosesIzlemeView.MachineDetailsRequested += OnMachineDetailsRequested;
            _prosesIzlemeView.MachineVncRequested += OnMachineVncRequested;
            _makineDetayView.BackRequested += OnBackRequested;
        }

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
            if (licenseData.TrialMinutes.HasValue)
            {
                _trialUsageTimer = new System.Windows.Forms.Timer();
                _trialUsageTimer.Interval = 60000; // 1 Dakika
                _trialUsageTimer.Tick += TrialUsageTimer_Tick;
                _trialUsageTimer.Start();
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

            _alarmUpdateTimer.Start();

            string hardwareKey = LicenseManager.GenerateHardwareKey();

            if (string.IsNullOrEmpty(hardwareKey))
            {
                MessageBox.Show("Donaným kimliði alýnamadý!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            // Gateway Servisini Baþlat
            try
            {
                string hubUrl = "https://api.yilmaktelemetry.com/scadaHub";
                string jwtToken = null;

                _gatewayService = new SignalRGatewayService(
                    hubUrl,
                    jwtToken,
                    _machineRepository,
                    _efficiencyRepository,
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
                    hardwareKey
                );

                _gatewayService.OnRemoteCommandReceived += CloudSyncService_OnRemoteCommandReceived;
                await _gatewayService.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gateway Hatasý: {ex.Message}", "Baðlantý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            _backupService = new AutoBackupService();
            _backupService.Start();
        }

        private void TrialUsageTimer_Tick(object sender, EventArgs e)
        {
            LicenseManager.AddUsedMinute();
            var check = LicenseManager.ValidateLicense();
            if (!check.IsValid)
            {
                _trialUsageTimer.Stop();
                MessageBox.Show(check.Message, "Deneme Süresi Doldu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0);
            }
        }

        private void ApplyPermissions()
        {
            bool isMaster = PermissionService.HasAnyPermission(new List<int> { 1000 });

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
                btnProsesKontrol.Visible = PermissionService.HasAnyPermission(new List<int> { 1 });
                btnProsesKontrol.Enabled = btnProsesKontrol.Visible;

                btnRaporlar.Visible = PermissionService.HasAnyPermission(new List<int> { 2 });
                btnRaporlar.Enabled = btnRaporlar.Visible;

                btnAyarlar.Visible = PermissionService.HasAnyPermission(new List<int> { 3 });
                btnAyarlar.Enabled = btnAyarlar.Visible;

                btnProsesIzleme.Visible = true;
                btnProsesIzleme.Enabled = true;
            }

            _ayarlarView.ApplyPermissions1();
            _user_setting.LoadAllRoles();
        }

        private void ReloadSystem(Control viewToShow)
        {
            _pollingService.Stop();

            List<Machine> machines = _machineRepository.GetAllEnabledMachines();
            if (machines == null)
            {
                MessageBox.Show(Resources.DatabaseConnectionFailed, Resources.CriticalError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _pollingService.Start(machines);
            var plcManagers = _pollingService.GetPlcManagers();

            _prosesIzlemeView.InitializeView(machines, _pollingService);
            _prosesKontrolView.InitializeControl(_recipeRepository, _machineRepository, plcManagers, _pollingService, _ftpTransferService, _userRepository);
            _ayarlarView.InitializeControl(_machineRepository, _efficiencyRepository, plcManagers, _pollingService);

            _raporlarView.InitializeControl(
                _machineRepository,
                _alarmRepository,
                _productionRepository,
                _dashboardRepository,
                _processLogRepository,
                _recipeRepository,
                _costRepository,
                _efficiencyRepository
            );
            _genelBakisView.InitializeControl(_pollingService, _machineRepository, _dashboardRepository, _alarmRepository, _processLogRepository, _productionRepository, _utilityRepository, _utilityPollingService);

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
            this.Text = Telemetry.Localization1.Strings.ApplicationTitle;
            btnGenelBakis.Text = Telemetry.Localization1.Strings.MainMenu_GeneralOverview;
            btnProsesIzleme.Text = Telemetry.Localization1.Strings.MainMenu_ProcessMonitoring;
            btnProsesKontrol.Text = Telemetry.Localization1.Strings.MainMenu_ProcessControl;
            btnRaporlar.Text = Telemetry.Localization1.Strings.MainMenu_Reports;
            btnAyarlar.Text = Telemetry.Localization1.Strings.MainMenu_Settings;

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
                try { _userRepository.LogAction(CurrentUser.User.Id, "Log", "Session Login"); } catch { }
            }
            else
            {
                lblStatusCurrentUser.Text = $"{Resources.Loggedin}: -";
            }

            _user_setting.LoadAllRoles();
            _ayarlarView.RefreshUserRoles();
            ApplyPermissions();
        }

        // SPEED OPTÝMÝZASYON: Görünüm geçiþleri esnasýnda anlýk donmayý engeller
        private void ShowView(UserControl view)
        {
            pnlContent.SuspendLayout(); // Form yerleþim hesaplamalarýný durdur
            pnlContent.Controls.Clear();
            view.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(view);
            pnlContent.ResumeLayout(true); // Deðiþiklikleri tek bir frame'de ekrana çiz
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

        private void AlarmUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (!this.IsHandleCreated || this.IsDisposed) return;

            try
            {
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
            }
            catch (Exception) { }
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

            if (_alarmUpdateTimer != null)
            {
                _alarmUpdateTimer.Stop();
                _alarmUpdateTimer.Dispose();
            }

            if (_utilityPollingService != null)
            {
                _utilityPollingService.Stop();
            }

            if (_activeVncViewerForm != null && !_activeVncViewerForm.IsDisposed)
            {
                try { _activeVncViewerForm.Close(); } catch { }
            }

            if (_gatewayService != null)
            {
                _gatewayService.OnRemoteCommandReceived -= CloudSyncService_OnRemoteCommandReceived;
            }

            foreach (var server in _activeVncServers.Values)
            {
                server.StopStream();
            }
            _backupService?.Stop();
        }

        private void CloudSyncService_OnRemoteCommandReceived(int machineId, string command, string parameters)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => CloudSyncService_OnRemoteCommandReceived(machineId, command, parameters)));
                return;
            }

            try
            {
                if (command == "START_VNC")
                {
                    var parts = parameters.Split(';');
                    string ip = parts[0];
                    string pass = parts.Length > 1 ? parts[1] : "";

                    if (_activeVncServers.ContainsKey(machineId))
                    {
                        _activeVncServers[machineId].StopStream();
                        _activeVncServers.Remove(machineId);
                    }

                    var newVnc = new VncProxyServer(_gatewayService);
                    newVnc.StartStream(machineId, ip, pass);

                    _activeVncServers.Add(machineId, newVnc);
                }
                else if (command == "STOP_VNC")
                {
                    if (_activeVncServers.ContainsKey(machineId))
                    {
                        _activeVncServers[machineId].StopStream();
                        _activeVncServers.Remove(machineId);
                    }
                }
                else if (command == "CLICK")
                {
                    if (_activeVncServers.TryGetValue(machineId, out var server))
                    {
                        var parts = parameters.Split(';');
                        if (parts.Length == 2)
                            server.SendClick(int.Parse(parts[0]), int.Parse(parts[1]));
                    }
                }
            }
            catch (Exception ex) { }
        }

        private void AppendLog(string message) { }

        #endregion
    }
}