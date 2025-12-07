namespace App.Application.Interfaces.Services;

public interface IExportService
{
    Task<string> ExportToCsvAsync<T>(IEnumerable<T> data, string filePath, CancellationToken cancellationToken = default);
    Task<byte[]> ExportToCsvBytesAsync<T>(IEnumerable<T> data, CancellationToken cancellationToken = default);
}
