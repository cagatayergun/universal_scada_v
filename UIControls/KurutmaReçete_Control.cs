using System;
using System.Windows.Forms;
using TekstilScada.Models;

namespace TekstilScada.UI.Controls
{
    public partial class KurutmaReçete_Control : UserControl
    {
        private ScadaRecipeStep _recipeStep;

        // YENİ: Yükleme sırasında olayları durdurmak için bayrak
        private bool _isLoading = false;

        public event EventHandler ValueChanged;

        public KurutmaReçete_Control()
        {
            InitializeComponent();

            // Olayları bağlıyoruz
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

                try
                {
                    _recipeStep = recipe.Steps[0];
                    var kurutmaParams = new KurutmaParams(_recipeStep.StepDataWords);

                    // Değerleri PLC hafıza haritasına göre kontrollerden oku
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
                    // Yükleme bitti veya hata oldu, kilidi her durumda aç
                    _isLoading = false;
                }
            }
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            // Eğer veri yükleniyorsa veya reçete boşsa işlem yapma!
            if (_isLoading || _recipeStep == null) return;

            var kurutmaParams = new KurutmaParams(_recipeStep.StepDataWords);

            // Değişiklikleri anında _recipeStep nesnesine kaydet
            kurutmaParams.Temperature = (short)(numSicaklik.Value );
            kurutmaParams.Humidity = (short)numNem.Value;
            kurutmaParams.DurationMinutes = (short)numZaman.Value;
            kurutmaParams.Rpm = (short)numCalismaDevri.Value;
            kurutmaParams.CoolingTimeMinutes = (short)numSogutmaZamani.Value;

            // Kontrol bitlerini yaz
            kurutmaParams.HumidityControlActive = chkNemAktif.Checked;
            kurutmaParams.TimeControlActive = chkZamanAktif.Checked;

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}