using System;
using System.Collections.Generic;
using System.Linq;

namespace Wanderer.Models.Ranking;

/// <summary>
///     某位人员在一个状态上出现过的日期列表，用于"人员日志"页。
/// </summary>
public class PersonStatusLog
{
    /// <summary>
    ///     显示用的标题。默认为状态名称，"无状态"等特殊分组使用自己的标题。
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    ///     该状态出现过的日期，按时间升序排列。
    /// </summary>
    public required List<DateOnly> Dates { get; init; }

    public int Count => Dates.Count;

    /// <summary>
    ///     显示与复制用的日期文本，格式为 yyyy-MM-dd。
    /// </summary>
    public IEnumerable<string> DateTexts => Dates.Select(date => date.ToString("yyyy-MM-dd"));
}
