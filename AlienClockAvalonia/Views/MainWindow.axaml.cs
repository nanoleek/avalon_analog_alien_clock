using AlienClock;
using AlienClockAvalonia.Models;
using AlienClockAvalonia.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using MsBox.Avalonia;
using Splat;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AlienClockAvalonia.Views;

public partial class MainWindow : Window
{
    private static readonly int userId = 1;
    private const double _flatCornerDegrees = 180;
    private const int _clockMinutes = 90;
    private const double _clockBorderTickness = 5;
    private const double _hourTickness = 5;
    private const double _minuteTickness = 3;

    private const double _handHoursTickness = 5;
    private const double _handMinutesTickness = 3;
    private const double _handSecondsTickness = 2;

    private const double _degreesPerSecond = 4;
    private const double _degreesPerMinute = 4;
    private const double _degreesPerHour = 20;
    private const int _clockFreqUpdatesMilliseconds = 500;

    private const double _secondsPerMinute = 90;
    private const double _minutesPerHour = 90;

    private Canvas _canvas;
    private Ellipse _clockBorder;
    private Line[] _clockMarkers;
    private Line _handHours;
    private Line _handMinutes;
    private Line _handSeconds;

    // This will be used to trigger clock refresh
    private DispatcherTimer _timer;
    private readonly IAlarmService _alarm;
    private readonly IClockService _clock;
    private DateTime _lastTriggerTime;
    public MainWindow()
    {

    }

    public MainWindow(MainWindowViewModel viewModel, IAlarmService alarm, IClockService clock)
    {
        this.DataContext = viewModel;
        _alarm = alarm;
        _clock = clock;
        var config = _clock.Get(userId);
        if (config != null)
        {
            viewModel.AlienDateTime = new AlienDateTime(config.AlienTimestamp + (long)(DateTime.Now - config.SettingTime).TotalMilliseconds / 500);
        }
        else
        {
            viewModel.AlienDateTime = AlienDateTime.Now;
        }
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private double GetClockRadius()
    {
        return (ClientSize.Width > ClientSize.Height ? ClientSize.Height : ClientSize.Width) / 2;
    }

    private double GetClockSize()
    {
        return ClientSize.Width > ClientSize.Height ? ClientSize.Height : ClientSize.Width;
    }

    private Point GetClockCenteredPosition()
    {
        return new Point((ClientSize.Width - GetClockSize()) / 2, (ClientSize.Height - GetClockSize()) / 2);
    }

    private Point GetNewOrigin()
    {
        return new Point(ClientSize.Width / 2, ClientSize.Height / 2);
    }


    private Point TranslatePoint(Point pt, Point newOrigin)
    {
        return new Point(newOrigin.X + pt.X, newOrigin.Y - pt.Y);
    }

    private Point GetMarkerPosition(double angle, double radius)
    {
        // Returns the coordinates of the point on the circumference
        // given an angle and the radius
        double angleRad = Math.PI / _flatCornerDegrees * angle;
        return new Point(radius * Math.Sin(angleRad), radius * Math.Cos(angleRad));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        // Create shapes, which will be repositioned in DrawClock
        _canvas = this.FindControl<Canvas>("ClockContainer")!;

        _clockBorder = new()
        {
            Fill = Brushes.Transparent,
            Stroke = Brushes.Black,
            StrokeThickness = _clockBorderTickness
        };
        _canvas.Children.Add(_clockBorder);

        // Create clock markers,
        // Hour markers are thicker than minute markers
        _clockMarkers = new Line[_clockMinutes];
        for (int currentMinute = 0; currentMinute < _clockMinutes; currentMinute++)
        {
            Line newMarker = new()
            {
                Fill = Brushes.Black,
                Stroke = Brushes.Black
            };

            if (currentMinute % 5 == 0)
                newMarker.StrokeThickness = _hourTickness;      // Hour (a thicker line "every 5 minutes")
            else
                newMarker.StrokeThickness = _minuteTickness;    // Minute

            _clockMarkers[currentMinute] = newMarker;
            _canvas.Children.Add(newMarker);
        }

        // Create clock hands
        _handHours = new()
        {
            Fill = Brushes.Black,
            Stroke = Brushes.Black,
            StrokeThickness = _handHoursTickness
        };
        _canvas.Children.Add(_handHours);

        _handMinutes = new()
        {
            Fill = Brushes.Black,
            Stroke = Brushes.Black,
            StrokeThickness = _handMinutesTickness
        };
        _canvas.Children.Add(_handMinutes);

        _handSeconds = new()
        {
            Fill = Brushes.Red,
            Stroke = Brushes.Red,
            StrokeThickness = _handSecondsTickness
        };
        _canvas.Children.Add(_handSeconds);

        // Get notified when window size changes, so we can redraw the clock
        IObservable<double> obsWidth = this.GetObservable(WidthProperty);
        obsWidth.Subscribe((x) => ReDrawClock());

        IObservable<double> obsHeight = this.GetObservable(HeightProperty);
        obsHeight.Subscribe((x) => ReDrawClock());

        IObservable<WindowState> obsWindowState = this.GetObservable(WindowStateProperty);
        obsWindowState.Subscribe((x) => ReDrawClock());

        // Start the timer
        _timer = new(TimeSpan.FromMilliseconds(_clockFreqUpdatesMilliseconds), DispatcherPriority.Render, (s, e) => ReDrawClock());

        _timer.Start();
    }

    private void ReDrawClock()
    {
        double clockRadius = GetClockRadius();
        double clockSize = GetClockSize();

        // Reposition and resize previously created shapes

        // Clock border
        _clockBorder.Width = clockSize;
        _clockBorder.Height = clockSize;

        Point pointClockPos = GetClockCenteredPosition();

        Canvas.SetLeft(_clockBorder, pointClockPos.X);
        Canvas.SetTop(_clockBorder, pointClockPos.Y);

        // Reposition clock markers, use the center of the window as new axis origin.
        // We assume that the y axis grows upward (see TranslatePoint).
        Point newOrigin = GetNewOrigin();

        for (int i = 0; i < _clockMinutes; i++)
        {
            Line marker = _clockMarkers[i];
            double hourMarkerLength = clockRadius - 25;
            double minuteMarkerLength = clockRadius - 10;
            Point startPos = TranslatePoint(GetMarkerPosition(i * _degreesPerMinute, clockRadius), newOrigin);

            Point endPos;
            if (i % 5 == 0)
                endPos = TranslatePoint(GetMarkerPosition(i * _degreesPerMinute, hourMarkerLength), newOrigin);
            else
                endPos = TranslatePoint(GetMarkerPosition(i * _degreesPerMinute, minuteMarkerLength), newOrigin);

            marker.StartPoint = startPos;
            marker.EndPoint = endPos;
        }
        var vm = this.DataContext as MainWindowViewModel;
        vm!.AlienDateTime = new AlienDateTime(vm.AlienDateTime.AlienTimeSeconds + ((long)(DateTime.Now - _lastTriggerTime).TotalMilliseconds / 500));
        _lastTriggerTime = DateTime.Now;
        ReRrawHands(vm!.AlienDateTime);
    }

    private void ReRrawHands(AlienDateTime now)
    {
        double clockRadius = GetClockRadius();
        Point newOrigin = GetNewOrigin();

        double secondDegrees = now.Second * _degreesPerSecond;
        double handSecondsLength1 = clockRadius * 0.8;
        double handSecondsLength2 = 40;
        Point startPointSec = TranslatePoint(GetMarkerPosition(secondDegrees, handSecondsLength1), newOrigin);
        Point endPointSec = TranslatePoint(GetMarkerPosition((secondDegrees + _flatCornerDegrees), handSecondsLength2), newOrigin);
        _handSeconds.StartPoint = startPointSec;
        _handSeconds.EndPoint = endPointSec;

        double minuteDegrees = now.Minute * _degreesPerMinute;
        double handMinutesLength1 = clockRadius * 0.8;
        double handMinutesLength2 = 40;
        Point startPointMin = TranslatePoint(GetMarkerPosition((minuteDegrees + (_degreesPerMinute * now.Second / _secondsPerMinute)), handMinutesLength1), newOrigin);
        Point endPointMin = TranslatePoint(GetMarkerPosition((minuteDegrees + (_degreesPerMinute * now.Second / _secondsPerMinute) + _flatCornerDegrees), handMinutesLength2), newOrigin);
        _handMinutes.StartPoint = startPointMin;
        _handMinutes.EndPoint = endPointMin;

        double hourDegrees = now.Hour * _degreesPerHour;
        double handHoursLength1 = clockRadius * 0.6;
        double handHoursLength2 = 30;
        Point startPointHour = TranslatePoint(GetMarkerPosition((hourDegrees + (_degreesPerHour * now.Minute / _minutesPerHour)), handHoursLength1), newOrigin);
        Point endPointHour = TranslatePoint(GetMarkerPosition((hourDegrees + (_degreesPerHour * now.Minute / _minutesPerHour) + _flatCornerDegrees), handHoursLength2), newOrigin);
        _handHours.StartPoint = startPointHour;
        _handHours.EndPoint = endPointHour;
    }
    public void Button_Click(object? sender, RoutedEventArgs args)
    {
        try
        {
            var vm = this.DataContext as MainWindowViewModel;
            var year = vm.AlienDateTime.Year;
            var month = vm.AlienDateTime.Month;
            var day = vm.AlienDateTime.Day;
            var hour = vm.AlienDateTime.Hour;
            var minute = vm.AlienDateTime.Minute;
            var second = vm.AlienDateTime.Second;
            if (!string.IsNullOrWhiteSpace(vm.AlienDateString))
            {
                var ymd = vm.AlienDateString.Split("-");
                year = int.Parse(ymd[0]);
                month = int.Parse(ymd[1]);
                day = int.Parse(ymd[2]);

            }
            if (!string.IsNullOrWhiteSpace(vm.AlienTimeString))
            {
                var hm = vm.AlienTimeString.Split(":");
                hour = int.Parse(hm[0]);
                minute = int.Parse(hm[1]);
            }

            vm.AlienDateTime = new AlienDateTime(year, month, day, hour, minute, second);
        }
        catch (Exception)
        {
            //ignore
        }
    }
    public void ResetButton_Click(object? sender, RoutedEventArgs args)
    {
        try
        {
            var vm = this.DataContext as MainWindowViewModel;
            vm.AlienDateTime = AlienDateTime.Now;
        }
        catch (Exception)
        {
            //ignore
        }
    }

    public void AddAlarm(object? sender, RoutedEventArgs args)
    {
        try
        {
            var vm = this.DataContext as MainWindowViewModel;
            vm.AlienDateTime = AlienDateTime.Now;
        }
        catch (Exception)
        {
            //ignore
        }
    }
    public void RemoveAlarm(object? sender, RoutedEventArgs args)
    {
        // _alarm.Remove(id);
        // Alarms.Remove(id);
    }
}