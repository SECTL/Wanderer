using System.Collections.Generic;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.Models;

public record StatusAndCount
{
    /// <summary>
    ///     "无状态"分组（当天有考勤记录、但该人员一个状态都没有）使用的标题。
    /// </summary>
    public const string NoStatusTitle = "无状态";

    /// <summary>
    ///     "无状态"分组使用的占位状态。
    /// </summary>
    public static readonly Status NoStatus = new(NoStatusTitle);

    public required Status Status { get; init; }
    public required int Count { get; init; }
    public required List<Person> Persons { get; init; }

    /// <summary>
    ///     显示用的标题。默认为状态名称，特殊分组使用自己的标题。
    /// </summary>
    public string Title => Status.Name;
}
