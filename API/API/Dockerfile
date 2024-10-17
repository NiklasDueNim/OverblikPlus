# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Kopier csproj-filerne og genskab afhængigheder
COPY ["API/API/API.csproj", "API/API/"]
COPY ["API/DataAccess/DataAccess.csproj", "API/DataAccess/"]

# Genskab dependencies
RUN dotnet restore "API/API/API.csproj"

# Kopier resten af applikationen og byg den
COPY . ./
RUN dotnet build -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Sørg for, at artefakterne bliver kopieret korrekt fra build-stadiet
COPY --from=build /app/out . 

# Sørg for, at Kestrel bruger port 5165
ENV ASPNETCORE_URLS=http://+:5165
EXPOSE 5165
ENTRYPOINT ["dotnet", "API.dll"]
