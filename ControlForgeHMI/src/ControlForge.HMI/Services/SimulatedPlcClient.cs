using System.Collections.Concurrent;

namespace ControlForge.HMI.Services;

public sealed class SimulatedPlcClient : IPlcClient
{
    private readonly ConcurrentDictionary<string, object> _tags = new();
    private readonly Random _random = new();

    public SimulatedPlcClient()
    {
        _tags["Pump01.Running"] = false;
        _tags["Pump01.Faulted"] = false;
        _tags["Pump01.Enabled"] = true;
        _tags["Pump01.Speed"] = 0.0;
        _tags["Pump01.Current"] = 0.0;
    }

    public async Task<T> ReadAsync<T>(string tag, CancellationToken cancellationToken = default)
    {
        await Task.Delay(30, cancellationToken);

        SimulateProcessDrift(tag);

        if (_tags.TryGetValue(tag, out var value) && value is T typedValue)
            return typedValue;

        throw new KeyNotFoundException($"Tag '{tag}' was not found or was not of type {typeof(T).Name}.");
    }

    public async Task WriteAsync<T>(string tag, T value, CancellationToken cancellationToken = default)
    {
        await Task.Delay(30, cancellationToken);
        _tags[tag] = value!;

        if (tag == "Pump01.StartCommand" && value is bool start && start)
        {
            _tags["Pump01.Running"] = true;
            _tags["Pump01.Speed"] = 1450.0;
            _tags["Pump01.Current"] = 4.8;
            _tags["Pump01.StartCommand"] = false;
        }
        else if (tag == "Pump01.StopCommand" && value is bool stop && stop)
        {
            _tags["Pump01.Running"] = false;
            _tags["Pump01.Speed"] = 0.0;
            _tags["Pump01.Current"] = 0.0;
            _tags["Pump01.StopCommand"] = false;
        }
        else if (tag == "Pump01.ResetFaultCommand" && value is bool reset && reset)
        {
            _tags["Pump01.Faulted"] = false;
            _tags["Pump01.ResetFaultCommand"] = false;
        }
    }

    private void SimulateProcessDrift(string tag)
    {
        if (tag is not ("Pump01.Speed" or "Pump01.Current"))
            return;

        bool running = (bool)_tags["Pump01.Running"];
        if (!running)
            return;

        double speed = 1450 + _random.NextDouble() * 30 - 15;
        double current = 4.8 + _random.NextDouble() * 0.6 - 0.3;

        _tags["Pump01.Speed"] = Math.Round(speed, 1);
        _tags["Pump01.Current"] = Math.Round(current, 2);
    }
}
