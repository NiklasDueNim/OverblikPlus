# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files (preserving the directory layout so relative
# ProjectReferences resolve) and restore dependencies.
COPY TaskMicroService/TaskMicroService.csproj TaskMicroService/
COPY SeedData/SeedData.csproj SeedData/
COPY OverblikPlus.Shared/OverblikPlus.Shared.csproj OverblikPlus.Shared/
RUN dotnet restore TaskMicroService/TaskMicroService.csproj

# Copy the rest of the source code (same layout)
COPY TaskMicroService/ TaskMicroService/
COPY SeedData/ SeedData/
COPY OverblikPlus.Shared/ OverblikPlus.Shared/

# Publish the application
RUN dotnet publish TaskMicroService/TaskMicroService.csproj -c Release -o /app/publish

# Use the official .NET 8 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# Expose the port the app runs on
EXPOSE 80

# Set the entry point
ENTRYPOINT ["dotnet", "TaskMicroService.dll"]
