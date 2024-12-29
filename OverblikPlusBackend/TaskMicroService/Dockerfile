# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy Shared project
COPY OverblikPlus.Shared/OverblikPlus.Shared.csproj OverblikPlus.Shared/
COPY TaskMicroService/TaskMicroService.csproj TaskMicroService/

# Restore dependencies
RUN dotnet restore TaskMicroService/TaskMicroService.csproj

# Copy all source files
COPY OverblikPlus.Shared/ OverblikPlus.Shared/
COPY TaskMicroService/ TaskMicroService/

# Build and publish the app
WORKDIR /app/TaskMicroService
RUN dotnet publish -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .

# Expose port
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "TaskMicroService.dll"]