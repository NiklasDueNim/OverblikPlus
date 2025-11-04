# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY OverblikPlus.csproj .
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Publish the Blazor WebAssembly app (this produces static files)
RUN dotnet publish -c Release -o /app/publish

# Use nginx to serve the static files (lightweight and perfect for static files)
FROM nginx:alpine AS runtime
WORKDIR /usr/share/nginx/html

# Remove default nginx static assets
RUN rm -rf ./*

# Copy the published static files from the build stage
COPY --from=build /app/publish/wwwroot .

# Copy nginx configuration for SPA routing support
RUN echo 'server { \
    listen 80; \
    server_name localhost; \
    root /usr/share/nginx/html; \
    index index.html; \
    location / { \
        try_files $uri $uri/ /index.html; \
    } \
}' > /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
