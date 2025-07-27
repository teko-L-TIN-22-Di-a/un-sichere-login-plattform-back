# � Quick Start Guide

## 📋 Prerequisites

- **.NET 8 SDK** installed
- **Azure AD Tenant** with App Registration configured
- **Visual Studio Code** with REST Client extension

> **Need Azure AD setup?** See [CONFIGURATION.md](CONFIGURATION.md) for detailed Azure AD configuration.

## ⚡ Quick Setup

### 1. Clone and Install

```bash
git clone <your-repo>
cd un-sichere-login-plattform-back
dotnet restore
```

### 2. Configure Your Secrets

**⚠️ IMPORTANT:** Never commit real secrets to Git!

Copy your Azure AD credentials to a local config file:

```bash
# Copy the template
cp src/appsettings.json src/appsettings.Local.json
```

Edit `src/appsettings.Local.json` with your Azure AD values:

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
    "Scope": "https://graph.microsoft.com/.default",
    "Domain": "yourdomain.onmicrosoft.com"
  }
}
```

### 3. Run the API

```bash
cd src
dotnet run
```

🎉 **API is running at:** `http://localhost:5000`

### 4. Test the Endpoints

Open `test.http` in VS Code and try:

- **Health Check**: `GET /api/health/detailed`
- **Register User**: `POST /api/auth/register` 
- **Login**: `POST /api/auth/login`
- **Get Profile**: `GET /api/auth/profile` (needs token)

## 🛡️ Security Features Included

- ✅ **Rate Limiting**: 5 registrations per IP/hour
- ✅ **Azure AD Integration**: No local user database
- ✅ **JWT Authentication**: Stateless tokens
- ✅ **Secret Management**: Local config files (not in Git)

## � Security Checklist

Before you start coding:

- ✅ `appsettings.Local.json` exists with your real credentials
- ✅ `appsettings.json` has only placeholder values
- ✅ `.gitignore` blocks secret files
- ✅ Azure AD app has required permissions (see [CONFIGURATION.md](CONFIGURATION.md))

## 🚨 Common Issues

### "Azure AD configuration is missing"

Check your `appsettings.Local.json` file exists and has all required fields.

### "Failed to get application token"

Your Azure AD app needs proper permissions. See [CONFIGURATION.md](CONFIGURATION.md) for setup details.

### "Rate limit exceeded"

You've hit the 5 registrations/hour limit. Wait or restart the API to reset.

## 📁 Project Structure

```
src/
├── Controllers/         # API endpoints
├── Services/           # Business logic  
├── Models/             # Data transfer objects
├── Attributes/         # Custom attributes (rate limiting)
├── appsettings.json    # Public config (NO SECRETS!)
└── appsettings.Local.json   # Your secrets (NOT in Git)
```

## 🌐 Next Steps

- **Frontend Integration**: Use the JWT tokens from `/api/auth/login`
- **Deployment**: See [CONFIGURATION.md](CONFIGURATION.md) for production setup
- **Customization**: Modify rate limits, add new endpoints, etc.

---

📚 **Need more details?** Check [ARCHITECTURE.md](ARCHITECTURE.md) for system design or [CONFIGURATION.md](CONFIGURATION.md) for Azure AD setup.
