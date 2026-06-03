// UIViews/RecipeTypeSelection_Form.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Telemetry.Core;
using Telemetry.UI.Views;
using MaterialSkin;          // YENİ EKLENDİ
using MaterialSkin.Controls; // YENİ EKLENDİ

namespace Telemetry.UIViews
{
    public partial class RecipeTypeSelection_Form : MaterialForm // Form'dan MaterialForm'a yükseltildi
    {
        public string SelectedType { get; private set; }

        public RecipeTypeSelection_Form(List<string> machineTypes)
        {
            InitializeComponent();

            // SPEED OPTİMİZASYON: Pencere animasyonlu açılırken veya taşınırken kırpışmayı önler
            this.DoubleBuffered = true;
            this.FormStyle = FormStyles.ActionBar_None; // Dialog pencerelerinde çift başlık barını önler

            // ComboBox'ı doldur
            cmbRecipeType.DataSource = machineTypes;
            if (cmbRecipeType.Items.Count > 0)
            {
                cmbRecipeType.SelectedIndex = 0;
            }

            makinetip.Text = "Select Machine Type";

            // Koyu/Açık mod uyumu için standart ComboBox renklerini eşitle
            ConfigureThemeColors();
        }

        // =========================================================================
        // MODERNİZASYON: STANDART SEÇİM KUTUSUNU DARK MODE ADAPTÖRÜ
        // Açılır kutunun arka planını merkezi temayla tam olarak eşitler.
        // =========================================================================
        private void ConfigureThemeColors()
        {
            bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;
            Color controlBg = isDark ? Color.FromArgb(44, 52, 64) : Color.FromArgb(241, 245, 249);
            Color controlFg = isDark ? Color.FromArgb(240, 240, 240) : Color.FromArgb(51, 65, 85);

            if (cmbRecipeType != null)
            {
                cmbRecipeType.BackColor = controlBg;
                cmbRecipeType.ForeColor = controlFg;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (cmbRecipeType.SelectedItem != null)
            {
                SelectedType = cmbRecipeType.SelectedItem.ToString();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a prescription type.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}