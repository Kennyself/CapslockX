using System.Runtime.InteropServices;
using CapslockX.Core.Native;

namespace CapslockX.Core.Bindings;

/// <summary>
/// Encapsulates Windows SendInput API for simulating keyboard input.
/// Used to: toggle CapsLock, simulate direction keys, send key combos (Ctrl+C, etc.).
/// </summary>
public static class InputSimulator
{
    /// <summary>Size of the INPUT structure for marshalling.</summary>
    private static readonly int InputSize = Marshal.SizeOf<INPUT>();

    /// <summary>
    /// Simulates a CapsLock toggle: sends CapsLock DOWN then UP.
    /// Use when the hook intercepted a short CapsLock press and needs to re-trigger it.
    /// </summary>
    public static void ToggleCapsLock()
    {
        SendKeyDown(NativeConstants.VK_CAPITAL);
        SendKeyUp(NativeConstants.VK_CAPITAL);
    }

    /// <summary>
    /// Simulates a CapsLock toggle using keybd_event (lower-level than SendInput).
    /// Use this from within hook callbacks — avoids the issues with SendInput
    /// injecting events that get queued behind the current hook processing.
    /// </summary>
    public static void ToggleCapsLockViaKeybdEvent()
    {
        const byte VK_CAPITAL = 0x14;
        NativeMethods.keybd_event(VK_CAPITAL, 0, 0, UIntPtr.Zero);                                // DOWN
        NativeMethods.keybd_event(VK_CAPITAL, 0, NativeConstants.KEYEVENTF_KEYUP, UIntPtr.Zero); // UP
    }

    /// <summary>
    /// Simulates pressing a key down.</summary>
    public static void SendKeyDown(int vkCode)
    {
        SendKey(vkCode, keyUp: false);
    }

    /// <summary>Simulates releasing a key.</summary>
    public static void SendKeyUp(int vkCode)
    {
        SendKey(vkCode, keyUp: true);
    }

    /// <summary>Simulates a full key press (down + up).</summary>
    public static void SendKeyPress(int vkCode)
    {
        SendKeyDown(vkCode);
        SendKeyUp(vkCode);
    }

    /// <summary>
    /// Simulates a key combo: modifier held down, key pressed, key released, modifier released.
    /// Example: SendCombo(VK_CONTROL, 'C') → Ctrl+C
    /// </summary>
    public static void SendCombo(int modifierVk, int keyVk)
    {
        var inputs = new INPUT[4];

        // 1. Modifier down
        inputs[0] = CreateKeyboardInput(modifierVk, keyUp: false);

        // 2. Key down
        inputs[1] = CreateKeyboardInput(keyVk, keyUp: false);

        // 3. Key up
        inputs[2] = CreateKeyboardInput(keyVk, keyUp: true);

        // 4. Modifier up
        inputs[3] = CreateKeyboardInput(modifierVk, keyUp: true);

        SendInput(inputs);
    }

    /// <summary>
    /// Simulates a key held down (modifier or otherwise) — caller must release it.
    /// Used for holding shift while the user manually selects text.
    /// </summary>
    public static void SendKeyDownOnly(int vkCode)
    {
        var inputs = new INPUT[1];
        inputs[0] = CreateKeyboardInput(vkCode, keyUp: false);
        SendInput(inputs);
    }

    /// <summary>
    /// Simulates releasing a key previously held down by SendKeyDownOnly.
    /// </summary>
    public static void SendKeyUpOnly(int vkCode)
    {
        var inputs = new INPUT[1];
        inputs[0] = CreateKeyboardInput(vkCode, keyUp: true);
        SendInput(inputs);
    }

    // ── Private helpers ─────────────────────────────────────

    private static void SendKey(int vkCode, bool keyUp)
    {
        var inputs = new INPUT[1];
        inputs[0] = CreateKeyboardInput(vkCode, keyUp);
        SendInput(inputs);
    }

    private static INPUT CreateKeyboardInput(int vkCode, bool keyUp)
    {
        // Check if this is an extended key (needs KEYEVENTF_EXTENDEDKEY flag)
        bool isExtended = IsExtendedKey(vkCode);

        return new INPUT
        {
            type = NativeConstants.INPUT_KEYBOARD,
            u = new INPUT_UNION
            {
                ki = new KEYBDINPUT
                {
                    wVk = (ushort)vkCode,
                    wScan = 0,
                    dwFlags = (keyUp ? NativeConstants.KEYEVENTF_KEYUP : 0u)
                            | (isExtended ? NativeConstants.KEYEVENTF_EXTENDEDKEY : 0u),
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };
    }

    private static void SendInput(INPUT[] inputs)
    {
        var count = NativeMethods.SendInput(
            (uint)inputs.Length,
            inputs,
            InputSize);

        if (count != inputs.Length)
        {
            var error = Marshal.GetLastWin32Error();
            System.Diagnostics.Debug.WriteLine(
                $"[InputSimulator] SendInput failed: sent {count}/{inputs.Length}, error {error}");
        }
    }

    /// <summary>Check if a virtual key code corresponds to an extended key.</summary>
    private static bool IsExtendedKey(int vkCode)
    {
        return vkCode switch
        {
            NativeConstants.VK_INSERT  => true,
            NativeConstants.VK_DELETE  => true,
            NativeConstants.VK_HOME    => true,
            NativeConstants.VK_END     => true,
            NativeConstants.VK_PRIOR   => true,  // Page Up
            NativeConstants.VK_NEXT    => true,  // Page Down
            NativeConstants.VK_LEFT    => true,
            NativeConstants.VK_RIGHT   => true,
            NativeConstants.VK_UP      => true,
            NativeConstants.VK_DOWN    => true,
            NativeConstants.VK_NUMLOCK => true,
            NativeConstants.VK_RWIN    => true,
            NativeConstants.VK_LWIN    => true,
            NativeConstants.VK_RCONTROL => true,
            NativeConstants.VK_RMENU   => true,
            _ => false
        };
    }
}