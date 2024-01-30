#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Imv.Messaging.Mqtt.Dispatcher.ConsoleHost/Imv.Messaging.Mqtt.Dispatcher.ConsoleHost.csproj", "Imv.Messaging.Mqtt.Dispatcher.ConsoleHost/"]
COPY ["Imv.Messaging.Mqtt.Core/Imv.Messaging.Mqtt.Core.csproj", "Imv.Messaging.Mqtt.Core/"]
COPY ["Imv.Messaging.Mqtt.Interfaces/Imv.Messaging.Mqtt.Interfaces.csproj", "Imv.Messaging.Mqtt.Interfaces/"]
COPY ["Imv.Messaging.Mqtt.Models/Imv.Messaging.Mqtt.Models.csproj", "Imv.Messaging.Mqtt.Models/"]
RUN dotnet restore "./Imv.Messaging.Mqtt.Dispatcher.ConsoleHost/./Imv.Messaging.Mqtt.Dispatcher.ConsoleHost.csproj"
COPY . .
WORKDIR "/src/Imv.Messaging.Mqtt.Dispatcher.ConsoleHost"
RUN dotnet build "./Imv.Messaging.Mqtt.Dispatcher.ConsoleHost.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Imv.Messaging.Mqtt.Dispatcher.ConsoleHost.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Imv.Messaging.Mqtt.Dispatcher.ConsoleHost.dll"]