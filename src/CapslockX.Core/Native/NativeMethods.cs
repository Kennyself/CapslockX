using System.Runtime.InteropServices;

namespace CapslockX.Core.Native;

/// <summary>
/// P/Invoke declarations for Win32 APIs used by CapslockX.
/// </summary>
public static class NativeMethods
{
    // ── Keyboard Hook ───────────────────────────────────────

    /// <summary>Low-level keyboard hook delegate type.</summary>
    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    /// <summary>
    /// Installs a hook procedure that monitors low-level keyboard input events.
    /// </summary>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(
        int idHook,
        LowLevelKeyboardProc lpfn,
        IntPtr hMod,
        uint dwThreadId);

    /// <summary>Removes a hook procedure installed by SetWindowsHookEx.</summary>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    /// <summary>
    /// Passes hook information to the next hook procedure in the chain.
    /// Must be called for every event unless you choose to suppress it.
    /// </summary>
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr CallNextHookEx(
        IntPtr hhk,
        int nCode,
        IntPtr wParam,
        IntPtr lParam);

    // ── Module Handle ───────────────────────────────────────

    /// <summary>
    /// Retrieves a module handle for the specified module.
    /// When lpModuleName is null, returns the handle for the file used
    /// to create the calling process.
    /// </summary>
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr GetModuleHandle(string? lpModuleName);

    // ── SendInput ───────────────────────────────────────────

    /// <summary>
    /// Synthesizes keystrokes, mouse motions, and button clicks.
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint SendInput(
        uint nInputs,
        [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs,
        int cbSize);

    /// <summary>
    /// Synthesizes a keystroke at a lower level than SendInput.
    /// Used for CapsLock toggle from within hook callbacks where SendInput may not work reliably.
    /// </summary>
    [DllImport("user32.dll")]
    public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    // ── Key State ───────────────────────────────────────────

    /// <summary>
    /// Determines whether a key is up or down at the time the function is called.
    /// Returns 0x8000 if the key is currently down (most significant bit set).
    /// </summary>
    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vKey);

    /// <summary>
    /// Retrieves the status of the specified virtual key (toggle state).
    /// Bit 0: toggle state (1 = on for CapsLock/NumLock/ScrollLock).
    /// </summary>
    [DllImport("user32.dll")]
    public static extern short GetKeyState(int nVirtKey);

    // ── Foreground Window ───────────────────────────────────

    /// <summary>Retrieves the handle to the foreground window.</summary>
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    /// <summary>
    /// Retrieves the identifier of the thread that created the specified window,
    /// and optionally the process that created the window.
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    // ── Window Management ───────────────────────────────────

    /// <summary>Changes the size, position, and Z-order of a window.</summary>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags);

    /// <summary>Retrieves the dimensions of the bounding rectangle of the specified window.</summary>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    /// <summary>Sets the specified window's show state.</summary>
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    /// <summary>Brings the thread that created the specified window into the foreground.</summary>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    // ── Monitor ─────────────────────────────────────────────

    /// <summary>Monitor enumeration callback delegate.</summary>
    public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    /// <summary>Enumerates display monitors.</summary>
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumDisplayMonitors(
        IntPtr hdc,
        IntPtr lprcClip,
        MonitorEnumProc lpfnEnum,
        IntPtr dwData);

    /// <summary>Retrieves information about a display monitor.</summary>
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    // ── Console Window ──────────────────────────────────────

    /// <summary>Allocates a new console for the calling process (for debug output).</summary>
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool AllocConsole();

    /// <summary>Frees the console associated with the calling process.</summary>
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FreeConsole();
}