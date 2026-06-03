using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Telemetry.Models;
using MaterialSkin;          // YENİ EKLENDİ: MaterialSkin ana kütüphanesi
using MaterialSkin.Controls; // YENİ EKLENDİ: Material bileşen desteği

namespace Telemetry.UI.Controls
{
    public partial class KurutmaReçete_Control : UserControl
    {
        private ScadaRecipeStep _recipeStep;

        // YÜKLEME OPTMİZASYONU: Yükleme sırasında olay zincirini durdurmak için bayrak
        private bool _isLoading = false;

        public event EventHandler ValueChanged;

        public KurutmaReçete_Control()
        {
            InitializeComponent();

            // SPEED OPTİMİZASYON: Sekme geçişlerinde ve reçete yüklemelerinde arayüzün titremesini engeller
            this.DoubleBuffered = true;

            // =========================================================================
            // MODERNİZASYON: NUMERICUPDOWN BİLEŞENLERİNİ DARK MODE UYUMLU YAPMA
            // Material kütüphanesinde sayı kutusu olmadığı için standart kutuları kodla özelleştiriyoruz.
            // =========================================================================
            Color numericBg = Color.FromArgb(44, 52, 64);    // Modern koyu mavi-gri arka plan
            Color numericFg = Color.FromArgb(240, 240, 240); // Net okunabilir beyaz yazı

            var numericControls = new List<NumericUpDown> { numSicaklik, numNem, numZaman, numCalismaDevri, numSogutmaZamani };
            foreach (var num in numericControls)
            {
                if (num != null)
                {
                    num.BackColor = numericBg;
                    num.ForeColor = numericFg;
                    num.BorderStyle = BorderStyle.FixedSingle;
                }
            }

            // Olayları bağlıyoruz (Event Wiring)
            numSicaklik.ValueChanged += OnValueChanged;
            numNem.ValueChanged += OnValueChanged;
            numZaman.ValueChanged += OnValueChanged;
            numCalismaDevri.ValueChanged += OnValueChanged;
            numSogutmaZamani.ValueChanged += OnValueChanged;
            chkNemAktif.CheckedChanged += OnValueChanged;
            chkZamanAktif.CheckedChanged += OnValueChanged;
        }

        public void LoadRecipe(ScadaRecipe recipe)
        {
            if (recipe != null && recipe.Steps.Count > 0)
            {
                // Yükleme başladı, olayları kilitle
                _isLoading = true;

                // SPEED OPTİMİZASYON: Tüm değerler basılana kadar arayüz yerleşim hesaplamalarını askıya al
                this.SuspendLayout();

                try
                {
                    _recipeStep = recipe.Steps[0];
                    var kurutmaParams = new KurutmaParams(_recipeStep.StepDataWords);

                    // Değerleri PLC hafıza haritasına göre kontrollere ata
                    numSicaklik.Value = kurutmaParams.Temperature;
                    numNem.Value = kurutmaParams.Humidity;
                    numZaman.Value = kurutmaParams.DurationMinutes;
                    numCalismaDevri.Value = kurutmaParams.Rpm;
                    numSogutmaZamani.Value = kurutmaParams.CoolingTimeMinutes;

                    // Kontrol bitlerini oku
                    chkNemAktif.Checked = kurutmaParams.HumidityControlActive;
                    chkZamanAktif.Checked = kurutmaParams.TimeControlActive;
                }
                finally
                {
                    // Değişiklikleri tek bir karede (frame) ekrana toplu olarak çiz
                    this.ResumeLayout(true);

                    // Yükleme bitti veya hata oldu, kilidi her durumda aç
                    _isLoading = false;
                }
            }
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            // Eğer veri yükleniyorsa veya reçete boşsa işlem yapma! (Gereksiz döngüleri önler)
            if (_isLoading || _recipeStep == null) return;

            var kurutmaParams = new KurutmaParams(_recipeStep.StepDataWords);

            // Değişiklikleri anında _recipeStep nesnesine kaydet
            kurutmaParams.Temperature = (short)numSicaklik.Value;
            kurutmaParams.Humidity = (short)numNem.Value;
            kurutmaParams.DurationMinutes = (short)numZaman.Value;
            kurutmaParams.Rpm = (short)numCalismaDevri.Value;
            kurutmaParams.CoolingTimeMinutes = (short)numSogutmaZamani.Value;

            // Kontrol bitlerini yaz
            kurutmaParams.HumidityControlActive = chkNemAktif.Checked;
            kurutmaParams.TimeControlActive = chkZamanAktif.Checked;

            // Değişiklik olayını (event) tetikle
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}