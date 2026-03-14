using System.Collections.Generic;
using WandererAttendance.Shared.Models.Profile;

namespace WandererAttendance.Models.Ranking;

public class StatusWithRanking
{
    public required Status Status { get; set; }
    public required List<StatusWithRankingItem> Items { get; set; }
}