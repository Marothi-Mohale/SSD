using SSD.Domain.Enums;
using SSD.Mobile.Presentation;

namespace SSD.Mobile.UnitTests;

public sealed class DiscoverStateTests
{
    [Fact]
    public void DiscoverState_StartsWithSafeDefaults()
    {
        var state = new DiscoverState();

        Assert.Equal(MoodCategory.Calm, state.SelectedMood);
        Assert.True(state.IncludeMusic);
        Assert.True(state.IncludeMovies);
        Assert.False(state.FamilyFriendlyOnly);
    }
}
