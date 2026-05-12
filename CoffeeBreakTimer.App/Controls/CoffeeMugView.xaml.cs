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
    private SessionType _previousSessionType = SessionType.Work;

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
        view.DetectSessionTransition();
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
        var containerHeight = LiquidClip.Height > 0 ? LiquidClip.Height : 142;
        var targetHeight = Math.Clamp(CoffeeLevel, 0.0, 1.0) * containerHeight;

        LiquidView.Background = CreateLiquidBrush();
        LiquidView.AbortAnimation("CoffeeLevel");
        LiquidSurfaceLayer.AbortAnimation("CoffeeSurface");
        LiquidWave.AbortAnimation("CoffeeWave");
        LiquidWaveBack.AbortAnimation("CoffeeWaveBack");

        if (!animate)
        {
            LiquidView.HeightRequest = targetHeight;
            UpdateSurface(targetHeight);
            return;
        }

        var startHeight = LiquidView.HeightRequest < 0 ? targetHeight : LiquidView.HeightRequest;

        LiquidView.Animate(
            "CoffeeLevel",
            new Animation(value =>
            {
                LiquidView.HeightRequest = value;
                UpdateSurface(value);
            }, startHeight, targetHeight, Easing.Linear),
            length: 110);
    }

    private void UpdateSurface(double liquidHeight)
    {
        var opacity = liquidHeight > 14 ? 0.92 : 0;
        var marginBottom = Math.Max(0, liquidHeight - 15);
        LiquidSurfaceLayer.Margin = new Thickness(0, 0, 0, marginBottom);
        LiquidSheen.Opacity = liquidHeight > 34 ? 0.18 : 0;
        LiquidSurfaceLayer.Opacity = opacity;
        var bubbleOpacity = liquidHeight > 46 ? 1 : 0;
        BubbleOne.Opacity = bubbleOpacity * 0.44;
        BubbleTwo.Opacity = bubbleOpacity * 0.52;
        BubbleThree.Opacity = bubbleOpacity * 0.38;
    }

    private Brush CreateLiquidBrush()
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1)
        };

        brush.GradientStops.Add(new GradientStop(Color.FromArgb("#D4A06C"), 0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb("#9B5E32"), 0.46f));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb("#5B2B18"), 0.78f));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb("#3C1B0F"), 1));
        return brush;
    }

    private void UpdateAmbientState()
    {
        SteamLayer.FadeTo(GetSteamOpacity(), 350, Easing.CubicOut);

        if (!IsActive)
        {
            GlowView.FadeTo(0.18, 520, Easing.CubicOut);
            GlowView.ScaleTo(0.96, 520, Easing.CubicOut);
        }
    }

    private void StartAmbientAnimations()
    {
        _animationTokenSource?.Cancel();
        _animationTokenSource = new CancellationTokenSource();
        var token = _animationTokenSource.Token;

        _ = RunSteamAnimationAsync(SteamOne, 0, token);
        _ = RunSteamAnimationAsync(SteamTwo, 220, token);
        _ = RunSteamAnimationAsync(SteamThree, 440, token);
        _ = RunSteamAnimationAsync(SteamFour, 660, token);
        _ = RunCoffeeSurfaceAnimationAsync(token);
        _ = RunGlowBreathingAnimationAsync(token);
        _ = RunBubbleAnimationAsync(BubbleOne, 0, token);
        _ = RunBubbleAnimationAsync(BubbleTwo, 620, token);
        _ = RunBubbleAnimationAsync(BubbleThree, 1180, token);
    }

    private async Task RunSteamAnimationAsync(VisualElement steam, uint delay, CancellationToken token)
    {
        await Task.Delay((int)delay, token);

        while (!token.IsCancellationRequested)
        {
            steam.TranslationY = 12;
            steam.Scale = 0.96;
            steam.Opacity = GetSteamOpacity();

            await Task.WhenAll(
                steam.TranslateTo(0, -18, 2600, Easing.SinOut),
                steam.ScaleTo(1.08, 2600, Easing.SinInOut),
                steam.FadeTo(Math.Max(0.03, GetSteamOpacity() * 0.18), 2600, Easing.CubicOut));

            await Task.Delay(120, token);
        }
    }

    private async Task RunCoffeeSurfaceAnimationAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.WhenAll(
                LiquidWave.TranslateTo(42, 0, 1600, Easing.SinInOut),
                LiquidWaveBack.TranslateTo(-34, 0, 1800, Easing.SinInOut),
                LiquidSurfaceLayer.RotateTo(1.2, 1600, Easing.SinInOut));

            await Task.WhenAll(
                LiquidWave.TranslateTo(-20, 0, 1700, Easing.SinInOut),
                LiquidWaveBack.TranslateTo(26, 0, 1500, Easing.SinInOut),
                LiquidSurfaceLayer.RotateTo(-1.2, 1700, Easing.SinInOut));
        }
    }

    private async Task RunGlowBreathingAnimationAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (!IsActive)
            {
                await Task.Delay(240, token);
                continue;
            }

            await Task.WhenAll(
                GlowView.ScaleTo(1.14, 1600, Easing.SinInOut),
                GlowView.FadeTo(0.86, 1600, Easing.SinInOut));

            await Task.WhenAll(
                GlowView.ScaleTo(1.02, 1700, Easing.SinInOut),
                GlowView.FadeTo(0.56, 1700, Easing.SinInOut));
        }
    }

    private async Task RunBubbleAnimationAsync(VisualElement bubble, int delay, CancellationToken token)
    {
        await Task.Delay(delay, token);

        while (!token.IsCancellationRequested)
        {
            var visible = CoffeeLevel > 0.18;
            bubble.TranslationY = 0;
            bubble.Scale = 0.72;
            bubble.Opacity = visible ? 0.42 : 0;

            await Task.WhenAll(
                bubble.TranslateTo(0, -58, 2800, Easing.SinOut),
                bubble.ScaleTo(1.18, 2800, Easing.SinInOut),
                bubble.FadeTo(0, 2800, Easing.CubicOut));

            await Task.Delay(450, token);
        }
    }

    private double GetSteamOpacity()
    {
        if (!IsActive)
        {
            return 0.16;
        }

        var levelIntensity = Math.Clamp(CoffeeLevel, 0.0, 1.0);
        return 0.10 + (levelIntensity * 0.84);
    }

    private void DetectSessionTransition()
    {
        if (_previousSessionType == SessionType.Work && SessionType == SessionType.Break)
        {
            _ = RunFocusFinishedAttentionAsync();
        }

        _previousSessionType = SessionType;
    }

    private async Task RunFocusFinishedAttentionAsync()
    {
        MugStage.AbortAnimation("FocusFinishedPulse");
        MugStage.AbortAnimation("FocusFinishedShake");

        for (var i = 0; i < 3; i++)
        {
            await MugStage.ScaleTo(1.045, 140, Easing.CubicOut);
            await MugStage.TranslateTo(-5, 0, 60, Easing.CubicOut);
            await MugStage.TranslateTo(5, 0, 80, Easing.CubicInOut);
            await MugStage.TranslateTo(0, 0, 70, Easing.CubicOut);
            await MugStage.ScaleTo(1, 180, Easing.CubicInOut);
            await Task.Delay(120);
        }
    }
}
