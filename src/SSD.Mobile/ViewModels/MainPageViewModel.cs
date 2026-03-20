using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SSD.Domain.Enums;
using SSD.Mobile.Models;
using SSD.Mobile.Presentation;

namespace SSD.Mobile.ViewModels;

public sealed class MainPageViewModel : INotifyPropertyChanged
{
    private readonly DiscoverState _state = new();
    private bool _isBusy;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<MoodCategory> Moods { get; } = Enum.GetValues<MoodCategory>();

    public MoodCategory SelectedMood
    {
        get => _state.SelectedMood;
        set
        {
            if (_state.SelectedMood == value)
            {
                return;
            }

            _state.SelectedMood = value;
            OnPropertyChanged();
        }
    }

    public string? Energy
    {
        get => _state.Energy;
        set
        {
            if (_state.Energy == value)
            {
                return;
            }

            _state.Energy = value;
            OnPropertyChanged();
        }
    }

    public string? TimeOfDay
    {
        get => _state.TimeOfDay;
        set
        {
            if (_state.TimeOfDay == value)
            {
                return;
            }

            _state.TimeOfDay = value;
            OnPropertyChanged();
        }
    }

    public bool IncludeMusic
    {
        get => _state.IncludeMusic;
        set
        {
            if (_state.IncludeMusic == value)
            {
                return;
            }

            _state.IncludeMusic = value;
            OnPropertyChanged();
        }
    }

    public bool IncludeMovies
    {
        get => _state.IncludeMovies;
        set
        {
            if (_state.IncludeMovies == value)
            {
                return;
            }

            _state.IncludeMovies = value;
            OnPropertyChanged();
        }
    }

    public bool FamilyFriendlyOnly
    {
        get => _state.FamilyFriendlyOnly;
        set
        {
            if (_state.FamilyFriendlyOnly == value)
            {
                return;
            }

            _state.FamilyFriendlyOnly = value;
            OnPropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy == value)
            {
                return;
            }

            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<RecommendationCardViewModel> Recommendations { get; } =
    [
        new(
            "Weightless",
            "Music · Marconi Union",
            "A calm, spacious ambient recommendation for slowing the room down.",
            "Matches a calm mood with gentle pacing and low sensory pressure."),
        new(
            "Paddington 2",
            "Movie · Paul King",
            "A warm, bright comfort-watch that works especially well for cozy sessions.",
            "Matches a cozy mood through warmth, humor, and family-friendly tone.")
    ];

    public async Task DiscoverAsync()
    {
        IsBusy = true;
        try
        {
            await Task.Delay(350);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
