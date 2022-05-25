using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.ComponentModel;

namespace MicaWindow.Controls;

public class Button : System.Windows.Controls.Primitives.ButtonBase
{
    readonly Border Border;
    public TextBlock TextBlock { get; } = new ();
    readonly SolidColorBrush Brush = new(Color.FromRgb(255 / 2, 255 / 2, 255 / 2));
    public Button()
    {
        TextBlock = new TextBlock
        {
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Content = Border = new Border {
            Child = TextBlock
        };
        Background = Brush;
        DependencyPropertyDescriptor.FromProperty(
            IsEnabledProperty,
            typeof(Button)
        ).AddValueChanged(this, delegate
        {
            UpdateBackground();
        });
    }
    void UpdateBackground()
    {
        Border.Opacity = IsEnabled ? 1 : 0.5;
        double opacity = 0;
        if (IsMouseOver) opacity += 0.2;
        if (IsPressed) opacity += 0.2;
        Brush.Opacity = opacity;
        Border.Background = Brush;
    }
    
    protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateBackground();
        base.OnIsPressedChanged(e);
    }
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        UpdateBackground();
        base.OnMouseEnter(e);
    }
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        UpdateBackground();
        base.OnMouseLeave(e);
    }
}