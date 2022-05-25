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
using Properties;
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
            MainWindow = new MainWindow();
            MainWindow.Show();
        }
    }
}
