using System.Diagnostics;
using System.Runtime.InteropServices;
using Prometheus;

namespace Mentalist.Monitoring.Memory;

public class MetricsHostedService: IHostedService
{
    private readonly Gauge _freeMemoryBytes =
        Metrics.CreateGauge(
            "node_os_physical_memory_free_bytes",
            "Free OS memory"
        );

    private readonly Gauge _usedMemoryBytes =
        Metrics.CreateGauge(
            "node_os_physical_memory_used_bytes",
            "Used OS memory"
        );

    private readonly Gauge _totalMemoryBytes =
        Metrics.CreateGauge(
            "node_os_physical_memory_bytes",
            "Total OS memory"
        );

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Factory.StartNew(
            () => Monitor(cancellationToken),
            CancellationToken.None,
            TaskCreationOptions.DenyChildAttach,
            TaskScheduler.Current
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task Monitor(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                MetricsHostedServiceState.Ping();

                var metrics = GetMetrics();

                _freeMemoryBytes.Set(metrics.Free);
                _usedMemoryBytes.Set(metrics.Used);
                _totalMemoryBytes.Set(metrics.Total);

                await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public MemoryMetrics GetMetrics()
    {
        if (IsUnix())
        {
            return GetUnixMetrics();
        }

        return GetWindowsMetrics();
    }

    private bool IsUnix()
    {
        var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        return isUnix;
    }

    private MemoryMetrics GetUnixMetrics()
    {
        var output = "";

        var info = new ProcessStartInfo("free -m")
        {
            FileName = "/bin/bash",
            Arguments = "-c \"free -m\"",
            RedirectStandardOutput = true
        };

        using (var process = Process.Start(info))
        {
            output = process?.StandardOutput.ReadToEnd() ?? "";
        }

        var lines = output.Split("\n");
        var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

        var metrics = new MemoryMetrics
        {
            Total = double.Parse(memory[1]),
            Used = double.Parse(memory[2]),
            Free = double.Parse(memory[3])
        };

        return metrics;
    }

    private MemoryMetrics GetWindowsMetrics()
    {
        var output = "";

        var info = new ProcessStartInfo
        {
            FileName = "wmic",
            Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value",
            RedirectStandardOutput = true
        };

        using (var process = Process.Start(info))
        {
            output = process?.StandardOutput.ReadToEnd() ?? "";
        }

        var lines = output.Trim().Split("\n");
        var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
        var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

        var metrics = new MemoryMetrics
        {
            Total = Math.Round(double.Parse(totalMemoryParts[1]) * 1024, 0),
            Free = Math.Round(double.Parse(freeMemoryParts[1]) * 1024, 1)
        };
        metrics.Used = metrics.Total - metrics.Free;

        return metrics;
    }

    public class MemoryMetrics
    {
        public double Total;
        public double Used;
        public double Free;
    }
}