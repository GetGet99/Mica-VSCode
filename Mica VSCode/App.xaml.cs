using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MicaWindow;
using PInvoke;
using Properties;
using static MicaVSCode.HwndHostEx;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;

namespace MicaVSCode
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            if (!Settings.Default.AlreadySetup)
            {
                if (
                    MessageBox.Show(
                        caption: "Initial Setup",
                        text: "We're going to install some extension in VSCode. " +
                        "It is going to affect your normal use of visual studio code also!\n" +
                        "Would you like to continue anyway?",
                        buttons: MessageBoxButtons.YesNo
                    ) == DialogResult.Yes)
                {
                    var proc = Process.Start(new ProcessStartInfo
                    {
                        FileName = "code",
                        Arguments = "--install-extension eyhn.vscode-vibrancy",
                        UseShellExecute = true,
                    });
                    while (!proc?.HasExited ?? false)
                    {
                        Thread.Sleep(100);
                    }
                    proc = Process.Start(new ProcessStartInfo
                    {
                        FileName = "code",
                        Arguments = "--install-extension lehni.vscode-fix-checksums",
                        UseShellExecute = true,
                    });
                    while (!proc?.HasExited ?? false)
                    {
                        Thread.Sleep(100);
                    }
                    Settings.Default.AlreadySetup = true;
                    Settings.Default.Save();
                }
            }
            //Process.Start(new ProcessStartInfo
            //{
            //    FileName = "code",
            //    UseShellExecute = true,
            //});
            //Thread.Sleep(2000);
            //var f = new Form();
            //f.HandleCreated += delegate
            //{
            //    var windows = GetWindowAPI.GetWindows();
            //    var window = windows.First(x => x.WinTitle?.Contains("Visual Studio Code") ?? false);
            //    var handle = (IntPtr)window.MainWindowHandle;
            //    _ = User32.SetWindowLong(handle, User32.WindowLongIndexFlags.GWL_STYLE, User32.SetWindowLongFlags.WS_CHILD);
            //    User32.SetParent(handle, f.Handle);
            //    SetLayeredWindowAttributes(handle, 0, 128, (uint)LayeredWindowFlags.LWA_ALPHA);
            //    SetBackdrop(handle, BackdropType.Mica);
            //};
            MainWindow = new MainWindow();
            MainWindow.Show();
        }
        static void SetBackdrop(IntPtr handle, BackdropType BackdropType) => SetBackdrop(handle, (int)BackdropType);
        static void SetBackdrop(IntPtr handle, int BackdropType)
        {
            CustomPInvoke.DwmApi.SetWindowAttribute(
                handle,
                CustomPInvoke.DwmApi.DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                BackdropType);
        }
    }
}
