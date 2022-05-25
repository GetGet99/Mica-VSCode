using System;
using MicaWindow.Environment;
using Constants = MicaWindow.Environment.Constants;
#if WINDOWS10_0_17763_0_OR_GREATER
using Windows.UI.ViewManagement;
using WinUIColor = Windows.UI.Color;
#endif

namespace MicaWindow;

public enum BackdropTheme
{
    Light = 0,
    Dark = 1
}
public enum BackdropType
{
    Regular = 1,
    Mica = 2,
    Acrylic = 3,
    Tabbed = 4
}

public interface IMicaWindowSettings
{
    bool UseBackdropAnyway { get; }
#if WINDOWS10_0_17763_0_OR_GREATER
    BackdropType BackdropType { get; set; }
#endif
    BackdropTheme ThemeColor { get; set; }
}
partial class MicaWindow : IMicaWindowSettings
{
    public IMicaWindowSettings Settings => this;
    public bool _UseBackdropAnyway = false;
    public bool UseBackdropAnyway => _UseBackdropAnyway;
#if WINDOWS10_0_17763_0_OR_GREATER
    BackdropType _BackdropType = Constants.OSVersion.IsDWMBackdropSupported() ? BackdropType.Mica : BackdropType.Regular;
    public BackdropType BackdropType
    {
        get => _BackdropType;
        set
        {
            if (UseBackdropAnyway || Constants.OSVersion.IsDWMBackdropSupported() || value == BackdropType.Regular)
            {
                _BackdropType = value;
                SetBackdrop(value);
            }
        }
    }
#endif
    public BackdropTheme ThemeColor
    {
        get => IsDarkTheme ? BackdropTheme.Dark : BackdropTheme.Light;
        set
        {
            switch (value)
            {
                case BackdropTheme.Light:
                    IsDarkTheme = false;
                    break;
                case BackdropTheme.Dark:
                    IsDarkTheme = true;
                    break;
            }
        }
    }

    bool _Dark =
#if WINDOWS10_0_17763_0_OR_GREATER
        IsDarkBackground(Constants.UISettings.GetColorValue(UIColorType.Background));
    static bool IsDarkBackground(WinUIColor color)
        => color.R + color.G + color.B < (255 * 3 - color.R - color.G - color.B);
#else
        true;
#endif


    bool IsDarkTheme
    {
        set
        {
            _Dark = value;
            ThemeColorChanged?.Invoke();
        }
        get => _Dark;
    }
}
partial class MicaWindow : IMicaWindowEvents
{
    public IMicaWindowEvents Events => this;
    public event Action? ThemeColorChanged = null;
}
public interface IMicaWindowEvents
{
    public event Action? ThemeColorChanged;
}
