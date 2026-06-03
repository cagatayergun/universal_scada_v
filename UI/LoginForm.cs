// UI/LoginForm.cs
using System;
using System.Windows.Forms;
using Telemetry.Services;
using MaterialSkin;          // YENİ EKLENDİ: MaterialSkin ana kütüphanesi
using MaterialSkin.Controls; // YENİ EKLENDİ: MaterialForm bileşenleri için

namespace Telemetry.UI
{
    // Form yerine MaterialForm sınıfından türetiyoruz
    public partial class LoginForm : MaterialForm
    {
        private readonly AuthService _authService;

        public LoginForm()
        {
            InitializeComponent();
            _authService = new AuthService();

            // =========================================================================
            // MATERIALSKIN FORM ENTEGRASYONU
            // Formun üst barlarını ve pencere çerçevesini modern flat tasarıma dönüştürür.
            // =========================================================================
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this); // Bu formu temalandırma motoruna kaydet

            this.DoubleBuffered = true; // Ekran yüklenirken veya form taşınırken titremeyi engeller
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblError.Text = "Username and password cannot be blank.";
                return;
            }

            bool success = _authService.Login(username, password);

            if (success)
            {
                // Giriş başarılı, bu formu kapat ve ana formu aç
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                lblError.Text = "Username or password is incorrect!";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}