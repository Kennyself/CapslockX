namespace CapslockX.Core.Bindings;

/// <summary>
/// Represents a single CapsLock+X key binding that maps to a target action.
/// The action is either a simulated keystroke (SendKey) or a combo (modifier+key).
/// </summary>
public class KeyBinding
{
    /// <summary>Human-readable name, e.g. "Cursor Up".</summary>
    public string Name { get; }

    /// <summary>Virtual key code that triggers this binding (e.g. 'E' = 0x45).</summary>
    public int TriggerKey { get; }

    /// <summary>If Shift must be held, uses this variant instead of the unshifted binding.</summary>
    public bool RequiresShift { get; }

    /// <summary>
    /// The action to perform. Supported types:
    /// - "SendKey": send a single key press
    /// - "SendCombo": send a modifier+key combo
    /// </summary>
    public string ActionType { get; }

    /// <summary>Modifier VK code for SendCombo (0 for none/SendKey).</summary>
    public int ModifierVk { get; }

    /// <summary>Target key VK code to simulate.</summary>
    public int TargetVk { get; }

    /// <summary>Whether this binding is currently enabled.</summary>
    public bool Enabled { get; set; } = true;

    public KeyBinding(
        string name,
        int triggerKey,
        bool requiresShift,
        string actionType,
        int targetVk,
        int modifierVk = 0)
    {
        Name = name;
        TriggerKey = triggerKey;
        RequiresShift = requiresShift;
        ActionType = actionType;
        TargetVk = targetVk;
        ModifierVk = modifierVk;
    }

    /// <summary>Execute this binding's action via InputSimulator.</summary>
    public void Execute()
    {
        if (!Enabled) return;

        switch (ActionType)
        {
            case "SendKey":
                InputSimulator.SendKeyPress(TargetVk);
                break;

            case "SendCombo":
                InputSimulator.SendCombo(ModifierVk, TargetVk);
                break;

            default:
                System.Diagnostics.Debug.WriteLine(
                    $"[KeyBinding] Unknown action type: {ActionType} for binding '{Name}'");
                break;
        }
    }

    public override string ToString() => $"{(Enabled ? "" : "[DISABLED] ")}" +
        $"CapsLock{(RequiresShift ? "+Shift" : "")}+{TriggerKey:X2} → {ActionType}({TargetVk:X2}) [{Name}]";
}