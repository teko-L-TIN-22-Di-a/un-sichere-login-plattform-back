# 🏛️ Architecture Overview

## System Architecture (Database-Free)

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│                 │    │                 │    │                 │
│   Frontend      │    │   Backend API   │    │   Azure AD      │
│   (React/Vue)   │    │   (.NET Core)   │    │   (Microsoft)   │
│                 │    │                 │    │                 │
└─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
          │                      │                      │
          │ 1. Username/Password  │                      │
          ├─────────────────────>│                      │
          │                      │ 2. ROPC Auth Request │
          │                      ├─────────────────────>│
          │                      │                      │
          │                      │ 3. Azure AD Token    │
          │                      │<─────────────────────┤
          │ 4. Token Response     │                      │
          │<─────────────────────┤                      │
          │                      │                      │
          │ 5. API Request + Token│                      │
          ├─────────────────────>│                      │
          │                      │ 6. Validate Token    │
          │                      ├─────────────────────>│
          │                      │                      │
          │                      │ 7. Token Valid       │
          │                      │<─────────────────────┤
          │ 8. Protected Resource │                      │
          │<─────────────────────┤                      │
          │                      │                      │
```

## Data Flow

### 🔐 Authentication Flow
1. **Frontend** → Backend: Username/Password
2. **Backend** → Azure AD: ROPC Authentication Request
3. **Azure AD** → Backend: Azure AD Token
4. **Backend** → Frontend: Token Response

### 🛡️ Authorization Flow
5. **Frontend** → Backend: API Request + Azure AD Token
6. **Backend** → Azure AD: Token Validation
7. **Azure AD** → Backend: Token Valid/Invalid
8. **Backend** → Frontend: Protected Resource

## 🗃️ No Database Required

### Why Database-Free?
- **✅ User Data**: Stored in Azure AD
- **✅ Authentication**: Handled by Azure AD
- **✅ Authorization**: Based on Azure AD Claims
- **✅ User Info**: Retrieved from Microsoft Graph
- **✅ Tokens**: Validated by Azure AD

### Benefits
- **🚀 Simplified Deployment**: No database setup required
- **🔒 Enhanced Security**: No local credentials to protect
- **📈 Scalability**: Stateless architecture
- **💰 Cost Effective**: No database hosting costs
- **🔧 Maintenance**: No database migrations or backups

## 🔧 Configuration Only

The system requires minimal configuration through environment-specific files:

### Base Configuration (`appsettings.json`)

Contains no secrets - only placeholders and structure:

```json
{
  "AzureAd": {
    "TenantId": "PLACEHOLDER_TENANT_ID",
    "ClientId": "PLACEHOLDER_CLIENT_ID",
    "ClientSecret": "PLACEHOLDER_CLIENT_SECRET",
    "Domain": "PLACEHOLDER_DOMAIN"
  },
  "Authentication": {
    "JwtBearer": {
      "Authority": "https://login.microsoftonline.com/PLACEHOLDER_TENANT_ID/v2.0",
      "Audience": "PLACEHOLDER_CLIENT_ID"
    }
  },
  "RateLimit": {
    "RegistrationLimitPerHour": 5
  }
}
```

### Local Development (`appsettings.Local.json`)

Contains actual secrets for local development (gitignored):

```json
{
  "AzureAd": {
    "TenantId": "your-actual-tenant-id",
    "ClientId": "your-actual-client-id",
    "ClientSecret": "your-actual-client-secret",
    "Domain": "yourdomain.onmicrosoft.com"
  }
}
```

### Security Features

- **🔒 No Secrets in Code**: All sensitive data in gitignored files
- **🛡️ Rate Limiting**: Configurable registration limits (5/IP/hour default)
- **🔐 Token Validation**: Direct Azure AD token validation
- **📝 Audit Trail**: All authentication through Azure AD logs

## 🛡️ Security Features

### Rate Limiting

- **Registration**: Max 5 registrations per IP per hour
- **Automatic blocking**: HTTP 429 response when exceeded
- **Headers**: `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`

### Authentication & Authorization

- **Stateless**: No session storage, pure JWT
- **Azure AD Tokens**: Direct validation with Microsoft
- **Claims-based**: User info from Azure AD claims
- **CORS Protection**: Configurable allowed origins

### User Management

- **Registration**: Creates users directly in Azure AD via Microsoft Graph
- **Login**: ROPC flow for username/password authentication
- **Profile**: Retrieves user data from Azure AD
- **No Local Storage**: All user data stays in Azure AD

## 🏗️ API Endpoints

### Authentication Controller (`/api/auth`)

- `POST /login` - Authenticate user with Azure AD
- `POST /register` - Create new user in Azure AD (rate limited)
- `GET /profile` - Get current user profile (protected)

### Health Controller (`/api/health`)

- `GET /` - Basic health check
- `GET /detailed` - Detailed health with config validation

**No connection strings, no database migrations, no user tables - just pure Azure AD integration!** 🎉

## 📚 Documentation

- **[SETUP.md](SETUP.md)** - Developer onboarding and local development
- **[CONFIGURATION.md](CONFIGURATION.md)** - Azure AD setup guide for administrators
