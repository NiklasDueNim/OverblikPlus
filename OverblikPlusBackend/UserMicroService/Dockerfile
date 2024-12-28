# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY UserMicroService.csproj ./
RUN dotnet restore

# Copy everything and build
COPY ./ ./
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Expose ports and environment settings
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Add healthcheck (optional)
HEALTHCHECK CMD curl --fail http://localhost:80/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "UserMicroService.dll"]
