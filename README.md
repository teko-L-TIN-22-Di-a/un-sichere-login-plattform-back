# Azure AD Authentication Backend

## 🔐 Overview

This project is a .NET 8 backend API that implements **Azure AD authentication** using the Resource Owner Password Credentials (ROPC) flow. The API is **completely stateless** and requires **no database** - all user data comes directly from Azure AD and Microsoft Graph.

## 🏗️ Project Structure

```
un-sichere-login-plattform-back/
├── src/
│   ├── Controllers/
│   │   └── AuthController.cs      # Login & Profile endpoints
│   ├── Models/
│   │   └── UserModel.cs           # DTOs (LoginRequest, LoginResponse, UserInfo)
│   ├── Services/
│   │   └── AzureAuthService.cs    # Azure AD ROPC authentication
│   ├── Program.cs
│   ├── Startup.cs                 # JWT configuration
│   ├── appsettings.json           # Configuration (sanitized)
│   └── appsettings.Example.json   # Configuration template
├── test.http                      # REST Client tests
├── CONFIGURATION.md               # Detailed setup guide
└── README.md
```

## ✨ Features

- **🔑 Azure AD Authentication**: ROPC flow for username/password login
- **🛡️ JWT Token Validation**: Validates Azure AD tokens with `[Authorize]` attribute
- **👤 Microsoft Graph Integration**: Retrieves user information from Microsoft Graph
- **📚 Swagger Documentation**: Interactive API documentation
- **🌐 CORS Support**: Ready for frontend integration
- **🧪 REST Client Tests**: Easy testing with VS Code REST Client
- **🗃️ Database-Free**: Completely stateless, no local data storage needed

## 🚀 Quick Start

### 1. ⚠️ Configuration Required
**This repository does NOT contain sensitive configuration data!**

Copy the example configuration:
```bash
cp src/appsettings.Example.json src/appsettings.json
```

Then follow the detailed setup guide in [`CONFIGURATION.md`](CONFIGURATION.md) to:
- Set up Azure AD App Registration
- Configure authentication settings
- Create test users

### 2. Install Dependencies
```bash
dotnet restore
```

### 3. Run the Application
```bash
cd src
dotnet run
```

### 4. Test with REST Client
Use the provided `test.http` file with the REST Client extension in VS Code:
```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "demo@YOURDOMAIN.onmicrosoft.com",
  "password": "YourPassword123!"
}
```

## 🔧 API Endpoints

### POST `/api/auth/login`
Authenticates user against Azure AD and returns access token.

**Request:**
```json
{
  "username": "user@domain.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "success": true,
  "accessToken": "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6...",
  "message": "Authentication successful",
  "userInfo": {
    "id": "user-guid",
    "displayName": "Demo User",
    "email": "user@domain.com",
    "userPrincipalName": "user@domain.com"
  }
}
```

### GET `/api/auth/profile`
Protected endpoint that requires Azure AD token.

**Headers:**
```
Authorization: Bearer <azure-ad-token>
```

## 🛠️ Configuration

### Required Settings

Create `src/appsettings.json` with your Azure AD configuration:

```json
{
  "Authentication": {
    "JwtBearer": {      
      "Authority": "https://login.microsoftonline.com/YOUR_TENANT_ID/v2.0",
      "Audience": "YOUR_CLIENT_ID"
    }
  },
  "AzureAd": {
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_CLIENT_ID", 
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "Scope": "https://graph.microsoft.com/.default"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> **✅ No Database Required**: This application is completely stateless and requires no database setup. All user data comes directly from Azure AD and Microsoft Graph.

### Environment Variables (Production)
```bash
export AzureAd__TenantId="YOUR_TENANT_ID"
export AzureAd__ClientId="YOUR_CLIENT_ID"
export AzureAd__ClientSecret="YOUR_CLIENT_SECRET"
```

## 🔒 Security

### ⚠️ Never commit sensitive data:
- ❌ Tenant IDs
- ❌ Client IDs  
- ❌ Client Secrets
- ❌ User passwords

### ✅ Use instead:
- Environment variables
- Azure Key Vault
- Local configuration files (in .gitignore)

## 📖 Documentation

For detailed setup instructions, troubleshooting, and Azure AD configuration, see:

- **[CONFIGURATION.md](CONFIGURATION.md)** - Complete setup guide
- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Database-free architecture overview
- **[Swagger UI](http://localhost:5000/swagger)** - Interactive API documentation (when running)

## 🧪 Testing

### With REST Client (VS Code)
Install the REST Client extension and use `test.http`.

### With cURL
```bash
# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"user@domain.com","password":"password123"}'

# Profile (replace TOKEN with actual token)
curl -X GET http://localhost:5000/api/auth/profile \
  -H "Authorization: Bearer TOKEN"
```

## 🤝 Contributing
Contributions are welcome! Please ensure you:
1. Don't commit sensitive configuration data
2. Follow the existing code style
3. Add appropriate tests
4. Update documentation

## 📄 License
This project is licensed under the MIT License.