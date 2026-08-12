namespace ControlForge.HMI.Models;

public sealed class PumpStatus
{
    public bool Running { get; set; }
    public bool Faulted { get; set; }
    public bool Enabled { get; set; }
    public double Speed { get; set; }
    public double Current { get; set; }
}
