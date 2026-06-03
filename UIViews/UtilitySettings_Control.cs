// UIViews/UtilitySettings_Control.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Telemetry.Core;
using Telemetry.Models;
using Telemetry.Repositories;
using MaterialSkin;          // YENİ EKLENDİ
using MaterialSkin.Controls; // YENİ EKLENDİ

namespace Telemetry.UIViews
{
    public partial class UtilitySettings_Control : UserControl
    {
        private readonly UtilityRepository _repository;
        private List<UtilityLine> _allLines;
        private UtilityLine _currentLine;

        // Dinamik sensör kontrollerini hafızada tutmak için harita yapısı
        private Dictionary<string, SensorControls> _sensorMap = new Dictionary<string, SensorControls>();

        private class SensorControls
        {
            public CheckBox ChkEnabled { get; set; }
            public NumericUpDown NumAddress { get; set; }
            public ComboBox CmbDataType { get; set; }
            public TextBox TxtMultiplier { get; set; }
        }

        public UtilitySettings_Control()
        {
            InitializeComponent();
            _repository = new UtilityRepository();

            // SPEED OPTİMİZASYON: Kontrol sekmeleri arası geçişlerde donma ve titremeleri önler
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent; // Arka plan yönetimi üst ebeveyne devredildi

            // Çizim performans adaptörleri tetiklendi
            EnableDoubleBuffer(flowLayoutPanelSensors);
            EnableDoubleBuffer(lstLines);

            // =========================================================================
            // MODERNİZASYON: STANDART GİRİŞ KUTULARINI DARK MODE UYARLAMA ADAPTÖRÜ
            // Statik yerleşimdeki ham bileşenleri koyu grafit renk şemasıyla eşitler.
            // =========================================================================
            Color controlBg = Color.FromArgb(44, 52, 64);
            Color controlFg = Color.FromArgb(240, 240, 240);

            if (txtLineName != null) { txtLineName.BackColor = controlBg; txtLineName.ForeColor = controlFg; txtLineName.BorderStyle = BorderStyle.FixedSingle; }
            if (txtIpAddress != null) { txtIpAddress.BackColor = controlBg; txtIpAddress.ForeColor = controlFg; txtIpAddress.BorderStyle = BorderStyle.FixedSingle; }
            if (txtPort != null) { txtPort.BackColor = controlBg; txtPort.ForeColor = controlFg; }
            if (txtSlaveId != null) { txtSlaveId.BackColor = controlBg; txtSlaveId.ForeColor = controlFg; }
            if (lstLines != null) { lstLines.BackColor = controlBg; lstLines.ForeColor = controlFg; }
        }

        private void UtilitySettings_Control_Load(object sender, EventArgs e)
        {
            LoadLines();
            CreateSensorLayouts();
        }

        private void LoadLines()
        {
            lstLines.Items.Clear();
            _allLines = _repository.GetUtilityLines() ?? new List<UtilityLine>();
            foreach (var line in _allLines)
            {
                lstLines.Items.Add(line);
            }
            lstLines.DisplayMember = "LineName";
        }

        private void CreateSensorLayouts()
        {
            // SPEED OPTİMİZASYON: Çoklu grup yerleşim çizimini dondurarak tek frame'de basar
            flowLayoutPanelSensors.SuspendLayout();
            try
            {
                flowLayoutPanelSensors.Controls.Clear();
                _sensorMap.Clear();

                AddSensorGroup("Water", "Su Sayacı Ayarları");
                AddSensorGroup("Elec", "Elektrik Sayacı Ayarları");
                AddSensorGroup("Steam", "Buhar Sayacı Ayarları");
                AddSensorGroup("Air", "Hava Sayacı Ayarları");
            }
            finally
            {
                flowLayoutPanelSensors.ResumeLayout(true);
            }
        }

        // =========================================================================
        // MODERNİZASYON: DİNAMİK SAYAÇ GRUP PANELİ TEMA ADAPTÖRÜ
        // Çalışma anında üretilen kontrollerin koyu modda kaybolması/parlaması kesin önlenmiştir.
        // =========================================================================
        private void AddSensorGroup(string key, string title)
        {
            bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;

            // Koyu modda mat grafit yazı, açık modda koyu metalik mavi font dengesi
            Color textStyle = isDark ? Color.FromArgb(230, 230, 230) : Color.FromArgb(26, 35, 126);
            Color subBg = isDark ? Color.FromArgb(44, 52, 64) : Color.FromArgb(245, 247, 250);

            GroupBox grp = new GroupBox
            {
                Text = title,
                Size = new Size(700, 80),
                BackColor = Color.Transparent, // Panelin çiğ beyaz parlamasını kesin olarak engeller
                ForeColor = textStyle,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold)
            };

            CheckBox chk = new CheckBox
            {
                Text = "Aktif",
                Location = new Point(20, 32),
                AutoSize = true,
                ForeColor = isDark ? Color.FromArgb(210, 210, 210) : Color.Black,
                Font = new Font("Segoe UI", 9F)
            };

            Label lblAddr = new Label { Text = "Modbus Adres:", Location = new Point(100, 34), AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = isDark ? Color.FromArgb(180, 180, 180) : Color.DimGray };
            NumericUpDown numAddr = new NumericUpDown { Location = new Point(200, 31), Size = new Size(80, 23), Maximum = 65535, BackColor = subBg, ForeColor = isDark ? Color.White : Color.Black };

            Label lblType = new Label { Text = "Veri Tipi:", Location = new Point(300, 34), AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = isDark ? Color.FromArgb(180, 180, 180) : Color.DimGray };
            ComboBox cmbType = new ComboBox { Location = new Point(370, 31), Size = new Size(110, 23), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = subBg, ForeColor = isDark ? Color.White : Color.Black };
            cmbType.Items.AddRange(new object[] { "Word (Int16)", "Int32", "DWord (UInt32)", "Float" });

            Label lblMult = new Label { Text = "Çarpan:", Location = new Point(500, 34), AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = isDark ? Color.FromArgb(180, 180, 180) : Color.DimGray };
            TextBox txtMult = new TextBox { Location = new Point(560, 31), Size = new Size(70, 23), Text = "1", BackColor = subBg, ForeColor = isDark ? Color.White : Color.Black, BorderStyle = BorderStyle.FixedSingle };

            grp.Controls.Add(chk);
            grp.Controls.Add(lblAddr);
            grp.Controls.Add(numAddr);
            grp.Controls.Add(lblType);
            grp.Controls.Add(cmbType);
            grp.Controls.Add(lblMult);
            grp.Controls.Add(txtMult);

            _sensorMap[key] = new SensorControls
            {
                ChkEnabled = chk,
                NumAddress = numAddr,
                CmbDataType = cmbType,
                TxtMultiplier = txtMult
            };

            flowLayoutPanelSensors.Controls.Add(grp);
        }

        private void lstLines_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstLines.SelectedItem is UtilityLine line)
            {
                _currentLine = line;
                FillForm(line);
            }
        }

        private void FillForm(UtilityLine line)
        {
            txtLineName.Text = line.LineName;
            txtIpAddress.Text = line.IpAddress;
            txtPort.Value = line.Port;
            txtSlaveId.Value = line.SlaveId;

            SetSensorValues("Water", line.WaterEnabled, line.WaterAddress, line.WaterDataType, line.WaterMultiplier);
            SetSensorValues("Elec", line.ElecEnabled, line.ElecAddress, line.ElecDataType, line.ElecMultiplier);
            SetSensorValues("Steam", line.SteamEnabled, line.SteamAddress, line.SteamDataType, line.SteamMultiplier);
            SetSensorValues("Air", line.AirEnabled, line.AirAddress, line.AirDataType, line.AirMultiplier);
        }

        private void SetSensorValues(string key, bool enabled, int addr, string type, double mult)
        {
            if (_sensorMap.TryGetValue(key, out var ctrls))
            {
                ctrls.ChkEnabled.Checked = enabled;
                ctrls.NumAddress.Value = Math.Min(ctrls.NumAddress.Maximum, Math.Max(ctrls.NumAddress.Minimum, addr));

                if (!string.IsNullOrEmpty(type) && ctrls.CmbDataType.Items.Contains(type))
                    ctrls.CmbDataType.SelectedItem = type;
                else
                    ctrls.CmbDataType.SelectedIndex = 3; // Default Float

                ctrls.TxtMultiplier.Text = mult.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void btnAddLine_Click(object sender, EventArgs e)
        {
            var newLine = new UtilityLine
            {
                LineName = "Yeni Hat",
                IpAddress = "192.168.1.100",
                Port = 502,
                SlaveId = 1
            };

            _allLines.Add(newLine);
            lstLines.Items.Add(newLine);
            lstLines.SelectedItem = newLine;
        }

        private void btnDeleteLine_Click(object sender, EventArgs e)
        {
            if (lstLines.SelectedItem is UtilityLine line)
            {
                if (MessageBox.Show($"{line.LineName} hattını silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _allLines.Remove(line);
                    lstLines.Items.Remove(line);
                    ClearForm();
                }
            }
        }

        private void ClearForm()
        {
            txtLineName.Clear();
            txtIpAddress.Clear();
            txtPort.Value = txtPort.Minimum;
            txtSlaveId.Value = txtSlaveId.Minimum;
            _currentLine = null;

            foreach (var key in _sensorMap.Keys)
            {
                SetSensorValues(key, false, 0, "Float", 1.0);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_currentLine == null) return;

            _currentLine.LineName = txtLineName.Text;
            _currentLine.IpAddress = txtIpAddress.Text;
            _currentLine.Port = (int)txtPort.Value;
            _currentLine.SlaveId = (int)txtSlaveId.Value;

            GetSensorValues("Water", out bool wEn, out int wAddr, out string wType, out double wMult);
            _currentLine.WaterEnabled = wEn; _currentLine.WaterAddress = wAddr; _currentLine.WaterDataType = wType; _currentLine.WaterMultiplier = wMult;

            GetSensorValues("Elec", out bool eEn, out int eAddr, out string eType, out double eMult);
            _currentLine.ElecEnabled = eEn; _currentLine.ElecAddress = eAddr; _currentLine.ElecDataType = eType; _currentLine.ElecMultiplier = eMult;

            GetSensorValues("Steam", out bool sEn, out int sAddr, out string sType, out double sMult);
            _currentLine.SteamEnabled = sEn; _currentLine.SteamAddress = sAddr; _currentLine.SteamDataType = sType; _currentLine.SteamMultiplier = sMult;

            GetSensorValues("Air", out bool aEn, out int aAddr, out string aType, out double aMult);
            _currentLine.AirEnabled = aEn; _currentLine.AirAddress = aAddr; _currentLine.AirDataType = aType; _currentLine.AirMultiplier = aMult;

            try
            {
                _repository.SaveUtilityLine(_currentLine);
                MessageBox.Show("Ayarlar başarıyla kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                int index = lstLines.SelectedIndex;
                if (index != -1)
                {
                    lstLines.Items[index] = _currentLine;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayıt hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GetSensorValues(string key, out bool enabled, out int addr, out string type, out double mult)
        {
            enabled = false; addr = 0; type = "Float"; mult = 1.0;

            if (_sensorMap.TryGetValue(key, out var ctrls))
            {
                enabled = ctrls.ChkEnabled.Checked;
                addr = (int)ctrls.NumAddress.Value;
                type = ctrls.CmbDataType.SelectedItem?.ToString() ?? "Float";

                // =========================================================================
                // KORUMA: KÜLTÜR BAĞIMSIZ PARSING SİGORTASI
                // Bölgesel ayarlardan (nokta/virgül ayrımı) kaynaklı sayaç kalibrasyon hatalarını kesin önler.
                // =========================================================================
                string val = ctrls.TxtMultiplier.Text.Trim().Replace(",", ".");
                if (double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result))
                {
                    mult = result;
                }
            }
        }

        // SPEED OPTİMİZASYON: GDI+ grafik arabelleğini uçuran yansıtma (Reflection) tetiği
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