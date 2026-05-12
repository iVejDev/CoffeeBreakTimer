using CoffeeBreakTimer.Core.Domain.Enums;

namespace CoffeeBreakTimer.App.Controls;

public partial class CoffeeMugView : ContentView
{
    public static readonly BindableProperty CoffeeLevelProperty = BindableProperty.Create(
        nameof(CoffeeLevel),
        typeof(double),
        typeof(CoffeeMugView),
        1.0,
        propertyChanged: OnVisualStateChanged);

    public static readonly BindableProperty SessionTypeProperty = BindableProperty.Create(
        nameof(SessionType),
        typeof(SessionType),
        typeof(CoffeeMugView),
        SessionType.Work,
        propertyChanged: OnVisualStateChanged);

    public static readonly BindableProperty RunStateProperty = BindableProperty.Create(
        nameof(RunState),
        typeof(TimerRunState),
        typeof(CoffeeMugView),
        TimerRunState.Ready,
        propertyChanged: OnVisualStateChanged);

    private CancellationTokenSource? _animationTokenSource;

    public CoffeeMugView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += (_, _) => UpdateLiquid(false);
    }

    public double CoffeeLevel
    {
        get => (double)GetValue(CoffeeLevelProperty);
        set => SetValue(CoffeeLevelProperty, value);
    }

    public SessionType SessionType
    {
        get => (SessionType)GetValue(SessionTypeProperty);
        set => SetValue(SessionTypeProperty, value);
    }

    public TimerRunState RunState
    {
        get => (TimerRunState)GetValue(RunStateProperty);
        set => SetValue(RunStateProperty, value);
    }

    private bool IsActive => RunState == TimerRunState.Running;

    private static void OnVisualStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (CoffeeMugView)bindable;
        view.UpdateLiquid(true);
        view.UpdateAmbientState();
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        UpdateLiquid(false);
        StartAmbientAnimations();
        UpdateAmbientState();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        _animationTokenSource?.Cancel();
        _animationTokenSource = null;
    }

    private void UpdateLiquid(bool animate)
    {
        var containerHeight = LiquidClip.Height > 0 ? LiquidClip.Height : 126;
        var targetHeight = Math.Clamp(CoffeeLevel, 0.0, 1.0) * containerHeight;

        LiquidView.Background = CreateLiquidBrush();
        LiquidView.AbortAnimation("CoffeeLevel");
        LiquidHighlight.AbortAnimation("CoffeeHighlight");

        if (!animate)
        {
            LiquidView.HeightRequest = targetHeight;
            UpdateHighlight(targetHeight, false);
            return;
        }

        var startHeight = LiquidView.HeightRequest < 0 ? targetHeight : LiquidView.HeightRequest;

        LiquidView.Animate(
            "CoffeeLevel",
            new Animation(value =>
            {
                LiquidView.HeightRequest = value;
                UpdateHighlight(value, false);
            }, startHeight, targetHeight, Easing.CubicInOut),
            length: 420);
    }

    private void UpdateHighlight(double liquidHeight, bool animate)
    {
        var opacity = liquidHeight > 14 ? 0.72 : 0;
        var marginBottom = Math.Max(0, liquidHeight - 14);
        LiquidHighlight.Margin = new Thickness(0, 0, 0, marginBottom);

        if (animate)
        {
            LiquidHighlight.FadeTo(opacity, 250, Easing.CubicOut);
            return;
        }

        LiquidHighlight.Opacity = opacity;
    }

    private Brush CreateLiquidBrush()
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1)
        };

        if (SessionType == SessionType.Break)
        {
            brush.GradientStops.Add(new GradientStop(Color.FromArgb("#A8D7DF"), 0));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb("#77AFC0"), 0.55f));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb("#4C7F91"), 1));
            return brush;
        }

        brush.GradientStops.Add(new GradientStop(Color.FromArgb("#C98950"), 0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb("#8D512C"), 0.58f));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb("#5A2E18"), 1));
        return brush;
    }

    private void UpdateAmbientState()
    {
        var showSteam = IsActive && SessionType == SessionType.Work;
        SteamLayer.FadeTo(showSteam ? 0.78 : 0.24, 300, Easing.CubicOut);
        GlowView.FadeTo(IsActive ? 0.78 : 0.2, 450, Easing.CubicOut);
        GlowView.ScaleTo(IsActive ? 1.08 : 0.94, 450, Easing.CubicOut);
    }

    private void StartAmbientAnimations()
    {
        _animationTokenSource?.Cancel();
        _animationTokenSource = new CancellationTokenSource();
        var token = _animationTokenSource.Token;

        _ = RunBreathingAnimationAsync(token);
        _ = RunSteamAnimationAsync(SteamOne, 0, token);
        _ = RunSteamAnimationAsync(SteamTwo, 220, token);
        _ = RunSteamAnimationAsync(SteamThree, 440, token);
    }

    private async Task RunBreathingAnimationAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await MugStage.TranslateTo(0, -7, 1800, Easing.SinInOut);
            await MugStage.TranslateTo(0, 0, 1800, Easing.SinInOut);
        }
    }

    private async Task RunSteamAnimationAsync(VisualElement steam, uint delay, CancellationToken token)
    {
        await Task.Delay((int)delay, token);

        while (!token.IsCancellationRequested)
        {
            steam.TranslationY = 10;
            steam.Opacity = IsActive && SessionType == SessionType.Work ? 0.65 : 0.18;

            await Task.WhenAll(
                steam.TranslateTo(0, -16, 2100, Easing.SinOut),
                steam.FadeTo(IsActive && SessionType == SessionType.Work ? 0.08 : 0.02, 2100, Easing.CubicOut));

            await Task.Delay(120, token);
        }
    }
}
