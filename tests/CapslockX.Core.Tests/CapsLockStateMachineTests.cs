using CapslockX.Core.Native;
using CapslockX.Core.StateMachine;

namespace CapslockX.Core.Tests;

public class CapsLockStateMachineTests
{
    private const long Threshold = 300;

    [Fact]
    public void InitialState_IsIdle()
    {
        var sm = new CapsLockStateMachine(Threshold);
        Assert.Equal(CapsLockStateMachine.State.Idle, sm.CurrentState);
    }

    [Fact]
    public void CapsLockPress_TransitionsToPressPending()
    {
        var sm = new CapsLockStateMachine(Threshold);
        var result = sm.ProcessKeyEvent(NativeConstants.VK_CAPITAL, isKeyDown: true);
        Assert.Equal(CapsLockStateMachine.State.PressPending, sm.CurrentState);
        Assert.Equal(HookResult.Suppress, result);
    }

    [Fact]
    public void ShortPress_TriggersToggle()
    {
        var sm = new CapsLockStateMachine(Threshold);
        bool toggleRequested = false;
        sm.CapsLockToggleRequested += (_, _) => toggleRequested = true;

        sm.ProcessKeyEvent(NativeConstants.VK_CAPITAL, isKeyDown: true);
        Thread.Sleep(50);
        var result = sm.ProcessKeyEvent(NativeConstants.VK_CAPITAL, isKeyDown: false);

        Assert.Equal(CapsLockStateMachine.State.Idle, sm.CurrentState);
        Assert.Equal(HookResult.Suppress, result);
        Assert.True(toggleRequested, "Short press should trigger CapsLock toggle via SendInput");
    }

    [Fact]
    public void LongPressWithoutCombo_NoToggle()
    {
        var sm = new CapsLockStateMachine(Threshold);
        bool toggleRequested = false;
        sm.CapsLockToggleRequested += (_, _) => toggleRequested = true;

        sm.ProcessKeyEvent(NativeConstants.VK_CAPITAL, isKeyDown: true);
        Thread.Sleep((int)(Threshold + 50));
        var result = sm.ProcessKeyEvent(NativeConstants.VK_CAPITAL, isKeyDown: false);

        Assert.Equal(CapsLockStateMachine.State.Idle, sm.CurrentState);
        Assert.Equal(HookResult.Suppress, result);
        Assert.False(toggleRequested, "Long press should NOT toggle CapsLock");
    }

    [Fact]
    public void LongPressWithCombo_EntersModifierMode()
    {
        var sm = new CapsLockStateMachine(Threshold);
        bool comboTriggered = false;
        sm.ComboKeyPressed += (_, e) => { comboTriggered = true; e.Handled = true; };

        sm.ProcessKeyEvent(NativeConstants.VK_CAPITAL, isKeyDown: true);
        Thread.Sleep((int)(Threshold + 50));
        var result = sm.ProcessKeyEvent(NativeConstants.VK_E, isKeyDown: true);

        Assert.Equal(CapsLockStateMachine.State.ModifierActive, sm.CurrentState);
        Assert.Equal(HookResult.Suppress, result);
        Assert.True(comboTriggered);
    }

    [Fact]
    public void NonCapsLockKey_WhileIdle_PassesThrough()
    {
        var sm = new CapsLockStateMachine(Threshold);
        var result = sm.ProcessKeyEvent(NativeConstants.VK_A, isKeyDown: true);
        Assert.Equal(HookResult.PassThrough, result);
    }

    [Fact]
    public void KeyDuringPressPending_Immediately_EntersModifierMode()
    {
        var sm = new CapsLockStateMachine(Threshold);
        sm.ProcessKeyEvent(NativeConstants.VK_CAPITAL, isKeyDown: true);
        Thread.Sleep(50);
        var result = sm.ProcessKeyEvent(NativeConstants.VK_E, isKeyDown: true);

        Assert.Equal(HookResult.Suppress, result);
        Assert.Equal(CapsLockStateMachine.State.ModifierActive, sm.CurrentState);
    }

    [Fact]
    public void ShortPressWithInterveningKey_NoToggle()
    {
        var sm = new CapsLockStateMachine(Threshold);
        bool toggleRequested = false;
        sm.CapsLockToggleRequested += (_, _) => toggleRequested = true;

        sm.ProcessKeyEvent(NativeConstants.VK_CAPITAL, isKeyDown: true);
        Thread.Sleep(50);
        sm.ProcessKeyEvent(NativeConstants.VK_E, isKeyDown: true);
        Thread.Sleep(50);
        sm.ProcessKeyEvent(NativeConstants.VK_CAPITAL, isKeyDown: false);

        Assert.False(toggleRequested,
            "Combo key pressed: should not toggle CapsLock");
    }

    [Fact]
    public void ModifierActive_KeyUp_IsSuppressed()
    {
        var sm = new CapsLockStateMachine(Threshold);
        sm.ProcessKeyEvent(NativeConstants.VK_CAPITAL, isKeyDown: true);
        Thread.Sleep((int)(Threshold + 50));
        sm.ProcessKeyEvent(NativeConstants.VK_E, isKeyDown: true);
        var result = sm.ProcessKeyEvent(NativeConstants.VK_E, isKeyDown: false);
        Assert.Equal(HookResult.Suppress, result);
    }

    [Fact]
    public void ModifierActive_ReleaseCapsLock_ReturnsToIdle()
    {
        var sm = new CapsLockStateMachine(Threshold);
        sm.ProcessKeyEvent(NativeConstants.VK_CAPITAL, isKeyDown: true);
        Thread.Sleep((int)(Threshold + 50));
        var result = sm.ProcessKeyEvent(NativeConstants.VK_CAPITAL, isKeyDown: false);

        Assert.Equal(CapsLockStateMachine.State.Idle, sm.CurrentState);
        Assert.Equal(HookResult.Suppress, result);
    }
}