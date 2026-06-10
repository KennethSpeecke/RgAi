using System.Diagnostics;

namespace backend.Workers;

public static class SystemResourceWatchdogWorker
{
    public static (int Load, int MemoryMb) GetNvidiaGpuUsage()
    {
        try
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "nvidia-smi",
                Arguments = "--query-gpu=utilization.gpu,memory.used --format=csv,noheader,nounits",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return (0, 0);
            }
            
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            var parts = output.Split(',');
            if (parts.Length >= 2 &&
                int.TryParse(parts[0], out int load) &&
                int.TryParse(parts[1], out int memoryMb))
            {
                return (load, memoryMb);
            }
        }
        catch (Exception e)
        {
            throw new Exception("Failed to retrieve NVIDIA GPU usage", e);
        }
        return (0, 0);
    }
}