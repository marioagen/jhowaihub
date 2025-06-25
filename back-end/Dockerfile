#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
ENV COMPlus_GCHeapHardLimitPercent 0x5A
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /back-end
COPY ["DocAnalyzer.Api/DocAnalyzer.Api.csproj", "DocAnalyzer.Api/"]
COPY ["DocAnalyzer.Repository/DocAnalyzer.Repository.csproj", "DocAnalyzer.Repository/"]
COPY ["DocAnalyzer.Domain/DocAnalyzer.Domain.csproj", "DocAnalyzer.Domain/"]
COPY ["DocAnalyzer.Application/DocAnalyzer.Application.csproj", "DocAnalyzer.Application/"]
RUN dotnet restore "DocAnalyzer.Api/DocAnalyzer.Api.csproj"
COPY . .
WORKDIR "/back-end/DocAnalyzer.Api"
RUN dotnet build "DocAnalyzer.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DocAnalyzer.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DocAnalyzer.Api.dll"]