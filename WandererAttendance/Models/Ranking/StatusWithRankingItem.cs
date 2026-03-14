using WandererAttendance.Shared.Models.Profile;

namespace WandererAttendance.Models.Ranking;

public class StatusWithRankingItem
{
    public required Person Person { get; set; }
    public required int Count { get; set; }
}