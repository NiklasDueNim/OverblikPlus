# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Kopier csproj-filerne og genskab afhængigheder
# Change 1: Correct the path to DataAccess.csproj (absolute path based on your folder structure)
COPY ["API/API/API.csproj", "./API/"]
COPY ["API/DataAccess/DataAccess.csproj", "API/DataAccess/"]

# Genskab dependencies
RUN dotnet restore "./API/API.csproj"

# Kopier resten af projektfilerne og byg applikationen
COPY . .
WORKDIR /app/API
RUN dotnet build --configuration Release --no-restore
RUN dotnet publish --configuration Release -o /app/publish

# Runtime stage - kør applikationen i en lille runtime container
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]
