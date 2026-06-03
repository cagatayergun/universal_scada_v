// UIViews/ActionLogReport_Control.cs
using System;
using System.Linq;
using System.Windows.Forms;
using Telemetry.Core.Services;
using Telemetry.Repositories;

namespace Telemetry.UIViews
{
    public partial class ActionLogReport_Control : UserControl
    {
        private readonly UserRepository _userRepository;

        public ActionLogReport_Control()
        {
            InitializeComponent();

            // SPEED OPTİMİZASYON: Kontrol ilk yüklenirken veya sekme geçişlerinde titremeyi önler
            this.DoubleBuffered = true;

            // Otomatik boyutlandırma mantığını veri yüklendikten sonra çalışacak olaya bağlıyoruz.
            dataGridView1.DataBindingComplete += DataGridView1_DataBindingComplete;

            // SPEED OPTİMİZASYON: Binlerce log satırı arasında operatör hızlıca gezinirken 
            // tablonun kasılmasını, donmasını ve takılmasını tamamen engeller.
            EnableDoubleBuffer(dataGridView1);

            _userRepository = new UserRepository();

            // Kullanıcıları filtreleme için combobox'a yükle
            LoadUsers();

            // İlk açılışta verileri otomatik yükle
            btnFilter_Click(this, EventArgs.Empty);
        }

        private void DataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            var grid = sender as DataGridView;
            if (grid == null) return;

            // Performans için sütun genişlik hesaplamaları sırasında düzen motorunu askıya al
            grid.SuspendLayout();

            try
            {
                // 1. Adım: Önce tüm sütunları içeriğine göre (başlık dahil) en dar hale getir.
                foreach (DataGridViewColumn col in grid.Columns)
                {
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                }

                // 2. Adım: Uzun metin içeren 'Details' sütununu bul ve kalan tüm boşluğu doldur (Fill) emrini ver.
                var detailsColumn = grid.Columns["Details"];
                if (detailsColumn != null)
                {
                    detailsColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                else
                {
                    // Eğer 'Details' isminde sütun yoksa, en son sütunu dolduracak şekilde ayarla
                    if (grid.Columns.Count > 0)
                    {
                        grid.Columns[grid.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                }
            }
            finally
            {
                // Çizim kilidini kaldır ve yeni yerleşimi ekrana bas
                grid.ResumeLayout(true);
            }
        }

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
            if (this.IsDisposed) return;

            // SPEED OPTİMİZASYON: Veri tabanından yeni log listesi çekilip tabloya
            // bağlanırken arayüzün parça parça parıldayarak çizilmesini engeller.
            this.SuspendLayout();

            try
            {
                DateTime? startDate = dtpStartDate.Value;
                DateTime? endDate = dtpEndDate.Value.AddDays(1).AddSeconds(-1);

                string username = cmbUser.SelectedIndex > 0 ? cmbUser.SelectedItem.ToString() : null;
                string details = string.IsNullOrEmpty(txtDetails.Text) ? null : txtDetails.Text;

                // Veritabanı sorgusunu çalıştır ve bağla
                var logs = _userRepository.GetActionLogs(startDate, endDate, username, details);
                dataGridView1.DataSource = logs;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Log filtering error: {ex.Message}");
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }

        // SPEED OPTİMİZASYON: DataGridView'in korunan (protected) DoubleBuffered özelliğini aktif eden yansıtma metodu
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
                System.Diagnostics.Debug.WriteLine($"DoubleBuffering could not be enabled for {control.Name}: {ex.Message}");
            }
        }
    }
}