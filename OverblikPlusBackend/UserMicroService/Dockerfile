# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopier Shared-projektet
COPY OverblikPlusBackend/OverblikPlus.Shared/OverblikPlus.Shared.csproj ./OverblikPlus.Shared/
RUN dotnet restore ./OverblikPlus.Shared/OverblikPlus.Shared.csproj

# Kopier UserMicroService-projektet
COPY OverblikPlusBackend/UserMicroService/UserMicroService.csproj ./UserMicroService/
RUN dotnet restore ./UserMicroService/UserMicroService.csproj

# Kopier al kildekode
COPY OverblikPlusBackend/OverblikPlus.Shared ./OverblikPlus.Shared/
COPY OverblikPlusBackend/UserMicroService ./UserMicroService/

# Byg UserMicroService-projektet
WORKDIR /src/UserMicroService
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Expose ports og miljøvariabler
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Healthcheck (valgfrit)
HEALTHCHECK CMD curl --fail http://localhost:80/health || exit 1

# Start applikationen
ENTRYPOINT ["dotnet", "UserMicroService.dll"]
