// TekstilScada.Core/Core/LicenseManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;

namespace TekstilScada.Core
{
    public class LicenseData
    {
        public string HardwareKey { get; set; }
        public int MachineLimit { get; set; }
        public string Signature { get; set; }
        public string EncryptedConnectionString { get; set; }
        public int? TrialMinutes { get; set; }
    }

    public static class LicenseManager
    {
        // SECURITY NOTE: Replace the public key here with your own generated key.
        private const string PublicKeyXml = "<RSAKeyValue><Modulus>yck6I5qC/8sWOzOOiJx985LZwUCX+MIcYN5ymdsfCq8SjHhZleV7ZSN6LmChihhDQNLHZjqV7rhY/n+509NYI8aWILtDAI8j2RJNJFZcSMLEsFovEj+ZXqCVqOk/djDAbHSK/Ty3hbCpG4mIAooSqr4NF2qlNwTu1hDCj/gjX8Y2xZp9J1T3VnuKrU/U32XteZLcB2FH9kU+AeM8hkFqK7SaShaxahCFFXr3DJU6OF7ULMed1Efq0vOyp1WDurfOKH0zlbSnZ4GnhfXBN9+WXVdtzBpyYv0AUuwGm6umEnIvaeBEDgPrTSTeJGVLv3G5QMc2E13YkMMTOUMXVCSwgQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        private const string UsageTrackerFile = "system_cache.bin";

        // Win32 API sarmalayıcısı (WMI çökerse C: sürücüsünün seri numarasını almak için)
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetVolumeInformation(
            string rootPathName,
            StringBuilder volumeNameBuffer,
            int volumeNameSize,
            out uint volumeSerialNumber,
            out uint maximumComponentLength,
            out uint fileSystemFlags,
            StringBuilder fileSystemNameBuffer,
            int nFileSystemNameSize);

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

                // Deneme süresi kontrolü
                if (licenseData.TrialMinutes.HasValue)
                {
                    int usedMinutes = GetUsedMinutes();
                    int allowedMinutes = licenseData.TrialMinutes.Value;

                    if (usedMinutes >= allowedMinutes)
                    {
                        return (false, $"Deneme kullanım süreniz ({allowedMinutes} dakika) sona erdi. Satın almak için satış temsilcisi ile irtibata geçiniz.", null);
                    }
                }

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
                string decryptedStr = DecryptConnectionString(encryptedStr);
                if (int.TryParse(decryptedStr, out int minutes)) return minutes;
                return 0;
            }
            catch { return 999999; } // Dosya kurcalandıysa kilitle
        }

        public static void AddUsedMinute()
        {
            try
            {
                int currentMinutes = GetUsedMinutes();
                currentMinutes += 1;

                string encryptedStr = EncryptData(currentMinutes.ToString());

                if (File.Exists(UsageTrackerFile))
                {
                    File.SetAttributes(UsageTrackerFile, FileAttributes.Normal);
                }

                File.WriteAllText(UsageTrackerFile, encryptedStr);
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
            byte[] key = Encoding.UTF8.GetBytes("mysupersecretkeythatis32byteslon");
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
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        // ASLA ÇÖKMEYEN YENİ NESİL DONANIM ANAHTARI ÜRETİCİSİ
        public static string GenerateHardwareKey()
        {
            try
            {
                string motherboardId = GetHardwareInfo("Win32_BaseBoard", "SerialNumber");
                string biosId = GetHardwareInfo("Win32_BIOS", "SerialNumber");
                string diskId = GetSystemDiskSerial();

                // Yedek Mekanizma 1: Boş kalan donanımlara hata fırlatmak yerine varsayılan etiket ata
                if (string.IsNullOrEmpty(motherboardId)) motherboardId = "BASEBOARD_UNKNOWN";
                if (string.IsNullOrEmpty(biosId)) biosId = "BIOS_UNKNOWN";

                // Yedek Mekanizma 2: Eğer fiziksel disk WMI sorgusu boş döndüyse Win32 API ile C Sürücü nosunu dene
                if (string.IsNullOrEmpty(diskId))
                {
                    diskId = GetVolumeSerialNumber();
                    if (string.IsNullOrEmpty(diskId)) diskId = "DISK_UNKNOWN";
                }

                // Yedek Mekanizma 3: Eğer WMI komple çökmüşse, Windows'un benzersiz kurumsal GUID değerini çek
                if (motherboardId == "BASEBOARD_UNKNOWN" && biosId == "BIOS_UNKNOWN" && diskId == "DISK_UNKNOWN")
                {
                    diskId = GetWindowsRegistryMachineGuid();
                }

                string combinedString = $"{motherboardId}|{biosId}|{diskId}".Trim();

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedString));
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
                    return builder.ToString().ToUpper();
                }
            }
            catch
            {
                // En kötü senaryoda (Kritik OS hataları) acil durum registry anahtarını döndürerek çöküşü engelle
                return GetWindowsRegistryMachineGuid();
            }
        }

        private static string GetSystemDiskSerial()
        {
            try
            {
                string systemDrive = Path.GetPathRoot(Environment.SystemDirectory).Substring(0, 2);
                string partitionQuery = $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{systemDrive}'}} WHERE AssocClass = Win32_LogicalDiskToPartition";
                ManagementObjectSearcher partitionSearcher = new ManagementObjectSearcher(partitionQuery);

                foreach (ManagementObject partition in partitionSearcher.Get())
                {
                    string diskQuery = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass = Win32_DiskDriveToDiskPartition";
                    ManagementObjectSearcher diskSearcher = new ManagementObjectSearcher(diskQuery);

                    foreach (ManagementObject disk in diskSearcher.Get())
                    {
                        return disk["SerialNumber"].ToString().Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                LogToFile($"System Disk Error: {ex.Message}");
            }
            return "";
        }

        private static string GetVolumeSerialNumber()
        {
            try
            {
                string systemDrive = Path.GetPathRoot(Environment.SystemDirectory);
                uint serialNum;
                if (GetVolumeInformation(systemDrive, null, 0, out serialNum, out _, out _, null, 0))
                {
                    return serialNum.ToString("X");
                }
            }
            catch (Exception ex)
            {
                LogToFile($"Volume Serial Error: {ex.Message}");
            }
            return "";
        }

        private static string GetWindowsRegistryMachineGuid()
        {
            try
            {
                using (RegistryKey localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (RegistryKey rgbKey = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                    {
                        if (rgbKey != null)
                        {
                            object machineGuid = rgbKey.GetValue("MachineGuid");
                            if (machineGuid != null) return machineGuid.ToString().ToUpper().Replace("-", "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogToFile($"Registry GUID Error: {ex.Message}");
            }
            return "SAFE_BACKUP_HARDWARE_KEY_2026";
        }

        private static string GetHardwareInfo(string wmiClass, string wmiProperty)
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT * FROM {wmiClass}");
                var list = new List<string>();
                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj[wmiProperty] != null)
                    {
                        list.Add(obj[wmiProperty].ToString().Trim());
                    }
                }
                list.Sort();
                return list.Count > 0 ? list[0] : "";
            }
            catch (Exception ex)
            {
                LogToFile($"WMI access error - Class: {wmiClass}, Error: {ex.Message}");
                return "";
            }
        }

        private static void LogToFile(string logMessage)
        {
            try
            {
                string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                string logFilePath = Path.Combine(logDirectory, "hardware_log.txt");
                string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {logMessage}{Environment.NewLine}";
                File.AppendAllText(logFilePath, formattedMessage);
            }
            catch { }
        }
    }
}