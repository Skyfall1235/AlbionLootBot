# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# create the data folder and give the non-root user ownership
RUN mkdir -p /data && chown $APP_UID /data
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AlbionLootBot.csproj", "."]
RUN dotnet restore "./AlbionLootBot.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./AlbionLootBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AlbionLootBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV DB_PATH=/data/lootbot.db
ENV DATA_DIR=/data/json
VOLUME /data
ENTRYPOINT ["dotnet", "AlbionLootBot.dll"]