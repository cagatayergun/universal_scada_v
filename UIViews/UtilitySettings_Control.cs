using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Telemetry.Models;
using Telemetry.Repositories;

namespace Telemetry.UIViews
{
    public partial class UtilitySettings_Control : UserControl
    {
        private readonly UtilityRepository _repository;
        private List<UtilityLine> _allLines;
        private UtilityLine _currentLine;

        // Dinamik sensör kontrollerini tutmak için
        private Dictionary<string, SensorControls> _sensorMap = new Dictionary<string, SensorControls>();

        private class SensorControls
        {
            public CheckBox ChkEnabled { get; set; }
            public NumericUpDown NumAddress { get; set; }
            public ComboBox CmbDataType { get; set; }
            public TextBox TxtMultiplier { get; set; } // Ondalık girmek için TextBox kullandık
        }

        public UtilitySettings_Control()
        {
            InitializeComponent();
            // Repository oluşturma (Bağımlılık enjeksiyonu varsa oradan da alabilirsiniz)
            _repository = new UtilityRepository();
        }

        private void UtilitySettings_Control_Load(object sender, EventArgs e)
        {
            LoadLines();
            CreateSensorLayouts();
        }

        private void LoadLines()
        {
            lstLines.Items.Clear();
            _allLines = _repository.GetUtilityLines();
            foreach (var line in _allLines)
            {
                lstLines.Items.Add(line); // ToString() override edilmiş olmalı: DisplayInfo
            }
            // UtilityLine sınıfında ToString() yoksa ListBox'ta class name görünür.
            // Bu yüzden DisplayMember kullanıyoruz:
            lstLines.DisplayMember = "LineName";
        }

        // Dinamik Sensör Panellerini Oluştur
        private void CreateSensorLayouts()
        {
            flowLayoutPanelSensors.Controls.Clear();
            _sensorMap.Clear();

            AddSensorGroup("Water", "Su Sayacı Ayarları", Color.AliceBlue);
            AddSensorGroup("Elec", "Elektrik Sayacı Ayarları", Color.MistyRose);
            AddSensorGroup("Steam", "Buhar Sayacı Ayarları", Color.LemonChiffon);
            AddSensorGroup("Air", "Hava Sayacı Ayarları", Color.MintCream);
        }

        private void AddSensorGroup(string key, string title, Color bgColor)
        {
            GroupBox grp = new GroupBox
            {
                Text = title,
                Size = new Size(700, 80),
                BackColor = bgColor,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            // 1. Enable Checkbox
            CheckBox chk = new CheckBox
            {
                Text = "Aktif",
                Location = new Point(20, 30),
                AutoSize = true
            };

            // 2. Adres
            Label lblAddr = new Label { Text = "Modbus Adres:", Location = new Point(100, 32), AutoSize = true, Font = new Font("Segoe UI", 9) };
            NumericUpDown numAddr = new NumericUpDown { Location = new Point(200, 30), Size = new Size(80, 23), Maximum = 65535 };

            // 3. Veri Tipi
            Label lblType = new Label { Text = "Veri Tipi:", Location = new Point(300, 32), AutoSize = true, Font = new Font("Segoe UI", 9) };
            ComboBox cmbType = new ComboBox { Location = new Point(370, 30), Size = new Size(100, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.AddRange(new object[] { "Word (Int16)", "Int32", "DWord (UInt32)", "Float" });

            // 4. Çarpan (Multiplier)
            Label lblMult = new Label { Text = "Çarpan:", Location = new Point(490, 32), AutoSize = true, Font = new Font("Segoe UI", 9) };
            TextBox txtMult = new TextBox { Location = new Point(550, 30), Size = new Size(60, 23), Text = "1" };

            grp.Controls.Add(chk);
            grp.Controls.Add(lblAddr);
            grp.Controls.Add(numAddr);
            grp.Controls.Add(lblType);
            grp.Controls.Add(cmbType);
            grp.Controls.Add(lblMult);
            grp.Controls.Add(txtMult);

            // Kontrolleri Haritaya Ekle
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
            // Genel Ayarlar
            txtLineName.Text = line.LineName;
            txtIpAddress.Text = line.IpAddress;
            txtPort.Value = line.Port;
            txtSlaveId.Value = line.SlaveId;

            // Sensör Ayarları (Refleksiyon yerine manuel atama daha güvenli ve hızlıdır)
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
                ctrls.NumAddress.Value = addr;

                // ComboBox seçimi
                if (!string.IsNullOrEmpty(type))
                    ctrls.CmbDataType.SelectedItem = type;
                else
                    ctrls.CmbDataType.SelectedIndex = 3; // Default Float

                ctrls.TxtMultiplier.Text = mult.ToString();
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

            // Veritabanına eklemeden önce hafızada listeye ekleyelim
            // (Gerçek projede önce DB'ye insert atıp ID almanız önerilir)
            // Simülasyon için:
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
                    // DB'den silme işlemi Repository'e eklenecek metot ile yapılacak:
                    // _repository.DeleteUtilityLine(line.Id);

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
            _currentLine = null;
            // Sensörleri resetle...
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_currentLine == null) return;

            // Formdan modele aktar
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

            // Veritabanına kaydet
            try
            {
                // Repository'de Update veya Save metodunu çağıracağız
                 _repository.SaveUtilityLine(_currentLine); 
                // Şimdilik sadece mesaj veriyoruz:

                MessageBox.Show("Ayarlar başarıyla kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // ListBox'ı güncelle (İsim değişmiş olabilir)
                int index = lstLines.SelectedIndex;
                lstLines.Items[index] = _currentLine;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayıt hatası: " + ex.Message);
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

                // Virgül kontrolü (Nokta veya virgül ayrımını sisteme göre yap)
                string val = ctrls.TxtMultiplier.Text.Replace(",", ".");
                if (double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result))
                {
                    mult = result;
                }
            }
        }
    }
}