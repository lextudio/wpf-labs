using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using LeXtudio.DevFlow.Agent.Core;
using Microsoft.Maui.DevFlow.Agent.Core;

namespace LeXtudio.DevFlow.Agent.WinForms;

public sealed class WinFormsAgentService(AgentOptions? options = null) : DevFlowAgentServiceBase(options)
{
    private readonly WinFormsVisualTreeWalker _walker = new();

    protected override string AgentId => "LeXtudio.DevFlow.Agent";
    protected override string AgentName => "LeXtudio.DevFlow.Agent";
    protected override string FrameworkName => "winforms";

    protected override object GetCapabilities() => new
    {
        screenshots = true,
        elementScreenshots = true,
        selectorScreenshots = true,
        tap = true,
        scroll = true,
        structuredErrors = true,
        appTheme = false,
        webview = false,
        webviewCdp = false,
        multiWindow = true
    };

    protected override Task<string?> GetApplicationNameAsync() => Task.FromResult(Application.ProductName);
    protected override Task<List<ElementInfo>> BuildTreeAsync() => Task.FromResult(_walker.WalkTree());
    protected override Task<ElementInfo?> FindElementAsync(string id) => Task.FromResult(_walker.FindElementById(id));

    protected override Task<List<ElementInfo>> QueryElementsAsync(string? type = null, string? automationId = null, string? text = null)
    {
        var all = new List<ElementInfo>();
        foreach (var root in _walker.WalkTree()) Flatten(root, all);
        return Task.FromResult(all.Where(e =>
            (string.IsNullOrWhiteSpace(type) || string.Equals(e.Type, type, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(automationId) || string.Equals(e.AutomationId, automationId, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(text) || (e.Text?.Contains(text, StringComparison.OrdinalIgnoreCase) == true))
        ).ToList());
    }

    protected override Task<byte[]?> CaptureScreenshotAsync(string? elementId = null, string? selector = null)
    {
        Control? control = null;
        if (!string.IsNullOrWhiteSpace(elementId))
            control = _walker.ResolveControlById(elementId);
        else if (!string.IsNullOrWhiteSpace(selector))
            control = ResolveBySelector(selector);

        if (control != null)
            return Task.FromResult(CaptureControl(control));

        var form = Application.OpenForms.Cast<Form>().FirstOrDefault(f => f.Visible);
        return Task.FromResult(form == null ? null : CaptureControl(form));
    }

    protected override Task<bool> TryTapAsync(string elementId)
    {
        return InvokeOnUiThread(() =>
        {
            var control = _walker.ResolveControlById(elementId);
            if (control is Button button)
            {
                button.PerformClick();
                return true;
            }

            if (control == null)
                return false;

            _ = control.Focus();
            return true;
        });
    }

    protected override Task<bool> TryScrollAsync(string elementId, double deltaX, double deltaY)
    {
        return InvokeOnUiThread(() =>
        {
            var control = _walker.ResolveControlById(elementId);
            if (control is ScrollableControl scrollable)
            {
                var current = scrollable.AutoScrollPosition;
                var x = Math.Max(0, -current.X + (int)deltaX);
                var y = Math.Max(0, -current.Y + (int)deltaY);
                scrollable.AutoScrollPosition = new Point(x, y);
                return true;
            }

            return false;
        });
    }

    protected override Task<bool> TryFillAsync(string elementId, string text)
    {
        return InvokeOnUiThread(() =>
        {
            var control = _walker.ResolveControlById(elementId);
            switch (control)
            {
                case TextBoxBase tb:
                    tb.Text = text;
                    return true;
                case ComboBox cb:
                    cb.Text = text;
                    return true;
                default:
                    return false;
            }
        });
    }

    protected override Task<bool> TryClearAsync(string elementId) => TryFillAsync(elementId, string.Empty);
    protected override Task<bool> TryFocusAsync(string elementId)
    {
        return InvokeOnUiThread(() =>
        {
            var control = _walker.ResolveControlById(elementId);
            if (control == null)
                return false;

            _ = control.Focus();
            return true;
        });
    }

    protected override Task<object?> TryKeyAsync(string? elementId, string? key, string? text)
    {
        if (string.IsNullOrWhiteSpace(elementId))
            return Task.FromResult<object?>(new { success = false, reason = "elementId required" });

        return InvokeOnUiThread<object?>(() =>
        {
            var control = _walker.ResolveControlById(elementId);
            if (control is not TextBoxBase tb)
                return new { success = false, reason = "Unsupported control" };

            var normalized = (key ?? text ?? string.Empty).Trim().ToLowerInvariant();
            if (normalized is "backspace" or "delete")
            {
                if (!string.IsNullOrEmpty(tb.Text))
                    tb.Text = tb.Text[..^1];
                return new { success = true, elementId };
            }

            var insert = text ?? (string.IsNullOrWhiteSpace(key) ? null : key);
            if (!string.IsNullOrEmpty(insert) && insert!.Length == 1)
            {
                tb.Text += insert;
                return new { success = true, elementId };
            }

            return new { success = false, reason = "Unsupported key" };
        });
    }

    protected override Task<bool> TryBackAsync() => Task.FromResult(false);
    protected override Task<object?> GetThemeAsync() => Task.FromResult<object?>(new { theme = "system", supportedThemes = new[] { "system" } });
    protected override Task<object?> SetThemeAsync(string theme) => Task.FromResult<object?>(null);

    private Control? ResolveBySelector(string selector)
    {
        var s = selector.Trim();
        if (s.StartsWith("#", StringComparison.Ordinal))
            return _walker.ResolveControlById(s[1..]);
        return null;
    }

    private static byte[]? CaptureControl(Control control)
    {
        if (control.Width <= 0 || control.Height <= 0)
            return null;

        using var bmp = new Bitmap(control.Width, control.Height);
        control.DrawToBitmap(bmp, new Rectangle(Point.Empty, bmp.Size));
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    private static Task<T> InvokeOnUiThread<T>(Func<T> action)
    {
        var invoker = Application.OpenForms.Cast<Form>().FirstOrDefault();
        if (invoker == null || invoker.IsDisposed || !invoker.IsHandleCreated)
            return Task.FromResult(action());

        if (!invoker.InvokeRequired)
            return Task.FromResult(action());

        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        invoker.BeginInvoke(() =>
        {
            try
            {
                completion.SetResult(action());
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }
        });

        return completion.Task;
    }

    private static void Flatten(ElementInfo node, List<ElementInfo> acc)
    {
        acc.Add(node);
        if (node.Children == null) return;
        foreach (var c in node.Children) Flatten(c, acc);
    }
}
