using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniExcelLibs;
using MiniExcelLibs.Csv;
using MiniExcelLibs.OpenXml;

namespace WandererAttendance.Views.MainPages;

public partial class ProfilePage
{
    private static async Task<List<List<string>>> LoadFromTxtAsync(Stream stream)
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

    private static async Task<List<List<string>>> LoadFromCsvAsync(Stream stream)
    {
        await using var memoryStream = new MemoryStream(await ReadAllBytesAsync(stream), writable: false);
        
        return GetExcelList(memoryStream.Query(configuration: new CsvConfiguration(), excelType: ExcelType.CSV));
    }

    private static async Task<List<List<string>>> LoadFromExcelAsync(Stream stream)
    {
        await using var memoryStream = new MemoryStream(await ReadAllBytesAsync(stream), writable: false);

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
                .OrderBy(kv => kv.Key)
                .Select(kv => kv.Value?.ToString() ?? "")
                .ToList())
            .ToList();
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
