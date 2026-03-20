using SSD.Domain.Enums;
using SSD.Domain.Moods;

namespace SSD.Application.Abstractions;

public interface IMoodRuleCatalog
{
    MoodEngineConfiguration GetConfiguration();

    MoodRuleDefinition GetRule(MoodCategory mood);
}
