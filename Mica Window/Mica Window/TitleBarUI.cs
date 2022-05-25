using MicaWindow.Controls;
using MicaWindow.Environment;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;
using Button = MicaWindow.Controls.Button;
#if WINDOWS10_0_17763_0_OR_GREATER
using WinUIColor = Windows.UI.Color;
using Windows.Graphics;
#endif
namespace MicaWindow;
partial class MicaWindow : IMicaWindowCustomization
{
    Grid? _TitleBar;
    Border? TitleBarElementContainer, MainContentContainer;
    StackPanel? TitleBarCaptionButtons;
#if WINDOWS10_0_17763_0_OR_GREATER
    public delegate void CalculateDragRectanglesEventHandler(ref List<RectInt32> Rectangles);
    public event CalculateDragRectanglesEventHandler? CalculateDragRectangles;
    public void DefaultCalculateDragRectangles(ref List<RectInt32> Rectangles)
    {
        var TitleBarElement = this.TitleBarElement as FrameworkElement;
        if (TitleBarElement == null) return;
        var location = TitleBarElement.TransformToVisual(this).Transform(new Point());
        
        static int Round(double x) => (int)Math.Round(x);

        Rectangles.Add(new RectInt32
        {
            X = Round(location.X),
            Y = Round(location.Y),
            Width = Round(TitleBarElement.ActualWidth),
            Height = Round(TitleBarElement.ActualHeight)
        });
    }
#endif
    public IMicaWindowCustomization Customization => this;
    public int TitleBarHeight { get; set; } = 32;
    public UIElement? TitleBarElement
    {
        get => TitleBarElementContainer?.Child;
        set => (TitleBarElementContainer ?? throw new NullReferenceException()).Child = value;
    }
    public UIElement? MainContent
    {
        get => MainContentContainer?.Child;
        set => (MainContentContainer ?? throw new NullReferenceException()).Child = value;
    }
    
    void InitializeUI()
    {
        Resources["Foreground"] = new SolidColorBrush(IsDarkTheme ? Colors.White : Colors.Black);
        ThemeColorChanged += delegate
        {
            if (Resources["Foreground"] is SolidColorBrush Brush)
                Brush.Color = IsDarkTheme ? Colors.White : Colors.Black;
        };
        Button Creator(string Content, RoutedEventHandler OnClick)
            => new Button
            {
                TextBlock =
                {
                    Text = Content
                },
                Width = 40,
                Height = 40,
                //Style = Styles.ButtonStyle,
                VerticalAlignment = VerticalAlignment.Center
            }.Edit(x =>
            {
                x.SetResourceReference(ForegroundProperty, "Foreground");
                x.Click += OnClick;
                MakeHitTestVisibleInTitleBar(x);
            });
        _TitleBar = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Children =
            {
                new Border
                {
                    Child = new Grid()
                }
                .Assign(out var TitleBarElementContainer)
                .Edit(x => Grid.SetColumn(x, 0)),
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        Creator("-", (_, _) => WindowState = WindowState.Minimized),
                        Creator("❏", (_, _) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized),
                        Creator("X", (_, _) => Close())
                    },
                    Visibility = Visibility.Collapsed
                }.Assign(out var TitleBarCaptionButtons)
                .Edit(x => Grid.SetColumn(x, 1))
            },
            Height = TitleBarHeight
        };
        this.TitleBarElementContainer = TitleBarElementContainer;
        this.TitleBarCaptionButtons = TitleBarCaptionButtons;

        MainContentContainer = new Border();
        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition {Height = GridLength.Auto },
                new RowDefinition {Height = new GridLength(1, GridUnitType.Star)}
            },
            Children =
            {
                _TitleBar.Edit(x => Grid.SetRow(x, 0)),
                MainContentContainer.Edit(x => Grid.SetRow(x, 1))
            }
        };
    }
    void InitializeTitleBarOnLoad()
    {
#if WINDOWS10_0_17763_0_OR_GREATER
        if (Constants.IsNewTitleBarSupported)
        {
            //WindowChrome.CaptionHeight = TitleBarHeight;

            var AppTitleBar = AppWindow.TitleBar;
            AppTitleBar.ExtendsContentIntoTitleBar = true;
            if (TitleBarElementContainer == null) throw new NullReferenceException();
            var Margin = TitleBarElementContainer.Margin;
            Margin.Left += AppTitleBar.LeftInset;
            Margin.Right = AppTitleBar.RightInset;
            TitleBarElementContainer.Margin = Margin;

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
                var rects = new List<RectInt32>();
                CalculateDragRectangles?.Invoke(ref rects);
                AppTitleBar.SetDragRectangles(rects.ToArray());
                //var Title1Location = TitleText.TransformToVisual(this).Transform(new System.Windows.Point());
                //var Title2Location = TitleBarDragable.TransformToVisual(this).Transform(new System.Windows.Point());
                //AppTitleBar.SetDragRectangles(new Windows.Graphics.RectInt32[]
                //{
                //    new Windows.Graphics.RectInt32
                //    {
                //        X = (int)(Title1Location.X - Margin.Left),
                //        Y = 0,
                //        Width = (int)(TitleText.ActualWidth + Margin.Left + Margin.Right),
                //        Height = (int)TitleBar.ActualHeight
                //    },
                //    new Windows.Graphics.RectInt32
                //    {
                //        X = (int)Title2Location.X,
                //        Y = 0,
                //        Width = (int)TitleBarDragable.ActualWidth,
                //        Height = (int)TitleBar.ActualHeight
                //    }
                //});
            }

            SizeChanged += (_, _) => UpdateDragRectangles();
            IsVisibleChanged += (_, _) => UpdateDragRectangles();
            UpdateDragRectangles();
            goto SetWindowChromeComplete;
        }
#endif
        if (TitleBarCaptionButtons != null)
            TitleBarCaptionButtons.Visibility = Visibility.Visible;
        SizeChanged += (_, _) =>
        {
            var a = WindowState == WindowState.Maximized ? 7.5 : 0;
            if (_TitleBar != null)
                _TitleBar.Margin = new Thickness(a, a, a, 0);
            if (MainContentContainer != null)
                MainContentContainer.Margin = new Thickness(7.5, 0, 7.5, 7.5);
        };
        //WindowChrome.SetIsHitTestVisibleInChrome(Back, true);
        //WindowChrome.SetIsHitTestVisibleInChrome(Forward, true);
        //WindowChrome.SetIsHitTestVisibleInChrome(Reload, true);
        //WindowChrome.SetIsHitTestVisibleInChrome(Setting, true);
        goto SetWindowChromeComplete;
    SetWindowChromeComplete:
        //Width += 1;
        ;
    }
    protected static T MakeHitTestVisibleInTitleBar<T>(T Item) where T : IInputElement
    {
        WindowChrome.SetIsHitTestVisibleInChrome(Item, true);
        return Item;
    }
}
public interface IMicaWindowCustomization
{
    UIElement? TitleBarElement { get; set; }
    int TitleBarHeight { get; set; }
    UIElement? MainContent { get; set; }
}
static class Extension
{
    public static T Assign<T>(this T value, out T variable)
    {
        variable = value;
        return value;
    }
    public static T Edit<T>(this T value, Action<T> action)
    {
        action?.Invoke(value);
        return value;
    }
}