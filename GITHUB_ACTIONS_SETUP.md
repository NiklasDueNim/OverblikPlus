# GitHub Actions Setup Guide

## 🚀 Sådan sætter du GitHub Actions op til Pulumi og Azure Static Web App

### 1. Opret GitHub Repository
Hvis du ikke allerede har et GitHub repository:
```bash
git init
git add .
git commit -m "Initial commit"
git remote add origin https://github.com/[DIN-USERNAME]/overblikplus.git
git push -u origin main
```

### 2. Sæt op Pulumi Cloud
1. Gå til [app.pulumi.com](https://app.pulumi.com)
2. Login med din GitHub account
3. Opret en ny organization (hvis du ikke har en)
4. Gå til "Settings" → "Access Tokens"
5. Opret en ny token og kopier den

### 3. Sæt GitHub Secrets op
I dit GitHub repository, gå til:
**Settings** → **Secrets and variables** → **Actions**

Tilføj følgende secrets:

#### Pulumi Secrets:
- `PULUMI_ACCESS_TOKEN`: Din Pulumi Cloud access token
- `PULUMI_CONFIG_PASSPHRASE`: En sikker passphrase til encryption

#### Azure Secrets:
- `AZURE_CREDENTIALS`: Azure service principal credentials (JSON format)
- `AZURE_STATIC_WEB_APPS_API_TOKEN`: Din Azure Static Web App deployment token

### 4. Opret Azure Service Principal
```bash
# Login til Azure
az login

# Opret service principal
az ad sp create-for-rbac --name "overblikplus-github-actions" \
  --role contributor \
  --scopes /subscriptions/[DIN-SUBSCRIPTION-ID]/resourceGroups/[RESOURCE-GROUP-NAME] \
  --sdk-auth
```

Kopier output JSON og sæt det som `AZURE_CREDENTIALS` secret.

### 5. Opret Azure Static Web App
1. Gå til [portal.azure.com](https://portal.azure.com)
2. Opret ny "Static Web App"
3. Navn: `overblikplus-dev-swa`
4. Resource Group: `overblikplus-dev-rg`
5. Plan: Free
6. Region: West Europe
7. Source: GitHub
8. Vælg dit repository og main branch

### 6. Hent Deployment Token
1. Gå til din Static Web App i Azure Portal
2. Klik på "Manage deployment token"
3. Kopier tokenet og sæt det som `AZURE_STATIC_WEB_APPS_API_TOKEN` secret

### 7. Sæt Pulumi Stack op
```bash
# I Pulumi mappen
cd Pulumi

# Sæt SQL admin password
pulumi config set sqlAdminPassword "DinSikrePassword123!" --secret

# Opret stack
pulumi stack init dev
```

### 8. Push til GitHub
```bash
git add .
git commit -m "Add GitHub Actions workflows"
git push origin main
```

## 🔄 Workflow Process

### Infrastructure Deployment (Pulumi)
- **Trigger:** Push til main branch med ændringer i `Pulumi/` mappen
- **Hvad sker der:**
  1. Builds Pulumi projektet
  2. Deployer Azure infrastructure (SQL Server, Storage, Static Web App)
  3. Outputter URLs til de oprettede ressourcer

### Frontend Deployment
- **Trigger:** Push til main branch med ændringer i `OverblikPlus/` mappen
- **Hvad sker der:**
  1. Builds Blazor WebAssembly appen
  2. Deployer til Azure Static Web App
  3. Opdaterer din website automatisk

## 📋 Næste skridt

1. **Følg denne guide** til at sætte GitHub Actions op
2. **Push din kode** til GitHub
3. **Workflows kører automatisk** og deployer din infrastructure og frontend
4. **Din website** vil være tilgængelig på den nye Azure Static Web App URL

## 🆓 Gratis tier
- **GitHub Actions:** 2000 minutter per måned gratis
- **Azure Static Web App:** 100 GB bandwidth per måned gratis
- **Azure SQL:** S0 tier (Basic) for development

## ✅ Fordele ved denne setup
- **Automatisk deployment** når du pusher kode
- **Infrastructure as Code** med Pulumi
- **Gratis hosting** på Azure
- **Custom domain** support
- **SSL certificates** automatisk
