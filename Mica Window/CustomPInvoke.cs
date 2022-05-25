using System;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace CustomPInvoke;


class DwmApi
{
    [Flags]
    enum DWM_SYSTEMBACKDROP_TYPE
    {
        DWMSBT_MAINWINDOW = 2, // Mica
        DWMSBT_TRANSIENTWINDOW = 3, // Acrylic
        DWMSBT_TABBEDWINDOW = 4 // Tabbed
    }


    [Flags]
    public enum DWMWINDOWATTRIBUTE
    {
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        DWMWA_SYSTEMBACKDROP_TYPE = 38
    }

    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref int pvAttribute, int cbAttribute);


    public static int SetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute, int parameter)
        => DwmSetWindowAttribute(hwnd, attribute, ref parameter, Marshal.SizeOf<int>());
}
class UxTheme
{
    [DllImport("uxtheme.dll", EntryPoint = "#94")]
    public static extern int GetImmersiveColorSetCount();

    /// <summary>
    /// Retrieves an immersive colour from the specified colour set.
    /// </summary>
    /// <param name="dwImmersiveColorSet">Colour set index. Use <see cref="GetImmersiveColorSetCount"/> to get the number of colour sets available.</param>
    /// <param name="dwImmersiveColorType">The colour type. Use <see cref="GetImmersiveColorTypeFromName"/> to get the type from an element name.</param>
    /// <param name="bIgnoreHighContrast">Set this to true to return colours from the current colour set, even if a high contrast theme is being used.</param>
    /// <param name="dwHighContrastCacheMode">Set this to 1 to force UxTheme to check whether high contrast mode is enabled. If this is set to 0, UxTheme will only perform this check if high contrast mode is currently disabled.</param>
    /// <returns>Returns a colour (0xAABBGGRR).</returns>
    [DllImport("uxtheme.dll", EntryPoint = "#95")]
    public static extern uint GetImmersiveColorFromColorSetEx(uint dwImmersiveColorSet, uint dwImmersiveColorType, bool bIgnoreHighContrast, uint dwHighContrastCacheMode);


    /// <summary>
    /// Retrieves an immersive colour type given its name.
    /// </summary>
    /// <param name="pName">Pointer to a string containing the name preprended with 9 characters (e.g. "Immersive" + name).</param>
    /// <returns>Colour type.</returns>
    [DllImport("uxtheme.dll", EntryPoint = "#96")]
    public static extern uint GetImmersiveColorTypeFromName(IntPtr pName);
    /// <summary>
    /// Gets the user’s colour set preference (or default colour set if the user isn’t allowed to modify this setting according to group policy).
    /// </summary>
    /// <param name="bForceCheckRegistry">Forces update from registry (HKCUSoftwareMicrosoftWindowsCurrentVersionExplorerAccentColorSet_Version3).</param>
    /// <param name="bSkipCheckOnFail">Skip second check if first result is -1.</param>
    /// <returns>User colour set preference.</returns>
    [DllImport("uxtheme.dll", EntryPoint = "#98")]
    public static extern int GetImmersiveUserColorSetPreference(bool bForceCheckRegistry, bool bSkipCheckOnFail);

    /// <summary>
    /// Retrieves names of colour types by index.
    /// </summary>
    /// <param name="dwIndex">Colour type index (0 to 766/0x2fe).</param>
    /// <returns>Pointer to a string containing the colour type’s name.</returns>
    [DllImport("uxtheme.dll", EntryPoint = "#100")]
    public static extern IntPtr GetImmersiveColorNamedTypeByIndex(uint dwIndex);
    public enum AccentColorTypes
    {
        ImmersiveSaturatedHighlight,
        ImmersiveSaturatedInlineErrorText,
        ImmersiveSaturatedSelectionBackground,
        ImmersiveSaturatedCommandRowDisabled,
        ImmersiveSaturatedCommandRowHover
    }
    public static Color GetAccentColor(AccentColorTypes Type)
    {
        int colourset = GetImmersiveUserColorSetPreference(false, false);
        IntPtr pElementName = Marshal.StringToHGlobalUni(Type.ToString());
        uint type = GetImmersiveColorTypeFromName(pElementName);
        Marshal.FreeCoTaskMem(pElementName);
        uint colourdword = GetImmersiveColorFromColorSetEx((uint)colourset, type, false, 0);
        byte[] colourbytes = new byte[4];
        colourbytes[0] = (byte)((0xFF000000 & colourdword) >> 24); // A
        colourbytes[1] = (byte)((0x00FF0000 & colourdword) >> 16); // B
        colourbytes[2] = (byte)((0x0000FF00 & colourdword) >> 8); // G
        colourbytes[3] = (byte)(0x000000FF & colourdword); // R
        return Color.FromArgb(colourbytes[0], colourbytes[3], colourbytes[2], colourbytes[1]);
    }
}
