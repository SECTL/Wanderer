using Wanderer.Shared.Models.Profile;

namespace Wanderer.Models.Ranking;

public class StatusWithRankingItem
{
    public required Person Person { get; set; }
    public required int Count { get; set; }
}