// UIViews/ActionLogReport_Control.cs
using System;
using System.Linq;
using System.Windows.Forms;
using TekstilScada.Core.Services;
using TekstilScada.Repositories;

namespace TekstilScada.UIViews
{
    public partial class ActionLogReport_Control : UserControl
    {
        private readonly UserRepository _userRepository;

        public ActionLogReport_Control()
        {
            InitializeComponent();

            // --- GÜNCELLENEN KISIM ---
            // Otomatik boyutlandırma mantığını veri yüklendikten sonra çalışacak olaya bağlıyoruz.
            // Bu sayede sütun isimlerine (Details vb.) erişip özel ayar yapabiliriz.
            dataGridView1.DataBindingComplete += DataGridView1_DataBindingComplete;
            // -------------------------

            _userRepository = new UserRepository();

            // Kullanıcıları filtreleme için combobox'a yükle
            LoadUsers();

            // İlk açılışta verileri yükle
            btnFilter_Click(this, EventArgs.Empty);
        }

        // --- YENİ EKLENEN METOT: Sütun Genişliklerini Optimize Eden Kod ---
        private void DataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            var grid = sender as DataGridView;
            if (grid == null) return;

            // 1. Adım: Önce tüm sütunları içeriğine göre (başlık dahil) en dar hale getir.
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }

            // 2. Adım: Uzun metin içeren 'Details' (veya Detaylar) sütununu bul ve
            // ona "Kalan tüm boşluğu doldur" (Fill) emrini ver.
            // Not: ActionLogEntry modelindeki property ismi 'Details' olduğu varsayılmıştır.
            var detailsColumn = grid.Columns["Details"];
            if (detailsColumn != null)
            {
                detailsColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            else
            {
                // Eğer 'Details' isminde sütun yoksa, en son sütunu dolduracak şekilde ayarla (Genelde açıklama sondadır)
                if (grid.Columns.Count > 0)
                {
                    grid.Columns[grid.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            }
        }
        // -----------------------------------------------------------------

        private void LoadUsers()
        {
            var users = _userRepository.GetAllUsers();
            cmbUser.Items.Clear();
            cmbUser.Items.Add("All Users");
            foreach (var user in users)
            {
                cmbUser.Items.Add(user.Username);
            }
            cmbUser.SelectedIndex = 0;
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            DateTime? startDate = dtpStartDate.Value;
            DateTime? endDate = dtpEndDate.Value.AddDays(1).AddSeconds(-1);

            string username = cmbUser.SelectedIndex > 0 ? cmbUser.SelectedItem.ToString() : null;
            string details = string.IsNullOrEmpty(txtDetails.Text) ? null : txtDetails.Text;

            var logs = _userRepository.GetActionLogs(startDate, endDate, username, details);
            dataGridView1.DataSource = logs;
        }
    }
}