#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["FatherBot/FatherBot.csproj", "FatherBot/"]
COPY ["FatherBotDatabase/FatherBotDatabase.csproj", "FatherBotDatabase/"]
RUN dotnet restore "FatherBot/FatherBot.csproj"
COPY . .
WORKDIR "/src/FatherBot"
RUN dotnet build "FatherBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FatherBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FatherBot.dll"]