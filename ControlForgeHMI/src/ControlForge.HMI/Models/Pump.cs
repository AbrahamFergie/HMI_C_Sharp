using ControlForge.HMI.Services;

namespace ControlForge.HMI.Models;

public sealed class Pump
{
    private readonly IPlcClient _plc;

    public string Name { get; }
    public PumpStatus Status { get; } = new();
    public MachineState State { get; private set; } = MachineState.Stopped;

    public Pump(string name, IPlcClient plc)
    {
        Name = name;
        _plc = plc;
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Status.Running = await _plc.ReadAsync<bool>($"{Name}.Running", cancellationToken);
        Status.Faulted = await _plc.ReadAsync<bool>($"{Name}.Faulted", cancellationToken);
        Status.Enabled = await _plc.ReadAsync<bool>($"{Name}.Enabled", cancellationToken);
        Status.Speed = await _plc.ReadAsync<double>($"{Name}.Speed", cancellationToken);
        Status.Current = await _plc.ReadAsync<double>($"{Name}.Current", cancellationToken);

        State = Status.Faulted
            ? MachineState.Faulted
            : Status.Running
                ? MachineState.Running
                : MachineState.Stopped;
    }

    public Task StartAsync(CancellationToken cancellationToken = default) =>
        _plc.WriteAsync($"{Name}.StartCommand", true, cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken = default) =>
        _plc.WriteAsync($"{Name}.StopCommand", true, cancellationToken);

    public Task ResetFaultAsync(CancellationToken cancellationToken = default) =>
        _plc.WriteAsync($"{Name}.ResetFaultCommand", true, cancellationToken);
}
