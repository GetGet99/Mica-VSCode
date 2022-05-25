using System;
using System.Windows.Media;

namespace MicaWindow;

public partial class MicaWindow : WindowPlus
{
    public MicaWindow()
    {
        Background = Brushes.Transparent;
        AllowsTransparency = false;
        InitializeUI();
#if WINDOWS10_0_17763_0_OR_GREATER
        CalculateDragRectangles += DefaultCalculateDragRectangles;
#endif
        Loaded += delegate
        {
            InitializeBackdrop();
            InitializeTitleBarOnLoad();
        };
    }
}

