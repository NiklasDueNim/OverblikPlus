# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Ryd tidligere builds for at undgå adgangsfejl
RUN rm -rf /src/TaskMicroService/bin /src/TaskMicroService/obj

# Kopier Shared-projektet
COPY OverblikPlusBackend/OverblikPlus.Shared/OverblikPlus.Shared.csproj ./OverblikPlus.Shared/
WORKDIR /src/OverblikPlus.Shared
RUN dotnet restore OverblikPlus.Shared.csproj

# Kopier TaskMicroService-projektet
WORKDIR /src
COPY OverblikPlusBackend/TaskMicroService/TaskMicroService.csproj ./TaskMicroService/
WORKDIR /src/TaskMicroService
RUN dotnet restore TaskMicroService.csproj

# Kopier al kildekode
COPY OverblikPlusBackend/OverblikPlus.Shared ./OverblikPlus.Shared/
COPY OverblikPlusBackend/TaskMicroService ./TaskMicroService/

# Byg TaskMicroService-projektet
WORKDIR /src/TaskMicroService
RUN dotnet clean
RUN rm -rf bin obj
RUN dotnet publish -c Release -o /app/TaskMicroService

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/TaskMicroService .

# Expose ports og miljøvariabler
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Healthcheck (valgfrit)
HEALTHCHECK CMD curl --fail http://localhost:80/health || exit 1

# Start applikationen
ENTRYPOINT ["dotnet", "TaskMicroService.dll"]
