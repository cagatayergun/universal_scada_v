// UI/Controls/MachineCard_Control.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using TekstilScada.Models;
using TekstilScada.Services;

namespace TekstilScada.UI.Controls
{
    public partial class MachineCard_Control : UserControl
    {
        private readonly Color _colorAlarm = Color.FromArgb(231, 76, 60);    // Kırmızı
        private readonly Color _colorRunning = Color.FromArgb(46, 204, 113);  // Yeşil
        private readonly Color _colorIdle = Color.FromArgb(243, 156, 18);     // Turuncu
        private readonly Color _colorStopped = Color.SlateGray;               // Gri
        public int MachineId { get; private set; }
        private int _lastValidProgress = 0;
        public string MachineUserDefinedId { get; private set; }
        public string MachineName { get; private set; }
        public string MachineType { get; private set; }

        public event EventHandler DetailsRequested;
        public event EventHandler VncRequested;

        private readonly Image _originalPlayIcon;
        private readonly Image _originalPauseIcon;
        private readonly Image _originalAlarmIcon;
        private readonly Image _originalAlarmyokIcon;
        private readonly Image _originalbaglantivarIcon;
        private readonly Image _originalbaglantiyokIcon;

        public MachineCard_Control(int machineId, string machineUserDefinedId, string machineName, int displayIndex, string machineType)
        {
            InitializeComponent();

            this.MachineId = machineId;
            this.MachineUserDefinedId = machineUserDefinedId;
            this.MachineName = machineName;
            this.MachineType = machineType;

            // HATA DÜZELTME: lblMachineNumber -> lblCraneNumber
            lblCraneNumber.Text = $"{displayIndex}.";

            // Vinç yapısında kapasite ve progress her zaman görünür
           

            _originalPlayIcon = Properties.Resource1.play2;
            _originalPauseIcon = Properties.Resource1.pause2;
            _originalAlarmIcon = Properties.Resource1.alarm_var;
            _originalAlarmyokIcon = Properties.Resource1.alarm_yok;
            _originalbaglantivarIcon = Properties.Resource1.malkan_baglanti_2;
            _originalbaglantiyokIcon = Properties.Resource1.malkan_baglanti;

            UpdateView(new FullMachineStatus { ConnectionState = ConnectionStatus.Disconnected, MachineName = this.MachineName });
        }

        public void UpdateView(FullMachineStatus status)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateView(status)));
                return;
            }
            if (status.HasActiveAlarm)
            {
                pnlStatusIndicator.BackColor = _colorAlarm;
                lblStatus.Text = $"ALARM #{status.ActiveAlarmText}";
                lblStatus.ForeColor = _colorAlarm;
            }
            else
            {
                if (status.manuel_status)
                {
                    pnlStatusIndicator.BackColor = _colorRunning;
                    lblStatus.Text = "MANUEL SÜRÜŞ";
                    lblStatus.ForeColor = _colorRunning;
                }
                else if (status.IsInRecipeMode)
                {
                    pnlStatusIndicator.BackColor = _colorRunning;
                    lblStatus.Text = $"OTOMATİK - MOD {status.AktifAdimNo}";
                    lblStatus.ForeColor = _colorRunning;
                }
                else
                {
                    pnlStatusIndicator.BackColor = _colorStopped;
                    lblStatus.Text = "BEKLEMEDE";
                    lblStatus.ForeColor = _colorStopped;
                }
            }
            switch (status.ConnectionState)
            {
                case ConnectionStatus.Connected:
                    picConnection.Image = _originalbaglantivarIcon;
                    picPlay.Visible = true;
                    picPause.Visible = true;
                    picAlarm.Visible = true;
                    btnVnc.Visible = true;
                    btnInfo.Visible = true;
                    break;
                default:
                    picConnection.Image = _originalbaglantiyokIcon;
                    ClearData();
                    return;
            }

            // HATA DÜZELTMELERİ (Eski İsim -> Yeni İsim)

            // lblRecipeNameValue -> lblLoadValue (Yük)
            

            if (status.HasActiveAlarm)
            {
                picAlarm.Image = _originalAlarmIcon;
               
            }
            else
            {
                picAlarm.Image = _originalAlarmyokIcon;
                _lastValidProgress = Math.Max(0, Math.Min(100, (int)status.ProsesYuzdesi));
            }

            // lblCapacityUsage (Eski lblProcessing)
           

            picPlay.Visible = (status.IsInRecipeMode || status.manuel_status) && !status.IsPaused;
            picPause.Visible = status.IsPaused;

            ApplyPermissions();
        }

        private void ClearData()
        {
           
            picPlay.Visible = false;
            picPause.Visible = false;
            picAlarm.Visible = false;
        }

        // Diğer metodlar (ApplyPermissions, btnInfo_Click vb.) Designer isimlerine göre güncellenmiştir.
        private void ApplyPermissions()
        {
            btnVnc.Visible = PermissionService.HasAnyPermission(new List<int> { 4, 1000 });
            btnVnc.Enabled = btnVnc.Visible;
        }

        private void btnInfo_Click(object sender, EventArgs e) => DetailsRequested?.Invoke(this, EventArgs.Empty);
        private void btnVnc_Click(object sender, EventArgs e) => VncRequested?.Invoke(this, EventArgs.Empty);

        private void lblCraneIdTitle_Click(object sender, EventArgs e)
        {
                    }
    }
}