// UI/LoginForm.cs
using System;
using System.Windows.Forms;
using Universalscada.Services;
using System.Net.Http; // YENİ: API çağrıları için
using System.Text; // YENİ: JSON işlemleri için
using Newtonsoft.Json; // YENİ: JSON işlemleri için (NuGet'ten yükleyin)
using Universalscada.Models; // YENİ: User modeli için

namespace Universalscada.UI
{
    public partial class LoginForm : Form
    {
        // === KALDIRILDI ===
        // private readonly AuthService _authService;

        // === YENİ ===
        private static readonly HttpClient _apiClient = new HttpClient();
        // !!! KENDİ WEBAPI ADRESİNİZLE DEĞİŞTİRİN !!!
        private const string API_BASE_URL = "http://localhost:7039/scadaHub";

        public LoginForm()
        {
            InitializeComponent();
            // _authService = new AuthService(); // KALDIRILDI

            // YENİ: API İstemcisini bir kez ayarla
            if (_apiClient.BaseAddress == null)
            {
                _apiClient.BaseAddress = new Uri(API_BASE_URL);
            }
        }

        // === DEĞİŞTİ: async void ===
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblError.Text = "Username and password cannot be blank.";
                return;
            }

            // === ESKİ KOD ===
            // bool success = _authService.Login(username, password);

            // === YENİ KOD: WebAPI'ye Bağlan ===
            lblError.Text = "Giriş yapılıyor, lütfen bekleyin...";
            this.Cursor = Cursors.WaitCursor;
            btnLogin.Enabled = false;

            try
            {
                // 1. API'ye gönderilecek giriş bilgilerini hazırla
                var loginData = new { Username = username, Password = password };
                var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");

                // 2. API'deki AuthController'a isteği gönder
                var response = await _apiClient.PostAsync("/api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    // 3. API'den gelen yanıtı (User ve Token) oku
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var loginResult = JsonConvert.DeserializeObject<LoginResponseObject>(jsonResponse);

                    // 4. Kullanıcıyı ve en önemlisi TOKEN'ı global CurrentUser'a kaydet
                    CurrentUser.Login(loginResult.User);
                    CurrentUser.Token = loginResult.Token;

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lblError.Text = "Username or password is incorrect!";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = $"API'ye bağlanılamadı: {ex.Message}";
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnLogin.Enabled = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // API'den gelen { "user": ..., "token": ... } yanıtını yakalamak için
        // yardımcı bir iç sınıf
        private class LoginResponseObject
        {
            public User User { get; set; }
            public string Token { get; set; }
        }
    }
}