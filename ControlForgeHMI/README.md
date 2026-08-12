# ControlForge HMI

A self-contained C#/.NET 8 WPF HMI practice solution built around MVVM, equipment models, asynchronous PLC-style I/O, command binding, and a simulated PLC client.

## Open in Visual Studio

1. Extract the entire `ControlForgeHMI` folder. Do not move the `.sln` away from the `src` folder.
2. Open `ControlForgeHMI.sln` in Visual Studio 2022.
3. Make sure the **.NET desktop development** workload and **.NET 8 SDK** are installed.
4. In Solution Explorer, `ControlForge.HMI` should appear under the solution automatically.
5. Set `ControlForge.HMI` as the startup project if Visual Studio does not do so automatically.
6. Press F5.

## Solution layout

- `ControlForgeHMI.sln`
- `src/ControlForge.HMI/ControlForge.HMI.csproj`
- `src/ControlForge.HMI/App.xaml`
- `src/ControlForge.HMI/MainWindow.xaml`
- `src/ControlForge.HMI/Models`
- `src/ControlForge.HMI/Services`
- `src/ControlForge.HMI/ViewModels`
- `src/ControlForge.HMI/Commands`

The project has no third-party NuGet dependencies. WPF is supplied by the .NET Windows Desktop framework.
