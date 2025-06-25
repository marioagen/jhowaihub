#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
ENV COMPlus_GCHeapHardLimitPercent 0x5A
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /external-api
COPY ["FileRepository.Api/FileRepository.Api.csproj", "FileRepository.Api/"]
COPY ["FileRepository.Application/FileRepository.Application.csproj", "FileRepository.Application/"]
COPY ["FileRepository.Domain/FileRepository.Domain.csproj", "FileRepository.Domain/"]
RUN dotnet restore "FileRepository.Api/FileRepository.Api.csproj"
COPY . .
WORKDIR "/external-api/FileRepository.Api"
RUN dotnet build "FileRepository.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileRepository.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileRepository.Api.dll"]