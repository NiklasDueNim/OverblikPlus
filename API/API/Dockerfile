# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Kopier csproj-filerne og genskab afhængigheder
COPY ["API/API/API.csproj", "API/API/"]
COPY ["API/DataAccess/DataAccess.csproj", "API/DataAccess/"]
COPY ["API/Tests/Tests.csproj", "API/Tests/"]
COPY ["API/UserService/UserService.csproj", "API/UserService/"]



# Genskab dependencies for alle projekter
RUN dotnet restore "API/API/API.csproj" \
    && dotnet restore "API/Tests/Tests.csproj" \
    && dotnet restore "API/UserService/UserService.csproj"

# Kopier resten af applikationen
COPY . .

# Byg og publicer applikationen
WORKDIR /app/API
RUN dotnet publish --configuration Release --no-restore -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]
