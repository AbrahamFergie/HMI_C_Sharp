using System.Collections.ObjectModel;
using System.Windows.Input;
using ControlForge.HMI.Commands;
using ControlForge.HMI.Models;
using ControlForge.HMI.Services;

namespace ControlForge.HMI.ViewModels;

public sealed class MainViewModel : ViewModelBase, IDisposable
{
    private readonly Pump _pump;
    private readonly CancellationTokenSource _cts = new();

    private string _state = "Stopped";
    private bool _running;
    private bool _faulted;
    private bool _enabled;
    private double _speed;
    private double _current;
    private string _statusMessage = "Ready";

    public MainViewModel()
    {
        IPlcClient plc = new SimulatedPlcClient();
        _pump = new Pump("Pump01", plc);

        // Commands expose RaiseCanExecuteChanged so the VM can update their enabled state
        StartCommand = new AsyncRelayCommand(StartAsync, () => !Running && !Faulted);
        StopCommand = new AsyncRelayCommand(StopAsync, () => Running);
        ResetFaultCommand = new AsyncRelayCommand(ResetFaultAsync, () => Faulted);

        _ = PollAsync(_cts.Token);
    }

    public string PumpName => _pump.Name;

    public string State
    {
        get => _state;
        private set => SetField(ref _state, value);
    }

    // Expose public setter so TwoWay bindings can update Running.
    public bool Running
    {
        get => _running;
        set
        {
            if (SetField(ref _running, value))
            {
                StartCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
                ResetFaultCommand.RaiseCanExecuteChanged();
                // Ensure property change notification follows the project's pattern.
                OnPropertyChanged(nameof(Running));
            }
        }
    }

    public bool Faulted
    {
        get => _faulted;
        private set
        {
            if (SetField(ref _faulted, value))
            {
                StartCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
                ResetFaultCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool Enabled
    {
        get => _enabled;
        set => SetField(ref _enabled, value);
    }

    public double Speed
    {
        get => _speed;
        private set => SetField(ref _speed, value);
    }

    public double Current
    {
        get => _current;
        private set => SetField(ref _current, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetField(ref _statusMessage, value);
    }

    public ObservableCollection<string> EventLog { get; } = new();

    // Expose concrete AsyncRelayCommand so the VM can raise CanExecuteChanged
    public AsyncRelayCommand StartCommand { get; }
    public AsyncRelayCommand StopCommand { get; }
    public AsyncRelayCommand ResetFaultCommand { get; }

    private async Task StartAsync()
    {
        try
        {
            State = MachineState.Starting.ToString();
            StatusMessage = "Sending start command...";
            EventLog.Insert(0, $"{DateTime.Now:T} Start command requested");

            await _pump.StartAsync(_cts.Token);

            // The simulated PLC will update running state quickly; PollAsync will observe it.
            StatusMessage = "Start command sent";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private async Task StopAsync()
    {
        try
        {
            State = MachineState.Stopping.ToString();
            StatusMessage = "Sending stop command...";
            EventLog.Insert(0, $"{DateTime.Now:T} Stop command requested");

            await _pump.StopAsync(_cts.Token);

            StatusMessage = "Stop command sent";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private async Task ResetFaultAsync()
    {
        try
        {
            StatusMessage = "Sending reset fault command...";
            EventLog.Insert(0, $"{DateTime.Now:T} Fault reset requested");

            await _pump.ResetFaultAsync(_cts.Token);

            StatusMessage = "Fault reset command sent";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private async Task PollAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _pump.RefreshAsync(cancellationToken);

                // capture previous state for transition logging
                var previousState = State;

                Running = _pump.Status.Running;
                Faulted = _pump.Status.Faulted;
                Enabled = _pump.Status.Enabled;
                Speed = _pump.Status.Speed;
                Current = _pump.Status.Current;
                State = _pump.State.ToString();

                // if state changed, log it
                if (previousState != State)
                {
                    EventLog.Insert(0, $"{DateTime.Now:T} State changed: {previousState} => {State}");
                    StatusMessage = $"State: {State}";
                }
                else
                {
                    StatusMessage = "PLC connected";
                }

                await Task.Delay(250, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Communication error: {ex.Message}";
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    // Local helper that prefers a base OnPropertyChanged implementation if present,
    // otherwise raises the PropertyChanged event via reflection. This keeps the
    // notification behavior safe whether or not the base class exposes a helper.
    protected void OnPropertyChanged(string propertyName)
    {
        var baseType = GetType().BaseType;
        if (baseType != null)
        {
            var method = baseType.GetMethod("OnPropertyChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public, null, new[] { typeof(string) }, null);
            if (method != null)
            {
                method.Invoke(this, new object[] { propertyName });
                return;
            }

            var field = baseType.GetField("PropertyChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                var handler = field.GetValue(this) as System.ComponentModel.PropertyChangedEventHandler;
                handler?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                return;
            }
        }

        // Fallback: try to invoke an event named PropertyChanged on this type if present.
        var selfField = GetType().GetField("PropertyChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        if (selfField != null)
        {
            var handler = selfField.GetValue(this) as System.ComponentModel.PropertyChangedEventHandler;
            handler?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
