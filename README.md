# dotnet-backend-api

## Overview
This project is a backend API built using .NET 8 that implements authentication and multi-factor authentication (MFA) following Microsoft standard practices. The API is designed to handle requests from a frontend application, process authentication tokens (from Microsoft Entra ID/Azure AD), and provide secure access to resources.

## Project Structure
```
un-sichere-login-plattform-back
├── src
│   ├── Controllers
│   │   └── AuthController.cs
│   ├── Models
│   │   └── UserModel.cs
│   ├── Services
│   │   └── AuthService.cs
│   ├── Program.cs
│   └── Startup.cs
│   └── appsettings.json
├── un-sichere-login-plattform-back.sln
└── README.md
```

## Features
- **Authentication**: Accepts and validates JWT tokens issued by Microsoft Entra ID/Azure AD.
- **Multi-Factor Authentication (MFA)**: Supports MFA logic for enhanced security.
- **Swagger Integration**: The API is documented using Swagger for easy exploration and testing of endpoints.

## Setup Instructions
1. **Clone the Repository**: 
   ```
   git clone <repository-url>
   cd dotnet-backend-api
   ```

2. **Restore Dependencies**: 
   ```
   dotnet restore
   ```

3. **Run the Application**: 
   ```
   dotnet run --project src/dotnet-backend-api.csproj
   ```

4. **Access Swagger UI**: Open your browser and navigate to `http://localhost:5000/swagger` (oder Port aus der Konsole) to explore the API documentation and test endpoints.

## Usage Guidelines
- Use the `/login` endpoint to authenticate users and receive a token (if local login).
- For Microsoft login: Obtain a JWT from Microsoft Entra ID/Azure AD in the frontend and send it in the `Authorization` header (`Bearer <token>`) for protected endpoints.
- For MFA, follow the instructions provided during the login process to complete authentication.

## Contributing
Contributions are welcome! Please submit a pull request or open an issue for any enhancements or bug fixes.

## License
This project is licensed under the MIT License. See the LICENSE file for more details.