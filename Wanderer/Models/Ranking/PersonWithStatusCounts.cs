using System.Collections.Generic;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.Models.Ranking;

public class PersonWithStatusCounts
{
    public required Person Person { get; set; }
    public required List<int> StatusCounts { get; set; }
}