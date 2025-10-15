# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY TaskMicroService/TaskMicroService.csproj .
COPY OverblikPlus.Shared/OverblikPlus.Shared.csproj .
RUN dotnet restore TaskMicroService.csproj

# Copy the rest of the source code
COPY TaskMicroService/ .
COPY OverblikPlus.Shared/ .

# Build the application
RUN dotnet build TaskMicroService.csproj -c Release -o /app/build

# Publish the application
RUN dotnet publish TaskMicroService.csproj -c Release -o /app/publish

# Use the official .NET 8 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# Expose the port the app runs on
EXPOSE 80

# Set the entry point
ENTRYPOINT ["dotnet", "TaskMicroService.dll"]