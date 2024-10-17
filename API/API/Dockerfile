# Bruger SDK som basebillede for at bygge applikationen
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY ["API/API.csproj", "./API/"]
COPY ["DataAccess/DataAccess.csproj", "./DataAccess/"]
RUN dotnet restore "./API/API.csproj"

# Kopier resten af applikationen og byg den
COPY . ./
RUN dotnet build -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
# Sørg for, at Kestrel bruger port 5165
ENV ASPNETCORE_URLS=http://+:5165
EXPOSE 5165
ENTRYPOINT ["dotnet", "API.dll"]
