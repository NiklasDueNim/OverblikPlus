# ---- Build the Blazor WebAssembly app ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY OverblikPlus.csproj .
RUN dotnet restore OverblikPlus.csproj

COPY . .
RUN dotnet publish OverblikPlus.csproj -c Release -o /app/publish

# ---- Serve the static output with nginx ----
FROM nginx:alpine AS runtime
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
