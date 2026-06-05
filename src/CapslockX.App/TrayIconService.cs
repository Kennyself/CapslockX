using System.Drawing;
using System.Windows;

namespace CapslockX.App;

/// <summary>
/// Manages the system tray icon with context menu (Exit, etc.).
/// Uses WinForms NotifyIcon since WPF has no built-in tray support.
/// </summary>
public sealed class TrayIconService : IDisposable
{
    private NotifyIcon? _notifyIcon;
    private bool _disposed;

    /// <summary>Raised when the user clicks "Exit" in the tray menu.</summary>
    public event EventHandler? ExitRequested;

    /// <summary>Initialize and show the tray icon.</summary>
    public void Initialize()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = CreateTrayIcon(),
            Text = "CapslockX",
            Visible = true
        };

        // Left-click: toggle enable/disable (future)
        _notifyIcon.Click += (s, e) =>
        {
            // Future: toggle hook enable/disable
        };

        // Right-click context menu
        var contextMenu = new ContextMenuStrip();

        var aboutItem = new ToolStripMenuItem("About CapslockX");
        aboutItem.Click += (s, e) =>
        {
            System.Windows.MessageBox.Show(
                "CapslockX v0.1.0\n\n" +
                "CapsLock as a powerful modifier key.\n" +
                "Use CapsLock+E/D/S/F to move cursor.\n" +
                "Use CapsLock+A/G for Home/End.\n\n" +
                "Short press CapsLock = normal toggle.",
                "About CapslockX",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        };

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);

        contextMenu.Items.Add(aboutItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _notifyIcon!.Visible = false;
        _notifyIcon!.Dispose();
    }

    /// <summary>Generate a tray icon: rounded blue background with "CX" text.</summary>
    private static Icon CreateTrayIcon()
    {
        // Draw at 64x64 for crisp scaling on high-DPI screens
        const int size = 64;
        var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        // Circular background
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddEllipse(0, 0, size - 1, size - 1);
        using var bgBrush = new SolidBrush(Color.FromArgb(46, 89, 163)); // #2e59a3
        g.FillPath(bgBrush, path);

        // "CX" text centered
        using var font = new Font(new FontFamily("Segoe UI Black"), 24, System.Drawing.FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.White);
        var text = "CX";
        var textSize = g.MeasureString(text, font);
        var x = (size - textSize.Width) / 2;
        var y = (size - textSize.Height) / 2;
        g.DrawString(text, font, textBrush, x, y);

        return Icon.FromHandle(bitmap.GetHicon());
    }
}