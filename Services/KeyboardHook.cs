using System;
using System.Runtime.InteropServices;

namespace MouseRecorderWpf.Services
{
    public class KeyboardHook : IDisposable
    {
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookId = IntPtr.Zero;

        public event Action<string, bool>? KeyEvent;

        public bool IsHooked { get; private set; }

        public KeyboardHook()
        {
            _proc = HookCallback;
        }

        public void StartHook()
        {
            if (IsHooked) return;

            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                var moduleName = curModule?.ModuleName ?? string.Empty;
                _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(moduleName), 0);
                IsHooked = _hookId != IntPtr.Zero;
            }
        }

        public void StopHook()
        {
            if (!IsHooked) return;
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
            IsHooked = false;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_KEYUP))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                string keyName = GetKeyName(vkCode);
                bool isKeyDown = wParam == (IntPtr)WM_KEYDOWN;

                if (!string.IsNullOrEmpty(keyName))
                {
                    KeyEvent?.Invoke(keyName, isKeyDown);
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public static string GetKeyName(int vkCode)
        {
            return vkCode switch
            {
                0x08 => "BACKSPACE",
                0x09 => "TAB",
                0x0D => "ENTER",
                0x10 => "SHIFT",
                0x11 => "CTRL",
                0x12 => "ALT",
                0x1B => "ESC",
                0x20 => "SPACE",
                0x21 => "PAGEUP",
                0x22 => "PAGEDOWN",
                0x23 => "END",
                0x24 => "HOME",
                0x25 => "LEFT",
                0x26 => "UP",
                0x27 => "RIGHT",
                0x28 => "DOWN",
                0x2D => "INSERT",
                0x2E => "DELETE",
                0x30 => "0",
                0x31 => "1",
                0x32 => "2",
                0x33 => "3",
                0x34 => "4",
                0x35 => "5",
                0x36 => "6",
                0x37 => "7",
                0x38 => "8",
                0x39 => "9",
                0x41 => "A",
                0x42 => "B",
                0x43 => "C",
                0x44 => "D",
                0x45 => "E",
                0x46 => "F",
                0x47 => "G",
                0x48 => "H",
                0x49 => "I",
                0x4A => "J",
                0x4B => "K",
                0x4C => "L",
                0x4D => "M",
                0x4E => "N",
                0x4F => "O",
                0x50 => "P",
                0x51 => "Q",
                0x52 => "R",
                0x53 => "S",
                0x54 => "T",
                0x55 => "U",
                0x56 => "V",
                0x57 => "W",
                0x58 => "X",
                0x59 => "Y",
                0x5A => "Z",
                0x5B => "WIN",
                0x60 => "NUMPAD0",
                0x61 => "NUMPAD1",
                0x62 => "NUMPAD2",
                0x63 => "NUMPAD3",
                0x64 => "NUMPAD4",
                0x65 => "NUMPAD5",
                0x66 => "NUMPAD6",
                0x67 => "NUMPAD7",
                0x68 => "NUMPAD8",
                0x69 => "NUMPAD9",
                0x6A => "*",
                0x6B => "+",
                0x6D => "-",
                0x6E => ".",
                0x6F => "/",
                0x70 => "F1",
                0x71 => "F2",
                0x72 => "F3",
                0x73 => "F4",
                0x74 => "F5",
                0x75 => "F6",
                0x76 => "F7",
                0x77 => "F8",
                0x78 => "F9",
                0x79 => "F10",
                0x7A => "F11",
                0x7B => "F12",
                0xA0 => "LSHIFT",
                0xA1 => "RSHIFT",
                0xA2 => "LCTRL",
                0xA3 => "RCTRL",
                0xA4 => "LALT",
                0xA5 => "RALT",
                0xBA => ";",
                0xBB => "=",
                0xBC => ",",
                0xBD => "-",
                0xBE => ".",
                0xBF => "/",
                0xC0 => "`",
                0xDB => "[",
                0xDC => "\\",
                0xDD => "]",
                0xDE => "'",
                _ => ""
            };
        }

        public void Dispose()
        {
            StopHook();
        }
    }
}