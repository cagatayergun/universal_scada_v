// UI/Views/PlcOperatorSettings_Control.cs
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Universalscada.Models;
using Universalscada.Repositories;
using Universalscada.Services;
using System.Net.Http; // YENİ: API çağrıları için eklendi
using System.Text; // YENİ: JSON işlemleri için eklendi
using Newtonsoft.Json; // YENİ: JSON işlemleri için eklendi (NuGet'ten yükleyin)
using System.Net.Http.Headers; // YENİ: JWT Token için eklendi

namespace Universalscada.UI.Views
{
    public partial class PlcOperatorSettings_Control : UserControl
    {
        private MachineRepository _machineRepository;
        private PlcOperatorRepository _plcOperatorRepository;

        // === KALDIRILDI ===
        // private Dictionary<int, IPlcManager> _plcManagers;

        // === YENİ: WebAPI İstemcisi ===
        private static readonly HttpClient _apiClient = new HttpClient();
        // !!! KENDİ WEBAPI ADRESİNİZLE DEĞİŞTİRİN !!!
        private const string API_BASE_URL = "http://localhost:5000";

        public PlcOperatorSettings_Control()
        {
            InitializeComponent();
            _plcOperatorRepository = new PlcOperatorRepository();

            // YENİ: API İstemcisini bir kez ayarla
            if (_apiClient.BaseAddress == null)
            {
                _apiClient.BaseAddress = new Uri(API_BASE_URL);
            }
        }

        // === DEĞİŞTİ: InitializeControl ===
        // 'plcManagers' parametresi kaldırıldı
        public void InitializeControl(MachineRepository machineRepo)
        {
            _machineRepository = machineRepo;
            // _plcManagers = plcManagers; // KALDIRILDI
        }

        // YENİ: API çağrıları için JWT Token'ı ekleyen yardımcı metod
        private HttpClient GetApiClient()
        {
            // Her çağrıdan önce güncel token'ı ekle
            _apiClient.DefaultRequestHeaders.Authorization = null;
            if (CurrentUser.IsLoggedIn && !string.IsNullOrEmpty(CurrentUser.Token))
            {
                _apiClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", CurrentUser.Token);
            }
            return _apiClient;
        }

        private void PlcOperatorSettings_Control_Load(object sender, EventArgs e)
        {
            var machines = _machineRepository.GetAllEnabledMachines();
            cmbMachines.DataSource = machines;
            cmbMachines.DisplayMember = "DisplayInfo";
            cmbMachines.ValueMember = "Id";

            for (int i = 1; i <= 5; i++)
            {
                cmbSlot.Items.Add(i);
            }
            cmbSlot.SelectedIndex = 0;

            RefreshGrid();
        }

        private void RefreshGrid()
        {
            try
            {
                dgvOperators.DataSource = null;
                dgvOperators.DataSource = _plcOperatorRepository.GetAll();
                if (dgvOperators.Columns["SlotIndex"] != null) dgvOperators.Columns["SlotIndex"].HeaderText = "DB ID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading operator templates: {ex.Message}", "Error");
            }
        }

        // === DEĞİŞTİ: btnRead_Click ===
        // Artık 'plcManager' bulmuyor, 'machineId' ile API'yi çağırıyor
        private void btnRead_Click(object sender, EventArgs e)
        {
            if (cmbMachines.SelectedItem is Machine selectedMachine)
            {
                // if (_plcManagers.TryGetValue(selectedMachine.Id, out var plcManager)) // KALDIRILDI
                // {
                int slotIndex = cmbSlot.SelectedIndex; // 0-4
                ReadOperatorFromPlc(selectedMachine.Id, slotIndex); // machineId ile çağır
                // }
            }
        }

        // === DEĞİŞTİ: ReadOperatorFromPlc ===
        // Artık IPlcManager yerine WebAPI'yi kullanıyor
        private async void ReadOperatorFromPlc(int machineId, int slotIndex)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                var apiClient = GetApiClient();
                // YENİ API ÇAĞRISI (WebAPI'de /api/operators/read-from-plc/5/0 gibi bir endpoint gerekir)
                var response = await apiClient.GetAsync($"api/operators/read-from-plc/{machineId}/{slotIndex}");

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var opFromPlc = JsonConvert.DeserializeObject<PlcOperator>(jsonResponse);

                    if (opFromPlc != null)
                    {
                        _plcOperatorRepository.SaveOrUpdate(opFromPlc);
                        RefreshGrid();
                        MessageBox.Show($"The operator information at {slotIndex + 1} on the machine was read and added/updated to the list.", "Success");
                    }
                    else
                    {
                        MessageBox.Show("Failed to deserialize operator data from API.", "Error");
                    }
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error reading operator: {response.ReasonPhrase}\n{errorMsg}", "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API Connection Error: {ex.Message}", "Error");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        // === DEĞİŞTİ: btnSend_Click ===
        // Artık 'plcManager' bulmuyor, 'machineId' ile API'yi çağırıyor
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (dgvOperators.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an operator from the list to send to PLC.", "Warning");
                return;
            }
            if (cmbMachines.SelectedItem is Machine selectedMachine)
            {
                // if (_plcManagers.TryGetValue(selectedMachine.Id, out var plcManager)) // KALDIRILDI
                // {
                var selectedOperator = dgvOperators.SelectedRows[0].DataBoundItem as PlcOperator;
                int slotIndex = cmbSlot.SelectedIndex; // 0-4
                selectedOperator.SlotIndex = slotIndex;

                SendOperatorToPlc(selectedMachine.Id, selectedOperator); // machineId ile çağır
                // }
            }
        }

        // === DEĞİŞTİ: SendOperatorToPlc ===
        // Artık IPlcManager yerine WebAPI'yi kullanıyor
        private async void SendOperatorToPlc(int machineId, PlcOperator plcOperator)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                string jsonBody = JsonConvert.SerializeObject(plcOperator);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var apiClient = GetApiClient();
                // YENİ API ÇAĞRISI (WebAPI'de /api/operators/send-to-plc/5 gibi bir endpoint gerekir)
                var response = await apiClient.PostAsync($"api/operators/send-to-plc/{machineId}", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Operator '{plcOperator.Name}' was successfully written to slot {plcOperator.SlotIndex + 1} of the selected machine.", "Success");
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error sending operator: {response.ReasonPhrase}\n{errorMsg}", "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API Connection Error: {ex.Message}", "Error");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        // === DEĞİŞİKLİK YOK: btnDelete_Click ===
        // Bu metod sadece lokal veritabanını etkiler, PLC'ye gitmez.
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvOperators.SelectedRows.Count > 0)
            {
                var selectedOperator = dgvOperators.SelectedRows[0].DataBoundItem as PlcOperator;
                var result = MessageBox.Show($"'{selectedOperator.Name}' Are you sure you want to delete the template?", "Confirm", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    _plcOperatorRepository.Delete(selectedOperator.SlotIndex);
                    RefreshGrid();
                }
            }
        }

        // === DEĞİŞİKLİK YOK: ekle_Click ===
        // Bu metod sadece lokal veritabanını etkiler, PLC'ye gitmez.
        private void ekle_Click(object sender, EventArgs e)
        {
            try
            {
                _plcOperatorRepository.AddDefaultOperator();
                RefreshGrid();
                MessageBox.Show("A new blank operator template has been added successfully. Click on it to edit and save.", "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding a new operator: {ex.Message}", "Error");
            }
        }
    }
}