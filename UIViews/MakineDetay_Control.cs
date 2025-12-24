
using ScottPlot; // Grafik kütüphanesi
using System;
using System.Collections.Generic;
using System.Drawing; // Çizim kütüphanesi
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using TekstilScada.Core;
using TekstilScada.Models;
using TekstilScada.Properties;
using TekstilScada.Repositories;
using TekstilScada.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TekstilScada.UI.Views
{
    public partial class MakineDetay_Control : UserControl
    {
        public event EventHandler BackRequested;

        private PlcPollingService _pollingService;
        private ProcessLogRepository _logRepository;
        private AlarmRepository _alarmRepository;
        private RecipeRepository _recipeRepository;
        private ProductionRepository _productionRepository;
        private readonly RecipeConfigurationRepository _configRepo = new RecipeConfigurationRepository();
        private Machine _machine;

        // Plot nesneleri
        private ScottPlot.Plottables.Scatter _tempScatter;
        private ScottPlot.Plottables.Scatter _rpmScatter;
        private ScottPlot.Plottables.Scatter _waterScatter;

        private List<string> _currentlyDisplayedAlarms = new List<string>();
        private System.Windows.Forms.Timer _uiUpdateTimer;
        private string _lastLoadedBatchIdForChart = null;
        private bool _isSyncing = false;

        public MakineDetay_Control()
        {
            InitializeComponent();
            btnGeri.Click += (sender, args) => BackRequested?.Invoke(this, EventArgs.Empty);

            // --- KRİTİK AYARLAR ---
            // Panelin Paint olayını bağlıyoruz
            this.progressTemp.Paint += new System.Windows.Forms.PaintEventHandler(this.progressTemp_Paint);
            
            this.humuditybar.Paint += new System.Windows.Forms.PaintEventHandler(this.humuditybar_Paint);
            // Titremeyi önlemek için DoubleBuffering açıyoruz (Reflection ile)
            typeof(Panel).InvokeMember("DoubleBuffered",
    System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
    null, progressTemp, new object[] { true });

            typeof(Panel).InvokeMember("DoubleBuffered",
            System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
    null, humuditybar, new object[] { true });
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;

            // Grafik Eksenlerini Bağlama (Senkronizasyon)
            formsPlotTemp.Plot.RenderManager.AxisLimitsChanged += (s, e) => SyncAxes(formsPlotTemp);
            formsPlotRpm.Plot.RenderManager.AxisLimitsChanged += (s, e) => SyncAxes(formsPlotRpm);
            formsPlotWater.Plot.RenderManager.AxisLimitsChanged += (s, e) => SyncAxes(formsPlotWater);
        }

        private void SyncAxes(ScottPlot.WinForms.FormsPlot sourcePlot)
        {
            if (_isSyncing) return;
            _isSyncing = true;

            var limits = sourcePlot.Plot.Axes.GetLimits();

            if (sourcePlot != formsPlotTemp)
            {
                formsPlotTemp.Plot.Axes.SetLimitsX(limits.Left, limits.Right);
                formsPlotTemp.Refresh();
            }
            if (sourcePlot != formsPlotRpm)
            {
                formsPlotRpm.Plot.Axes.SetLimitsX(limits.Left, limits.Right);
                formsPlotRpm.Refresh();
            }
            if (sourcePlot != formsPlotWater)
            {
                formsPlotWater.Plot.Axes.SetLimitsX(limits.Left, limits.Right);
                formsPlotWater.Refresh();
            }

            _isSyncing = false;
        }

        private void CleanupPreviousSession()
        {
            // Eski Timer temizliği
            if (_uiUpdateTimer != null)
            {
                _uiUpdateTimer.Stop();
                _uiUpdateTimer.Tick -= UpdateLiveGauges_Tick;
                _uiUpdateTimer.Dispose();
                _uiUpdateTimer = null;
            }

            // Event aboneliklerini kaldır
            if (_pollingService != null)
            {
                _pollingService.OnMachineDataRefreshed -= OnDataRefreshed;
                _pollingService.OnMachineConnectionStateChanged -= OnConnectionStateChanged;
                _pollingService.OnActiveAlarmStateChanged -= OnAlarmStateChanged;
            }

            // VisibleChanged olayını temizlemeye gerek yok, InitializeControl'de tekrar eklemiyoruz,
            // Constructor'da eklemek yerine Initialize'da ekliyorsanız -= yapmalısınız. 
            // Sizin kodunuzda aşağıda += yapılıyor, o yüzden burada -= yapıyoruz:
            this.VisibleChanged -= MakineDetay_Control_VisibleChanged;

            // Grafik ve değişken sıfırlama (Her makine için temiz sayfa garantisi)
            _tempScatter = null;
            _rpmScatter = null;
            _waterScatter = null;
            _lastLoadedBatchIdForChart = null;
            _currentlyDisplayedAlarms.Clear();

            formsPlotTemp.Plot.Clear();
            formsPlotRpm.Plot.Clear();
            formsPlotWater.Plot.Clear();

            formsPlotTemp.Refresh();
            formsPlotRpm.Refresh();
            formsPlotWater.Refresh();

            lblMakineAdi.Text = "---";
            lblReceteAdi.Text = "---";
            dgvAdimlar.DataSource = null;
        }

        public void InitializeControl(Machine machine, PlcPollingService service, ProcessLogRepository logRepo, AlarmRepository alarmRepo, RecipeRepository recipeRepo, ProductionRepository productionRepo)
        {
            CleanupPreviousSession(); // Temizlik

            _machine = machine;
            _pollingService = service;
            _logRepository = logRepo;
            _alarmRepository = alarmRepo;
            _recipeRepository = recipeRepo;
            _productionRepository = productionRepo;

            // Timer başlatma
            _uiUpdateTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _uiUpdateTimer.Tick += UpdateLiveGauges_Tick;
            _uiUpdateTimer.Start();

            // Event abonelikleri
            _pollingService.OnMachineDataRefreshed += OnDataRefreshed;
            _pollingService.OnMachineConnectionStateChanged += OnConnectionStateChanged;
            _pollingService.OnActiveAlarmStateChanged += OnAlarmStateChanged;

            this.VisibleChanged += MakineDetay_Control_VisibleChanged;

            LoadInitialData();
        }

        private void UpdateLiveGauges_Tick(object sender, EventArgs e)
        {
            // Sadece görünürse güncelle (Ekstra güvenlik)
            if (this.Visible)
                UpdateLiveGauges();
        }

        private void LoadInitialData()
        {
            bool isDrying = _machine.MachineType == "Kurutma Makinesi";

            // Kurutma makinesi ise barı gizle, nemi göster
            waterTankGauge1.Visible = !isDrying;
            humuditypanel.Visible = isDrying;
            SetWaterGaugeLimitAsync();
            SetRpmGaugeLimitAsync();
            if (_pollingService.MachineDataCache.TryGetValue(_machine.Id, out var status))
            {
                UpdateUI(status);
                UpdateAlarmList();
                LoadRecipeStepsFromPlcAsync();
            }
        }

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

        // --- GAUGE LİMİT AYARLARI ---
        private async void SetRpmGaugeLimitAsync()
        {
            try
            {
                var stepTypesTable = await Task.Run(() => _configRepo.GetStepTypes());
                int rpmStepTypeId = -1;

                foreach (System.Data.DataRow row in stepTypesTable.Rows)
                {
                    string stepName = row["StepName"].ToString();
                    if (stepName.Contains("Sıkma") || stepName.Contains("Squeezing"))
                    {
                        rpmStepTypeId = Convert.ToInt32(row["Id"]);
                        break;
                    }
                }

                if (rpmStepTypeId != -1)
                {
                    string layoutJson = await Task.Run(() => _configRepo.GetLayoutJson(_machine.MachineSubType, rpmStepTypeId));

                    if (!string.IsNullOrEmpty(layoutJson))
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var controls = JsonSerializer.Deserialize<List<ControlMetadata>>(layoutJson, options);

                        var rpmControl = controls.FirstOrDefault(c =>
                         (c.Name != null && (c.Name.IndexOf("numSikmaDevri", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              c.Name.IndexOf("Rpm", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              c.Name.IndexOf("Squeezing Speed", StringComparison.OrdinalIgnoreCase) >= 0)) ||
                         (c.Text != null && c.Text.IndexOf("Devir", StringComparison.OrdinalIgnoreCase) >= 0)
                        );

                        if (rpmControl != null)
                        {
                            // --- DEĞİŞİKLİK BURADA ---
                            // Maximum değerini 1.33 ile çarpıp int'e çeviriyoruz.
                            // Örnek: 1000 * 1.33 = 1330
                            int newMax = (int)(rpmControl.Maximum * 1.33m);

                            if (gaugeRpm.InvokeRequired)
                            {
                                gaugeRpm.Invoke(new Action(() => gaugeRpm.Maximum = newMax));
                            }
                            else
                            {
                                gaugeRpm.Maximum = newMax;
                            }
                            // -------------------------
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //($"RPM limiti ayarlanamadı: {ex.Message}");
            }
        }

        private async void SetWaterGaugeLimitAsync()
        {
            try
            {
                var stepTypesTable = await Task.Run(() => _configRepo.GetStepTypes());
                int waterStepTypeId = -1;

                foreach (System.Data.DataRow row in stepTypesTable.Rows)
                {
                    string stepName = row["StepName"].ToString();
                    if (stepName.Contains("Su Alma") || stepName.Contains("Water Intake"))
                    {
                        waterStepTypeId = Convert.ToInt32(row["Id"]);
                        break;
                    }
                }

                if (waterStepTypeId != -1)
                {
                    string layoutJson = await Task.Run(() => _configRepo.GetLayoutJson(_machine.MachineSubType, waterStepTypeId));

                    if (!string.IsNullOrEmpty(layoutJson))
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var controls = JsonSerializer.Deserialize<List<ControlMetadata>>(layoutJson, options);

                        var waterControl = controls.FirstOrDefault(c =>
                         (c.Name != null && (c.Name.IndexOf("numLitre", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              c.Name.IndexOf("Su", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              c.Name.IndexOf("Water", StringComparison.OrdinalIgnoreCase) >= 0))
                        );

                        if (waterControl != null)
                        {
                            if (waterTankGauge1.InvokeRequired)
                                waterTankGauge1.Invoke(new Action(() => waterTankGauge1.Maximum = (int)waterControl.Maximum));
                            else
                                waterTankGauge1.Maximum = (int)waterControl.Maximum;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //($"Su seviyesi limiti ayarlanamadı: {ex.Message}");
            }
        }

        // --- DİL AYARLARI ---
        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
        }

        public void ApplyLocalization()
        {
            btnGeri.Text = Resources.geri;
            label1.Text = Resources.makinebilgileri;
            label2.Text = Resources.RecipeName;
            label3.Text = Resources.Operator;
            label4.Text = Resources.CustomerNo;
            label5.Text = Resources.BatchNo;
            label6.Text = Resources.OrderNo;
            lblTempTitle.Text = Resources.Temperature;
            lstAlarmlar.Text = Resources.baglantibekleniyro;
        }

        // --- EVENT HANDLERS ---
        private void OnConnectionStateChanged(int machineId, FullMachineStatus status)
        {
            if (machineId == _machine.Id && this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke(new Action(() => UpdateUI(status)));
            }
        }

        private void OnDataRefreshed(int machineId, FullMachineStatus status)
        {
            if (machineId == _machine.Id && this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke(new Action(() => UpdateUI(status)));
            }
        }

        private void MakineDetay_Control_VisibleChanged(object sender, EventArgs e)
        {
            // Sayfa görünürse Timer çalışsın, gizliyse dursun (Arka planda çalışmayı engeller)
            if (this.Visible)
            {
                if (_uiUpdateTimer != null && !_uiUpdateTimer.Enabled)
                    _uiUpdateTimer.Start();

                if (_machine != null && _pollingService != null)
                {
                    _lastLoadedBatchIdForChart = null;
                    if (_pollingService.MachineDataCache.TryGetValue(_machine.Id, out var status))
                    {
                        UpdateUI(status);
                        UpdateAlarmList();
                    }
                }
            }
            else
            {
                if (_uiUpdateTimer != null)
                    _uiUpdateTimer.Stop();
            }
        }

        private void UpdateLiveGauges()
        {
            if (_machine != null && _pollingService.MachineDataCache.TryGetValue(_machine.Id, out var status))
            {
                SafeInvoke(() =>
                {
                    gaugeRpm.Value = status.AnlikDevirRpm;
                    gaugeRpm.Text = status.AnlikDevirRpm.ToString();

                    // Sıcaklık verisi (decimal olarak Tag'e atıyoruz)
                    
                    bool isDrying = _machine.MachineType == "Kurutma Makinesi";

                   
                  if(!isDrying)
                    {
                        decimal anlikSicaklikDecimal = status.AnlikSicaklik / 10.0m;
                        progressTemp.Tag = anlikSicaklikDecimal;
                        lblTempValue.Text = $"{anlikSicaklikDecimal:F1} °C";

                    }
                    else
                    {
                        decimal anlikSicaklikDecimal = status.AnlikSicaklik / 100.0m;
                        progressTemp.Tag = anlikSicaklikDecimal;
                        lblTempValue.Text = $"{anlikSicaklikDecimal:F1} °C";
                    }

                    decimal AnlikSuSeviyesi = status.AnlikSuSeviyesi ;
                    humuditybar.Tag = AnlikSuSeviyesi;
                    humuditytxt.Text =  $"{AnlikSuSeviyesi} Rh";
                    humuditytxt.ForeColor = System.Drawing.Color.Blue;
                   
                    lblTempValue.ForeColor = System.Drawing.Color.Red;

                    // Paneli yeniden çizmeye zorla
                    progressTemp.Invalidate();
                    progressTemp.Update();
                    humuditybar.Invalidate();
                    humuditybar.Update();

                    waterTankGauge1.Value = status.AnlikSuSeviyesi;
                    // *** CANLI İLERLEME (SCROLLING) İÇİN YENİ KISIM ***
                    if (_tempScatter != null)
                    {
                        var limits = formsPlotTemp.Plot.Axes.GetLimits();
                        double span = limits.Right - limits.Left; // Mevcut zoom aralığı

                        double newMaxX;
                        double newMinX;

                        if (string.IsNullOrEmpty(_lastLoadedBatchIdForChart))
                        {
                            // 1. CANLI AKIŞ MODU: Daima anlık zamana ilerle
                            newMaxX = DateTime.Now.ToOADate();
                            newMinX = newMaxX - span;
                        }
                        else
                        {
                            // 2. BATCH MODU: Batch aktifse ilerle

                            // Batch'in hala aktif olup olmadığını kontrol ediyoruz.
                            // Bu kontrolü, status nesnesinin BatchNumarasi doluysa batch'in aktif olduğu varsayımıyla yapıyoruz.
                            if (status.BatchNumarasi == _lastLoadedBatchIdForChart)
                            {
                                // Batch hala aktif. Grafik, en son veri noktasına (veya anlık zamana) ilerlemelidir.
                                newMaxX = DateTime.Now.ToOADate(); // Maksimum zamanı anlık zamana getir.
                                newMinX = newMaxX - span;
                            }
                            else
                            {
                                // Batch ID'si değişmiş veya batch tamamlanmıştır. İlerleme (pan) durdurulur.
                                // Mevcut limitleri koru
                                return;
                            }
                        }

                        // Eksenleri yeni limitlere ayarla
                        formsPlotTemp.Plot.Axes.SetLimitsX(newMinX, newMaxX);
                        formsPlotRpm.Plot.Axes.SetLimitsX(newMinX, newMaxX);
                        formsPlotWater.Plot.Axes.SetLimitsX(newMinX, newMaxX);

                        // Grafikleri güncelle
                        formsPlotTemp.Refresh();
                        formsPlotRpm.Refresh();
                        formsPlotWater.Refresh();
                    }
                });
            }
        }

        private void UpdateUI(FullMachineStatus status)
        {
            lblMakineAdi.Text = status.MachineName;
            lblOperator.Text = string.IsNullOrEmpty(status.OperatorIsmi) ? "---" : status.OperatorIsmi;
            lblReceteAdi.Text = string.IsNullOrEmpty(status.RecipeName) ? "---" : status.RecipeName;
            lblMusteriNo.Text = string.IsNullOrEmpty(status.MusteriNumarasi) ? "---" : status.MusteriNumarasi;
            lblBatchNo.Text = string.IsNullOrEmpty(status.BatchNumarasi) ? "---" : status.BatchNumarasi;
            lblSiparisNo.Text = string.IsNullOrEmpty(status.SiparisNumarasi) ? "---" : status.SiparisNumarasi;
            lblCalisanAdim.Text = $"#{status.AktifAdimNo} - {status.AktifAdimAdi}";

            if (status.ConnectionState != ConnectionStatus.Connected)
            {
                ClearAllFieldsWithMessage($"{Resources.baglantibekleniyro}");
                return;
            }

            if (!string.IsNullOrEmpty(status.BatchNumarasi))
            {
                // --- DÜZELTME BURADA BAŞLIYOR ---

                // Batch ID değişti mi kontrolü (Sadece ID değiştiğinde yapılacak ağır işler için)
                bool isNewBatch = status.BatchNumarasi != _lastLoadedBatchIdForChart;
                _lastLoadedBatchIdForChart = status.BatchNumarasi;

                // Eğer yeni bir batch başladıysa alarmları ve plotu temizleyerek hazırla
                if (isNewBatch)
                {
                    var alarms = _alarmRepository.GetAlarmDetailsForBatch(status.BatchNumarasi, _machine.Id);
                    var alarmStrings = alarms.Any() ? alarms.Select(a => a.AlarmDescription).ToList() : new List<string> { $"{Resources.bupartiicinalarmyok}" };

                    _currentlyDisplayedAlarms = alarmStrings;
                    lstAlarmlar.DataSource = _currentlyDisplayedAlarms;

                    // Yeni batch ise grafiği temizle ki eskiler kalmasın
                    formsPlotTemp.Plot.Clear();
                    formsPlotRpm.Plot.Clear();
                    formsPlotWater.Plot.Clear();

                    // Scatter nesnelerini null yap ki tekrar oluşturulsun
                    _tempScatter = null;
                    _rpmScatter = null;
                    _waterScatter = null;
                }

                // Batch ID aynı olsa bile verileri grafiğe YÜKLE (Continuous Update)
                LoadTimelineChartForBatch(status.BatchNumarasi);

                // --- DÜZELTME BURADA BİTİYOR ---
            }
            else
            {
                if (_lastLoadedBatchIdForChart != null)
                {
                    LoadDataForLive(status);
                    UpdateAlarmList();
                }
                _lastLoadedBatchIdForChart = null;
                LoadDataForLive(status);
            }

            HighlightCurrentStep(status.AktifAdimNo);
        }

        // --- GRAFİK & VERİ YÜKLEME ---
        private void LoadDataForBatch(FullMachineStatus status)
        {
            _lastLoadedBatchIdForChart = status.BatchNumarasi;
            var alarms = _alarmRepository.GetAlarmDetailsForBatch(status.BatchNumarasi, _machine.Id);
            var alarmStrings = alarms.Any() ? alarms.Select(a => a.AlarmDescription).ToList() : new List<string> { $"{Resources.bupartiicinalarmyok}" };

            _currentlyDisplayedAlarms = alarmStrings;
            lstAlarmlar.DataSource = _currentlyDisplayedAlarms;
            LoadTimelineChartForBatch(status.BatchNumarasi);
        }

        private void LoadDataForLive(FullMachineStatus status)
        {
            LoadTimelineChartForLive();
        }

        private async void LoadRecipeStepsFromPlcAsync()
        {
            dgvAdimlar.DataSource = new List<object> { new { Adım = "...", Açıklama = $"{Resources.receteplcdenokunuyor}" } };

            if (_pollingService.GetPlcManagers().TryGetValue(_machine.Id, out var plcManager))
            {
                var result = await plcManager.ReadRecipeFromPlcAsync();
                if (result.IsSuccess)
                {
                    var steps = new List<ScadaRecipeStep>();
                    var rawData = result.Content;

                    if (_machine.MachineType == $"{Resources.kurutmamakinesi}")
                    {
                        var step = new ScadaRecipeStep { StepNumber = 1 };
                        Array.Copy(rawData, 0, step.StepDataWords, 0, Math.Min(rawData.Length, 6));
                        steps.Add(step);
                    }
                    else
                    {
                        for (int i = 0; i < 98; i++)
                        {
                            var step = new ScadaRecipeStep { StepNumber = i + 1 };
                            int offset = i * 25;
                            if (offset + 25 <= rawData.Length)
                            {
                                Array.Copy(rawData, offset, step.StepDataWords, 0, 25);
                                steps.Add(step);
                            }
                        }
                    }
                    dgvAdimlar.DataSource = steps.Select(s => new { Adım = s.StepNumber, Açıklama = GetStepTypeName(s) }).ToList();
                }
                else
                {
                    dgvAdimlar.DataSource = new List<object> { new { Adım = "!", Açıklama = $"{Resources.plcdenreceteokunmadı} {result.Message}" } };
                }
            }
            else
            {
                dgvAdimlar.DataSource = new List<object> { new { Adım = "!", Açıklama = $"{Resources.makinebaglantısıbulunamadı}" } };
            }
        }

        private void LoadTimelineChartForBatch(string batchId)
        {
            SafeInvoke(() =>
            {
                var (startTime, endTime) = _productionRepository.GetBatchTimestamps(batchId, _machine.Id);

                if (!startTime.HasValue)
                {
                    formsPlotTemp.Plot.Title($"{Resources.partibaslangıczamanıkayip}");
                    formsPlotTemp.Refresh();
                    return;
                }

                DateTime effectiveEndTime = endTime ?? DateTime.Now;
                var dataPoints = _logRepository.GetLogsForDateRange(_machine.Id, startTime.Value, effectiveEndTime);

                if (!dataPoints.Any())
                {
                    formsPlotTemp.Plot.Title($"{Resources.bupartihenüzkaydedilmemis}");
                    formsPlotTemp.Refresh();
                    return;
                }

                // Verileri hazırla
                var xs = dataPoints.Select(p => p.Timestamp.ToOADate()).ToArray();
                bool isDrying = _machine.MachineType == "Kurutma Makinesi";
                double[] ysTemp;

                if (!isDrying)
                {
                  ysTemp = dataPoints.Select(p => (double)p.Temperature / 10.0).ToArray();
                }
                else { ysTemp = dataPoints.Select(p => (double)p.Temperature / 100.0).ToArray(); }


                    var ysRpm = dataPoints.Select(p => (double)p.Rpm).ToArray();
                var ysWater = dataPoints.Select(p => (double)p.WaterLevel).ToArray();

                // --- DÜZELTME: Scatter nesnelerini yönetme ---

                // Temp Scatter güncelleme veya oluşturma
                if (_tempScatter == null || !formsPlotTemp.Plot.GetPlottables().Contains(_tempScatter))
                {
                    formsPlotTemp.Plot.Clear(); // Temizle
                    formsPlotTemp.Plot.Title($"{_machine.MachineName} - {Resources.proseszamancizgisi} ({batchId})");

                    _tempScatter = formsPlotTemp.Plot.Add.Scatter(xs, ysTemp);
                    _tempScatter.Color = ScottPlot.Colors.Red;
                    _tempScatter.LineWidth = 1;
                    _tempScatter.MarkerSize = 0;
                    formsPlotTemp.Plot.Axes.DateTimeTicksBottom();
                }
                else
                {
                    // Mevcut grafiği güncelle (daha performanslı)
                    formsPlotTemp.Plot.Remove(_tempScatter);
                    _tempScatter = formsPlotTemp.Plot.Add.Scatter(xs, ysTemp);
                    _tempScatter.Color = ScottPlot.Colors.Red;
                    _tempScatter.LineWidth = 1;
                    _tempScatter.MarkerSize = 0;
                }

                // RPM Scatter güncelleme veya oluşturma
                if (_rpmScatter == null || !formsPlotRpm.Plot.GetPlottables().Contains(_rpmScatter))
                {
                    formsPlotRpm.Plot.Clear();
                    _rpmScatter = formsPlotRpm.Plot.Add.Scatter(xs, ysRpm);
                    _rpmScatter.Color = ScottPlot.Colors.Green;
                    _rpmScatter.LineWidth = 1;
                    _rpmScatter.MarkerSize = 0;
                    formsPlotRpm.Plot.Axes.DateTimeTicksBottom();
                }
                else
                {
                    formsPlotRpm.Plot.Remove(_rpmScatter);
                    _rpmScatter = formsPlotRpm.Plot.Add.Scatter(xs, ysRpm);
                    _rpmScatter.Color = ScottPlot.Colors.Green;
                    _rpmScatter.LineWidth = 1;
                    _rpmScatter.MarkerSize = 0;
                }

                // Water Scatter güncelleme veya oluşturma
                if (_waterScatter == null || !formsPlotWater.Plot.GetPlottables().Contains(_waterScatter))
                {
                    formsPlotWater.Plot.Clear();
                    _waterScatter = formsPlotWater.Plot.Add.Scatter(xs, ysWater);
                    _waterScatter.Color = ScottPlot.Colors.Blue;
                    _waterScatter.LineWidth = 1;
                    _waterScatter.MarkerSize = 0;
                    formsPlotWater.Plot.Axes.DateTimeTicksBottom();
                }
                else
                {
                    formsPlotWater.Plot.Remove(_waterScatter);
                    _waterScatter = formsPlotWater.Plot.Add.Scatter(xs, ysWater);
                    _waterScatter.Color = ScottPlot.Colors.Blue;
                    _waterScatter.LineWidth = 1;
                    _waterScatter.MarkerSize = 0;
                }

                // Not: AutoScale her seferinde çağrılırsa kullanıcı zoom yapamaz. 
                // Ancak sürekli akan bir veri olduğu için Y eksenini otomatize edebiliriz.
                // X eksenini UpdateLiveGauges yönetiyor (Scrolling).

                formsPlotTemp.Plot.Axes.AutoScaleY();
                formsPlotRpm.Plot.Axes.AutoScaleY();
                formsPlotWater.Plot.Axes.AutoScaleY();

                formsPlotTemp.Refresh();
                formsPlotRpm.Refresh();
                formsPlotWater.Refresh();
            });
        }

        private void LoadTimelineChartForLive()
        {
            // *** DEĞİŞİKLİK BURADA (BAŞLANGIÇ) ***
            // Veri penceresini 360 dakika (6 saat) olarak ayarlıyoruz
            int timeWindowMinutes = 360;

            SafeInvoke(() =>
            {
                DateTime endTime = DateTime.Now;
                DateTime startTime = endTime.AddMinutes(-timeWindowMinutes); // Geriye dönük 360 dakikalık veri

                var dataPoints = _logRepository.GetManualLogs(_machine.Id, startTime, endTime);

                if (!dataPoints.Any())
                {
                    formsPlotTemp.Plot.Title($"{Resources.canlidata} (Veri Yok)");
                    formsPlotTemp.Refresh();
                    formsPlotRpm.Refresh();
                    formsPlotWater.Refresh();
                    return;
                }

                double[] timeData = dataPoints.Select(p => p.Timestamp.ToOADate()).ToArray();
                
                bool isDrying = _machine.MachineType == "Kurutma Makinesi";

                double[] tempData;

                // Sonra koşula göre içini doldur

                if (!isDrying)
                {
                    tempData = dataPoints.Select(p => (double)p.Temperature / 10.0).ToArray();
                }
                else { tempData = dataPoints.Select(p => (double)p.Temperature / 100.0).ToArray(); }
                
                double[] rpmData = dataPoints.Select(p => (double)p.Rpm).ToArray();
                double[] waterLevelData = dataPoints.Select(p => (double)p.WaterLevel).ToArray();

                if (_tempScatter == null)
                {
                    formsPlotTemp.Plot.Clear(); formsPlotRpm.Plot.Clear(); formsPlotWater.Plot.Clear();

                    formsPlotTemp.Plot.Axes.DateTimeTicksBottom();
                    formsPlotRpm.Plot.Axes.DateTimeTicksBottom();
                    formsPlotWater.Plot.Axes.DateTimeTicksBottom();

                    formsPlotTemp.Plot.Title($"{_machine.MachineName} - {Resources.canliprosesdata}");

                    _tempScatter = formsPlotTemp.Plot.Add.Scatter(timeData, tempData);
                    _tempScatter.Color = ScottPlot.Colors.Red;
                    _tempScatter.LineWidth = 1;
                    _tempScatter.MarkerSize = 0;
                    formsPlotTemp.Plot.Axes.Left.Label.Text = Resources.Temperature;
                    formsPlotTemp.Plot.Axes.Left.Label.ForeColor = ScottPlot.Colors.Red;
                    formsPlotTemp.Plot.Axes.Left.TickLabelStyle.ForeColor = ScottPlot.Colors.Red;

                    _rpmScatter = formsPlotRpm.Plot.Add.Scatter(timeData, rpmData);
                    _rpmScatter.Color = ScottPlot.Colors.Green;
                    _rpmScatter.LineWidth = 1;
                    _rpmScatter.MarkerSize = 0;
                    formsPlotRpm.Plot.Axes.Left.Label.Text = Resources.devir;
                    formsPlotRpm.Plot.Axes.Left.Label.ForeColor = ScottPlot.Colors.Green;
                    formsPlotRpm.Plot.Axes.Left.TickLabelStyle.ForeColor = ScottPlot.Colors.Green;

                    _waterScatter = formsPlotWater.Plot.Add.Scatter(timeData, waterLevelData);
                    _waterScatter.Color = ScottPlot.Colors.Blue;
                    _waterScatter.LineWidth = 1;
                    _waterScatter.MarkerSize = 0;
                    
                    if (_machine.MachineType == "Kurutma Makinesi")
                    {

                        formsPlotWater.Plot.Axes.Left.Label.Text = "Humidity (Rh)";
                    }
                    else {

                        
                        formsPlotWater.Plot.Axes.Left.Label.Text = Resources.suseviyesi;

                    }
                        formsPlotWater.Plot.Axes.Left.Label.ForeColor = ScottPlot.Colors.Blue;
                    formsPlotWater.Plot.Axes.Left.TickLabelStyle.ForeColor = ScottPlot.Colors.Blue;

                    // Sayfa ilk açıldığında otomatik 5 dakikalık zoom yapılıyor
                    double startZoomOA = endTime.AddMinutes(-5).ToOADate(); // Son 5 dakikanın başlangıcı
                    double endOA = endTime.ToOADate();

                    // Grafik X eksen limitlerini son 5 dakikaya ayarla (otomatik zoom)
                    formsPlotTemp.Plot.Axes.SetLimitsX(startZoomOA, endOA);
                    formsPlotRpm.Plot.Axes.SetLimitsX(startZoomOA, endOA);
                    formsPlotWater.Plot.Axes.SetLimitsX(startZoomOA, endOA);

                    formsPlotTemp.Plot.Axes.AutoScaleY();
                    formsPlotRpm.Plot.Axes.AutoScaleY();
                    formsPlotWater.Plot.Axes.AutoScaleY();
                }
                else
                {
                    formsPlotTemp.Plot.Remove(_tempScatter);
                    formsPlotRpm.Plot.Remove(_rpmScatter);
                    formsPlotWater.Plot.Remove(_waterScatter);

                    _tempScatter = formsPlotTemp.Plot.Add.Scatter(timeData, tempData);
                    _tempScatter.Color = ScottPlot.Colors.Red;
                    _tempScatter.LineWidth = 1;
                    _tempScatter.MarkerSize = 0;
                    _rpmScatter = formsPlotRpm.Plot.Add.Scatter(timeData, rpmData);
                    _rpmScatter.Color = ScottPlot.Colors.Green;
                    _rpmScatter.LineWidth = 1;
                    _rpmScatter.MarkerSize = 0;
                    _waterScatter = formsPlotWater.Plot.Add.Scatter(timeData, waterLevelData);
                    _waterScatter.Color = ScottPlot.Colors.Blue;
                    _waterScatter.LineWidth = 1;
                    _waterScatter.MarkerSize = 0;

                    // Not: Yenilemelerde X ekseni limitleri SyncAxes metodu sayesinde korunur.
                    // Bu nedenle burada tekrar zoom yapmaya gerek yoktur.
                }
                // *** DEĞİŞİKLİK BURADA (SON) ***
                formsPlotTemp.Plot.Axes.AutoScaleY();
                formsPlotRpm.Plot.Axes.AutoScaleY();
                formsPlotWater.Plot.Axes.AutoScaleY();
                formsPlotTemp.Refresh();
                formsPlotRpm.Refresh();
                formsPlotWater.Refresh();
            });
        }

        private void ClearBatchSpecificFieldsWithMessage(string message)
        {
            lstAlarmlar.DataSource = new List<string> { message };
            dgvAdimlar.DataSource = null;
            formsPlotTemp.Plot.Clear(); formsPlotTemp.Plot.Title(message); formsPlotTemp.Refresh();
            formsPlotRpm.Plot.Clear(); formsPlotRpm.Refresh();
            formsPlotWater.Plot.Clear(); formsPlotWater.Refresh();
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
                            row.DefaultCellStyle.BackColor = System.Drawing.Color.LightGreen;
                            row.DefaultCellStyle.Font = new System.Drawing.Font(dgvAdimlar.Font, System.Drawing.FontStyle.Bold);
                        }
                        else
                        {
                            row.DefaultCellStyle.BackColor = System.Drawing.Color.White;
                            row.DefaultCellStyle.Font = new System.Drawing.Font(dgvAdimlar.Font, System.Drawing.FontStyle.Regular);
                        }
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.White;
                        row.DefaultCellStyle.Font = new System.Drawing.Font(dgvAdimlar.Font, System.Drawing.FontStyle.Regular);
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
            if ((controlWord & 64) != 0) stepTypes.Add("Humidity Working");
            if ((controlWord & 128) != 0) stepTypes.Add("Timed Working");
            if ((controlWord & 256) != 0) stepTypes.Add("Humidity/Timed Working");
            if ((controlWord & 512) != 0) stepTypes.Add("Cooling");
            return string.Join(" + ", stepTypes);
        }
      
        // --- RENK HESAPLAMA ---
        private System.Drawing.Color GetTemperatureColor(int temp)
        {
            if (temp < 40) return System.Drawing.Color.DodgerBlue;
            if (temp < 60) return System.Drawing.Color.SeaGreen;
            if (temp < 90) return System.Drawing.Color.Orange;
            return System.Drawing.Color.Crimson;
        }

        // --- CUSTOM PAINT METODU (DİKEY BAR) ---
        private void progressTemp_Paint(object sender, PaintEventArgs e)
        {
            // Panel olup olmadığını kontrol et (Eğer hala ProgressBar ise Panel'e çevirin!)
            Control barControl = sender as Control;
            if (barControl == null) return;

            // Değeri Tag'den oku
            float currentValue = 0;
            if (barControl.Tag != null)
            {
                try { currentValue = Convert.ToSingle(barControl.Tag); } catch { }
            }

            // Maksimum değer (Termometre için 150 mantıklıdır, ancak kodunuzda 100f kullanılmış)
            float maximumValue = 100f; // Burayı 150f yapmak isterseniz değiştirebilirsiniz

            // Sınırla
            currentValue = Math.Max(0, Math.Min(maximumValue, currentValue));

            // Boyutlar
            int w = barControl.Width;
            int h = barControl.Height;

            // Arka Plan (Temizle)
            e.Graphics.FillRectangle(new SolidBrush(System.Drawing.Color.WhiteSmoke), 0, 0, w, h);

            // Doluluk Oranı
            float ratio = currentValue / maximumValue;
            int fillHeight = (int)(h * ratio);

            // Y Koordinatı (Aşağıdan yukarı dolması için: Toplam Boy - Dolu Boy)
            int yPos = h - fillHeight;

            // Çizim
            Rectangle filledRect = new Rectangle(0, yPos, w, fillHeight);

            // *** BURADA DEĞİŞİKLİK YAPILDI ***
            // Sabit kırmızı renk (System.Drawing.Color.Red) kullanılıyor
            using (SolidBrush brush = new SolidBrush(System.Drawing.Color.Red)) // Rengi direkt kırmızı yaptık
            {
                e.Graphics.FillRectangle(brush, filledRect);
            }
            // **********************************

            // Çerçeve
            using (Pen borderPen = new Pen(System.Drawing.Color.LightGray, 1))
            {
                e.Graphics.DrawRectangle(borderPen, 0, 0, w - 1, h - 1);
            }
        }
        private void humuditybar_Paint(object sender, PaintEventArgs e)
        {
            // Panel olup olmadığını kontrol et (Eğer hala ProgressBar ise Panel'e çevirin!)
            Control barControl = sender as Control;
            if (barControl == null) return;

            // Değeri Tag'den oku
            float currentValue = 0;
            if (barControl.Tag != null)
            {
                try { currentValue = Convert.ToSingle(barControl.Tag); } catch { }
            }

            // Maksimum değer (Termometre için 150 mantıklıdır, ancak kodunuzda 100f kullanılmış)
            float maximumValue = 100f; // Burayı 150f yapmak isterseniz değiştirebilirsiniz

            // Sınırla
            currentValue = Math.Max(0, Math.Min(maximumValue, currentValue));

            // Boyutlar
            int w = barControl.Width;
            int h = barControl.Height;

            // Arka Plan (Temizle)
            e.Graphics.FillRectangle(new SolidBrush(System.Drawing.Color.WhiteSmoke), 0, 0, w, h);

            // Doluluk Oranı
            float ratio = currentValue / maximumValue;
            int fillHeight = (int)(h * ratio);

            // Y Koordinatı (Aşağıdan yukarı dolması için: Toplam Boy - Dolu Boy)
            int yPos = h - fillHeight;

            // Çizim
            Rectangle filledRect = new Rectangle(0, yPos, w, fillHeight);

            // *** BURADA DEĞİŞİKLİK YAPILDI ***
            // Sabit kırmızı renk (System.Drawing.Color.Red) kullanılıyor
            using (SolidBrush brush = new SolidBrush(System.Drawing.Color.Blue)) // Rengi direkt kırmızı yaptık
            {
                e.Graphics.FillRectangle(brush, filledRect);
            }
            // **********************************

            // Çerçeve
            using (Pen borderPen = new Pen(System.Drawing.Color.LightGray, 1))
            {
                e.Graphics.DrawRectangle(borderPen, 0, 0, w - 1, h - 1);
            }
        }
        // ... Kalan olaylar ...
        private void lblMakineAdi_Click(object sender, EventArgs e) { }

        private void SafeInvoke(Action action)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                try { this.BeginInvoke(action); } catch (Exception) { }
            }
        }

        private void OnAlarmStateChanged(int machineId, FullMachineStatus status)
        {
            if (machineId == _machine.Id && this.IsHandleCreated && !this.IsDisposed)
                this.BeginInvoke(new Action(UpdateAlarmList));
        }

        private void UpdateAlarmList()
        {
            if (string.IsNullOrEmpty(_lastLoadedBatchIdForChart))
            {
                var activeAlarms = _pollingService.GetActiveAlarmsForMachine(_machine.Id);
                List<string> newAlarmList;

                if (activeAlarms.Any())
                    newAlarmList = activeAlarms.Select(a => $"#{a.AlarmNumber}: {a.AlarmText}").ToList();
                else
                    newAlarmList = new List<string> { $"{Resources.aktifalarmyok}" };

                if (!_currentlyDisplayedAlarms.SequenceEqual(newAlarmList))
                {
                    _currentlyDisplayedAlarms = newAlarmList;
                    lstAlarmlar.DataSource = _currentlyDisplayedAlarms;
                }
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            // 1. Dil değişikliği aboneliğini kaldır (Memory Leak önlemi)
            LanguageManager.LanguageChanged -= LanguageManager_LanguageChanged;

            // 2. Servis aboneliklerini kaldır
            if (_pollingService != null)
            {
                _pollingService.OnMachineDataRefreshed -= OnDataRefreshed;
                _pollingService.OnMachineConnectionStateChanged -= OnConnectionStateChanged;
                _pollingService.OnActiveAlarmStateChanged -= OnAlarmStateChanged;
            }

            // 3. Timer'ı durdur ve yok et
            if (_uiUpdateTimer != null)
            {
                _uiUpdateTimer.Stop();
                _uiUpdateTimer.Dispose();
            }

            base.OnHandleDestroyed(e);
        }

        private void pnlAlarmsAndSteps_Paint(object sender, PaintEventArgs e) { }
    }
}