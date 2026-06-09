using System;
using System.Drawing;
using System.Runtime.InteropServices;
using MouseRecorderWpf.Models;
using MouseAction = MouseRecorderWpf.Models.MouseAction;

namespace MouseRecorderWpf.Services
{
    public class LowLevelMouseHook
    {
        public event EventHandler<MouseAction>? MouseAction;

        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONDBLCLK = 0x0206;
        private const int WM_MBUTTONDOWN = 0x0207;

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        private HookProc? proc;
        private IntPtr hookId = IntPtr.Zero;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        public void Start()
        {
            proc = HookCallback;
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                hookId = SetWindowsHookEx(WH_MOUSE_LL, proc!, GetModuleHandle(curModule?.ModuleName), 0);
            }
        }

        public void Stop()
        {
            UnhookWindowsHookEx(hookId);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int wm = wParam.ToInt32();
                
                bool isClick = wm is WM_LBUTTONDOWN or WM_LBUTTONDBLCLK or WM_RBUTTONDOWN or WM_RBUTTONDBLCLK or WM_MBUTTONDOWN;
                
                if (isClick)
                {
                    GetCursorPos(out var cursorPos);
                    Point position = new Point(cursorPos.X, cursorPos.Y);
                    DateTime timestamp = DateTime.Now;

                    MouseActionType actionType = wm switch
                    {
                        WM_LBUTTONDOWN => MouseActionType.LeftClick,
                        WM_LBUTTONDBLCLK => MouseActionType.DoubleClick,
                        WM_RBUTTONDOWN => MouseActionType.RightClick,
                        WM_RBUTTONDBLCLK => MouseActionType.RightClick,
                        WM_MBUTTONDOWN => MouseActionType.MiddleClick,
                        _ => MouseActionType.LeftClick
                    };

                    MouseAction?.Invoke(this, new MouseAction(actionType, position, timestamp));
                }
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }
    }
}
