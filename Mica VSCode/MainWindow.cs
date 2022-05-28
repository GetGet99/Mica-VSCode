using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using PInvoke;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using Path = System.IO.Path;
using System.IO;

namespace MicaVSCode
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MicaWindow.MicaWindow
    {
        //class Host : HwndHost
        //{
        //    readonly IntPtr ChildHandle;
        //    public Host(IntPtr Handle)
        //    {
        //        this.ChildHandle = Handle;
        //    }
        //    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        //    {
        //        var style = (User32.SetWindowLongFlags) User32.GetWindowLong(Handle, User32.WindowLongIndexFlags.GWL_STYLE);
        //        if (User32.SetWindowLong(Handle, User32.WindowLongIndexFlags.GWL_STYLE, style | User32.SetWindowLongFlags.WS_CHILD) != 0)
        //        {
        //            throw new Exception();
        //        }
        //        User32.SetParent(ChildHandle, hwndParent.Handle);
        //        return new HandleRef(this, ChildHandle);
        //    }

        //    protected override void DestroyWindowCore(HandleRef hwnd)
        //    {

        //    }
        //}
        public MainWindow()
        {
#if WINDOWS10_0_17763_0_OR_GREATER
            Settings.BackdropType = MicaWindow.BackdropType.Mica;
#endif
            TitleBarContainer.Visibility = Visibility.Visible;
            //var win = new Form
            //{
            //    Controls =
            //    {
            //        new System.Windows.Forms.Label { Text = "Hi!" }
            //    }
            //};
            Loaded += delegate
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "code",
                    UseShellExecute = true,
                });
                Thread.Sleep(2000);

                var windows = GetWindowAPI.GetWindows();
                var window = windows.First(x => x.WinTitle?.Contains("Visual Studio Code") ?? false);
                var handle = (IntPtr)window.MainWindowHandle;
                var hwndhost = new HwndHostEx(handle)
                {
                    Margin = new Thickness(0, -32, 0, 0)
                };
                MainContent = hwndhost;
                hwndhost.Focus();
            };

        }
    }
    class HwndHostEx : HwndHost
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetFocus(IntPtr hWnd);
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            SetFocus(ChildHandle);
            base.OnGotFocus(e);
        }
        protected override bool TabIntoCore(TraversalRequest request)
        {
            SetFocus(ChildHandle);
            return true;
        }
        private IntPtr ChildHandle = IntPtr.Zero;
        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        [Flags]
        public enum LayeredWindowFlags : uint
        {
            LWA_ALPHA = 0x00000002,
            LWA_COLORKEY = 0x00000001,
        }
        
        public HwndHostEx(IntPtr handle)
        {
            this.ChildHandle = handle;
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            HandleRef href = new HandleRef();

            if (ChildHandle != IntPtr.Zero)
            {

                // GetWindowLong(ChildHandle, GWL_STYLE) | 
                _ = User32.SetWindowLong(ChildHandle, User32.WindowLongIndexFlags.GWL_STYLE, User32.SetWindowLongFlags.WS_CHILD | User32.SetWindowLongFlags.WS_EX_LAYERED);
                SetLayeredWindowAttributes(ChildHandle, 0, 128, (uint)LayeredWindowFlags.LWA_ALPHA);

                User32.SetParent(ChildHandle, hwndParent.Handle);
                href = new HandleRef(this, ChildHandle);
            }

            return href;
        }

        protected override void DestroyWindowCore(System.Runtime.InteropServices.HandleRef hwnd)
        {

        }
    }
    public class WinStruct
    {
        public string? WinTitle { get; set; }
        public int MainWindowHandle { get; set; }
    }

    public static class GetWindowAPI
    {
        private delegate bool CallBackPtr(int hwnd, int lParam);
        private readonly static CallBackPtr callBackPtr = Callback;
        private static List<WinStruct> _WinStructList = new();

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(CallBackPtr lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private static bool Callback(int hWnd, int lparam)
        {
            StringBuilder sb = new StringBuilder(256);
            int res = GetWindowText((IntPtr)hWnd, sb, 256);
            _WinStructList.Add(new WinStruct { MainWindowHandle = hWnd, WinTitle = sb.ToString() });
            return true;
        }

        public static List<WinStruct> GetWindows()
        {
            _WinStructList = new List<WinStruct>();
            EnumWindows(callBackPtr, IntPtr.Zero);
            return _WinStructList;
        }
    }
}
