// UI/Views/UserSettings_Control.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telemetry.Models;
using Telemetry.Properties;
using Telemetry.Repositories;
using Telemetry.Services;
using MaterialSkin;          // YENİ EKLENDİ
using MaterialSkin.Controls; // YENİ EKLENDİ

namespace Telemetry.UI.Views
{
    public partial class UserSettings_Control : UserControl
    {
        private readonly UserRepository _repository;
        private List<User> _users;
        private List<Role> _allRoles;
        private User _selectedUser;

        public UserSettings_Control()
        {
            InitializeComponent();
            _repository = new UserRepository();

            // SPEED OPTİMİZASYON: Pencereler kaydırılırken ve sekmeler değişirken titremeyi engeller
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent; // Arka plan yönetimi üst ebeveyne devredildi
            EnableDoubleBuffer(dgvUsers);
            EnableDoubleBuffer(clbRoles);

            // =========================================================================
            // MODERNİZASYON: STANDART METİN VE LİSTE KUTULARINI DARK MODE UYARLAMA ADAPTÖRÜ
            // Parametre alanlarını modern grafit koyu arka plan şemasıyla pürüzsüzce eşitler.
            // =========================================================================
            Color controlBg = Color.FromArgb(44, 52, 64);    // Koyu grafit gri
            Color controlFg = Color.FromArgb(240, 240, 240); // Soft mat beyaz

            var textControls = new List<TextBox> { txtUsername, txtFullName, txtPassword };
            foreach (var txt in textControls)
            {
                if (txt != null)
                {
                    txt.BackColor = controlBg;
                    txt.ForeColor = controlFg;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }
            }

            if (clbRoles != null)
            {
                clbRoles.BackColor = controlBg;
                clbRoles.ForeColor = controlFg;
                clbRoles.BorderStyle = BorderStyle.None;
            }
        }

        private void UserSettings_Control_Load(object sender, EventArgs e)
        {
            LoadAllRoles();
            ApplyLocalization();
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLocalization();
        }

        public void ApplyLocalization()
        {
            groupBox1.Text = Resources.Userdetail;
            label1.Text = Resources.Username;
            label3.Text = Resources.namesurname;
            label4.Text = Resources.Password;
            chkIsActive.Text = Resources.Useractive;
            label5.Text = Resources.roller;
            btnNew.Text = Resources.New;
            btnSave.Text = Resources.Save;
            btnDelete.Text = Resources.Delete;
        }

        public void LoadAllRoles()
        {
            _allRoles = _repository.GetAllRoles() ?? new List<Role>();
            var filteredRoles = _allRoles.Where(r => r.Id != 1000).ToList();

            clbRoles.DataSource = null;
            clbRoles.Items.Clear();

            if (PermissionService.HasAnyPermission(new List<int> { 1000 }))
            {
                clbRoles.DataSource = _allRoles;
            }
            else
            {
                clbRoles.DataSource = filteredRoles;
            }

            clbRoles.DisplayMember = "RoleName";
            clbRoles.ValueMember = "Id";

            // Liste yenileme işlemini tetikle
            RefreshUserList();
        }

        // =========================================================================
        // OPTİMİZASYON: ASENKRON KULLANICI VE ROL VERİ TABANI TARAMA MOTORU
        // Operatör rolleri kontrol edilirken SCADA arayüzünün donması önlenmiştir.
        // =========================================================================
        private async void RefreshUserList()
        {
            if (this.IsDisposed) return;

            // UI yenileme esnasında yerleşim motorunu kitle (Kırpışma koruması)
            this.SuspendLayout();
            dgvUsers.SuspendLayout();

            try
            {
                var rawUsers = _repository.GetAllUsers() ?? new List<User>();
                bool hasMasterPermission = PermissionService.HasAnyPermission(new List<int> { 1000 });

                // Ağır SQL döngü işlemlerini asenkron olarak arka plan thread'ine (Task) geçiriyoruz
                _users = await Task.Run(() =>
                {
                    if (hasMasterPermission)
                    {
                        return rawUsers;
                    }

                    var filteredList = new List<User>();
                    foreach (var user in rawUsers)
                    {
                        var userRoles = _repository.GetUserRoles(user.Id) ?? new List<Role>();
                        // Eğer kullanıcının rolleri arasında 1000 ID'li Master rolü yoksa listeye ekle
                        if (!userRoles.Any(r => r.Id == 1000))
                        {
                            filteredList.Add(user);
                        }
                    }
                    return filteredList;
                });

                dgvUsers.DataSource = null;
                dgvUsers.DataSource = _users;

                // Grid Sütun Düzenlemeleri
                if (dgvUsers.Columns["Id"] != null) dgvUsers.Columns["Id"].Visible = false;
                if (dgvUsers.Columns["Roles"] != null) dgvUsers.Columns["Roles"].Visible = false;
                if (dgvUsers.Columns["RefreshToken"] != null) dgvUsers.Columns["RefreshToken"].Visible = false;
                if (dgvUsers.Columns["RefreshTokenExpiry"] != null) dgvUsers.Columns["RefreshTokenExpiry"].Visible = false;

                ConfigureGridAppearance();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing user list: {ex.Message}");
            }
            finally
            {
                dgvUsers.ResumeLayout(true);
                this.ResumeLayout(true);
            }
        }

        private void ConfigureGridAppearance()
        {
            dgvUsers.BorderStyle = BorderStyle.None;
            dgvUsers.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvUsers.EnableHeadersVisualStyles = false;
           
            dgvUsers.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;

            // Koyu/Açık Tema Koruyucu Grid Matrisi
            dgvUsers.BackgroundColor = isDark ? Color.FromArgb(30, 41, 59) : Color.White;
            dgvUsers.DefaultCellStyle.SelectionBackColor = isDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(219, 234, 254);
            dgvUsers.DefaultCellStyle.SelectionForeColor = isDark ? Color.FromArgb(240, 240, 240) : Color.FromArgb(15, 23, 42);
            dgvUsers.AlternatingRowsDefaultCellStyle.BackColor = isDark ? Color.FromArgb(38, 50, 68) : Color.FromArgb(248, 250, 252);

            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = isDark ? Color.FromArgb(15, 23, 42) : Color.FromArgb(241, 245, 249);
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);
            dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F);
            dgvUsers.ColumnHeadersHeight = 45;
            dgvUsers.RowTemplate.Height = 36;
        }

        private void dgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count > 0)
            {
                _selectedUser = dgvUsers.SelectedRows[0].DataBoundItem as User;
                if (_selectedUser != null)
                {
                    PopulateFields(_selectedUser);
                }
            }
        }

        private void PopulateFields(User user)
        {
            txtUsername.Text = user.Username;
            txtFullName.Text = user.FullName;
            chkIsActive.Checked = user.IsActive;
            txtPassword.Text = ""; // Güvenlik için şifre alanını temizle

            var userRoles = _repository.GetUserRoles(user.Id) ?? new List<Role>();
            for (int i = 0; i < clbRoles.Items.Count; i++)
            {
                if (clbRoles.Items[i] is Role role)
                {
                    bool hasRole = userRoles.Any(ur => ur.Id == role.Id);
                    clbRoles.SetItemChecked(i, hasRole);
                }
            }
        }

        private void ClearFields()
        {
            _selectedUser = null;
            dgvUsers.ClearSelection();
            txtUsername.Text = "";
            txtFullName.Text = "";
            txtPassword.Text = "";
            chkIsActive.Checked = true;
            for (int i = 0; i < clbRoles.Items.Count; i++)
            {
                clbRoles.SetItemChecked(i, false);
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show($"{Resources.usernamehard}", $"{Resources.EksikBilgi}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRoleIds = clbRoles.CheckedItems.OfType<Role>().Select(r => r.Id).ToList();

            try
            {
                if (_selectedUser == null) // Yeni Kayıt
                {
                    if (string.IsNullOrWhiteSpace(txtPassword.Text))
                    {
                        MessageBox.Show($"{Resources.Yenikullanıcıiçinsifrezorunludur}", $"{Resources.EksikBilgi}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var newUser = new User { Username = txtUsername.Text, FullName = txtFullName.Text, IsActive = chkIsActive.Checked };
                    _repository.AddUser(newUser, txtPassword.Text, selectedRoleIds);
                }
                else // Güncelleme
                {
                    _selectedUser.Username = txtUsername.Text;
                    _selectedUser.FullName = txtFullName.Text;
                    _selectedUser.IsActive = chkIsActive.Checked;
                    _repository.UpdateUser(_selectedUser, selectedRoleIds, txtPassword.Text);
                }
                RefreshUserList();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Resources.Kayıt_sırasında_hata_}{ex.Message}", $"{Resources.Error}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedUser == null) return;
            var result = MessageBox.Show($"'{_selectedUser.Username}' {Resources.kullanıcısını_silmek_istediğinizden_emin_misiniz_}", $"{Resources.Confirim}", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _repository.DeleteUser(_selectedUser.Id);
                    RefreshUserList();
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{Resources.Silmesırasındahata} {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // SPEED OPTİMİZASYON: GDI+ çizim kuyruğunu hızlandıran yansıtma (Reflection) metodu
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