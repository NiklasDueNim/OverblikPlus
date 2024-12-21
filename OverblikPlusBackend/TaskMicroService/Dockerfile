# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ./TaskMicroService.csproj ./
RUN dotnet restore

# Copy the rest of the source code
COPY ./ ./
RUN dotnet publish -c Release -o /app/publish

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy build output
COPY --from=build /app/publish .

# Eksponér både HTTP (80) og HTTPS (443)
EXPOSE 80 443

# Tilføj miljøvariabler
ARG BUILD_ENV=Production
ENV ASPNETCORE_ENVIRONMENT=$BUILD_ENV
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Opret mappe til logs
VOLUME ["/app/logs"]

# Start applikationen
ENTRYPOINT ["dotnet", "TaskMicroService.dll"]
