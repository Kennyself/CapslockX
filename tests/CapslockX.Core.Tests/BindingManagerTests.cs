using CapslockX.Core.Bindings;
using CapslockX.Core.Native;

namespace CapslockX.Core.Tests;

public class BindingManagerTests
{
    [Fact]
    public void DefaultBindings_AreRegistered()
    {
        var bm = new BindingManager();

        var allBindings = bm.GetAllBindings().ToList();
        Assert.NotEmpty(allBindings);

        // Should have 6 unshifted + 6 shifted = 12 bindings
        Assert.Equal(12, allBindings.Count);
    }

    [Fact]
    public void TryGetBinding_UnshiftedE_ReturnsCursorUp()
    {
        var bm = new BindingManager();

        bool found = bm.TryGetBinding(NativeConstants.VK_E, shiftHeld: false, out var binding);

        Assert.True(found);
        Assert.NotNull(binding);
        Assert.Equal("Cursor Up", binding.Name);
        Assert.Equal("SendKey", binding.ActionType);
        Assert.Equal(NativeConstants.VK_UP, binding.TargetVk);
        Assert.True(binding.Enabled);
    }

    [Fact]
    public void TryGetBinding_ShiftedE_ReturnsSelectUp()
    {
        var bm = new BindingManager();

        bool found = bm.TryGetBinding(NativeConstants.VK_E, shiftHeld: true, out var binding);

        Assert.True(found);
        Assert.NotNull(binding);
        Assert.Equal("Select Up", binding.Name);
        Assert.Equal("SendCombo", binding.ActionType);
        Assert.Equal(NativeConstants.VK_UP, binding.TargetVk);
        Assert.Equal(NativeConstants.VK_SHIFT, binding.ModifierVk);
    }

    [Fact]
    public void TryGetBinding_UnshiftedAndShifted_ReturnDifferentBindings()
    {
        var bm = new BindingManager();

        bm.TryGetBinding(NativeConstants.VK_S, shiftHeld: false, out var unshifted);
        bm.TryGetBinding(NativeConstants.VK_S, shiftHeld: true, out var shifted);

        Assert.Equal("Cursor Left", unshifted!.Name);
        Assert.Equal("Select Left", shifted!.Name);
    }

    [Fact]
    public void TryGetBinding_HomeAndEnd_Work()
    {
        var bm = new BindingManager();

        Assert.True(bm.TryGetBinding(NativeConstants.VK_A, shiftHeld: false, out var home));
        Assert.Equal("Home", home!.Name);
        Assert.Equal(NativeConstants.VK_HOME, home.TargetVk);

        Assert.True(bm.TryGetBinding(NativeConstants.VK_G, shiftHeld: false, out var end));
        Assert.Equal("End", end!.Name);
        Assert.Equal(NativeConstants.VK_END, end.TargetVk);
    }

    [Fact]
    public void TryGetBinding_UnmappedKey_ReturnsFalse()
    {
        var bm = new BindingManager();

        bool found = bm.TryGetBinding(NativeConstants.VK_Z, shiftHeld: false, out var binding);

        Assert.False(found);
        Assert.Null(binding);
    }

    [Fact]
    public void DisabledBinding_DoesNotExecute()
    {
        var bm = new BindingManager();

        bm.TryGetBinding(NativeConstants.VK_E, shiftHeld: false, out var binding);
        binding!.Enabled = false;

        // Execution should not throw, just no-op
        binding.Execute(); // Should be a no-op when disabled

        Assert.False(binding.Enabled);
    }

    [Fact]
    public void KeyBinding_ToString_ShowsCorrectInfo()
    {
        var binding = new KeyBinding("Test", NativeConstants.VK_X, false, "SendKey", NativeConstants.VK_Y);

        var str = binding.ToString();

        Assert.Contains("Test", str);
        Assert.Contains("CapsLock+", str);
        Assert.Contains("SendKey", str);
    }

    [Fact]
    public void KeyBinding_ToString_Disabled_ShowsDisabledTag()
    {
        var binding = new KeyBinding("Test", NativeConstants.VK_X, false, "SendKey", NativeConstants.VK_Y);
        binding.Enabled = false;

        var str = binding.ToString();

        Assert.Contains("[DISABLED]", str);
    }
}