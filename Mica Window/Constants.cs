#if WINDOWS10_0_17763_0_OR_GREATER
using Microsoft.UI.Windowing;
using Windows.UI.ViewManagement;
#endif

using System;

namespace MicaWindow.Environment;

public static class Constants
{
#if WINDOWS10_0_17763_0_OR_GREATER
    public static UISettings UISettings { get; } = new UISettings();
    public static bool IsNewTitleBarSupported => AppWindowTitleBar.IsCustomizationSupported();
#else
    public static bool IsNewTitleBarSupported => false;
#endif
    public static OSVersions OSVersion { get; private set; } = new Func<OSVersions>(delegate
    {
        var ToReturn = OSVersions.Other;
        var Build = System.Environment.OSVersion.Version.Build;
        var Major = System.Environment.OSVersion.Version.Major;

        if (Build > 22523 || Major > 10) ToReturn |= OSVersions.SupportsDWMBackdrop;
        if (Build > 22000 || Major > 10) ToReturn |= OSVersions.Windows11OrAbove;
        if (Major >= 10) ToReturn |= OSVersions.Windows10OrAbove;
        if (Major >= 8) ToReturn |= OSVersions.Windows8OrAbove;
        if (Major >= 7) ToReturn |= OSVersions.Windows7OrAbove;

        return ToReturn;
    }).Invoke();

    public static void EmulateVersion(OSVersions OSVersion)
        => Constants.OSVersion = OSVersion;
}
[Flags]
public enum OSVersions
{
    SupportsDWMBackdrop = 0x00001,
    Windows11OrAbove =    0x00010,
    Windows10OrAbove =    0x00100,
    Windows8OrAbove =     0x01000,
    Windows7OrAbove =     0x10000,
    Other =               0x00000
}
public static partial class Extension
{
    public static bool IsDWMBackdropSupported(this OSVersions Version) => Version.HasFlag(OSVersions.SupportsDWMBackdrop);
    public static bool IsWindows11OrAbove(this OSVersions Version) => Version.HasFlag(OSVersions.Windows11OrAbove);
    public static bool IsWindows10OrAbove(this OSVersions Version) => Version.HasFlag(OSVersions.Windows10OrAbove);
    public static bool IsWindows8OrAbove(this OSVersions Version) => Version.HasFlag(OSVersions.Windows8OrAbove);
    public static bool IsWindows7OrAbove(this OSVersions Version) => Version.HasFlag(OSVersions.Windows7OrAbove);
    public static bool IsAeroSupported(this OSVersions Version) => Version.IsWindows7OrAbove() && !Version.IsWindows8OrAbove();
}