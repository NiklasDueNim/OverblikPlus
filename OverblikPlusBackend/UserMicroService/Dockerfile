# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopier projektfil og gendan afhængigheder
COPY UserMicroService.csproj ./
RUN dotnet restore

# Kopier kildekode og byg projektet
COPY . .
RUN dotnet publish -c Release -o /app/publish

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Kopier build output
COPY --from=build /app/publish .

# Eksponér porte
EXPOSE 80 443

# Tilføj miljøvariabler
ARG BUILD_ENV=Production
ENV ASPNETCORE_ENVIRONMENT=$BUILD_ENV
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Tilføj log-mappe
VOLUME ["/app/logs"]

# Start applikationen
ENTRYPOINT ["dotnet", "UserMicroService.dll"]
