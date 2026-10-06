using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniExcelLibs;
using MiniExcelLibs.Csv;
using MiniExcelLibs.OpenXml;

namespace Wanderer.Shared;

/// <summary>
///     名单/座位表文件读取（.txt、.csv、.xlsx），返回“行 → 单元格文本”的二维表。
///     档案页与座位页共用。
/// </summary>
public static class SheetLoader
{
    /// <summary>
    ///     按扩展名读取名单文件。不支持的类型返回空表。
    /// </summary>
    /// <param name="stream">文件内容流。</param>
    /// <param name="extension">小写扩展名，含点号。</param>
    public static async Task<List<List<string>>> LoadAsync(Stream stream, string extension)
    {
        return extension switch
        {
            ".txt"  => await LoadFromTxtAsync(stream),
            ".xlsx" => await LoadFromExcelAsync(stream),
            ".csv"  => await LoadFromCsvAsync(stream),
            _       => []
        };
    }

    public static async Task<List<List<string>>> LoadFromTxtAsync(Stream stream)
    {
        var content = Encoding.UTF8.GetString(await ReadAllBytesAsync(stream));
        using var reader = new StringReader(content);

        List<List<string>> lines = [];
        while (await reader.ReadLineAsync() is { } line)
        {
            lines.Add([line.Trim()]);
        }

        return lines;
    }

    public static async Task<List<List<string>>> LoadFromCsvAsync(Stream stream)
    {
        await using var memoryStream = new MemoryStream(await ReadAllBytesAsync(stream), false);

        return GetExcelList(memoryStream.Query(configuration: new CsvConfiguration(), excelType: ExcelType.CSV));
    }

    public static async Task<List<List<string>>> LoadFromExcelAsync(Stream stream)
    {
        await using var memoryStream = new MemoryStream(await ReadAllBytesAsync(stream), false);

        var config = new OpenXmlConfiguration
        {
            FillMergedCells = true
        };
        return GetExcelList(memoryStream.Query(configuration: config));
    }

    private static List<List<string>> GetExcelList(IEnumerable<dynamic> excel)
    {
        return excel
               .Select(row => (IDictionary<string, object?>)row)
               .Select(dict => dict
                               .OrderBy(kv => ColumnIndex(kv.Key))
                               .Select(kv => kv.Value?.ToString() ?? "")
                               .ToList())
               .ToList();
    }

    /// <summary>
    ///     把列名（"A"、"B"、…、"AA"）换算为从 1 开始的列号。
    ///     MiniExcel 的行是按列名索引的字典，直接按字符串排序会在超过 26 列时错位（A、AA、B…）。
    /// </summary>
    private static int ColumnIndex(string key)
    {
        var index = 0;
        foreach (var character in key)
        {
            if (character is >= 'A' and <= 'Z')
            {
                index = index * 26 + (character - 'A' + 1);
            }
            else if (character is >= 'a' and <= 'z')
            {
                index = index * 26 + (character - 'a' + 1);
            }
            else
            {
                return int.MaxValue;
            }
        }

        return index;
    }

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream)
    {
        var memoryStream = new MemoryStream();
        var buffer = new byte[16 * 1024];

        while (true)
        {
            var bytesRead = await stream.ReadAsync(buffer);
            if (bytesRead == 0)
            {
                break;
            }

            memoryStream.Write(buffer, 0, bytesRead);
        }

        return memoryStream.ToArray();
    }
}
