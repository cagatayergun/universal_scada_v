// UI/Views/ProsesKontrol_Control.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TekstilScada.Core;
using TekstilScada.Models;
using TekstilScada.Repositories;
using TekstilScada.Services;
using TekstilScada.UI.Controls;
using TekstilScada.UI.Controls.RecipeStepEditors;
using TekstilScada.UIViews;

namespace TekstilScada.UI.Views
{
    public partial class ProsesKontrol_Control : UserControl
    {
        private RecipeRepository _recipeRepository;
        private MachineRepository _machineRepository;
        private Dictionary<int, IPlcManager> _plcManagers;

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
        public ProsesKontrol_Control()
        {
            InitializeComponent();
            _costRepository = new CostRepository(); // YENİ
            this.Load += ProsesKontrol_Control_Load;
            btnNewRecipe.Click += BtnNewRecipe_Click;
            btnDeleteRecipe.Click += BtnDeleteRecipe_Click;
            btnSaveRecipe.Click += BtnSaveRecipe_Click;
            btnSendToPlc.Click += BtnSendToPlc_Click;
            btnReadFromPlc.Click += BtnReadFromPlc_Click;
            lstRecipes.SelectedIndexChanged += LstRecipes_SelectedIndexChanged;
            cmbTargetMachine.SelectedIndexChanged += CmbTargetMachine_SelectedIndexChanged;
            btnFtpSync.Click += BtnFtpSync_Click;
            this.Load += ProsesKontrol_Control_Load;
            // --- YENİ EKLENEN KLAVYE OLAY YÖNETİCİSİ ---
            lstRecipes.KeyDown += LstRecipes_KeyDown;
        }

        public void InitializeControl(RecipeRepository recipeRepo, MachineRepository machineRepo, Dictionary<int, IPlcManager> plcManagers, PlcPollingService plcPollingService, FtpTransferService ftpTransferService)
        {
            _recipeRepository = recipeRepo;
            _machineRepository = machineRepo;
            _plcManagers = plcManagers;
            _plcPollingService = plcPollingService;
            _ftpTransferService = ftpTransferService; // YENİ: Alanı atayın
        }

        private void ProsesKontrol_Control_Load(object sender, EventArgs e)
        {
            LoadRecipeList();
            LoadMachineList();
            ApplyRolePermissions(); // YENİ: Yetki kontrolünü çağır
            ApplyPermissions(); // YENİ: Bu ekran için yetkileri uygula
            FtpTransferService.Instance.RecipeListChanged += OnRecipeListChanged;
        }

        private void ApplyPermissions()
        {
            // Reçete kaydetme yetkisi kontrolü
            // btnSaveRecipe.Enabled = PermissionService.CanEditRecipes;
            btnDeleteRecipe.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });
            btnFtpSync.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });
            btnNewRecipe.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });
            btnReadFromPlc.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });
            btnSaveRecipe.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });
            btnSendToPlc.Enabled = PermissionService.HasAnyPermission(new List<int> { 5 });

            var master = PermissionService.HasAnyPermission(new List<int> { 1000 });
            if (master == true)
            {

                btnDeleteRecipe.Enabled = PermissionService.HasAnyPermission(new List<int> { 1000 });
                btnFtpSync.Enabled = PermissionService.HasAnyPermission(new List<int> { 1000 });
                btnNewRecipe.Enabled = PermissionService.HasAnyPermission(new List<int> { 1000 });
                btnReadFromPlc.Enabled = PermissionService.HasAnyPermission(new List<int> { 1000 });
                btnSaveRecipe.Enabled = PermissionService.HasAnyPermission(new List<int> { 1000 });
                btnSendToPlc.Enabled = PermissionService.HasAnyPermission(new List<int> { 1000 });

            }
            // PLC'ye gönderme yetkisi kontrolü
            //  btnSendToPlc.Enabled = PermissionService.CanTransferToPlc;
            //  btnReadFromPlc.Enabled = PermissionService.CanTransferToPlc;
            //  btnFtpSync.Enabled = PermissionService.CanTransferToPlc;

            // Reçete Adı metin kutusunu sadece yetkisi olanlar düzenleyebilir
            //  txtRecipeName.ReadOnly = !PermissionService.CanEditRecipes;
        }
        private void ApplyRolePermissions()
        {
            // Sadece Admin ve Muhendis (Mühendis) rolleri kaydedebilir.
            //  btnSaveRecipe.Enabled = CurrentUser.HasRole("Admin") || CurrentUser.HasRole("Muhendis");


        }
        // YENİ EKLENEN METOT: Sinyal geldiğinde bu metot çalışacak
        private void OnRecipeListChanged(object sender, EventArgs e)
        {
            // Farklı bir thread'den gelebileceği için Invoke kullanarak UI'ı güvenli şekilde güncelle
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
                _recipeList = _recipeRepository.GetAllRecipes(); // Tüm reçeteleri al
                FilterRecipeList(); // Ve seçili makineye göre filtrele

                if (selectedId != -1)
                {
                    // Eğer önceden seçili bir reçete varsa ve hala listedeyse, onu tekrar seç
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
            // Sistemdeki mevcut ve aktif olan makine tiplerini/alt tiplerini al
            var machineTypes = _machineRepository.GetAllEnabledMachines()
                .Select(m => !string.IsNullOrEmpty(m.MachineSubType) ? m.MachineSubType : m.MachineType)
                .Distinct()
                .ToList();

            if (!machineTypes.Any())
            {
                MessageBox.Show("No active machine type was found in the system for which a recipe could be created.", "Warning");
                return;
            }

            // Yeni formu oluşturup kullanıcıdan tip seçmesini iste
            using (var typeForm = new RecipeTypeSelection_Form(machineTypes))
            {
                if (typeForm.ShowDialog() == DialogResult.OK)
                {
                    string selectedType = typeForm.SelectedType;
                    if (string.IsNullOrEmpty(selectedType)) return;

                    _currentRecipe = new ScadaRecipe
                    {
                        RecipeName = "NEW RECIPE",
                        TargetMachineType = selectedType // Seçilen tipi yeni reçeteye ata
                    };

                    // Seçilen tipe göre adım sayısını belirle
                    int stepCount = (selectedType == "Kurutma Makinesi") ? 1 : 98;

                    _currentRecipe.Steps.Clear();
                    for (int i = 1; i <= stepCount; i++)
                    {
                        _currentRecipe.Steps.Add(new ScadaRecipeStep { StepNumber = i });
                    }

                    // Seçili reçeteyi temizle ve editörü yeni reçete ile doldur
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
            // 1. Önce seçime göre reçete listesini filtrele (Mevcut Kod)
            FilterRecipeList();

            // 2. Makine seçimi kontrolü ve Buton Gizleme/Gösterme Mantığı (YENİ EKLENEN KISIM)
            if (cmbTargetMachine.SelectedItem is Machine selectedMachine)
            {
                // Eğer makine tipi "BYMakinesi" ise butonları gizle, değilse göster.
                bool isByMachine = selectedMachine.MachineType == "BYMakinesi";

                btnReadFromPlc.Visible = !isByMachine; // BYMakinesi ise false olur (gizlenir)
                btnSendToPlc.Visible = !isByMachine;   // BYMakinesi ise false olur (gizlenir)

                // 3. Reçete uyumluluk kontrolü (Mevcut Kod)
                if (_currentRecipe != null)
                {
                    string machineTypeForRecipe = !string.IsNullOrEmpty(selectedMachine.MachineSubType)
                                                  ? selectedMachine.MachineSubType
                                                  : selectedMachine.MachineType;

                    // Mevcut reçete, yeni seçilen makine tipiyle uyumlu değilse...
                    if (_currentRecipe.TargetMachineType != machineTypeForRecipe)
                    {
                        _currentRecipe = null; // Aktif reçeteyi temizle
                        lstRecipes.ClearSelected(); // Listeden seçimi kaldır
                        DisplayCurrentRecipe(); // Editör panelini temizle
                    }
                }
            }
            else
            {
                // Eğer makine seçimi boşsa veya listede hiç reçete kalmadıysa editörü temizle (Mevcut Kod)
                _currentRecipe = null;
                DisplayCurrentRecipe();

                // Makine seçili değilse butonların varsayılan durumu (isteğe bağlı, genelde açık kalabilir veya kapanabilir)
                btnReadFromPlc.Visible = true;
                btnSendToPlc.Visible = true;
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
            // --- YENİ EKLENEN OLAY YÖNETİCİSİ BURAYA GELİYOR ---
            dgvRecipeSteps.CellMouseDown += DgvRecipeSteps_CellMouseDown; // <--- BU SATIRI EKLEYİN
                                                                          // -----------------------------------------------------
                                                                          // --- YENİ EKLENEN KISIM: SAĞ TIK MENÜSÜ ---
            ContextMenuStrip ctxMenu = new ContextMenuStrip();

            var itemInsert = ctxMenu.Items.Add("Insert Step (Araya Ekle)");
            itemInsert.Click += BtnInsertStep_Click;
            // İkon eklemek isterseniz: itemInsert.Image = Properties.Resources.AddIcon;

            var itemDelete = ctxMenu.Items.Add("Delete Step (Sil)");
            itemDelete.Click += BtnDeleteStep_Click;

            ctxMenu.Items.Add(new ToolStripSeparator()); // Çizgi çek

            var itemCopy = ctxMenu.Items.Add("Copy Step (Kopyala)");
            itemCopy.Click += BtnCopyStep_Click;

            var itemPaste = ctxMenu.Items.Add("Paste Step (Yapıştır)");
            itemPaste.Click += BtnPasteStep_Click;

            // Menüyü tabloya bağla
            dgvRecipeSteps.ContextMenuStrip = ctxMenu;
            // -------------------------------------------
            pnlStepDetails.Dock = DockStyle.Fill;
            pnlStepDetails.BorderStyle = BorderStyle.FixedSingle;
            pnlStepDetails.Controls.Add(lblStepDetailsTitle);

            lblStepDetailsTitle.Dock = DockStyle.Top;
            lblStepDetailsTitle.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            lblStepDetailsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblStepDetailsTitle.Text = "Step Details";

            SetupStepsGridView();
        }
        private void DgvRecipeSteps_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Sadece sağ tıklama için (e.Button == MouseButtons.Right) ve geçerli bir satır için kontrol yapıyoruz.
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                // 1. Sağ tıklanan satırı seç
                dgvRecipeSteps.ClearSelection();
                dgvRecipeSteps.Rows[e.RowIndex].Selected = true;

                // 2. DataGridView'in CurrentCell özelliğini tıklanan hücreye ayarla (opsiyonel ama önerilir)
                dgvRecipeSteps.CurrentCell = dgvRecipeSteps.Rows[e.RowIndex].Cells[e.ColumnIndex];

                // 3. ContextMenuStrip'i aç (DataGridView'e bağlı olduğu için otomatik olarak açılacaktır).
                // Ancak konum kontrolü veya ek özelleştirme gerekirse:
                // dgvRecipeSteps.ContextMenuStrip.Show(dgvRecipeSteps, e.Location);
            }
        }
        private void BtnInsertStep_Click(object sender, EventArgs e)
        {
            if (dgvRecipeSteps.CurrentRow == null || _currentRecipe == null) return;

            int selectedIndex = dgvRecipeSteps.CurrentRow.Index; // Örn: 7 (Adım 8)
            int totalSteps = _currentRecipe.Steps.Count; // 99

            // Onay al
            var result = MessageBox.Show($"Step {selectedIndex + 1} will be inserted. The last step (99) will be lost. Continue?", "Insert Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            // SONDAN BAŞLAYARAK KAYDIR (99. adım 98. adımdan veriyi alır)
            // List indexi 0-98 arasıdır (Toplam 99 adım)
            for (int i = totalSteps - 1; i > selectedIndex; i--)
            {
                // Bir önceki adımın verisini kopyala
                // Array.Copy kullanıyoruz ki referanslar karışmasın (Deep Copy)
                Array.Copy(_currentRecipe.Steps[i - 1].StepDataWords, _currentRecipe.Steps[i].StepDataWords, 25);
            }

            // Seçili adımı temizle (Boş yeni adım)
            Array.Clear(_currentRecipe.Steps[selectedIndex].StepDataWords, 0, 25);

            // Tabloyu yenile
            PopulateStepsGridView();

            // Seçili satırı koru ve detay panelini temizle
            dgvRecipeSteps.Rows[selectedIndex].Selected = true;
            DisplayCurrentRecipe(); // Detayları yenilemek için (veya sadece o anki editorü resetle)
        }

        private void BtnDeleteStep_Click(object sender, EventArgs e)
        {
            if (dgvRecipeSteps.CurrentRow == null || _currentRecipe == null) return;

            int selectedIndex = dgvRecipeSteps.CurrentRow.Index;
            int totalSteps = _currentRecipe.Steps.Count;

            var result = MessageBox.Show($"Step {selectedIndex + 1} will be deleted. Steps below will move up. Continue?", "Delete Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;

            // SEÇİLİ YERDEN BAŞLAYARAK YUKARI KAYDIR
            for (int i = selectedIndex; i < totalSteps - 1; i++)
            {
                // Bir sonraki adımın verisini buraya kopyala
                Array.Copy(_currentRecipe.Steps[i + 1].StepDataWords, _currentRecipe.Steps[i].StepDataWords, 25);
            }

            // Son adımı temizle (Veriler yukarı kaydığı için sonuncu boşalmalı)
            Array.Clear(_currentRecipe.Steps[totalSteps - 1].StepDataWords, 0, 25);

            // Tabloyu yenile
            PopulateStepsGridView();
        }

        private void BtnCopyStep_Click(object sender, EventArgs e)
        {
            if (dgvRecipeSteps.CurrentRow == null || _currentRecipe == null) return;

            int selectedIndex = dgvRecipeSteps.CurrentRow.Index;

            // Hafızayı başlat
            _copiedStepData = new short[25];

            // Seçili adımın verilerini hafızaya kopyala
            Array.Copy(_currentRecipe.Steps[selectedIndex].StepDataWords, _copiedStepData, 25);

            // Kullanıcıya bilgi ver (İsteğe bağlı, StatusBar varsa oraya yazılabilir)
            // MessageBox.Show("Step copied to clipboard.", "Info"); 
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

            // Hafızadaki veriyi seçili adıma yapıştır
            Array.Copy(_copiedStepData, _currentRecipe.Steps[selectedIndex].StepDataWords, 25);

            // Tabloyu yenile (Adım tipi değişmiş olabilir)
            // Sadece ilgili satırı güncellemek performans için daha iyidir:
            string newStepName = GetStepTypeName(_currentRecipe.Steps[selectedIndex]);
            dgvRecipeSteps.Rows[selectedIndex].Cells["StepType"].Value = newStepName;

            // Eğer o an detay paneli açıksa onu da güncellemek gerekir
            // Basitçe o satıra tekrar tıklanmış gibi tetikleyebiliriz:
            DgvRecipeSteps_CellClick(dgvRecipeSteps, new DataGridViewCellEventArgs(0, selectedIndex));
        }
        private void SetupStepsGridView()
        {
            if (dgvRecipeSteps == null) return;
            dgvRecipeSteps.DataSource = null;
            dgvRecipeSteps.Rows.Clear();
            dgvRecipeSteps.Columns.Clear();
            dgvRecipeSteps.AutoGenerateColumns = false;

            // DataGridView'in sütun başlıklarına tıklanınca sıralama yapmasını kapatıyoruz (isteğe bağlı)
            dgvRecipeSteps.AllowUserToResizeColumns = false;
            dgvRecipeSteps.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;


            // 1. ADIM NO SÜTUNU (Küçük ve sabit olmalı)
            // Absolute (sabit piksel) yerine Minimum/NotSet (40) kullanıyoruz.
            dgvRecipeSteps.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StepNumber",
                HeaderText = "Step No",
                DataPropertyName = "StepNumber",
                Width = 60, // Görünüm için biraz büyütüldü
                MinimumWidth = 60
            });

            // 2. ADIM TİPİ SÜTUNU (Geri kalan tüm alanı doldurmalı)
            dgvRecipeSteps.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StepType",
                HeaderText = "Step Type",
                // *** EN ÖNEMLİ DEĞİŞİKLİK BURADA: FILL modu tüm kalan alanı kullanır ***
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
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
            if ((controlWord & 8) != 0) stepTypes.Add("Dosage");
            if ((controlWord & 16) != 0) stepTypes.Add("Unloading");
            if ((controlWord & 32) != 0) stepTypes.Add("Squeezing");
            return string.Join(" + ", stepTypes);
        }

        private void DgvRecipeSteps_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Satır indeksi geçerli değilse veya gerekli nesneler yoksa metottan çık.
            if (e.RowIndex < 0 || _currentRecipe == null || pnlStepDetails == null) return;

            try
            {
                // --- BU SEFERKİ KESİN ÇÖZÜM BURASI ---

                // 1. Tıklanan satırdaki "Adım Numarası" hücresinin değerini alıyoruz.
                // NOT: Eğer tablodaki adım numarası kolonunun adı farklıysa, "StepNumber" yazan
                // yeri o kolonun adıyla değiştirmen gerekir. (Örn: "AdimNoKolonu")
                var stepNumberCell = dgvRecipeSteps.Rows[e.RowIndex].Cells["StepNumber"].Value;

                if (stepNumberCell == null) return; // Hücre boşsa hata vermemesi için kontrol.

                int stepNumberToFind = Convert.ToInt32(stepNumberCell);

                // 2. Orijinal ve sırasız "_currentRecipe.Steps" listesi içinde,
                // bu adım numarasına sahip olan adımı buluyoruz. Bu yöntem sıralamadan etkilenmez.
                var selectedStep = _currentRecipe.Steps.FirstOrDefault(s => s.StepNumber == stepNumberToFind);

                // 3. Aradığımız adım bulunamazsa (normalde olmamalı), güvenli bir şekilde metottan çıkıyoruz.
                if (selectedStep == null) return;


                // --- DÜZELTME BİTTİ, KODUNUN GERİ KALANI ARTIK DOĞRU ÇALIŞACAK ---

                pnlStepDetails.Controls.Clear();
                pnlStepDetails.Controls.Add(lblStepDetailsTitle);

                var selectedMachine = cmbTargetMachine.SelectedItem as Machine;
                lblStepDetailsTitle.Text = $"Step Details - Step No: {selectedStep.StepNumber}";

                var mainEditor = new StepEditor_Control();
                mainEditor.LoadStep(selectedStep, selectedMachine);

                mainEditor.StepDataChanged += (s, ev) =>
                {
                    // Tıklanan görsel satırı güncellemek için e.RowIndex kullanımı burada doğrudur.
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
                // Olası bir "kolon adı bulunamadı" veya "tip dönüşümü" hatasını yakalamak için.
                MessageBox.Show($"An error occurred while loading step details: {ex.Message}", "Error");
            }
        }

        private void BtnFtpSync_Click(object sender, EventArgs e)
        {
            // 1. FTP özelliği olan ve Kurutma Makinesi olmayan makine tiplerini bul.
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

            // 2. Kullanıcıdan bu tiplerden birini seçmesini iste.
            using (var typeForm = new RecipeTypeSelection_Form(ftpMachineTypes))
            {
                if (typeForm.ShowDialog() == DialogResult.OK)
                {
                    string selectedType = typeForm.SelectedType;
                    if (string.IsNullOrEmpty(selectedType)) return;

                    // 3. FTP formunu seçilen tiple aç.
                    if (_ftpFormInstance != null && !_ftpFormInstance.IsDisposed)
                    {
                        _ftpFormInstance.BringToFront();
                    }
                    else
                    {

                        // FtpSync_Form'u seçilen makine tipiyle başlat.
                        _ftpFormInstance = new FtpSync_Form(_machineRepository, _recipeRepository, _plcPollingService, selectedType, _ftpTransferService); // DÜZELTME
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
            // Butonları ve imleci işlem süresince yönet
            btnSendToPlc.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                // --- YENİ MANTIK: MAKİNE TİPİNE GÖRE İŞLEM SEÇİMİ ---
                if (selectedMachine.MachineType == "BYMakinesi")
                {
                    // 1. FTP bilgileri kontrolü
                    if (string.IsNullOrEmpty(selectedMachine.FtpUsername) || string.IsNullOrEmpty(selectedMachine.IpAddress))
                    {
                        MessageBox.Show("FTP information (IP Address, Username) is missing for this machine. Please enter the information from the Settings > Machine Management screen.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 2. Yeni numaratik giriş panelini kullanarak kullanıcıdan numara al
                    string recipeNumberStr = ShowFtpRecipeNumberDialog();
                    if (string.IsNullOrEmpty(recipeNumberStr))
                    {
                        return; // Kullanıcı iptal etti
                    }

                    if (!int.TryParse(recipeNumberStr, out int recipeNumber) || recipeNumber < 1 || recipeNumber > 99)
                    {
                        MessageBox.Show("Invalid prescription number. Please enter a number between 1-99.", "Error");
                        return;
                    }

                    // 3. Dosya adını otomatik olarak XPR0000.csv formatına çevir
                    string remoteFileName = string.Format("XPR{0:D5}.csv", recipeNumber);

                    btnSendToPlc.Enabled = false;
                    this.Cursor = Cursors.WaitCursor;
                    try
                    {
                        // 4. Reçeteyi CSV'ye çevir
                        string csvContent = RecipeCsvConverter.ToCsv(_currentRecipe);

                        // 5. FTP servisi ile dosyayı gönder
                        var ftpService = new FtpService(selectedMachine.IpAddress, selectedMachine.FtpUsername, selectedMachine.FtpPassword);
                        await ftpService.UploadFileAsync($"/{remoteFileName}", csvContent);

                        MessageBox.Show($"'Recipe '{_currentRecipe.RecipeName}' was successfully sent to machine '{selectedMachine.MachineName}' with name '{remoteFileName}'.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error sending recipe via FTP: {ex.Message}", "FTP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                        btnSendToPlc.Enabled = true;
                    }
                }
                else // Kurutma Makinesi gibi diğer makineler için eski, doğrudan PLC'ye yazma yöntemi devam eder
                {
                    if (_plcManagers == null || !_plcManagers.TryGetValue(selectedMachine.Id, out var plcManager))
                    {
                        MessageBox.Show($"'{selectedMachine.MachineName}' No active PLC connection found for .", "Connection Error");
                        return;
                    }

                    int? recipeSlot = null;
                    if (selectedMachine.MachineType == "Kurutma Makinesi")
                    {
                        // Güncellenmiş ShowInputDialog metodunu kullanıyoruz (isNumeric = true)
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

                    btnSendToPlc.Enabled = false;
                    this.Cursor = Cursors.WaitCursor;
                    try
                    {
                        var result = await plcManager.WriteRecipeToPlcAsync(_currentRecipe, recipeSlot);

                        if (result.IsSuccess)
                        {
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
                    finally
                    {
                        this.Cursor = Cursors.Default;
                        btnSendToPlc.Enabled = true;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while sending: {ex.Message}", "Error");
            }
            finally
            {
                // İşlem bitince UI elemanlarını eski haline getir
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
            Control inputBox; // Kontrol tipini dinamik olarak belirliyoruz

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

            if (_plcManagers == null || !_plcManagers.TryGetValue(selectedMachine.Id, out var plcManager))
            {
                MessageBox.Show($"'{selectedMachine.MachineName}' No active PLC connection found for .", "Connection Error");
                return;
            }

            btnReadFromPlc.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                var result = await plcManager.ReadRecipeFromPlcAsync();
                if (result.IsSuccess)
                {
                    var recipeFromPlc = new ScadaRecipe { RecipeName = $"PLC_{selectedMachine.MachineUserDefinedId}_{DateTime.Now:HHmm}" };

                    if (selectedMachine.MachineType == "Kurutma Makinesi")
                    {
                        var step = new ScadaRecipeStep { StepNumber = 1 };
                        // Kurutma makinesi 5 word + 1 kontrol word'ü okur
                        Array.Copy(result.Content, 0, step.StepDataWords, 0, Math.Min(result.Content.Length, 6));
                        recipeFromPlc.Steps.Add(step);
                    }
                    else
                    {
                        for (int i = 0; i < 98; i++)
                        {
                            var step = new ScadaRecipeStep { StepNumber = i + 1 };
                            Array.Copy(result.Content, i * 25, step.StepDataWords, 0, 25);
                            recipeFromPlc.Steps.Add(step);
                        }
                    }

                    _currentRecipe = recipeFromPlc;
                    DisplayCurrentRecipe();
                    MessageBox.Show($"'{selectedMachine.MachineName}' The recipe in the machine was read successfully.Please give a new name and save it.", "Successful");
                }
                else { MessageBox.Show($"Error reading prescription: {result.Message}", "Error"); }
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
            // 1. Listeden seçili olan tüm reçeteleri al.
            var selectedRecipes = lstRecipes.SelectedItems.Cast<ScadaRecipe>().ToList();

            // 2. Hiçbir reçete seçilmediyse uyarı ver ve metottan çık.
            if (!selectedRecipes.Any())
            {
                MessageBox.Show("Please select at least one recipe from the list to delete.", "Warning");
                return;
            }

            // 3. Kullanıcıdan toplu silme için onay al.
            var result = MessageBox.Show(
                $"{selectedRecipes.Count} Are you sure you want to permanently delete the prescription?\nThis action cannot be undone.", "Bulk Deletion Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            // 4. Kullanıcı "Evet" derse silme işlemine başla.
            if (result == DialogResult.Yes)
            {
                try
                {
                    // Seçilen her bir reçete için döngü kur ve sil.
                    foreach (var recipeToDelete in selectedRecipes)
                    {
                        _recipeRepository.DeleteRecipe(recipeToDelete.Id);
                    }

                    MessageBox.Show($"{selectedRecipes.Count} The prescription was deleted successfully.", "Process Completed");

                    // 5. Mevcut reçete ekranını temizle ve listeyi yenile.
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
                // GÜNCELLENDİ: Yeni metottan 3 değer alınıyor
                var (totalCost, currencySymbol, breakdown) = RecipeCostCalculator.Calculate(_currentRecipe, costParams);

                // GÜNCELLENDİ: Sonuç para birimi sembolü ile birlikte gösteriliyor
                lblTotalCost.Text = $"{totalCost:F2} {currencySymbol}";

                // Detaylı döküm tooltip'e yazdırılıyor
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
            // Her harf girişinde listeyi filtrele
            FilterRecipeList();
        }

        private void SortOption_CheckedChanged(object sender, EventArgs e)
        {
            // Sadece "Checked" olan tetiklediğinde çalışsın (gereksiz çift çalışmayı önler)
            if (((RadioButton)sender).Checked)
            {
                FilterRecipeList();
            }
        }

        // --- GÜNCELLENMİŞ FILTER METODU ---

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

            // 1. MAKİNE TİPİNE GÖRE FİLTRELEME
            var filteredRecipes = _recipeList
                .Where(r => r.TargetMachineType == filterType);

            // 2. ARAMA METNİNE GÖRE FİLTRELEME (YENİ)
            string searchText = txtSearchRecipe.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                filteredRecipes = filteredRecipes.Where(r =>
                    r.RecipeName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // 3. SIRALAMA (YENİ)
            if (radioSortName.Checked)
            {
                // A'dan Z'ye Sıralama
                filteredRecipes = filteredRecipes.OrderBy(r => r.RecipeName);
            }
            else if (radioSortDate.Checked)
            {
                // Eklenme/Güncellenme Sırasına Göre (Id genelde bu sırayı verir)
                // En yeni en üstte olsun istiyorsanız OrderByDescending
                filteredRecipes = filteredRecipes.OrderByDescending(r => r.Id);
            }

            // Listeye çevir
            var finalRecipeList = filteredRecipes.ToList();

            // UI GÜNCELLEME
            lstRecipes.SelectedIndexChanged -= LstRecipes_SelectedIndexChanged;

            lstRecipes.DataSource = null;
            lstRecipes.DataSource = finalRecipeList;
            lstRecipes.DisplayMember = "RecipeName";
            lstRecipes.ValueMember = "Id";

            lstRecipes.SelectedIndexChanged += LstRecipes_SelectedIndexChanged;

            // Filtreleme sonrası durum kontrolü
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
        private void ProsesKontrol_Control_KeyDown(object sender, KeyEventArgs e)
        {
            // Yalnızca klavye odağı bu UserControl'deyken çalışır.
            // Eğer listbox'a odaklanılırsa bu olay çalışmayabilir.
            // Daha güvenilir yöntem için 2. adıma bakınız.
        }


        private void LstRecipes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                BtnCopyRecipes_Click(null, null);
                e.Handled = true; // Olayın daha fazla işlenmesini engeller
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                BtnPasteRecipes_Click(null, null);
                e.Handled = true;
            }
        }
        // Ctrl+C İşlevi
        private void BtnCopyRecipes_Click(object sender, EventArgs e)
        {
            // Listeden seçili olan tüm reçeteleri al
            var selectedRecipes = lstRecipes.SelectedItems.Cast<ScadaRecipe>().ToList();

            if (!selectedRecipes.Any())
            {
                MessageBox.Show("Please select the recipes to copy.", "Warning");
                return;
            }

            // Listeyi temizle ve yeni kopyalanacak reçeteleri içine ekle
            _copiedRecipes.Clear();

            // Not: Burada sadece referans değil, nesnenin kendisini kopyalamak önemlidir (Deep Copy ihtiyacı olabilir)
            // Varsayım: ScadaRecipe ve Steps listesi, ListBox'a yüklenmeden önce veritabanından tamamen çekildiği için
            // buradaki listenin kopyalanması yeterli olacaktır. Eğer RecipeRepository.GetRecipeById(id) deep copy yapıyorsa sorun yok.
            // En güvenli yöntem: Tüm seçili reçeteleri veritabanından tekrar çekmek veya bir kopyalama metodu kullanmak.

            foreach (var recipe in selectedRecipes)
            {
                // Detaylı reçeteyi tekrar çekip kopyasına kaydedelim
                var fullRecipe = _recipeRepository.GetRecipeById(recipe.Id);
                _copiedRecipes.Add(fullRecipe);
            }

            MessageBox.Show($"{_copiedRecipes.Count} recipe(s) copied. (Ctrl+V to paste)", "Success");
        }

        // Ctrl+V İşlevi
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
                    // Yeni bir kopya reçete oluştur (DB'ye kaydetmek için ID'yi sıfırla)
                    var newRecipe = new ScadaRecipe
                    {
                        Id = 0, // Yeni kayıt olacağını belirtir
                        TargetMachineType = originalRecipe.TargetMachineType,
                        Steps = new List<ScadaRecipeStep>()
                    };

                    // Yeni ismi belirle: "Copy_1_RecipeName" veya "Copy_2_RecipeName"
                    string originalName = originalRecipe.RecipeName.StartsWith("Copy_")
                                        ? originalRecipe.RecipeName.Substring(originalRecipe.RecipeName.IndexOf('_', originalRecipe.RecipeName.IndexOf('_') + 1) + 1) // Copy_n_Name kısmından sonraki adı alır
                                        : originalRecipe.RecipeName;

                    // Veritabanında mevcut en yüksek 'Copy_n' sayısını bulalım
                    int copyIndex = 1;
                    while (_recipeRepository.GetRecipeByName($"Copy_{copyIndex}_{originalName}") != null)
                    {
                        copyIndex++;
                    }

                    newRecipe.RecipeName = $"Copy_{copyIndex}_{originalName}";

                    // Adımların derin kopyasını yap
                    foreach (var step in originalRecipe.Steps)
                    {
                        var newStep = new ScadaRecipeStep
                        {
                            StepNumber = step.StepNumber,
                            StepDataWords = new short[25] // 25'lik dizi olduğu varsayılıyor
                        };
                        // Diziyi kopyala
                        Array.Copy(step.StepDataWords, newStep.StepDataWords, 25);
                        newRecipe.Steps.Add(newStep);
                    }

                    // Veritabanına kaydet
                    _recipeRepository.SaveRecipe(newRecipe);
                    pasteCount++;
                }

                MessageBox.Show($"{pasteCount} recipe(s) successfully created and saved.", "Successful");

                // Listeyi yenile
                LoadRecipeList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while pasting the recipe: {ex.Message}", "Error");
            }
        }
    }
}
