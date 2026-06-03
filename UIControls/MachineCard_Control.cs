// UI/Controls/MachineCard_Control.cs
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection.PortableExecutable;
using System.Windows.Forms;
using Telemetry.Models;
using Telemetry.Services;
using MaterialSkin;
using MaterialSkin.Controls;

namespace Telemetry.UI.Controls
{
    public partial class MachineCard_Control : UserControl
    {
        public int MachineId { get; private set; }
        private int _lastValidProgress = 0;
        public string MachineUserDefinedId { get; private set; }
        public string MachineName { get; private set; }
        public string MachineType { get; private set; }

        public event EventHandler DetailsRequested;
        public event EventHandler VncRequested;

        // =========================================================================
        // GOOGLE MATERIAL DESIGN MAT VE SOFT DURUM RENKLERİ
        // =========================================================================
        private readonly Color _colorConnected = Color.FromArgb(102, 187, 106);    // Pastel Yeşil
        private readonly Color _colorDisconnected = Color.FromArgb(239, 83, 80);  // Pastel Kırmızı
        private readonly Color _colorConnecting = Color.FromArgb(255, 202, 40);    // Pastel Sarı
        private readonly Color _colorAlarm = Color.FromArgb(183, 28, 28);          // Derin Koyu Kırmızı
        private readonly Color _colorPlay = Color.FromArgb(76, 175, 80);           // Canlı Material Yeşil
        private readonly Color _colorPause = Color.FromArgb(255, 152, 0);          // Canlı Material Turuncu

        private readonly Image _originalPlayIcon;
        private readonly Image _originalPauseIcon;
        private readonly Image _originalAlarmIcon;
        private readonly Image _originalAlarmyokIcon;
        private readonly Image _originalbaglantivarIcon;
        private readonly Image _originalbaglantiyokIcon;

        private readonly bool _isDryingMachine = false;

        public MachineCard_Control(int machineId, string machineUserDefinedId, string machineName, int displayIndex, string machineType)
        {
            InitializeComponent();

            this.MachineId = machineId;
            this.MachineUserDefinedId = machineUserDefinedId;
            this.MachineName = machineName;
            this.MachineType = machineType;

            // Kurutma makinesi kontrolü için esnek arama yapıyoruz
            _isDryingMachine = this.MachineType != null && this.MachineType.Contains("Kurutma", StringComparison.OrdinalIgnoreCase);

            lblMachineNumber.Text = $"{displayIndex}.";

            // SPEED OPTİMİZASYON: Hızlı veri akışında titremeyi (Flickering) tamamen engeller
            this.DoubleBuffered = true;

            // Kurutma Makinesi ise ilgili proses ilerleme çubuklarını ve yüzdelerini kapat
            if (_isDryingMachine)
            {
                lblProcessing.Visible = false;
                progressBar.Visible = false;
                lblPercentage.Visible = false;
            }

            // Kaynaklardan orijinal ikonları yükle
            _originalPlayIcon = Properties.Resource1.play2;
            _originalPauseIcon = Properties.Resource1.pause2;
            _originalAlarmIcon = Properties.Resource1.alarm_var;
            _originalAlarmyokIcon = Properties.Resource1.alarm_yok;
            _originalbaglantivarIcon = Properties.Resource1.yilmak_baglanti_2;
            _originalbaglantiyokIcon = Properties.Resource1.yilmak_baglanti;

            // PictureBox'ların arkaplanını şeffaf yap ve ilk durumları gizle
            picPlay.BackColor = Color.Transparent;
            picPause.BackColor = Color.Transparent;
            picAlarm.BackColor = Color.Transparent;

            picPlay.Visible = false;
            picPause.Visible = false;
            picAlarm.Visible = false;
            btnVnc.Visible = false;
            btnInfo.Visible = false;

            // Dark Mode Metin Uyumu: Açık gri/beyaz tonlar uygulandı
            Color textLight = Color.FromArgb(240, 240, 240);

            lblMachineNameValue.ForeColor = textLight;
            lblRecipeNameValue.ForeColor = textLight;
            lblOperatorValue.ForeColor = textLight;
            lblStepValue.ForeColor = textLight;

            UpdateView(new FullMachineStatus { ConnectionState = ConnectionStatus.Disconnected, MachineName = this.MachineName });
        }

        private Image TintImage(Image sourceImage, Color tintColor)
        {
            if (sourceImage == null) return null;

            Bitmap newBitmap = new Bitmap(sourceImage.Width, sourceImage.Height);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                float r = tintColor.R / 255f;
                float gg = tintColor.G / 255f;
                float b = tintColor.B / 255f;

                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {r, gg, b, 0, 1}
                });

                using (ImageAttributes attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    g.DrawImage(sourceImage, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                          0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;
        }

        private void ApplyPermissions(ConnectionStatus connectionState)
        {
            if (connectionState == ConnectionStatus.Connected)
            {
                bool hasVncPerm = PermissionService.HasAnyPermission(new List<int> { 4 }) ||
                                  PermissionService.HasAnyPermission(new List<int> { 1000 });

                if (btnVnc.Visible != hasVncPerm) btnVnc.Visible = hasVncPerm;
                if (btnVnc.Enabled != hasVncPerm) btnVnc.Enabled = hasVncPerm;
            }
            else
            {
                if (btnVnc.Visible) btnVnc.Visible = false;
                if (btnVnc.Enabled) btnVnc.Enabled = false;
            }
        }

        public void UpdateView(FullMachineStatus status)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateView(status)));
                return;
            }

            // SPEED OPTİMİZASYON: Kontrol görünürlükleri değişirken arayüzün kasılmasını önler
            this.SuspendLayout();

            switch (status.ConnectionState)
            {
                case ConnectionStatus.Connected:
                    if (picConnection.Image != _originalbaglantivarIcon) picConnection.Image = _originalbaglantivarIcon;
                    if (!picPlay.Visible) picPlay.Visible = true;
                    if (!picPause.Visible) picPause.Visible = true;
                    if (!picAlarm.Visible) picAlarm.Visible = true;
                    if (!btnVnc.Visible) btnVnc.Visible = true;
                    if (!btnInfo.Visible) btnInfo.Visible = true;
                    break;

                case ConnectionStatus.Connecting:
                    if (picConnection.Image != _originalbaglantiyokIcon) picConnection.Image = _originalbaglantiyokIcon;
                    if (picPlay.Visible) picPlay.Visible = false;
                    if (picPause.Visible) picPause.Visible = false;
                    if (picAlarm.Visible) picAlarm.Visible = false;
                    if (btnVnc.Visible) btnVnc.Visible = false;
                    if (btnInfo.Visible) btnInfo.Visible = false;
                    break;

                case ConnectionStatus.ConnectionLost:
                case ConnectionStatus.Disconnected:
                    if (picConnection.Image != _originalbaglantiyokIcon) picConnection.Image = _originalbaglantiyokIcon;
                    if (picPlay.Visible) picPlay.Visible = false;
                    if (picPause.Visible) picPause.Visible = false;
                    if (picAlarm.Visible) picAlarm.Visible = false;
                    if (btnVnc.Visible) btnVnc.Visible = false;
                    if (btnInfo.Visible) btnInfo.Visible = false;
                    ClearData();
                    this.ResumeLayout(true);
                    return;
            }

            // State Check: Sadece değer farklıysa ata (Gereksiz render döngüleri engellenir)
            if (lblRecipeNameValue.Text != status.RecipeName) lblRecipeNameValue.Text = status.RecipeName;
            if (lblOperatorValue.Text != status.OperatorIsmi) lblOperatorValue.Text = status.OperatorIsmi;

            // ÇÖZÜM: Bulunmayan AlternatifAdimAdi kaldırıldı, orijinal model mülkiyetiniz getirildi.
            string currentStepText = status.manuel_status ? "Working - Manuel" : status.AktifAdimAdi;
            if (lblStepValue.Text != currentStepText) lblStepValue.Text = currentStepText;

            if (lblMachineNameValue.Text != status.MachineName) lblMachineNameValue.Text = status.MachineName;
            if (lblMachineIdValue.Text != this.MachineUserDefinedId) lblMachineIdValue.Text = this.MachineUserDefinedId;

            if (status.HasActiveAlarm)
            {
                if (picAlarm.Image != _originalAlarmIcon) picAlarm.Image = _originalAlarmIcon;

                if (picPause.Visible != status.IsPaused) picPause.Visible = status.IsPaused;
                if (picPause.Visible && picPause.Image != _originalPauseIcon) picPause.Image = _originalPauseIcon;

                bool shouldPlayShow = status.IsInRecipeMode && !status.IsPaused && status.manuel_status;
                if (picPlay.Visible != shouldPlayShow) picPlay.Visible = shouldPlayShow;

                if (progressBar.Value > 0) _lastValidProgress = progressBar.Value;

                if (!_isDryingMachine)
                {
                    if (progressBar.Value != _lastValidProgress) progressBar.Value = _lastValidProgress;
                    string pctText = $"{_lastValidProgress} %";
                    if (lblPercentage.Text != pctText) lblPercentage.Text = pctText;
                }
            }
            else
            {
                if (picAlarm.Image != _originalAlarmyokIcon) picAlarm.Image = _originalAlarmyokIcon;

                bool shouldPlayShow = status.IsInRecipeMode && !status.IsPaused;
                if (picPlay.Visible != shouldPlayShow) picPlay.Visible = shouldPlayShow;
                if (picPlay.Visible && picPlay.Image != _originalPlayIcon) picPlay.Image = _originalPlayIcon;

                if (picPause.Visible != status.IsPaused) picPause.Visible = status.IsPaused;
                if (picPause.Visible && picPause.Image != _originalPauseIcon) picPause.Image = _originalPauseIcon;

                if (!_isDryingMachine)
                {
                    _lastValidProgress = Math.Max(0, Math.Min(100, (int)status.ProsesYuzdesi));
                    if (progressBar.Value != _lastValidProgress) progressBar.Value = _lastValidProgress;
                    string pctText = $"{_lastValidProgress} %";
                    if (lblPercentage.Text != pctText) lblPercentage.Text = pctText;
                }
            }

            ApplyPermissions(status.ConnectionState);

            this.ResumeLayout(true);
        }

        private void ClearData()
        {
            string noConnectionText = "---";
            lblRecipeNameValue.Text = noConnectionText;
            lblOperatorValue.Text = noConnectionText;
            lblStepValue.Text = noConnectionText;
            lblMachineNameValue.Text = this.MachineName;
            lblMachineIdValue.Text = this.MachineUserDefinedId;

            if (!_isDryingMachine)
            {
                progressBar.Value = 0;
                lblPercentage.Text = "0 %";
            }

            picPlay.Visible = false;
            picPause.Visible = false;
            picAlarm.Visible = false;
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            DetailsRequested?.Invoke(this, EventArgs.Empty);
        }

        // ÇÖZÜM: Hatalı olan nokta (.) karakteri, alt çizgi (_) ile değiştirilerek metod imzası düzeltildi.
        private void btnVnc_Click(object sender, EventArgs e)
        {
            VncRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}