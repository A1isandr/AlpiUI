using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

// ReSharper disable CheckNamespace
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

    /// <summary>
    /// Defines the <see cref="TintColor"/> property.
    /// </summary>
    public static readonly StyledProperty<Color> TintColorProperty =
        AvaloniaProperty.Register<BackgroundBlur, Color>(nameof(TintColor), defaultValue: Colors.Transparent);

    /// <summary>
    /// Color of tint.
    /// </summary>
    public Color TintColor
    {
        get => GetValue(TintColorProperty);
        set => SetValue(TintColorProperty, value);
    }
    
    static BackgroundBlur()
    {
        AffectsRender<BackgroundBlur>(BlurRadiusProperty);
        AffectsRender<BackgroundBlur>(TintColorProperty);
    }
    
    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        context.Custom(new BackdropBlurDrawOperation(
            this,
            Bounds,
            BlurRadius,
            TintColor));
        
        base.Render(context);
    }
    
    // private static Color GetEffectiveTintColor(Color tintColor, double tintOpacity) => 
    //     new((byte)(255 * (255.0 / tintColor.A * tintOpacity)), tintColor.R, tintColor.G, tintColor.B);
}



// Made with help from
// https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/GlassMorphism/BlurBackground.cs
// and
// https://gist.github.com/StefanKoell/b7dd1c847984a9a9ae75ed4a96fbc4b5
internal class BackdropBlurDrawOperation(
    Control backgroundBlur,
    Rect controlBounds,
    double blurRadius,
    Color tintColor) : ICustomDrawOperation
{
    private readonly Control _backgroundBlur = backgroundBlur;
    private readonly Rect _controlBounds = controlBounds;
    private readonly double _blurRadius = blurRadius;
    private readonly Color _tintColor = tintColor;
    
    private SKImage? _backgroundSnapshot;
    private bool _disposed;

    public Rect Bounds => _controlBounds;
    
    public bool Equals(ICustomDrawOperation? other)
    {
        return other is BackdropBlurDrawOperation operation &&
               operation._backgroundBlur == _backgroundBlur &&
               operation._controlBounds == _controlBounds &&
               Math.Abs(operation._blurRadius - _blurRadius) < double.Epsilon &&
               operation._tintColor == _tintColor;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _backgroundSnapshot?.Dispose();
        _disposed = true;
    }

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
        
        if (canvas.GetLocalClipBounds(out var bounds) && 
            !bounds.Contains(SKRect.Create(
                bounds.Left,
                bounds.Top,
                (float)_backgroundBlur.Bounds.Width,
                (float)_backgroundBlur.Bounds.Height)))
        {
            Dispatcher.UIThread.Post(() => _backgroundBlur.InvalidateVisual());
        }
        else
        {
            _backgroundSnapshot?.Dispose();
            _backgroundSnapshot = lease.SkSurface?.Snapshot();
        }
        
        _backgroundSnapshot ??= lease.SkSurface?.Snapshot();

        using var backgroundShader = SKShader.CreateImage(
            _backgroundSnapshot,
            SKShaderTileMode.Clamp,
            SKShaderTileMode.Clamp,
            invertedTransform);

        using var blurFilter = SKImageFilter.CreateBlur(
            (float)_blurRadius, 
            (float)_blurRadius, 
            SKShaderTileMode.Clamp);

        using var blurPaint = new SKPaint();
        blurPaint.Shader = backgroundShader;
        blurPaint.ImageFilter = blurFilter;
        blurPaint.ColorFilter = SKColorFilter.CreateBlendMode(_tintColor.ToSKColor(), SKBlendMode.Overlay);
        
        canvas.DrawRect(0, 0, (float)_controlBounds.Width, (float)_controlBounds.Height, blurPaint);
    }
}