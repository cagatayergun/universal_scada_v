using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Telemetry.Core;
using Telemetry.Models;
using Telemetry.Repositories;
using Telemetry.Services;
using Telemetry.UI.Controls;
using Telemetry.UI.Controls.RecipeStepEditors;
using Telemetry.UI.Views;
using Telemetry.Core.Models;
using MaterialSkin;          // YENİ EKLENDİ: MaterialSkin ana kütüphanesi
using MaterialSkin.Controls; // YENİ EKLENDİ: MaterialForm ve bileşenler için

namespace Telemetry.UI
{
    // Form yerine MaterialForm'dan türetiyoruz
    public partial class FtpSync_Form : MaterialForm
    {
        private readonly MachineRepository _machineRepository;
        private readonly RecipeRepository _recipeRepository;
        private readonly FtpTransferService _transferService;
        private readonly UserRepository _userRepository;

        // Ön izleme editörü için gerekli değişkenler
        private SplitContainer _byMakinesiEditor;
        private DataGridView dgvRecipeSteps;
        private Panel pnlStepDetails;
        private ScadaRecipe _previewRecipe;
        private readonly string _targetMachineType;
        private readonly PlcPollingService _plcPollingService;

        public FtpSync_Form(
            MachineRepository machineRepo,
            RecipeRepository recipeRepo,
            PlcPollingService plcPollingService,
            string targetMachineType,
            FtpTransferService transferService,
            UserRepository userRepo)
        {
            InitializeComponent();
            _machineRepository = machineRepo;
            _recipeRepository = recipeRepo;
            _targetMachineType = targetMachineType;
            _transferService = transferService;
            _transferService.SetSyncContext(SynchronizationContext.Current);
            _plcPollingService = plcPollingService;
            _userRepository = userRepo;

            // =========================================================================
            // MATERIALSKIN FORM KAYDI VE PÜRÜZSÜZLEŞTİRME
            // =========================================================================
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this); // Bu formu temalandırma motoruna bağla

            this.DoubleBuffered = true; // Formun genel çizim kırpışmalarını engelle
        }

        private void FtpSync_Form_Load(object sender, EventArgs e)
        {
            LoadMachines();
            LoadLocalRecipes();
            SetupTransfersGrid();
            dgvTransfers.DataSource = _transferService.Jobs;
            _transferService.Jobs.ListChanged += Jobs_ListChanged;
        }

        private void LoadMachines()
        {
            var machineCache = _plcPollingService.MachineDataCache;

            var machines = _machineRepository.GetAllEnabledMachines()
                .Where(m =>
                    !string.IsNullOrEmpty(m.FtpUsername) &&
                    m.MachineType != "Kurutma Makinesi" &&
                    (!string.IsNullOrEmpty(m.MachineSubType) ? m.MachineSubType : m.MachineType) == _targetMachineType &&
                    machineCache.TryGetValue(m.Id, out FullMachineStatus status) && status.ConnectionState == ConnectionStatus.Connected
                )
                .ToList();

            ((ListBox)clbMachines).DataSource = machines;
            ((ListBox)clbMachines).DisplayMember = "DisplayInfo";
            ((ListBox)clbMachines).ValueMember = "Id";
        }

        private void LoadLocalRecipes()
        {
            if (lstLocalRecipes.Items.Count > 0)
            {
                lstLocalRecipes.SelectedIndex = -1;
                lstLocalRecipes.ClearSelected();
            }

            lstLocalRecipes.DataSource = _recipeRepository.GetAllRecipes()
                .Where(r => r.TargetMachineType == _targetMachineType)
                .ToList();
            lstLocalRecipes.DisplayMember = "RecipeName";
            lstLocalRecipes.ValueMember = "Id";

            lstLocalRecipes.SelectedIndex = -1;
        }

        private async void LoadHmiRecipes()
        {
            var selectedMachine = clbMachines.CheckedItems.Count == 1
                ? clbMachines.CheckedItems.Cast<Machine>().FirstOrDefault()
                : null;

            if (selectedMachine == null)
            {
                lstHmiRecipes.DataSource = null;
                btnReceive.Enabled = false;
                ClearPreview();
                return;
            }

            btnReceive.Enabled = true;
            btnRefreshHmi.Enabled = false;
            lstHmiRecipes.DataSource = new List<string> { "Prescription names are read from the PLC..." };
            ClearPreview();

            try
            {
                if (!_plcPollingService.GetPlcManagers().TryGetValue(selectedMachine.Id, out var plcManager))
                {
                    throw new Exception("PLC manager for the machine could not be found.");
                }

                var readResult = await plcManager.ReadRecipeNamesFromPlcAsync();
                if (readResult.IsSuccess)
                {
                    var recipeNames = readResult.Content;
                    var displayList = new List<string>();
                    foreach (var kvp in recipeNames)
                    {
                        displayList.Add($"{kvp.Key} - {kvp.Value}");
                    }

                    if (!displayList.Any())
                    {
                        displayList.Add("No prescription name registered in PLC found.");
                    }

                    lstHmiRecipes.DataSource = displayList;
                    lstHmiRecipes.ClearSelected();
                }
                else
                {
                    throw new Exception(readResult.Message);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred: {ex.Message}";
                MessageBox.Show(errorMessage, "General Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lstHmiRecipes.DataSource = null;
            }
            finally
            {
                btnRefreshHmi.Enabled = true;
            }
        }

        private void SetupTransfersGrid()
        {
            dgvTransfers.AutoGenerateColumns = false;
            dgvTransfers.Columns.Clear();
            dgvTransfers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MachineName", HeaderText = "Machine", FillWeight = 150 });
            dgvTransfers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RecipeName", HeaderText = "Recipe/File", FillWeight = 200 });
            dgvTransfers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "OperationType", HeaderText = "Operation" });
            dgvTransfers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Status" });
            dgvTransfers.Columns.Add(new DataGridViewProgressBarColumn { DataPropertyName = "Progress", HeaderText = "Progress" });
            dgvTransfers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ErrorMessage", HeaderText = "Error", FillWeight = 250 });

            // SPEED OPTİMİZASYON: Transfer listesi çok hızlı akarken Grid'in donmasını/titremesini engeller
            EnableDoubleBuffer(dgvTransfers);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedRecipes = lstLocalRecipes.SelectedItems.OfType<ScadaRecipe>().ToList();
                var selectedMachines = clbMachines.CheckedItems.Cast<Machine>().ToList();

                if (!selectedRecipes.Any() || !selectedMachines.Any())
                {
                    MessageBox.Show("Please select at least one recipe and one target machine.", "Warning");
                    return;
                }

                string startNumberStr = ProsesKontrol_Control.ShowInputDialog("Enter the first prescription number to be sent (1-98):", true);
                if (string.IsNullOrEmpty(startNumberStr) || !int.TryParse(startNumberStr, out int startNumber))
                {
                    return;
                }

                if (startNumber + selectedRecipes.Count - 1 > 98)
                {
                    MessageBox.Show($"The {selectedRecipes.Count} number of recipes you selected exceeds the limit of 98 with a starting number of {startNumber}. Please select a lower starting number.", "Error");
                    return;
                }

                _transferService.QueueSequentiallyNamedSendJobs(selectedRecipes, selectedMachines, startNumber);

                if (CurrentUser.User != null && _userRepository != null)
                {
                    string details = $"{selectedRecipes.Count} recipes queued for sending to {selectedMachines.Count} machines. Start #: {startNumber}";
                    _userRepository.LogAction(CurrentUser.User.Id, "FTP_BATCH_SEND", details);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred during submission: {ex.Message}", "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReceive_Click(object sender, EventArgs e)
        {
            var selectedMachines = clbMachines.CheckedItems.Cast<Machine>().ToList();

            if (!selectedMachines.Any() || selectedMachines.Count > 1)
            {
                MessageBox.Show("Please select ONLY ONE source machine from the list.", "Warning");
                return;
            }

            var selectedMachine = selectedMachines.First();
            var selectedIndices = lstHmiRecipes.SelectedIndices;

            if (selectedIndices.Count == 0)
            {
                MessageBox.Show("Please select at least one HMI recipe to download.", "Warning");
                return;
            }

            var filesToReceive = new List<string>();
            foreach (int index in selectedIndices)
            {
                int recipeNumber = index + 1;
                string remoteFileName = $"XPR{recipeNumber:D5}.csv";
                filesToReceive.Add(remoteFileName);
            }

            _transferService.QueueReceiveJobs(filesToReceive, selectedMachine);

            if (CurrentUser.User != null && _userRepository != null)
            {
                string details = $"{filesToReceive.Count} recipes queued for download from machine '{selectedMachine.MachineName}'.";
                _userRepository.LogAction(CurrentUser.User.Id, "FTP_BATCH_RECEIVE", details);
            }
        }

        private void btnRefreshHmi_Click(object sender, EventArgs e)
        {
            LoadHmiRecipes();
        }

        private void clbMachines_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate { LoadHmiRecipes(); });
        }

        private void Jobs_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => Jobs_ListChanged(sender, e)));
                return;
            }

            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                var job = _transferService.Jobs[e.NewIndex] as TransferJob;
                if (job != null && job.OperationType == TransferType.Send && job.Status == TransferStatus.Successful)
                {
                    LoadLocalRecipes();
                }
            }

            if (this.IsDisposed || !this.IsHandleCreated) return;

            // OPTİMİZASYON: Kuyruk çok hızlı akarken dgvTransfers.Refresh() arayüzü kilitleyebilir. 
            // Invalidate() kullanarak çizim Windows'un kendi döngüsüne asenkron bırakıldı.
            dgvTransfers.Invalidate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _transferService.Jobs.ListChanged -= Jobs_ListChanged;
            base.OnFormClosing(e);
        }

        #region Ön İzleme Metotları

        private async void lstHmiRecipes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstHmiRecipes.SelectedItems.Count != 1 || lstHmiRecipes.SelectedIndex < 0)
            {
                ClearPreview();
                return;
            }

            var selectedMachine = clbMachines.CheckedItems.Cast<Machine>().FirstOrDefault();
            if (selectedMachine == null)
            {
                ClearPreview();
                return;
            }

            int recipeNumber = lstHmiRecipes.SelectedIndex + 1;
            string remoteFileName = $"XPR{recipeNumber:D5}.csv";

            tabControlMain.SelectedTab = tabPagePreview;

            pnlPreviewArea.SuspendLayout(); // SPEED OPTİMİZASYON: Alt kontroller yüklenirken yerleşim hesaplamasını durdur
            pnlPreviewArea.Controls.Clear();
            lblPreviewStatus.Visible = true;
            lblPreviewStatus.Text = $"'{remoteFileName}' loading...";
            pnlPreviewArea.ResumeLayout(true);

            try
            {
                var ftpService = new FtpService(selectedMachine.VncAddress, selectedMachine.FtpUsername, selectedMachine.FtpPassword);
                string csvContent = await ftpService.DownloadFileAsync($"/{remoteFileName}");

                string previewName = lstHmiRecipes.SelectedItem.ToString();
                _previewRecipe = RecipeCsvConverter.ToRecipe(csvContent, previewName);

                pnlPreviewArea.SuspendLayout();
                lblPreviewStatus.Visible = false;
                InitializeBYMakinesiEditor(previewName);
                PopulateStepsGridView();
                pnlPreviewArea.Controls.Add(_byMakinesiEditor);
                pnlPreviewArea.ResumeLayout(true); // Toplu olarak ekrana bas
            }
            catch (Exception ex)
            {
                lblPreviewStatus.Text = $"Preview failed to load: {ex.Message}";
            }
        }

        private void ClearPreview()
        {
            pnlPreviewArea.SuspendLayout();
            pnlPreviewArea.Controls.Clear();
            pnlPreviewArea.Controls.Add(lblPreviewStatus);
            lblPreviewStatus.Visible = true;
            lblPreviewStatus.Text = "Select a prescription from the HMI list for preview.";
            _previewRecipe = null;
            pnlPreviewArea.ResumeLayout(true);
        }

        private void InitializeBYMakinesiEditor(string recipeName)
        {
            _byMakinesiEditor = new SplitContainer();
            dgvRecipeSteps = new DataGridView();
            pnlStepDetails = new Panel();

            var pnlTopBar = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.FromArgb(38, 50, 56) }; // Material Koyu Arka Plan
            var lblRecipeName = new Label { Dock = DockStyle.Fill, Text = recipeName, Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
            pnlTopBar.Controls.Add(lblRecipeName);

            _byMakinesiEditor.SuspendLayout();
            _byMakinesiEditor.Dock = DockStyle.Fill;
            _byMakinesiEditor.SplitterDistance = 450;
            _byMakinesiEditor.Panel1.Controls.Add(dgvRecipeSteps);
            _byMakinesiEditor.Panel2.Controls.Add(pnlTopBar);
            _byMakinesiEditor.Panel2.Controls.Add(pnlStepDetails);

            dgvRecipeSteps.Dock = DockStyle.Fill;
            dgvRecipeSteps.AllowUserToAddRows = false;
            dgvRecipeSteps.AllowUserToDeleteRows = false;
            dgvRecipeSteps.ReadOnly = true;
            dgvRecipeSteps.MultiSelect = false;
            dgvRecipeSteps.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecipeSteps.CellClick += dgvRecipeSteps_CellClick;

            // Önizleme adımları gridine pürüzsüz kaydırma ivmesi ver
            EnableDoubleBuffer(dgvRecipeSteps);

            pnlStepDetails.Dock = DockStyle.Fill;
            pnlStepDetails.AutoScroll = true;
            pnlStepDetails.BorderStyle = BorderStyle.None;

            SetupStepsGridView();
            _byMakinesiEditor.ResumeLayout(true);
        }

        private void SetupStepsGridView()
        {
            if (dgvRecipeSteps == null) return;
            dgvRecipeSteps.DataSource = null;
            dgvRecipeSteps.Rows.Clear();
            dgvRecipeSteps.Columns.Clear();
            dgvRecipeSteps.AutoGenerateColumns = false;

            dgvRecipeSteps.Columns.Add(new DataGridViewTextBoxColumn { Name = "StepNumber", HeaderText = "Step No", DataPropertyName = "StepNumber", Width = 70 });
            dgvRecipeSteps.Columns.Add(new DataGridViewTextBoxColumn { Name = "StepType", HeaderText = "Step Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        }

        private void PopulateStepsGridView()
        {
            if (_previewRecipe == null || _previewRecipe.Steps == null || dgvRecipeSteps == null) return;
            dgvRecipeSteps.Rows.Clear();
            foreach (var step in _previewRecipe.Steps)
            {
                string stepTypeName = GetStepTypeName(step);
                dgvRecipeSteps.Rows.Add(step.StepNumber, stepTypeName);
            }
        }

        private string GetStepTypeName(ScadaRecipeStep step)
        {
            var stepTypes = new List<string>();
            if (step.StepDataWords.Length > 24)
            {
                short controlWord = step.StepDataWords[24];
                if ((controlWord & 1) != 0) stepTypes.Add("Take Water");
                if ((controlWord & 2) != 0) stepTypes.Add("Heating");
                if ((controlWord & 4) != 0) stepTypes.Add("Working");
                if ((controlWord & 8) != 0) stepTypes.Add("Dosing");
                if ((controlWord & 16) != 0) stepTypes.Add("Drain");
                if ((controlWord & 32) != 0) stepTypes.Add("Extraction");
                if ((controlWord & 1024) != 0) stepTypes.Add("Operator Call");
            }
            return stepTypes.Any() ? string.Join(" + ", stepTypes) : "-";
        }

        private void dgvRecipeSteps_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _previewRecipe == null || pnlStepDetails == null) return;

            var selectedMachine = clbMachines.CheckedItems.Cast<Machine>().FirstOrDefault();
            if (selectedMachine == null) return;

            pnlStepDetails.SuspendLayout();
            pnlStepDetails.Controls.Clear();

            int selectedIndex = e.RowIndex;
            if (selectedIndex < _previewRecipe.Steps.Count)
            {
                var selectedStep = _previewRecipe.Steps[selectedIndex];
                var mainEditor = new StepEditor_Control();
                mainEditor.LoadStep(selectedStep, selectedMachine);
                mainEditor.SetReadOnly(true);
                mainEditor.Dock = DockStyle.Top;
                mainEditor.AutoSize = true;
                pnlStepDetails.Controls.Add(mainEditor);
            }
            pnlStepDetails.ResumeLayout(true);
        }

        #endregion

        // SPEED OPTİMİZASYON: Grid verileri yenilenirken donanım ivmesini açan yansıtma (reflection) metodu
        private void EnableDoubleBuffer(Control control)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, control, new object[] { true });
        }

        // DataGridView için özel ProgressBar kolonu
        public class DataGridViewProgressBarColumn : DataGridViewTextBoxColumn
        {
            public DataGridViewProgressBarColumn()
            {
                this.CellTemplate = new DataGridViewProgressBarCell();
            }
        }

        public class DataGridViewProgressBarCell : DataGridViewTextBoxCell
        {
            protected override void Paint(Graphics g, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
            {
                base.Paint(g, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts & ~DataGridViewPaintParts.ContentForeground);

                int progressVal = (value == null) ? 0 : (int)value;
                float percentage = ((float)progressVal / 100.0f);

                if (percentage > 0.0)
                {
                    // Material Skin uyumlu yumuşak yeşil renk tonu
                    Brush progressBarBrush = new SolidBrush(Color.FromArgb(76, 175, 80));
                    g.FillRectangle(progressBarBrush, cellBounds.X + 2, cellBounds.Y + 2, Convert.ToInt32((percentage * cellBounds.Width - 4)), cellBounds.Height - 4);
                    progressBarBrush.Dispose();
                }

                string text = progressVal.ToString() + "%";
                SizeF textSize = g.MeasureString(text, cellStyle.Font);
                float textX = cellBounds.X + (cellBounds.Width - textSize.Width) / 2;
                float textY = cellBounds.Y + (cellBounds.Height - textSize.Height) / 2;

                // Arka plan rengine göre okunabilir metin rengi (Dark mode'da beyaz/açık gri)
                g.DrawString(text, cellStyle.Font, Brushes.White, textX, textY);
            }
        }

        // --- Reçete Silme (Temizleme) Butonu Olayı ---
        private void btnDeleteRecipes_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedMachines = clbMachines.CheckedItems.Cast<Machine>().ToList();
                if (!selectedMachines.Any())
                {
                    MessageBox.Show("Please select at least one machine where prescriptions can be erased.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string startNumberStr = ProsesKontrol_Control.ShowInputDialog("Enter the first prescription number to be deleted (1-98):", true);
                if (string.IsNullOrEmpty(startNumberStr) || !int.TryParse(startNumberStr, out int startNumber))
                {
                    return;
                }

                string countStr = ProsesKontrol_Control.ShowInputDialog("How many prescriptions should be deleted?", true);
                if (string.IsNullOrEmpty(countStr) || !int.TryParse(countStr, out int count))
                {
                    return;
                }

                if (startNumber + count - 1 > 98)
                {
                    MessageBox.Show($"Of your choice {count} pcs recipe, {startNumber} The starting number exceeds the limit of 98. Please enter fewer quantities or a lower starting number.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var confirmResult = MessageBox.Show(
                    $"{selectedMachines.Count} machines, {startNumber} starting from number {count} prescription will be deleted.\nThis action is irreversible!\nDo you want to continue?",
                    "Critical Process Approval",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirmResult != DialogResult.Yes) return;

                var emptyRecipesList = new List<ScadaRecipe>();

                for (int i = 0; i < count; i++)
                {
                    var emptyRecipe = CreateEmptyRecipeForMachineType(_targetMachineType);
                    emptyRecipe.RecipeName = "-";
                    emptyRecipesList.Add(emptyRecipe);
                }

                _transferService.QueueSequentiallyNamedSendJobs(emptyRecipesList, selectedMachines, startNumber);

                MessageBox.Show($"{count} The number of prescription deletions has been added to the transfer queue.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (CurrentUser.User != null && _userRepository != null)
                {
                    string details = $"{count} recipes cleared (overwritten with empty) on {selectedMachines.Count} machines. Start #: {startNumber}";
                    _userRepository.LogAction(CurrentUser.User.Id, "FTP_BATCH_DELETE", details);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during the deletion process: {ex.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private ScadaRecipe CreateEmptyRecipeForMachineType(string machineType)
        {
            var recipe = new ScadaRecipe
            {
                TargetMachineType = machineType,
                RecipeName = "",
                CreationDate = DateTime.Now,
                Steps = new List<ScadaRecipeStep>()
            };

            for (int i = 1; i <= 99; i++)
            {
                var emptyStep = new ScadaRecipeStep
                {
                    StepNumber = (short)i,
                    StepDataWords = new short[25]
                };
                recipe.Steps.Add(emptyStep);
            }

            return recipe;
        }
    }
}