FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY TrustTrade/TrustTrade/TrustTrade.sln ./TrustTrade/TrustTrade/
COPY TrustTrade/TrustTrade/TrustTrade.csproj ./TrustTrade/TrustTrade/
COPY TrustTrade/TestTrustTrade/TestTrustTrade.csproj ./TrustTrade/TestTrustTrade/

# Restore dependencies
WORKDIR /app/TrustTrade/TrustTrade
RUN dotnet restore

# Copy everything else and build
WORKDIR /app
COPY . .
WORKDIR /app/TrustTrade/TrustTrade
RUN dotnet publish -c Release -o /app/out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TrustTrade.dll"]
