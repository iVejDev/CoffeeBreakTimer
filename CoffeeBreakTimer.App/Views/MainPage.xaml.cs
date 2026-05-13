using CoffeeBreakTimer.App.ViewModels;
using CoffeeBreakTimer.Core.Domain.Enums;
using System.ComponentModel;

namespace CoffeeBreakTimer.App.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private int _quoteTransitionId;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        QuoteLabel.Text = viewModel.QuoteText;
        ConfigureButtonAnimation(StartButton);
        ConfigureButtonAnimation(PauseButton);
        ConfigureButtonAnimation(ResetButton);
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        ApplySessionTheme(viewModel.SessionType, false);
    }

    protected override void OnDisappearing()
    {
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        base.OnDisappearing();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SessionType))
        {
            ApplySessionTheme(_viewModel.SessionType, true);
            return;
        }

        if (e.PropertyName == nameof(MainViewModel.QuoteText))
        {
            _ = FadeQuoteAsync();
        }
    }

    private async Task FadeQuoteAsync()
    {
        var transitionId = ++_quoteTransitionId;
        var nextQuote = _viewModel.QuoteText;

        await QuoteLabel.FadeTo(0, 250, Easing.CubicOut);
        if (transitionId != _quoteTransitionId)
        {
            return;
        }

        QuoteLabel.Text = nextQuote;
        await QuoteLabel.FadeTo(1, 250, Easing.CubicIn);
    }

    private void ApplySessionTheme(SessionType sessionType, bool animate)
    {
        var colors = sessionType == SessionType.Break
            ? CreateBreakTheme()
            : CreateFocusTheme();

        if (!animate)
        {
            BackgroundStart.Color = colors.BackgroundStart;
            BackgroundMid.Color = colors.BackgroundMid;
            BackgroundEnd.Color = colors.BackgroundEnd;
            StartButton.BackgroundColor = colors.PrimaryButton;
            return;
        }

        AnimateColor(BackgroundStart.Color, colors.BackgroundStart, color => BackgroundStart.Color = color, 900);
        AnimateColor(BackgroundMid.Color, colors.BackgroundMid, color => BackgroundMid.Color = color, 900);
        AnimateColor(BackgroundEnd.Color, colors.BackgroundEnd, color => BackgroundEnd.Color = color, 900);
        AnimateColor(StartButton.BackgroundColor, colors.PrimaryButton, color => StartButton.BackgroundColor = color, 650);
    }

    private void AnimateColor(Color from, Color to, Action<Color> update, uint length)
    {
        var animation = new Animation(progress => update(Color.FromRgba(
            from.Red + (to.Red - from.Red) * progress,
            from.Green + (to.Green - from.Green) * progress,
            from.Blue + (to.Blue - from.Blue) * progress,
            from.Alpha + (to.Alpha - from.Alpha) * progress)));

        animation.Commit(PageRoot, $"color-{Guid.NewGuid()}", length: length, easing: Easing.CubicInOut);
    }

    private static SessionTheme CreateFocusTheme() => new(
        Color.FromArgb("#171210"),
        Color.FromArgb("#241712"),
        Color.FromArgb("#120D0B"),
        Color.FromArgb("#DCA06A"));

    private static SessionTheme CreateBreakTheme() => new(
        Color.FromArgb("#101A18"),
        Color.FromArgb("#172922"),
        Color.FromArgb("#0B1219"),
        Color.FromArgb("#8DDBB5"));

    private static void ConfigureButtonAnimation(Button button)
    {
        button.Pressed += async (_, _) => await button.ScaleTo(0.97, 80, Easing.CubicOut);
        button.Released += async (_, _) => await button.ScaleTo(1, 120, Easing.CubicOut);

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += async (_, _) => await button.ScaleTo(1.02, 120, Easing.CubicOut);
        pointer.PointerExited += async (_, _) => await button.ScaleTo(1, 120, Easing.CubicOut);
        button.GestureRecognizers.Add(pointer);
    }

    private sealed record SessionTheme(
        Color BackgroundStart,
        Color BackgroundMid,
        Color BackgroundEnd,
        Color PrimaryButton);
}
