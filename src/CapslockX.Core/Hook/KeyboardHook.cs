using System.Diagnostics;
using System.Runtime.InteropServices;
using CapslockX.Core.Native;

namespace CapslockX.Core.Hook;

/// <summary>
/// Wraps the Win32 WH_KEYBOARD_LL low-level keyboard hook.
/// Runs on the calling thread — requires a message pump (WPF/WinForms).
/// </summary>
public sealed class KeyboardHook : IDisposable
{
    private IntPtr _hookId = IntPtr.Zero;
    private readonly NativeMethods.LowLevelKeyboardProc _proc;
    private bool _disposed;

    /// <summary>Fired when a key is pressed down.</summary>
    public event EventHandler<KeyEventArgs>? KeyDown;

    /// <summary>Fired when a key is released.</summary>
    public event EventHandler<KeyEventArgs>? KeyUp;

    public KeyboardHook()
    {
        // Must hold a reference to prevent GC of the delegate
        _proc = HookCallback;
    }

    /// <summary>
    /// Installs the global keyboard hook.
    /// Must be called on a thread with a message pump.
    /// </summary>
    public void Install()
    {
        if (_hookId != IntPtr.Zero)
            return; // Already installed

        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule
            ?? throw new InvalidOperationException("Cannot get main module for hook registration.");
        if (module.BaseAddress == IntPtr.Zero)
            throw new InvalidOperationException("Cannot get module handle for hook registration.");

        _hookId = NativeMethods.SetWindowsHookEx(
            NativeConstants.WH_KEYBOARD_LL,
            _proc,
            NativeMethods.GetModuleHandle(module.ModuleName),
            0);

        if (_hookId == IntPtr.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"Failed to install keyboard hook. Error: {error}. " +
                "Try running as Administrator.");
        }
    }

    /// <summary>Uninstalls the keyboard hook.</summary>
    public void Uninstall()
    {
        if (_hookId == IntPtr.Zero)
            return;

        NativeMethods.UnhookWindowsHookEx(_hookId);
        _hookId = IntPtr.Zero;
    }

    /// <summary>
    /// The hook callback — called by Windows for every keyboard event system-wide.
    /// Must be FAST. Returns non-zero to suppress the event.
    /// </summary>
    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            // nCode < 0: must pass to CallNextHookEx without processing
            if (nCode < 0)
                return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);

            // Parse the KBDLLHOOKSTRUCT from lParam
            var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

            // Ignore injected events (simulated by SendInput) to prevent infinite loops
            if ((hookStruct.flags & NativeConstants.LLKHF_INJECTED) != 0)
                return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);

            var vkCode = hookStruct.vkCode;
            bool isKeyDown;

            switch ((int)wParam)
            {
                case NativeConstants.WM_KEYDOWN:
                case NativeConstants.WM_SYSKEYDOWN:
                    isKeyDown = true;
                    break;
                case NativeConstants.WM_KEYUP:
                case NativeConstants.WM_SYSKEYUP:
                    isKeyDown = false;
                    break;
                default:
                    return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
            }

            var args = new KeyEventArgs(vkCode, isKeyDown, hookStruct.flags);

            if (isKeyDown)
                KeyDown?.Invoke(this, args);
            else
                KeyUp?.Invoke(this, args);

            // If the event handler set Suppress = true, signal Windows to drop the event
            if (args.Suppress)
                return (IntPtr)1; // Non-zero = suppress
        }
        catch (Exception ex)
        {
            // Hook callbacks must never throw — log and continue
            Debug.WriteLine($"[KeyboardHook] Error in callback: {ex.Message}");
        }

        return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Uninstall();
    }
}

/// <summary>Event args for keyboard hook events.</summary>
public class KeyEventArgs : EventArgs
{
    /// <summary>Virtual key code of the key.</summary>
    public int VkCode { get; }

    /// <summary>True if the key is being pressed down.</summary>
    public bool IsKeyDown { get; }

    /// <summary>Raw flags from KBDLLHOOKSTRUCT.</summary>
    public int Flags { get; }

    /// <summary>
    /// Set to true to suppress this key event (prevent it from reaching the target application).
    /// </summary>
    public bool Suppress { get; set; }

    public KeyEventArgs(int vkCode, bool isKeyDown, int flags)
    {
        VkCode = vkCode;
        IsKeyDown = isKeyDown;
        Flags = flags;
    }
}