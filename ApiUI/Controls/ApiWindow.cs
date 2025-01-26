using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace ApiUI.Controls
{
    /// <summary>
    /// Api window.
    /// </summary>
    [TemplatePart(PartMinimizeButton, typeof(Button), IsRequired = true)]
    [TemplatePart(PartMaximizeButton, typeof(Button), IsRequired = true)]
    [TemplatePart(PartCloseButton, typeof(Button), IsRequired = true)]
    [TemplatePart(PartTitleBarBackground, typeof(Decorator), IsRequired = true)]
    public class ApiWindow : Window
    {
        private IDisposable? _subscriptionDisposables;

        private const string PartMinimizeButton = "PART_MinimizeButton";
        private const string PartMaximizeButton = "PART_MaximizeButton";
        private const string PartCloseButton = "PART_CloseButton";
        private const string PartTitleBarBackground = "PART_TitleBarBackground";
        
        /// <summary>
        /// Returns the type of the style key for the <see cref="ApiWindow"/> class.
        /// </summary>
        protected override Type StyleKeyOverride => typeof(ApiWindow);

        /// <summary>
        /// Defines the <see cref="LogoContent"/> property.
        /// </summary>
        public static readonly StyledProperty<Control?> LogoContentProperty =
            AvaloniaProperty.Register<ApiWindow, Control?>(nameof(LogoContent));

        /// <summary>
        /// Content that is displayed on the left corner of title bar.
        /// </summary>
        public Control? LogoContent
        {
            get => GetValue(LogoContentProperty);
            set => SetValue(LogoContentProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="TitleBarControls"/> property.
        /// </summary>
        public static readonly StyledProperty<Avalonia.Controls.Controls?> TitleBarControlsProperty =
            AvaloniaProperty.Register<ApiWindow, Avalonia.Controls.Controls?>(nameof(TitleBarControls));

        /// <summary>
        /// Controls that are displayed on the title bar.
        /// </summary>
        public Avalonia.Controls.Controls? TitleBarControls
        {
            get => GetValue(TitleBarControlsProperty);
            set => SetValue(TitleBarControlsProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="BackgroundAcrylicMaterialProperty"/> property.
        /// </summary>
        public static readonly StyledProperty<ExperimentalAcrylicMaterial> BackgroundAcrylicMaterialProperty =
            AvaloniaProperty.Register<ApiWindow, ExperimentalAcrylicMaterial>(nameof(Background),
                defaultValue: new ExperimentalAcrylicMaterial());

        /// <summary>
        /// Background acrylic material of window.
        /// </summary>
        public ExperimentalAcrylicMaterial BackgroundAcrylicMaterial
        {
            get => GetValue(BackgroundAcrylicMaterialProperty);
            set => SetValue(BackgroundAcrylicMaterialProperty, value);
        }
        
        /// <summary>
        /// Defines the <see cref="TitleBarBackgroundProperty"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush> TitleBarBackgroundProperty =
            AvaloniaProperty.Register<ApiWindow, IBrush>(nameof(TitleBarBackground));
        
        /// <summary>
        /// Title bar background.
        /// </summary>
        public IBrush TitleBarBackground
        {
            get => GetValue(TitleBarBackgroundProperty);
            set => SetValue(TitleBarBackgroundProperty, value);
        }
        
        
        
        /// <summary>
        /// Api window constructor.
        /// </summary>
        public ApiWindow()
        {
            BackgroundAcrylicMaterial = new ExperimentalAcrylicMaterial
            {
                BackgroundSource = AcrylicBackgroundSource.Digger,
                TintOpacity = 0.4,
                MaterialOpacity = .0,
                TintColor = Colors.Black,
                FallbackColor = Colors.Transparent
            };

            Activated += (_, _) =>
            {
                TitleBarBackground = Brushes.Transparent;
            };
            Deactivated += (_, _) =>
            {
                TitleBarBackground = Background ?? Brushes.Transparent;
            };
        }
        
        
        
        /// <inheritdoc/>
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return;

            if (desktop.MainWindow is ApiWindow window && window != this)
            {
                Icon ??= window.Icon;
            }
        }
        
        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _subscriptionDisposables = this
                .GetObservable(WindowStateProperty)
                .ObserveOn(new AvaloniaSynchronizationContext())
                .Subscribe(OnWindowStateChanged);

            try
            {
                if (e.NameScope.Get<Button>("PART_MaximizeButton") is { } maximize)
                {
                    maximize.Click += OnMaximizeButtonClicked;
                    var pointerOnMaxButton = false;
                    var setter = typeof(Button).GetProperty("IsPointerOver");

                    // Windows Snap Layout
                    var proc =
                        (IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
                        {
                            switch (msg)
                            {
                                case 533:
                                    if (!pointerOnMaxButton) break;
                                    if (!CanResize) break;

                                    WindowState = WindowState == WindowState.Maximized
                                    ? WindowState.Normal
                                    : WindowState.Maximized;
                                    break;
                                case 0x0084:
                                    var point = new PixelPoint(
                                        (short)(ToInt32(lParam) & 0xffff),
                                        (short)(ToInt32(lParam) >> 16));
                                    
                                    var buttonLeftTop = maximize.PointToScreen(new Point(0, 0));
                                    var x = (point.X - buttonLeftTop.X) / RenderScaling;
                                    var y = (point.Y - buttonLeftTop.Y) / RenderScaling;

                                    if (new Rect(0, 0,
                                        maximize.Bounds.Width,
                                        maximize.Bounds.Height)
                                        .Contains(new Point(x, y)))
                                    {
                                        setter?.SetValue(maximize, true);
                                        pointerOnMaxButton = true;
                                        handled = true;
                                        return 9;
                                    }

                                    pointerOnMaxButton = false;
                                    setter?.SetValue(maximize, false);
                                    break;
                            }

                            return IntPtr.Zero;

                            static int ToInt32(IntPtr ptr) =>
                                IntPtr.Size == 4
                                    ? ptr.ToInt32()
                                    : (int)(ptr.ToInt64() & 0xffffffff);
                        };

                    Win32Properties.AddWndProcHookCallback(this,
                        new Win32Properties.CustomWndProcHookCallback(proc));
                }
            }
            catch { }
            
            if (e.NameScope.Get<Button>("PART_MinimizeButton") is { } minimize)
            {
                minimize.Click += (_, _) => WindowState = WindowState.Minimized;   
            }

            if (e.NameScope.Get<Button>("PART_CloseButton") is { } close)
            {
                close.Click += (_, _) => Close();   
            }

            if (e.NameScope.Get<Border>("PART_TitleBarBackground") is { } titleBar)
            {
                titleBar.PointerPressed += OnTitleBarPointerPressed;
                titleBar.DoubleTapped += OnMaximizeButtonClicked;
            }
        }

        /// <summary>
        /// On maximize button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnMaximizeButtonClicked(object? sender, RoutedEventArgs args)
        {
            if (!CanResize) return;

            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        /// <summary>
        /// On window state changed.
        /// </summary>
        /// <param name="state"></param>
        private void OnWindowStateChanged(WindowState state)
        {
            if (state == WindowState.FullScreen)
                CanResize = false;

            Margin = state == WindowState.Maximized 
                ? new Thickness(5)
                : new Thickness(0);
        }

        /// <summary>
        /// On title bar pointer pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            BeginMoveDrag(e);
        }

        /// <inheritdoc/>
        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            _subscriptionDisposables?.Dispose();
        }
    }
}
