using SSD.Domain.Enums;
using SSD.Mobile.Models;

namespace SSD.Mobile.Presentation;

public sealed class DiscoverState
{
    public MoodCategory SelectedMood { get; set; } = MoodCategory.Relaxed;

    public string? Energy { get; set; } = "low";

    public string? TimeOfDay { get; set; } = "evening";

    public bool IncludeMusic { get; set; } = true;

    public bool IncludeMovies { get; set; } = true;

    public bool FamilyFriendlyOnly { get; set; }

    public IReadOnlyList<RecommendationCardViewModel> Recommendations { get; set; } = [];
}
