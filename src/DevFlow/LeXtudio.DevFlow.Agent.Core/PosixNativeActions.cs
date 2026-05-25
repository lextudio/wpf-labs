using System.Runtime.InteropServices;

namespace LeXtudio.DevFlow.Agent.Core;

/// <summary>
/// Posts keyboard events at the OS level on macOS (CGEventPost) and Linux/X11
/// (XTestFakeKeyEvent). The caller must ensure the target element already holds
/// in-app focus — these helpers don't compute or change window focus.
/// </summary>
public static class PosixNativeActions
{
    public static bool IsAvailable
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return MacOSNativeInput.IsAvailable;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return LinuxNativeInput.IsAvailable;
            return false;
        }
    }

    public static bool TryTextInput(string text, bool replace)
    {
        if (!IsAvailable)
            return false;

        if (replace)
        {
            if (!SendSelectAll() || !SendBackspace())
                return false;
        }

        return string.IsNullOrEmpty(text) || SendUnicodeText(text);
    }

    public static bool TryKeyInput(string normalizedKey, string? insertText)
    {
        if (!IsAvailable)
            return false;

        if (normalizedKey is "enter" or "return")
            return SendReturn();

        if (normalizedKey is "backspace" or "delete")
            return SendBackspace();

        return !string.IsNullOrEmpty(insertText) && SendUnicodeText(insertText);
    }

    private static bool SendUnicodeText(string text)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return MacOSNativeInput.SendUnicodeText(text);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return LinuxNativeInput.SendUnicodeText(text);
        return false;
    }

    private static bool SendReturn()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return MacOSNativeInput.SendReturn();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return LinuxNativeInput.SendReturn();
        return false;
    }

    private static bool SendBackspace()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return MacOSNativeInput.SendBackspace();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return LinuxNativeInput.SendBackspace();
        return false;
    }

    private static bool SendSelectAll()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return MacOSNativeInput.SendSelectAll();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return LinuxNativeInput.SendSelectAll();
        return false;
    }
}
