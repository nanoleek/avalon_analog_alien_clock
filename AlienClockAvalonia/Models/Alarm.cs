using System;
using Avalonia;

namespace AlienClockAvalonia.Models;

public class Alarm
{
    public int Id { get; set; }
    public string? Message { get; set; }
    public long AlienTimestamp { get; set; }
    public bool IsActive { get; set; }
    public AlarmType AlarmType { get; set; }
}

public enum AlarmType
{
    Once,
    Daily
}

public class ClockConfig
{
    public int Id { get; set; }
    public long AlienTimestamp { get; set; } 
    public DateTime SettingTime { get; set; } // to calculate the offset time
}