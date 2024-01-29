using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.RegularExpressions;
using AlienClock;
using Avalonia.Data;
using ReactiveUI;

namespace AlienClockAvalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string? _alienTimeString = "";
    [AlienTimeValidation]
    public string? AlienTimeString
    {
        get => _alienTimeString;
        set
        {
            this.RaiseAndSetIfChanged(ref _alienTimeString, value);
        }
    }

    private string? _alienDateString = "";
    [AlienDateValidation]
    public string? AlienDateString
    {
        get => _alienDateString;
        set
        {
            this.RaiseAndSetIfChanged(ref _alienDateString, value);
        }
    }

    private AlienDateTime _dateTime = AlienDateTime.Now;

    [Required]
    public AlienDateTime AlienDateTime
    {
        get => _dateTime;
        set
        {
            this.RaiseAndSetIfChanged(ref _dateTime, value);
        }
    }
    public ObservableDictionary<int, AlienDateTime> Alarms { get; set; } = new();

}

internal class AlienTimeValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        try
        {
            if (value is string time)
            {
                if (string.IsNullOrEmpty(time))
                {
                    return ValidationResult.Success;
                }
                var match = Regex.Match(time, "(\\d{1,2}):(\\d{1,2})");
                if (!match.Success)
                {
                    return new ValidationResult($"please input correct alien time");
                }
                var hour = int.Parse(match.Groups[1].Value);
                var minute = int.Parse(match.Groups[2].Value);
                if (hour < 0 || hour > 36)
                {
                    throw new ArgumentOutOfRangeException("error hour");
                }
                if (minute < 0 || minute > 90)
                {
                    throw new ArgumentOutOfRangeException("error minute");
                }
                return ValidationResult.Success;
            }

            return ValidationResult.Success;

        }
        catch (ArgumentOutOfRangeException ex)
        {
            return new ValidationResult(ex.Message);
        }
    }
}

internal class AlienDateValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        try
        {
            if (value is string date)
            {
                if (string.IsNullOrEmpty(date))
                {
                    return ValidationResult.Success;
                }
                var match = Regex.Match(date, "(^\\d+)-(\\d{1,2})-(\\d{1,2})");
                if (!match.Success)
                {
                    return new ValidationResult($"please input correct alien date");
                }
                var year = int.Parse(match.Groups[1].Value);
                var month = int.Parse(match.Groups[2].Value);
                var day = int.Parse(match.Groups[3].Value);
                var alienDatetime = new AlienDateTime(year, month, day, 0, 0, 0);
                return ValidationResult.Success;
            }

            return ValidationResult.Success;

        }
        catch (ArgumentOutOfRangeException ex)
        {
            return new ValidationResult(ex.Message);
        }
    }

}