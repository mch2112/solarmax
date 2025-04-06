using System.Runtime.InteropServices;

namespace SolarMax;

public sealed class Clock
{
    [DllImport("kernel32.dll")]
    static extern int QueryPerformanceFrequency(ref long x);
    [DllImport("kernel32.dll")]
    static extern int QueryPerformanceCounter(ref long x);

    private readonly double ticksPerMS;
    private long ticks;
    private long tickBase;
    private bool paused = false;
    private long ticksAtPause = 0;
    public Clock()
    {
        long longTicksPerMS = 0;
        QueryPerformanceFrequency(ref longTicksPerMS);
        ticksPerMS = longTicksPerMS;
        tickBase = 0;
        Reset();
    }
    
    public double Seconds
    {
        get
        {
            if (paused)
                return (ticksAtPause - tickBase) / ticksPerMS;
            
            QueryPerformanceCounter(ref ticks);
            return (ticks - tickBase) / ticksPerMS;
        }
    }
    public bool Paused
    {
        get => paused;
        set
        {
            if (paused != value)
            {
                paused = value;
                if (paused)
                    ticksAtPause = this.Ticks;
                else
                    tickBase += this.Ticks - ticksAtPause;
            }
        }
    }
    public void Reset()
    {
        tickBase = Ticks;
        ticksAtPause = tickBase;
    }
    private long Ticks
    {
        get
        {
            QueryPerformanceCounter(ref ticks);
            return ticks;
        }
    }
}
