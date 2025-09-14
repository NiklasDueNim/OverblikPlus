# OverblikPlus Deployment Guide

## 🚀 Deployment til Azure med Pulumi

### 1. Installer Azure CLI og Pulumi

```bash
# Installer Azure CLI (hvis ikke allerede installeret)
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Installer Pulumi
curl -fsSL https://get.pulumi.com | sh
```

### 2. Login til Azure

```bash
az login
```

### 3. Sæt din SQL Admin password

```bash
cd Pulumi
pulumi config set sqlAdminPassword "DinSikrePassword123!" --secret
```

### 4. Deploy infrastructure

```bash
pulumi up
```

### 5. Hent de nye URLs

Efter deployment får du outputs som:
- `StaticWebAppUrl`: Din nye Azure Static Web App URL
- `StaticWebAppName`: Navnet på din Static Web App

### 6. Opdater DNS indstillinger

I din domæne provider (hvor du har overblikplus.dk):

**Opdater CNAME record:**
- `www.overblikplus.dk` → `[DinNyeStaticWebAppUrl]` (fra Pulumi output)

**Opdater A record:**
- `overblikplus.dk` → `[DinNyeStaticWebAppIP]` (hvis nødvendigt)

### 7. Deploy din frontend

```bash
cd OverblikPlus
dotnet publish -c Release -o ./publish
```

### 8. Upload til Azure Static Web App

Du kan bruge Azure CLI eller Azure Portal til at uploade dine filer til Static Web App.

## 🔧 Lokal udvikling

Din setup understøtter nu:

- **Development (Rider):** Bruger automatisk `localhost:5101` og `localhost:5102`
- **Production (overblikplus.dk):** Bruger automatisk Azure endpoints

## 📋 Næste skridt

1. Kør `pulumi up` for at oprette den nye Static Web App
2. Hent den nye URL fra Pulumi outputs
3. Opdater dine DNS indstillinger
4. Deploy din frontend til den nye Static Web App

## 🆓 Gratis tier

Azure Static Web Apps har en gratis tier der inkluderer:
- 100 GB bandwidth per måned
- 0.5 GB storage
- Custom domains
- SSL certificates
