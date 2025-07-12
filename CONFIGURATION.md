# 🔧 Konfigurationsanleitung

## Azure AD App Registration Setup

### 1. Azure AD App Registration erstellen

1. Gehe zu [Azure Portal](https://portal.azure.com)
2. Navigiere zu **Azure Active Directory** → **App registrations**
3. Klicke auf **New registration**
4. Konfiguriere:
   - **Name**: `demo backend` (oder ein beliebiger Name)
   - **Supported account types**: `Accounts in this organizational directory only (Single tenant)`
   - **Redirect URI**: Leer lassen

### 2. App Registration konfigurieren

#### Authentication Tab:
- **Allow public client flows**: `Yes` (für ROPC Flow)

#### API Permissions:
- `Microsoft Graph` → `User.Read` (Delegated)
- `Microsoft Graph` → `profile` (Delegated)  
- `Microsoft Graph` → `openid` (Delegated)
- `Microsoft Graph` → `email` (Delegated)
- **Admin Consent**: Klicke auf "Grant admin consent"

#### Certificates & Secrets:
- Erstelle ein **Client Secret**
- Notiere dir den **Value** (nicht die Secret ID!)

#### Manifest:
```json
{
  "signInAudience": "AzureADMyOrg"
}
```

### 3. Tenant und IDs sammeln

Du benötigst:
- **Tenant ID**: Aus Azure AD → Properties → Directory ID
- **Client ID**: Aus App Registration → Overview → Application ID
- **Client Secret**: Aus Certificates & Secrets → Client secrets

### 4. Test-User erstellen

```powershell
# PowerShell als Administrator
Connect-AzureAD

# User erstellen
New-AzureADUser -DisplayName "Demo User" -UserPrincipalName "demo@YOURDOMAIN.onmicrosoft.com" -MailNickName "demo" -PasswordProfile @{Password="DeinSicheresPasswort123!"; ForceChangePasswordNextLogin=$false} -AccountEnabled $true

# Passwort setzen (falls nötig)
Set-AzureADUserPassword -ObjectId "USER_OBJECT_ID" -Password (ConvertTo-SecureString "DeinSicheresPasswort123!" -AsPlainText -Force)
```

## Konfiguration

### appsettings.json (src/)
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

> **Hinweis**: Diese Anwendung ist vollständig **stateless** und benötigt **keine Datenbank**. Alle User-Daten kommen direkt von Azure AD / Microsoft Graph.

### test.http
```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "demo@YOURDOMAIN.onmicrosoft.com",
  "password": "DeinSicheresPasswort123!"
}
```

## Deployment

### Lokale Entwicklung
```bash
cd src
dotnet run
```

### Umgebungsvariablen (Production)
```bash
export AzureAd__TenantId="YOUR_TENANT_ID"
export AzureAd__ClientId="YOUR_CLIENT_ID"  
export AzureAd__ClientSecret="YOUR_CLIENT_SECRET"
```

## Troubleshooting

### Häufige Fehler:

**AADSTS50126: Error validating credentials**
- Falsches Passwort oder Username

**AADSTS50034: User account does not exist**
- User existiert nicht im Tenant

**AADSTS65001: The user or administrator has not consented**
- Admin Consent für API Permissions fehlt

**AADSTS7000218: The request body must contain the following parameter: 'client_assertion' or 'client_secret'**
- Client Secret fehlt oder ist falsch

### Logs checken:
```bash
# Backend Logs
dotnet run --urls http://localhost:5000

# Test Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"demo@YOURDOMAIN.onmicrosoft.com","password":"DeinSicheresPasswort123!"}'
```

## Sicherheitshinweise

⚠️ **Niemals in Git committen:**
- `TenantId`
- `ClientId` 
- `ClientSecret`
- Passwörter

✅ **Verwende stattdessen:**
- Umgebungsvariablen
- Azure Key Vault
- `appsettings.Development.json` (in .gitignore)
