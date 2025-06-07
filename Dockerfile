FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy just the main project file (not the solution)
COPY TrustTrade/TrustTrade/TrustTrade.csproj ./TrustTrade/TrustTrade/

# Restore dependencies for main project only
WORKDIR /app/TrustTrade/TrustTrade
RUN dotnet restore TrustTrade.csproj

# Copy everything else and build
WORKDIR /app
COPY . .
WORKDIR /app/TrustTrade/TrustTrade

# Build the main project directly (skip solution file)
RUN dotnet publish TrustTrade.csproj -c Release -o /app/out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port and configure for Railway
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

ENTRYPOINT ["dotnet", "TrustTrade.dll"]
