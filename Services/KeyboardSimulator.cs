using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MouseRecorderWpf.Services
{
    public static class KeyboardSimulator
    {
        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern short VkKeyScan(char ch);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public INPUTUNION u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUTUNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
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

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        public static void KeyDown(short keyCode)
        {
            var scanCode = MapVirtualKey((uint)keyCode, 0);
            var input = CreateKeyboardInput((ushort)keyCode, (ushort)scanCode, KEYEVENTF_KEYDOWN);
            INPUT[] inputs = { input };
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(10);
        }

        public static void KeyUp(short keyCode)
        {
            var scanCode = MapVirtualKey((uint)keyCode, 0);
            var input = CreateKeyboardInput((ushort)keyCode, (ushort)scanCode, KEYEVENTF_KEYUP);
            INPUT[] inputs = { input };
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(10);
        }

        public static void KeyPress(short keyCode)
        {
            var scanCode = MapVirtualKey((uint)keyCode, 0);
            var downInput = CreateKeyboardInput((ushort)keyCode, (ushort)scanCode, KEYEVENTF_KEYDOWN);
            var upInput = CreateKeyboardInput((ushort)keyCode, (ushort)scanCode, KEYEVENTF_KEYUP);
            INPUT[] inputs = { downInput, upInput };
            SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(50);
        }

        public static void KeyDown(string keyName)
        {
            short keyCode = GetKeyCode(keyName);
            if (keyCode != 0)
                KeyDown(keyCode);
        }

        public static void KeyUp(string keyName)
        {
            short keyCode = GetKeyCode(keyName);
            if (keyCode != 0)
                KeyUp(keyCode);
        }

        public static void KeyPress(string keyName)
        {
            short keyCode = GetKeyCode(keyName);
            if (keyCode != 0)
                KeyPress(keyCode);
        }

        public static short GetKeyCode(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
                return 0;

            keyName = keyName.Trim().ToUpper();

            return keyName switch
            {
                "BACKSPACE" => 0x08,
                "TAB" => 0x09,
                "ENTER" => 0x0D,
                "RETURN" => 0x0D,
                "SHIFT" => 0x10,
                "LSHIFT" => 0xA0,
                "RSHIFT" => 0xA1,
                "CONTROL" => 0x11,
                "CTRL" => 0x11,
                "LCONTROL" => 0xA2,
                "LCTRL" => 0xA2,
                "RCONTROL" => 0xA3,
                "RCTRL" => 0xA3,
                "ALT" => 0x12,
                "LMENU" => 0xA4,
                "LALT" => 0xA4,
                "RMENU" => 0xA5,
                "RALT" => 0xA5,
                "PAUSE" => 0x13,
                "CAPSLOCK" => 0x14,
                "ESCAPE" => 0x1B,
                "ESC" => 0x1B,
                "SPACE" => 0x20,
                "PAGEUP" => 0x21,
                "PAGEDOWN" => 0x22,
                "END" => 0x23,
                "HOME" => 0x24,
                "LEFT" => 0x25,
                "UP" => 0x26,
                "RIGHT" => 0x27,
                "DOWN" => 0x28,
                "SELECT" => 0x29,
                "PRINT" => 0x2A,
                "EXECUTE" => 0x2B,
                "PRINTSCREEN" => 0x2C,
                "PRTSC" => 0x2C,
                "INSERT" => 0x2D,
                "INS" => 0x2D,
                "DELETE" => 0x2E,
                "DEL" => 0x2E,
                "HELP" => 0x2F,
                "0" => 0x30,
                "1" => 0x31,
                "2" => 0x32,
                "3" => 0x33,
                "4" => 0x34,
                "5" => 0x35,
                "6" => 0x36,
                "7" => 0x37,
                "8" => 0x38,
                "9" => 0x39,
                "A" => 0x41,
                "B" => 0x42,
                "C" => 0x43,
                "D" => 0x44,
                "E" => 0x45,
                "F" => 0x46,
                "G" => 0x47,
                "H" => 0x48,
                "I" => 0x49,
                "J" => 0x4A,
                "K" => 0x4B,
                "L" => 0x4C,
                "M" => 0x4D,
                "N" => 0x4E,
                "O" => 0x4F,
                "P" => 0x50,
                "Q" => 0x51,
                "R" => 0x52,
                "S" => 0x53,
                "T" => 0x54,
                "U" => 0x55,
                "V" => 0x56,
                "W" => 0x57,
                "X" => 0x58,
                "Y" => 0x59,
                "Z" => 0x5A,
                "LWIN" => 0x5B,
                "RWIN" => 0x5C,
                "WIN" => 0x5B,
                "APPS" => 0x5D,
                "SLEEP" => 0x5F,
                "NUMPAD0" => 0x60,
                "NUMPAD1" => 0x61,
                "NUMPAD2" => 0x62,
                "NUMPAD3" => 0x63,
                "NUMPAD4" => 0x64,
                "NUMPAD5" => 0x65,
                "NUMPAD6" => 0x66,
                "NUMPAD7" => 0x67,
                "NUMPAD8" => 0x68,
                "NUMPAD9" => 0x69,
                "MULTIPLY" => 0x6A,
                "*" => 0x6A,
                "ADD" => 0x6B,
                "+" => 0x6B,
                "SEPARATOR" => 0x6C,
                "SUBTRACT" => 0x6D,
                "DECIMAL" => 0x6E,
                "DIVIDE" => 0x6F,
                "/" => 0x6F,
                "F1" => 0x70,
                "F2" => 0x71,
                "F3" => 0x72,
                "F4" => 0x73,
                "F5" => 0x74,
                "F6" => 0x75,
                "F7" => 0x76,
                "F8" => 0x77,
                "F9" => 0x78,
                "F10" => 0x79,
                "F11" => 0x7A,
                "F12" => 0x7B,
                "NUMLOCK" => 0x90,
                "SCROLLLOCK" => 0x91,
                "SEMICOLON" => 0xBA,
                ";" => 0xBA,
                "EQUAL" => 0xBB,
                "=" => 0xBB,
                "COMMA" => 0xBC,
                "," => 0xBC,
                "MINUS" => 0xBD,
                "-" => 0xBD,
                "PERIOD" => 0xBE,
                "." => 0xBE,
                "SLASH" => 0xBF,
                "GRAVE" => 0xC0,
                "`" => 0xC0,
                "LBRACKET" => 0xDB,
                "[" => 0xDB,
                "BACKSLASH" => 0xDC,
                "\\" => 0xDC,
                "RBRACKET" => 0xDD,
                "]" => 0xDD,
                "APOSTROPHE" => 0xDE,
                "'" => 0xDE,
                _ => 0
            };
        }

        private static INPUT CreateKeyboardInput(ushort keyCode, ushort scanCode, uint flags)
        {
            return new INPUT
            {
                type = 1,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = keyCode,
                        wScan = scanCode,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
        }
    }
}