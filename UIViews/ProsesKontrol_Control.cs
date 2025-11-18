// UI/Views/ProsesKontrol_Control.cs
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq; // YENİ: .Any() ve .Cast() için
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows.Forms;
using Universalscada.core;
using Universalscada.Core.Repositories;
using Universalscada.Models;
using Universalscada.Repositories;
using Universalscada.Services;
using Universalscada.Services; // YENİ: CurrentUser için
using Universalscada.UI.Controls;
using Universalscada.UI.Controls.RecipeStepEditors;
using Universalscada.UIViews;

namespace Universalscada.UI.Views
{
    public partial class ProsesKontrol_Control : UserControl
    {
        private RecipeRepository _recipeRepository;
        private readonly IMachineRepository _machineRepository;

        // === KALDIRILDI ===
        // private Dictionary<int, IPlcManager> _plcManagers;
        // private PlcPollingService _plcPollingService;

        private List<ScadaRecipe> _recipeList;
        private ScadaRecipe _currentRecipe;

        private SplitContainer _byMakinesiEditor;
        private DataGridView dgvRecipeSteps;
        private Panel pnlStepDetails;
        private Label lblStepDetailsTitle;
        private CostRepository _costRepository;
        private FtpSync_Form _ftpFormInstance;
        private FtpTransferService _ftpTransferService;

        // === YENİ: WebAPI İstemcisi ===
        private static readonly HttpClient _apiClient = new HttpClient();
        // !!! KENDİ WEBAPI ADRESİNİZLE DEĞİŞTİRİN !!!
        private const string API_BASE_URL = "http://localhost:5000";

        public ProsesKontrol_Control()
        {
            InitializeComponent();
            _costRepository = new CostRepository();
            this.Load += ProsesKontrol_Control_Load;
            btnNewRecipe.Click += BtnNewRecipe_Click;
            btnDeleteRecipe.Click += BtnDeleteRecipe_Click;
            btnSaveRecipe.Click += BtnSaveRecipe_Click;
            btnSendToPlc.Click += BtnSendToPlc_Click;
            btnReadFromPlc.Click += BtnReadFromPlc_Click;
            lstRecipes.SelectedIndexChanged += LstRecipes_SelectedIndexChanged;
            cmbTargetMachine.SelectedIndexChanged += CmbTargetMachine_SelectedIndexChanged;
            btnFtpSync.Click += BtnFtpSync_Click;

            if (_apiClient.BaseAddress == null)
            {
                _apiClient.BaseAddress = new Uri(API_BASE_URL);
            }
        }

        // === DEĞİŞTİ: InitializeControl ===
        public void InitializeControl(RecipeRepository recipeRepository, IMachineRepository machineRepository, FtpTransferService ftpTransferService)
        {
            // Alan atamaları yapıldı (CS0103 hatalarını çözer)
            _recipeRepository = recipeRepository;
            _machineRepository = machineRepository;
            _ftpTransferService = ftpTransferService;
            // ...
        }

        private void ProsesKontrol_Control_Load(object sender, EventArgs e)
        {
            LoadRecipeList();
            LoadMachineList();
            ApplyRolePermissions();
            ApplyPermissions();
            // FtpTransferService.Instance.RecipeListChanged += OnRecipeListChanged; // Bu satır designer'da hata veriyorsa oradan da silinmeli
        }

        // YENİ: API çağrıları için JWT Token'ı ekleyen yardımcı metod
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

        // === EKLENDİ (Hata Grubu 3 Çözümü) ===
        #region EKSİK OLAN YARDIMCI METOTLAR

        private void ApplyPermissions()
        {
            btnDeleteRecipe.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });
            btnFtpSync.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });
            btnNewRecipe.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });
            btnReadFromPlc.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });
            btnSaveRecipe.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });
            btnSendToPlc.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });

            var master = PermissionService.HasAnyPermission(new List<int> { 1000 });
            if (master == true)
            {
                btnDeleteRecipe.Enabled = true;
                btnFtpSync.Enabled = true;
                btnNewRecipe.Enabled = true;
                btnReadFromPlc.Enabled = true;
                btnSaveRecipe.Enabled = true;
                btnSendToPlc.Enabled = true;
            }
        }

        private void ApplyRolePermissions()
        {
            // Bu metot eski kodunuzda boştu, isterseniz yetki kontrollerini buraya ekleyebilirsiniz.
        }

        private void OnRecipeListChanged(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => LoadRecipeList()));
            }
            else
            {
                LoadRecipeList();
            }
        }

        private void LoadMachineList()
        {
            var machines = _machineRepository.GetAllEnabledMachines();
            cmbTargetMachine.DataSource = machines;
            cmbTargetMachine.DisplayMember = "DisplayInfo";
            cmbTargetMachine.ValueMember = "Id";
        }

        private void LoadRecipeList()
        {
            try
            {
                int selectedId = (lstRecipes.SelectedItem as ScadaRecipe)?.Id ?? -1;
                _recipeList = _recipeRepository.GetAllRecipes();
                FilterRecipeList();

                if (selectedId != -1)
                {
                    var selectedItem = (lstRecipes.DataSource as List<ScadaRecipe>)?.FirstOrDefault(r => r.Id == selectedId);
                    if (selectedItem != null)
                    {
                        lstRecipes.SelectedItem = selectedItem;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading recipes: {ex.Message}", "Database Error");
            }
        }

        private void BtnNewRecipe_Click(object sender, EventArgs e)
        {
            var machineTypes = _machineRepository.GetAllEnabledMachines()
                .Select(m => !string.IsNullOrEmpty(m.MachineSubType) ? m.MachineSubType : m.MachineType)
                .Distinct()
                .ToList();

            if (!machineTypes.Any())
            {
                MessageBox.Show("No active machine type was found in the system for which a recipe could be created.", "Warning");
                return;
            }

            using (var typeForm = new RecipeTypeSelection_Form(machineTypes))
            {
                if (typeForm.ShowDialog() == DialogResult.OK)
                {
                    string selectedType = typeForm.SelectedType;
                    if (string.IsNullOrEmpty(selectedType)) return;

                    _currentRecipe = new ScadaRecipe
                    {
                        RecipeName = "NEW RECIPE",
                        TargetMachineType = selectedType
                    };

                    int stepCount = (selectedType == "Kurutma Makinesi") ? 1 : 98;
                    _currentRecipe.Steps.Clear();
                    for (int i = 1; i <= stepCount; i++)
                    {
                        _currentRecipe.Steps.Add(new ScadaRecipeStep { StepNumber = i });
                    }

                    lstRecipes.ClearSelected();
                    DisplayCurrentRecipe();
                }
            }
        }

        private void LstRecipes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstRecipes.SelectedItem is ScadaRecipe selected)
            {
                try
                {
                    _currentRecipe = _recipeRepository.GetRecipeById(selected.Id);
                    DisplayCurrentRecipe();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading prescription details: {ex.Message}", "Database Error");
                }
            }
        }

        private void CmbTargetMachine_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterRecipeList();

            if (_currentRecipe != null && cmbTargetMachine.SelectedItem is Machine selectedMachine)
            {
                string machineTypeForRecipe = !string.IsNullOrEmpty(selectedMachine.MachineSubType)
                                                ? selectedMachine.MachineSubType
                                                : selectedMachine.MachineType;

                if (_currentRecipe.TargetMachineType != machineTypeForRecipe)
                {
                    _currentRecipe = null;
                    lstRecipes.ClearSelected();
                    DisplayCurrentRecipe();
                }
            }
            else
            {
                _currentRecipe = null;
                DisplayCurrentRecipe();
            }
        }

        private string ShowFtpRecipeNumberDialog()
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "HMI Prescription Number",
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = "Enter the recipe number to be saved in the HMI (1-99):", Width = 300 };
            NumericUpDown inputBox = new NumericUpDown() { Left = 50, Top = 50, Width = 300, Minimum = 1, Maximum = 99 };
            Button confirmation = new Button() { Text = "Ok", Left = 250, Width = 100, Top = 90, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(inputBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? inputBox.Value.ToString() : "";
        }

        private void DisplayCurrentRecipe()
        {
            if (_currentRecipe != null)
            {
                txtRecipeName.Text = _currentRecipe.RecipeName;
                LoadEditorForSelectedMachine();
            }
            else
            {
                txtRecipeName.Text = "";
                pnlEditorArea.Controls.Clear();
            }
        }

        private void LoadEditorForSelectedMachine()
        {
            pnlEditorArea.Controls.Clear();
            var selectedMachine = cmbTargetMachine.SelectedItem as Machine;

            if (selectedMachine == null) return;

            if (selectedMachine.MachineType == "Kurutma Makinesi")
            {
                var editor = new KurutmaReçete_Control();
                editor.LoadRecipe(_currentRecipe);
                editor.ValueChanged += (s, ev) => { /* Değişiklikleri kaydetmek için event'i dinle */ };
                editor.Dock = DockStyle.Fill;
                pnlEditorArea.Controls.Add(editor);
            }
            else // Varsayılan olarak BYMakinesi
            {
                InitializeBYMakinesiEditor();
                PopulateStepsGridView();
                pnlEditorArea.Controls.Add(_byMakinesiEditor);
            }
        }

        private void InitializeBYMakinesiEditor()
        {
            _byMakinesiEditor = new SplitContainer();
            dgvRecipeSteps = new DataGridView();
            pnlStepDetails = new Panel();
            lblStepDetailsTitle = new Label();

            _byMakinesiEditor.Dock = DockStyle.Fill;
            _byMakinesiEditor.SplitterDistance = 40;

            _byMakinesiEditor.Panel1.Controls.Add(dgvRecipeSteps);
            _byMakinesiEditor.Panel2.Controls.Add(pnlStepDetails);

            dgvRecipeSteps.Dock = DockStyle.Fill;
            dgvRecipeSteps.AllowUserToAddRows = false;
            dgvRecipeSteps.AllowUserToDeleteRows = false;
            dgvRecipeSteps.MultiSelect = false;
            dgvRecipeSteps.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecipeSteps.CellClick += DgvRecipeSteps_CellClick;

            pnlStepDetails.Dock = DockStyle.Fill;
            pnlStepDetails.BorderStyle = BorderStyle.FixedSingle;
            pnlStepDetails.Controls.Add(lblStepDetailsTitle);

            lblStepDetailsTitle.Dock = DockStyle.Top;
            lblStepDetailsTitle.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            lblStepDetailsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblStepDetailsTitle.Text = "Step Details";

            SetupStepsGridView();
        }

        private void SetupStepsGridView()
        {
            if (dgvRecipeSteps == null) return;
            dgvRecipeSteps.DataSource = null;
            dgvRecipeSteps.Rows.Clear();
            dgvRecipeSteps.Columns.Clear();
            dgvRecipeSteps.AutoGenerateColumns = false;

            dgvRecipeSteps.Columns.Add(new DataGridViewTextBoxColumn { Name = "StepNumber", HeaderText = "Step No", DataPropertyName = "StepNumber", Width = 40 });
            dgvRecipeSteps.Columns.Add(new DataGridViewTextBoxColumn { Name = "StepType", HeaderText = "Step Type", Width = 300 });
        }

        private void PopulateStepsGridView()
        {
            if (_currentRecipe == null || _currentRecipe.Steps == null || dgvRecipeSteps == null) return;
            dgvRecipeSteps.Rows.Clear();
            foreach (var step in _currentRecipe.Steps)
            {
                string stepTypeName = GetStepTypeName(step);
                dgvRecipeSteps.Rows.Add(step.StepNumber, stepTypeName);
            }
        }

        private string GetStepTypeName(ScadaRecipeStep step)
        {
            var stepTypes = new List<string>();
            short controlWord = step.StepDataWords[24];
            if ((controlWord & 1) != 0) stepTypes.Add("Water Intake");
            if ((controlWord & 2) != 0) stepTypes.Add("Heating");
            if ((controlWord & 4) != 0) stepTypes.Add("Working");
            if ((controlWord & 8) != 0) stepTypes.Add("Dozage");
            if ((controlWord & 16) != 0) stepTypes.Add("Unloading");
            if ((controlWord & 32) != 0) stepTypes.Add("Squeezing");
            return string.Join(" + ", stepTypes);
        }

        private void DgvRecipeSteps_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _currentRecipe == null || pnlStepDetails == null) return;
            try
            {
                var stepNumberCell = dgvRecipeSteps.Rows[e.RowIndex].Cells["StepNumber"].Value;
                if (stepNumberCell == null) return;
                int stepNumberToFind = Convert.ToInt32(stepNumberCell);
                var selectedStep = _currentRecipe.Steps.FirstOrDefault(s => s.StepNumber == stepNumberToFind);
                if (selectedStep == null) return;

                pnlStepDetails.Controls.Clear();
                pnlStepDetails.Controls.Add(lblStepDetailsTitle);

                var selectedMachine = cmbTargetMachine.SelectedItem as Machine;
                lblStepDetailsTitle.Text = $"Step Details - Step No: {selectedStep.StepNumber}";

                var mainEditor = new StepEditor_Control();
                mainEditor.LoadStep(selectedStep, selectedMachine);

                mainEditor.StepDataChanged += (s, ev) =>
                {
                    if (dgvRecipeSteps.Rows.Count > e.RowIndex)
                    {
                        dgvRecipeSteps.Rows[e.RowIndex].Cells["StepType"].Value = GetStepTypeName(selectedStep);
                    }
                };
                mainEditor.Dock = DockStyle.Fill;
                pnlStepDetails.Controls.Add(mainEditor);
                mainEditor.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading step details: {ex.Message}", "Error");
            }
        }

        #endregion

        // === API'YE TAŞINAN METOTLAR (İÇERİKLERİ DEĞİŞTİ) ===

        private void BtnFtpSync_Click(object sender, EventArgs e)
        {
            var ftpMachineTypes = _machineRepository.GetAllEnabledMachines()
               .Where(m => !string.IsNullOrEmpty(m.FtpUsername) && m.MachineType != "Kurutma Makinesi")
               .Select(m => !string.IsNullOrEmpty(m.MachineSubType) ? m.MachineSubType : m.MachineType)
               .Distinct()
               .ToList();

            if (!ftpMachineTypes.Any())
            {
                MessageBox.Show("No suitable machine type was found in the system for FTP transfer.", "Warning");
                return;
            }

            using (var typeForm = new RecipeTypeSelection_Form(ftpMachineTypes))
            {
                if (typeForm.ShowDialog() == DialogResult.OK)
                {
                    string selectedType = typeForm.SelectedType;
                    if (string.IsNullOrEmpty(selectedType)) return;

                    if (_ftpFormInstance != null && !_ftpFormInstance.IsDisposed)
                    {
                        _ftpFormInstance.BringToFront();
                    }
                    else
                    {
                        // FtpSync_Form'a artık 'null' geçiyoruz.
                        _ftpFormInstance = new FtpSync_Form(_machineRepository, _recipeRepository, null, selectedType, _ftpTransferService); // _plcPollingService -> null
                        _ftpFormInstance.FormClosed += (s, args) => _ftpFormInstance = null;
                        _ftpFormInstance.Show(this);
                    }
                }
            }
        }

        private async void BtnSendToPlc_Click(object sender, EventArgs e)
        {
            if (_currentRecipe == null || cmbTargetMachine.SelectedItem is not Machine selectedMachine)
            {
                MessageBox.Show("Please select a recipe and target machine.", "Warning");
                return;
            }

            btnSendToPlc.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (selectedMachine.MachineType == "BYMakinesi")
                {
                    if (string.IsNullOrEmpty(selectedMachine.FtpUsername) || string.IsNullOrEmpty(selectedMachine.IpAddress))
                    {
                        MessageBox.Show("FTP information (IP Address, Username) is missing for this machine. Please enter the information from the Settings > Machine Management screen.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string recipeNumberStr = ShowFtpRecipeNumberDialog();
                    if (string.IsNullOrEmpty(recipeNumberStr)) return;

                    if (!int.TryParse(recipeNumberStr, out int recipeNumber) || recipeNumber < 1 || recipeNumber > 99)
                    {
                        MessageBox.Show("Invalid prescription number. Please enter a number between 1-99.", "Error");
                        return;
                    }
                    string remoteFileName = string.Format("XPR{0:D5}.csv", recipeNumber);

                    try
                    {
                        string csvContent = RecipeCsvConverter.ToCsv(_currentRecipe);
                        var ftpService = new FtpService(selectedMachine.IpAddress, selectedMachine.FtpUsername, selectedMachine.FtpPassword);
                        await ftpService.UploadFileAsync($"/{remoteFileName}", csvContent);
                        MessageBox.Show($"'Recipe '{_currentRecipe.RecipeName}' was successfully sent to machine '{selectedMachine.MachineName}' with name '{remoteFileName}'.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error sending recipe via FTP: {ex.Message}", "FTP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else // Kurutma Makinesi gibi diğer makineler için YENİ WebAPI MANTIĞI
                {
                    int? recipeSlot = null;
                    if (selectedMachine.MachineType == "Kurutma Makinesi")
                    {
                        string input = ShowInputDialog("Please enter the recipe number to be registered in the PLC (1-20):", true);
                        if (int.TryParse(input, out int slot) && slot >= 1 && slot <= 20)
                        {
                            recipeSlot = slot;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(input)) { MessageBox.Show("You have entered an invalid prescription number.", "Error"); }
                            return;
                        }
                    }

                    try
                    {
                        var requestBody = new
                        {
                            Recipe = _currentRecipe,
                            MachineId = selectedMachine.Id,
                            Slot = recipeSlot
                        };
                        string jsonBody = JsonConvert.SerializeObject(requestBody);
                        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                        var apiClient = GetApiClient();
                        var response = await apiClient.PostAsync("api/recipes/send-to-plc", content);

                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show($"'Recipe '{_currentRecipe.RecipeName}' was successfully sent to machine '{selectedMachine.MachineName}'.", "Success");
                        }
                        else
                        {
                            string errorMsg = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Error while sending prescription: {response.ReasonPhrase}\n{errorMsg}", "Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An unexpected error occurred: {ex.Message}", "System Error");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while sending: {ex.Message}", "Error");
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnSendToPlc.Enabled = true;
            }
        }

        public static string ShowInputDialog(string text, bool isNumeric = false)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Login Required",
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text, Width = 400 };
            Control inputBox;
            if (isNumeric)
            {
                inputBox = new NumericUpDown() { Left = 50, Top = 50, Width = 400, Minimum = 1, Maximum = 98 };
            }
            else
            {
                inputBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            }
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 90, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(inputBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            return prompt.ShowDialog() == DialogResult.OK ? inputBox.Text : "";
        }

        private async void BtnReadFromPlc_Click(object sender, EventArgs e)
        {
            var selectedMachine = cmbTargetMachine.SelectedItem as Machine;
            if (selectedMachine == null) { MessageBox.Show("Please select a target machine.", "Warning"); return; }

            btnReadFromPlc.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                var apiClient = GetApiClient();
                var response = await apiClient.GetAsync($"api/recipes/read-from-plc/{selectedMachine.Id}");

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var recipeFromPlc = JsonConvert.DeserializeObject<ScadaRecipe>(jsonResponse);

                    if (recipeFromPlc != null)
                    {
                        recipeFromPlc.RecipeName = $"PLC_{selectedMachine.MachineUserDefinedId}_{DateTime.Now:HHmm}";
                        _currentRecipe = recipeFromPlc;
                        DisplayCurrentRecipe();
                        MessageBox.Show($"'{selectedMachine.MachineName}' The recipe in the machine was read successfully.Please give a new name and save it.", "Successful");
                    }
                    else
                    {
                        MessageBox.Show("An empty recipe was read from the PLC.", "Error");
                    }
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error reading prescription: {response.ReasonPhrase}\n{errorMsg}", "Error");
                }
            }
            catch (NotImplementedException)
            {
                MessageBox.Show($"'{selectedMachine.MachineType}' The recipe reading feature for the type is not yet complete.", "Under Development");
            }
            catch (Exception ex) { MessageBox.Show($"An unexpected error occurred: {ex.Message}", "System Error"); }
            finally
            {
                this.Cursor = Cursors.Default;
                btnReadFromPlc.Enabled = true;
            }
        }

        // === VERİTABANI İŞLEM METOTLARI (DEĞİŞİKLİK YOK) ===
        #region Kalan Metotlar (Değişiklik Yok)

        private void BtnSaveRecipe_Click(object sender, EventArgs e)
        {
            if (_currentRecipe == null) { MessageBox.Show("There is no prescription to save.", "Warning"); return; }
            if (string.IsNullOrWhiteSpace(txtRecipeName.Text)) { MessageBox.Show("Prescription name cannot be empty.", "Warning"); return; }
            _currentRecipe.RecipeName = txtRecipeName.Text;
            try
            {
                _recipeRepository.SaveRecipe(_currentRecipe);
                MessageBox.Show("Reçete başarıyla kaydedildi.", "Başarılı");
                LoadRecipeList();
            }
            catch (Exception ex) { MessageBox.Show($"An error occurred while saving the recipe: {ex.Message}", "Error"); }
        }

        private void BtnDeleteRecipe_Click(object sender, EventArgs e)
        {
            var selectedRecipes = lstRecipes.SelectedItems.Cast<ScadaRecipe>().ToList();
            if (!selectedRecipes.Any())
            {
                MessageBox.Show("Please select at least one recipe from the list to delete.", "Warning");
                return;
            }

            var result = MessageBox.Show(
                $"{selectedRecipes.Count} Are you sure you want to permanently delete the prescription?\nThis action cannot be undone.", "Bulk Deletion Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    foreach (var recipeToDelete in selectedRecipes)
                    {
                        _recipeRepository.DeleteRecipe(recipeToDelete.Id);
                    }
                    MessageBox.Show($"{selectedRecipes.Count} The prescription was deleted successfully.", "Process Completed");
                    _currentRecipe = null;
                    DisplayCurrentRecipe();
                    LoadRecipeList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting prescriptions: {ex.Message}", "Error");
                }
            }
        }

        private void btnCalculateCost_Click(object sender, EventArgs e)
        {
            if (_currentRecipe == null)
            {
                MessageBox.Show("Please select or create a prescription to calculate its cost.", "Warning");
                return;
            }
            _currentRecipe.RecipeName = txtRecipeName.Text;
            try
            {
                var costParams = _costRepository.GetAllParameters();
                var (totalCost, currencySymbol, breakdown) = RecipeCostCalculator.Calculate(_currentRecipe, costParams);
                lblTotalCost.Text = $"{totalCost:F2} {currencySymbol}";
                ToolTip toolTip = new ToolTip();
                toolTip.SetToolTip(pnlCost, breakdown);
                toolTip.SetToolTip(lblTotalCost, breakdown);
                toolTip.SetToolTip(lblCostTitle, breakdown);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while calculating the cost: {ex.Message}", "Error");
            }
        }

        private void FilterRecipeList()
        {
            if (cmbTargetMachine.SelectedItem is not Machine selectedMachine || _recipeList == null)
            {
                lstRecipes.DataSource = null;
                return;
            }

            string filterType = !string.IsNullOrEmpty(selectedMachine.MachineSubType)
                                   ? selectedMachine.MachineSubType
                                   : selectedMachine.MachineType;

            var compatibleRecipes = _recipeList
                .Where(r => r.TargetMachineType == filterType)
                .ToList();

            lstRecipes.SelectedIndexChanged -= LstRecipes_SelectedIndexChanged;
            lstRecipes.DataSource = null;
            lstRecipes.DataSource = compatibleRecipes;
            lstRecipes.DisplayMember = "RecipeName";
            lstRecipes.ValueMember = "Id";
            lstRecipes.SelectedIndexChanged += LstRecipes_SelectedIndexChanged;

            if (lstRecipes.Items.Count == 0 || lstRecipes.SelectedIndex == -1)
            {
                _currentRecipe = null;
                DisplayCurrentRecipe();
            }
        }

        private void yenile_Click(object sender, EventArgs e)
        {
            LoadRecipeList();
        }
        #endregion
    }
}