#!/bin/bash

echo "🗄️  Starter SQL Server for lokal udvikling..."

# Tjek om SQL Server allerede kører
if docker ps | grep -q "sqlserver"; then
    echo "✅ SQL Server kører allerede"
    exit 0
fi

# Stop eksisterende container hvis den findes
if docker ps -a | grep -q "sqlserver"; then
    echo "🛑 Stopper eksisterende SQL Server container..."
    docker stop sqlserver
    docker rm sqlserver
fi

# Start SQL Server
echo "🚀 Starter SQL Server container..."
docker run -e "ACCEPT_EULA=Y" \
           -e "SA_PASSWORD=YourStrong@Passw0rd" \
           -p 1433:1433 \
           --name sqlserver \
           -d mcr.microsoft.com/mssql/server:2022-latest

# Vent på at SQL Server starter
echo "⏳ Venter på at SQL Server starter..."
sleep 10

# Test forbindelse
echo "🔍 Tester forbindelse til SQL Server..."
if docker exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1" > /dev/null 2>&1; then
    echo "✅ SQL Server er klar!"
    echo ""
    echo "📊 Database connection string:"
    echo "Server=localhost;Database=UserDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;"
    echo ""
    echo "🛑 For at stoppe: docker stop sqlserver"
    echo "🗑️  For at fjerne: docker rm sqlserver"
else
    echo "❌ SQL Server startede ikke korrekt. Tjek logs med: docker logs sqlserver"
    exit 1
fi
