# TaskManager

A cross-platform desktop task management application built with .NET 9.0 and Avalonia UI, following Clean Architecture and SOLID principles.

## 📋 Description

TaskManager is a Jira-inspired task management system designed for team collaboration, project tracking, and sprint planning. The application provides:

- **User Authentication** - Secure login with PBKDF2 password hashing
- **Role-Based Access Control** - Admin and Worker roles with different permissions
- **Project Management** - Create, edit, and organize projects with unique keys
- **Task Management** - Support for Bug and Feature task types with comprehensive properties
- **Kanban Board** - Drag-and-drop task management across multiple statuses
- **CSV Export** - Export projects and tasks for external analysis
- **Structured Logging** - Comprehensive logging with Serilog across all layers

## 🏗️ Architecture

```
TaskManager/
│
├── App.Domain/                    # Domain Layer (Core business entities, no dependencies)
│   ├── Entities/                  # Domain entities (Project, Task, User, Sprint, Tag)
│   └── Enums/                     # Domain enums (TaskStatus, Priority, UserRole)
│
├── App.Application/               # Application Layer (Business logic orchestration)
│   ├── Interfaces/                # Contracts for repositories and services
│   │   ├── Repositories/          # Repository interfaces
│   │   └── Services/              # Service interfaces
│   └── Services/                  # Business logic implementations
│
├── App.Infrastructure/            # Infrastructure Layer (Data access and external services)
│   ├── Persistence/               # Database context and configurations
│   │   ├── Configurations/        # Entity Framework entity configurations
│   │   └── Migrations/            # EF Core database migrations
│   └── Repositories/              # Repository pattern implementations
│
├── App.UI/                        # Presentation Layer (Avalonia UI)
│   ├── Views/                     # AXAML view files (Login, Dashboard, Kanban, etc.)
│   ├── ViewModels/                # MVVM ViewModels
│   ├── Resources/                 # UI resources (styles, themes, icons)
│   ├── Converters/                # Value converters for data binding
│   └── Windows/                   # Modal windows (CreateTask, CreateUser)
│
├── .github/
│   └── workflows/                 # GitHub Actions CI/CD workflows
│
├── logs/                          # Application log files (auto-generated)
│
├── Makefile                       # GNU Make build automation
├── TaskManager.sln                # Visual Studio solution file
├── .gitignore                     # Git ignore rules
└── README.md                      # Project documentation
```

**Clean Architecture Layers:**
```
┌─────────────────────────────────────┐
│  App.UI (Presentation)              │  ← Avalonia Views + ViewModels
├─────────────────────────────────────┤
│  App.Application (Business Logic)   │  ← Services, Interfaces, Use Cases
├─────────────────────────────────────┤
│  App.Infrastructure (Data Access)   │  ← Repositories, EF Core, DbContext
├─────────────────────────────────────┤
│  App.Domain (Core Domain)           │  ← Entities, Enums (NO dependencies)
└─────────────────────────────────────┘
```

**Key Architectural Patterns:**
- Repository Pattern (Generic + Specialized)
- Service Layer Pattern
- Dependency Injection (Microsoft.Extensions.DI)
- MVVM Pattern (CommunityToolkit.Mvvm)
- Table Per Hierarchy (TPH) for task inheritance
- Soft Delete Pattern for data retention

## ✨ Features

### Authentication & Authorization
- **Secure Login System**: PBKDF2 password hashing with salt
- **Role-Based Access Control**: Admin and Worker roles with different UI permissions
  - *Note: Current implementation uses hardcoded UI-level permissions. Future releases will include dynamic permission management.*
- **Session Management**: Persistent user sessions with CurrentUserService

### Project Management
- **Project CRUD Operations**: Create, read, update, and soft-delete projects
- **Unique Project Keys**: Short identifiers (e.g., "PROJ", "WEB") with uniqueness validation
- **Soft Delete**: Projects marked as inactive instead of permanent deletion
- **Active/Inactive Toggle**: Enable/disable projects while preserving data
- **Duplicate Key Validation**: Prevents creating projects with existing keys
- **Project-Task Relationship**: Track all tasks belonging to a project

### Task Management
- **Polymorphic Task Types**:
  - **BugTask**: Includes steps to reproduce, severity, environment
  - **FeatureTask**: Includes story points, acceptance criteria, epic
- **Task Properties**:
  - Status tracking (Todo, Assigned, InProgress, Review, Done)
  - Priority levels (Low, Medium, High, Critical)
  - Due dates
  - User assignment
- **Task Master View**: Browse and manage all tasks
- **Task Detail View**: Edit task properties and metadata

### Kanban Board
- **Drag-and-Drop Interface**: Move tasks between status columns (Todo → InProgress → Done)
- **Project Filtering**: View tasks from specific projects or all projects
- **Visual Task Cards**: Display title, assignee, priority, and tags
- **Real-Time Updates**: Board refreshes after task status changes

### Admin Panel
- **Centralized Administration**: Admin-only features in dedicated panel
- **User Management**: Create new users with role assignment (Admin/Worker)
- **Project Management**: Create, edit, delete projects with validation
- **Task Creation**: Create Bug or Feature tasks with full metadata
- **Export to CSV**: Export projects to CSV files with task counts
- **Dynamic UI**: Right-side container shows forms/lists based on selected action

### CSV Export
- **Project Export**: Export all projects with metadata (Name, Key, Description, TasksCount, SprintsCount)
- **File Naming**: Timestamped filenames (Projects_YYYYMMDD_HHmmss.csv)
- **Data Integrity**: Exports use current database state

### Logging
- **Structured Logging**: Serilog integration across all layers
- **Log Levels**: Debug, Info, Warning, Error for different scenarios
- **File Logging**: Daily rotating log files (logs/taskmanager-YYYY-MM-DD.log)
- **Console Logging**: Colored output for development
- **Performance Tracking**: Log database queries and operation timing

### Data Persistence
- **SQLite Database**: Lightweight, file-based database
- **Entity Framework Core 9.0**: Modern ORM with migrations
- **Data Seeding**: Automatic sample data on first run
- **Database Indexing**: Optimized queries on frequently accessed fields
- **Migration System**: Version-controlled schema changes

## 🚧 Future Features

### 🔴 CSV Export Issues & Improvements (🔄 In Progress)

**1. TasksCount = 0 in CSV Export**
When exporting projects immediately after login (without loading tasks in Dashboard), the TasksCount field shows 0 even if projects have tasks. This occurs because the repository method doesn't eager-load the Tasks relationship.

**2. IsActive Checkbox Logic**
When deactivating a project (unchecking Active checkbox), the project disappears from the list but its key remains reserved, preventing creation of new projects with the same key. Inactive projects should remain visible with visual distinction (grayed out).

**3. Public ID for CSV Export**
Current CSV exports contain internal GUIDs which are not user-friendly and pose security risks. Need to implement human-readable public IDs:
- Projects: `PROJ-001`, `PROJ-002`
- Tasks: `{ProjectKey}-001` (e.g., `WEB-042`, `MOBILE-015`)

### Task Manager UI with Filtering
- Advanced task filtering by Project, Status, Assigned To, Deadline
- Filter validation (at least one filter required)
- Export filtered tasks to CSV
- Load tasks based on selected filters
- Task search functionality

### Sprint Management
- Sprint CRUD operations with UI
- Sprint-Project relationship
- Assign tasks to sprints
- Sprint statistics (completion percentage, burndown)
- Close sprint functionality (admin-only)
- Sprint timeline visualization

### Tag System
- Tag CRUD operations with UI
- Multiple tags per task (N-N relationship)
- Tag-based filtering
- Color-coded tags
- Tag management interface

### Enhanced Export Features
- Export dropdown in Admin Panel (Projects / Tasks / Sprints)
- Filter-based task export
- Sprint export with statistics
- Custom date range exports
- Export format options (CSV, Excel)

### User Management Improvements
- User profile editing
- Password change functionality
- User avatar upload
- Activity history tracking
- User deactivation/reactivation

### Password Management System
- **Change Password Functionality**: Allow users to change their own passwords
- **Password Reset**: Admin ability to reset user passwords
- **Password Strength Validation**: Enforce strong password policies
- **Password Expiration**: Optional password expiration with configurable periods
- **Password History**: Prevent password reuse

### Dynamic Role Management
- **Custom Roles**: Create and manage custom roles beyond Admin/Worker
- **Permission Sets**: Define granular permissions for each role
- **Role Assignment UI**: Admin interface for assigning roles to users
- **Dynamic Permission Checks**: Replace hardcoded UI permissions with database-driven checks
- **Role Hierarchy**: Support for role inheritance and permission overrides
- **Audit Trail**: Log role and permission changes

### UI/UX Enhancements
- Create User/Task forms in dynamic container in Admin Panel (instead of modals )
- Improved inactive project visualization
- Dark mode support
- Responsive layout improvements
- Keyboard shortcuts

## 🚀 Getting Started

### Using Make (Recommended)

```bash
# Clone repository
git clone https://github.com/Vilis322/TaskManager.git
cd TaskManager

# Show all available commands
make help

# Restore dependencies and build
make restore
make build

# Apply database migrations (first-time setup)
make migrations-apply

# Run the application
make run

# Clean build artifacts
make clean

# Connect to database (requires sqlite3)
make connect-db

# Create new migration
make migrations-add NAME=YourMigrationName
```

### Using macOS/Linux

```bash
# Clone repository
git clone https://github.com/Vilis322/TaskManager.git
cd TaskManager

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Apply migrations (first-time setup)
dotnet ef database update \
  -p App.Infrastructure/App.Infrastructure.csproj \
  -s App.UI/App.UI.csproj

# Run application
dotnet run --project App.UI/App.UI.csproj
```

### Using Windows

```powershell
# Clone repository
git clone https://github.com/Vilis322/TaskManager.git
cd TaskManager

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Apply migrations (first-time setup)
dotnet ef database update `
  -p App.Infrastructure\App.Infrastructure.csproj `
  -s App.UI\App.UI.csproj

# Run application
dotnet run --project App.UI\App.UI.csproj
```

### Default Credentials

After first run, the following test users are seeded:

| Username | Password | Role   |
|----------|----------|--------|
| admin    | admin123 | Admin  |
| john     | john123  | Worker |
| jane     | jane123  | Worker |

## 📦 Requirements

### Runtime
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Optional Development Tools
- [SQLite Browser](https://sqlitebrowser.org/) - Database viewer
- [Make](https://www.gnu.org/software/make/) - Build automation (pre-installed on macOS/Linux)
- Git - Version control

### NuGet Packages (Automatically Restored)
- Avalonia 11.3.9 - Cross-platform UI framework
- Entity Framework Core 9.0 - ORM
- Serilog 4.2.0 - Structured logging
- CommunityToolkit.Mvvm 8.4.0 - MVVM helpers

## 🔄 CI/CD

GitHub Actions workflow (`.github/workflows/branch-policy.yml`) runs on:
- Pull requests
- Pushes to main, develop, feature/**, bugfix/**, hotfix/**

Workflow includes:
- Build verification
- Test execution
- Code formatting checks

## 🤝 Contributing

1. Create a feature branch (`git checkout -b feature/amazing-feature`)
2. Commit your changes (`git commit -m 'feat: Add amazing feature'`)
3. Push to the branch (`git push origin feature/amazing-feature`)
4. Open a Pull Request

### Branch Naming Convention

- `feature/` - New features
- `bugfix/` - Bug fixes
- `hotfix/` - Critical production fixes

## 👥 Authors

- **Kyrylo Pryiomyshev**
- **Oleksii Khodus**

## 🙏 Acknowledgments

- Inspired by Jira's task management workflow
- Built with [Avalonia UI](https://avaloniaui.net/)
- Uses [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

---

**Note**: This is a work-in-progress project. More features and documentation will be added as development continues.
