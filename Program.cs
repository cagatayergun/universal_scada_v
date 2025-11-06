// Program.cs
using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Universalscada.core;
using Universalscada.UI;
using Universalscada.Properties;
using System.IO; // YENÝ: Hata yakalama için eklendi

namespace Universalscada
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // === HATA YAKALAMA ===
            // Uygulamanýn en baþýna genel hata yakalayýcýlarý ekleyin
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            // === HATA YAKALAMA SONU ===

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // === LÝSANS DOÐRULAMA (Sizin kodunuz) ===
            var (isValid, message, licenseData) = LicenseManager.ValidateLicense();
            if (!isValid)
            {
                // (isValid false ise LicenseManager zaten MessageBox göstermiþ olmalý)
                Application.Exit();
                return;
            }
            AppConfig.SetConnectionString(licenseData.EncryptedConnectionString);
            // === LÝSANS DOÐRULAMA BÝTÝÞÝ ===

            // === YENÝ GÝRÝÞ AKIÞI ===
            try
            {
                // 1. Önce Login formunu aç
                using (var loginForm = new LoginForm()) //
                {
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        // 2. Giriþ baþarýlý olduysa (CurrentUser dolduysa)
                        // Ana formu çalýþtýr
                        Application.Run(new MainForm()); //
                    }
                    else
                    {
                        // 3. Giriþ baþarýsýz olduysa veya iptal edildiyse
                        Application.Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            // === YENÝ GÝRÝÞ AKIÞI SONU ===

            // ESKÝ KOD:
            // Application.Run(new MainForm()); // Bu satýr artýk yukarýdaki try-catch içinde
        }

        // === HATA YAKALAMA METOTLARI ===
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private static void HandleException(Exception ex)
        {
            if (ex == null) return;
            string errorMessage = $"Beklenmeyen bir hata oluþtu:\r\n{ex.Message}\r\n\r\n{ex.StackTrace}";
            try
            {
                string logFilePath = Path.Combine(Application.StartupPath, "error_log.txt");
                File.AppendAllText(logFilePath, $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}] - {errorMessage}\r\n\r\n");
            }
            catch (Exception) { }
            MessageBox.Show(errorMessage, "Kritik Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }
}