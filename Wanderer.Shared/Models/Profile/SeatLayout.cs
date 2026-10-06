using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Wanderer.Shared.Models.Profile;

/// <summary>
///     座位表中的一个座位。座位的位置由它在 <see cref="SeatLayout.Seats" /> 中的索引
///     （行优先：行号 * 列数 + 列号）决定，因此座位本身只保存占用者。
/// </summary>
public partial class Seat : ObservableRecipient
{
    /// <summary>
    ///     占用该座位的人员在档案人员表中的键。为 null 表示该座位为空。
    /// </summary>
    [ObservableProperty]
    private Guid? _personKey;
}

/// <summary>
///     档案中的座位布局。座位允许为空，行列数由用户指定或从座位表导入。
/// </summary>
public partial class SeatLayout : ObservableRecipient
{
    /// <summary>
    ///     行列数上限，与座位页可选值保持一致。
    /// </summary>
    public const int MaxCount = 50;

    /// <summary>
    ///     行数。
    /// </summary>
    [ObservableProperty]
    private int _rows = 6;

    /// <summary>
    ///     列数。
    /// </summary>
    [ObservableProperty]
    private int _columns = 8;

    /// <summary>
    ///     座位集合，顺序为行优先。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Seat> _seats = [];

    /// <summary>
    ///     用户是否已经配置过座位表。从未配置时座位页首次打开会进入设置模式。
    /// </summary>
    [JsonIgnore]
    public bool IsConfigured => Seats.Count > 0;

    /// <summary>
    ///     调整座位表的行列数。按 (行, 列) 坐标保留原有座位（含指派），新格子建立空座位，
    ///     越界被裁掉的座位会丢失。
    /// </summary>
    /// <param name="rows">新的行数，会被限制在 1 到 <see cref="MaxCount" /> 之间。</param>
    /// <param name="columns">新的列数，会被限制在 1 到 <see cref="MaxCount" /> 之间。</param>
    /// <param name="droppedOccupied">被裁掉的、原本有人的座位数量。</param>
    /// <returns>调整后的座位总数。</returns>
    public int Resize(int rows, int columns, out int droppedOccupied)
    {
        rows = Math.Clamp(rows, 1, MaxCount);
        columns = Math.Clamp(columns, 1, MaxCount);

        var oldRows = Rows;
        var oldColumns = Columns;
        var old = Seats.ToList();

        droppedOccupied = 0;
        for (var row = 0; row < oldRows; row++)
        {
            for (var column = 0; column < oldColumns; column++)
            {
                var index = row * oldColumns + column;
                if (index >= old.Count) continue;
                if (old[index].PersonKey is null) continue;
                if (row >= rows || column >= columns) droppedOccupied++;
            }
        }

        List<Seat> next = new(rows * columns);
        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                // 只有落在旧网格范围内的坐标才能取到原有座位，否则是新格子。
                var index = column < oldColumns ? row * oldColumns + column : -1;
                next.Add(index >= 0 && index < old.Count ? old[index] : new Seat());
            }
        }

        Rows = rows;
        Columns = columns;
        Seats.Clear();
        foreach (var seat in next)
        {
            Seats.Add(seat);
        }

        return next.Count;
    }
}
