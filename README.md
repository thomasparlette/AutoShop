# AutoShop

AutoShop is a WPF shop-management application for work orders, inventory, purchase orders, customers, vehicles, technicians, and printable receipts/checklists.

## Local database
The app stores its SQLite database in the current user's Local AppData folder:

`%LocalAppData%\AutoShop\AutoShop.db`

## Startup
The app migrates the database on startup, seeds default settings/users, then shows the login window.

## Default login
- Username: `admin`
- Password: `Admin123!`

## Solution structure
- `AutoShop.Core` — entities and enums
- `AutoShop.Data` — Entity Framework Core context, migrations, and database factory
- `AutoShop.Services` — business services
- `AutoShop.MainApp` — WPF application, views, and view models

## Version control notes
The repository ignores local SQLite database files and Visual Studio build artifacts. Migrations are tracked in source control so the database schema can be recreated from a clean clone.
