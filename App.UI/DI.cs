using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Application.Services;
using App.Infrastructure.Persistence;
using App.Infrastructure.Repositories;
using App.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace App.UI;

public static class DI
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Serilog
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: false); // We'll dispose manually in App.axaml.cs
        });

        // Register DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));

        // Register generic repository
        services.AddScoped(typeof(IRepository<>), typeof(RepositoryBase<>));

        // Register specific repositories
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ISprintRepository, SprintRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        // Register business services
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IExportService, CsvExportService>();
        services.AddScoped<IKanbanService, KanbanService>();

        // Register authentication services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<ICurrentUserService, CurrentUserService>();

        // Register user management services
        services.AddScoped<IUserManagementService, UserManagementService>();

        // Register ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProjectMasterViewModel>();
        services.AddTransient<ProjectDetailViewModel>();
        services.AddTransient<TaskMasterViewModel>();
        services.AddTransient<TaskDetailViewModel>();
        services.AddTransient<CreateUserViewModel>();
        services.AddTransient<KanbanBoardViewModel>();
        services.AddTransient<CreateTaskViewModel>();
    }
}
