namespace CapslockX.Core.Native;

/// <summary>
/// Win32 API constants used by CapslockX.
/// </summary>
public static class NativeConstants
{
    // ── Hook types ──────────────────────────────────────────
    /// <summary>Low-level keyboard hook (global, no DLL injection needed).</summary>
    public const int WH_KEYBOARD_LL = 13;

    // ── Window messages ─────────────────────────────────────
    public const int WM_KEYDOWN     = 0x0100;
    public const int WM_KEYUP       = 0x0101;
    public const int WM_SYSKEYDOWN  = 0x0104;
    public const int WM_SYSKEYUP    = 0x0105;

    // ── Virtual Key Codes ───────────────────────────────────
    public const int VK_LBUTTON    = 0x01;
    public const int VK_RBUTTON    = 0x02;
    public const int VK_CANCEL     = 0x03;
    public const int VK_MBUTTON    = 0x04;
    public const int VK_BACK       = 0x08;
    public const int VK_TAB        = 0x09;
    public const int VK_RETURN     = 0x0D;
    public const int VK_SHIFT      = 0x10;
    public const int VK_CONTROL    = 0x11;
    public const int VK_MENU       = 0x12;  // Alt
    public const int VK_PAUSE      = 0x13;
    public const int VK_CAPITAL    = 0x14;  // CapsLock
    public const int VK_ESCAPE     = 0x1B;
    public const int VK_SPACE      = 0x20;
    public const int VK_PRIOR      = 0x21;  // Page Up
    public const int VK_NEXT       = 0x22;  // Page Down
    public const int VK_END        = 0x23;
    public const int VK_HOME       = 0x24;
    public const int VK_LEFT       = 0x25;
    public const int VK_UP         = 0x26;
    public const int VK_RIGHT      = 0x27;
    public const int VK_DOWN       = 0x28;
    public const int VK_INSERT     = 0x2D;
    public const int VK_DELETE     = 0x2E;

    // Numbers
    public const int VK_0 = 0x30;
    public const int VK_1 = 0x31;
    public const int VK_2 = 0x32;
    public const int VK_3 = 0x33;
    public const int VK_4 = 0x34;
    public const int VK_5 = 0x35;
    public const int VK_6 = 0x36;
    public const int VK_7 = 0x37;
    public const int VK_8 = 0x38;
    public const int VK_9 = 0x39;

    // Letters
    public const int VK_A = 0x41;
    public const int VK_B = 0x42;
    public const int VK_C = 0x43;
    public const int VK_D = 0x44;
    public const int VK_E = 0x45;
    public const int VK_F = 0x46;
    public const int VK_G = 0x47;
    public const int VK_H = 0x48;
    public const int VK_I = 0x49;
    public const int VK_J = 0x4A;
    public const int VK_K = 0x4B;
    public const int VK_L = 0x4C;
    public const int VK_M = 0x4D;
    public const int VK_N = 0x4E;
    public const int VK_O = 0x4F;
    public const int VK_P = 0x50;
    public const int VK_Q = 0x51;
    public const int VK_R = 0x52;
    public const int VK_S = 0x53;
    public const int VK_T = 0x54;
    public const int VK_U = 0x55;
    public const int VK_V = 0x56;
    public const int VK_W = 0x57;
    public const int VK_X = 0x58;
    public const int VK_Y = 0x59;
    public const int VK_Z = 0x5A;

    // Windows keys
    public const int VK_LWIN = 0x5B;
    public const int VK_RWIN = 0x5C;

    // Function keys
    public const int VK_F1  = 0x70;
    public const int VK_F2  = 0x71;
    public const int VK_F3  = 0x72;
    public const int VK_F4  = 0x73;
    public const int VK_F5  = 0x74;
    public const int VK_F6  = 0x75;
    public const int VK_F7  = 0x76;
    public const int VK_F8  = 0x77;
    public const int VK_F9  = 0x78;
    public const int VK_F10 = 0x79;
    public const int VK_F11 = 0x7A;
    public const int VK_F12 = 0x7B;

    // Modifier / lock keys
    public const int VK_NUMLOCK  = 0x90;
    public const int VK_SCROLL   = 0x91;
    public const int VK_LSHIFT   = 0xA0;
    public const int VK_RSHIFT   = 0xA1;
    public const int VK_LCONTROL = 0xA2;
    public const int VK_RCONTROL = 0xA3;
    public const int VK_LMENU    = 0xA4;
    public const int VK_RMENU    = 0xA5;

    // Punctuation / symbols
    public const int VK_OEM_1      = 0xBA;  // ;: (US)
    public const int VK_OEM_PLUS   = 0xBB;  // =+
    public const int VK_OEM_COMMA  = 0xBC;  // ,<
    public const int VK_OEM_MINUS  = 0xBD;  // -_
    public const int VK_OEM_PERIOD = 0xBE;  // .>
    public const int VK_OEM_2      = 0xBF;  // /?
    public const int VK_OEM_3      = 0xC0;  // `~
    public const int VK_OEM_4      = 0xDB;  // [{
    public const int VK_OEM_5      = 0xDC;  // \|
    public const int VK_OEM_6      = 0xDD;  // ]}
    public const int VK_OEM_7      = 0xDE;  // '"

    // ── KBDLLHOOKSTRUCT flags ───────────────────────────────
    public const int LLKHF_EXTENDED = 0x01;
    public const int LLKHF_INJECTED = 0x10;
    public const int LLKHF_ALTDOWN  = 0x20;
    public const int LLKHF_UP       = 0x80;

    // ── SendInput flags ─────────────────────────────────────
    public const int INPUT_MOUSE    = 0;
    public const int INPUT_KEYBOARD = 1;
    public const int INPUT_HARDWARE = 2;

    public const int KEYEVENTF_EXTENDEDKEY = 0x0001;
    public const int KEYEVENTF_KEYUP       = 0x0002;
    public const int KEYEVENTF_UNICODE     = 0x0004;
    public const int KEYEVENTF_SCANCODE    = 0x0008;

    // ── Window management ───────────────────────────────────
    public static readonly IntPtr HWND_TOPMOST    = new(-1);
    public static readonly IntPtr HWND_NOTOPMOST  = new(-2);
    public static readonly IntPtr HWND_TOP        = new(0);

    public const uint SWP_NOSIZE     = 0x0001;
    public const uint SWP_NOMOVE     = 0x0002;
    public const uint SWP_NOZORDER   = 0x0004;
    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint SWP_SHOWWINDOW = 0x0040;

    // ── GetAsyncKeyState ────────────────────────────────────
    public const int KEY_DOWN_MASK = 0x8000;
}