# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy Shared project
COPY OverblikPlus.Shared/OverblikPlus.Shared.csproj OverblikPlus.Shared/
COPY UserMicroService/UserMicroService.csproj UserMicroService/

# Restore dependencies
RUN dotnet restore UserMicroService/UserMicroService.csproj

# Copy all source files
COPY OverblikPlus.Shared/ OverblikPlus.Shared/
COPY UserMicroService/ UserMicroService/

# Build and publish the app
WORKDIR /app/UserMicroService
RUN dotnet publish -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .

# Expose port
ENV ASPNETCORE_URLS=https://+:443;http://+:80
EXPOSE 80 443
ENTRYPOINT ["dotnet", "UserMicroService.dll"]