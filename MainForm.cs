// MainForm.cs
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Universalscada.core;
using Universalscada.core.Services;
using Universalscada.Core;
using Universalscada.Core.Core;
using Universalscada.Core.Repositories;
using Universalscada.Localization;
using Universalscada.Models;
using Universalscada.Properties;
using Universalscada.Repositories;
using Universalscada.Services;
using Universalscada.UI;
using Universalscada.UI.Controls;
using Universalscada.UI.Views;
// EK BÝLGÝ: Artýk bu dosyalara ihtiyacýnýz yok, çünkü DI bunlarý otomatik çözer
// using Universalscada.Core;              
// using Universalscada.Core.Core;         
// using Universalscada.Core.Repositories; 

namespace Universalscada
{
    public partial class MainForm : Form
    {
        // DI ile alýnan baðýmlýlýklar artýk constructor'dan atanacak.
        // Manuel oluþturma ortadan kalktýðý için DI nesneleri artýk burada tanýmlý.
        private readonly FtpTransferService _ftpTransferService;
        private readonly MachineRepository _machineRepository;
        private readonly RecipeRepository _recipeRepository;
        private readonly ProcessLogRepository _processLogRepository;
        private readonly AlarmRepository _alarmRepository;
        private readonly ProductionRepository _productionRepository;
        private readonly DashboardRepository _dashboardRepository;
        private readonly CostRepository _costRepository;
        private readonly UserRepository _userRepository;

        // === KALDIRILDI ===
        // private readonly ScadaDbContext _dbContext; 
        // private readonly IMetaDataRepository _metaDataRepository;
        // private readonly IRecipeTimeCalculator _timeCalculator;
        // private readonly PlcPollingService _pollingService; 
        // private readonly IPlcManagerFactory _plcManagerFactory; 

        // === YENÝ ===
        private HubConnection _hubConnection; // SignalR baðýmlýlýðý DI ile alýnamaz/almýyoruz, bu yüzden burada kalýyor.
        private readonly ConcurrentDictionary<int, FullMachineStatus> _machineDataCache; // Canlý veri önbelleði (Constructor'da new'lenmeli)

        // Arayüz Kontrolleri (Views)
        private readonly ProsesÝzleme_Control _prosesIzlemeView;
        private readonly ProsesKontrol_Control _prosesKontrolView;
        private readonly Ayarlar_Control _ayarlarView;
        private readonly MakineDetay_Control _makineDetayView;
        private readonly Raporlar_Control _raporlarView;
        private readonly LiveEventPopup_Form _liveEventPopup;
        private readonly DashboardControl _genelBakisView;
        private VncViewer_Form _activeVncViewerForm = null;
        private readonly UserSettings_Control _user_setting;


        // CONSTRUCTOR ENJEKSÝYONU: Tüm baðýmlýlýklar DI konteynerinden alýnýr
        public MainForm(
            FtpTransferService ftpTransferService,
            MachineRepository machineRepository,
            RecipeRepository recipeRepository,
            ProcessLogRepository processLogRepository,
            AlarmRepository alarmRepository,
            ProductionRepository productionRepository,
            DashboardRepository dashboardRepository,
            CostRepository costRepository,
            UserRepository userRepository)
        {
            InitializeComponent();

            // 1. ADIM: Baðýmlýlýklarý alanlara atama
            _ftpTransferService = ftpTransferService;
            _machineRepository = machineRepository;
            _recipeRepository = recipeRepository;
            _processLogRepository = processLogRepository;
            _alarmRepository = alarmRepository;
            _productionRepository = productionRepository;
            _dashboardRepository = dashboardRepository;
            _costRepository = costRepository;
            _userRepository = userRepository;

            // Manuel new'leme artýk sadece UI nesneleri ve SignalR için kullanýlýr
            _machineDataCache = new ConcurrentDictionary<int, FullMachineStatus>();

            // Arayüz Kontrolleri (Views) oluþturulur
            _prosesIzlemeView = new ProsesÝzleme_Control();
            _prosesKontrolView = new ProsesKontrol_Control();
            _ayarlarView = new Ayarlar_Control();
            _makineDetayView = new MakineDetay_Control();
            _raporlarView = new Raporlar_Control();
            _liveEventPopup = new LiveEventPopup_Form();
            _genelBakisView = new DashboardControl();
            _user_setting = new UserSettings_Control();

            // Olay abonelikleri (Events)
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;
            _ayarlarView.MachineListChanged += OnMachineListChanged;
            // _pollingService.OnActiveAlarmStateChanged += OnActiveAlarmStateChanged; // KALDIRILDI
            _prosesIzlemeView.MachineDetailsRequested += OnMachineDetailsRequested;
            _prosesIzlemeView.MachineVncRequested += OnMachineVncRequested;
            _makineDetayView.BackRequested += OnBackRequested;

            // YENÝ: SignalR Hub baðlantýsýný baþlat
            InitializeSignalR();
        }

        // YENÝ: WebAPI'ye baðlanmak için eklendi
        private async void InitializeSignalR()
        {
            // WebAPI'nizin adresini (ve /scadaHub yolunu) buraya yazýn
            // ÖNEMLÝ: WebAPI (Program.cs) dosyanýzdaki 'app.MapHub<ScadaHub>("/scadaHub")' ile eþleþmeli
            string hubUrl = "http://localhost:7039/scadaHub"; // !!! KENDÝ WEBAPI ADRESÝNÝZLE DEÐÝÞTÝRÝN !!!

            _hubConnection = new HubConnectionBuilder()
       .WithUrl(hubUrl, options =>
       {
           options.AccessTokenProvider = () => Task.FromResult(CurrentUser.Token);
       })
        .WithAutomaticReconnect() // Baðlantý koparsa otomatik yeniden baðlan
                .Build();

            // "ReceiveMachineStatus", WebAPI'deki PlcPollingService'in çaðýrdýðý metodun adýdýr.
            _hubConnection.On<int, FullMachineStatus>("OnMachineDataRefreshed", (machineId, status) =>
            {
                if (status == null) return;

                // Veritabaný yerine direkt canlý veriyi önbelleðe al
                _machineDataCache[status.MachineId] = status;

                // Bu metod, veriyi aldýktan sonra arayüzü günceller
                // UI thread'inde çalýþmasý için Invoke kullanýlýr
                this.Invoke(new Action(() =>
                {
                    UpdateUIWithNewStatus(status);
                }));
            });

            // "OnActiveAlarmStateChanged", WebAPI'deki SignalRBridgeService'in tetiklediði olaydýr
            _hubConnection.On<int, FullMachineStatus>("OnActiveAlarmStateChanged", (machineId, status) =>
            {
                this.Invoke(new Action(() =>
                {
                    // Alarm durum çubuðunu güncellemek için
                    // Bu metod artýk _pollingService.MachineDataCache yerine _machineDataCache kullanacak (aþaðýda düzenlendi)
                    OnActiveAlarmStateChanged(machineId, status);
                }));
            });


            try
            {
                await _hubConnection.StartAsync();
                // Baðlantý baþarýlý olduktan sonra veritabanýndan ilk verileri yükle
                ReloadSystem(_genelBakisView);
                foreach (var status in _machineDataCache.Values)
                {
                    UpdateUIWithNewStatus(status);
                }
            }
            catch (Exception ex)
            {
                // Baðlantý hatasý (WebAPI çalýþmýyor olabilir)
                MessageBox.Show($"WebAPI sunucusuna ({hubUrl}) baðlanýlamadý. Sunucunun çalýþtýðýndan emin olun.\n\nHata: {ex.Message}", "Baðlantý Hatasý", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // WebAPI'ye baðlanamazsa uygulama açýlamaz.
                this.Close();
            }
        }

        // YENÝ: SignalR'dan gelen veriyi arayüze daðýtan metod
        private void UpdateUIWithNewStatus(FullMachineStatus status)
        {
            if (this.IsDisposed) return;

            // TODO: Bu UserControl'lerin ÝÇÝNE 'UpdateMachineStatus(FullMachineStatus status)'
            //       adlý public metotlar eklemeniz ve arayüzü güncellemelerini saðlamanýz gerekir.

            // 1. Proses Ýzleme ekranýndaki ilgili kartý güncelle
            _prosesIzlemeView.UpdateMachineStatus(status);

            // 2. Genel Bakýþ ekranýný güncelle
            _genelBakisView.UpdateMachineStatus(status);

            // 3. Makine Detay ekraný açýksa onu güncelle
            if (_makineDetayView.Visible && _makineDetayView.CurrentMachineId == status.MachineId)
            {
                _makineDetayView.UpdateStatus(status);
            }
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            // === YENÝ LÝSANS DOÐRULAMA KODU BAÞLANGICI ===
            var (isValid, message, licenseData) = LicenseManager.ValidateLicense();

            if (!isValid)
            {
                MessageBox.Show($"Lisans Hatasý: {message}", "Uygulama Lisansý", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
            // Lisans baþarýlý, makine sayýsýný kontrol et
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
                    // Fazla makineleri silme mantýðý
                    for (int i = licenseData.MachineLimit; i < machines.Count; i++)
                    {
                        _machineRepository.DeleteMachine(machines[i].Id);
                    }
                    MessageBox.Show($"{machines.Count - licenseData.MachineLimit} adet makine baþarýyla silindi.", "Ýþlem Tamamlandý");
                }
                else
                {
                    this.Close();
                    return;
                }
            }
            // 2. ADIM: 
            ApplyLocalization();
            UpdateUserInfoAndPermissions();

            // ReloadSystem artýk SignalR baðlandýktan sonra (InitializeSignalR içinde) çaðrýlýyor.
            // ReloadSystem(_genelBakisView); // BURADAN TAÞINDI

            LanguageManager.SetLanguage("en-US");
        }

        private void ApplyPermissions()
        {
            // ... (Bu metotta deðiþiklik yok) ...
            // === ANA MENÜ BUTONLARI ÝÇÝN YETKÝLENDÝRME ===
            // 5 numaralý role sahip kullanýcýlar rapor alabilir
            btnProsesKontrol.Visible = PermissionService.HasAnyPermission(new List<int> { 1 });
            btnProsesKontrol.Enabled = btnProsesKontrol.Visible; // Yetkisi yoksa butonun týklanmasýný engelle

            btnRaporlar.Visible = PermissionService.HasAnyPermission(new List<int> { 2 });
            btnRaporlar.Enabled = btnRaporlar.Visible; // Yetkisi yoksa butonun týklanmasýný engelle

            // 8 numaralý role sahip kullanýcýlar ayarlar ekranýna eriþebilir
            btnAyarlar.Visible = PermissionService.HasAnyPermission(new List<int> { 3 });
            btnAyarlar.Enabled = btnAyarlar.Visible; // Yetkisi yoksa butonun týklanmasýný engelle

            // === ANA MENÜ BUTONLARI ÝÇÝN YETKÝLENDÝRME ===
            var master = PermissionService.HasAnyPermission(new List<int> { 1000 });
            if (master == true)
            {

                // 5 numaralý role sahip kullanýcýlar rapor alabilir
                btnProsesKontrol.Visible = PermissionService.HasAnyPermission(new List<int> { 1000 });
                btnProsesKontrol.Enabled = btnProsesKontrol.Visible; // Yetkisi yoksa butonun týklanmasýný engelle

                btnRaporlar.Visible = PermissionService.HasAnyPermission(new List<int> { 1000 });
                btnRaporlar.Enabled = btnRaporlar.Visible; // Yetkisi yoksa butonun týklanmasýný engelle

                // 8 numaralý role sahip kullanýcýlar ayarlar ekranýna eriþebilir
                btnAyarlar.Visible = PermissionService.HasAnyPermission(new List<int> { 1000 });
                btnAyarlar.Enabled = btnAyarlar.Visible; // Yetkisi yoksa butonun týklanmasýný engelle
            }
            _ayarlarView.ApplyPermissions1();
            _user_setting.LoadAllRoles();
        }

        private void ReloadSystem(Control viewToShow)
        {


            // === YENÝ LÝSANS KONTROLÜ BAÞLANGICI ===
            // ... (Lisans kontrol mantýðý ayný kalýr) ...
            var (isValid, message, licenseData) = LicenseManager.ValidateLicense();
            if (!isValid)
            {
                MessageBox.Show($"Lisans Hatasý: {message}", "Uygulama Lisansý", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            // Makine sayýsýný kontrol et ve lisans limitini aþanlarý sil
            var machines1 = _machineRepository.GetAllMachines();
            if (machines1.Count > licenseData.MachineLimit)
            {
                MessageBox.Show(
                  $"Lisansýnýz {licenseData.MachineLimit} makine ile sýnýrlýdýr. Veritabanýnýzda {machines1.Count} makine bulunmaktadýr.\nFazla makineler otomatik olarak silinecektir.",
                  "Makine Sayýsý Limiti Aþýldý",
                  MessageBoxButtons.OK,
                  MessageBoxIcon.Warning);

                // Sondan baþlayarak fazla makineleri sil
                for (int i = machines1.Count - 1; i >= licenseData.MachineLimit; i--)
                {
                    _machineRepository.DeleteMachine(machines1[i].Id);
                }

                // Makine listesini yeniden oku
                machines1 = _machineRepository.GetAllMachines();
            }
            // === YENÝ LÝSANS KONTROLÜ BÝTÝÞÝ ===

            // Sunucuya (WebAPI) baðlanmaya gerek yok, sadece veritabanýndan makineleri oku
            List<Machine> machines = _machineRepository.GetAllEnabledMachines();
            if (machines == null)
            {
                MessageBox.Show(Resources.DatabaseConnectionFailed, Resources.CriticalError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // _pollingService.Start(machines); // KALDIRILDI
            // var plcManagers = _pollingService.GetPlcManagers(); // KALDIRILDI

            // Kontrolleri en güncel verilerle baþlat
            // ÖNEMLÝ: Bu UserControl'lerin (*_Control.cs) Initialize... metotlarýndan
            // _pollingService ve plcManagers parametrelerini kaldýrmanýz GEREKÝR.

            _prosesIzlemeView.InitializeView(machines); // Parametreler kaldýrýldý
            _prosesKontrolView.InitializeControl(_recipeRepository, _machineRepository, _ftpTransferService); // Parametreler kaldýrýldý
            _ayarlarView.InitializeControl(_machineRepository); // Parametreler kaldýrýldý
            _raporlarView.InitializeControl(_machineRepository, _alarmRepository, _productionRepository, _dashboardRepository, _processLogRepository, _recipeRepository, _costRepository);
            _genelBakisView.InitializeControl(_machineRepository, _dashboardRepository, _alarmRepository, _processLogRepository, _productionRepository); // Parametre kaldýrýldý

            _ayarlarView.RefreshMachineSettingsView();

            // ... (ShowView mantýðý ayný kalýr) ...
            if (viewToShow != _genelBakisView)
            {
                ShowView(_ayarlarView);
            }
            else
            {
                ShowView(_genelBakisView);
            }
        }

        #region Arayüz ve Dil Yönetimi

        // ... (ApplyLocalization ve UpdateUserInfoAndPermissions metotlarý ayný kalýr) ...

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
            UpdateUserInfoAndPermissions();
        }

        private void ApplyLocalization()
        {
            this.Text = Universalscada.Localization.Strings.ApplicationTitle;
            btnGenelBakis.Text = Universalscada.Localization.Strings.MainMenu_GeneralOverview;
            btnProsesIzleme.Text = Universalscada.Localization.Strings.MainMenu_ProcessMonitoring;
            btnProsesKontrol.Text = Universalscada.Localization.Strings.MainMenu_ProcessControl;
            btnRaporlar.Text = Universalscada.Localization.Strings.MainMenu_Reports;
            btnAyarlar.Text = Universalscada.Localization.Strings.MainMenu_Settings;
            dilToolStripMenuItem.Text = Resources.Language;
            oturumToolStripMenuItem.Text = Resources.Session;
            çýkýþYapToolStripMenuItem.Text = Resources.Logout;
            lblStatusLiveEvents.Text = Resources.Livelogsee;
        }

        private void UpdateUserInfoAndPermissions()
        {
            if (CurrentUser.IsLoggedIn)
            {
                lblStatusCurrentUser.Text = $"{Resources.Loggedin}: {CurrentUser.User.FullName}";
                try
                {
                    if (CurrentUser.IsLoggedIn && CurrentUser.User != null)
                    {
                        _userRepository.LogAction(CurrentUser.User.Id, "Log", $"Session Login");
                    }
                }
                catch (Exception logEx)
                {
                    MessageBox.Show($" 'Session Login' is Log error : {logEx.Message}", Resources.Error);
                }
            }
            else
            {
                lblStatusCurrentUser.Text = $"{Resources.Loggedin}: -";
                try
                {
                    if (CurrentUser.IsLoggedIn && CurrentUser.User != null)
                    {
                        _userRepository.LogAction(CurrentUser.User.Id, "Log", $"Session Logout");
                    }
                }
                catch (Exception logEx)
                {
                    MessageBox.Show($" 'Session Logout' is Log error : {logEx.Message}", Resources.Error);
                }
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
                else
                {
                    // Application.Exit();
                }
            }
        }

        private void OnMachineDetailsRequested(object sender, int machineId)
        {
            var machine = _machineRepository.GetAllMachines().FirstOrDefault(m => m.Id == machineId);
            if (machine != null)
            {
                // _pollingService parametresi kaldýrýldý
                _makineDetayView.InitializeControl(machine, _processLogRepository, _alarmRepository, _recipeRepository, _productionRepository);
                ShowView(_makineDetayView);
            }
        }

        private void OnMachineVncRequested(object sender, int machineId)
        {
            // ... (Bu metodun içinde deðiþiklik yok, _pollingService kullanýlmýyor) ...
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
                        _userRepository.LogAction(CurrentUser.User.Id, "VNC Baðlantýsý", $"{machine.MachineName} makinesine VNC ile baðlandý.");
                    }
                }
                catch (Exception logEx)
                {
                    MessageBox.Show($"VNC baðlantýsý loglanýrken bir hata oluþtu: {logEx.Message}", Resources.Error);
                }

                try
                {
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

        // DEÐÝÞTÝ: Bu metod artýk SignalR tarafýndan tetikleniyor
        // ve _pollingService.MachineDataCache yerine _machineDataCache kullanýyor.
        private void OnActiveAlarmStateChanged(int machineId, FullMachineStatus status)
        {
            if (!this.IsHandleCreated || this.IsDisposed) return;

            // Invoke zaten çaðýran yerde (InitializeSignalR) yapýldý.
            // this.Invoke(new Action(() =>
            // {
            if (this.IsDisposed) return;

            // _pollingService.MachineDataCache yerine _machineDataCache kullan
            var activeAlarms = _machineDataCache.Values.Where(s => s.HasActiveAlarm).ToList();

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
                    lblStatusLiveEvents.BackColor = Color.FromArgb(231, 76, 60); // Kýrmýzý
                    lblStatusLiveEvents.ForeColor = Color.White;
                }
            }
            else
            {
                lblStatusLiveEvents.Text = Resources.Livelogsee; // YENÝ: Alarmlar temizlendiðinde varsayýlan metne dön
                lblStatusLiveEvents.BackColor = System.Drawing.SystemColors.Control;
                lblStatusLiveEvents.ForeColor = System.Drawing.SystemColors.ControlText;
            }
            // }));
        }

        private void lblStatusLiveEvents_Click(object sender, EventArgs e)
        {
            if (_liveEventPopup.Visible)
            {
                _liveEventPopup.Hide();
            }
            else
            {
                _liveEventPopup.Show(this);
            }
        }

        // DEÐÝÞTÝ: async void yapýldý ve SignalR baðlantýsý kapatýldý
        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            LanguageManager.LanguageChanged -= LanguageManager_LanguageChanged;

            // _pollingService.Stop(); // KALDIRILDI

            // YENÝ: SignalR baðlantýsýný güvenle kapat
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
            }

            if (_activeVncViewerForm != null && !_activeVncViewerForm.IsDisposed)
            {
                try
                {
                    _activeVncViewerForm.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"{Resources.Closeandvnc} {ex.Message}");
                }
            }
        }

        #endregion
    }
}
