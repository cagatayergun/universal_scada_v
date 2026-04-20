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
        private RecipeRepository _recipeRepository;
        private MachineRepository _machineRepository;
        private Dictionary<int, IPlcManager> _plcManagers;
        private UserRepository _userRepository; // YENİ: Loglama için eklendi
        private List<ScadaRecipe> _recipeList;
        private ScadaRecipe _currentRecipe;

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

        private int _lastValidProgress = 0;

        public DashboardMachineCard_Control(Machine machine)
        {
            InitializeComponent();
            _machine = machine;

            // Başlık ayarı (Örn: Vinç No: 01)
            lblMachineName.Text = _machine.MachineName;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            // Vinç Prosesine Uygun Renk Teması
            lblMachineName.ForeColor = Color.FromArgb(44, 62, 80);

            // Yük Kapasite Limitini (SWL) Veritabanından Getir
            SetLoadGaugeLimitAsync();
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

            }
            catch { }

            // Kanca Yüksekliği ve Kedi Pozisyonu (Mesafe birimleri)
            // status.AnlikSicaklik -> Kanca Yüksekliği (Örn: 155 -> 15.5m)


            // Alt Bar: Kapasite Kullanımı (%)
            _lastValidProgress = Math.Max(0, Math.Min(100, (int)status.ProsesYuzdesi));


            // Durum ve Alarm Yönetimi
            if (status.HasActiveAlarm)
            {
                pnlStatusIndicator.BackColor = _colorAlarm;
                lblStatus.Text = $"ALARM #{status.ActiveAlarmText}";
                lblStatus.ForeColor = _colorAlarm;
                btnSendToPlc.ForeColor = Color.Black;

            }
            else
            {
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

        private void lblStatus_Click(object sender, EventArgs e)
        {

        }

        private async void BtnSendToPlc_Click(object sender, EventArgs e)
        {
            try
            {
                var result = await plcManager.WriteRecipeToPlcAsync(_currentRecipe, recipeSlot);

                if (result.IsSuccess)
                {
                    // --- LOGLAMA (PLC Gönderimi) ---
                    if (CurrentUser.User != null && _userRepository != null)
                    {
                        string slotInfo = recipeSlot.HasValue ? $"(Slot: {recipeSlot})" : "";
                        _userRepository.LogAction(
                            CurrentUser.User.Id,
                            "RECIPE_SEND_PLC",
                            $"Recipe '{_currentRecipe.RecipeName}' written to PLC of '{selectedMachine.MachineName}' {slotInfo} [rcp-{_currentRecipe.Id}]"
                        );
                    }
                    // ------------------------------

                    MessageBox.Show($"'Recipe '{_currentRecipe.RecipeName}' was successfully sent to machine '{selectedMachine.MachineName}'.", "Success");
                }
                else
                {
                    MessageBox.Show($"Error while sending prescription: {result.Message}", "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "System Error");
            }
        }
    }
}