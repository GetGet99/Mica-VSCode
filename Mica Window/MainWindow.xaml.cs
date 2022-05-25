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

