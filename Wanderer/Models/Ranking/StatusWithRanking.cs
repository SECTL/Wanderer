using System.Collections.Generic;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.Models.Ranking;

public class StatusWithRanking
{
    public required Status Status { get; set; }
    public required List<StatusWithRankingItem> Items { get; set; }

    /// <summary>
    ///     显示用的标题。默认为状态名称，"无状态"等特殊分组使用自己的标题。
    /// </summary>
    public string Title => Status.Name;
}