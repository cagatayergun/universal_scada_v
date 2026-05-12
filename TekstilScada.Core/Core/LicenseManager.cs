// TekstilScada.Core/Core/LicenseManager.cs
using System;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TekstilScada.Core
{
    public class LicenseData
    {
        public string HardwareKey { get; set; }
        public int MachineLimit { get; set; }
        public string Signature { get; set; }
        public string EncryptedConnectionString { get; set; } // NEWLY ADDED
        public int? TrialMinutes { get; set; } // Model dakika bazlı oldu
    }

    public static class LicenseManager
    {
        // SECURITY NOTE: Replace the public key here with your own generated key.
        private const string PublicKeyXml = "<RSAKeyValue><Modulus>yck6I5qC/8sWOzOOiJx985LZwUCX+MIcYN5ymdsfCq8SjHhZleV7ZSN6LmChihhDQNLHZjqV7rhY/n+509NYI8aWILtDAI8j2RJNJFZcSMLEsFovEj+ZXqCVqOk/djDAbHSK/Ty3hbCpG4mIAooSqr4NF2qlNwTu1hDCj/gjX8Y2xZp9J1T3VnuKrU/U32XteZLcB2FH9kU+AeM8hkFqK7SaShaxahCFFXr3DJU6OF7ULMed1Efq0vOyp1WDurfOKH0zlbSnZ4GnhfXBN9+WXVdtzBpyYv0AUuwGm6umEnIvaeBEDgPrTSTeJGVLv3G5QMc2E13YkMMTOUMXVCSwgQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        // Kullanılan dakikaların şifreli tutulacağı dosya
        private const string UsageTrackerFile = "system_cache.bin";
        public static (bool IsValid, string Message, LicenseData Data) ValidateLicense()
        {
            try
            {
                string currentHardwareKey = GenerateHardwareKey();
                if (string.IsNullOrEmpty(currentHardwareKey))
                {
                    return (false, "Could not retrieve hardware information.", null);
                }

                if (!File.Exists("license.lic"))
                {
                    return (false, "License file not found (license.lic).", null);
                }
                string licenseJson = File.ReadAllText("license.lic");
                var licenseData = JsonSerializer.Deserialize<LicenseData>(licenseJson);

                if (licenseData == null || string.IsNullOrEmpty(licenseData.Signature))
                {
                    return (false, "The license file is invalid.", null);
                }

                // İmza doğrulama
                string originalSignature = licenseData.Signature;
                licenseData.Signature = null;
                string unsignedDataJson = JsonSerializer.Serialize(licenseData);

                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(PublicKeyXml);
                    byte[] dataBytes = Encoding.UTF8.GetBytes(unsignedDataJson);
                    byte[] signatureBytes = Convert.FromBase64String(originalSignature);

                    if (!rsa.VerifyData(dataBytes, new SHA256CryptoServiceProvider(), signatureBytes))
                    {
                        return (false, "Invalid license signature. The file may have been tampered with.", null);
                    }
                }

                // Donanım anahtarı kontrolü
                if (licenseData.HardwareKey != currentHardwareKey)
                {
                    return (false, "The license is not valid for this computer.", null);
                }

                // ========================================================
                // YENİ: DOĞRUDAN DAKİKA KONTROLÜ
                // ========================================================
                if (licenseData.TrialMinutes.HasValue)
                {
                    int usedMinutes = GetUsedMinutes();

                    // Artık 60 ile çarpmaya gerek yok, veri zaten dakika olarak geliyor
                    int allowedMinutes = licenseData.TrialMinutes.Value;

                    if (usedMinutes >= allowedMinutes)
                    {
                        return (false, $"Deneme kullanım süreniz ({allowedMinutes} dakika) sona erdi. Satın almak için satış temsilcisi ile irtibata geçiniz.", null);
                    }
                }
                // ========================================================

                string connectionString = DecryptConnectionString(licenseData.EncryptedConnectionString);
                licenseData.EncryptedConnectionString = connectionString;

                return (true, "License successfully verified.", licenseData);
            }
            catch (Exception ex)
            {
                return (false, $"An unexpected error occurred during license verification: {ex.Message}", null);
            }
        }
        public static int GetUsedMinutes()
        {
            if (!File.Exists(UsageTrackerFile)) return 0;
            try
            {
                string encryptedStr = File.ReadAllText(UsageTrackerFile);
                string decryptedStr = DecryptConnectionString(encryptedStr); // Aynı AES şifrelemeyi kullanıyoruz
                if (int.TryParse(decryptedStr, out int minutes)) return minutes;
                return 0;
            }
            catch { return 999999; } // Dosya ile oynanmışsa maksimum süre verip kilitleriz
        }
        public static void AddUsedMinute()
        {
            try
            {
                int currentMinutes = GetUsedMinutes();
                currentMinutes += 1;

                string encryptedStr = EncryptData(currentMinutes.ToString());

                // KRİTİK DÜZELTME: Dosya zaten varsa ve gizliyse, üzerine yazabilmek için önce gizliliği kaldır
                if (File.Exists(UsageTrackerFile))
                {
                    File.SetAttributes(UsageTrackerFile, FileAttributes.Normal);
                }

                // Şimdi güvenle üzerine yazabiliriz
                File.WriteAllText(UsageTrackerFile, encryptedStr);

                // İşlem bitince tekrar gizle
                File.SetAttributes(UsageTrackerFile, FileAttributes.Hidden);
            }
            catch { }
        }
        private static string EncryptData(string plainText)
        {
            byte[] key = Encoding.UTF8.GetBytes("mysupersecretkeythatis32byteslon");
            byte[] iv = Encoding.UTF8.GetBytes("16-byte-vector-!");
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key; aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC; aesAlg.Padding = PaddingMode.PKCS7;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
        private static string DecryptConnectionString(string encryptedData)
        {
            // The key length must be 32 bytes (256 bits).
            // Make sure it is the same as the key in the license creation program.
            byte[] key = Encoding.UTF8.GetBytes("mysupersecretkeythatis32byteslon");

            // The initialization vector (IV) length must be 16 bytes (128 bits).
            // Make sure it is the same as the IV in the license creation program.
            byte[] iv = Encoding.UTF8.GetBytes("16-byte-vector-!");

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedData)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }


        // Method to generate the hardware key
        public static string GenerateHardwareKey()
        {
            try
            {
                string motherboardId = GetHardwareInfo("Win32_BaseBoard", "SerialNumber");
                string biosId = GetHardwareInfo("Win32_BIOS", "SerialNumber");

                // DEĞİŞEN KISIM BURASI:
                // Eski yöntem: string diskId = GetHardwareInfo("Win32_DiskDrive", "SerialNumber");

                // YENİ GÜVENLİ YÖNTEM (Sadece Windows Diski):
                string diskId = GetSystemDiskSerial();

                // Eğer disk ID boş geldiyse (WMI hatası vb.), lisans üretme ki güvenlik açığı olmasın.
                if (string.IsNullOrEmpty(diskId)) return null;

                string combinedString = $"{motherboardId}|{biosId}|{diskId}".Trim();

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedString));
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
                    return builder.ToString().ToUpper();
                }
            }
            catch { return null; }
        }
        private static string GetSystemDiskSerial()
        {
            try
            {
                // 1. Windows'un kurulu olduğu sürücü harfini bul (Örn: "C:")
                string systemDrive = Path.GetPathRoot(Environment.SystemDirectory).Substring(0, 2);

                // 2. Bu sürücü harfinin (LogicalDisk) hangi Bölümde (Partition) olduğunu bul
                string partitionQuery = $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{systemDrive}'}} WHERE AssocClass = Win32_LogicalDiskToPartition";
                ManagementObjectSearcher partitionSearcher = new ManagementObjectSearcher(partitionQuery);

                foreach (ManagementObject partition in partitionSearcher.Get())
                {
                    // 3. Bu Bölümün (Partition) hangi Fiziksel Diskte (DiskDrive) olduğunu bul
                    string diskQuery = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass = Win32_DiskDriveToDiskPartition";
                    ManagementObjectSearcher diskSearcher = new ManagementObjectSearcher(diskQuery);

                    foreach (ManagementObject disk in diskSearcher.Get())
                    {
                        // 4. İşte Windows'un olduğu Fiziksel Diski bulduk! Seri numarasını al.
                        return disk["SerialNumber"].ToString().Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                LogToFile($"System Disk Error: {ex.Message}");
            }

            return ""; // Bulunamazsa boş dön
        }
        private static string GetHardwareInfo(string wmiClass, string wmiProperty)
        {
            try
            {
                // Donanımları her zaman belirli bir özelliğe göre sırala (Örn: DeviceID veya Name)
                ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT * FROM {wmiClass}");

                var list = new List<string>();
                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj[wmiProperty] != null)
                    {
                        list.Add(obj[wmiProperty].ToString().Trim());
                    }
                }

                // Listeyi sırala ve her zaman ilkini al (Böylece USB takılsa bile sıra değişmez)
                list.Sort();
                return list.Count > 0 ? list[0] : "";
            }
            catch (Exception ex)
            {
                LogToFile($"WMI access error - Class: {wmiClass}, Error: {ex.Message}");
                // Hata durumunda null dönerek üst katmanın lisansı geçersiz saymasını değil, işlemi durdurmasını sağlayabilirsiniz.
                return "";
            }
        }
        private static void LogToFile(string logMessage)
        {
            // Creates a folder named "logs" in the application's working directory.
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // File path: [ApplicationDirectory]/logs/hardware_log.txt
            string logFilePath = Path.Combine(logDirectory, "hardware_log.txt");

            // Appends the message to the file with a timestamp.
            string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {logMessage}{Environment.NewLine}";
            File.AppendAllText(logFilePath, formattedMessage);
        }
    }
}