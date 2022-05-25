extern alias WV2;
using WV2::Microsoft.Web.WebView2.Core;

using System;
using System.Collections.Generic;
using System.Text;

namespace MicaBrowser;

partial class MicaBrowserWindowNoTab : MicaWindowWithTitleBar
{
    readonly WV2::Microsoft.Web.WebView2.Wpf.WebView2 WebView;
    //ThemeColorChanged +=
    //        () => (Resources["Color"] as SolidColorBrush ?? throw new NullReferenceException()).Color = IsDarkTheme? Colors.White : Colors.Black;
    public MicaBrowserWindowNoTab()
    {
        InitializeComponent();
        Back.Click += (_, _) =>
        {
            if (WebView.CanGoBack) WebView.GoBack();
        };
        Forward.Click += (_, _) =>
        {
            if (WebView.CanGoForward) WebView.GoForward();
        };
        if (!Constants.IsWin11) this.Resources["IconFont"] = new System.Windows.Media.FontFamily("Segoe MDL2 Assets");
        WebView2AddHere.Children.Add(WebView = new WV2::Microsoft.Web.WebView2.Wpf.WebView2
        {
            DefaultBackgroundColor = SysDrawColor.Transparent
        });
        async void InitializeAsync()
        {
            if (WebView == null) return;
            await WebView.EnsureCoreWebView2Async();
            WebView.CoreWebView2.SetVirtualHostNameToFolderMapping("jupyterlite", "./JupyterLite/", CoreWebView2HostResourceAccessKind.Allow);
            WebView.Source = new Uri(@"http://jupyterlite/repl/index.html");

        }
        InitializeAsync();
        TitleBar.Height = 32;
        Load += delegate
        {
            if (!Constants.IsNewTitleBarSupported)
            {
                TitleBarCaptionButtons.Visibility = Visibility.Visible;
                foreach (var child in TitleBarCaptionButtons.Children)
                    WindowChrome.SetIsHitTestVisibleInChrome(child as IInputElement, true);
            }
            WebView.CoreWebView2InitializationCompleted += (_, _) =>
            {
                var CoreWebView2 = WebView.CoreWebView2;
                CoreWebView2.NewWindowRequested += (_, e) =>
                {
                    if (Control.ModifierKeys.HasFlag(Keys.Shift)) return;
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = e.Uri,
                        UseShellExecute = true
                    });
                    e.Handled = true;
                };

                var handle = WebView.Handle;
                _ = User32.SetWindowLong(handle, User32.WindowLongIndexFlags.GWL_EXSTYLE,
                    (User32.SetWindowLongFlags)User32.GetWindowLong(handle, User32.WindowLongIndexFlags.GWL_EXSTYLE)
                    | User32.SetWindowLongFlags.WS_CLIPCHILDREN
                );

                CoreWebView2.DocumentTitleChanged += (_, _) =>
                {
                    var title = WebView.CoreWebView2.DocumentTitle;
                    if (title == "Discord")
                    {
                        title = "";
                        Title = "Mica Discord";
                    }
                    else Title = $"Mica Discord - {title}";
                    WebsiteTitle.Text = title;
                };
                CoreWebView2.HistoryChanged += delegate
                {
                    Back.IsEnabled = WebView.CanGoBack;
                    Forward.IsEnabled = WebView.CanGoForward;
                    RefreshFrame();
                };

                CoreWebView2.Settings.AreDevToolsEnabled = Settings.Default.EnableDevTools;
                void DevToolsCheck(object _, System.Windows.Input.KeyEventArgs e)
                {
                    if (e.Key == Key.I && Control.ModifierKeys == (Keys.Control | Keys.Shift))
                        goto OK;
                    if (e.Key == Key.C && Control.ModifierKeys == (Keys.Control | Keys.Shift))
                        goto OK;
                    if (e.Key == Key.F12)
                        goto OK;

                    e.Handled = false;
                    return;
                OK:
                    if (Settings.Default.EnableDevTools)
                    {
                        if (Settings.Default.ReplaceDiscordBackground)
                        {
#if WINDOWS10_0_17763_0_OR_GREATER
                            WinUIColor PrimaryColor = UISettings.GetColorValue(UIColorType.AccentLight3);
#else
                        SysDrawColor PrimaryColor = SysDrawColor.FromArgb(255, 88, 101, 242);
#endif
                            CoreWebView2.ExecuteScriptAsync($@"
(function() {{
let baseStyles = [
  'color: rgb({PrimaryColor.R}, {PrimaryColor.G}, {PrimaryColor.B})',
  'font-size: 100px',
] 
console.log('%cWARNING!',baseStyles.join(';'));
baseStyles = [
  'color: rgb({PrimaryColor.R}, {PrimaryColor.G}, {PrimaryColor.B})',
  'font-size: 50px',
] ;
console.log('%cDO NOT Paste ANYTHING that you do not understand how it works.', baseStyles.join(';'));
}})()");
                        }
                        e.Handled = false;
                    }
                    else
                        MessageBox.Show(caption: "Warning", text: "DevTools is currently disabled. You can enable it in Settings");
                }
                WebView.KeyDown += DevToolsCheck;

                CoreWebView2.WebMessageReceived += (_, e) =>
                {
                    switch (e.TryGetWebMessageAsString())
                    {
                        case "dark":
                            IsDarkTheme = true;
                            break;
                        case "light":
                            IsDarkTheme = false;
                            break;
                    }
                };

                RefreshFrame();
            };

        };

    }
}
