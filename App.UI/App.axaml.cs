using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using AvaloniaApplication = Avalonia.Application;

namespace App.UI;

public partial class App : AvaloniaApplication
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Build configuration
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Setup DI
        var services = new ServiceCollection();
        DI.RegisterServices(services, configuration);
        ServiceProvider = services.BuildServiceProvider();

        // Initialize and seed database
        using (var scope = ServiceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<global::App.Infrastructure.Persistence.ApplicationDbContext>();
            dbContext.Database.EnsureCreated();

            // Seed data if database is empty
            if (!dbContext.Users.Any())
            {
                global::App.Infrastructure.Persistence.DataSeeder.SeedData(dbContext);
            }
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindowViewModel = ServiceProvider.GetRequiredService<ViewModels.MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}