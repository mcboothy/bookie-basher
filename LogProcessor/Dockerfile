ARG BASE_IMAGE=buster-slim
ARG SDK_IMAGE=buster

FROM mcr.microsoft.com/dotnet/core/runtime:3.1.6-$BASE_IMAGE AS base

WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-$SDK_IMAGE AS build

COPY LogProcessor ./LogProcessor
COPY Core ./Core
COPY config.json .

RUN dotnet restore "LogProcessor/BookieBasher.LogProcessor.csproj"
RUN dotnet build "LogProcessor/BookieBasher.LogProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LogProcessor/BookieBasher.LogProcessor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookieBasher.LogProcessor.dll"]