using PInvoke;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;
using MediaColor = System.Windows.Media.Color;
using Constants = MicaWindow.Environment.Constants;
using MicaWindow.Environment;

namespace MicaWindow;

public partial class MicaWindow : WindowPlus
{
    void InitializeBackdrop()
    {
        HwndSource mainWindowSrc = HwndSource.FromHwnd(Handle);
#if WINDOWS10_0_17763_0_OR_GREATER
        if (Constants.OSVersion.IsDWMBackdropSupported() || UseBackdropAnyway)
        {
            mainWindowSrc.CompositionTarget.BackgroundColor = MediaColor.FromArgb(0, 0, 0, 0);
            DwmApi.DwmExtendFrameIntoClientArea(Handle, new()
            {
                cxLeftWidth = -1,
                cxRightWidth = -1,
                cyTopHeight = -1,
                cyBottomHeight = -1,
            });
            goto RefreshThemeColor;
        }
#endif
        SetValue(WindowChrome.WindowChromeProperty, WindowChrome);
        WindowChrome.GlassFrameThickness = new Thickness(0);
        WindowChrome.UseAeroCaptionButtons = false;

        mainWindowSrc.CompositionTarget.BackgroundColor = IsDarkTheme ? MediaColor.FromArgb(255, 52, 52, 52) :
                MediaColor.FromArgb(255, 250, 250, 250);
        goto RefreshThemeColor;
    RefreshThemeColor:
#if WINDOWS10_0_17763_0_OR_GREATER
        ThemeColorChanged += () => CustomPInvoke.DwmApi.SetWindowAttribute(
            Handle,
            CustomPInvoke.DwmApi.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
            IsDarkTheme ? 1 : 0
        );
        SetBackdrop(BackdropType);
#endif
        ThemeColorChanged?.Invoke();
    }
#if WINDOWS10_0_17763_0_OR_GREATER
    void SetBackdrop(BackdropType BackdropType) => SetBackdrop((int)BackdropType);
    void SetBackdrop(int BackdropType)
    {
        CustomPInvoke.DwmApi.SetWindowAttribute(
            Handle,
            CustomPInvoke.DwmApi.DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
            BackdropType);
    }
#endif

}