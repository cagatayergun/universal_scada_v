// UI/Views/ProsesKontrol_Control.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telemetry.Core;
using Telemetry.Models;
using Telemetry.Properties;
using Telemetry.Repositories;
using Telemetry.Services;
using Telemetry.UI.Controls;
using Telemetry.UI.Controls.RecipeStepEditors;
using Telemetry.UIViews;
using MaterialSkin;          // YENİ EKLENDİ
using MaterialSkin.Controls; // YENİ EKLENDİ

// Ad alanı çakışmalarını önlemek adına tipleri sisteme mühürlüyoruz
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using FontStyle = System.Drawing.FontStyle;

namespace Telemetry.UI.Views
{
    public partial class ProsesKontrol_Control : UserControl
    {
        private RecipeRepository _recipeRepository;
        private MachineRepository _machineRepository;
        private Dictionary<int, IPlcManager> _plcManagers;
        private UserRepository _userRepository;
        private List<ScadaRecipe> _recipeList;
        private ScadaRecipe _currentRecipe;

        // BYMakinesi editör bileşenleri
        private SplitContainer _byMakinesiEditor;
        private DataGridView dgvRecipeSteps;
        private Panel pnlStepDetails;
        private Label lblStepDetailsTitle;
        private CostRepository _costRepository;
        private FtpSync_Form _ftpFormInstance;
        private PlcPollingService _plcPollingService;
        private FtpTransferService _ftpTransferService;
        private short[] _copiedStepData = null;
        private List<ScadaRecipe> _copiedRecipes = new List<ScadaRecipe>();

        public ProsesKontrol_Control()
        {
            InitializeComponent();
            _costRepository = new CostRepository();

            // SPEED OPTİMİZASYON: Sekme ve editör geçişlerindeki kırpışmaları önler
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;

            // Olay abonelikleri zinciri
            btnNewRecipe.Click += BtnNewRecipe_Click;
            btnDeleteRecipe.Click += BtnDeleteRecipe_Click;
            btnSaveRecipe.Click += BtnSaveRecipe_Click;
            btnSendToPlc.Click += BtnSendToPlc_Click;
            btnReadFromPlc.Click += BtnReadFromPlc_Click;
            lstRecipes.SelectedIndexChanged += LstRecipes_SelectedIndexChanged;
            cmbTargetMachine.SelectedIndexChanged += CmbTargetMachine_SelectedIndexChanged;
            btnFtpSync.Click += BtnFtpSync_Click;
            lstRecipes.KeyDown += LstRecipes_KeyDown;

            // DÜZELTME: Mükerrer olan double Load olay aboneliği kaldırıldı.
            this.Load += ProsesKontrol_Control_Load;

            // =========================================================================
            // MODERNİZASYON: STANDART GİRİŞ VE LİSTE KUTULARINI DARK MODE UYARLAMA ADAPTÖRÜ
            // Standart kutuları mat koyu arka plan şemasıyla pürüzsüzce eşitler.
            // =========================================================================
            Color controlBg = Color.FromArgb(44, 52, 64);
            Color controlFg = Color.FromArgb(240, 240, 240);

            if (txtRecipeName != null) { txtRecipeName.BackColor = controlBg; txtRecipeName.ForeColor = controlFg; }
            if (txtSearchRecipe != null) { txtSearchRecipe.BackColor = controlBg; txtSearchRecipe.ForeColor = controlFg; }
            if (lstRecipes != null) { lstRecipes.BackColor = controlBg; lstRecipes.ForeColor = controlFg; }
            if (lstRecipeHistory != null) { lstRecipeHistory.BackColor = controlBg; lstRecipeHistory.ForeColor = controlFg; }
            if (cmbTargetMachine != null) { cmbTargetMachine.BackColor = controlBg; cmbTargetMachine.ForeColor = controlFg; }
        }

        public void InitializeControl(RecipeRepository recipeRepo, MachineRepository machineRepo, Dictionary<int, IPlcManager> plcManagers, PlcPollingService plcPollingService, FtpTransferService ftpTransferService, UserRepository userRepo)
        {
            _recipeRepository = recipeRepo;
            _machineRepository = machineRepo;
            _plcManagers = plcManagers;
            _plcPollingService = plcPollingService;
            _ftpTransferService = ftpTransferService;
            _userRepository = userRepo;
        }

        private void ProsesKontrol_Control_Load(object sender, EventArgs e)
        {
            LoadRecipeList();
            LoadMachineList();
            ApplyRolePermissions();
            ApplyPermissions();

            // Singleton event kaydı (OnHandleDestroyed içinde kesinlikle çıkarılmalıdır)
            FtpTransferService.Instance.RecipeListChanged += OnRecipeListChanged;
        }

        private void ApplyPermissions()
        {
            btnDeleteRecipe.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });
            btnFtpSync.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });
            btnNewRecipe.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });
            btnReadFromPlc.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });
            btnSaveRecipe.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });
            btnSendToPlc.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });

            if (PermissionService.HasAnyPermission(new List<int> { 1000 }))
            {
                btnDeleteRecipe.Enabled = true;
                btnFtpSync.Enabled = true;
                btnNewRecipe.Enabled = true;
                btnReadFromPlc.Enabled = true;
                btnSaveRecipe.Enabled = true;
                btnSendToPlc.Enabled = true;
            }
        }

        private void ApplyRolePermissions() { }

        private void OnRecipeListChanged(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.SafeInvoke(() => LoadRecipeList());
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
            if (cmbTargetMachine.SelectedItem is not Machine selectedMachine)
            {
                MessageBox.Show("Please select the target machine from the list first.", "Warning");
                return;
            }

            string selectedType = !string.IsNullOrEmpty(selectedMachine.MachineSubType)
                                  ? selectedMachine.MachineSubType
                                  : selectedMachine.MachineType;

            _currentRecipe = new ScadaRecipe
            {
                RecipeName = "NEW RECIPE",
                TargetMachineType = selectedType
            };

            int stepCount = (selectedType == "Kurutma Makinesi") ? 1 : 98;

            _currentRecipe.Steps.Clear();
            for (int i = 1; i <= stepCount; i++)
            {
                var newStep = new ScadaRecipeStep { StepNumber = i };
                newStep.StepDataWords = new short[25];
                _currentRecipe.Steps.Add(newStep);
            }

            lstRecipes.ClearSelected();
            DisplayCurrentRecipe();

            txtRecipeName.Focus();
            txtRecipeName.SelectAll();
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

            if (cmbTargetMachine.SelectedItem is Machine selectedMachine)
            {
                bool isByMachine = selectedMachine.MachineType == "BYMakinesi";

                btnReadFromPlc.Visible = !isByMachine;
                btnSendToPlc.Visible = !isByMachine;

                if (_currentRecipe != null)
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
            }
            else
            {
                _currentRecipe = null;
                DisplayCurrentRecipe();
                btnReadFromPlc.Visible = true;
                btnSendToPlc.Visible = true;
            }
        }

        // =========================================================================
        // MODERNİZASYON: DİNAMİK DIALOG KUTULARININ TEMA ADAPTASYONU
        // Pop-up pencereler koyu modda operatörü kör etmeyecek şekilde matlaştırıldı.
        // =========================================================================
        private string ShowFtpRecipeNumberDialog()
        {
            bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;
            Color formBg = isDark ? Color.FromArgb(44, 52, 64) : System.Drawing.SystemColors.Control;
            Color textFg = isDark ? Color.FromArgb(230, 230, 230) : Color.Black;

            Form prompt = new Form()
            {
                Width = 400,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "HMI Prescription Number",
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = formBg
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = "Enter the recipe number to be saved in the HMI (1-99):", Width = 300, ForeColor = textFg };
            NumericUpDown inputBox = new NumericUpDown() { Left = 50, Top = 50, Width = 300, Minimum = 1, Maximum = 99, BackColor = formBg, ForeColor = textFg };
            Button confirmation = new Button() { Text = "Ok", Left = 250, Width = 100, Top = 90, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, ForeColor = textFg };

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
                LoadRecipeHistory(_currentRecipe.Id);
            }
            else
            {
                txtRecipeName.Text = "";
                pnlEditorArea.Controls.Clear();
                if (lstRecipeHistory != null) lstRecipeHistory.Items.Clear();
            }
        }

        private void LoadRecipeHistory(int recipeId)
        {
            if (lstRecipeHistory == null) return;
            lstRecipeHistory.Items.Clear();

            if (_userRepository == null)
            {
                lstRecipeHistory.Items.Add("Log service is disabled.");
                return;
            }

            if (recipeId <= 0)
            {
                lstRecipeHistory.Items.Add("The new prescription has no prior record.");
                return;
            }

            try
            {
                string searchTag = $"[rcp-{recipeId}]";
                var logs = _userRepository.GetActionLogs(null, null, null, searchTag);

                var recentLogs = logs
                    .OrderByDescending(l => l.Timestamp)
                    .Take(10)
                    .ToList();

                if (recentLogs.Any())
                {
                    foreach (var log in recentLogs)
                    {
                        string displayText = $"{log.Timestamp:dd.MM HH:mm} [{log.Username}] - {GetFriendlyActionName(log.ActionType)}";
                        lstRecipeHistory.Items.Add(displayText);
                    }
                }
                else
                {
                    lstRecipeHistory.Items.Add("No transaction was found for this prescription.");
                }
            }
            catch (Exception ex)
            {
                lstRecipeHistory.Items.Add("The logs could not be loaded.");
                System.Diagnostics.Debug.WriteLine("Log Load Error: " + ex.Message);
            }
        }

        private string GetFriendlyActionName(string actionType)
        {
            return actionType switch
            {
                "RECIPE_CREATE" => "Created",
                "RECIPE_UPDATE" => "Edited",
                "RECIPE_DELETE" => "deleted",
                "RECIPE_SEND_FTP" => "Sent to the machine (FTP)",
                "RECIPE_SEND_PLC" => "Written to the Machine (PLC)",
                "RECIPE_READ_PLC" => "Read from the machine.",
                _ => actionType
            };
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
                editor.Dock = DockStyle.Fill;
                pnlEditorArea.Controls.Add(editor);
            }
            else
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

            // Tablo kaydırma ivmesini uçuran donanımsal çift tamponlama tetiği
            EnableDoubleBuffer(dgvRecipeSteps);

            _byMakinesiEditor.Panel1.Controls.Add(dgvRecipeSteps);
            _byMakinesiEditor.Panel2.Controls.Add(pnlStepDetails);

            dgvRecipeSteps.Dock = DockStyle.Fill;
            dgvRecipeSteps.AllowUserToAddRows = false;
            dgvRecipeSteps.AllowUserToDeleteRows = false;
            dgvRecipeSteps.MultiSelect = false;
            dgvRecipeSteps.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecipeSteps.CellClick += DgvRecipeSteps_CellClick;
            dgvRecipeSteps.CellMouseDown += DgvRecipeSteps_CellMouseDown;

            ContextMenuStrip ctxMenu = new ContextMenuStrip();
            var itemInsert = ctxMenu.Items.Add("Insert Step (Araya Ekle)");
            itemInsert.Click += BtnInsertStep_Click;

            var itemDelete = ctxMenu.Items.Add("Delete Step (Sil)");
            itemDelete.Click += BtnDeleteStep_Click;

            ctxMenu.Items.Add(new ToolStripSeparator());

            var itemCopy = ctxMenu.Items.Add("Copy Step (Kopyala)");
            itemCopy.Click += BtnCopyStep_Click;

            var itemPaste = ctxMenu.Items.Add("Paste Step (Yapıştır)");
            itemPaste.Click += BtnPasteStep_Click;

            dgvRecipeSteps.ContextMenuStrip = ctxMenu;

            pnlStepDetails.Dock = DockStyle.Fill;
            pnlStepDetails.BorderStyle = BorderStyle.FixedSingle;
            pnlStepDetails.Controls.Add(lblStepDetailsTitle);

            lblStepDetailsTitle.Dock = DockStyle.Top;
            lblStepDetailsTitle.Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold);
            lblStepDetailsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblStepDetailsTitle.Text = "Step Details";

            SetupStepsGridView();
        }

        private void DgvRecipeSteps_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                dgvRecipeSteps.ClearSelection();
                dgvRecipeSteps.Rows[e.RowIndex].Selected = true;
                dgvRecipeSteps.CurrentCell = dgvRecipeSteps.Rows[e.RowIndex].Cells[e.ColumnIndex];
            }
        }

        private void BtnInsertStep_Click(object sender, EventArgs e)
        {
            if (dgvRecipeSteps.CurrentRow == null || _currentRecipe == null) return;

            int selectedIndex = dgvRecipeSteps.CurrentRow.Index;
            int totalSteps = _currentRecipe.Steps.Count;

            var result = MessageBox.Show($"Step {selectedIndex + 1} will be inserted. The last step (99) will be lost. Continue?", "Insert Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            for (int i = totalSteps - 1; i > selectedIndex; i--)
            {
                Array.Copy(_currentRecipe.Steps[i - 1].StepDataWords, _currentRecipe.Steps[i].StepDataWords, 25);
            }

            Array.Clear(_currentRecipe.Steps[selectedIndex].StepDataWords, 0, 25);
            PopulateStepsGridView();

            dgvRecipeSteps.Rows[selectedIndex].Selected = true;
            DisplayCurrentRecipe();
        }

        private void BtnDeleteStep_Click(object sender, EventArgs e)
        {
            if (dgvRecipeSteps.CurrentRow == null || _currentRecipe == null) return;

            int selectedIndex = dgvRecipeSteps.CurrentRow.Index;
            int totalSteps = _currentRecipe.Steps.Count;

            var result = MessageBox.Show($"Step {selectedIndex + 1} will be deleted. Steps below will move up. Continue?", "Delete Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;

            for (int i = selectedIndex; i < totalSteps - 1; i++)
            {
                Array.Copy(_currentRecipe.Steps[i + 1].StepDataWords, _currentRecipe.Steps[i].StepDataWords, 25);
            }

            Array.Clear(_currentRecipe.Steps[totalSteps - 1].StepDataWords, 0, 25);
            PopulateStepsGridView();
        }

        private void BtnCopyStep_Click(object sender, EventArgs e)
        {
            if (dgvRecipeSteps.CurrentRow == null || _currentRecipe == null) return;
            int selectedIndex = dgvRecipeSteps.CurrentRow.Index;
            _copiedStepData = new short[25];
            Array.Copy(_currentRecipe.Steps[selectedIndex].StepDataWords, _copiedStepData, 25);
        }

        private void BtnPasteStep_Click(object sender, EventArgs e)
        {
            if (dgvRecipeSteps.CurrentRow == null || _currentRecipe == null) return;

            if (_copiedStepData == null)
            {
                MessageBox.Show("No step data found in clipboard. Please copy a step first.", "Warning");
                return;
            }

            int selectedIndex = dgvRecipeSteps.CurrentRow.Index;
            Array.Copy(_copiedStepData, _currentRecipe.Steps[selectedIndex].StepDataWords, 25);

            string newStepName = GetStepTypeName(_currentRecipe.Steps[selectedIndex]);
            dgvRecipeSteps.Rows[selectedIndex].Cells["StepType"].Value = newStepName;

            DgvRecipeSteps_CellClick(dgvRecipeSteps, new DataGridViewCellEventArgs(0, selectedIndex));
        }

        private void SetupStepsGridView()
        {
            if (dgvRecipeSteps == null) return;
            dgvRecipeSteps.DataSource = null;
            dgvRecipeSteps.Rows.Clear();
            dgvRecipeSteps.Columns.Clear();
            dgvRecipeSteps.AutoGenerateColumns = false;
            dgvRecipeSteps.AllowUserToResizeColumns = false;
            //dgvRecipeSteps.someColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvRecipeSteps.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgvRecipeSteps.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StepNumber",
                HeaderText = "Step No",
                DataPropertyName = "StepNumber",
                Width = 60,
                MinimumWidth = 60
            });

            dgvRecipeSteps.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StepType",
                HeaderText = "Step Type",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        // =========================================================================
        // SPEED OPTİMİZASYON: REÇETE ADIM MATRİS YENİLEME KİLİDİ
        // 98 adım arka arkaya eklenirken düzen motorunun donması (lag) engellenmiştir.
        // =========================================================================
        private void PopulateStepsGridView()
        {
            if (_currentRecipe == null || _currentRecipe.Steps == null || dgvRecipeSteps == null) return;

            dgvRecipeSteps.SuspendLayout(); // Tablo çizim hesaplamalarını dondur
            try
            {
                dgvRecipeSteps.Rows.Clear();
                foreach (var step in _currentRecipe.Steps)
                {
                    string stepTypeName = GetStepTypeName(step);
                    dgvRecipeSteps.Rows.Add(step.StepNumber, stepTypeName);
                }
            }
            finally
            {
                dgvRecipeSteps.ResumeLayout(true);
            }
        }

        private string GetStepTypeName(ScadaRecipeStep step)
        {
            var stepTypes = new List<string>();
            short controlWord = step.StepDataWords[24];
            if ((controlWord & 1) != 0) stepTypes.Add("Water Intake");
            if ((controlWord & 2) != 0) stepTypes.Add("Heating");
            if ((controlWord & 4) != 0) stepTypes.Add("Working");
            if ((controlWord & 8) != 0) stepTypes.Add("Dosage");
            if ((controlWord & 16) != 0) stepTypes.Add("Unloading");
            if ((controlWord & 32) != 0) stepTypes.Add("Squeezing");
            if ((controlWord & 1024) != 0) stepTypes.Add("Operator Call");
            return string.Join(" + ", stepTypes);
        }

        private void dgvRecipeSteps_SelectionChanged(object sender, EventArgs e) { }

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
                        _ftpFormInstance = new FtpSync_Form(_machineRepository, _recipeRepository, _plcPollingService, selectedType, _ftpTransferService, _userRepository);
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

                        if (CurrentUser.User != null && _userRepository != null)
                        {
                            _userRepository.LogAction(
                                CurrentUser.User.Id,
                                "RECIPE_SEND_FTP",
                                $"Recipe '{_currentRecipe.RecipeName}' sent to '{selectedMachine.MachineName}' as '{remoteFileName}' [rcp-{_currentRecipe.Id}]"
                            );
                        }

                        MessageBox.Show($"'Recipe '{_currentRecipe.RecipeName}' was successfully sent to machine '{selectedMachine.MachineName}' with name '{remoteFileName}'.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error sending recipe via FTP: {ex.Message}", "FTP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    if (_plcManagers == null || !_plcManagers.TryGetValue(selectedMachine.Id, out var plcManager))
                    {
                        MessageBox.Show($"'{selectedMachine.MachineName}' No active PLC connection found for .", "Connection Error");
                        return;
                    }

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
                            if (!string.IsNullOrEmpty(input))
                            {
                                MessageBox.Show("You have entered an invalid prescription number.", "Error");
                            }
                            return;
                        }
                    }

                    try
                    {
                        var result = await plcManager.WriteRecipeToPlcAsync(_currentRecipe, recipeSlot);

                        if (result.IsSuccess)
                        {
                            if (CurrentUser.User != null && _userRepository != null)
                            {
                                string slotInfo = recipeSlot.HasValue ? $"(Slot: {recipeSlot})" : "";
                                _userRepository.LogAction(
                                    CurrentUser.User.Id,
                                    "RECIPE_SEND_PLC",
                                    $"Recipe '{_currentRecipe.RecipeName}' written to PLC of '{selectedMachine.MachineName}' {slotInfo} [rcp-{_currentRecipe.Id}]"
                                );
                            }

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

        // =========================================================================
        // MODERNİZASYON: DİNAMİK DIALOG KUTULARININ TEMA ADAPTASYONU
        // =========================================================================
        public static string ShowInputDialog(string text, bool isNumeric = false)
        {
            bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;
            Color formBg = isDark ? Color.FromArgb(44, 52, 64) : System.Drawing.SystemColors.Control;
            Color textFg = isDark ? Color.FromArgb(230, 230, 230) : Color.Black;

            Form prompt = new Form()
            {
                Width = 500,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Login Required",
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = formBg
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text, Width = 400, ForeColor = textFg };
            Control inputBox;

            if (isNumeric)
            {
                inputBox = new NumericUpDown() { Left = 50, Top = 50, Width = 400, Minimum = 1, Maximum = 98, BackColor = formBg, ForeColor = textFg };
            }
            else
            {
                inputBox = new TextBox() { Left = 50, Top = 50, Width = 400, BackColor = formBg, ForeColor = textFg };
            }

            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 90, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, ForeColor = textFg };
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
            if (selectedMachine == null) { MessageBox.Show("Lütfen bir makine seçiniz.", "Uyarı"); return; }

            if (_plcManagers == null || !_plcManagers.TryGetValue(selectedMachine.Id, out var plcManager))
            {
                MessageBox.Show($"'{selectedMachine.MachineName}' için aktif PLC bağlantısı bulunamadı.", "Bağlantı Hatası");
                return;
            }

            if (selectedMachine.MachineType == "Kurutma Makinesi")
            {
                string input = ShowInputDialog("Enter the Prescription Slot Number to be Read (1-20):", true);
                if (string.IsNullOrEmpty(input)) return;

                if (!int.TryParse(input, out int slotNumber) || slotNumber < 1 || slotNumber > 20)
                {
                    MessageBox.Show("Please enter a valid slot number between 1 and 20.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnReadFromPlc.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                try
                {
                    if (plcManager is KurutmaMakinesiManager kurutmaManager)
                    {
                        var result = await kurutmaManager.ReadRecipeSlotAsync(slotNumber, selectedMachine.MachineName);

                        if (result.IsSuccess)
                        {
                            _currentRecipe = result.Content;
                            _currentRecipe.Id = 0;

                            string targetType = !string.IsNullOrEmpty(selectedMachine.MachineSubType)
                                                ? selectedMachine.MachineSubType
                                                : selectedMachine.MachineType;
                            _currentRecipe.TargetMachineType = targetType;

                            DisplayCurrentRecipe();

                            if (CurrentUser.User != null && _userRepository != null)
                            {
                                _userRepository.LogAction(
                                    CurrentUser.User.Id,
                                    "RECIPE_READ_PLC",
                                    $"Recipe read from PLC of '{selectedMachine.MachineName}' [rcp-{_currentRecipe.Id}]"
                                );
                            }

                            MessageBox.Show($"{slotNumber}. Slot başarıyla okundu.\nRecipe Name: {_currentRecipe.RecipeName}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            txtRecipeName.Focus();
                            txtRecipeName.SelectAll();
                        }
                        else
                        {
                            MessageBox.Show($"Okuma Hatası: {result.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Bu makine için uygun okuma yöntemi bulunamadı.", "Hata");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Beklenmedik bir hata oluştu: {ex.Message}", "Sistem Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    btnReadFromPlc.Enabled = true;
                }
                return;
            }

            btnReadFromPlc.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                var result = await plcManager.ReadRecipeFromPlcAsync();
                if (result.IsSuccess)
                {
                    var recipeFromPlc = new ScadaRecipe
                    {
                        Id = 0,
                        RecipeName = $"PLC_{selectedMachine.MachineUserDefinedId}_{DateTime.Now:HHmm}",
                    };

                    string targetType = !string.IsNullOrEmpty(selectedMachine.MachineSubType)
                                        ? selectedMachine.MachineSubType
                                        : selectedMachine.MachineType;

                    recipeFromPlc.TargetMachineType = targetType;

                    for (int i = 0; i < 98; i++)
                    {
                        var step = new ScadaRecipeStep
                        {
                            StepNumber = i + 1,
                            StepDataWords = new short[25]
                        };

                        if (result.Content != null && result.Content.Length >= (i * 25) + 25)
                        {
                            Array.Copy(result.Content, i * 25, step.StepDataWords, 0, 25);
                        }
                        recipeFromPlc.Steps.Add(step);
                    }

                    _currentRecipe = recipeFromPlc;
                    DisplayCurrentRecipe();

                    if (CurrentUser.User != null && _userRepository != null)
                    {
                        _userRepository.LogAction(
                            CurrentUser.User.Id,
                            "RECIPE_READ_PLC",
                            $"Recipe read from PLC of '{selectedMachine.MachineName}' (Active Memory)"
                        );
                    }

                    MessageBox.Show($"Recipe read successfully from '{selectedMachine.MachineName}'.\nPlease rename and save it.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtRecipeName.Focus();
                    txtRecipeName.SelectAll();
                }
                else
                {
                    MessageBox.Show($"Error reading prescription: {result.Message}", "Error");
                }
            }
            catch (NotImplementedException)
            {
                MessageBox.Show($"Reading feature not implemented for '{selectedMachine.MachineType}'.", "Info");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "System Error");
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnReadFromPlc.Enabled = true;
            }
        }

        private void BtnSaveRecipe_Click(object sender, EventArgs e)
        {
            if (_currentRecipe == null) { MessageBox.Show("There is no prescription to save.", "Warning"); return; }
            if (string.IsNullOrWhiteSpace(txtRecipeName.Text)) { MessageBox.Show("Prescription name cannot be empty.", "Warning"); return; }

            _currentRecipe.RecipeName = txtRecipeName.Text;

            try
            {
                bool isNew = _currentRecipe.Id == 0;
                string actionType = isNew ? "RECIPE_CREATE" : "RECIPE_UPDATE";

                _recipeRepository.SaveRecipe(_currentRecipe);

                if (CurrentUser.User != null && _userRepository != null)
                {
                    string details = isNew
                        ? $"New recipe created: {_currentRecipe.RecipeName} ({_currentRecipe.TargetMachineType}) [rcp-{_currentRecipe.Id}]"
                        : $"Recipe updated: {_currentRecipe.RecipeName} [rcp-{_currentRecipe.Id}]";

                    _userRepository.LogAction(CurrentUser.User.Id, actionType, details);
                }

                MessageBox.Show("Recipe successfully saved.", "Success");
                LoadRecipeList();
                DisplayCurrentRecipe();
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

                        if (CurrentUser.User != null && _userRepository != null)
                        {
                            _userRepository.LogAction(
                                CurrentUser.User.Id,
                                "RECIPE_DELETE",
                                $"Recipe deleted: {recipeToDelete.RecipeName} [rcp-{recipeToDelete.Id}]"
                            );
                        }
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

        private void txtSearchRecipe_TextChanged(object sender, EventArgs e)
        {
            FilterRecipeList();
        }

        private void SortOption_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                FilterRecipeList();
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

            // 1. Makine Tipi Filtrelemesi
            var filteredRecipes = _recipeList.Where(r => r.TargetMachineType == filterType);

            // 2. Arama Kutusu Filtrelemesi
            string searchText = txtSearchRecipe.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                filteredRecipes = filteredRecipes.Where(r =>
                    r.RecipeName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // 3. Sıralama Seçenekleri
            if (radioSortName.Checked)
            {
                filteredRecipes = filteredRecipes.OrderBy(r => r.RecipeName);
            }
            else if (radioSortDate.Checked)
            {
                filteredRecipes = filteredRecipes.OrderByDescending(r => r.Id);
            }

            var finalRecipeList = filteredRecipes.ToList();

            // UI Güncelleme (SelectedIndex oynamalarında döngüsel tetiklemeyi önlemek için event sökülür)
            lstRecipes.SelectedIndexChanged -= LstRecipes_SelectedIndexChanged;

            lstRecipes.DataSource = null;
            lstRecipes.DataSource = finalRecipeList;
            lstRecipes.DisplayMember = "RecipeName";
            lstRecipes.ValueMember = "Id";

            lstRecipes.SelectedIndexChanged += LstRecipes_SelectedIndexChanged;

            // Filtreleme sonucu listede eleman kalmadıysa editörü temizle
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

        private void ProsesKontrol_Control_KeyDown(object sender, KeyEventArgs e) { }

        private void LstRecipes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                BtnCopyRecipes_Click(null, null);
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                BtnPasteRecipes_Click(null, null);
                e.Handled = true;
            }
        }

        private void BtnCopyRecipes_Click(object sender, EventArgs e)
        {
            var selectedRecipes = lstRecipes.SelectedItems.Cast<ScadaRecipe>().ToList();

            if (!selectedRecipes.Any())
            {
                MessageBox.Show("Please select the recipes to copy.", "Warning");
                return;
            }

            _copiedRecipes.Clear();

            foreach (var recipe in selectedRecipes)
            {
                var fullRecipe = _recipeRepository.GetRecipeById(recipe.Id);
                _copiedRecipes.Add(fullRecipe);
            }

            MessageBox.Show($"{_copiedRecipes.Count} recipe(s) copied. (Ctrl+V to paste)", "Success");
        }

        private void BtnPasteRecipes_Click(object sender, EventArgs e)
        {
            if (!_copiedRecipes.Any())
            {
                MessageBox.Show("There is no copied recipe to paste. Please copy one or more recipes first (Ctrl+C).", "Warning");
                return;
            }

            int pasteCount = 0;
            try
            {
                foreach (var originalRecipe in _copiedRecipes)
                {
                    var newRecipe = new ScadaRecipe
                    {
                        Id = 0,
                        TargetMachineType = originalRecipe.TargetMachineType,
                        Steps = new List<ScadaRecipeStep>()
                    };

                    string originalName = originalRecipe.RecipeName.StartsWith("Copy_")
                                        ? originalRecipe.RecipeName.Substring(originalRecipe.RecipeName.IndexOf('_', originalRecipe.RecipeName.IndexOf('_') + 1) + 1)
                                        : originalRecipe.RecipeName;

                    int copyIndex = 1;
                    while (_recipeRepository.GetRecipeByName($"Copy_{copyIndex}_{originalName}") != null)
                    {
                        copyIndex++;
                    }

                    newRecipe.RecipeName = $"Copy_{copyIndex}_{originalName}";

                    foreach (var step in originalRecipe.Steps)
                    {
                        var newStep = new ScadaRecipeStep
                        {
                            StepNumber = step.StepNumber,
                            StepDataWords = new short[25]
                        };
                        Array.Copy(step.StepDataWords, newStep.StepDataWords, 25);
                        newRecipe.Steps.Add(newStep);
                    }

                    _recipeRepository.SaveRecipe(newRecipe);
                    pasteCount++;
                }

                MessageBox.Show($"{pasteCount} recipe(s) successfully created and saved.", "Successful");
                LoadRecipeList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while pasting the recipe: {ex.Message}", "Error");
            }
        }

        // =========================================================================
        // KUSURSUZ BELLEK TEMİZLİĞİ: SINGLETON EVENT ABONELİK BAĞLANTISI KOPARILDI
        // Kontrolün RAM'de asılı kalmasını kesin olarak engeller.
        // =========================================================================
        protected override void OnHandleDestroyed(EventArgs e)
        {
            FtpTransferService.Instance.RecipeListChanged -= OnRecipeListChanged;
            base.OnHandleDestroyed(e);
        }

        private void SafeInvoke(Action action)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                try { this.BeginInvoke(action); } catch (Exception) { }
            }
        }

        // SPEED OPTİMİZASYON: DataGrid akıcılığını sağlayan yansıtma (Reflection) metodu
        private void EnableDoubleBuffer(Control control)
        {
            try
            {
                typeof(Control).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.SetProperty |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic,
                    null, control, new object[] { true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DoubleBuffering could not be enabled: {ex.Message}");
            }
        }
    }
}