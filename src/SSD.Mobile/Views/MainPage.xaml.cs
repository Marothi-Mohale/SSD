using SSD.Mobile.ViewModels;

namespace SSD.Mobile.Views;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;

    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnDiscoverClicked(object? sender, EventArgs e)
    {
        await _viewModel.DiscoverAsync();
    }
}
