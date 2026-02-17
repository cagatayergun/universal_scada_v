using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using TekstilScada.Models;
using TekstilScada.Repositories;

namespace TekstilScada.UI.Controls.RecipeStepEditors
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
                if (control is Panel)
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
                if (control is NumericUpDown || control is TextBox || control is CheckBox || control is ComboBox || control is Button)
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
                MessageBox.Show("The 'Operator Call' step cannot be selected together with any other step.", "Rule Violation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show("Spinning or Draining steps cannot be selected together with other steps.", "Rule Violation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                var stepColorMap = new Dictionary<int, Color>
                {
                    { 1, Color.FromArgb(204, 229, 255) }, { 2, Color.FromArgb(255, 204, 204) },
                    { 3, Color.FromArgb(204, 255, 204) }, { 4, Color.FromArgb(255, 211, 106) },
                    { 5, Color.FromArgb(173, 216, 230) }, { 6, Color.FromArgb(213, 213, 211) },{ 7, Color.FromArgb(189, 195, 199) }
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
                        var containerPanel = new Panel
                        {
                            BorderStyle = BorderStyle.FixedSingle,
                            Margin = new Padding(10, 5, 10, 5),
                            Width = flpParameters.ClientSize.Width - 25
                        };
                        containerPanel.BackColor = stepColorMap.TryGetValue(stepId, out Color color) ? color : Color.WhiteSmoke;

                        var header = new Label
                        {
                            Text = stepName,
                            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                            ForeColor = Color.Black,
                            AutoSize = true,
                            Location = new Point(5, 5)
                        };
                        containerPanel.Controls.Add(header);

                        string layoutJson = _configRepo.GetLayoutJson(_machine.MachineSubType, stepId) ?? _configRepo.GetLayoutJson("DEFAULT", stepId);

                        int maxBottom = header.Bottom + 10;

                        if (!string.IsNullOrEmpty(layoutJson))
                        {
                            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var controlsData = JsonSerializer.Deserialize<List<ControlMetadata>>(layoutJson, options);

                            foreach (var data in controlsData)
                            {
                                var control = CreateControlFromMetadata(data);
                                if (control != null)
                                {
                                    // Header yüksekliği kadar aşağı ötele
                                    control.Top += header.Bottom + 10;
                                    containerPanel.Controls.Add(control);
                                    if (control.Bottom > maxBottom) maxBottom = control.Bottom;
                                }
                            }
                        }

                        containerPanel.Height = maxBottom + 20;
                        flpParameters.Controls.Add(containerPanel);
                    }
                }
            }
            finally
            {
                flpParameters.ResumeLayout(true);
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

            // --- GÖRÜNÜM AYARLARI (RENK & FONT) ---
            if (!string.IsNullOrEmpty(data.BackColor))
                control.BackColor = ColorTranslator.FromHtml(data.BackColor);

            if (!string.IsNullOrEmpty(data.ForeColor))
                control.ForeColor = ColorTranslator.FromHtml(data.ForeColor);

            float fontSize = data.FontSize > 0 ? data.FontSize : 9.75f;
            control.Font = new Font("Segoe UI", fontSize, data.FontBold ? FontStyle.Bold : FontStyle.Regular);

            // --- HİZALAMA AYARLARI ---
            if (control is Label lbl && Enum.TryParse(data.ContentAlignment, out ContentAlignment caLbl)) lbl.TextAlign = caLbl;
            else if (control is Button btn && Enum.TryParse(data.ContentAlignment, out ContentAlignment caBtn)) btn.TextAlign = caBtn;
            else if (control is TextBox txt && Enum.TryParse(data.HorizontalAlignment, out HorizontalAlignment haTxt)) txt.TextAlign = haTxt;
            else if (control is NumericUpDown numAlign && Enum.TryParse(data.HorizontalAlignment, out HorizontalAlignment haNum)) numAlign.TextAlign = haNum;

            // --- PLC MAPPING & DATA ---
            // Metadata'yı Tag'e gömüyoruz (özellikle buton durumları için gerekli)
            control.Tag = new ControlTagData
            {
                WordIndex = data.PLC_WordIndex,
                BitIndex = data.PLC_BitIndex,
                Metadata = data // Metadata'nın tamamını sakla (PressedColor vb. için)
            };

            // --- KONTROL TİPİNE GÖRE DAVRANIŞLAR ---

            // 1. NUMERIC UPDOWN
            if (control is NumericUpDown num)
            {
                num.Maximum = data.Maximum;
                num.Minimum = data.Minimum;
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
            // 2. CHECKBOX
            else if (control is CheckBox chk)
            {
                if (data.PLC_WordIndex < _step.StepDataWords.Length)
                {
                    short word = _step.StepDataWords[data.PLC_WordIndex];
                    int bitMask = 1 << data.PLC_BitIndex;
                    chk.Checked = (word & bitMask) != 0;
                }
                chk.CheckedChanged += OnDynamicControlValueChanged;
            }
            // 3. BUTTON (YENİ: TOGGLE ÖZELLİĞİ)
            else if (control is Button btnControl)
            {
                if (data.ButtonStyle == "Flat") btnControl.FlatStyle = FlatStyle.Flat;
                else btnControl.FlatStyle = FlatStyle.Standard;

                // Toggle ise başlangıç durumunu yükle
                if (data.IsToggleButton && data.PLC_WordIndex < _step.StepDataWords.Length)
                {
                    short word = _step.StepDataWords[data.PLC_WordIndex];
                    int bitMask = 1 << data.PLC_BitIndex;
                    bool isPressed = (word & bitMask) != 0;

                    UpdateButtonVisualState(btnControl, isPressed, data);
                }

                // Click olayını bağla
                btnControl.Click += OnDynamicButtonClick;
            }

            return control;
        }

        // --- BUTON TIKLAMA YÖNETİCİSİ ---
        private void OnDynamicButtonClick(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || !(btn.Tag is ControlTagData tagData)) return;

            var meta = tagData.Metadata;

            // Eğer Toggle (Kalıcı) Buton ise:
            if (meta.IsToggleButton)
            {
                // Mevcut değeri oku
                short currentWord = _step.StepDataWords[tagData.WordIndex];
                int bitMask = 1 << tagData.BitIndex;
                bool isCurrentlyPressed = (currentWord & bitMask) != 0;

                // Değeri tersine çevir (Toggle)
                bool newState = !isCurrentlyPressed;

                // PLC Verisini Güncelle
                SetBit(_step.StepDataWords, tagData.WordIndex, tagData.BitIndex, newState);

                // Görünümü Güncelle
                UpdateButtonVisualState(btn, newState, meta);

                // Değişikliği Bildir
                StepDataChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // Push Buton (Anlık) mantığı isteniyorsa buraya eklenebilir.
                // Şimdilik sadece görsel tıklama efekti verir.
            }
        }

        // --- GÖRSEL DURUM GÜNCELLEME ---
        private void UpdateButtonVisualState(Button btn, bool isPressed, ControlMetadata meta)
        {
            if (isPressed)
            {
                // Basılı Durum Renkleri
                if (!string.IsNullOrEmpty(meta.PressedBackColor))
                    btn.BackColor = ColorTranslator.FromHtml(meta.PressedBackColor);
                else
                    btn.BackColor = Color.Green; // Varsayılan Basılı Rengi

                if (!string.IsNullOrEmpty(meta.PressedForeColor))
                    btn.ForeColor = ColorTranslator.FromHtml(meta.PressedForeColor);

                if (!string.IsNullOrEmpty(meta.PressedText))
                    btn.Text = meta.PressedText;
            }
            else
            {
                // Normal Durum Renkleri (Metadata'daki ana renkler)
                if (!string.IsNullOrEmpty(meta.BackColor))
                    btn.BackColor = ColorTranslator.FromHtml(meta.BackColor);
                else
                    btn.BackColor = SystemColors.Control;

                if (!string.IsNullOrEmpty(meta.ForeColor))
                    btn.ForeColor = ColorTranslator.FromHtml(meta.ForeColor);
                else
                    btn.ForeColor = SystemColors.ControlText;

                btn.Text = meta.Text; // Normal metin
            }
        }

        private void OnDynamicControlValueChanged(object sender, EventArgs e)
        {
            if (_isUpdating) return;
            Control control = sender as Control;
            if (control?.Tag is not ControlTagData mapping) return;

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
            StepDataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SetBit(short[] data, int wordIndex, int bitIndex, bool value)
        {
            if (value) data[wordIndex] = (short)(data[wordIndex] | (1 << bitIndex));
            else data[wordIndex] = (short)(data[wordIndex] & ~(1 << bitIndex));
        }

        // Yardımcı Sınıf: Tag içinde hem mapping hem metadata saklamak için
        private class ControlTagData
        {
            public int WordIndex { get; set; }
            public int BitIndex { get; set; }
            public ControlMetadata Metadata { get; set; }
        }
    }
}