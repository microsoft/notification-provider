#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NotificationService/NotificationService.csproj", "NotificationService/"]
COPY ["NotificationService.Contracts/NotificationService.Contracts.csproj", "NotificationService.Contracts/"]
COPY ["NotificationService.Common/NotificationService.Common.csproj", "NotificationService.Common/"]
COPY ["NotificationService.BusinessLibrary/NotificationService.BusinessLibrary.csproj", "NotificationService.BusinessLibrary/"]
COPY ["NotificationService.Data/NotificationService.Data.csproj", "NotificationService.Data/"]
COPY ["NotificationService.SvCommon/NotificationService.SvCommon.csproj", "NotificationService.SvCommon/"]
COPY ["NotificationProviders/DirectSend.Core/DirectSend.NetCore.csproj", "NotificationProviders/DirectSend.Core/"]
COPY ["NotificationProviders/DirectSend.Shared/DirectSend.Shared.shproj", "NotificationProviders/DirectSend.Core/"]
RUN dotnet restore "NotificationService/NotificationService.csproj"
COPY . .
WORKDIR "/src/NotificationService"
RUN dotnet build "NotificationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.dll"]