# Multi-stage Dockerfile for AutoComplete.Playground
# Optimized for .NET 9.0 Blazor Server deployment on Coolify

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["EasyAppDev.Blazor.AutoComplete.sln", "./"]
COPY ["samples/AutoComplete.Playground/AutoComplete.Playground.csproj", "samples/AutoComplete.Playground/"]
COPY ["src/EasyAppDev.Blazor.AutoComplete/EasyAppDev.Blazor.AutoComplete.csproj", "src/EasyAppDev.Blazor.AutoComplete/"]
COPY ["src/EasyAppDev.Blazor.AutoComplete.AI/EasyAppDev.Blazor.AutoComplete.AI.csproj", "src/EasyAppDev.Blazor.AutoComplete.AI/"]
COPY ["src/EasyAppDev.Blazor.AutoComplete.Generators/EasyAppDev.Blazor.AutoComplete.Generators.csproj", "src/EasyAppDev.Blazor.AutoComplete.Generators/"]
COPY ["Directory.Build.props", "./"]
COPY ["global.json", "./"]

# Restore dependencies
RUN dotnet restore "samples/AutoComplete.Playground/AutoComplete.Playground.csproj"

# Copy all source files
COPY . .

# Build and publish the application
WORKDIR "/src/samples/AutoComplete.Playground"
RUN dotnet publish "AutoComplete.Playground.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Copy published application
COPY --from=build --chown=appuser:appuser /app/publish .

# Expose port (Coolify will map this)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl --fail http://localhost:8080/ || exit 1

# Start the application
ENTRYPOINT ["dotnet", "AutoComplete.Playground.dll"]
