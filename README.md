# RealTimeChatApp

A real-time chat application built with Angular for the frontend and ASP.NET Core for the backend.

## Features

- Real-time messaging
- Modern web UI with Angular
- RESTful API with .NET Core

## Tech Stack

- **Frontend**: Angular
- **Backend**: ASP.NET Core (.NET 9)
- **Database**: (To be configured, e.g., SQL Server, PostgreSQL)

## Prerequisites

- Node.js (v18 or later)
- .NET SDK (9.0 or later)
- Git

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/Shajjad002/RealTimeChatApp.git
   cd RealTimeChatApp
   ```

2. Set up the backend:
   ```bash
   cd BackEnd/API
   dotnet restore
   ```

3. Set up the frontend:
   ```bash
   cd ../../FrontEnd/client
   npm install
   ```

## Running the Application

1. Start the backend API:
   ```bash
   cd BackEnd/API
   dotnet run
   ```
   The API will be available at `https://localhost:5001` (or as configured in `launchSettings.json`).

2. Start the frontend:
   ```bash
   cd FrontEnd/client
   ng serve
   ```
   The Angular app will be available at `http://localhost:4200`.

## Project Structure

```
Chat/
├── BackEnd/
│   └── API/          # ASP.NET Core Web API
│       ├── Controllers/
│       ├── Models/
│       ├── Program.cs
│       ├── appsettings.json
│       └── Properties/
│           └── launchSettings.json
├── FrontEnd/
│   └── client/       # Angular application
│       ├── src/
│       │   ├── app/
│       │   └── ...
│       ├── angular.json
│       └── package.json
├── Chat.sln          # Visual Studio solution file
└── README.md
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.