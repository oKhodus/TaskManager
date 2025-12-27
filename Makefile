.PHONY: help run build test clean restore connect-db migrations-add migrations-apply

# Default target
help:
	@echo "╔════════════════════════════════════════════════════════════════╗"
	@echo "║                    TaskManager - Make Commands                 ║"
	@echo "╠════════════════════════════════════════════════════════════════╣"
	@echo "║                                                                ║"
	@echo "║  make run                                                      ║"
	@echo "║    → Run the Avalonia UI application                           ║"
	@echo "║                                                                ║"
	@echo "╠════════════════════════════════════════════════════════════════╣"
	@echo "║                                                                ║"
	@echo "║  make build                                                    ║"
	@echo "║    → Build the entire solution                                 ║"
	@echo "║                                                                ║"
	@echo "╠════════════════════════════════════════════════════════════════╣"
	@echo "║                                                                ║"
	@echo "║  make test                                                     ║"
	@echo "║    → Run all unit tests (when implemented)                     ║"
	@echo "║                                                                ║"
	@echo "╠════════════════════════════════════════════════════════════════╣"
	@echo "║                                                                ║"
	@echo "║  make clean                                                    ║"
	@echo "║    → Clean build artifacts                                     ║"
	@echo "║                                                                ║"
	@echo "╠════════════════════════════════════════════════════════════════╣"
	@echo "║                                                                ║"
	@echo "║  make restore                                                  ║"
	@echo "║    → Restore NuGet packages                                    ║"
	@echo "║                                                                ║"
	@echo "╠════════════════════════════════════════════════════════════════╣"
	@echo "║                                                                ║"
	@echo "║  make connect-db                                               ║"
	@echo "║    → Open database connection (SQLite browser)                 ║"
	@echo "║                                                                ║"
	@echo "╠════════════════════════════════════════════════════════════════╣"
	@echo "║                                                                ║"
	@echo "║  make migrations-add NAME=<migration_name>                     ║"
	@echo "║    → Create a new EF Core migration                            ║"
	@echo "║                                                                ║"
	@echo "╠════════════════════════════════════════════════════════════════╣"
	@echo "║                                                                ║"
	@echo "║  make migrations-apply                                         ║"
	@echo "║    → Apply pending migrations to the database                  ║"
	@echo "║                                                                ║"
	@echo "╚════════════════════════════════════════════════════════════════╝"

# Run the Avalonia application
run:
	@echo "▶ Running TaskManager application..."
	dotnet run --project App.UI/App.UI.csproj

# Build the solution
build:
	@echo "🔨 Building TaskManager solution..."
	dotnet build TaskManager.sln

# Run tests (placeholder for future tests)
test:
	@echo "🧪 Running tests..."
	@if [ -d "App.Tests" ]; then \
		dotnet test TaskManager.sln; \
	else \
		echo "⚠️  No test project found. Create App.Tests project to run tests."; \
	fi

# Clean build artifacts
clean:
	@echo "🧹 Cleaning build artifacts..."
	dotnet clean TaskManager.sln
	@find . -type d -name "bin" -o -name "obj" | xargs rm -rf
	@echo "✓ Clean complete"

# Restore NuGet packages
restore:
	@echo "📦 Restoring NuGet packages..."
	dotnet restore TaskManager.sln

# Connect to database (SQLite)
connect-db:
	@echo "🗄️  Connecting to database..."
	@if [ -f "App.UI/taskmanager.db" ]; then \
		if command -v sqlite3 >/dev/null 2>&1; then \
			sqlite3 App.UI/taskmanager.db; \
		else \
			echo "⚠️  sqlite3 not found. Install with: brew install sqlite"; \
			echo "Database location: App.UI/taskmanager.db"; \
		fi \
	else \
		echo "⚠️  Database not found. Run 'make migrations-apply' first."; \
	fi

# Add new migration
migrations-add:
	@if [ -z "$(NAME)" ]; then \
		echo "❌ Error: Migration name required. Usage: make migrations-add NAME=YourMigrationName"; \
		exit 1; \
	fi
	@echo "📝 Creating migration: $(NAME)..."
	dotnet ef migrations add $(NAME) -p App.Infrastructure/App.Infrastructure.csproj -s App.UI/App.UI.csproj -o Persistence/Migrations

# Apply migrations
migrations-apply:
	@echo "⚡ Applying migrations to database..."
	dotnet ef database update -p App.Infrastructure/App.Infrastructure.csproj -s App.UI/App.UI.csproj
