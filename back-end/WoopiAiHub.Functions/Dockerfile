# Build stage
# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# WORKDIR /src

# Copy csproj files in dependency order: Domain -> Repository/Infrastructure -> Application -> Functions
# COPY ["WoopiAiHub.Domain/WoopiAiHub.Domain.csproj", "WoopiAiHub.Domain/"]
# COPY ["WoopiAiHub.Repository/WoopiAiHub.Repository.csproj", "WoopiAiHub.Repository/"]
# COPY ["WoopiAiHub.Infrastructure/WoopiAiHub.Infrastructure.csproj", "WoopiAiHub.Infrastructure/"]
# COPY ["WoopiAiHub.Application/WoopiAiHub.Application.csproj", "WoopiAiHub.Application/"]
# COPY ["WoopiAiHub.Functions/WoopiAiHub.Functions.csproj", "WoopiAiHub.Functions/"]

# Restore dependencies
# RUN dotnet restore "WoopiAiHub.Functions/WoopiAiHub.Functions.csproj"

# Copy all source code
# COPY . .

# Build and publish
# WORKDIR "/src/WoopiAiHub.Functions"
# RUN dotnet build "WoopiAiHub.Functions.csproj" -c Release -o /app/build
# RUN dotnet publish "WoopiAiHub.Functions.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
# FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0
# FROM mcr.microsoft.com/azure-functions/dotnet:4WORKDIR /home/site/wwwroot

# Copy published files
# COPY --from=build /app/publish .

# Environment variables
# ENV FUNCTIONS_WORKER_RUNTIME=dotnet-isolated \
    # AzureWebJobsScriptRoot=/home/site/wwwroot \
    # AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    # ASPNETCORE_ENVIRONMENT=Production

# EXPOSE 80



# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files in dependency order: Domain -> Repository/Infrastructure -> Application -> Functions
COPY back-end/WoopiAiHub.Domain/WoopiAiHub.Domain.csproj back-end/WoopiAiHub.Domain/
COPY back-end/WoopiAiHub.Repository/WoopiAiHub.Repository.csproj back-end/WoopiAiHub.Repository/
COPY back-end/WoopiAiHub.Infrastructure/WoopiAiHub.Infrastructure.csproj back-end/WoopiAiHub.Infrastructure/
COPY back-end/WoopiAiHub.Application/WoopiAiHub.Application.csproj back-end/WoopiAiHub.Application/
COPY back-end/WoopiAiHub.Functions/WoopiAiHub.Functions.csproj back-end/WoopiAiHub.Functions/

# Restore dependencies
RUN dotnet restore "back-end/WoopiAiHub.Functions/WoopiAiHub.Functions.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/back-end/WoopiAiHub.Functions"
RUN dotnet build "WoopiAiHub.Functions.csproj" -c Release -o /app/build
RUN dotnet publish "WoopiAiHub.Functions.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0
# FROM mcr.microsoft.com/azure-functions/dotnet:4
WORKDIR /home/site/wwwroot

# Copy published files
COPY --from=build /app/publish .

# Environment variables
ENV FUNCTIONS_WORKER_RUNTIME=dotnet-isolated \
    AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80
