using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using TekstilScada.Core;
using TekstilScada.Models;
using TekstilScada.Repositories;

namespace TekstilScada.UI.Views
{
    public partial class RecipeStepDesigner_Control : UserControl
    {
        // --- ENUMLAR ---
        private enum HitTest { None, Body, TopLeft, TopRight, BottomLeft, BottomRight }
        public enum CustomButtonStyle { Kabartma, Solid } // Türkçe seçenekler için enum

        // --- SABİTLER ---
        private const int GridSize = 20;
        private const int HandleSize = 8;

        // --- DEĞİŞKENLER ---
        private Control _activeControl;
        private Point _dragStartPoint;
        private Rectangle _startBounds;
        private HitTest _currentHitTest = HitTest.None;
        private bool _isDraggingOrResizing = false;
        private string _clipboardJson = null;

        private readonly RecipeConfigurationRepository _configRepo = new RecipeConfigurationRepository();
        private ContextMenuStrip _contextMenu;

        public RecipeStepDesigner_Control()
        {
            InitializeComponent();
            InitializeDesigner();
            InitializeContextMenu();
        }

        private void InitializeDesigner()
        {
            if (pnlDesignSurface != null)
            {
                typeof(Panel).InvokeMember("DoubleBuffered",
                    BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, pnlDesignSurface, new object[] { true });
            }

            pnlDesignSurface.DragEnter += PnlDesignSurface_DragEnter;
            pnlDesignSurface.DragDrop += PnlDesignSurface_DragDrop;
            pnlDesignSurface.Paint += PnlDesignSurface_Paint;
            pnlDesignSurface.MouseDown += PnlDesignSurface_MouseDown;
            pnlDesignSurface.MouseMove += PnlDesignSurface_MouseMove;
            pnlDesignSurface.MouseUp += PnlDesignSurface_MouseUp;

            tsbNew.Click += (s, e) => ClearLayout();
            tsbSave.Click += BtnSaveLayout_Click;
            tsbCopy.Click += (s, e) => CopyControl();
            tsbPaste.Click += (s, e) => PasteControl();
            tsbDelete.Click += (s, e) => DeleteActiveControl();

            BindToolboxEvents();

            tsCmbMachineType.SelectedIndexChanged += LoadLayoutForSelection;
            tsCmbStepType.SelectedIndexChanged += LoadLayoutForSelection;
        }

        private void InitializeContextMenu()
        {
            _contextMenu = new ContextMenuStrip();

            var itemFront = new ToolStripMenuItem("En Öne Getir");
            itemFront.Image = SystemIcons.Shield.ToBitmap();
            itemFront.Click += (s, e) => { _activeControl?.BringToFront(); pnlDesignSurface.Invalidate(); };
            _contextMenu.Items.Add(itemFront);

            var itemBack = new ToolStripMenuItem("En Arkaya Gönder");
            itemBack.Click += (s, e) => { _activeControl?.SendToBack(); pnlDesignSurface.Invalidate(); };
            _contextMenu.Items.Add(itemBack);

            _contextMenu.Items.Add(new ToolStripSeparator());

            var itemDel = new ToolStripMenuItem("Sil");
            itemDel.ShortcutKeys = Keys.Delete;
            itemDel.Click += (s, e) => DeleteActiveControl();
            _contextMenu.Items.Add(itemDel);
        }

        private void BindToolboxEvents()
        {
            btnLabel.MouseDown += Toolbox_MouseDown;
            btnNumeric.MouseDown += Toolbox_MouseDown;
            btnCheckbox.MouseDown += Toolbox_MouseDown;
            btnTextbox.MouseDown += Toolbox_MouseDown;
            btnButton.MouseDown += Toolbox_MouseDown;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!this.DesignMode) LoadComboBoxes();
        }

        private void LoadComboBoxes()
        {
            var machineTypes = _configRepo.GetMachineSubTypes();
            tsCmbMachineType.Items.Clear();
            foreach (var type in machineTypes) tsCmbMachineType.Items.Add(type);

            var steps = _configRepo.GetStepTypes();
            tsCmbStepType.ComboBox.DataSource = steps;
            tsCmbStepType.ComboBox.DisplayMember = "StepName";
            tsCmbStepType.ComboBox.ValueMember = "Id";
        }

        // --- ÇİZİM ---
        private void PnlDesignSurface_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            DrawGrid(g);

            foreach (Control ctrl in pnlDesignSurface.Controls)
            {
                if (ctrl == _activeControl) continue;
                using (Pen pen = new Pen(Color.FromArgb(220, 220, 220)))
                {
                    pen.DashStyle = DashStyle.Dot;
                    Rectangle r = ctrl.Bounds; r.Inflate(1, 1);
                    g.DrawRectangle(pen, r);
                }
            }

            if (_activeControl != null)
            {
                Rectangle rect = _activeControl.Bounds;
                using (Pen pen = new Pen(Color.FromArgb(0, 122, 204), 1))
                {
                    g.DrawRectangle(pen, rect.X - 1, rect.Y - 1, rect.Width + 1, rect.Height + 1);
                }
                DrawHandles(g, rect);
            }
        }

        private void DrawGrid(Graphics g)
        {
            using (Brush brush = new SolidBrush(Color.FromArgb(235, 235, 240)))
            {
                for (int x = 0; x < pnlDesignSurface.Width; x += GridSize)
                    for (int y = 0; y < pnlDesignSurface.Height; y += GridSize)
                        g.FillRectangle(brush, x, y, 2, 2);
            }
        }

        private void DrawHandles(Graphics g, Rectangle rect)
        {
            DrawSingleHandle(g, rect.Left, rect.Top);
            DrawSingleHandle(g, rect.Right, rect.Top);
            DrawSingleHandle(g, rect.Left, rect.Bottom);
            DrawSingleHandle(g, rect.Right, rect.Bottom);
        }

        private void DrawSingleHandle(Graphics g, int x, int y)
        {
            var r = new Rectangle(x - HandleSize / 2, y - HandleSize / 2, HandleSize, HandleSize);
            g.FillRectangle(Brushes.White, r);
            using (Pen p = new Pen(Color.FromArgb(0, 122, 204))) g.DrawRectangle(p, r);
        }

        // --- FARE ---
        private void PnlDesignSurface_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_activeControl != null)
                {
                    _currentHitTest = CheckHitTest(e.Location);
                    if (_currentHitTest != HitTest.None && _currentHitTest != HitTest.Body)
                    {
                        _isDraggingOrResizing = true;
                        _dragStartPoint = e.Location;
                        _startBounds = _activeControl.Bounds;
                        return;
                    }
                }
                SelectControl(null);
            }
        }

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            Control clickedCtrl = sender as Control;
            if (e.Button == MouseButtons.Left)
            {
                SelectControl(clickedCtrl);
                _activeControl.BringToFront();
                _currentHitTest = HitTest.Body;
                _isDraggingOrResizing = true;
                _dragStartPoint = Cursor.Position;
                _startBounds = _activeControl.Bounds;
            }
            else if (e.Button == MouseButtons.Right)
            {
                SelectControl(clickedCtrl);
                _contextMenu.Show(Cursor.Position);
            }
        }

        private void PnlDesignSurface_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingOrResizing)
            {
                var hit = (_activeControl != null) ? CheckHitTest(e.Location) : HitTest.None;
                SetCursor(hit);
            }
            else if (_activeControl != null && _currentHitTest != HitTest.Body)
            {
                int dx = e.X - _dragStartPoint.X;
                int dy = e.Y - _dragStartPoint.Y;
                Rectangle newBounds = _startBounds;

                if (_currentHitTest == HitTest.BottomRight) { newBounds.Width += dx; newBounds.Height += dy; }
                else if (_currentHitTest == HitTest.BottomLeft) { newBounds.X += dx; newBounds.Width -= dx; newBounds.Height += dy; }
                else if (_currentHitTest == HitTest.TopRight) { newBounds.Y += dy; newBounds.Width += dx; newBounds.Height -= dy; }
                else if (_currentHitTest == HitTest.TopLeft) { newBounds.X += dx; newBounds.Y += dy; newBounds.Width -= dx; newBounds.Height -= dy; }

                newBounds.Width = Math.Max(GridSize, Snap(newBounds.Width));
                newBounds.Height = Math.Max(GridSize, Snap(newBounds.Height));

                if (_currentHitTest.ToString().Contains("Left")) newBounds.X = Snap(newBounds.X);
                if (_currentHitTest.ToString().Contains("Top")) newBounds.Y = Snap(newBounds.Y);

                _activeControl.Bounds = newBounds;
                UpdateStatus();
                pnlDesignSurface.Invalidate();
            }
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingOrResizing && _currentHitTest == HitTest.Body && _activeControl != null)
            {
                int dx = Cursor.Position.X - _dragStartPoint.X;
                int dy = Cursor.Position.Y - _dragStartPoint.Y;
                int newX = Snap(_startBounds.X + dx);
                int newY = Snap(_startBounds.Y + dy);
                _activeControl.Location = new Point(newX, newY);
                UpdateStatus();
                pnlDesignSurface.Invalidate();
            }
        }

        private void PnlDesignSurface_MouseUp(object sender, MouseEventArgs e) => FinishOp();
        private void Control_MouseUp(object sender, MouseEventArgs e) => FinishOp();

        private void FinishOp()
        {
            _isDraggingOrResizing = false;
            _currentHitTest = HitTest.None;
            propertyGrid.Refresh();
            pnlDesignSurface.Invalidate();
        }

        private HitTest CheckHitTest(Point p)
        {
            if (_activeControl == null) return HitTest.None;
            Rectangle r = _activeControl.Bounds;
            if (IsOverHandle(r.Left, r.Top, p)) return HitTest.TopLeft;
            if (IsOverHandle(r.Right, r.Top, p)) return HitTest.TopRight;
            if (IsOverHandle(r.Left, r.Bottom, p)) return HitTest.BottomLeft;
            if (IsOverHandle(r.Right, r.Bottom, p)) return HitTest.BottomRight;
            if (r.Contains(p)) return HitTest.Body;
            return HitTest.None;
        }

        private bool IsOverHandle(int x, int y, Point p) => new Rectangle(x - HandleSize / 2, y - HandleSize / 2, HandleSize, HandleSize).Contains(p);

        private void SetCursor(HitTest hit)
        {
            if (hit == HitTest.TopLeft || hit == HitTest.BottomRight) Cursor.Current = Cursors.SizeNWSE;
            else if (hit == HitTest.TopRight || hit == HitTest.BottomLeft) Cursor.Current = Cursors.SizeNESW;
            else if (hit == HitTest.Body) Cursor.Current = Cursors.SizeAll;
            else Cursor.Current = Cursors.Default;
        }

        private int Snap(int val) => (int)(Math.Round((double)val / GridSize) * GridSize);

        private void SelectControl(Control control)
        {
            _activeControl = control;
            pnlDesignSurface.Invalidate();
            propertyGrid.SelectedObject = control != null ? new ControlPropertyWrapper(control) : null;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (_activeControl != null)
            {
                lblStatusReady.Text = $"Seçili: {_activeControl.Name}";
                lblStatusPosition.Text = $"Konum: {_activeControl.Left}, {_activeControl.Top}";
                lblStatusSize.Text = $"Boyut: {_activeControl.Width} x {_activeControl.Height}";
            }
            else
            {
                lblStatusReady.Text = "Hazır";
                lblStatusPosition.Text = "";
                lblStatusSize.Text = "";
            }
        }

        private void CopyControl()
        {
            if (_activeControl == null) return;
            var metadata = CreateMetadataFromControl(_activeControl);
            _clipboardJson = JsonSerializer.Serialize(metadata);
        }

        private void PasteControl()
        {
            if (string.IsNullOrEmpty(_clipboardJson)) return;
            try
            {
                var metadata = JsonSerializer.Deserialize<ControlMetadata>(_clipboardJson);
                metadata.Name += "_" + DateTime.Now.Ticks.ToString().Substring(10);
                CreateControlFromJson(metadata, true);
            }
            catch { }
        }

        private void DeleteActiveControl()
        {
            if (_activeControl != null)
            {
                pnlDesignSurface.Controls.Remove(_activeControl);
                SelectControl(null);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.C)) { CopyControl(); return true; }
            if (keyData == (Keys.Control | Keys.V)) { PasteControl(); return true; }
            if (keyData == (Keys.Control | Keys.D)) { CopyControl(); PasteControl(); return true; }
            if (keyData == Keys.Delete) { DeleteActiveControl(); return true; }

            if (_activeControl != null)
            {
                int step = (keyData & Keys.Shift) == Keys.Shift ? GridSize : 1;
                switch (keyData & Keys.KeyCode)
                {
                    case Keys.Left: _activeControl.Left -= step; break;
                    case Keys.Right: _activeControl.Left += step; break;
                    case Keys.Up: _activeControl.Top -= step; break;
                    case Keys.Down: _activeControl.Top += step; break;
                    default: return base.ProcessCmdKey(ref msg, keyData);
                }
                UpdateStatus();
                pnlDesignSurface.Invalidate();
                propertyGrid.Refresh();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Toolbox_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is Control c && c.Tag is Type t) c.DoDragDrop(t.AssemblyQualifiedName, DragDropEffects.Copy);
        }

        private void PnlDesignSurface_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text)) e.Effect = DragDropEffects.Copy;
        }

        private void PnlDesignSurface_DragDrop(object sender, DragEventArgs e)
        {
            string typeName = (string)e.Data.GetData(DataFormats.Text);
            var meta = new ControlMetadata
            {
                ControlType = typeName,
                Name = "Control_" + DateTime.Now.Ticks.ToString().Substring(12),
                Text = "Yeni Kontrol",
                Location = "0,0",
                Size = "100,25"
            };
            Point p = pnlDesignSurface.PointToClient(new Point(e.X, e.Y));
            meta.Location = $"{Snap(p.X)},{Snap(p.Y)}";
            CreateControlFromJson(meta);
        }

        // --- JSON / LOAD / SAVE ---

        private ControlMetadata CreateMetadataFromControl(Control ctrl)
        {
            var wrapper = new ControlPropertyWrapper(ctrl);
            var meta = new ControlMetadata
            {
                ControlType = ctrl.GetType().AssemblyQualifiedName,
                Name = ctrl.Name,
                Text = ctrl.Text,
                Location = $"{ctrl.Left}, {ctrl.Top}",
                Size = $"{ctrl.Width}, {ctrl.Height}",
                PLC_WordIndex = wrapper.PLC_WordIndex,
                PLC_BitIndex = wrapper.PLC_BitIndex,
                BackColor = ColorTranslator.ToHtml(ctrl.BackColor),
                ForeColor = ColorTranslator.ToHtml(ctrl.ForeColor),
                FontSize = ctrl.Font.Size,
                FontBold = ctrl.Font.Bold,
                // --- YENİ EKLENEN ÖZELLİKLER KAYDEDİLİYOR ---
                IsToggleButton = wrapper.IsToggleButton,
                PressedText = wrapper.PressedText,
                PressedBackColor = ColorTranslator.ToHtml(wrapper.PressedBackColor),
                PressedForeColor = ColorTranslator.ToHtml(wrapper.PressedForeColor),
                ButtonStyle = wrapper.ButtonStyle.ToString() // Enum'ı string'e çevir
            };

            if (ctrl is Label lbl) meta.ContentAlignment = lbl.TextAlign.ToString();
            else if (ctrl is Button btn) meta.ContentAlignment = btn.TextAlign.ToString();
            else if (ctrl is TextBox txt) meta.HorizontalAlignment = txt.TextAlign.ToString();
            else if (ctrl is NumericUpDown num) meta.HorizontalAlignment = num.TextAlign.ToString();

            if (ctrl is NumericUpDown numControl)
            {
                meta.Maximum = numControl.Maximum;
                meta.Minimum = numControl.Minimum;
                meta.DecimalPlaces = numControl.DecimalPlaces;
            }
            return meta;
        }

        private void CreateControlFromJson(ControlMetadata data, bool applyOffset = false)
        {
            Type type = Type.GetType(data.ControlType);
            if (type == null) return;

            Control ctrl = (Control)Activator.CreateInstance(type);
            var wrapper = new ControlPropertyWrapper(ctrl);

            wrapper.Name = data.Name;
            wrapper.Text = data.Text;

            if (!string.IsNullOrEmpty(data.BackColor)) ctrl.BackColor = ColorTranslator.FromHtml(data.BackColor);
            if (!string.IsNullOrEmpty(data.ForeColor)) ctrl.ForeColor = ColorTranslator.FromHtml(data.ForeColor);
            float fontSize = data.FontSize > 0 ? data.FontSize : 9.75f;
            ctrl.Font = new Font("Segoe UI", fontSize, data.FontBold ? FontStyle.Bold : FontStyle.Regular);

            // --- YENİ EKLENEN ÖZELLİKLER YÜKLENİYOR ---
            wrapper.IsToggleButton = data.IsToggleButton;
            wrapper.PressedText = data.PressedText;
            if (!string.IsNullOrEmpty(data.PressedBackColor)) wrapper.PressedBackColor = ColorTranslator.FromHtml(data.PressedBackColor);
            if (!string.IsNullOrEmpty(data.PressedForeColor)) wrapper.PressedForeColor = ColorTranslator.FromHtml(data.PressedForeColor);
            if (Enum.TryParse(data.ButtonStyle, out CustomButtonStyle style)) wrapper.ButtonStyle = style;

            if (ctrl is Label lbl && Enum.TryParse(data.ContentAlignment, out ContentAlignment caLbl)) lbl.TextAlign = caLbl;
            else if (ctrl is Button btn && Enum.TryParse(data.ContentAlignment, out ContentAlignment caBtn)) btn.TextAlign = caBtn;
            else if (ctrl is TextBox txt && Enum.TryParse(data.HorizontalAlignment, out HorizontalAlignment haTxt)) txt.TextAlign = haTxt;
            else if (ctrl is NumericUpDown num && Enum.TryParse(data.HorizontalAlignment, out HorizontalAlignment haNum)) num.TextAlign = haNum;

            var loc = data.Location.Split(',').Select(s => int.Parse(s.Trim())).ToArray();
            var size = data.Size.Split(',').Select(s => int.Parse(s.Trim())).ToArray();
            int x = loc[0] + (applyOffset ? GridSize : 0);
            int y = loc[1] + (applyOffset ? GridSize : 0);

            wrapper.Location = new Point(x, y);
            wrapper.Size = new Size(size[0], size[1]);
            wrapper.PLC_WordIndex = data.PLC_WordIndex;
            wrapper.PLC_BitIndex = data.PLC_BitIndex;

            if (ctrl is NumericUpDown numControl)
            {
                numControl.Maximum = data.Maximum;
                numControl.Minimum = data.Minimum;
                numControl.DecimalPlaces = data.DecimalPlaces;
            }

            AttachEvents(ctrl);
            pnlDesignSurface.Controls.Add(ctrl);
            if (applyOffset) SelectControl(ctrl);
        }

        private void AttachEvents(Control ctrl)
        {
            ctrl.MouseDown += Control_MouseDown;
            ctrl.MouseMove += Control_MouseMove;
            ctrl.MouseUp += Control_MouseUp;
        }

        private void LoadLayoutForSelection(object sender, EventArgs e)
        {
            if (tsCmbMachineType.SelectedItem == null) return;
            if (tsCmbStepType.ComboBox.SelectedValue == null) return;
            if (!int.TryParse(tsCmbStepType.ComboBox.SelectedValue.ToString(), out int stepTypeId)) return;

            string machineSubType = tsCmbMachineType.SelectedItem.ToString();
            string json = _configRepo.GetLayoutJson(machineSubType, stepTypeId);
            ClearLayout();

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var list = JsonSerializer.Deserialize<List<ControlMetadata>>(json);
                    foreach (var item in list) CreateControlFromJson(item);
                }
                catch (Exception ex) { MessageBox.Show($"Hata: {ex.Message}"); }
            }
        }

        private async void BtnSaveLayout_Click(object sender, EventArgs e)
        {
            if (tsCmbMachineType.SelectedItem == null || tsCmbStepType.ComboBox.SelectedValue == null)
            {
                MessageBox.Show("Lütfen Makine ve Adım Tipi seçin."); return;
            }

            string subType = tsCmbMachineType.SelectedItem.ToString();
            if (!int.TryParse(tsCmbStepType.ComboBox.SelectedValue.ToString(), out int stepTypeId)) { MessageBox.Show("Hata"); return; }
            string name = $"{subType} - {tsCmbStepType.ComboBox.Text}";

            var list = new List<ControlMetadata>();
            foreach (Control c in pnlDesignSurface.Controls) list.Add(CreateMetadataFromControl(c));

            string json = JsonSerializer.Serialize(list);
            await Task.Run(() => _configRepo.SaveLayout(name, subType, stepTypeId, json));
            MessageBox.Show("Kaydedildi!");
        }

        private void ClearLayout()
        {
            pnlDesignSurface.Controls.Clear();
            SelectControl(null);
            pnlDesignSurface.Invalidate();
        }

        // --- PROPERTY WRAPPER (GÜNCELLENDİ) ---
        public class ControlPropertyWrapper
        {
            private Control _c; private PlcMapping _m;
            // Buton özelliklerini geçici tutmak için (Control nesnesi üzerinde bu özellikler yok çünkü)
            // Bu verileri Tag içinde saklayacağız.
            private ControlMetadata _extraData;

            public ControlPropertyWrapper(Control c)
            {
                _c = c;
                _m = c.Tag as PlcMapping;
                if (_m == null)
                {
                    // Tag dolu ama Mapping değilse, belki daha önce extraData koyduk?
                    // Basitlik için: Tag her zaman bir nesne tutsun. 
                    // Ancak mevcut yapıda Tag = PlcMapping idi. 
                    // Bunu bozmamak için ControlMetadata'yı burada sadece wrapper seviyesinde tutamayız, kaybolur.
                    // Çözüm: ControlMetadata özelliklerini "Tag" içindeki bir sözlükte veya ControlMetadata'nın kendisini Tag'e gömerek saklamak.
                    // Ama PlcMapping kritik. O yüzden şöyle yapalım:
                    // PlcMapping'i genişletelim veya Tag'i object[] yapalım.
                    // En kolayı: ControlMetadata verilerini "geçici" olarak bu wrapper'da tutup, kaydederken okumak.
                    // FAKAT: Wrapper her seçimde yeniden oluşuyor. O yüzden veriyi Control üzerinde saklamalıyız.
                    // Control.Tag özelliğini genişletilmiş bir sınıfa çeviriyoruz.
                    _m = new PlcMapping();
                }

                // Hack: ControlMetadata özelliklerini saklamak için Tag özelliğini bir "Container" gibi kullanalım.
                // Veya şimdilik bu özellikleri sadece PropertyGrid'de gösterip JSON'a yazarken,
                // Control'ün "AccessibleDescription" gibi kullanılmayan bir özelliğini depo olarak kullanalım.
                // VEYA: En temiz yöntem, Control.Tag'i bir "DesignerData" sınıfına çevirmektir.
                // Ama var olan kodu kırmamak için: _extraData'yı "AccessibleDescription" içine JSON string olarak gömelim.

                if (!string.IsNullOrEmpty(_c.AccessibleDescription))
                {
                    try { _extraData = JsonSerializer.Deserialize<ControlMetadata>(_c.AccessibleDescription); }
                    catch { _extraData = new ControlMetadata(); }
                }
                else _extraData = new ControlMetadata();

                c.Tag = _m; // PlcMapping'i koru
            }

            private void SaveExtraData()
            {
                // Ekstra verileri kontrolün AccessibleDescription özelliğine gömüyoruz
                _c.AccessibleDescription = JsonSerializer.Serialize(_extraData);
            }

            [Category("Tasarım")][DisplayName("Ad")] public string Name { get => _c.Name; set => _c.Name = value; }
            [Category("Tasarım")][DisplayName("Metin")] public string Text { get => _c.Text; set => _c.Text = value; }
            [Category("Tasarım")][DisplayName("Konum")] public Point Location { get => _c.Location; set => _c.Location = value; }
            [Category("Tasarım")][DisplayName("Boyut")] public Size Size { get => _c.Size; set => _c.Size = value; }

            // --- YENİ BUTON ÖZELLİKLERİ ---
            [Category("Buton Ayarları")]
            [DisplayName("Kalıcı Buton (Toggle)")]
            [Description("İşaretlenirse buton tıklandığında basılı kalır.")]
            public bool IsToggleButton
            {
                get => _extraData.IsToggleButton;
                set { _extraData.IsToggleButton = value; SaveExtraData(); }
            }

            [Category("Buton Ayarları")]
            [DisplayName("Buton Stili")]
            public CustomButtonStyle ButtonStyle
            {
                get => _extraData.ButtonStyle == "Flat" ? CustomButtonStyle.Solid : CustomButtonStyle.Kabartma;
                set
                {
                    _extraData.ButtonStyle = (value == CustomButtonStyle.Solid) ? "Flat" : "Standard";
                    if (_c is Button btn) btn.FlatStyle = (value == CustomButtonStyle.Solid) ? FlatStyle.Flat : FlatStyle.Standard;
                    SaveExtraData();
                }
            }

            [Category("Basılı Durum Ayarları")]
            [DisplayName("Basılı Metin")]
            public string PressedText
            {
                get => _extraData.PressedText;
                set { _extraData.PressedText = value; SaveExtraData(); }
            }

            [Category("Basılı Durum Ayarları")]
            [DisplayName("Basılı Arka Plan")]
            public Color PressedBackColor
            {
                get => string.IsNullOrEmpty(_extraData.PressedBackColor) ? Color.Gray : ColorTranslator.FromHtml(_extraData.PressedBackColor);
                set { _extraData.PressedBackColor = ColorTranslator.ToHtml(value); SaveExtraData(); }
            }

            [Category("Basılı Durum Ayarları")]
            [DisplayName("Basılı Yazı Rengi")]
            public Color PressedForeColor
            {
                get => string.IsNullOrEmpty(_extraData.PressedForeColor) ? Color.White : ColorTranslator.FromHtml(_extraData.PressedForeColor);
                set { _extraData.PressedForeColor = ColorTranslator.ToHtml(value); SaveExtraData(); }
            }
            // ------------------------------

            [Category("Görünüm")]
            [DisplayName("Metin Hizalama (Etiket/Buton)")]
            public ContentAlignment TextAlign
            {
                get { if (_c is Label l) return l.TextAlign; if (_c is Button b) return b.TextAlign; return ContentAlignment.MiddleLeft; }
                set { if (_c is Label l) l.TextAlign = value; if (_c is Button b) b.TextAlign = value; }
            }

            [Category("Görünüm")]
            [DisplayName("Yatay Hizalama (Giriş)")]
            public HorizontalAlignment HorizontalAlign
            {
                get { if (_c is TextBox t) return t.TextAlign; if (_c is NumericUpDown n) return n.TextAlign; return HorizontalAlignment.Left; }
                set { if (_c is TextBox t) t.TextAlign = value; if (_c is NumericUpDown n) n.TextAlign = value; }
            }

            [Category("Görünüm")][DisplayName("Arka Plan Rengi")] public Color BackColor { get => _c.BackColor; set => _c.BackColor = value; }
            [Category("Görünüm")][DisplayName("Yazı Rengi")] public Color ForeColor { get => _c.ForeColor; set => _c.ForeColor = value; }
            [Category("Görünüm")][DisplayName("Yazı Boyutu")] public float FontSize { get => _c.Font.Size; set => _c.Font = new Font(_c.Font.FontFamily, value, _c.Font.Style); }
            [Category("Görünüm")][DisplayName("Kalın Yazı")] public bool FontBold { get => _c.Font.Bold; set => _c.Font = new Font(_c.Font, value ? FontStyle.Bold : FontStyle.Regular); }

            [Category("PLC")][DisplayName("Word Index")] public int PLC_WordIndex { get => _m.WordIndex; set => _m.WordIndex = value; }
            [Category("PLC")][DisplayName("Bit Index")] public int PLC_BitIndex { get => _m.BitIndex; set => _m.BitIndex = value; }
            [Category("PLC")][DisplayName("String Uzunluğu")] public int PLC_StringWordLength { get => _m.StringWordLength; set => _m.StringWordLength = value; }

            [Category("Değer")][DisplayName("Maksimum")] public decimal Maximum { get => (_c as NumericUpDown)?.Maximum ?? 100; set { if (_c is NumericUpDown num) num.Maximum = value; } }
            [Category("Değer")][DisplayName("Minimum")] public decimal Minimum { get => (_c as NumericUpDown)?.Minimum ?? 0; set { if (_c is NumericUpDown num) num.Minimum = value; } }
            [Category("Değer")][DisplayName("Ondalık")] public int DecimalPlaces { get => (_c as NumericUpDown)?.DecimalPlaces ?? 0; set { if (_c is NumericUpDown num) num.DecimalPlaces = value; } }
        }
    }
}