using System.Runtime.CompilerServices;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
namespace AlienClock;

public class AlienDateTime : INotifyPropertyChanged
{
    public int Second => (int)(AlienTimeSeconds % 90);
    public int Minute => (int)(AlienTimeSeconds / SecondsOfMinute % 90);
    public int Hour => (int)(AlienTimeSeconds / SecPerHour % 36);
    public int Day => GetDatePart(DatePartDay);
    public int Month => GetDatePart(DatePartMonth);
    public int Year => GetDatePart(DatePartYear);
    public long AlienTimeSeconds { get; private set; }

    public AlienDateTime(long timeSeconds)
    {
        this.AlienTimeSeconds = timeSeconds;
    }

    public AlienDateTime(int year, int month, int day, int hour, int minute, int second)
    {
        if (year < 1)
        {
            throw new ArgumentOutOfRangeException("error year");
        }
        if (month < 1 || month > 18)
        {
            throw new ArgumentOutOfRangeException("error month");
        }
        if (day < 1 || day > DaysInMonth[month - 1])
        {
            throw new ArgumentOutOfRangeException("error day");
        }
        if (hour < 0 || hour > 36)
        {
            throw new ArgumentOutOfRangeException("error hour");
        }
        if (minute < 0 || minute > 90)
        {
            throw new ArgumentOutOfRangeException("error minute");
        }
        if (second < 0 || second > 90)
        {
            throw new ArgumentOutOfRangeException("error second");
        }
        var alseconds = (year - 1) * DaysPerYear * SecPerDay
                                + DaysToMonth[month - 1] * SecPerDay
                                + (day - 1) * SecPerDay
                                + hour * SecPerHour
                                + minute * SecondsOfMinute
                                + second;
        this.AlienTimeSeconds = (long)alseconds;
    }

    public AlienDateTime()
    {
        var unixSecond = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        this.AlienTimeSeconds = BaseAlienSecond + (unixSecond / 500);
    }

    public AlienDateTime SetAlienDateTimeFromEarthDateTime(DateTime dateTime)
    {
        var unixSecond = new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        return new AlienDateTime(BaseAlienSecond + (unixSecond / 500));
    }
    public static AlienDateTime Now => new();

    public long AlienTimestamp => this.AlienTimeSeconds;

    public AlienDateTime Add(long second)
    {
        return new AlienDateTime(this.AlienTimeSeconds += second);
    }

    public DateTime? EarthDateTime
    {
        get
        {
            var unixTimeMilliseconds = (AlienTimestamp - BaseAlienSecond) * 500;
            if (unixTimeMilliseconds < -62135596800000 || unixTimeMilliseconds > 253402300799999)
            {
                return null;
            }
            return DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMilliseconds).DateTime;
        }
    }

    private int GetDatePart(int part)
    {
        var totalSeconds = this.AlienTimeSeconds;
        var totalDays = totalSeconds / SecPerDay;
        var year = totalDays / DaysPerYear;
        if (part == DatePartYear)
        {
            return (int)Math.Ceiling(year);
        }
        var days = totalDays - Math.Floor(year) * DaysPerYear;
        var m = 0;
        while (m < 18 && days >= DaysToMonth[m])
        {
            m++;
        }
        if (part == DatePartMonth)
        {
            return m;
        }
        var day = (int)days - DaysToMonth[m - 1] + 1;
        return day;
    }
    private const int DatePartYear = 0;
    private const int DatePartMonth = 1;
    private const int DatePartDay = 2;
    public static readonly int SecondsOfMinute = 90;
    public static readonly int MinutesOfHour = 90;
    public static readonly int HoursOfDay = 36;
    private static readonly double SecPerHour = 90 * 90;
    private static readonly double SecPerDay = 36 * 90 * 90;

    public static readonly int[] DaysInMonth = {
        44, 42, 48,  40,  48,  44,  40,  44,  42,  40,  40,  42,  44,  48,  42,  40,  44, 38
    };
    private static readonly int[] DaysToMonth =  {
        0,  44, 86, 134, 174, 222, 266, 306, 350, 392, 432, 472, 514, 558, 606, 648, 688, 732
    };
    private static readonly long BaseAlienSecond = 629585411668; //start at 2804-18-31 2:2:88

    private static readonly int DaysPerYear = 770;

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        return $"{Year}-{Month}-{Day} {Hour}:{Minute}:{Second}";
    }
}