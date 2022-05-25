#if WINDOWS10_0_17763_0_OR_GREATER
using Microsoft.UI;
using Microsoft.UI.Windowing;
#endif
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;

namespace MicaWindow;

public class WindowPlus : Window
{
    public WindowPlus() => WindowInteropHelper = new WindowInteropHelper(this);
    protected WindowInteropHelper WindowInteropHelper { get; }
    public IntPtr Handle => WindowInteropHelper.Handle;
#if WINDOWS10_0_17763_0_OR_GREATER
    public WindowId WindowId => Win32Interop.GetWindowIdFromWindow(Handle);
    public AppWindow AppWindow => AppWindow.GetFromWindowId(WindowId);
#endif
    public WindowChrome WindowChrome { get; } = new WindowChrome
    {
        UseAeroCaptionButtons = true,
        ResizeBorderThickness = new Thickness(7.5)
    };
}
