using System.Globalization;
using System.Reflection;
using System.Text;
using App.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace App.Application.Services;

public class CsvExportService : IExportService
{
    private readonly ILogger<CsvExportService> _logger;

    public CsvExportService(ILogger<CsvExportService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ExportToCsvAsync<T>(IEnumerable<T> data, string filePath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Exporting data to CSV file. Type: {Type}, FilePath: {FilePath}",
            typeof(T).Name, filePath);

        try
        {
            var csvBytes = await ExportToCsvBytesAsync(data, cancellationToken);
            await File.WriteAllBytesAsync(filePath, csvBytes, cancellationToken);

            _logger.LogInformation(
                "CSV export completed successfully. Type: {Type}, FilePath: {FilePath}, Size: {Size} bytes",
                typeof(T).Name, filePath, csvBytes.Length);

            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "CSV export failed. Type: {Type}, FilePath: {FilePath}",
                typeof(T).Name, filePath);
            throw;
        }
    }

    public Task<byte[]> ExportToCsvBytesAsync<T>(IEnumerable<T> data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Converting data to CSV bytes. Type: {Type}", typeof(T).Name);

        var dataList = data.ToList();
        var sb = new StringBuilder();
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Write header
        sb.AppendLine(string.Join(",", properties.Select(p => EscapeCsvField(p.Name))));

        // Write data rows
        foreach (var item in dataList)
        {
            if (item == null) continue;

            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return EscapeCsvField(value?.ToString() ?? string.Empty);
            });

            sb.AppendLine(string.Join(",", values));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());

        _logger.LogInformation(
            "CSV bytes conversion completed. Type: {Type}, RecordCount: {RecordCount}, Size: {Size} bytes",
            typeof(T).Name, dataList.Count, bytes.Length);

        return Task.FromResult(bytes);
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
