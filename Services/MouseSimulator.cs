using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MouseRecorderWpf.Models;

namespace MouseRecorderWpf.Services
{
    public static class MouseSimulator
    {
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint Type;
            public INPUTUNION Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUTUNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public static Point GetCurrentPosition()
        {
            GetCursorPos(out var pos);
            return new Point(pos.X, pos.Y);
        }

        public static async Task MoveMouseSmoothAsync(int targetX, int targetY, double speedMultiplier, CancellationToken token = default)
        {
            var currentPos = GetCurrentPosition();
            int startX = currentPos.X;
            int startY = currentPos.Y;

            int dx = targetX - startX;
            int dy = targetY - startY;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < 5)
            {
                SetCursorPos(targetX, targetY);
                return;
            }

            double moveTime = (80 + distance * 0.15) / speedMultiplier;
            moveTime = Math.Min(moveTime, 300 / speedMultiplier);

            int steps = Math.Max((int)(distance / 5), 5);
            int delay = Math.Max((int)(moveTime / steps), 1);

            for (int i = 1; i <= steps; i++)
            {
                if (token.IsCancellationRequested)
                    break;

                double t = (double)i / steps;
                double eased = t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;

                int newX = startX + (int)(dx * eased);
                int newY = startY + (int)(dy * eased);

                SetCursorPos(newX, newY);

                if (delay > 0)
                {
                    await Task.Delay(delay, token);
                }
            }

            SetCursorPos(targetX, targetY);
        }

        public static void MoveMouse(int x, int y)
        {
            SetCursorPos(x, y);
        }

        public static void LeftClick()
        {
            SendMouseInput(MOUSEEVENTF_LEFTDOWN);
            Thread.Sleep(30);
            SendMouseInput(MOUSEEVENTF_LEFTUP);
        }

        public static void RightClick()
        {
            SendMouseInput(MOUSEEVENTF_RIGHTDOWN);
            Thread.Sleep(30);
            SendMouseInput(MOUSEEVENTF_RIGHTUP);
        }

        public static void DoubleClick()
        {
            LeftClick();
            Thread.Sleep(60);
            LeftClick();
        }

        public static void MiddleClick()
        {
            SendMouseInput(MOUSEEVENTF_MIDDLEDOWN);
            Thread.Sleep(30);
            SendMouseInput(MOUSEEVENTF_MIDDLEUP);
        }

        public static void PerformAction(MouseAction action)
        {
            MoveMouse(action.Position.X, action.Position.Y);
            Thread.Sleep(50);

            switch (action.ActionType)
            {
                case MouseActionType.LeftClick:
                    LeftClick();
                    break;
                case MouseActionType.RightClick:
                    RightClick();
                    break;
                case MouseActionType.DoubleClick:
                    DoubleClick();
                    break;
                case MouseActionType.MiddleClick:
                    MiddleClick();
                    break;
            }
        }

        private static void SendMouseInput(uint flags)
        {
            var input = new INPUT
            {
                Type = 0,
                Data = new INPUTUNION
                {
                    Mouse = new MOUSEINPUT
                    {
                        dx = 0,
                        dy = 0,
                        mouseData = 0,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            INPUT[] inputs = { input };
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}
