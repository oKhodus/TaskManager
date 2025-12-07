using App.Domain.Entities;
using App.Domain.Enums;
using TaskStatus = App.Domain.Enums.TaskStatus;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence;

public static class DataSeeder
{
    public static void SeedData(ApplicationDbContext context)
    {
        // Ensure database is created
        context.Database.EnsureCreated();

        // Check if data already exists
        if (context.Users.Any() || context.Projects.Any())
        {
            return; // Data already seeded
        }

        // Seed Users
        var users = new List<User>
        {
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Username = "admin",
                Email = "admin@taskmanager.com",
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Username = "john.doe",
                Email = "john.doe@taskmanager.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Username = "jane.smith",
                Email = "jane.smith@taskmanager.com",
                FirstName = "Jane",
                LastName = "Smith",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            }
        };

        context.Users.AddRange(users);
        context.SaveChanges();

        // Seed Projects
        var projects = new List<Project>
        {
            new Project
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "Website Redesign",
                Description = "Complete redesign of the company website",
                Key = "WR",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Project
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Name = "Mobile App",
                Description = "Development of mobile application",
                Key = "MA",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };

        context.Projects.AddRange(projects);
        context.SaveChanges();

        // Seed Tags
        var tags = new List<Tag>
        {
            new Tag
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                Name = "Frontend",
                Color = "#3B82F6",
                CreatedAt = DateTime.UtcNow.AddDays(-18)
            },
            new Tag
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                Name = "Backend",
                Color = "#10B981",
                CreatedAt = DateTime.UtcNow.AddDays(-18)
            },
            new Tag
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
                Name = "UI/UX",
                Color = "#F59E0B",
                CreatedAt = DateTime.UtcNow.AddDays(-18)
            },
            new Tag
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000004"),
                Name = "Critical",
                Color = "#EF4444",
                CreatedAt = DateTime.UtcNow.AddDays(-18)
            }
        };

        context.Tags.AddRange(tags);
        context.SaveChanges();

        // Seed Sprints
        var sprints = new List<Sprint>
        {
            new Sprint
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                Name = "Sprint 1 - Q1 2024",
                Description = "First sprint of Q1",
                ProjectId = projects[0].Id,
                StartDate = DateTime.UtcNow.AddDays(-14),
                EndDate = DateTime.UtcNow.AddDays(0),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-14)
            },
            new Sprint
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
                Name = "Sprint 2 - Q1 2024",
                Description = "Second sprint of Q1",
                ProjectId = projects[0].Id,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(14),
                IsActive = false,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            }
        };

        context.Sprints.AddRange(sprints);
        context.SaveChanges();

        // Seed Bug Tasks
        var bugTasks = new List<BugTask>
        {
            new BugTask
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                Title = "Login button not responding",
                Description = "Users report that the login button does nothing when clicked",
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                ProjectId = projects[0].Id,
                SprintId = sprints[0].Id,
                CreatedById = users[1].Id,
                AssignedToId = users[2].Id,
                StepsToReproduce = "1. Navigate to login page\n2. Enter credentials\n3. Click login button",
                ExpectedBehavior = "User should be redirected to dashboard",
                ActualBehavior = "Nothing happens, button appears unresponsive",
                Environment = "Chrome 120, Windows 11",
                Severity = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                DueDate = DateTime.UtcNow.AddDays(2)
            },
            new BugTask
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
                Title = "Mobile menu overlaps content",
                Description = "On mobile devices, the navigation menu overlaps page content",
                Status = TaskStatus.ToDo,
                Priority = TaskPriority.Medium,
                ProjectId = projects[0].Id,
                SprintId = sprints[1].Id,
                CreatedById = users[0].Id,
                AssignedToId = users[1].Id,
                StepsToReproduce = "1. Open website on mobile device\n2. Tap menu icon\n3. Observe overlap",
                ExpectedBehavior = "Menu should slide in without overlapping content",
                ActualBehavior = "Menu overlaps main content area",
                Environment = "iOS Safari, Android Chrome",
                Severity = "Medium",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                DueDate = DateTime.UtcNow.AddDays(10)
            }
        };

        context.BugTasks.AddRange(bugTasks);
        context.SaveChanges();

        // Seed Feature Tasks
        var featureTasks = new List<FeatureTask>
        {
            new FeatureTask
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                Title = "Implement dark mode",
                Description = "Add dark mode theme toggle to the application",
                Status = TaskStatus.InReview,
                Priority = TaskPriority.Medium,
                ProjectId = projects[0].Id,
                SprintId = sprints[0].Id,
                CreatedById = users[0].Id,
                AssignedToId = users[2].Id,
                AcceptanceCriteria = "1. Toggle switch in settings\n2. Theme persists across sessions\n3. All pages support dark mode\n4. Smooth transition animation",
                StoryPoints = 5,
                Epic = "User Experience",
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                DueDate = DateTime.UtcNow.AddDays(-2)
            },
            new FeatureTask
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000002"),
                Title = "Add task filtering",
                Description = "Allow users to filter tasks by status, priority, and assignee",
                Status = TaskStatus.ToDo,
                Priority = TaskPriority.Low,
                ProjectId = projects[0].Id,
                SprintId = sprints[1].Id,
                CreatedById = users[1].Id,
                AssignedToId = users[1].Id,
                AcceptanceCriteria = "1. Filter by status dropdown\n2. Filter by priority dropdown\n3. Filter by assignee dropdown\n4. Multiple filters can be applied simultaneously",
                StoryPoints = 3,
                Epic = "Task Management",
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                DueDate = DateTime.UtcNow.AddDays(12)
            },
            new FeatureTask
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000003"),
                Title = "User profile page",
                Description = "Create user profile page with avatar, bio, and activity history",
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                ProjectId = projects[1].Id,
                CreatedById = users[0].Id,
                AssignedToId = users[2].Id,
                AcceptanceCriteria = "1. Display user avatar\n2. Show user bio\n3. Display task activity history\n4. Allow editing own profile",
                StoryPoints = 8,
                Epic = "User Management",
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                DueDate = DateTime.UtcNow.AddDays(5)
            }
        };

        context.FeatureTasks.AddRange(featureTasks);
        context.SaveChanges();

        // Associate tags with tasks
        var taskTagAssociations = new[]
        {
            new { TaskId = bugTasks[0].Id, TagId = tags[0].Id }, // Login button bug - Frontend
            new { TaskId = bugTasks[0].Id, TagId = tags[3].Id }, // Login button bug - Critical
            new { TaskId = bugTasks[1].Id, TagId = tags[0].Id }, // Mobile menu bug - Frontend
            new { TaskId = bugTasks[1].Id, TagId = tags[2].Id }, // Mobile menu bug - UI/UX
            new { TaskId = featureTasks[0].Id, TagId = tags[0].Id }, // Dark mode - Frontend
            new { TaskId = featureTasks[0].Id, TagId = tags[2].Id }, // Dark mode - UI/UX
            new { TaskId = featureTasks[1].Id, TagId = tags[0].Id }, // Task filtering - Frontend
            new { TaskId = featureTasks[2].Id, TagId = tags[1].Id }, // User profile - Backend
            new { TaskId = featureTasks[2].Id, TagId = tags[0].Id }  // User profile - Frontend
        };

        foreach (var association in taskTagAssociations)
        {
            var task = context.Tasks.First(t => t.Id == association.TaskId);
            var tag = context.Tags.First(t => t.Id == association.TagId);
            task.Tags.Add(tag);
        }

        context.SaveChanges();
    }
}

