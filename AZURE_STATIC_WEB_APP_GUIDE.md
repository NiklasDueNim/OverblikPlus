# Azure Static Web App Setup Guide

## 🚀 Opret ny Azure Static Web App via Azure Portal

### 1. Gå til Azure Portal
- Åbn [portal.azure.com](https://portal.azure.com)
- Login med din Azure account

### 2. Opret ny Static Web App
- Klik på "Create a resource"
- Søg efter "Static Web App"
- Klik "Create"

### 3. Konfigurer Static Web App
**Subscription:** Vælg din subscription

**Resource Group:** 
- Vælg "Create new"
- Navn: `overblikplus-dev-rg`

**Name:** `overblikplus-dev-swa`

**Plan type:** Free

**Region:** West Europe

**Source:** 
- Vælg "Other" (da vi ikke bruger GitHub Actions lige nu)

**App location:** `/` (root directory)

**Output location:** `wwwroot`

### 4. Klik "Review + create" og derefter "Create"

### 5. Hent din nye URL
Efter oprettelse får du en URL som:
`https://overblikplus-dev-swa.azurestaticapps.net`

## 📝 Opdater DNS indstillinger

I din domæne provider (hvor du har overblikplus.dk):

**Opdater CNAME record:**
- `www.overblikplus.dk` → `overblikplus-dev-swa.azurestaticapps.net`

**Opdater A record:**
- `overblikplus.dk` → `[IP fra Azure Static Web App]` (hvis nødvendigt)

## 🔧 Opdater frontend konfiguration

Når du har din nye URL, opdater:

1. **appsettings.production.json:**
```json
{
  "ENVIRONMENT": "production",
  "TASK_API_BASE_URL": "https://overblikplus-task-api-dev-aqcja5a8htcwb8fp.westeurope-01.azurewebsites.net",
  "USER_API_BASE_URL": "https://overblikplus-user-api-dev-cheeh0a0fgc0ayh5.westeurope-01.azurewebsites.net",
  "STATIC_WEB_APP_URL": "https://overblikplus-dev-swa.azurestaticapps.net"
}
```

2. **Program.cs** vil automatisk detektere dit domæne og bruge de korrekte endpoints

## 🚀 Deploy din frontend

### Metode 1: Azure CLI
```bash
# Installer Azure CLI
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Login til Azure
az login

# Deploy
az staticwebapp deploy \
  --name overblikplus-dev-swa \
  --resource-group overblikplus-dev-rg \
  --source ./OverblikPlus/out/wwwroot
```

### Metode 2: Azure Portal
1. Gå til din Static Web App i Azure Portal
2. Klik på "Overview"
3. Klik på "Browse" for at se din app
4. Upload dine filer via "Manage deployment token" eller "GitHub Actions"

## 🆓 Gratis tier inkluderer
- 100 GB bandwidth per måned
- 0.5 GB storage
- Custom domains
- SSL certificates
- Global CDN

## ✅ Næste skridt
1. Opret Static Web App via Azure Portal
2. Hent den nye URL
3. Opdater DNS indstillinger
4. Deploy din frontend
5. Test på overblikplus.dk

Din app vil automatisk:
- **Lokalt (Rider):** Bruge `localhost:5101` og `localhost:5102`
- **På overblikplus.dk:** Bruge Azure endpoints
