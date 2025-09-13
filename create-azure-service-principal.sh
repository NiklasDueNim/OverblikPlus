#!/bin/bash

echo "🔧 Opretter Azure Service Principal til GitHub Actions..."

# Login til Azure (hvis ikke allerede logget ind)
echo "📝 Login til Azure..."
az login

# Hent subscription ID
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
echo "📋 Subscription ID: $SUBSCRIPTION_ID"

# Opret resource group (hvis den ikke eksisterer)
RESOURCE_GROUP="overblikplus-dev-rg"
echo "📁 Opretter resource group: $RESOURCE_GROUP"
az group create --name $RESOURCE_GROUP --location "West Europe"

# Opret service principal
echo "🔑 Opretter service principal..."
SP_OUTPUT=$(az ad sp create-for-rbac --name "overblikplus-github-actions" \
  --role contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP \
  --sdk-auth)

echo "✅ Service Principal oprettet!"
echo ""
echo "📋 Kopier denne JSON og sæt den som 'AZURE_CREDENTIALS' secret i GitHub:"
echo ""
echo "$SP_OUTPUT"
echo ""
echo "🔧 Næste skridt:"
echo "1. Kopier JSON'en ovenfor"
echo "2. Gå til GitHub repository Settings → Secrets and variables → Actions"
echo "3. Opret ny secret med navn: AZURE_CREDENTIALS"
echo "4. Sæt value til JSON'en"
