using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
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

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            Log.Information("Starting TaskManager application...");

            // Setup DI
            var services = new ServiceCollection();
            DI.RegisterServices(services, configuration);
            ServiceProvider = services.BuildServiceProvider();

            // Initialize and seed database
            using (var scope = ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<global::App.Infrastructure.Persistence.ApplicationDbContext>();
                Log.Information("Ensuring database is created...");
                dbContext.Database.EnsureCreated();

                // Seed data if database is empty
                if (!dbContext.Users.Any())
                {
                    Log.Information("Database is empty, seeding initial data...");
                    global::App.Infrastructure.Persistence.DataSeeder.SeedData(dbContext);
                    Log.Information("Database seeding completed");
                }
                else
                {
                    Log.Information("Database already contains data, skipping seeding");
                }
            }

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindowViewModel = ServiceProvider.GetRequiredService<ViewModels.MainWindowViewModel>();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };
                Log.Information("Application initialized successfully");

                // Subscribe to application shutdown to properly close Serilog
                desktop.ShutdownRequested += (s, e) =>
                {
                    Log.Information("Application shutting down...");
                    Log.CloseAndFlush();
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            throw;
        }
    }
}