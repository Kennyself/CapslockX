using System.Runtime.InteropServices;

namespace CapslockX.Core.Native;

/// <summary>
/// Win32 structures used by CapslockX.
/// </summary>

/// <summary>Low-level keyboard hook structure passed to WH_KEYBOARD_LL callback.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct KBDLLHOOKSTRUCT
{
    public int vkCode;       // Virtual key code
    public int scanCode;     // Hardware scan code
    public int flags;        // LLKHF_* flags
    public int time;         // Timestamp
    public IntPtr dwExtraInfo;
}

/// <summary>Represents a point (x, y).</summary>
[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;
}

/// <summary>Represents a rectangle (left, top, right, bottom).</summary>
[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}

/// <summary>Keyboard input structure for SendInput.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct KEYBDINPUT
{
    public ushort wVk;
    public ushort wScan;
    public uint dwFlags;
    public uint time;
    public IntPtr dwExtraInfo;
}

/// <summary>Mouse input structure for SendInput.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct MOUSEINPUT
{
    public int dx;
    public int dy;
    public uint mouseData;
    public uint dwFlags;
    public uint time;
    public IntPtr dwExtraInfo;
}

/// <summary>Hardware input structure for SendInput.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct HARDWAREINPUT
{
    public uint uMsg;
    public ushort wParamL;
    public ushort wParamH;
}

/// <summary>Union of all input types for SendInput.</summary>
[StructLayout(LayoutKind.Explicit)]
public struct INPUT_UNION
{
    [FieldOffset(0)] public MOUSEINPUT mi;
    [FieldOffset(0)] public KEYBDINPUT ki;
    [FieldOffset(0)] public HARDWAREINPUT hi;
}

/// <summary>Input structure for SendInput API.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct INPUT
{
    public uint type;  // INPUT_MOUSE, INPUT_KEYBOARD, or INPUT_HARDWARE
    public INPUT_UNION u;
}

/// <summary>Monitor information structure.</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct MONITORINFO
{
    public int cbSize;
    public RECT rcMonitor;
    public RECT rcWork;
    public uint dwFlags;
}