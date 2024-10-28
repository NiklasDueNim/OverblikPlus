# Brug .NET SDK til at bygge projektet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopier TaskMicroService csproj og gendan afhængigheder
COPY OverblikPlusBackend/TaskMicroService/TaskMicroService.csproj ./
RUN dotnet restore

# Kopier resten af TaskMicroService-koden og byg projektet
COPY OverblikPlusBackend/TaskMicroService/. .
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TaskMicroService.dll"]
