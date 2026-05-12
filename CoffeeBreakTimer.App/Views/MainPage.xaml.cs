using CoffeeBreakTimer.App.ViewModels;

namespace CoffeeBreakTimer.App.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
