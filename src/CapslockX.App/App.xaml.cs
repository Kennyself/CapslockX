using System.Diagnostics;
using CapslockX.Core.Bindings;
using CapslockX.Core.Hook;
using CapslockX.Core.Native;
using CapslockX.Core.StateMachine;
using WpfApp = System.Windows.Application;
using WpfStartupEventArgs = System.Windows.StartupEventArgs;
using HookKeyEventArgs = CapslockX.Core.Hook.KeyEventArgs;

namespace CapslockX.App;

/// <summary>
/// Application entry point. Starts hidden, registers the keyboard hook,
/// shows the system tray icon, and runs the WPF message loop.
/// </summary>
public partial class App : WpfApp
{
    private KeyboardHook? _hook;
    private CapsLockStateMachine? _stateMachine;
    private BindingManager? _bindingManager;
    private TrayIconService? _trayIcon;

    protected override void OnStartup(WpfStartupEventArgs e)
    {
        base.OnStartup(e);

        Debug.WriteLine("[CapslockX] Starting up...");

        try
        {
            // Initialize core components
            _bindingManager = new BindingManager();
            _stateMachine = new CapsLockStateMachine(longPressThresholdMs: 300);
            _hook = new KeyboardHook();

            // Wire up events
            _hook.KeyDown += OnKeyDown;
            _hook.KeyUp += OnKeyUp;
            _stateMachine.ComboKeyPressed += OnComboKeyPressed;
            _stateMachine.CapsLockToggleRequested += OnCapsLockToggleRequested;

            // Install the global keyboard hook
            _hook.Install();
            Debug.WriteLine("[CapslockX] Hook installed successfully.");

            // Initialize tray icon
            _trayIcon = new TrayIconService();
            _trayIcon.Initialize();
            _trayIcon.ExitRequested += OnExit;

            Debug.WriteLine("[CapslockX] Ready. Press CapsLock+ESDF to navigate.");
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to start CapslockX:\n\n{ex.Message}\n\n" +
                "Please ensure you are running as Administrator.",
                "CapslockX — Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            Shutdown(1);
        }
    }

    private void OnKeyDown(object? sender, HookKeyEventArgs e)
    {
        var result = _stateMachine!.ProcessKeyEvent(e.VkCode, isKeyDown: true);
        if (result == HookResult.Suppress)
            e.Suppress = true;
    }

    private void OnKeyUp(object? sender, HookKeyEventArgs e)
    {
        var result = _stateMachine!.ProcessKeyEvent(e.VkCode, isKeyDown: false);
        if (result == HookResult.Suppress)
            e.Suppress = true;
    }

    private void OnComboKeyPressed(object? sender, ComboKeyEventArgs e)
    {
        if (_bindingManager!.TryGetBinding(e.VkCode, e.ShiftHeld, out var binding))
        {
            Debug.WriteLine($"[CapslockX] Executing: {binding}");

            // When Shift is physically held (passed through by the state machine),
            // don't simulate Shift in SendInput — just send the target key.
            // Otherwise the simulated Shift UP would cancel the physical Shift DOWN,
            // breaking continuous text selection.
            if (binding!.ActionType == "SendCombo"
                && binding.ModifierVk == NativeConstants.VK_SHIFT
                && e.ShiftHeld)
            {
                InputSimulator.SendKeyPress(binding.TargetVk);
            }
            else
            {
                binding!.Execute();
            }

            e.Handled = true;
        }
    }

    private void OnCapsLockToggleRequested(object? sender, EventArgs e)
    {
        Debug.WriteLine("[CapslockX] Toggling CapsLock via SendInput");
        InputSimulator.ToggleCapsLock();
    }

    private void OnExit(object? sender, EventArgs e)
    {
        Debug.WriteLine("[CapslockX] Shutting down...");
        _hook?.Dispose();
        _trayIcon?.Dispose();
        Shutdown();
    }
}