using System.Diagnostics;
using CapslockX.Core.Native;

namespace CapslockX.Core.StateMachine;

/// <summary>
/// Tracks the CapsLock key state and distinguishes between:
/// - Short press (tap only, no other keys, < threshold) → CapsLock toggles (normal)
/// - Long press (hold >= threshold, release, no other keys) → nothing happens
/// - Combo press (CapsLock + another key) → execute binding, no CapsLock toggle
///
/// Strategy:
///   - Suppress CapsLock DOWN (prevents toggle on key-down for some keyboards)
///   - Let CapsLock UP pass through (kernel toggles on key-up)
///   - For long press / combo: undo the kernel's toggle via SendInput
///     (SendInput events have LLKHF_INJECTED flag, our hook ignores them = no re-entrancy)
/// </summary>
public class CapsLockStateMachine
{
    /// <summary>Delay (ms) before a CapsLock-alone hold is considered "long press" (no toggle).</summary>
    private readonly long _longPressThresholdMs;

    private readonly Stopwatch _holdTimer = new();
    private State _currentState = State.Idle;
    private bool _comboKeyPressed;

    /// <summary>Raised when a combo action should be executed (while in modifier mode).</summary>
    public event EventHandler<ComboKeyEventArgs>? ComboKeyPressed;

    /// <summary>Raised when CapsLock should be toggled (short press detected).</summary>
    public event EventHandler? CapsLockToggleRequested;

    public CapsLockStateMachine(long longPressThresholdMs = 300)
    {
        _longPressThresholdMs = longPressThresholdMs;
    }

    /// <summary>Current state of the state machine.</summary>
    public State CurrentState => _currentState;

    /// <summary>
    /// Process a key event from the keyboard hook.
    /// Returns a result indicating whether to suppress the event.
    /// </summary>
    public HookResult ProcessKeyEvent(int vkCode, bool isKeyDown)
    {
        // Is this a CapsLock event?
        if (vkCode == NativeConstants.VK_CAPITAL)
            return ProcessCapsLock(isKeyDown);

        // Non-CapsLock key — delegate to current state
        return _currentState switch
        {
            State.Idle => HookResult.PassThrough,
            State.PressPending => ProcessKeyInPressPending(vkCode, isKeyDown),
            State.ModifierActive => ProcessKeyInModifier(vkCode, isKeyDown),
            _ => HookResult.PassThrough
        };
    }

    private HookResult ProcessCapsLock(bool isKeyDown)
    {
        if (isKeyDown)
        {
            _holdTimer.Restart();
            _comboKeyPressed = false;
            _currentState = State.PressPending;

            Debug.WriteLine("[StateMachine] CapsLock DOWN — suppressed");
            return HookResult.Suppress;
        }
        else
        {
            _holdTimer.Stop();
            var elapsed = _holdTimer.ElapsedMilliseconds;

            Debug.WriteLine($"[StateMachine] CapsLock UP | elapsed={elapsed}ms | comboKey={_comboKeyPressed} | state={_currentState}");

            if (_currentState == State.ModifierActive || elapsed >= _longPressThresholdMs || _comboKeyPressed)
            {
                // Long press or combo → CapsLock should NOT toggle
                Debug.WriteLine("[StateMachine] → Long press / combo: no toggle");
            }
            else
            {
                // Short press → toggle CapsLock via SendInput
                Debug.WriteLine("[StateMachine] → Short press: toggling CapsLock");
                CapsLockToggleRequested?.Invoke(this, EventArgs.Empty);
            }

            _currentState = State.Idle;
            return HookResult.Suppress;
        }
    }

    /// <summary>
    /// Another key pressed while CapsLock is held (PressPending state).
    /// Modifier keys (Shift/Ctrl/Alt/Win) are passed through so the system
    /// registers them, enabling GetAsyncKeyState detection for combos like CapsLock+Shift+E.
    /// Non-modifier keys immediately enter modifier mode and are suppressed.
    /// </summary>
    private HookResult ProcessKeyInPressPending(int vkCode, bool isKeyDown)
    {
        if (!isKeyDown)
            return HookResult.Suppress;

        // Let modifier keys pass through so the OS registers them.
        // Otherwise GetAsyncKeyState won't detect e.g. Shift for CapsLock+Shift+letter combos.
        if (IsModifierKey(vkCode))
        {
            _comboKeyPressed = true;
            Debug.WriteLine($"[StateMachine] Modifier key passed through: VK={vkCode:X2}");
            return HookResult.PassThrough;
        }

        // Non-modifier key pressed while CapsLock is held = combo attempt
        _comboKeyPressed = true;
        Debug.WriteLine($"[StateMachine] → Combo key detected: VK={vkCode:X2}, entering ModifierActive");
        _currentState = State.ModifierActive;
        return ProcessKeyInModifier(vkCode, isKeyDown);
    }

    private HookResult ProcessKeyInModifier(int vkCode, bool isKeyDown)
    {
        // Modifier keys pass through (needed for Shift/Ctrl/Alt state detection)
        if (IsModifierKey(vkCode))
            return HookResult.PassThrough;

        if (!isKeyDown)
            return HookResult.Suppress;

        // Check if there's a binding for this combo
        var args = new ComboKeyEventArgs(vkCode, isModifierShiftPressed());
        ComboKeyPressed?.Invoke(this, args);

        if (args.Handled)
            Debug.WriteLine($"[StateMachine] Combo executed: VK={vkCode:X2} shift={args.ShiftHeld}");

        return HookResult.Suppress;
    }

    /// <summary>Check if a virtual key code is a modifier key (Shift, Ctrl, Alt, Win).</summary>
    private static bool IsModifierKey(int vkCode) => vkCode switch
    {
        NativeConstants.VK_SHIFT or NativeConstants.VK_LSHIFT or NativeConstants.VK_RSHIFT
            or NativeConstants.VK_CONTROL or NativeConstants.VK_LCONTROL or NativeConstants.VK_RCONTROL
            or NativeConstants.VK_MENU or NativeConstants.VK_LMENU or NativeConstants.VK_RMENU
            or NativeConstants.VK_LWIN or NativeConstants.VK_RWIN
            => true,
        _ => false
    };

    /// <summary>Check if Shift is currently held down (for Shift+letter combos).</summary>
    private static bool isModifierShiftPressed()
    {
        return (NativeMethods.GetAsyncKeyState(NativeConstants.VK_LSHIFT) & NativeConstants.KEY_DOWN_MASK) != 0
            || (NativeMethods.GetAsyncKeyState(NativeConstants.VK_RSHIFT) & NativeConstants.KEY_DOWN_MASK) != 0;
    }

    public enum State
    {
        /// <summary>Waiting for CapsLock press.</summary>
        Idle,
        /// <summary>CapsLock is held, no other keys pressed yet.</summary>
        PressPending,
        /// <summary>CapsLock held + another key pressed = modifier mode active.</summary>
        ModifierActive
    }
}

/// <summary>Represents the result of processing a key event.</summary>
public enum HookResult
{
    /// <summary>Let the key event pass through to the system.</summary>
    PassThrough,
    /// <summary>Suppress (block) the key event.</summary>
    Suppress
}

/// <summary>Event args for a combo key press while CapsLock is in modifier mode.</summary>
public class ComboKeyEventArgs : EventArgs
{
    /// <summary>Virtual key code of the pressed key.</summary>
    public int VkCode { get; }

    /// <summary>Whether Shift is currently held down.</summary>
    public bool ShiftHeld { get; }

    /// <summary>
    /// Set to true if this combo key was handled by a binding.
    /// If false after all handlers run, the key will be suppressed without action.
    /// </summary>
    public bool Handled { get; set; }

    public ComboKeyEventArgs(int vkCode, bool shiftHeld)
    {
        VkCode = vkCode;
        ShiftHeld = shiftHeld;
    }
}