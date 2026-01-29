using System.Data;

namespace LibraryManagement.Helpers;

public static class ConvertHelper
{
    public static DataTable BuildIntListTvp(IEnumerable<int>? ids)
    {
        var dt = new DataTable();
        dt.Columns.Add("Value", typeof(int));
        if (ids == null) return dt;
        foreach (var id in ids.Distinct())
        {
            dt.Rows.Add(id);
        }
        return dt;
    }
}