ARG BASE_IMAGE=buster-slim
ARG SDK_IMAGE=buster

FROM mcr.microsoft.com/dotnet/runtime:5.0-$BASE_IMAGE AS base

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-$SDK_IMAGE AS build

COPY TeamUpdater ./TeamUpdater
COPY Core ./Core
COPY config.json .

RUN dotnet restore "TeamUpdater/BookieBasher.TeamUpdater.csproj"
RUN dotnet build "TeamUpdater/BookieBasher.TeamUpdater.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TeamUpdater/BookieBasher.TeamUpdater.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookieBasher.TeamUpdater.dll"]