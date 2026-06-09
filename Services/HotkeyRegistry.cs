using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MouseRecorderWpf.Services
{
    public static class HotkeyRegistry
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static HookProc? hookProc;
        private static IntPtr hookId = IntPtr.Zero;
        private static readonly Dictionary<int, List<Action>> keyHandlers = new Dictionary<int, List<Action>>();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public static void Register(int keyCode, Action handler)
        {
            if (!keyHandlers.ContainsKey(keyCode))
            {
                keyHandlers[keyCode] = new List<Action>();
            }
            keyHandlers[keyCode].Add(handler);

            if (hookId == IntPtr.Zero)
            {
                InstallHook();
            }
        }

        public static void Unregister(int keyCode, Action handler)
        {
            if (keyHandlers.ContainsKey(keyCode))
            {
                keyHandlers[keyCode].Remove(handler);
                if (keyHandlers[keyCode].Count == 0)
                {
                    keyHandlers.Remove(keyCode);
                }
            }

            if (keyHandlers.Count == 0)
            {
                UninstallHook();
            }
        }

        private static void InstallHook()
        {
            hookProc = KeyboardHookCallback;
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                hookId = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc!, GetModuleHandle(curModule?.ModuleName), 0);
            }
        }

        private static void UninstallHook()
        {
            UnhookWindowsHookEx(hookId);
            hookId = IntPtr.Zero;
        }

        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam.ToInt32() == WM_KEYDOWN || wParam.ToInt32() == WM_SYSKEYDOWN))
            {
                var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                int keyCode = (int)hookStruct.vkCode;

                if (keyHandlers.TryGetValue(keyCode, out var handlers))
                {
                    foreach (var handler in handlers)
                    {
                        handler?.Invoke();
                    }
                    return (IntPtr)1;
                }
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }
    }
}
