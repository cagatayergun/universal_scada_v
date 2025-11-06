// UI/Views/MakineDetay_Control.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Windows.Forms;
using Universalscada.core;
using Universalscada.Models;
using Universalscada.Properties;
using Universalscada.Repositories;
// using Universalscada.Services; // KALDIRILDI
using System.Net.Http; // YENİ
using System.Net.Http.Headers; // YENİ
using Newtonsoft.Json; // YENİ (NuGet'ten Newtonsoft.Json eklemelisiniz)
using System.Threading.Tasks; // YENİ
using Universalscada.Services;
namespace Universalscada.UI.Views
{
    public partial class MakineDetay_Control : UserControl
    {
        public event EventHandler BackRequested;

        // === KALDIRILDI ===
        // private PlcPollingService _pollingService;

        // === YENİ ===
        private static readonly HttpClient _apiClient = new HttpClient();
        // !!! KENDİ WEBAPI ADRESİNİZLE DEĞİŞTİRİN !!!
        private const string API_BASE_URL = "http://localhost:5000";
        private FullMachineStatus _currentStatus; // YENİ: Anlık durumu saklamak için

        private ProcessLogRepository _logRepository;
        private AlarmRepository _alarmRepository;
        private RecipeRepository _recipeRepository;
        private ProductionRepository _productionRepository;
        private Machine _machine;
        public int CurrentMachineId => _machine?.Id ?? 0; // YENİ: MainForm'un erişmesi için

        private ScottPlot.Plottables.Scatter _tempPlot;
        private ScottPlot.Plottables.Scatter _rpmPlot;
        private ScottPlot.Plottables.Scatter _waterLevelPlot;
        private List<string> _currentlyDisplayedAlarms = new List<string>();

        // === KALDIRILDI ===
        // private System.Windows.Forms.Timer _uiUpdateTimer;
        private string _lastLoadedBatchIdForChart = null;

        public MakineDetay_Control()
        {
            InitializeComponent();
            btnGeri.Click += (sender, args) => BackRequested?.Invoke(this, EventArgs.Empty);
            this.progressTemp.Paint += new System.Windows.Forms.PaintEventHandler(this.progressTemp_Paint);
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;

            // YENİ: API İstemcisini bir kez ayarla
            if (_apiClient.BaseAddress == null)
            {
                _apiClient.BaseAddress = new Uri(API_BASE_URL);
            }
        }

        // === DEĞİŞTİ: InitializeControl ===
        // PlcPollingService parametresi kaldırıldı
        public void InitializeControl(Machine machine, ProcessLogRepository logRepo, AlarmRepository alarmRepo, RecipeRepository recipeRepo, ProductionRepository productionRepo)
        {
            _machine = machine;
            // _pollingService = service; // KALDIRILDI
            _logRepository = logRepo;
            _alarmRepository = alarmRepo;
            _recipeRepository = recipeRepo;
            _productionRepository = productionRepo;

            // === KALDIRILDI ===
            // _uiUpdateTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            // _uiUpdateTimer.Tick += (sender, args) => UpdateLiveGauges();
            // _uiUpdateTimer.Start();
            // _pollingService.OnMachineDataRefreshed += OnDataRefreshed;
            // _pollingService.OnMachineConnectionStateChanged += OnConnectionStateChanged;
            // _pollingService.OnActiveAlarmStateChanged += OnAlarmStateChanged;

            this.VisibleChanged += MakineDetay_Control_VisibleChanged;

            // === DEĞİŞTİ ===
            // LoadInitialData(); // KALDIRILDI (Artık verinin SignalR'dan gelmesini bekleyeceğiz)
            _lastLoadedBatchIdForChart = null; // İzleyiciyi sıfırla
            ClearAllFieldsWithMessage(Resources.baglantibekleniyro); // Varsayılan mesajı göster
            ApplyLocalization(); // Lokalizasyonu uygula
        }

        // === YENİ: GetApiClient ===
        private HttpClient GetApiClient()
        {
            _apiClient.DefaultRequestHeaders.Authorization = null;
            if (CurrentUser.IsLoggedIn && !string.IsNullOrEmpty(CurrentUser.Token))
            {
                _apiClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", CurrentUser.Token);
            }
            return _apiClient;
        }

        // === YENİ: UpdateStatus (Ana Veri Giriş Noktası) ===
        // MainForm'dan gelen SignalR verilerini almak için eklendi.
        public void UpdateStatus(FullMachineStatus status)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                // UI thread'inde güvenli güncelleme yap
                this.BeginInvoke(new Action(() =>
                {
                    _currentStatus = status; // En son durumu sakla
                    UpdateUI(status); // Tüm arayüzü bu yeni durumla güncelle
                    UpdateAlarmList(status); // Alarm listesini bu yeni durumla güncelle
                }));
            }
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
        }

        public void ApplyLocalization()
        {
            // ... (Bu metotta değişiklik yok) ...
            btnGeri.Text = Resources.geri;
            label1.Text = Resources.makinebilgileri;
            label2.Text = Resources.RecipeName;
            label3.Text = Resources.Operator;
            label4.Text = Resources.CustomerNo;
            label5.Text = Resources.BatchNo;
            label6.Text = Resources.OrderNo;
            lblTempTitle.Text = Resources.Temperature;
            // lstAlarmlar.Text = Resources.baglantibekleniyro; // lstAlarmlar.Text özelliği yok
        }

        // === KALDIRILDI ===
        // private void OnConnectionStateChanged(int machineId, FullMachineStatus status)
        // { ... }
        // private void OnDataRefreshed(int machineId, FullMachineStatus status)
        // { ... }
        // private void LoadInitialData()
        // { ... }

        private void MakineDetay_Control_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible && _machine != null)
            {
                _lastLoadedBatchIdForChart = null; // Sayfa göründüğünde grafik izleyiciyi sıfırla

                // === DEĞİŞTİ ===
                // if (_pollingService.MachineDataCache.TryGetValue(_machine.Id, out var status)) // KALDIRILDI
                if (_currentStatus != null) // En son saklanan durum neyse onu yükle
                {
                    UpdateUI(_currentStatus);
                    UpdateAlarmList(_currentStatus);
                }
                else
                {
                    ClearAllFieldsWithMessage(Resources.baglantibekleniyro);
                }

                // Reçete adımlarını (API üzerinden) yeniden yükle
                LoadRecipeStepsFromPlcAsync();
            }
        }

        // === KALDIRILDI ===
        // private void UpdateLiveGauges()
        // { ... } // Bu metodun içeriği UpdateUI'a taşındı

        // === DEĞİŞTİ: UpdateUI ===
        // Artık _uiUpdateTimer tarafından değil, UpdateStatus tarafından tetikleniyor.
        private void UpdateUI(FullMachineStatus status)
        {
            // 1. Canlı Göstergeleri (Eski UpdateLiveGauges) Güncelle
            gaugeRpm.Value = status.AnlikDevirRpm;
            gaugeRpm.Text = status.AnlikDevirRpm.ToString();
            decimal anlikSicaklikDecimal = status.AnlikSicaklik / 10.0m;
            progressTemp.Tag = anlikSicaklikDecimal; // decimal olarak ata
            lblTempValue.Text = $"{anlikSicaklikDecimal:F1} °C";
            lblTempValue.ForeColor = GetTemperatureColor((int)anlikSicaklikDecimal);
            progressTemp.Invalidate();
            waterTankGauge1.Value = status.AnlikSuSeviyesi;

            // 2. Temel bilgileri güncelle
            lblMakineAdi.Text = status.MachineName;
            lblOperator.Text = string.IsNullOrEmpty(status.OperatorIsmi) ? "---" : status.OperatorIsmi;
            lblReceteAdi.Text = string.IsNullOrEmpty(status.RecipeName) ? "---" : status.RecipeName;
            lblMusteriNo.Text = string.IsNullOrEmpty(status.MusteriNumarasi) ? "---" : status.MusteriNumarasi;
            lblBatchNo.Text = string.IsNullOrEmpty(status.BatchNumarasi) ? "---" : status.BatchNumarasi;
            lblSiparisNo.Text = string.IsNullOrEmpty(status.SiparisNumarasi) ? "---" : status.SiparisNumarasi;
            lblCalisanAdim.Text = $"#{status.AktifAdimNo} - {status.AktifAdimAdi}";

            // 3. Bağlantı durumunu kontrol et
            if (status.ConnectionState != ConnectionStatus.Connected)
            {
                ClearAllFieldsWithMessage($"{Resources.baglantibekleniyro}");
                return;
            }

            // 4. Grafik ve Raporlama Mantığı (Bu kısım aynı kalabilir)
            if (!string.IsNullOrEmpty(status.BatchNumarasi))
            {
                if (status.BatchNumarasi != _lastLoadedBatchIdForChart)
                {
                    LoadDataForBatch(status);
                }
            }
            else
            {
                if (_lastLoadedBatchIdForChart != null)
                {
                    LoadDataForLive(status); // Canlı moda geçildiğinde grafiği temizle/yenile
                }
                _lastLoadedBatchIdForChart = null;
                LoadDataForLive(status);
            }

            HighlightCurrentStep(status.AktifAdimNo);
        }

        private void LoadDataForBatch(FullMachineStatus status)
        {
            _lastLoadedBatchIdForChart = status.BatchNumarasi;

            // Partiye özel geçmiş alarmları veritabanından yükle
            var alarms = _alarmRepository.GetAlarmDetailsForBatch(status.BatchNumarasi, _machine.Id);
            var alarmStrings = alarms.Any() ? alarms.Select(a => a.AlarmDescription).ToList() : new List<string> { $"{Resources.bupartiicinalarmyok}" };

            _currentlyDisplayedAlarms = alarmStrings;
            lstAlarmlar.DataSource = _currentlyDisplayedAlarms;

            LoadTimelineChartForBatch(status.BatchNumarasi);
        }

        private void LoadDataForLive(FullMachineStatus status)
        {
            // Canlı alarm listesi UpdateAlarmList(status) tarafından doldurulacak
            LoadTimelineChartForLive();
        }

        // === DEĞİŞTİ: LoadRecipeStepsFromPlcAsync ===
        // Artık _pollingService yerine WebAPI kullanıyor
        private async void LoadRecipeStepsFromPlcAsync()
        {
            dgvAdimlar.DataSource = new List<object> { new { Adım = "...", Açıklama = $"{Resources.receteplcdenokunuyor}" } };

            try
            {
                var apiClient = GetApiClient();
                // YENİ API ÇAĞRISI (ProsesKontrol'deki ile aynı)
                var response = await apiClient.GetAsync($"api/recipes/read-from-plc/{_machine.Id}");

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var recipeFromPlc = JsonConvert.DeserializeObject<ScadaRecipe>(jsonResponse);

                    if (recipeFromPlc == null || !recipeFromPlc.Steps.Any())
                    {
                        dgvAdimlar.DataSource = new List<object> { new { Adım = "!", Açıklama = $"{Resources.plcdenreceteokunmadı} (Boş döndü)" } };
                        return;
                    }

                    // (Aşağıdaki kod, orijinal kodunuzdaki ile aynı, sadece 'result.Content' yerine 'recipeFromPlc' kullanır)
                    var steps = new List<ScadaRecipeStep>();
                    if (_machine.MachineType == $"{Resources.kurutmamakinesi}") // Bu "Kaynaklar" kullanımı tehlikeli, "Kurutma Makinesi" string'i ile karşılaştırmak daha iyi
                    {
                        steps.Add(recipeFromPlc.Steps.First());
                    }
                    else // BYMakinesi
                    {
                        steps = recipeFromPlc.Steps;
                    }
                    dgvAdimlar.DataSource = steps.Select(s => new { Adım = s.StepNumber, Açıklama = GetStepTypeName(s) }).ToList();
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    dgvAdimlar.DataSource = new List<object> { new { Adım = "!", Açıklama = $"{Resources.plcdenreceteokunmadı} {errorMsg}" } };
                }
            }
            catch (Exception ex)
            {
                dgvAdimlar.DataSource = new List<object> { new { Adım = "!", Açıklama = $"API Hatası: {ex.Message}" } };
            }
        }

        // ... (LoadTimelineChartForBatch metodu aynı kalır) ...
        private void LoadTimelineChartForBatch(string batchId)
        {
            SafeInvoke(() =>
            {
                formsPlot1.Reset();
                var (startTime, endTime) = _productionRepository.GetBatchTimestamps(batchId, _machine.Id);

                if (!startTime.HasValue)
                {
                    formsPlot1.Plot.Title($"{Resources.partibaslangıczamanıkayip}");
                    formsPlot1.Refresh();
                    return;
                }

                DateTime effectiveEndTime = endTime ?? DateTime.Now;
                var dataPoints = _logRepository.GetLogsForDateRange(_machine.Id, startTime.Value, effectiveEndTime);

                if (!dataPoints.Any())
                {
                    formsPlot1.Plot.Title($"{Resources.bupartihenüzkaydedilmemis}");
                    formsPlot1.Refresh();
                    return;
                }

                formsPlot1.Plot.Title($"{_machine.MachineName} - ${Resources.proseszamancizgisi} ({batchId})");
                var tempPlot = formsPlot1.Plot.Add.Scatter(
                    dataPoints.Select(p => p.Timestamp.ToOADate()).ToArray(),
                   dataPoints.Select(p => (double)p.Temperature / 10.0).ToArray());

                tempPlot.Color = ScottPlot.Colors.Red;
                tempPlot.LegendText = $"{Resources.Temperature}";
                tempPlot.LineWidth = 2;

                var rpmAxis = formsPlot1.Plot.Axes.AddLeftAxis();
                rpmAxis.Label.Text = $"{Resources.devir}";
                var rpmPlot = formsPlot1.Plot.Add.Scatter(
                    dataPoints.Select(p => p.Timestamp.ToOADate()).ToArray(),
                    dataPoints.Select(p => (double)p.Rpm).ToArray());
                rpmPlot.Color = ScottPlot.Colors.Blue;
                rpmPlot.LegendText = $"{Resources.devir}";
                rpmPlot.Axes.YAxis = rpmAxis;

                var waterLevelAxis = formsPlot1.Plot.Axes.AddRightAxis();
                waterLevelAxis.Label.Text = $"{Resources.suseviyesi}";
                var waterLevelPlot = formsPlot1.Plot.Add.Scatter(
                    dataPoints.Select(p => p.Timestamp.ToOADate()).ToArray(),
                    dataPoints.Select(p => (double)p.WaterLevel).ToArray());
                waterLevelPlot.Color = ScottPlot.Colors.Green;
                waterLevelPlot.LegendText = $"{Resources.suseviyesi}";
                waterLevelPlot.Axes.YAxis = waterLevelAxis;


                formsPlot1.Plot.Axes.DateTimeTicksBottom();
                formsPlot1.Plot.ShowLegend(ScottPlot.Alignment.UpperLeft);
                formsPlot1.Plot.Axes.AutoScale();
                formsPlot1.Refresh();
            });
        }

        // ... (LoadTimelineChartForLive metodu aynı kalır) ...
        private void LoadTimelineChartForLive()
        {
            SafeInvoke(() =>
            {
                formsPlot1.Plot.Clear();
                DateTime endTime = DateTime.Now;
                DateTime startTime = endTime.AddMinutes(-100); // Son 100 dakika

                var dataPoints = _logRepository.GetManualLogs(_machine.Id, startTime, endTime);

                if (!dataPoints.Any())
                {
                    formsPlot1.Plot.Clear();
                    formsPlot1.Plot.Title($"{Resources.canlidata}");
                    formsPlot1.Refresh();
                    return;
                }

                double[] timeData = dataPoints.Select(p => p.Timestamp.ToOADate()).ToArray();
                double[] tempData = dataPoints.Select(p => (double)p.Temperature / 10.0).ToArray();
                double[] rpmData = dataPoints.Select(p => (double)p.Rpm).ToArray();
                double[] waterLevelData = dataPoints.Select(p => (double)p.WaterLevel).ToArray();

                if (_tempPlot == null)
                {
                    formsPlot1.Plot.Clear();
                    formsPlot1.Plot.Title($"{_machine.MachineName} - ${Resources.canliprosesdata}");
                    formsPlot1.Plot.Axes.DateTimeTicksBottom();
                    formsPlot1.Plot.ShowLegend(ScottPlot.Alignment.UpperLeft);

                    _tempPlot = formsPlot1.Plot.Add.Scatter(timeData, tempData);
                    _tempPlot.Color = ScottPlot.Colors.Red;
                    _tempPlot.LegendText = $"{Resources.Temperature}";
                    _tempPlot.LineWidth = 2;

                    _rpmPlot = formsPlot1.Plot.Add.Scatter(timeData, rpmData);
                    _rpmPlot.Color = ScottPlot.Colors.Blue;
                    _rpmPlot.LegendText = $"{Resources.devir}";

                    _waterLevelPlot = formsPlot1.Plot.Add.Scatter(timeData, waterLevelData);
                    _waterLevelPlot.Color = ScottPlot.Colors.Green;
                    _waterLevelPlot.LegendText = $"{Resources.suseviyesi}";

                    formsPlot1.Plot.Axes.SetLimitsX(startTime.ToOADate(), endTime.ToOADate());
                    formsPlot1.Plot.Axes.AutoScaleY();
                }
                else
                {
                    formsPlot1.Plot.Remove(_tempPlot);
                    formsPlot1.Plot.Remove(_rpmPlot);
                    formsPlot1.Plot.Remove(_waterLevelPlot);

                    _tempPlot = formsPlot1.Plot.Add.Scatter(timeData, tempData);
                    _tempPlot.Color = ScottPlot.Colors.Red;
                    _tempPlot.LegendText = $"{Resources.Temperature}";
                    _tempPlot.LineWidth = 2;

                    _rpmPlot = formsPlot1.Plot.Add.Scatter(timeData, rpmData);
                    _rpmPlot.Color = ScottPlot.Colors.Blue;
                    _rpmPlot.LegendText = $"{Resources.devir}";

                    _waterLevelPlot = formsPlot1.Plot.Add.Scatter(timeData, waterLevelData);
                    _waterLevelPlot.Color = ScottPlot.Colors.Green;
                    _waterLevelPlot.LegendText = $"{Resources.suseviyesi}";

                    var xRange = formsPlot1.Plot.Axes.Bottom.Range;
                    if (timeData.Any() && timeData.Last() > xRange.Max)
                    {
                        formsPlot1.Plot.Axes.SetLimitsX(startTime.ToOADate(), endTime.ToOADate());
                    }
                }

                formsPlot1.Refresh();
            });
        }

        // ... (ClearAllFieldsWithMessage, ClearBatchSpecificFieldsWithMessage, HighlightCurrentStep, GetStepTypeName, GetTemperatureColor, progressTemp_Paint, lblMakineAdi_Click, SafeInvoke metotları aynı kalır) ...
        #region Değişiklik Olmayan Metotlar
        private void ClearAllFieldsWithMessage(string message)
        {
            ClearBatchSpecificFieldsWithMessage(message);
            lblReceteAdi.Text = "---";
            lblOperator.Text = "---";
            lblMusteriNo.Text = "---";
            lblBatchNo.Text = "---";
            lblSiparisNo.Text = "---";
            lblCalisanAdim.Text = "---";
        }

        private void ClearBatchSpecificFieldsWithMessage(string message)
        {
            lstAlarmlar.DataSource = new List<string> { message };
            dgvAdimlar.DataSource = null;
            formsPlot1.Plot.Clear();
            formsPlot1.Plot.Title(message);
            formsPlot1.Refresh();
        }

        private void HighlightCurrentStep(int currentStepNumber)
        {
            foreach (DataGridViewRow row in dgvAdimlar.Rows)
            {
                if (row.Cells["Adım"] != null && row.Cells["Adım"].Value != null)
                {
                    if (int.TryParse(row.Cells["Adım"].Value.ToString(), out int stepValue))
                    {
                        if (stepValue == currentStepNumber)
                        {
                            row.DefaultCellStyle.BackColor = Color.LightGreen;
                            row.DefaultCellStyle.Font = new Font(dgvAdimlar.Font, FontStyle.Bold);
                        }
                        else
                        {
                            row.DefaultCellStyle.BackColor = Color.White;
                            row.DefaultCellStyle.Font = new Font(dgvAdimlar.Font, FontStyle.Regular);
                        }
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                        row.DefaultCellStyle.Font = new Font(dgvAdimlar.Font, FontStyle.Regular);
                    }
                }
            }
        }

        private string GetStepTypeName(ScadaRecipeStep step)
        {
            var stepTypes = new List<string>();
            short controlWord = step.StepDataWords[24];
            if ((controlWord & 1) != 0) stepTypes.Add($"{Resources.sualma}");
            if ((controlWord & 2) != 0) stepTypes.Add($"{Resources.isitma}");
            if ((controlWord & 4) != 0) stepTypes.Add($"{Resources.calisma}");
            if ((controlWord & 8) != 0) stepTypes.Add($"{Resources.dozaj}");
            if ((controlWord & 16) != 0) stepTypes.Add($"{Resources.bosaltma}");
            if ((controlWord & 32) != 0) stepTypes.Add($"{Resources.sikma}");
            return string.Join(" + ", stepTypes);
        }

        private Color GetTemperatureColor(int temp)
        {
            if (temp < 40) return Color.DodgerBlue;
            if (temp < 60) return Color.SeaGreen;
            if (temp < 90) return Color.Orange;
            return Color.Crimson;
        }

        private void progressTemp_Paint(object sender, PaintEventArgs e)
        {
            Panel barPanel = sender as Panel;
            if (barPanel == null || barPanel.Tag == null) return;

            // === DEĞİŞTİ: Tag'den decimal al ===
            decimal currentValueDecimal;
            try
            {
                currentValueDecimal = Convert.ToDecimal(barPanel.Tag);
            }
            catch { currentValueDecimal = 0; }

            decimal maximumValue = 150m; // Max değeri decimal yap (150.0)
            int controlWidth = barPanel.Width;
            int controlHeight = barPanel.Height;

            e.Graphics.FillRectangle(new SolidBrush(Color.WhiteSmoke), 0, 0, controlWidth, controlHeight);

            // Değeri 0'dan küçükse 0 yap
            if (currentValueDecimal < 0) currentValueDecimal = 0;

            int filledHeight = (int)(controlHeight * (currentValueDecimal / maximumValue));
            // Yüksekliğin kontrolden taşmamasını sağla
            if (filledHeight > controlHeight) filledHeight = controlHeight;
            if (filledHeight < 0) filledHeight = 0;


            Rectangle filledRect = new Rectangle(
                0,
                controlHeight - filledHeight,
                controlWidth,
                filledHeight
            );

            e.Graphics.FillRectangle(new SolidBrush(GetTemperatureColor((int)currentValueDecimal)), filledRect);

            using (Pen borderPen = new Pen(Color.LightGray, 1))
            {
                e.Graphics.DrawRectangle(borderPen, 0, 0, controlWidth - 1, controlHeight - 1);
            }
        }

        private void lblMakineAdi_Click(object sender, EventArgs e) { }

        private void SafeInvoke(Action action)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                try { this.BeginInvoke(action); }
                catch (Exception) { }
            }
        }
        #endregion

        // === KALDIRILDI ===
        // private void OnAlarmStateChanged(int machineId, FullMachineStatus status)
        // { ... }

        // === DEĞİŞTİ: UpdateAlarmList ===
        // Artık _pollingService'e değil, parametre olarak gelen 'status'e bakar.
        private void UpdateAlarmList(FullMachineStatus status)
        {
            if (status == null) return;

            // Sadece canlı izleme modundaysak (geçmiş bir rapora bakmıyorsak) çalış
            if (string.IsNullOrEmpty(_lastLoadedBatchIdForChart))
            {
                List<string> newAlarmList;

                if (status.HasActiveAlarm)
                {
                    // Alarm tanımını veritabanından çek
                    var alarmDef = _alarmRepository.GetAlarmDefinitionByNumber(status.ActiveAlarmNumber);
                    newAlarmList = new List<string> { $"#{status.ActiveAlarmNumber}: {alarmDef?.AlarmText ?? "Bilinmeyen Alarm"}" };
                }
                else
                {
                    newAlarmList = new List<string> { $"{Resources.aktifalarmyok}" };
                }

                // Sadece yeni alarm listesi eskisinden farklıysa arayüzü güncelle
                if (!_currentlyDisplayedAlarms.SequenceEqual(newAlarmList))
                {
                    _currentlyDisplayedAlarms = newAlarmList;
                    lstAlarmlar.DataSource = _currentlyDisplayedAlarms;
                }
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            // === KALDIRILDI ===
            // if (_pollingService != null)
            // {
            //     _pollingService.OnMachineDataRefreshed -= OnDataRefreshed;
            //     _pollingService.OnMachineConnectionStateChanged -= OnConnectionStateChanged;
            //     _pollingService.OnActiveAlarmStateChanged -= OnAlarmStateChanged;
            // }
            // _uiUpdateTimer?.Stop(); // KALDIRILDI
            // _uiUpdateTimer?.Dispose(); // KALDIRILDI

            base.OnHandleDestroyed(e);
        }

        private void pnlAlarmsAndSteps_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}