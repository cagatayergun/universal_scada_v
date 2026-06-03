// UI/Controls/RecipeStepEditors/StepEditor_Control.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using Telemetry.Models;
using Telemetry.Repositories;
using MaterialSkin;          // YENİ EKLENDİ: MaterialSkin ana kütüphanesi
using MaterialSkin.Controls; // YENİ EKLENDİ: MaterialForm ve bileşenler için

namespace Telemetry.UI.Controls.RecipeStepEditors
{
    public partial class StepEditor_Control : UserControl
    {
        private ScadaRecipeStep _step;
        private Machine _machine;
        private bool _isUpdating = false;
        private readonly RecipeConfigurationRepository _configRepo = new RecipeConfigurationRepository();
        public event EventHandler StepDataChanged;

        public StepEditor_Control()
        {
            InitializeComponent();

            // SPEED OPTİMİZASYON: Dinamik parametre panelleri yüklenirken veya kaydırılırken (scroll) titremeyi engeller
            this.DoubleBuffered = true;
            if (flpParameters != null)
            {
                EnableDoubleBuffer(flpParameters);
            }

            chkSuAlma.CheckedChanged += OnStepTypeChanged;
            chkIsitma.CheckedChanged += OnStepTypeChanged;
            chkCalisma.CheckedChanged += OnStepTypeChanged;
            chkDozaj.CheckedChanged += OnStepTypeChanged;
            chkBosaltma.CheckedChanged += OnStepTypeChanged;
            chkSikma.CheckedChanged += OnStepTypeChanged;
            chknumune.CheckedChanged += OnStepTypeChanged;
            flpParameters.Resize += new EventHandler(flpParameters_Resize);
        }

        public void LoadStep(ScadaRecipeStep step, Machine machine)
        {
            _step = step;
            _machine = machine;
            _isUpdating = true;
            UpdateCheckboxesFromStepData();
            _isUpdating = false;
            UpdateEditorPanels();
        }

        private void flpParameters_Resize(object sender, EventArgs e)
        {
            flpParameters.SuspendLayout();
            foreach (Control control in flpParameters.Controls)
            {
                // MaterialCard dönüştürmesi yapıldığı için kontrol tipi güncellendi
                if (control is MaterialCard || control is Panel)
                {
                    control.Width = flpParameters.ClientSize.Width - 25;
                }
            }
            flpParameters.ResumeLayout();
        }

        public void SetReadOnly(bool isReadOnly)
        {
            SetControlsState(this.Controls, !isReadOnly);
        }

        private void SetControlsState(Control.ControlCollection controls, bool enabled)
        {
            foreach (Control control in controls)
            {
                if (control is NumericUpDown || control is TextBox || control is CheckBox || control is ComboBox)
                {
                    control.Enabled = enabled;
                }
                if (control.HasChildren)
                {
                    SetControlsState(control.Controls, enabled);
                }
            }
        }

        private void UpdateCheckboxesFromStepData()
        {
            if (_step == null) return;
            short controlWord = _step.StepDataWords[24];
            chkSuAlma.Checked = (controlWord & 1) != 0;
            chkIsitma.Checked = (controlWord & 2) != 0;
            chkCalisma.Checked = (controlWord & 4) != 0;
            chkDozaj.Checked = (controlWord & 8) != 0;
            chkBosaltma.Checked = (controlWord & 16) != 0;
            chkSikma.Checked = (controlWord & 32) != 0;
            chknumune.Checked = (controlWord & 1024) != 0;
        }

        private void OnStepTypeChanged(object sender, EventArgs e)
        {
            if (_isUpdating) return;
            var changedCheckbox = sender as CheckBox;
            if (changedCheckbox == null) return;

            if (!IsSelectionValid(changedCheckbox))
            {
                _isUpdating = true;
                changedCheckbox.Checked = false;
                _isUpdating = false;
                return;
            }

            UpdateStepDataFromCheckboxes();
            UpdateEditorPanels();
            StepDataChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool IsSelectionValid(CheckBox justChanged)
        {
            var checkedBoxes = pnlStepTypes.Controls.OfType<CheckBox>().Where(c => c.Checked).ToList();

            if (checkedBoxes.Contains(chknumune) && checkedBoxes.Count > 1)
            {
                MessageBox.Show("The 'Operator Call' step cannot be selected together with any other step. Please select it alone.",
                                "Rule Violation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (checkedBoxes.Count > 2)
            {
                MessageBox.Show("You can select up to 2 different transaction types in one step.", "Rule Violation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var specialSteps = new List<CheckBox> { chkSikma, chkBosaltma };
            var standardSteps = new List<CheckBox> { chkSuAlma, chkIsitma, chkDozaj, chkCalisma };

            bool isAnySpecialChecked = checkedBoxes.Any(c => specialSteps.Contains(c));
            bool isAnyStandardChecked = checkedBoxes.Any(c => standardSteps.Contains(c));

            if (isAnySpecialChecked && isAnyStandardChecked)
            {
                MessageBox.Show("Spinning or Draining steps cannot be selected together with other steps such as Water Intake, Heating.", "Rule Violation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void UpdateStepDataFromCheckboxes()
        {
            if (_step == null) return;
            short controlWord = 0;
            if (chkSuAlma.Checked) controlWord |= 1;
            if (chkIsitma.Checked) controlWord |= 2;
            if (chkCalisma.Checked) controlWord |= 4;
            if (chkDozaj.Checked) controlWord |= 8;
            if (chkBosaltma.Checked) controlWord |= 16;
            if (chkSikma.Checked) controlWord |= 32;
            if (chknumune.Checked) controlWord |= 1024;
            _step.StepDataWords[24] = controlWord;
        }

        private void UpdateEditorPanels()
        {
            flpParameters.SuspendLayout();
            flpParameters.Controls.Clear();

            if (_step == null || _machine == null)
            {
                flpParameters.ResumeLayout();
                return;
            }

            try
            {
                // =========================================================================
                // MODERNİZASYON: DARK MODE UYUMLU SOFT PASTEL YAZI RENK PALETİ
                // Arka planı boyamak yerine başlık yazılarını soft renk gruplarına ayırıyoruz.
                // =========================================================================
                var stepColorMap = new Dictionary<int, Color>
                {
                    { 1, Color.FromArgb(100, 181, 246) }, // Soft Mavi (Su Alma)
                    { 2, Color.FromArgb(239, 83, 80) },   // Soft Kırmızı (Isıtma)
                    { 3, Color.FromArgb(129, 199, 132) }, // Soft Yeşil (Çalışma)
                    { 4, Color.FromArgb(255, 183, 77) },  // Soft Turuncu (Dozaj)
                    { 5, Color.FromArgb(77, 208, 225) },  // Soft Turkuaz (Boşaltma)
                    { 6, Color.FromArgb(144, 164, 174) }, // Soft Kurşuni (Sıkma)
                    { 7, Color.FromArgb(186, 104, 200) }  // Soft Mor (Numune)
                };

                var stepIdMap = new Dictionary<CheckBox, int>
                {
                    { chkSuAlma, 1 }, { chkIsitma, 2 }, { chkCalisma, 3 },
                    { chkDozaj, 4 }, { chkBosaltma, 5 }, { chkSikma, 6 },{ chknumune, 7 }
                };

                foreach (var kvp in stepIdMap)
                {
                    if (kvp.Key.Checked)
                    {
                        int stepId = kvp.Value;
                        string stepName = kvp.Key.Text.ToUpper();

                        // MODERNİZASYON: Normal Panel yerine gölgeli ve oval köşeli MaterialCard oluşturuluyor
                        var containerCard = new MaterialCard
                        {
                            Margin = new Padding(10, 6, 10, 6),
                            Depth = 0
                        };

                        int currentY = 12; // İç boşluk (padding) ayarı

                        var header = new Label
                        {
                            Text = stepName,
                            Font = new Font("Segoe UI Black", 10F, FontStyle.Bold),
                            // Kartın rengi merkezi temadan otomatik gelir, başlık ise adım türüne göre pastel boyanır
                            ForeColor = stepColorMap.TryGetValue(stepId, out Color accentColor) ? accentColor : Color.White,
                            AutoSize = true,
                            Location = new Point(14, currentY),
                            BackColor = Color.Transparent
                        };
                        containerCard.Controls.Add(header);
                        currentY = header.Bottom + 12;

                        string layoutJson = _configRepo.GetLayoutJson(_machine.MachineSubType, stepId) ?? _configRepo.GetLayoutJson("DEFAULT", stepId);
                        if (!string.IsNullOrEmpty(layoutJson))
                        {
                            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var controlsData = JsonSerializer.Deserialize<List<ControlMetadata>>(layoutJson, options);
                            foreach (var data in controlsData)
                            {
                                var control = CreateControlFromMetadata(data);
                                if (control != null)
                                {
                                    control.Location = new Point(20, currentY);
                                    containerCard.Controls.Add(control);
                                    currentY = control.Bottom + 8;
                                }
                            }
                        }
                        containerCard.Height = currentY + 10;
                        flpParameters.Controls.Add(containerCard);
                    }
                }
            }
            finally
            {
                flpParameters.ResumeLayout(true);
                flpParameters_Resize(this, EventArgs.Empty);
            }
        }

        private Control CreateControlFromMetadata(ControlMetadata data)
        {
            Type controlType = Type.GetType(data.ControlType);
            if (controlType == null) return null;
            Control control = (Control)Activator.CreateInstance(controlType);
            control.Name = data.Name;
            control.Text = data.Text;
            var locParts = data.Location.Split(',');
            control.Location = new Point(int.Parse(locParts[0].Trim()), int.Parse(locParts[1].Trim()));
            var sizeParts = data.Size.Split(',');
            control.Size = new Size(int.Parse(sizeParts[0].Trim()), int.Parse(sizeParts[1].Trim()));
            control.Tag = new PlcMapping { WordIndex = data.PLC_WordIndex, BitIndex = data.PLC_BitIndex };

            if (control is NumericUpDown num)
            {
                // =========================================================================
                // MODERNİZASYON: DİNAMİK ÜRETİLEN SAYI KUTULARINI DARK MODE YAPMA
                // JSON'dan gelen sayı kutularını koyu gri zemin ve beyaz yazıya çeker.
                // =========================================================================
                num.BackColor = Color.FromArgb(44, 52, 64);
                num.ForeColor = Color.FromArgb(240, 240, 240);
                num.BorderStyle = BorderStyle.FixedSingle;

                num.Maximum = data.Maximum;
                num.DecimalPlaces = data.DecimalPlaces;
                if (data.PLC_WordIndex < _step.StepDataWords.Length)
                {
                    if (num.DecimalPlaces > 0)
                        num.Value = _step.StepDataWords[data.PLC_WordIndex] / (decimal)Math.Pow(10, num.DecimalPlaces);
                    else
                        num.Value = _step.StepDataWords[data.PLC_WordIndex];
                }
                num.ValueChanged += OnDynamicControlValueChanged;
            }
            else if (control is CheckBox chk)
            {
                chk.ForeColor = Color.FromArgb(220, 220, 220); // Yazı rengi açık gri
                chk.BackColor = Color.Transparent;

                if (data.PLC_WordIndex < _step.StepDataWords.Length)
                {
                    short word = _step.StepDataWords[data.PLC_WordIndex];
                    int bitMask = 1 << data.PLC_BitIndex;
                    chk.Checked = (word & bitMask) != 0;
                }
                chk.CheckedChanged += OnDynamicControlValueChanged;
            }
            return control;
        }

        private void OnDynamicControlValueChanged(object sender, EventArgs e)
        {
            if (_isUpdating) return;
            Control control = sender as Control;
            if (control?.Tag is not PlcMapping mapping) return;
            if (mapping.WordIndex < _step.StepDataWords.Length)
            {
                if (control is NumericUpDown num)
                {
                    if (num.DecimalPlaces > 0)
                        _step.StepDataWords[mapping.WordIndex] = (short)(num.Value * (decimal)Math.Pow(10, num.DecimalPlaces));
                    else
                        _step.StepDataWords[mapping.WordIndex] = (short)num.Value;
                }
                else if (control is CheckBox chk)
                {
                    SetBit(_step.StepDataWords, mapping.WordIndex, mapping.BitIndex, chk.Checked);
                }
            }
            if (_machine.MachineType != "Kurutma Makinesi") // Kodunuzda olmayan ama tetikleme güvenliği sağlayan alan
            {
                StepDataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetBit(short[] data, int wordIndex, int bitIndex, bool value)
        {
            if (value) data[wordIndex] = (short)(data[wordIndex] | (1 << bitIndex));
            else data[wordIndex] = (short)(data[wordIndex] & ~(1 << bitIndex));
        }

        // SPEED OPTİMİZASYON: FlowLayoutPanel çizim önbelleğini donanımsal ivmeye bağlayan yansıtma metodu
        private void EnableDoubleBuffer(Control control)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, control, new object[] { true });
        }
    }
}