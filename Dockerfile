# ============================================================
# STAGE 1: BUILD
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy .csproj files first (enables layer caching for dotnet restore)
COPY QuantityMeasurementApp.Api/QuantityMeasurementApp.Api.csproj                       QuantityMeasurementApp.Api/
COPY QuantityMeasurementAppBusinessLayer/QuantityMeasurementAppBusinessLayer.csproj     QuantityMeasurementAppBusinessLayer/
COPY QuantityMeasurementAppRepositoryLayer/QuantityMeasurementAppRepositoryLayer.csproj QuantityMeasurementAppRepositoryLayer/
COPY QuantityMeasurementAppModelLayer/QuantityMeasurementAppModelLayer.csproj           QuantityMeasurementAppModelLayer/

# Restore dependencies
RUN dotnet restore QuantityMeasurementApp.Api/QuantityMeasurementApp.Api.csproj

# Copy all source and publish
COPY . .
RUN dotnet publish QuantityMeasurementApp.Api/QuantityMeasurementApp.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ============================================================
# STAGE 2: RUNTIME
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Render provides $PORT; Program.cs reads it and binds to 0.0.0.0:$PORT
EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "QuantityMeasurementApp.Api.dll"]
