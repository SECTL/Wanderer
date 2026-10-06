using System.Linq;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Wanderer.Controls;

public class AttendanceCalendarControl : Calendar
{
    /// <summary>
    ///     重新读取所有日期格子当天的数据，不重建控件。
    /// </summary>
    public void RefreshData()
    {
        foreach (var dayControl in this.GetVisualDescendants().OfType<AttendanceDayControl>())
        {
            dayControl.RefreshData();
        }
    }

    /// <summary>
    ///     重新读取指定日期格子的数据。
    /// </summary>
    public void RefreshDate(System.DateTime date)
    {
        foreach (var dayControl in this.GetVisualDescendants().OfType<AttendanceDayControl>())
        {
            if (dayControl.Date.Date == date.Date)
            {
                dayControl.RefreshData();
            }
        }
    }
}
