using System.Runtime.InteropServices;

namespace WorkTimer;

internal static partial class NativeMethods
{
    // --- Keep-awake via SetThreadExecutionState ---

    private const uint ES_CONTINUOUS = 0x80000000;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;

    [LibraryImport("kernel32.dll")]
    private static partial uint SetThreadExecutionState(uint esFlags);

    public static void PreventSleep()
    {
        SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
    }

    public static void AllowSleep()
    {
        SetThreadExecutionState(ES_CONTINUOUS);
    }

    // --- Synthetic F16 key press via SendInput ---

    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public nint dwExtraInfo;
    }

    // Padding field required: Win32 INPUT is a union sized to MOUSEINPUT (40 bytes on x64).
    // Without it, KEYBDINPUT makes the struct too small and SendInput silently fails.
    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public KEYBDINPUT ki;
        public long padding;
    }

    [LibraryImport("user32.dll")]
    private static partial uint SendInput(uint nInputs, [In] INPUT[] pInputs, int cbSize);

    public static void SimulateKeyPress(ushort virtualKeyCode)
    {
        var inputs = new INPUT[]
        {
            new() { type = INPUT_KEYBOARD, ki = new KEYBDINPUT { wVk = virtualKeyCode } },
            new() { type = INPUT_KEYBOARD, ki = new KEYBDINPUT { wVk = virtualKeyCode, dwFlags = KEYEVENTF_KEYUP } },
        };
        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
    }

    // --- Foreground window helpers (NotifyIcon context-menu z-order fix, KB135788) ---

    public const uint WM_NULL = 0x0000;

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetForegroundWindow(IntPtr hWnd);

    [LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
}
