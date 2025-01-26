using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace ApiUI.Controls;

/// <summary>
/// Card with blurred background.
/// </summary>
[TemplatePart("PART_BackgroundBlur", typeof(BackgroundBlur), IsRequired = true)]
public class BlurCard : ContentControl
{
    /// <summary>
    /// Defines the <see cref="BlurRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<double> BlurRadiusProperty =
        AvaloniaProperty.Register<BlurCard, double>(nameof(BlurRadius));

    /// <summary>
    /// Background blur radius.
    /// </summary>
    public double BlurRadius
    {
        get => GetValue(BlurRadiusProperty);
        set => SetValue(BlurRadiusProperty, value);
    }
}