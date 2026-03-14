using System.Collections.Generic;
using WandererAttendance.Shared.Models.Profile;

namespace WandererAttendance.Models.Ranking;

public class PersonWithStatusCounts
{
    public required Person Person { get; set; }
    public required List<int> StatusCounts { get; set; }
}