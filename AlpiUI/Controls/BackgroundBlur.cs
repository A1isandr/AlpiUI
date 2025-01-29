using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace AlpiUI.Controls;

/// <summary>
/// Control with ability to blur background.
/// </summary>
public class BackgroundBlur : Control
{
    /// <summary>
    /// Defines the <see cref="BlurRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<double> BlurRadiusProperty = 
        AvaloniaProperty.Register<BackgroundBlur, double>(nameof(BlurRadius), defaultValue: 10.0);
    
    /// <summary>
    /// Background blur radius.
    /// </summary>
    public double BlurRadius
    {
        get => GetValue(BlurRadiusProperty);
        set => SetValue(BlurRadiusProperty, value);
    }

    static BackgroundBlur()
    {
        AffectsRender<BackgroundBlur>(BlurRadiusProperty);
    }
    
    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        context.Custom(new BackdropBlurDrawOperation(Bounds, BlurRadius));
        
        base.Render(context);
    }
}



// Made with help from
// https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/GlassMorphism/BlurBackground.cs
internal class BackdropBlurDrawOperation(Rect controlBounds, double blurRadius) : ICustomDrawOperation
{
    private readonly Rect _controlBounds = controlBounds;
    private readonly double _blurRadius = blurRadius;

    public Rect Bounds => _controlBounds;
    
    public bool Equals(ICustomDrawOperation? other)
    {
        return other is BackdropBlurDrawOperation operation && 
               operation._controlBounds == _controlBounds &&
               Math.Abs(operation._blurRadius - _blurRadius) < 1e-6;
    }

    /// <inheritdoc/>
    public void Dispose() { }

    /// <inheritdoc/>
    public bool HitTest(Point p) => _controlBounds.Contains(p);
    
    /// <inheritdoc/>
    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature is null) return;
        
        using var lease = leaseFeature.Lease();
        var canvas = lease.SkCanvas;

        if (!canvas.TotalMatrix.TryInvert(out var invertedTransform)) return;
        
        using var backgroundSnapshot = lease.SkSurface?.Snapshot();
        if (backgroundSnapshot is null) return;

        using var backgroundShader = SKShader.CreateImage(
            backgroundSnapshot,
            SKShaderTileMode.Clamp,
            SKShaderTileMode.Clamp,
            invertedTransform);

        using var blurFilter = SKImageFilter.CreateBlur((float)_blurRadius, (float)_blurRadius);

        using var blurPaint = new SKPaint();
        blurPaint.Shader = backgroundShader;
        blurPaint.ImageFilter = blurFilter;
        blurPaint.IsAntialias = true;
        
        canvas.DrawRect(0, 0, (float)_controlBounds.Width, (float)_controlBounds.Height, blurPaint);
    }
}