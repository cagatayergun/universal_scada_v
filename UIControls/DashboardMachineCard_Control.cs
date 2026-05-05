using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using TekstilScada.Core;
using TekstilScada.Models;
using TekstilScada.Repositories;
using TekstilScada.Services;
using static TekstilScada.Repositories.ProcessLogRepository;

namespace TekstilScada.UI.Controls
{
    public partial class DashboardMachineCard_Control : UserControl
    {
        private List<string> _mevcutAlarmlar = new List<string>();
        // Veritabanından gelen alarmları tutacak sözlük (Key: Alarm Numarası, Value: Alarm Metni)
        public Dictionary<int, string> AlarmDefinitions { get; set; } = new Dictionary<int, string>();
        private RecipeRepository _recipeRepository;
        private MachineRepository _machineRepository;
        private Dictionary<int, IPlcManager> _plcManagers;
        private UserRepository _userRepository; // YENİ: Loglama için eklendi
        private List<ScadaRecipe> _recipeList;
        private ScadaRecipe _currentRecipe;
        private ListBox _lstAlarms;
        // BYMakinesi editörünü ve bileşenlerini tutmak için değişkenler
        private SplitContainer _byMakinesiEditor;
        private DataGridView dgvRecipeSteps;
        private Panel pnlStepDetails;
        private Label lblStepDetailsTitle;
        private CostRepository _costRepository;
        private FtpSync_Form _ftpFormInstance; // YENİ EKLENEN SATIR
        private PlcPollingService _plcPollingService;
        private FtpTransferService _ftpTransferService;
        private short[] _copiedStepData = null;
        private List<ScadaRecipe> _copiedRecipes = new List<ScadaRecipe>();
        private readonly Machine _machine;
        private readonly RecipeConfigurationRepository _configRepo = new RecipeConfigurationRepository();

        // Durum Renkleri
        private readonly Color _colorAlarm = Color.FromArgb(231, 76, 60);    // Kırmızı
        private readonly Color _colorRunning = Color.FromArgb(46, 204, 113);  // Yeşil
        private readonly Color _colorIdle = Color.FromArgb(243, 156, 18);     // Turuncu
        private readonly Color _colorStopped = Color.SlateGray;               // Gri
                                                                              // Sınıfın üst kısmındaki değişkenlere ekleyin
        private List<Panel> _bitLamps = new List<Panel>();
        private List<Label> _lampLabels = new List<Label>(); // Eğer TextBox yerine Label kullandıysanız
        private int _lastValidProgress = 0;

        public DashboardMachineCard_Control(Machine machine)
        {
            InitializeComponent();
            _machine = machine;

            // --- ALARM LİSTESİ OLUŞTURMA BAŞLANGICI ---
            _lstAlarms = new ListBox
            {
                Location = new Point(28, 260), // lblStatus'un sağına hizaladık
                Size = new Size(350, 90),       // Küçük bir liste boyutu
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                ForeColor = _colorAlarm,
                Visible = false                 // Sadece alarm varken görünecek
            };
            this.Controls.Add(_lstAlarms);
            // --- ALARM LİSTESİ OLUŞTURMA BİTİŞİ ---

            lblMachineName.Text = _machine.MachineName;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            lblMachineName.ForeColor = Color.FromArgb(44, 62, 80);

            SetLoadGaugeLimitAsync();
            // Constructor (DashboardMachineCard_Control) içine ekleyin
            for (int i = 1; i <= 21; i++)
            {
                var lamp = this.Controls.Find($"pnlBitLampStatus_{i}", true).FirstOrDefault() as Panel;
                var label = this.Controls.Find($"status_{i}", true).FirstOrDefault() as Label; // Tasarımda TextBox ise 'as TextBox' yapın

                if (lamp != null) _bitLamps.Add(lamp);
                if (label != null) _lampLabels.Add(label);
            }
            status_1.Text="KÖPRÜ-1";
            status_2.Text = "KÖPRÜ-2";
            status_3.Text = "KÖPRÜ-3";
            status_4.Text = "KÖPRÜ-4";
            status_5.Text = "KEDİ-1";
            status_6.Text = "KEDİ-2";
            status_7.Text = "KEDİ-3";
            status_8.Text = "KEDİ-4";
            status_9.Text = "MAYNA-1";
            status_10.Text = "MAYNA-2";
            status_11.Text = "MAYNA-3";
            status_12.Text = "MAYNA-4";
            status_13.Text = "KÖPRÜ-1";
            status_14.Text = "KÖPRÜ-2";
            status_15.Text = "KÖPRÜ-3";
            status_16.Text = "KEDİ-1";
            status_17.Text = "KEDİ-2";
            status_18.Text = "KEDİ-3";
            status_19.Text = "MAYNA-1";
            status_20.Text = "MAYNA-2";
            status_21.Text = "MAYNA-3";

        }
        public void InitializeControl(RecipeRepository recipeRepo, MachineRepository machineRepo, Dictionary<int, IPlcManager> plcManagers, PlcPollingService plcPollingService, FtpTransferService ftpTransferService, UserRepository userRepo)
        {
            _recipeRepository = recipeRepo;
            _machineRepository = machineRepo;
            _plcManagers = plcManagers;
            _plcPollingService = plcPollingService;
            _ftpTransferService = ftpTransferService; // YENİ: Alanı atayın
            _userRepository = userRepo;
        }
        /// <summary>
        /// Vincin güvenli çalışma yükü (SWL) limitini ayarlar.
        /// </summary>
        private async void SetLoadGaugeLimitAsync()
        {
            try
            {
                // Vinç konfigürasyonundan maksimum kapasite parametresini çekiyoruz
                // 'rpmStepTypeId' yerine 'loadLimitId' mantığına geçildi
                var configTable = await Task.Run(() => _configRepo.GetStepTypes());
                int loadLimitId = -1;

                foreach (System.Data.DataRow row in configTable.Rows)
                {
                    string name = row["StepName"].ToString();
                    // "Kapasite" veya "Load Limit" kelimelerini arıyoruz
                    if (name.Contains("Kapasite") || name.Contains("Load") || name.Contains("SWL"))
                    {
                        loadLimitId = Convert.ToInt32(row["Id"]);
                        break;
                    }
                }

                if (loadLimitId != -1)
                {
                    string layoutJson = await Task.Run(() =>
                        _configRepo.GetLayoutJson(_machine.MachineSubType, loadLimitId));

                    if (!string.IsNullOrEmpty(layoutJson))
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var controls = JsonSerializer.Deserialize<List<ControlMetadata>>(layoutJson, options);

                        // Maksimum yük değerini içeren kontrolü bul
                        var loadControl = controls.FirstOrDefault(c => c.Maximum > 0);


                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Yük limiti ayarlanamadı: {ex.Message}");
            }
        }

        public void UpdateData(FullMachineStatus status, List<ProcessDataPoint> trendData)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateData(status, trendData)));
                return;
            }

            try
            {
                UpdateStatusWord(status.status_word);
                UpdateStatusWord2(status.status_word_2);
                // 1. BAĞLANTI KONTROLÜ (PLC Koptuysa arayüzü grileştir ve uyar)
                if (status.ConnectionState != ConnectionStatus.Connected)
                {
                    pnlStatusIndicator.BackColor = Color.Gray;
                    lblStatus.Text = "BAĞLANTI YOK";
                    lblStatus.ForeColor = Color.Gray;
                    _lstAlarms.Visible = false;
                    btnSendToPlc.Enabled = false; // Bağlantı yoksa butonu kapat
                    return; // Çıkış yap, çünkü kalan veriler eski/geçersiz olabilir.
                }

                btnSendToPlc.Enabled = true;
                _lastValidProgress = Math.Max(0, Math.Min(100, (int)status.ProsesYuzdesi));

                // 2. DURUM VE ALARM YÖNETİMİ
                // 2. DURUM VE ALARM YÖNETİMİ
                if (status.HasActiveAlarm)
                {
                    pnlStatusIndicator.BackColor = _colorAlarm;
                    lblStatus.Text = "ALARMLAR:";
                    lblStatus.ForeColor = _colorAlarm;
                    btnSendToPlc.ForeColor = Color.Black;
                    _lstAlarms.Visible = true;

                    List<string> gelenAlarmlar = new List<string>();

                    // SENARYO 1: ActiveAlarmWords (Bit bazlı dizi) PLC'den dolu geliyorsa
                    if (status.ActiveAlarmWords != null && status.ActiveAlarmWords.Length > 0)
                    {
                        for (int wordIndex = 0; wordIndex < status.ActiveAlarmWords.Length; wordIndex++)
                        {
                            short currentWord = status.ActiveAlarmWords[wordIndex];
                            for (int bitIndex = 0; bitIndex < 16; bitIndex++)
                            {
                                if ((currentWord & (1 << bitIndex)) != 0) // Eğer bit 1 ise
                                {
                                    int alarmNumarasi = (wordIndex * 16) + bitIndex+1;

                                    // DİKKAT: Veritabanındaki numaralar 1'den başlıyorsa burayı (alarmNumarasi + 1) olarak değiştirin.
                                    if (AlarmDefinitions != null && AlarmDefinitions.TryGetValue(alarmNumarasi, out string alarmMetni))
                                    {
                                        gelenAlarmlar.Add(alarmMetni);
                                    }
                                    else
                                    {
                                        // DB'de karşılığı yoksa ekranda numarasını görelim ki DB'ye ekleyebilelim.
                                        gelenAlarmlar.Add($"[BİT] Tanımsız (No: {alarmNumarasi})");
                                    }
                                }
                            }
                        }
                    }

                    // SENARYO 2: Bit dizisi boş ama PLC'den sadece "ActiveAlarmNumber" dolu geliyorsa
                    if (gelenAlarmlar.Count == 0 && status.ActiveAlarmNumber > 0)
                    {
                        if (AlarmDefinitions != null && AlarmDefinitions.TryGetValue(status.ActiveAlarmNumber, out string alarmMetni))
                        {
                            gelenAlarmlar.Add(alarmMetni);
                        }
                        else
                        {
                            gelenAlarmlar.Add($"[SAYI] Tanımsız (No: {status.ActiveAlarmNumber})");
                        }
                    }

                    // SENARYO 3: Sayı da yok ama PLC'den metin (ActiveAlarmText) geliyorsa
                    if (gelenAlarmlar.Count == 0 && !string.IsNullOrEmpty(status.ActiveAlarmText))
                    {
                        gelenAlarmlar.Add($"[METİN] {status.ActiveAlarmText}");
                    }

                    // SENARYO 4: Alarm bayrağı (HasActiveAlarm) TRUE gelmiş ama PLC hiçbir detay göndermemiş
                    if (gelenAlarmlar.Count == 0)
                    {
                        gelenAlarmlar.Add("Bilinmeyen Alarm (Detay verisi yok)");
                    }

                    // --- AKILLI LİSTE GÜNCELLEMESİ (Titreşimi ve Başa Dönmeyi Engeller) ---
                    bool listeDegisti = false;

                    if (gelenAlarmlar.Count != _mevcutAlarmlar.Count)
                    {
                        listeDegisti = true;
                    }
                    else
                    {
                        for (int i = 0; i < gelenAlarmlar.Count; i++)
                        {
                            if (gelenAlarmlar[i] != _mevcutAlarmlar[i])
                            {
                                listeDegisti = true;
                                break;
                            }
                        }
                    }

                    if (listeDegisti)
                    {
                        _lstAlarms.Items.Clear();
                        foreach (var alarm in gelenAlarmlar)
                        {
                            _lstAlarms.Items.Add(alarm);
                        }
                        _mevcutAlarmlar = new List<string>(gelenAlarmlar);
                    }
                }
                else
                {
                    // Alarm yoksa listeyi gizle
                    _lstAlarms.Visible = false;

                    if (status.manuel_status)
                    {
                        pnlStatusIndicator.BackColor = _colorRunning;
                        lblStatus.Text = "MANUEL SÜRÜŞ";
                        lblStatus.ForeColor = _colorRunning;
                        btnSendToPlc.ForeColor = Color.Black;
                    }
                    else if (status.IsInRecipeMode)
                    {
                        pnlStatusIndicator.BackColor = _colorRunning;
                        lblStatus.Text = $"OTOMATİK - MOD {status.AktifAdimNo}";
                        lblStatus.ForeColor = _colorRunning;
                        btnSendToPlc.ForeColor = Color.Black;
                    }
                    else
                    {
                        pnlStatusIndicator.BackColor = _colorStopped;
                        lblStatus.Text = "BEKLEMEDE";
                        lblStatus.ForeColor = _colorStopped;
                        btnSendToPlc.ForeColor = Color.Black;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DashboardMachineCard_Control UpdateData Hatası: {ex.Message}");
            }
        }
        // Yardımcı Metot:
        private void UpdateStatusWord(int statusWord)
        {
            for (int i = 0; i < 16; i++)
            {
                // i. bitin 1 olup olmadığını kontrol et (1, 2, 4, 8... şeklinde gider)
                bool isBitSet = (statusWord & (1 << i)) != 0;

                if (i < _bitLamps.Count)
                {
                    // Bit 1 ise Yeşil, 0 ise Gri (Pasif)
                    _bitLamps[i].BackColor = isBitSet ? Color.LimeGreen : Color.LightGray;
                }

                if (i < _lampLabels.Count)
                {
                    // İsteğe bağlı: Sadece aktif olanların ismini koyulaştır veya rengini değiştir
                    _lampLabels[i].ForeColor = isBitSet ? Color.Black : Color.Gray;

                    // Eğer TextBox kullandıysanız ve sadece aktifken görünmesini istiyorsanız:
                    // _lampLabels[i].Visible = isBitSet; 
                }
            }
            
        }
        private void UpdateStatusWord2(int statusWord)
        {
            for (int i = 0; i < 5; i++)
            {
                // i. bitin 1 olup olmadığını kontrol et (1, 2, 4, 8... şeklinde gider)
                bool isBitSet = (statusWord & (1 << i)) != 0;

                if (i < _bitLamps.Count)
                {
                    // Bit 1 ise Yeşil, 0 ise Gri (Pasif)
                    _bitLamps[i+16].BackColor = isBitSet ? Color.LimeGreen : Color.LightGray;
                }

                if (i < _lampLabels.Count)
                {
                    // İsteğe bağlı: Sadece aktif olanların ismini koyulaştır veya rengini değiştir
                    _lampLabels[i+16].ForeColor = isBitSet ? Color.Black : Color.Gray;

                    // Eğer TextBox kullandıysanız ve sadece aktifken görünmesini istiyorsanız:
                    // _lampLabels[i].Visible = isBitSet; 
                }
            }

        }
        private void lblStatus_Click(object sender, EventArgs e)
        {

        }

        private async void BtnSendToPlc_Click(object sender, EventArgs e)
        {
            try
            {
                // Makineye ait PLC Manager'ın sözlükte olup olmadığını kontrol ediyoruz
                if (_plcManagers != null && _plcManagers.TryGetValue(_machine.Id, out IPlcManager plcManager))
                {
                    // IPlcManager içerisindeki AcknowledgeAlarm metodunu çağırıyoruz
                    var result = await plcManager.AcknowledgeAlarm();

                    if (result.IsSuccess)
                    {
                        // --- LOGLAMA (Alarm Reset) ---
                        if (CurrentUser.User != null && _userRepository != null)
                        {
                            _userRepository.LogAction(
                                CurrentUser.User.Id,
                                "ALARM_RESET",
                                $"'{_machine.MachineName}' makinesine alarm reset komutu gönderildi."
                            );
                        }
                        // ------------------------------

                        MessageBox.Show($"'{_machine.MachineName}' makinesi için alarm reset komutu başarıyla gönderildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Alarm resetleme başarısız oldu: {result.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Bu makine için aktif bir PLC bağlantısı bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Beklenmeyen bir hata oluştu: {ex.Message}", "Sistem Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}