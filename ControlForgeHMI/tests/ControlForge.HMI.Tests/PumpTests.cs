using System.Threading.Tasks;
using ControlForge.HMI.Models;
using ControlForge.HMI.Services;
using Xunit;

namespace ControlForge.HMI.Tests;

public class PumpTests
{
    [Fact]
    public async Task Start_sets_running_and_state_running()
    {
        var plc = new SimulatedPlcClient();
        var pump = new Pump("Pump01", plc);

        await pump.StartAsync();
        await pump.RefreshAsync();

        Assert.True(pump.Status.Running);
        Assert.Equal(MachineState.Running, pump.State);
    }

    [Fact]
    public async Task ResetFault_clears_fault_and_returns_to_stopped()
    {
        var plc = new SimulatedPlcClient();
        var pump = new Pump("Pump01", plc);

        // simulate a fault
        await plc.WriteAsync("Pump01.Faulted", true);
        await pump.RefreshAsync();

        Assert.True(pump.Status.Faulted);
        Assert.Equal(MachineState.Faulted, pump.State);

        await pump.ResetFaultAsync();
        await pump.RefreshAsync();

        Assert.False(pump.Status.Faulted);
        Assert.Equal(MachineState.Stopped, pump.State);
    }
}
