// UI/Views/RecipeStepDesigner_Control.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telemetry.Core;
using Telemetry.Models;
using Telemetry.Repositories;
using MaterialSkin;          // YENİ EKLENDİ
using MaterialSkin.Controls; // YENİ EKLENDİ

namespace Telemetry.UI.Views
{
    public partial class RecipeStepDesigner_Control : UserControl
    {
        private Control _activeControl;
        private Point _previousLocation;
        private readonly RecipeConfigurationRepository _configRepo = new RecipeConfigurationRepository();

        public RecipeStepDesigner_Control()
        {
            InitializeComponent();

            // SPEED OPTİMİZASYON: Tasarım panelinin element geçişlerinde titremesini sıfırlar
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;

            // Tasarım yüzeyinin sürükle-bırak esnasında anlık kırpışmasını (flickering) önleyen donanımsal önbellek
            EnableDoubleBuffer(pnlDesignSurface);

            // Olayları bağla
            pnlDesignSurface.DragEnter += PnlDesignSurface_DragEnter;
            pnlDesignSurface.DragDrop += PnlDesignSurface_DragDrop;
            pnlDesignSurface.Paint += PnlDesignSurface_Paint;
            pnlDesignSurface.Click += (s, e) => SelectControl(null);

            btnLabel.MouseDown += Toolbox_MouseDown;
            btnNumeric.MouseDown += Toolbox_MouseDown;
            btnCheckbox.MouseDown += Toolbox_MouseDown;
            btnSaveLayout.Click += BtnSaveLayout_Click;
            btnNewLayout.Click += BtnNewLayout_Click;

            cmbMachineSubType.SelectedIndexChanged += LoadLayoutForSelection;
            cmbStepType.SelectedIndexChanged += LoadLayoutForSelection;

            // DÜZELTME: Mükerrer olan btnTextbox.MouseDown olay aboneliği teke düşürüldü.
            btnTextbox.MouseDown += Toolbox_MouseDown;

            // =========================================================================
            // MODERNİZASYON: STANDART KONTROLLERİ DARK MODE UYARLAMA ADAPTÖRÜ
            // Standart seçim ve mülkiyet pencerelerini mat koyu temayla kusursuz eşitler.
            // =========================================================================
            Color controlBg = Color.FromArgb(44, 52, 64);    // Koyu grafit gri
            Color controlFg = Color.FromArgb(240, 240, 240); // Soft mat beyaz

            if (cmbMachineSubType != null) { cmbMachineSubType.BackColor = controlBg; cmbMachineSubType.ForeColor = controlFg; }
            if (cmbStepType != null) { cmbStepType.BackColor = controlBg; cmbStepType.ForeColor = controlFg; }
            if (propertyGrid != null)
            {
                propertyGrid.BackColor = controlBg;
                propertyGrid.HelpBackColor = controlBg;
                propertyGrid.LineColor = Color.FromArgb(71, 85, 105);
                propertyGrid.CategoryForeColor = controlFg;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!this.DesignMode)
            {
                LoadComboBoxes();
            }
        }

        private void LoadComboBoxes()
        {
            cmbMachineSubType.DataSource = _configRepo.GetMachineSubTypes();

            cmbStepType.DataSource = _configRepo.GetStepTypes();
            cmbStepType.DisplayMember = "StepName";
            cmbStepType.ValueMember = "Id";
        }

        private void LoadLayoutForSelection(object sender, EventArgs e)
        {
            if (cmbMachineSubType.SelectedItem == null || cmbStepType.SelectedItem == null) return;

            string machineSubType = cmbMachineSubType.SelectedItem.ToString();

            DataRowView selectedStepTypeRow = cmbStepType.SelectedItem as DataRowView;
            if (selectedStepTypeRow == null) return;
            int stepTypeId = Convert.ToInt32(selectedStepTypeRow["Id"]);

            string layoutJson = _configRepo.GetLayoutJson(machineSubType, stepTypeId);

            pnlDesignSurface.SuspendLayout(); // Toplu çizim yüklemesi için yerleşim motorunu dondur
            try
            {
                pnlDesignSurface.Controls.Clear();
                SelectControl(null);

                if (!string.IsNullOrEmpty(layoutJson))
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var controlsData = JsonSerializer.Deserialize<List<ControlMetadata>>(layoutJson, options);
                    foreach (var data in controlsData)
                    {
                        CreateControlFromJson(data);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading design: {ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                pnlDesignSurface.ResumeLayout(true);
            }
        }

        private async void BtnSaveLayout_Click(object sender, EventArgs e)
        {
            if (cmbMachineSubType.SelectedItem == null || cmbStepType.SelectedValue == null)
            {
                MessageBox.Show("Please select a machine subtype and step type.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string machineSubType = cmbMachineSubType.SelectedItem.ToString();
            int stepTypeId = Convert.ToInt32(cmbStepType.SelectedValue);
            string layoutName = $"{machineSubType} - {cmbStepType.Text}";

            this.Cursor = Cursors.WaitCursor;
            btnSaveLayout.Enabled = false;

            try
            {
                var controlsMetadata = new List<ControlMetadata>();
                foreach (Control control in pnlDesignSurface.Controls)
                {
                    var wrapper = new ControlPropertyWrapper(control);
                    var metadata = new ControlMetadata
                    {
                        ControlType = control.GetType().AssemblyQualifiedName,
                        Name = wrapper.Name,
                        Text = wrapper.Text,
                        Location = $"{wrapper.Location.X}, {wrapper.Location.Y}",
                        Size = $"{wrapper.Size.Width}, {wrapper.Size.Height}",
                        PLC_WordIndex = wrapper.PLC_WordIndex,
                        PLC_BitIndex = wrapper.PLC_BitIndex
                    };

                    if (control is NumericUpDown num)
                    {
                        metadata.Maximum = num.Maximum;
                        metadata.Minimum = num.Minimum;
                        metadata.DecimalPlaces = num.DecimalPlaces;
                    }

                    controlsMetadata.Add(metadata);
                }

                string jsonLayout = JsonSerializer.Serialize(controlsMetadata, new JsonSerializerOptions { WriteIndented = true });

                // Veritabanı disk yazma görevini arka planda asenkron olarak yürütüyoruz
                await Task.Run(() => _configRepo.SaveLayout(layoutName, machineSubType, stepTypeId, jsonLayout));
                MessageBox.Show("Interface design saved successfully!", "Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving design: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnSaveLayout.Enabled = true;
            }
        }

        private void BtnNewLayout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("The current design will be cleared. Are you sure?", "New Design", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                pnlDesignSurface.Controls.Clear();
                SelectControl(null);
            }
        }

        private void CreateControlFromJson(ControlMetadata data)
        {
            Type controlType = Type.GetType(data.ControlType);
            if (controlType == null) return;

            Control newControl = (Control)Activator.CreateInstance(controlType);
            var wrapper = new ControlPropertyWrapper(newControl);

            wrapper.Name = data.Name;
            wrapper.Text = data.Text;
            wrapper.Location = new Point(int.Parse(data.Location.Split(',')[0].Trim()), int.Parse(data.Location.Split(',')[1].Trim()));
            wrapper.Size = new Size(int.Parse(data.Size.Split(',')[0].Trim()), int.Parse(data.Size.Split(',')[1].Trim()));
            wrapper.PLC_WordIndex = data.PLC_WordIndex;
            wrapper.PLC_BitIndex = data.PLC_BitIndex;

            if (newControl is NumericUpDown num)
            {
                num.Maximum = data.Maximum;
                num.Minimum = data.Minimum;
                num.DecimalPlaces = data.DecimalPlaces;
            }

            newControl.MouseDown += Control_MouseDown;
            newControl.MouseMove += Control_MouseMove;
            newControl.MouseUp += Control_MouseUp;
            newControl.KeyDown += Control_KeyDown;
            pnlDesignSurface.Controls.Add(newControl);
        }

        private void Toolbox_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is Control control && control.Tag is Type type)
            {
                control.DoDragDrop(type.AssemblyQualifiedName, DragDropEffects.Copy);
            }
        }

        private void PnlDesignSurface_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text)) e.Effect = DragDropEffects.Copy;
        }

        private void PnlDesignSurface_DragDrop(object sender, DragEventArgs e)
        {
            string typeName = (string)e.Data.GetData(DataFormats.Text);
            Type controlType = Type.GetType(typeName);
            if (controlType != null)
            {
                Control newControl = (Control)Activator.CreateInstance(controlType);
                newControl.Location = pnlDesignSurface.PointToClient(new Point(e.X, e.Y));
                newControl.Tag = new PlcMapping();

                newControl.MouseDown += Control_MouseDown;
                newControl.MouseMove += Control_MouseMove;
                newControl.MouseUp += Control_MouseUp;
                newControl.KeyDown += Control_KeyDown;
                pnlDesignSurface.Controls.Add(newControl);
                SelectControl(newControl);
            }
        }

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _activeControl = sender as Control;
                if (_activeControl != null)
                {
                    _activeControl.Focus();
                    _previousLocation = e.Location;
                    SelectControl(_activeControl);
                }
            }
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _activeControl != null)
            {
                _activeControl.Left += e.X - _previousLocation.X;
                _activeControl.Top += e.Y - _previousLocation.Y;
                pnlDesignSurface.Invalidate(); // Çerçevenin akıcı kayması için yüzeyi tetikle
            }
        }

        private void Control_MouseUp(object sender, MouseEventArgs e)
        {
            _activeControl = null;
            propertyGrid.Refresh();
            pnlDesignSurface.Invalidate();
        }

        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && _activeControl != null)
            {
                pnlDesignSurface.Controls.Remove(_activeControl);
                SelectControl(null);
                pnlDesignSurface.Invalidate();
            }
        }

        private void SelectControl(Control control)
        {
            _activeControl = control;
            propertyGrid.SelectedObject = new ControlPropertyWrapper(control);
            pnlDesignSurface.Invalidate();
        }

        // =========================================================================
        // MODERNİZASYON: WYSIWYG ELEMAN SEÇİM ETİKET BOYAYICI (PAINT)
        // Koyu mod şemasında kaybolmayan material temalı kesikli çerçeve mühürlendi.
        // =========================================================================
        private void PnlDesignSurface_Paint(object sender, PaintEventArgs e)
        {
            if (_activeControl != null && !pnlDesignSurface.IsDisposed)
            {
                Rectangle rect = _activeControl.Bounds;
                rect.Inflate(2, 2);

                bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;
                // Koyu modda belirgin açık material mavi, açık modda koyu material lacivert fırça
                Color highlightColor = isDark ? Color.FromArgb(33, 150, 243) : Color.FromArgb(26, 35, 126);

                using (Pen customPen = new Pen(highlightColor, 1.5f))
                {
                    customPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawRectangle(customPen, rect);
                }
            }
        }

        public class ControlPropertyWrapper
        {
            private readonly Control _control;
            private readonly PlcMapping _mapping;

            public ControlPropertyWrapper(Control control)
            {
                if (control == null)
                {
                    _control = null;
                    _mapping = null;
                    return;
                }

                _control = control;
                _mapping = _control.Tag as PlcMapping ?? new PlcMapping();
                _control.Tag = _mapping;
            }

            [Category("Tasarım")]
            [DisplayName("Ondalık Basamak Sayısı")]
            public int DecimalPlaces
            {
                get => (_control as NumericUpDown)?.DecimalPlaces ?? 0;
                set { if (_control is NumericUpDown num) num.DecimalPlaces = value; }
            }

            [Category("Tasarım")]
            public string Name
            {
                get => _control?.Name ?? string.Empty;
                set { if (_control != null) _control.Name = value; }
            }

            [Category("Tasarım")]
            public string Text
            {
                get => _control?.Text ?? string.Empty;
                set { if (_control != null) _control.Text = value; }
            }

            [Category("Tasarım")]
            public Point Location
            {
                get => _control?.Location ?? Point.Empty;
                set { if (_control != null) _control.Location = value; }
            }

            [Category("Tasarım")]
            public Size Size
            {
                get => _control?.Size ?? Size.Empty;
                set { if (_control != null) _control.Size = value; }
            }

            [Category("PLC Eşleme")]
            [DisplayName("PLC Word Index")]
            public int PLC_WordIndex
            {
                get => _mapping?.WordIndex ?? 0;
                set { if (_mapping != null) _mapping.WordIndex = value; }
            }

            [Category("PLC Eşleme")]
            [DisplayName("PLC Bit Index")]
            public int PLC_BitIndex
            {
                get => _mapping?.BitIndex ?? 0;
                set { if (_control is CheckBox && _mapping != null) _mapping.BitIndex = value; }
            }

            [Category("PLC Eşleme")]
            [DisplayName("String Word Uzunluğu")]
            public int PLC_StringWordLength
            {
                get => _mapping?.StringWordLength ?? 0;
                set { if (_control is TextBox && _mapping != null) _mapping.StringWordLength = value; }
            }

            [Category("Tasarım")]
            [DisplayName("Maksimum Değer")]
            public decimal Maximum
            {
                get => (_control as NumericUpDown)?.Maximum ?? 100;
                set { if (_control is NumericUpDown num) num.Maximum = value; }
            }

            [Category("Tasarım")]
            [DisplayName("Minimum Değer")]
            public decimal Minimum
            {
                get => (_control as NumericUpDown)?.Minimum ?? 0;
                set { if (_control is NumericUpDown num) num.Minimum = value; }
            }
        }

        // SPEED OPTİMİZASYON: GDI+ render kuyruğunu hızlandıran yansıtma (Reflection) metodu
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