using System;
using System.Collections.Generic;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.Models.Ranking;

public class PersonWithStatusCounts
{
    /// <summary>
    ///     人员在档案中的 Id，即考勤记录里的字典键。
    /// </summary>
    public required Guid Id { get; set; }

    public required Person Person { get; set; }
    public required List<int> StatusCounts { get; set; }

    /// <summary>
    ///     当天有考勤记录、但该人员一个状态都没有的天数。
    /// </summary>
    public int NoStatusCount { get; set; }
}