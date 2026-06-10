namespace RgAi.Backend.Models;

public class ServerMetricsDto
{
    public double CpuUsagePercentage { get; set; }
    public long RamUsedMb { get; set; }
    public int GpuLoadPercentage { get; set; }
    public int GpuMemoryUsedMb { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}