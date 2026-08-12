namespace ControlForge.HMI.Services;

public interface IPlcClient
{
    Task<T> ReadAsync<T>(string tag, CancellationToken cancellationToken = default);
    Task WriteAsync<T>(string tag, T value, CancellationToken cancellationToken = default);
}
