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

namespace MicaBrowser;

public class MicaDiscord : MicaBrowserWindowNoTab
{
    public bool ForceClose { get; set; } = false;
    public static string DefinedCSS
#if DEBUG
        => File.Exists("../../../The CSS.css") ? File.ReadAllText("../../../The CSS.css") : File.ReadAllText("The CSS.css");// Read File Every time
#else
        = File.ReadAllText("./The CSS.css"); // Read only once
#endif
    public static string DefinedJavascript
#if DEBUG
        => File.Exists("../../../MicaDiscordScript.js") ? File.ReadAllText("../../../MicaDiscordScript.js") : File.ReadAllText("MicaDiscordScript.js");// Read File Every time
#else
        = File.ReadAllText("./MicaDiscordScript.js"); // Read only once
#endif
    public MicaDiscord()
    {
        Load += OnLoaded;
        SettingsDialog.OnClose += () =>
        {
            DialogPlace.Visibility = Visibility.Hidden;
            WebView.Visibility = Visibility.Visible;
            if (SettingsDialog.RequiresReload)
                WebView.Reload();
        };
        SettingsDialog.OnSettingsChanged += () =>
        {
#if WINDOWS10_0_17763_0_OR_GREATER
            SetBackdrop((CustomPInvoke.BackdropType)Enum.Parse(typeof(CustomPInvoke.BackdropType), Settings.Default.BackdropType, ignoreCase: true));
#endif
            var w = WebView.CoreWebView2;
            if (w != null)
            {
                w.Settings.AreDevToolsEnabled = Settings.Default.EnableDevTools;
            }
        };
    }
    bool DiscordEffectApplied = false;
    void OnLoaded(object sender, RoutedEventArgs e)
    {
        Closing += (_, e) =>
        {
            if (!ForceClose && Settings.Default.UseSystemTray)
            {
                e.Cancel = true;
                Hide();
            }
        };
        WebView.NavigationCompleted += async delegate
        {

            if (!WebView.Source.OriginalString.Contains("discord.com")) return;
            DiscordEffectApplied = Settings.Default.ReplaceDiscordBackground;
            if (DiscordEffectApplied)
            {
                var Dark = (await WebView.CoreWebView2.ExecuteScriptAsync("document.getElementsByTagName('html')[0].classList.contains('theme-dark')")) == "true";
                var Light = (await WebView.CoreWebView2.ExecuteScriptAsync("document.getElementsByTagName('html')[0].classList.contains('theme-light')")) == "true";
                if (Dark is false && Light is false)
                    Dark = this.IsDarkTheme; // Don't change
#if WINDOWS10_0_17763_0_OR_GREATER
                WinUIColor
                PrimaryColor = UISettings.GetColorValue(UIColorType.AccentLight3),
                DisabledColor = UISettings.GetColorValue(UIColorType.AccentLight2),
                HoverColor = UISettings.GetColorValue(UIColorType.AccentLight2),
                Accent = UISettings.GetColorValue(UIColorType.Accent);
                DisabledColor.A /= 2;
#else
                SysDrawColor
                        PrimaryColor = SysDrawColor.FromArgb(255, 88, 101, 242),
                        DisabledColor = SysDrawColor.FromArgb(255 / 2, 50, 57, 140),
                        HoverColor = SysDrawColor.FromArgb(255, 69, 79, 191),
                        Accent = SysDrawColor.FromArgb(255, 88, 101, 242);
#endif


                (Resources["Color"] as SolidColorBrush ?? throw new NullReferenceException())
                        .Color = Color.FromArgb(PrimaryColor.A, PrimaryColor.R, PrimaryColor.G, PrimaryColor.B);

                /*
                static double GetHue(int red, int green, int blue)
                {

                    float min = Math.Min(Math.Min(red, green), blue);
                    float max = Math.Max(Math.Max(red, green), blue);

                    if (min == max)
                    {
                        return 0;
                    }

                    float hue;
                    if (max == red) hue = (green - blue) / (max - min);
                    else if (max == green) hue = 2f + (blue - red) / (max - min);
                    else hue = 4f + (red - green) / (max - min);

                    hue *= 60;
                    if (hue < 0) hue += 360;

                    return hue;
                }
                */
                this.IsDarkTheme = Dark;
                var ErrorAccentColor = CustomPInvoke.UxTheme.GetAccentColor(AccentColorTypes.ImmersiveSaturatedInlineErrorText);
                await WebView.CoreWebView2.ExecuteScriptAsync(DefinedJavascript);

                await WebView.CoreWebView2.ExecuteScriptAsync($@"
(function () {{
    let s = document.createElement('style');
    s.innerHTML = `
:root {{ /* System Color */
    --sys-accent-prop: {Accent.R}, {Accent.G}, {Accent.B} !important;
    --sys-accent-alpha: {Accent.A} !important;
    --sys-text-color-on-accent: {((Accent.R * 0.299 + Accent.G * 0.587 + Accent.B * 0.114) > 186 ? "black" : "white")} !important;
    --sys-accent-light-3-prop: {PrimaryColor.R}, {PrimaryColor.G}, {PrimaryColor.B} !important;
    --sys-accent-light-3-alpha: {PrimaryColor.A} !important;
    --sys-text-color-on-accent-light-3: {((PrimaryColor.R * 0.299 + PrimaryColor.G * 0.587 + PrimaryColor.B * 0.114) > 186 ? "black" : "white")} !important;
    --sys-accent-disabled-prop: {DisabledColor.R}, {DisabledColor.G}, {DisabledColor.B} !important;
    --sys-accent-disabled-alpha: {DisabledColor.A} !important;
    --sys-error-accent-prop: {ErrorAccentColor.R}, {ErrorAccentColor.G}, {ErrorAccentColor.B} !important;
    --sys-error-accent-alpha: {ErrorAccentColor.A} !important;
    --sys-hover-accent-prop: {HoverColor.R}, {HoverColor.G}, {HoverColor.B} !important;
    --sys-hover-accent-alpha: {HoverColor.A} !important;
}}`.trim();
    document.head.appendChild(s);
}})()".Trim());
                await WebView.CoreWebView2.ExecuteScriptAsync($@"
(function () {{
    let s = document.createElement('style');
    s.innerHTML = `{DefinedCSS}`;
    document.head.appendChild(s);
}})()".Trim());
                RefreshFrame();
            }
        };
    }
    private void OpenSettings(object sender, RoutedEventArgs e)
    {
        SettingsDialog.ResetRequiresReload();
        DialogPlace.Visibility = Visibility.Visible;
        WebView.Visibility = Visibility.Hidden;
    }
}