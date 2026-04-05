using System.Collections.Generic;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.Models.Ranking;

public class StatusWithRanking
{
    public required Status Status { get; set; }
    public required List<StatusWithRankingItem> Items { get; set; }
}