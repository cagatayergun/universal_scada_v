using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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
        public enum CustomButtonStyle { Kabartma, Solid }

        // --- SABİTLER ---
        private const int GridSize = 20;
        private const int HandleSize = 8;

        // --- DEĞİŞKENLER ---
        private List<Control> _selectedControls = new List<Control>();
        private Control _interactionControl;
        private Point _dragStartPoint;
        private Dictionary<Control, Rectangle> _startBoundsDict = new Dictionary<Control, Rectangle>();

        private HitTest _currentHitTest = HitTest.None;
        private bool _isDraggingOrResizing = false;

        private bool _isLassoSelecting = false;
        private Rectangle _selectionRect;

        private string _clipboardJson = null;
        private Stack<string> _undoStack = new Stack<string>();

        private readonly RecipeConfigurationRepository _configRepo = new RecipeConfigurationRepository();
        private readonly MachineRepository _machineRepo = new MachineRepository(); // YENİ EKLENEN
        private ContextMenuStrip _contextMenu;
        // --- DEĞİŞKENLER --- bölgesine ekleyin
        //private readonly RecipeConfigurationRepository _configRepo1 = new RecipeConfigurationRepository();
        
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

            tsbNew.Click += (s, e) => { SaveUndoState(); ClearLayout(); };
            tsbSave.Click += BtnSaveLayout_Click;
            tsbCopy.Click += (s, e) => CopyControls();
            tsbPaste.Click += (s, e) => PasteControls();
            tsbDelete.Click += (s, e) => DeleteSelectedControls();

            BindToolboxEvents();

            tsCmbMachineType.SelectedIndexChanged += LoadLayoutForSelection;
            tsCmbStepType.SelectedIndexChanged += LoadLayoutForSelection;
            tsCmbStepType.Visible = false;
            tsLabelStep.Visible = false;
        }

        private void InitializeContextMenu()
        {
            _contextMenu = new ContextMenuStrip();

            var itemFront = new ToolStripMenuItem("En Öne Getir");
            itemFront.Image = SystemIcons.Shield.ToBitmap();
            itemFront.Click += (s, e) =>
            {
                SaveUndoState();
                foreach (var ctrl in _selectedControls) ctrl.BringToFront();
                pnlDesignSurface.Invalidate();
            };
            _contextMenu.Items.Add(itemFront);

            var itemBack = new ToolStripMenuItem("En Arkaya Gönder");
            itemBack.Click += (s, e) =>
            {
                SaveUndoState();
                foreach (var ctrl in _selectedControls) ctrl.SendToBack();
                pnlDesignSurface.Invalidate();
            };
            _contextMenu.Items.Add(itemBack);

            _contextMenu.Items.Add(new ToolStripSeparator());

            var itemCut = new ToolStripMenuItem("Kes");
            itemCut.ShortcutKeys = Keys.Control | Keys.X;
            itemCut.Click += (s, e) => CutControls();
            _contextMenu.Items.Add(itemCut);

            var itemCopy = new ToolStripMenuItem("Kopyala");
            itemCopy.ShortcutKeys = Keys.Control | Keys.C;
            itemCopy.Click += (s, e) => CopyControls();
            _contextMenu.Items.Add(itemCopy);

            var itemPaste = new ToolStripMenuItem("Yapıştır");
            itemPaste.ShortcutKeys = Keys.Control | Keys.V;
            itemPaste.Click += (s, e) => PasteControls();
            _contextMenu.Items.Add(itemPaste);

            _contextMenu.Items.Add(new ToolStripSeparator());

            var itemDel = new ToolStripMenuItem("Sil");
            itemDel.ShortcutKeys = Keys.Delete;
            itemDel.Click += (s, e) => DeleteSelectedControls();
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
            var machines = _machineRepo.GetAllMachines(); // Makine listenizi çeken metot
            tsCmbMachineType.ComboBox.DataSource = machines;
            tsCmbMachineType.ComboBox.DisplayMember = "MachineName"; // Ekranda makine adı görünsün
            tsCmbMachineType.ComboBox.ValueMember = "Id"; // Arka planda Makine Id'si tutulsun

            var steps = _configRepo.GetStepTypes();
            tsCmbStepType.ComboBox.DataSource = steps;
            tsCmbStepType.ComboBox.DisplayMember = "StepName";
            tsCmbStepType.ComboBox.ValueMember = "Id";
        }

        // --- UNDO / REDO ---
        private void SaveUndoState()
        {
            var list = new List<ControlMetadata>();
            foreach (Control c in pnlDesignSurface.Controls) list.Add(CreateMetadataFromControl(c));
            _undoStack.Push(JsonSerializer.Serialize(list));
        }

        private void PerformUndo()
        {
            if (_undoStack.Count > 0)
            {
                string state = _undoStack.Pop();
                pnlDesignSurface.Controls.Clear();
                _selectedControls.Clear();

                try
                {
                    var list = JsonSerializer.Deserialize<List<ControlMetadata>>(state);
                    foreach (var item in list) CreateControlFromJson(item, false, false);
                }
                catch { }

                UpdateSelectionUI();
            }
        }

        // --- ÇİZİM ---
        private void PnlDesignSurface_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            DrawGrid(g);

            foreach (Control ctrl in pnlDesignSurface.Controls)
            {
                if (_selectedControls.Contains(ctrl)) continue;

                using (Pen pen = new Pen(Color.FromArgb(220, 220, 220)))
                {
                    pen.DashStyle = DashStyle.Dot;
                    Rectangle r = ctrl.Bounds; r.Inflate(1, 1);
                    g.DrawRectangle(pen, r);
                }
            }

            foreach (var ctrl in _selectedControls)
            {
                Rectangle rect = ctrl.Bounds;
                using (Pen pen = new Pen(Color.FromArgb(0, 122, 204), 1))
                {
                    g.DrawRectangle(pen, rect.X - 1, rect.Y - 1, rect.Width + 1, rect.Height + 1);
                }
                DrawHandles(g, rect);
            }

            if (_isLassoSelecting)
            {
                using (Pen pen = new Pen(Color.DodgerBlue, 1) { DashStyle = DashStyle.Dash })
                {
                    g.DrawRectangle(pen, _selectionRect);
                }
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(50, Color.DodgerBlue)))
                {
                    g.FillRectangle(brush, _selectionRect);
                }
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
                _interactionControl = GetControlAtHitTest(e.Location, out _currentHitTest);

                if (_interactionControl != null && _currentHitTest != HitTest.None && _currentHitTest != HitTest.Body)
                {
                    SaveUndoState();
                    _isDraggingOrResizing = true;
                    _dragStartPoint = e.Location;
                    CaptureStartBounds();
                    return;
                }

                SelectControl(null);
                _isLassoSelecting = true;
                _dragStartPoint = e.Location;
                _selectionRect = new Rectangle(e.Location, new Size(0, 0));
                pnlDesignSurface.Invalidate();
            }
        }

        private Control GetControlAtHitTest(Point p, out HitTest hit)
        {
            foreach (var ctrl in _selectedControls)
            {
                Rectangle r = ctrl.Bounds;
                if (IsOverHandle(r.Left, r.Top, p)) { hit = HitTest.TopLeft; return ctrl; }
                if (IsOverHandle(r.Right, r.Top, p)) { hit = HitTest.TopRight; return ctrl; }
                if (IsOverHandle(r.Left, r.Bottom, p)) { hit = HitTest.BottomLeft; return ctrl; }
                if (IsOverHandle(r.Right, r.Bottom, p)) { hit = HitTest.BottomRight; return ctrl; }
            }
            hit = HitTest.None;
            return null;
        }

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            Control clickedCtrl = sender as Control;
            bool isCtrlPressed = ModifierKeys == Keys.Control;

            if (e.Button == MouseButtons.Left)
            {
                SelectControl(clickedCtrl, isCtrlPressed);
                _currentHitTest = HitTest.Body;
                _interactionControl = clickedCtrl;

                SaveUndoState();
                _isDraggingOrResizing = true;
                _dragStartPoint = Cursor.Position;
                CaptureStartBounds();

                foreach (var ctrl in _selectedControls) ctrl.BringToFront();
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (!_selectedControls.Contains(clickedCtrl))
                {
                    SelectControl(clickedCtrl, false);
                }
                _contextMenu.Show(Cursor.Position);
            }
        }

        private void CaptureStartBounds()
        {
            _startBoundsDict.Clear();
            foreach (var ctrl in _selectedControls)
            {
                _startBoundsDict[ctrl] = ctrl.Bounds;
            }
        }

        private void PnlDesignSurface_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isLassoSelecting)
            {
                _selectionRect = new Rectangle(
                    Math.Min(_dragStartPoint.X, e.X),
                    Math.Min(_dragStartPoint.Y, e.Y),
                    Math.Abs(e.X - _dragStartPoint.X),
                    Math.Abs(e.Y - _dragStartPoint.Y));
                pnlDesignSurface.Invalidate();
            }
            else if (!_isDraggingOrResizing)
            {
                HitTest hit;
                GetControlAtHitTest(e.Location, out hit);
                SetCursor(hit);
            }
            else if (_interactionControl != null && _currentHitTest != HitTest.Body)
            {
                int dx = e.X - _dragStartPoint.X;
                int dy = e.Y - _dragStartPoint.Y;

                foreach (var ctrl in _selectedControls)
                {
                    if (!_startBoundsDict.ContainsKey(ctrl)) continue;
                    Rectangle newBounds = _startBoundsDict[ctrl];

                    if (_currentHitTest == HitTest.BottomRight) { newBounds.Width += dx; newBounds.Height += dy; }
                    else if (_currentHitTest == HitTest.BottomLeft) { newBounds.X += dx; newBounds.Width -= dx; newBounds.Height += dy; }
                    else if (_currentHitTest == HitTest.TopRight) { newBounds.Y += dy; newBounds.Width += dx; newBounds.Height -= dy; }
                    else if (_currentHitTest == HitTest.TopLeft) { newBounds.X += dx; newBounds.Y += dy; newBounds.Width -= dx; newBounds.Height -= dy; }

                    newBounds.Width = Math.Max(GridSize, Snap(newBounds.Width));
                    newBounds.Height = Math.Max(GridSize, Snap(newBounds.Height));

                    if (_currentHitTest.ToString().Contains("Left")) newBounds.X = Snap(newBounds.X);
                    if (_currentHitTest.ToString().Contains("Top")) newBounds.Y = Snap(newBounds.Y);

                    ctrl.Bounds = newBounds;
                }

                UpdateStatus();
                pnlDesignSurface.Invalidate();
            }
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingOrResizing && _currentHitTest == HitTest.Body)
            {
                int dx = Cursor.Position.X - _dragStartPoint.X;
                int dy = Cursor.Position.Y - _dragStartPoint.Y;

                foreach (var ctrl in _selectedControls)
                {
                    if (!_startBoundsDict.ContainsKey(ctrl)) continue;
                    var startB = _startBoundsDict[ctrl];
                    int newX = Snap(startB.X + dx);
                    int newY = Snap(startB.Y + dy);
                    ctrl.Location = new Point(newX, newY);
                }

                UpdateStatus();
                pnlDesignSurface.Invalidate();
            }
        }

        private void PnlDesignSurface_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isLassoSelecting)
            {
                _isLassoSelecting = false;
                bool isCtrlPressed = ModifierKeys == Keys.Control;
                if (!isCtrlPressed) _selectedControls.Clear();

                foreach (Control ctrl in pnlDesignSurface.Controls)
                {
                    if (_selectionRect.IntersectsWith(ctrl.Bounds))
                    {
                        if (!_selectedControls.Contains(ctrl))
                            _selectedControls.Add(ctrl);
                    }
                }
                UpdateSelectionUI();
            }
            FinishOp();
        }

        private void Control_MouseUp(object sender, MouseEventArgs e) => FinishOp();

        private void FinishOp()
        {
            _isDraggingOrResizing = false;
            _currentHitTest = HitTest.None;
            _interactionControl = null;
            propertyGrid.Refresh();
            pnlDesignSurface.Invalidate();
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

        private void SelectControl(Control control, bool append = false)
        {
            if (append)
            {
                if (control != null)
                {
                    if (_selectedControls.Contains(control)) _selectedControls.Remove(control);
                    else _selectedControls.Add(control);
                }
            }
            else
            {
                _selectedControls.Clear();
                if (control != null) _selectedControls.Add(control);
            }

            UpdateSelectionUI();
        }

        private void UpdateSelectionUI()
        {
            pnlDesignSurface.Invalidate();

            if (_selectedControls.Count > 0)
            {
                propertyGrid.SelectedObjects = _selectedControls.Select(c => new ControlPropertyWrapper(c)).ToArray();
            }
            else
            {
                propertyGrid.SelectedObjects = null;
            }
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (_selectedControls.Count == 1)
            {
                var act = _selectedControls[0];
                lblStatusReady.Text = $"Seçili: {act.Name}";
                lblStatusPosition.Text = $"Konum: {act.Left}, {act.Top}";
                lblStatusSize.Text = $"Boyut: {act.Width} x {act.Height}";
            }
            else if (_selectedControls.Count > 1)
            {
                lblStatusReady.Text = $"Seçili: {_selectedControls.Count} nesne";
                lblStatusPosition.Text = "";
                lblStatusSize.Text = "";
            }
            else
            {
                lblStatusReady.Text = "Hazır";
                lblStatusPosition.Text = "";
                lblStatusSize.Text = "";
            }
        }

        private void CopyControls()
        {
            if (_selectedControls.Count == 0) return;
            var list = new List<ControlMetadata>();
            foreach (var ctrl in _selectedControls)
            {
                list.Add(CreateMetadataFromControl(ctrl));
            }
            _clipboardJson = JsonSerializer.Serialize(list);
        }

        private void CutControls()
        {
            CopyControls();
            DeleteSelectedControls();
        }

        private void PasteControls()
        {
            if (string.IsNullOrEmpty(_clipboardJson)) return;
            SaveUndoState();
            try
            {
                var list = JsonSerializer.Deserialize<List<ControlMetadata>>(_clipboardJson);
                _selectedControls.Clear();

                foreach (var metadata in list)
                {
                    metadata.Name += "_" + DateTime.Now.Ticks.ToString().Substring(10);
                    CreateControlFromJson(metadata, true, true);
                }
                UpdateSelectionUI();
            }
            catch { }
        }

        private void DeleteSelectedControls()
        {
            if (_selectedControls.Count > 0)
            {
                SaveUndoState();
                foreach (var ctrl in _selectedControls.ToList())
                {
                    pnlDesignSurface.Controls.Remove(ctrl);
                }
                _selectedControls.Clear();
                UpdateSelectionUI();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.C)) { CopyControls(); return true; }
            if (keyData == (Keys.Control | Keys.X)) { CutControls(); return true; }
            if (keyData == (Keys.Control | Keys.V)) { PasteControls(); return true; }
            if (keyData == (Keys.Control | Keys.Z)) { PerformUndo(); return true; }
            if (keyData == (Keys.Control | Keys.D)) { CopyControls(); PasteControls(); return true; }
            if (keyData == Keys.Delete) { DeleteSelectedControls(); return true; }
            if (keyData == (Keys.Control | Keys.A))
            {
                _selectedControls = pnlDesignSurface.Controls.Cast<Control>().ToList();
                UpdateSelectionUI();
                return true;
            }

            if (_selectedControls.Count > 0)
            {
                int step = (keyData & Keys.Shift) == Keys.Shift ? GridSize : 1;
                bool moved = false;

                switch (keyData & Keys.KeyCode)
                {
                    case Keys.Left: foreach (var c in _selectedControls) c.Left -= step; moved = true; break;
                    case Keys.Right: foreach (var c in _selectedControls) c.Left += step; moved = true; break;
                    case Keys.Up: foreach (var c in _selectedControls) c.Top -= step; moved = true; break;
                    case Keys.Down: foreach (var c in _selectedControls) c.Top += step; moved = true; break;
                }

                if (moved)
                {
                    UpdateStatus();
                    pnlDesignSurface.Invalidate();
                    propertyGrid.Refresh();
                    return true;
                }
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
            SaveUndoState();
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

            _selectedControls.Clear();
            CreateControlFromJson(meta, false, true);
            UpdateSelectionUI();
        }

        // --- JSON / LOAD / SAVE ---

        private ControlMetadata CreateMetadataFromControl(Control ctrl)
        {
            var wrapper = new ControlPropertyWrapper(ctrl);

            // Verilerin doğru serileşebilmesi için Wrapper içindeki koleksiyonu okuyoruz
            var mappedStates = wrapper.DurumAyarlari.Select(x => new MultiStateSetting
            {
                Value = x.Value,
                Text = x.Text,
                BackColor = ColorTranslator.ToHtml(x.BackColor),
                ForeColor = ColorTranslator.ToHtml(x.ForeColor),
                ImageBase64 = ImageToBase64(x.Image)
            }).ToList();

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
                IsToggleButton = wrapper.IsToggleButton,
                PressedText = wrapper.PressedText,
                PressedBackColor = ColorTranslator.ToHtml(wrapper.PressedBackColor),
                PressedForeColor = ColorTranslator.ToHtml(wrapper.PressedForeColor),
                ButtonStyle = wrapper.ButtonStyle.ToString(),
                ShowNumericArrows = wrapper.ShowNumericArrows,

                // Çoklu Durumları da buraya ekliyoruz
                IsMultiStateButton = wrapper.IsMultiStateButton,
                MaxStateValue = wrapper.MaxStateValue,
                MultiStates = mappedStates
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

        private void CreateControlFromJson(ControlMetadata data, bool applyOffset = false, bool selectAfter = false)
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

            wrapper.IsToggleButton = data.IsToggleButton;
            wrapper.PressedText = data.PressedText;
            if (!string.IsNullOrEmpty(data.PressedBackColor)) wrapper.PressedBackColor = ColorTranslator.FromHtml(data.PressedBackColor);
            if (!string.IsNullOrEmpty(data.PressedForeColor)) wrapper.PressedForeColor = ColorTranslator.FromHtml(data.PressedForeColor);
            if (Enum.TryParse(data.ButtonStyle, out CustomButtonStyle style)) wrapper.ButtonStyle = style;

            // Çoklu Durumları Yükle
            wrapper.IsMultiStateButton = data.IsMultiStateButton;
            wrapper.MaxStateValue = data.MaxStateValue;

            if (data.MultiStates != null)
            {
                wrapper.DurumAyarlari.Clear();
                foreach (var state in data.MultiStates)
                {
                    wrapper.DurumAyarlari.Add(new MultiStateUIItem
                    {
                        Value = state.Value,
                        Text = state.Text,
                        BackColor = string.IsNullOrEmpty(state.BackColor) ? Color.LightGray : ColorTranslator.FromHtml(state.BackColor),
                        ForeColor = string.IsNullOrEmpty(state.ForeColor) ? Color.Black : ColorTranslator.FromHtml(state.ForeColor),
                        Image = Base64ToImage(state.ImageBase64)
                    });
                }
            }

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

                // YENİ EKLENEN KISIM: Oklar ve Boşluk Yönetimi
                if (numControl.Controls.Count > 1)
                {
                    numControl.Controls[0].Visible = data.ShowNumericArrows;
                    TextBox innerTxt = numControl.Controls[1] as TextBox;

                    if (innerTxt != null)
                    {
                        if (!data.ShowNumericArrows)
                        {
                            // Oklar yoksa metin kutusunu sağa kadar tam uzat
                            innerTxt.Width = numControl.Width;
                        }
                    }
                }
                wrapper.ShowNumericArrows = data.ShowNumericArrows;
            }

            AttachEvents(ctrl);
            pnlDesignSurface.Controls.Add(ctrl);

            if (selectAfter) _selectedControls.Add(ctrl);
        }

        private void AttachEvents(Control ctrl)
        {
            ctrl.MouseDown += Control_MouseDown;
            ctrl.MouseMove += Control_MouseMove;
            ctrl.MouseUp += Control_MouseUp;
        }

        private void LoadLayoutForSelection(object sender, EventArgs e)
        {
            if (tsCmbMachineType.ComboBox.SelectedValue == null) return;
            if (tsCmbStepType.ComboBox.SelectedValue == null) return;
            if (!int.TryParse(tsCmbStepType.ComboBox.SelectedValue.ToString(), out int stepTypeId)) return;

            // Alt tip (SubType) yerine artık o makinenin ID'sini (veya adını) referans olarak alıyoruz.
            string machineRef = tsCmbMachineType.ComboBox.SelectedValue.ToString();

            // _configRepo metodu argüman isminde "machineSubType" yazsa da biz ona Makine ID'si yolluyoruz.
            string json = _configRepo.GetLayoutJson(machineRef, stepTypeId);
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
            if (tsCmbMachineType.ComboBox.SelectedValue == null || tsCmbStepType.ComboBox.SelectedValue == null)
            {
                MessageBox.Show("Lütfen Makine ve Adım Tipi seçin."); return;
            }

            // Makine ID ve ismini al
            string machineRef = tsCmbMachineType.ComboBox.SelectedValue.ToString();
            string machineName = tsCmbMachineType.ComboBox.Text;

            if (!int.TryParse(tsCmbStepType.ComboBox.SelectedValue.ToString(), out int stepTypeId)) { MessageBox.Show("Hata"); return; }

            // Tasarımın adı artık "Makine Adı - Adım Tipi" şeklinde kaydedilecek
            string name = $"{machineName} - {tsCmbStepType.ComboBox.Text}";

            var list = new List<ControlMetadata>();
            foreach (Control c in pnlDesignSurface.Controls) list.Add(CreateMetadataFromControl(c));

            string json = JsonSerializer.Serialize(list);

            // SaveLayout metodunda "subType" yerine makine referansını (ID) kaydediyoruz.
            await Task.Run(() => _configRepo.SaveLayout(name, machineRef, stepTypeId, json));
            MessageBox.Show("Kaydedildi!");
        }

        private void ClearLayout()
        {
            pnlDesignSurface.Controls.Clear();
            _selectedControls.Clear();
            UpdateSelectionUI();
        }

        // --- GÖRSEL YARDIMCI METOTLAR ---
        public static string ImageToBase64(Image image)
        {
            if (image == null) return null;
            using (MemoryStream ms = new MemoryStream())
            {
                // Standart resim formatına çevir (PNG)
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static Image Base64ToImage(string base64String)
        {
            if (string.IsNullOrEmpty(base64String)) return null;
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    return Image.FromStream(ms);
                }
            }
            catch { return null; }
        }

        // --- ÇOKLU DURUM (MULTI-STATE) LİSTE YÖNETİMİ İÇİN YARDIMCI SINIF ---
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class MultiStateUIItem
        {
            [DisplayName("1. Durum Değeri")]
            public int Value { get; set; }

            [DisplayName("2. Metin")]
            public string Text { get; set; }

            [DisplayName("3. Arka Plan Rengi")]
            public Color BackColor { get; set; } = Color.LightGray;

            [DisplayName("4. Yazı Rengi")]
            public Color ForeColor { get; set; } = Color.Black;

            [DisplayName("5. Resim (İsteğe Bağlı)")]
            [Description("Buton durumuna göre değişecek olan resmi seçin.")]
            public Image Image { get; set; }

            public override string ToString() => $"Durum: {Value}";
        }

        // --- PROPERTY WRAPPER ---
        public class ControlPropertyWrapper
        {
            private Control _c; private PlcMapping _m;
            private ControlMetadata _extraData;

            // PropertyGrid içindeki (Collection) editöründen anlık veri okumak için
            [Category("Çoklu Durum (Word) Ayarları")]
            [DisplayName("Durum Listesi (Resim/Renk)")]
            [Description("Butonun alacağı değerlere (0,1,2..) göre resim, metin ve renkleri buraya ekleyin.")]
            public BindingList<MultiStateUIItem> DurumAyarlari { get; private set; }

            public ControlPropertyWrapper(Control c)
            {
                _c = c;
                _m = c.Tag as PlcMapping;
                if (_m == null)
                {
                    _m = new PlcMapping();
                }

                if (!string.IsNullOrEmpty(_c.AccessibleDescription))
                {
                    try { _extraData = JsonSerializer.Deserialize<ControlMetadata>(_c.AccessibleDescription); }
                    catch { _extraData = new ControlMetadata(); }
                }
                else _extraData = new ControlMetadata();

                c.Tag = _m;

                // Binding listesini oluştur
                DurumAyarlari = new BindingList<MultiStateUIItem>();

                // --- YENİ EKLENEN KISIM: Kaydedilmiş verileri listeye geri yükle ---
                if (_extraData.MultiStates != null)
                {
                    foreach (var state in _extraData.MultiStates)
                    {
                        DurumAyarlari.Add(new MultiStateUIItem
                        {
                            Value = state.Value,
                            Text = state.Text,
                            BackColor = string.IsNullOrEmpty(state.BackColor) ? Color.LightGray : ColorTranslator.FromHtml(state.BackColor),
                            ForeColor = string.IsNullOrEmpty(state.ForeColor) ? Color.Black : ColorTranslator.FromHtml(state.ForeColor),
                            Image = Base64ToImage(state.ImageBase64)
                        });
                    }
                }
                // --------------------------------------------------------------------

                // Değişikliklerde arka plandaki JSON metnini güncellemeyi tetikle
                DurumAyarlari.ListChanged += (s, e) => SaveExtraData();
            }

            private void SaveExtraData()
            {
                // Kaydederken UI modellerini gerçek Modele aktarıyoruz
                _extraData.MultiStates = DurumAyarlari.Select(x => new MultiStateSetting
                {
                    Value = x.Value,
                    Text = x.Text,
                    BackColor = ColorTranslator.ToHtml(x.BackColor),
                    ForeColor = ColorTranslator.ToHtml(x.ForeColor),
                    ImageBase64 = ImageToBase64(x.Image)
                }).ToList();

                _c.AccessibleDescription = JsonSerializer.Serialize(_extraData);
            }

            [Category("Tasarım")][DisplayName("Ad")] public string Name { get => _c.Name; set => _c.Name = value; }
            [Category("Tasarım")][DisplayName("Metin")] public string Text { get => _c.Text; set => _c.Text = value; }
            [Category("Tasarım")][DisplayName("Konum")] public Point Location { get => _c.Location; set => _c.Location = value; }
            [Category("Tasarım")][DisplayName("Boyut")] public Size Size { get => _c.Size; set => _c.Size = value; }
            [Category("Değer")]
            [DisplayName("Okları Göster (Sayı)")]
            [Description("Sayı girişindeki yukarı/aşağı oklarını gösterir veya gizler.")]
            public bool ShowNumericArrows
            {
                get
                {
                    if (_c is NumericUpDown && _extraData != null)
                        return _extraData.ShowNumericArrows;
                    return true;
                }
                set
                {
                    if (_extraData != null)
                    {
                        _extraData.ShowNumericArrows = value;
                        SaveExtraData();
                    }
                    if (_c is NumericUpDown num && num.Controls.Count > 1)
                    {
                        // Ok butonunu (Controls[0]) gizle veya göster
                        num.Controls[0].Visible = value;

                        // İç metin kutusunun (Controls[1]) boyutunu ayarla
                        TextBox innerTxt = num.Controls[1] as TextBox;
                        if (innerTxt != null)
                        {
                            if (value)
                            {
                                // Oklar açıksa, metin kutusunun genişliğini normale döndür (oklara yer aç)
                                innerTxt.Width = num.Width - num.Controls[0].Width - 2;
                            }
                            else
                            {
                                // Oklar KAPALIYSA, metin kutusunu sınırları kaplayacak kadar tamamen genişlet
                                innerTxt.Width = num.Width;
                            }
                        }
                    }
                }
            }

            // --- YENİ EKLENEN ÇOKLU DURUM (MULTI-STATE) ÖZELLİKLERİ ---
            [Category("Çoklu Durum (Word) Ayarları")]
            [DisplayName("Çoklu Durum Butonu mu?")]
            [Description("Eğer işaretlenirse, bu buton tıklandığında Word adresi 'Maksimum Sınır'a kadar sıralı olarak artar.")]
            public bool IsMultiStateButton
            {
                get => _extraData.IsMultiStateButton;
                set { _extraData.IsMultiStateButton = value; SaveExtraData(); }
            }

            [Category("Çoklu Durum (Word) Ayarları")]
            [DisplayName("Maksimum Durum Sınırı")]
            [Description("Buton değeri bu rakamı aşınca 0'a döner (Örn: 3 yazarsanız döngü 0-1-2-3 olur)")]
            public int MaxStateValue
            {
                get => _extraData.MaxStateValue;
                set { _extraData.MaxStateValue = value; SaveExtraData(); }
            }

            // --- BUTON ÖZELLİKLERİ ---
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