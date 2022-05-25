extern alias WV2;
using WV2::Microsoft.Web.WebView2.Core;
using PInvoke;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
#if WINDOWS10_0_17763_0_OR_GREATER
using Windows.UI.ViewManagement;
#endif
using AccentColorTypes = CustomPInvoke.UxTheme.AccentColorTypes;
using System.IO;
using System.Windows.Input;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
#if WINDOWS10_0_17763_0_OR_GREATER
using WinRT.Interop;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using AppWindow = Microsoft.UI.Windowing.AppWindow;
using WinUIColor = Windows.UI.Color;
#endif
using Colors = System.Windows.Media.Colors;
using SysDrawColor = System.Drawing.Color;
using Color = System.Windows.Media.Color;
using Button = System.Windows.Controls.Button;
using Control = System.Windows.Forms.Control;
using Orientation = System.Windows.Controls.Orientation;

namespace MicaBrowser;



class MicaWindowWithTitleBar : MicaWindow
{
    public MicaWindowWithTitleBar()
    {
        
        Loaded += OnLoaded;

        if (!Constants.IsNewTitleBarSupported)
            SizeChanged += (_, _) =>
            {
                var a = WindowState == WindowState.Maximized ? 7.5 : 0;
                TitleBar.Margin = new Thickness(a, a, 0, 0);
                TitleBarCaptionButtons.Margin = new Thickness(0, 0, 7.5, 0);
                MainContentContainer.Margin = new Thickness(7.5, 0, 7.5, 7.5);
            };
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
#if WINDOWS10_0_17763_0_OR_GREATER
        if (Constants.IsNewTitleBarSupported)
        {
            void RefreshFrame()
            {
                if (!Constants.IsNewTitleBarSupported)
                {
                    WindowChrome.GlassFrameThickness = new Thickness(0);
                    WindowChrome.UseAeroCaptionButtons = false;
                    mainWindowSrc.CompositionTarget.BackgroundColor = IsDarkTheme ? Color.FromArgb(255, 52, 52, 52) :
                           Color.FromArgb(255, 250, 250, 250);
                }

                WindowChrome.CaptionHeight = 32;
            }
            var AppTitleBar = AppWindow.TitleBar;
            AppTitleBar.ExtendsContentIntoTitleBar = true;

            var leftmargin = TitleText.Margin;
            leftmargin.Left += AppTitleBar.LeftInset;
            TitleText.Margin = leftmargin;

            var rightmargin = TitleBarDragable.Margin;
            rightmargin.Right = AppTitleBar.RightInset;
            TitleBarDragable.Margin = rightmargin;
            void UpdateColor()
            {
                var TranColor = new WinUIColor { A = 0 };
                if (IsDarkTheme)
                {
                    TranColor.R = 0;
                    TranColor.G = 0;
                    TranColor.B = 0;
                }
                else
                {
                    TranColor.R = 255;
                    TranColor.G = 255;
                    TranColor.B = 255;
                }
                AppTitleBar.ButtonBackgroundColor = TranColor;
                AppTitleBar.ButtonInactiveBackgroundColor = TranColor;
                byte color = (byte)(255 - TranColor.R);
                TranColor.R = color;
                TranColor.G = color;
                TranColor.B = color;
                TranColor.A = 255 / 10;
                AppTitleBar.ButtonHoverBackgroundColor = TranColor;
                TranColor.A = 255 / 5;
                AppTitleBar.ButtonPressedBackgroundColor = TranColor;
                TranColor.A = 255;
                AppTitleBar.ButtonForegroundColor = TranColor;
                AppTitleBar.ButtonHoverForegroundColor = TranColor;
                AppTitleBar.ButtonInactiveForegroundColor = TranColor;
                AppTitleBar.ButtonPressedForegroundColor = TranColor;
            }
            UpdateColor();
            ThemeColorChanged += UpdateColor;
            void UpdateDragRectangles()
            {
                var Title1Location = TitleText.TransformToVisual(this).Transform(new System.Windows.Point());
                var Title2Location = TitleBarDragable.TransformToVisual(this).Transform(new System.Windows.Point());
                AppTitleBar.SetDragRectangles(new Windows.Graphics.RectInt32[]
                {
                    new Windows.Graphics.RectInt32
                    {
                        X = (int)(Title1Location.X - leftmargin.Left),
                        Y = 0,
                        Width = (int)(TitleText.ActualWidth + leftmargin.Left + leftmargin.Right),
                        Height = (int)TitleBar.ActualHeight
                    },
                    new Windows.Graphics.RectInt32
                    {
                        X = (int)Title2Location.X,
                        Y = 0,
                        Width = (int)TitleBarDragable.ActualWidth,
                        Height = (int)TitleBar.ActualHeight
                    }
                });
            }

            SizeChanged += delegate
            {
                UpdateDragRectangles();
            };
            UpdateDragRectangles();
            goto SetWindowChromeComplete;
        }
#endif
        SetValue(WindowChrome.WindowChromeProperty, WindowChrome);
        WindowChrome.SetIsHitTestVisibleInChrome(Back, true);
        WindowChrome.SetIsHitTestVisibleInChrome(Forward, true);
        WindowChrome.SetIsHitTestVisibleInChrome(Reload, true);
        WindowChrome.SetIsHitTestVisibleInChrome(Setting, true);
        Width += 1;

        SizeChanged += (_, _) => RefreshFrame();
        IsVisibleChanged += (_, _) => RefreshFrame();
    }


    public static ImageSource ImageSourceFromBitmap(Bitmap bmp)
    {
        var handle = bmp.GetHbitmap();
        try
        {
            return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
        finally { Gdi32.DeleteObject(handle); }
    }
    private void RefreshPage(object sender, RoutedEventArgs e)
    {
        try
        {
            WebView.Reload();
        }
        catch { }
    }
    private void Minimize(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void Close(object sender, RoutedEventArgs e)
    {
        Close();
    }
    public static SolidColorBrush GetColorFromHex(string hexaColor)
    {
        return new SolidColorBrush(
            Color.FromArgb(
            Convert.ToByte(hexaColor[1..2], 16),
            Convert.ToByte(hexaColor[3..2], 16),
            Convert.ToByte(hexaColor[5..2], 16),
            Convert.ToByte(hexaColor[7..2], 16)
        ));
    }
    public static Color GetColorFromUInt(uint value)
        => Color.FromArgb(
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)(value)
        );
}


