ARG BASE_IMAGE=buster-slim
ARG SDK_IMAGE=buster

FROM mcr.microsoft.com/dotnet/core/runtime:3.1.6-$BASE_IMAGE AS base

WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-$SDK_IMAGE AS build

COPY StatUpdater ./StatUpdater
COPY Core ./Core
COPY config.json .

RUN dotnet restore "StatUpdater/BookieBasher.StatUpdater.csproj"
RUN dotnet build "StatUpdater/BookieBasher.StatUpdater.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StatUpdater/BookieBasher.StatUpdater.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookieBasher.StatUpdater.dll"]