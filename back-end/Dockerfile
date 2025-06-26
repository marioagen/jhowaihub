#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ENV COMPlus_GCHeapHardLimitPercent 0x5A
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /back-end
COPY ["WoopiAiHub.Api/WoopiAiHub.Api.csproj", "WoopiAiHub.Api/"]
COPY ["WoopiAiHub.Repository/WoopiAiHub.Repository.csproj", "WoopiAiHub.Repository/"]
COPY ["WoopiAiHub.Domain/WoopiAiHub.Domain.csproj", "WoopiAiHub.Domain/"]
COPY ["WoopiAiHub.Application/WoopiAiHub.Application.csproj", "WoopiAiHub.Application/"]
RUN dotnet restore "WoopiAiHub.Api/WoopiAiHub.Api.csproj"
COPY . .
WORKDIR "/back-end/WoopiAiHub.Api"
RUN dotnet build "WoopiAiHub.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WoopiAiHub.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WoopiAiHub.Api.dll"]
