# Cinema Booking App (F# / WinForms)

Simple Windows desktop app for managing cinema halls, movies, and seat bookings, backed by MySQL. Includes xUnit tests.

## Prerequisites
- .NET SDK 8 (Windows)
- MySQL Server (or MariaDB) running locally
- Optional: VS Code with Ionide or Visual Studio for F#

## Database setup
1. Ensure MySQL is running and you have a user with create DB permissions.
2. Create schema and seed sample data:
   ```sh
   mysql -u <user> -p < db.sql
   ```
   The script creates `cinema_db` with `halls`, `movies`, and `bookings` tables.
3. If your MySQL credentials differ, update the connection string in [Config.fs](Config.fs).

## Restore & build
From the repo root:
```sh
dotnet restore
dotnet build
```

## Run the app
```sh
dotnet run --project CinemaApp.fsproj
```
The WinForms UI will open; use **ADMIN DASHBOARD** to add halls/movies (password is set in [Config.fs](Config.fs)).

## Run tests
```sh
dotnet test CinemaApp.Tests/CinemaApp.Tests.fsproj
```

## Project layout
- [CinemaApp.fsproj](CinemaApp.fsproj) — app project (WinForms).
- [CinemaApp.Tests/CinemaApp.Tests.fsproj](CinemaApp.Tests/CinemaApp.Tests.fsproj) — xUnit tests.
- [Config.fs](Config.fs) — DB connection string and admin password.
- [db.sql](db.sql) — schema and seed data.
- [UI.fs](UI.fs) — WinForms screens (menu, booking grid, admin tabs).
- [Database.fs](Database.fs) — data access (MySQL connector).
- [Core.fs](Core.fs) and [Domain.fs](Domain.fs) — domain types and helpers.

## Notes
- The app targets `net8.0-windows` and uses Windows Forms; run on Windows.
- Use a real MySQL password and matching user instead of the empty default.
- Tests create temporary rows; use a dedicated database for testing if needed.
