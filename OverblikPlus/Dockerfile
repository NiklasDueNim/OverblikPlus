# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Kopier csproj og genskab afhængigheder
COPY *.sln ./
COPY OverblikPlus.csproj ./
RUN dotnet restore "./OverblikPlus.csproj"

# Byg og publicer applikationen
COPY . . 
WORKDIR /app
RUN dotnet publish -c Release -o /app/publish

# Runtime stage - brug NGINX som webserver til at serve frontenden
FROM nginx:alpine AS runtime
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
CMD ["nginx", "-g", "daemon off;"]
