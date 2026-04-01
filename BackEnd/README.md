# BackEnd API

This folder contains the ASP.NET Core backend API for the chat application.

## Project
- Path: `BackEnd/API`
- Framework: .NET 9
- Database: SQLite (configured in `Program.cs` as `Data Source=chat`)
- Authentication: JWT bearer tokens

## Prerequisites
- .NET SDK 9.0 or later

## Setup
1. Open a terminal in `BackEnd/API`:
   ```powershell
   cd d:\MyProject\Angular\Chat\BackEnd\API
   ```
2. Restore dependencies:
   ```powershell
   dotnet restore
   ```

## Running the backend
1. Run the API:
   ```powershell
   dotnet run
   ```
2. By default, the API uses HTTPS and will be available on the local development URL shown in the terminal.

## Configuration
- `appsettings.json`: contains logging and JWT settings.
- `Program.cs`: configures SQLite, Identity, JWT authentication, and OpenAPI support.

## Notes
- The default JWT secret key is set to `YourSecretKeyHere` in `appsettings.json`. Replace this value before production use.
- The SQLite database file is created automatically using the configured connection string.
