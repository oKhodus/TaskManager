# Task & Sprint Manager

A mini-Jira task management application built with C# and Avalonia UI, following Clean Architecture principles.

## 📋 Overview

Task & Sprint Manager is a desktop application for managing projects, tasks, sprints, and team collaboration. It provides a Jira-like experience with support for different task types (Bugs and Features), sprint planning, user assignments, and tagging.

## 🏗️ Architecture

The project follows **Clean Architecture** with clear separation of concerns across four main layers:

```
TaskManager/
├── App.UI/              # Presentation Layer (Avalonia UI)
├── App.Application/     # Application Layer (Use Cases, DTOs, Services)
├── App.Domain/          # Domain Layer (Entities, Enums, Business Rules)
└── App.Infrastructure/  # Infrastructure Layer (Persistence, External Services)
```

### Layer Responsibilities

- **App.UI**: Avalonia-based user interface, ViewModels, Views
- **App.Application**: Application services, use cases, DTOs, business logic orchestration
- **App.Domain**: Core business entities, enums, domain rules (no dependencies)
- **App.Infrastructure**: EF Core DbContext, repositories, data access, external integrations

## 🛠️ Technology Stack

- **.NET 9.0** - Latest .NET framework
- **Avalonia UI 11.3.9** - Cross-platform UI framework
- **Entity Framework Core 9.0** - ORM for data access
- **SQLite** - Database (can be easily switched to SQL Server/PostgreSQL)
- **CommunityToolkit.Mvvm 8.4.0** - MVVM toolkit

## 📦 Project Structure

```
App.Domain/
├── Entities/           # Domain entities (TaskBase, BugTask, FeatureTask, Project, Sprint, User, Tag)
├── Enums/              # Domain enums (TaskStatus, TaskPriority, TaskType)
├── ValueObjects/       # Value objects (future)
├── Aggregates/         # Aggregate roots (future)
└── Rules/              # Business rules (future)

App.Application/
├── UseCases/           # Application use cases
├── Services/           # Application services
├── DTO/                # Data Transfer Objects
└── Interfaces/         # Application interfaces

App.Infrastructure/
├── Persistence/        # EF Core DbContext and configurations
│   ├── ApplicationDbContext.cs
│   ├── Configurations/ # Entity configurations
│   └── DataSeeder.cs   # Sample data seeder
├── Repositories/       # Repository implementations
├── Logging/            # Logging infrastructure
└── Files/              # File handling

App.UI/
├── Views/              # Avalonia views (.axaml)
├── ViewModels/         # MVVM view models
├── Resources/          # UI resources (styles, images)
└── DI.cs               # Dependency injection setup
```

## 🗄️ Domain Model

### Entities

#### TaskBase (Abstract)
Base class for all task types with common properties:
- `Id`, `Title`, `Description`
- `Status` (ToDo, InProgress, InReview, Done, Blocked, Cancelled)
- `Priority` (Low, Medium, High, Critical)
- `CreatedAt`, `UpdatedAt`, `DueDate`
- Relationships: Project, Sprint, CreatedBy, AssignedTo, Tags

#### BugTask : TaskBase
Bug-specific properties:
- `StepsToReproduce`
- `ExpectedBehavior`, `ActualBehavior`
- `Environment`, `Severity`

#### FeatureTask : TaskBase
Feature-specific properties:
- `AcceptanceCriteria`
- `StoryPoints`
- `Epic`

#### Other Entities
- **Project**: Projects containing tasks and sprints
- **Sprint**: Time-boxed iterations for task completion
- **User**: Team members who create and are assigned tasks
- **Tag**: Labels for categorizing tasks (N-N relationship)

### Relationships

- **1-N**: Project → Tasks, Project → Sprints
- **1-N**: Sprint → Tasks (optional)
- **1-N**: User → Created Tasks, User → Assigned Tasks
- **N-N**: Tasks ↔ Tags (via join table)

### Inheritance Strategy

**Table Per Hierarchy (TPH)** is used for task inheritance:
- All task types stored in a single `Tasks` table
- Discriminator column `TaskType` distinguishes BugTask vs FeatureTask
- Type-specific columns are nullable

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/oKhodus/TaskManager.git
   cd TaskManager
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   cd App.UI
   dotnet run
   ```

### Database Setup

The application uses SQLite by default. The database will be created automatically on first run.

To seed sample data, call `DataSeeder.SeedData()` in your application startup:

```csharp
using App.Infrastructure.Persistence;

var context = // ... get your DbContext
DataSeeder.SeedData(context);
```

## 📊 Sample Data

The `DataSeeder` includes:
- **3 Users**: admin, john.doe, jane.smith
- **2 Projects**: Website Redesign, Mobile App
- **4 Tags**: Frontend, Backend, UI/UX, Critical
- **2 Sprints**: Sprint 1 & 2 for Q1 2024
- **2 Bug Tasks**: Login button issue, Mobile menu overlap
- **3 Feature Tasks**: Dark mode, Task filtering, User profile page

## 🔧 Development

### Building

```bash
# Build all projects
dotnet build

# Build in Release mode
dotnet build --configuration Release
```

### Running Tests

```bash
# Run all tests (when test projects are added)
dotnet test
```

### Code Formatting

```bash
# Check formatting
dotnet format --verify-no-changes

# Auto-format code
dotnet format
```

## 🔄 CI/CD

GitHub Actions workflow (`.github/workflows/branch-policy.yml`) runs on:
- Pull requests
- Pushes to main, develop, feature/**, bugfix/**, hotfix/**

Workflow includes:
- Build verification
- Test execution
- Code formatting checks

## 📝 Current Status

### ✅ Completed (Step 0 & Step 1)

- [x] Solution and project structure
- [x] GitHub branch policy workflow
- [x] Domain entities with inheritance (TaskBase → BugTask/FeatureTask)
- [x] EF Core configuration with TPH strategy
- [x] Entity relationships (1-N, N-N)
- [x] Data seeding with sample data
- [x] Base folder structure

### 🚧 Next Steps

- [ ] Application layer (Use Cases, DTOs, Services)
- [ ] Repository pattern implementation
- [ ] Dependency injection setup
- [ ] Avalonia UI views and ViewModels
- [ ] Task CRUD operations
- [ ] Sprint management
- [ ] User authentication
- [ ] Search and filtering
- [ ] Unit tests

## 🤝 Contributing

1. Create a feature branch (`git checkout -b feature/amazing-feature`)
2. Commit your changes (`git commit -m 'feat: Add amazing feature'`)
3. Push to the branch (`git push origin feature/amazing-feature`)
4. Open a Pull Request

### Branch Naming Convention

- `feature/` - New features
- `bugfix/` - Bug fixes
- `hotfix/` - Critical production fixes

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👥 Authors

- **Your Name** - *Initial work*

## 🙏 Acknowledgments

- Inspired by Jira's task management workflow
- Built with [Avalonia UI](https://avaloniaui.net/)
- Uses [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

---

**Note**: This is a work-in-progress project. More features and documentation will be added as development continues.

