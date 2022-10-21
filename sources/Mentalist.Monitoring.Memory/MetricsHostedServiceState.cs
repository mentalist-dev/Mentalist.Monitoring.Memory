namespace Mentalist.Monitoring.Memory;

public static class MetricsHostedServiceState
{
    private static DateTime _lastPing = DateTime.UtcNow;

    public static void Ping()
    {
        _lastPing = DateTime.UtcNow;
    }

    public static bool IsAlive()
    {
        return DateTime.UtcNow - _lastPing < TimeSpan.FromMinutes(1);
    }
}