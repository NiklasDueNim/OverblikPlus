# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj files and restore dependencies
COPY ["API/API.csproj", "./API/"]
COPY ["DataAccess/DataAccess.csproj", "./API/DataAccess/"]
RUN dotnet restore "./API/API.csproj"

# Copy the rest of the files and build the application
COPY . .
WORKDIR /app/API
RUN dotnet build --configuration Release --no-restore
RUN dotnet publish --configuration Release -o /app/publish

# Runtime stage - run the application in a small runtime container
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]
