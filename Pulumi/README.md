# OverblikPlus Azure Infrastructure

Denne Pulumi konfiguration opretter Azure ressourcer til OverblikPlus applikationen.

## Ressourcer

- **Resource Group**: `overblikplus-{environment}-rg`
- **SQL Server**: `overblikplus-{environment}-sql`
- **SQL Database**: `overblikplus-{environment}-db`
- **Storage Account**: `overblikplus{environment}storage`
- **Blob Container**: `images`

## Installation

1. Installer Pulumi CLI:
```bash
curl -fsSL https://get.pulumi.com | sh
```

2. Installer Azure CLI og login:
```bash
az login
az account set --subscription "your-subscription-id"
```

3. Installer dependencies:
```bash
cd Pulumi
dotnet restore
```

## Brug

### Development Environment
```bash
pulumi stack select dev
pulumi config set sqlAdminPassword "YourStrongPassword123!"
pulumi up
```

### Production Environment
```bash
pulumi stack select prod
pulumi config set sqlAdminPassword "YourProductionPassword123!"
pulumi up
```

## Outputs

Efter deployment får du:
- Connection string til SQL Database
- Blob storage connection string
- Blob base URL
- Alle nødvendige connection details

## Konfiguration

Du kan ændre følgende i Pulumi.{env}.yaml:
- `environment`: dev/prod
- `projectName`: overblikplus
- `sqlAdminPassword`: SQL Server admin password

## Sikkerhed

- SQL Server password er gemt som secret
- Firewall rules er konfigureret
- Azure services har adgang til SQL Server
