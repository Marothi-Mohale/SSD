using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SSD.Mobile.Models;
using SSD.Mobile.Services;

namespace SSD.Mobile.ViewModels;

public sealed class MainPageViewModel : INotifyPropertyChanged
{
    private readonly MockRecommendationApiClient _apiClient;
    private string _selectedMood = "Calm";
    private bool _includeMusic = true;
    private bool _includeMovies = true;
    private bool _isBusy;

    public MainPageViewModel(MockRecommendationApiClient apiClient)
    {
        _apiClient = apiClient;
        DiscoverCommand = new Command(async () => await DiscoverAsync(), () => !IsBusy);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> Moods { get; } =
    [
        "Calm",
        "Energetic",
        "Focused",
        "Nostalgic",
        "Adventurous",
        "Cozy"
    ];

    public ObservableCollection<RecommendationCard> Recommendations { get; } = [];

    public ICommand DiscoverCommand { get; }

    public string SelectedMood
    {
        get => _selectedMood;
        set => SetProperty(ref _selectedMood, value);
    }

    public bool IncludeMusic
    {
        get => _includeMusic;
        set => SetProperty(ref _includeMusic, value);
    }

    public bool IncludeMovies
    {
        get => _includeMovies;
        set => SetProperty(ref _includeMovies, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                (DiscoverCommand as Command)?.ChangeCanExecute();
            }
        }
    }

    private async Task DiscoverAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Recommendations.Clear();

            var results = await _apiClient.DiscoverAsync(SelectedMood, IncludeMusic, IncludeMovies);
            foreach (var item in results)
            {
                Recommendations.Add(item);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return false;
        }

        storage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
