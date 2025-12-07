using System.Globalization;
using System.Reflection;
using System.Text;
using App.Application.Interfaces.Services;

namespace App.Application.Services;

public class CsvExportService : IExportService
{
    public async Task<string> ExportToCsvAsync<T>(IEnumerable<T> data, string filePath, CancellationToken cancellationToken = default)
    {
        var csvBytes = await ExportToCsvBytesAsync(data, cancellationToken);
        await File.WriteAllBytesAsync(filePath, csvBytes, cancellationToken);
        return filePath;
    }

    public Task<byte[]> ExportToCsvBytesAsync<T>(IEnumerable<T> data, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Write header
        sb.AppendLine(string.Join(",", properties.Select(p => EscapeCsvField(p.Name))));

        // Write data rows
        foreach (var item in data)
        {
            if (item == null) continue;

            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return EscapeCsvField(value?.ToString() ?? string.Empty);
            });

            sb.AppendLine(string.Join(",", values));
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        // If field contains comma, newline, or double quote, wrap in quotes and escape quotes
        if (field.Contains(',') || field.Contains('\n') || field.Contains('"'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}
