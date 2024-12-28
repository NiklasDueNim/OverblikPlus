FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Kopier solution-filen (en mappe op)
COPY ../OverblikPlusBackend.sln .

# Kopier .csproj-filer
COPY ../OverblikPlus.Shared/OverblikPlus.Shared.csproj OverblikPlus.Shared/
COPY ./TaskMicroService.csproj TaskMicroService.csproj

# Restore
RUN dotnet restore OverblikPlusBackend.sln

# Kopier resten af koden
COPY ../OverblikPlus.Shared/ OverblikPlus.Shared/
COPY . .

# Publish
RUN dotnet publish TaskMicroService.csproj -c Release -o /out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "TaskMicroService.dll"]