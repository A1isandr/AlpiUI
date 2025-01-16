using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace ApiUI.Controls;

public class BlurCard : ContentControl
{
    public static readonly StyledProperty<double> BlurRadiusProperty = 
        AvaloniaProperty.Register<BlurCard, double>(nameof(BlurRadius), 10.0);
    
    public double BlurRadius
    {
        get => GetValue(BlurRadiusProperty);
        set => SetValue(BlurRadiusProperty, value);
    }

    public static readonly DirectProperty<BlurCard, RenderTargetBitmap?> BlurBitmapProperty =
        AvaloniaProperty.RegisterDirect<BlurCard, RenderTargetBitmap?>(
            nameof(BlurBitmap),
            b => b.BlurBitmap);
    
    private RenderTargetBitmap? _blurBitmap;

    public RenderTargetBitmap? BlurBitmap
    {
        get => _blurBitmap;
        private set => SetAndRaise(BlurBitmapProperty, ref _blurBitmap, value);
    }
        
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        
        BlurBitmap = new RenderTargetBitmap(PixelSize.FromSize(new Size(Bounds.Width, Bounds.Height), 1));
    }
}