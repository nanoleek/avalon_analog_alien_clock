using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AlienClockAvalonia.ViewModels;
using AlienClockAvalonia.Views;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Splat.Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using LiteDB;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Splat;
using ReactiveUI;

namespace AlienClockAvalonia;

public partial class App : Application
{
    public IServiceProvider? Container { get; private set; }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        using var host = Host.CreateDefaultBuilder().ConfigureServices(services =>
        {
            services.UseMicrosoftDependencyResolver();
            var resolver = Locator.CurrentMutable;
            resolver.InitializeSplat();
            resolver.InitializeReactiveUI();
            services.AddOptions();
            services.AddOptions<LiteDatabaseServiceOptions>()
                .Configure<IServiceProvider>((options, provider) =>
                {
                    options.ConnectionString = new ConnectionString("./alien_clock.db");
                    options.Mapper = BsonMapper.Global;
                });
            services.TryAddTransient<LiteDbFactory>();
            services.TryAddSingleton<LiteDatabase>(sp =>
            {
                var factory = sp.GetRequiredService<LiteDbFactory>();
                return factory.Create();
            });
            services.AddSingleton<IAlarmService, AlarmService>();
            services.AddSingleton<IClockService, ClockService>();
            services.AddTransient<MainWindow>();
            services.AddTransient<MainWindowViewModel>();

        }).Build();
        Container = host.Services;
        Container.UseMicrosoftDependencyResolver();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Locator.Current.GetService<MainWindow>();
        }


        base.OnFrameworkInitializationCompleted();

    }
}