# Azure Entra ID Integration Setup

## Übersicht

Diese Anwendung wurde für die Integration mit Azure Entra ID (ehemals Azure AD) konfiguriert. Das Backend kann jetzt Benutzername und Passwort vom Frontend empfangen und die Authentifizierung gegen Azure Entra ID durchführen.

## Erforderliche Azure AD-Konfiguration

### 1. App-Registrierung in Azure Portal

1. Gehen Sie zum [Azure Portal](https://portal.azure.com)
2. Navigieren Sie zu **Azure Active Directory** > **App-Registrierungen**
3. Wählen Sie Ihre bestehende App-Registrierung aus oder erstellen Sie eine neue
4. Notieren Sie sich:
   - **Application (client) ID**: `7cc5e9c9-9bea-4b32-bcc5-c597be151fc8`
   - **Directory (tenant) ID**: `6d453790-2dfb-4cdf-a804-e836719f5b8a`

### 2. Client Secret erstellen

1. Gehen Sie zu **Certificates & secrets** > **Client secrets**
2. Klicken Sie auf **New client secret**
3. Geben Sie eine Beschreibung ein und wählen Sie ein Ablaufdatum
4. Kopieren Sie den **Value** (nicht die ID!) - dies ist Ihr `ClientSecret`

### 3. API-Berechtigungen konfigurieren

1. Gehen Sie zu **API permissions**
2. Stellen Sie sicher, dass folgende Berechtigungen vorhanden sind:
   - **Microsoft Graph** > **User.Read** (Application oder Delegated)
   - Andere benötigte Berechtigungen je nach Anforderung

### 4. Authentifizierungstyp aktivieren

1. Gehen Sie zu **Authentication** > **Advanced settings**
2. Aktivieren Sie **Allow public client flows** (für Resource Owner Password Credentials Flow)

⚠️ **Wichtiger Hinweis**: Der Resource Owner Password Credentials Flow sollte nur für vertrauenswürdige Anwendungen verwendet werden und ist nicht für öffentliche Clients empfohlen.

## Konfiguration der Anwendung

### appsettings.json aktualisieren

Aktualisieren Sie die `appsettings.json` mit Ihren Azure AD-Werten:

```json
{
  "AzureAd": {
    "TenantId": "6d453790-2dfb-4cdf-a804-e836719f5b8a",
    "ClientId": "7cc5e9c9-9bea-4b32-bcc5-c597be151fc8",
    "ClientSecret": "IHR_CLIENT_SECRET_HIER",
    "Scope": "https://graph.microsoft.com/.default"
  }
}
```

## API-Endpunkte

### POST /api/auth/login

Authentifiziert einen Benutzer mit Azure Entra ID.

**Request Body:**
```json
{
  "username": "benutzer@domain.com",
  "password": "passwort123"
}
```

**Response (Success):**
```json
{
  "success": true,
  "accessToken": "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIs...",
  "message": "Authentication successful",
  "userInfo": {
    "id": "12345678-1234-1234-1234-123456789012",
    "displayName": "Max Mustermann",
    "email": "max.mustermann@domain.com",
    "userPrincipalName": "max.mustermann@domain.com"
  }
}
```

**Response (Error):**
```json
{
  "success": false,
  "message": "Invalid username or password"
}
```

### GET /api/auth/profile

Gibt die Profildaten des authentifizierten Benutzers zurück.

**Headers:**
```
Authorization: Bearer YOUR_ACCESS_TOKEN
```

## Frontend-Integration

Ihr Frontend kann jetzt folgendermaßen mit dem Backend kommunizieren:

```javascript
// Login
const loginResponse = await fetch('/api/auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    username: 'benutzer@domain.com',
    password: 'passwort123'
  })
});

const loginData = await loginResponse.json();

if (loginData.success) {
  // Token speichern
  localStorage.setItem('accessToken', loginData.accessToken);
  
  // Profil abrufen
  const profileResponse = await fetch('/api/auth/profile', {
    headers: {
      'Authorization': `Bearer ${loginData.accessToken}`
    }
  });
}
```

## Sicherheitshinweise

1. **Client Secret sicher aufbewahren**: Verwenden Sie Azure Key Vault oder Umgebungsvariablen
2. **HTTPS verwenden**: Stellen Sie sicher, dass alle Kommunikation über HTTPS erfolgt
3. **Token-Lebensdauer**: Implementieren Sie Token-Refresh-Mechanismen
4. **Fehlerbehandlung**: Implementieren Sie robuste Fehlerbehandlung für verschiedene Authentifizierungsszenarien

## Nächste Schritte

1. Konfigurieren Sie Ihr Azure AD mit den oben genannten Schritten
2. Aktualisieren Sie die `appsettings.json` mit Ihrem Client Secret
3. Testen Sie die Login-Funktionalität
4. Implementieren Sie die Frontend-Integration
5. Konfigurieren Sie CORS für Ihre Frontend-URL

## Fehlerbehebung

- **Invalid client**: Überprüfen Sie Client ID und Secret
- **Invalid scope**: Stellen Sie sicher, dass die API-Berechtigungen korrekt konfiguriert sind
- **CORS-Fehler**: Aktualisieren Sie die CORS-Konfiguration in `Startup.cs`
- **Token-Validierung fehlgeschlagen**: Überprüfen Sie die JWT-Konfiguration
