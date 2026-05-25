using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace LeXtudio.DevFlow.Agent.Core;

[SupportedOSPlatform("macos")]
public static class MacOSNativeInput
{
    private const string ApplicationServices = "/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices";
    private const uint CGHIDEventTap = 0;
    private const ulong CGEventFlagMaskCommand = 0x00100000;
    private const ulong CGEventFlagMaskShift = 0x00020000;

    private const ushort KVK_Return = 0x24;
    private const ushort KVK_Delete = 0x33; // Backspace on macOS
    private const ushort KVK_ANSI_A = 0x00;

    // CGEventPost requires the process to have Accessibility permission (TCC).
    // GitHub-hosted macOS runners don't grant it, so the call silently no-ops:
    // we'd then falsely report "native" success without delivering events.
    // Probe AXIsProcessTrusted at startup and refuse to report availability
    // when untrusted, letting the agent fall through to the semantic /
    // property-mutation paths that actually work.
    private static readonly bool _isTrusted = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && TryProbeTrusted();

    public static bool IsAvailable => _isTrusted;

    private static bool TryProbeTrusted()
    {
        try
        {
            return AXIsProcessTrusted();
        }
        catch
        {
            return false;
        }
    }

    public static bool SendUnicodeText(string text)
    {
        if (!IsAvailable || string.IsNullOrEmpty(text))
            return IsAvailable; // empty text is a no-op success when available

        foreach (var ch in text)
        {
            if (!SendUnicodeChar(ch))
                return false;
        }

        return true;
    }

    public static bool SendReturn() => SendVirtualKey(KVK_Return);

    public static bool SendBackspace() => SendVirtualKey(KVK_Delete);

    public static bool SendSelectAll()
    {
        if (!IsAvailable)
            return false;

        // Cmd+A using the 'A' keycode with command flag.
        var down = CGEventCreateKeyboardEvent(IntPtr.Zero, KVK_ANSI_A, true);
        var up = CGEventCreateKeyboardEvent(IntPtr.Zero, KVK_ANSI_A, false);
        if (down == IntPtr.Zero || up == IntPtr.Zero)
        {
            ReleaseIfNotZero(down);
            ReleaseIfNotZero(up);
            return false;
        }

        try
        {
            CGEventSetFlags(down, CGEventFlagMaskCommand);
            CGEventSetFlags(up, CGEventFlagMaskCommand);
            CGEventPost(CGHIDEventTap, down);
            CGEventPost(CGHIDEventTap, up);
            return true;
        }
        finally
        {
            CFRelease(down);
            CFRelease(up);
        }
    }

    private static bool SendVirtualKey(ushort keyCode)
    {
        if (!IsAvailable)
            return false;

        var down = CGEventCreateKeyboardEvent(IntPtr.Zero, keyCode, true);
        var up = CGEventCreateKeyboardEvent(IntPtr.Zero, keyCode, false);
        if (down == IntPtr.Zero || up == IntPtr.Zero)
        {
            ReleaseIfNotZero(down);
            ReleaseIfNotZero(up);
            return false;
        }

        try
        {
            CGEventPost(CGHIDEventTap, down);
            CGEventPost(CGHIDEventTap, up);
            return true;
        }
        finally
        {
            CFRelease(down);
            CFRelease(up);
        }
    }

    private static bool SendUnicodeChar(char ch)
    {
        // Use a keyboard event with no virtual key, then override the Unicode payload.
        var down = CGEventCreateKeyboardEvent(IntPtr.Zero, 0, true);
        var up = CGEventCreateKeyboardEvent(IntPtr.Zero, 0, false);
        if (down == IntPtr.Zero || up == IntPtr.Zero)
        {
            ReleaseIfNotZero(down);
            ReleaseIfNotZero(up);
            return false;
        }

        try
        {
            var buffer = new ushort[] { ch };
            CGEventKeyboardSetUnicodeString(down, (UIntPtr)buffer.Length, buffer);
            CGEventKeyboardSetUnicodeString(up, (UIntPtr)buffer.Length, buffer);
            CGEventPost(CGHIDEventTap, down);
            CGEventPost(CGHIDEventTap, up);
            return true;
        }
        finally
        {
            CFRelease(down);
            CFRelease(up);
        }
    }

    private static void ReleaseIfNotZero(IntPtr handle)
    {
        if (handle != IntPtr.Zero)
            CFRelease(handle);
    }

    [DllImport(ApplicationServices)]
    private static extern IntPtr CGEventCreateKeyboardEvent(IntPtr source, ushort virtualKey, [MarshalAs(UnmanagedType.I1)] bool keyDown);

    [DllImport(ApplicationServices)]
    private static extern void CGEventKeyboardSetUnicodeString(IntPtr @event, UIntPtr stringLength, [In] ushort[] unicodeString);

    [DllImport(ApplicationServices)]
    private static extern void CGEventPost(uint tap, IntPtr @event);

    [DllImport(ApplicationServices)]
    private static extern void CGEventSetFlags(IntPtr @event, ulong flags);

    [DllImport(ApplicationServices)]
    private static extern void CFRelease(IntPtr cf);

    [DllImport(ApplicationServices)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool AXIsProcessTrusted();
}
