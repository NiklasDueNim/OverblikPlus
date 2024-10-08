FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev
WORKDIR /app

# Kopier begge csproj filer og genskab afhængigheder
COPY ["API/API.csproj", "API/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
RUN dotnet restore "API/API.csproj"

# Kopier resten af koden
COPY . .

# Start applikationen med dotnet watch for at understøtte hot-reloading
WORKDIR /app/API
CMD ["dotnet", "watch", "run"]
