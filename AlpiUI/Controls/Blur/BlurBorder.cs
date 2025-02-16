using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Media;

// ReSharper disable CheckNamespace
namespace AlpiUI.Controls;

/// <summary>
/// Card with blurred background.
/// </summary>
[TemplatePart("PART_BackgroundBlur", typeof(BackgroundBlur), IsRequired = true)]
public class BlurBorder : ContentControl
{
    /// <summary>
    /// Defines the <see cref="BlurRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<double> BlurRadiusProperty =
        AvaloniaProperty.Register<BlurBorder, double>(nameof(BlurRadius));

    /// <summary>
    /// Background blur radius.
    /// </summary>
    public double BlurRadius
    {
        get => GetValue(BlurRadiusProperty);
        set => SetValue(BlurRadiusProperty, value);
    }
    
    /// <summary>
    /// Defines the <see cref="TintColor"/> property.
    /// </summary>
    public static readonly StyledProperty<Color> TintColorProperty =
        AvaloniaProperty.Register<BlurBorder, Color>(nameof(TintColor));
    
    /// <summary>
    /// Color of tint.
    /// </summary>
    public Color TintColor
    {
        get => GetValue(TintColorProperty);
        set => SetValue(TintColorProperty, value);
    }
}