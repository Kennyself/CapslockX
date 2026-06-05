using CapslockX.Core.Native;

namespace CapslockX.Core.Bindings;

/// <summary>
/// Manages all CapsLock+X key bindings.
/// Phase 1: hardcoded bindings (ESDF cursor + AG home/end + Shift variants).
/// Phase 2+: loaded from JSON config.
/// </summary>
public class BindingManager
{
    /// <summary>
    /// Key: composite key of (RequiresShift << 31 | VkCode).
    /// Allows separate bindings for CapsLock+E (no shift) and CapsLock+Shift+E.
    /// </summary>
    private readonly Dictionary<int, KeyBinding> _bindings = new();

    public BindingManager()
    {
        RegisterDefaultBindings();
    }

    /// <summary>Register all Phase 1 default bindings (hardcoded).</summary>
    private void RegisterDefaultBindings()
    {
        // ── Cursor Navigation ───────────────────────────────
        Register(new KeyBinding("Cursor Up",    NativeConstants.VK_E, false, "SendKey", NativeConstants.VK_UP));
        Register(new KeyBinding("Cursor Down",  NativeConstants.VK_D, false, "SendKey", NativeConstants.VK_DOWN));
        Register(new KeyBinding("Cursor Left",  NativeConstants.VK_S, false, "SendKey", NativeConstants.VK_LEFT));
        Register(new KeyBinding("Cursor Right", NativeConstants.VK_F, false, "SendKey", NativeConstants.VK_RIGHT));
        Register(new KeyBinding("Home",         NativeConstants.VK_A, false, "SendKey", NativeConstants.VK_HOME));
        Register(new KeyBinding("End",          NativeConstants.VK_G, false, "SendKey", NativeConstants.VK_END));

        // ── Text Selection (Shift + direction) ──────────────
        Register(new KeyBinding("Select Up",    NativeConstants.VK_E, true, "SendCombo", NativeConstants.VK_UP,    NativeConstants.VK_SHIFT));
        Register(new KeyBinding("Select Down",  NativeConstants.VK_D, true, "SendCombo", NativeConstants.VK_DOWN,  NativeConstants.VK_SHIFT));
        Register(new KeyBinding("Select Left",  NativeConstants.VK_S, true, "SendCombo", NativeConstants.VK_LEFT,  NativeConstants.VK_SHIFT));
        Register(new KeyBinding("Select Right", NativeConstants.VK_F, true, "SendCombo", NativeConstants.VK_RIGHT, NativeConstants.VK_SHIFT));
        Register(new KeyBinding("Select Home",  NativeConstants.VK_A, true, "SendCombo", NativeConstants.VK_HOME,  NativeConstants.VK_SHIFT));
        Register(new KeyBinding("Select End",   NativeConstants.VK_G, true, "SendCombo", NativeConstants.VK_END,   NativeConstants.VK_SHIFT));
    }

    /// <summary>Register a key binding.</summary>
    public void Register(KeyBinding binding)
    {
        int key = MakeKey(binding.TriggerKey, binding.RequiresShift);
        if (_bindings.ContainsKey(key))
        {
            System.Diagnostics.Debug.WriteLine(
                $"[BindingManager] Warning: Duplicate binding for {binding}. Overwriting.");
        }
        _bindings[key] = binding;
    }

    /// <summary>
    /// Try to find a binding for the given vkCode and shift state.
    /// </summary>
    /// <param name="vkCode">Virtual key code of the pressed key.</param>
    /// <param name="shiftHeld">Whether Shift is currently held down.</param>
    /// <param name="binding">The matching binding, or null if not found.</param>
    /// <returns>True if a binding was found.</returns>
    public bool TryGetBinding(int vkCode, bool shiftHeld, out KeyBinding? binding)
    {
        int key = MakeKey(vkCode, shiftHeld);
        return _bindings.TryGetValue(key, out binding);
    }

    /// <summary>Get all registered bindings (for settings UI, debugging).</summary>
    public IEnumerable<KeyBinding> GetAllBindings() => _bindings.Values;

    /// <summary>Composite key: high bit = shift state, low bits = vkCode.</summary>
    private static int MakeKey(int vkCode, bool requiresShift)
        => (requiresShift ? (1 << 31) : 0) | vkCode;
}