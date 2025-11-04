#!/bin/bash

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

echo "========================================="
echo "Testing OverblikPlus Configuration"
echo "========================================="
echo ""

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test 1: Check Frontend Build
echo "1. Testing Frontend Build..."
cd OverblikPlus
BUILD_OUTPUT=$(dotnet build --no-restore 2>&1)
if echo "$BUILD_OUTPUT" | grep -q "Build succeeded"; then
    echo -e "   ${GREEN}✅ Frontend builds successfully${NC}"
elif echo "$BUILD_OUTPUT" | grep -q "error"; then
    echo -e "   ${RED}❌ Frontend build failed with errors${NC}"
    exit 1
else
    echo -e "   ${YELLOW}⚠️  Frontend build has warnings but no errors${NC}"
fi

# Go back to root
cd "$SCRIPT_DIR"

# Test 2: Check Environment Detection URLs
echo ""
echo "2. Testing Environment Detection Logic..."

# Check prod domain
if grep -q 'host.Contains("overblikplus.dk")' "$SCRIPT_DIR/OverblikPlus/Program.cs"; then
    echo -e "   ${GREEN}✅ Production domain check exists${NC}"
else
    echo -e "   ${RED}❌ Production domain check missing${NC}"
fi

# Check prod static web app
if grep -q "nice-wave-08dd97903.1.azurestaticapps.net" "$SCRIPT_DIR/OverblikPlus/Program.cs"; then
    echo -e "   ${GREEN}✅ Production Static Web App URL configured${NC}"
else
    echo -e "   ${RED}❌ Production Static Web App URL missing${NC}"
fi

# Check dev static web app
if grep -q "witty-meadow-0c52c9003.2.azurestaticapps.net" "$SCRIPT_DIR/OverblikPlus/Program.cs"; then
    echo -e "   ${GREEN}✅ Development Static Web App URL configured${NC}"
else
    echo -e "   ${RED}❌ Development Static Web App URL missing${NC}"
fi

# Test 3: Check Backend CORS Settings
echo ""
echo "3. Testing Backend CORS Configuration..."

# TaskMicroService CORS
echo "   TaskMicroService CORS:"
if grep -q "nice-wave-08dd97903.1.azurestaticapps.net" "$SCRIPT_DIR/OverblikPlusBackend/TaskMicroService/Program.cs"; then
    echo -e "      ${GREEN}✅ Prod URL allowed${NC}"
else
    echo -e "      ${RED}❌ Prod URL missing${NC}"
fi

if grep -q "witty-meadow-0c52c9003.2.azurestaticapps.net" "$SCRIPT_DIR/OverblikPlusBackend/TaskMicroService/Program.cs"; then
    echo -e "      ${GREEN}✅ Dev URL allowed${NC}"
else
    echo -e "      ${RED}❌ Dev URL missing${NC}"
fi

if grep -q "overblikplus.dk" "$SCRIPT_DIR/OverblikPlusBackend/TaskMicroService/Program.cs"; then
    echo -e "      ${GREEN}✅ Custom domain allowed${NC}"
else
    echo -e "      ${RED}❌ Custom domain missing${NC}"
fi

# UserMicroService CORS
echo "   UserMicroService CORS:"
if grep -q "nice-wave-08dd97903.1.azurestaticapps.net" "$SCRIPT_DIR/OverblikPlusBackend/UserMicroService/Program.cs"; then
    echo -e "      ${GREEN}✅ Prod URL allowed${NC}"
else
    echo -e "      ${RED}❌ Prod URL missing${NC}"
fi

if grep -q "witty-meadow-0c52c9003.2.azurestaticapps.net" "$SCRIPT_DIR/OverblikPlusBackend/UserMicroService/Program.cs"; then
    echo -e "      ${GREEN}✅ Dev URL allowed${NC}"
else
    echo -e "      ${RED}❌ Dev URL missing${NC}"
fi

if grep -q "overblikplus.dk" "$SCRIPT_DIR/OverblikPlusBackend/UserMicroService/Program.cs"; then
    echo -e "      ${GREEN}✅ Custom domain allowed${NC}"
else
    echo -e "      ${RED}❌ Custom domain missing${NC}"
fi

# Test 4: Check API URLs
echo ""
echo "4. Testing API URL Configuration..."

# Check prod API URLs
if grep -q "overblikplus-task-api-prod.azurewebsites.net" "$SCRIPT_DIR/OverblikPlus/Program.cs"; then
    echo -e "   ${GREEN}✅ Prod Task API URL configured${NC}"
else
    echo -e "   ${YELLOW}⚠️  Prod Task API URL not found (might not be deployed yet)${NC}"
fi

if grep -q "overblikplus-user-api-prod.azurewebsites.net" "$SCRIPT_DIR/OverblikPlus/Program.cs"; then
    echo -e "   ${GREEN}✅ Prod User API URL configured${NC}"
else
    echo -e "   ${YELLOW}⚠️  Prod User API URL not found (might not be deployed yet)${NC}"
fi

# Check dev API URLs
if grep -q "overblikplus-task-api-dev.azurewebsites.net" "$SCRIPT_DIR/OverblikPlus/Program.cs"; then
    echo -e "   ${GREEN}✅ Dev Task API URL configured${NC}"
else
    echo -e "   ${RED}❌ Dev Task API URL missing${NC}"
fi

if grep -q "overblikplus-user-api-dev.azurewebsites.net" "$SCRIPT_DIR/OverblikPlus/Program.cs"; then
    echo -e "   ${GREEN}✅ Dev User API URL configured${NC}"
else
    echo -e "   ${RED}❌ Dev User API URL missing${NC}"
fi

echo ""
echo "========================================="
echo "Configuration Test Complete!"
echo "========================================="
echo ""
echo "Next steps:"
echo "1. Deploy backend services to Azure (dev and prod)"
echo "2. Deploy frontend to Azure Static Web Apps"
echo "3. Test in browser with different URLs"
echo "4. Check console logs for environment detection"

