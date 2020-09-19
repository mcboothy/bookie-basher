ARG BASE_IMAGE=buster-slim
ARG SDK_IMAGE=buster

FROM mcr.microsoft.com/dotnet/core/runtime:3.1.6-$BASE_IMAGE AS base

WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-$SDK_IMAGE AS build

COPY ErrorHandler ./ErrorHandler
COPY Core ./Core
COPY config.json .

RUN dotnet restore "ErrorHandler/BookieBasher.ErrorHandler.csproj"
RUN dotnet build "ErrorHandler/BookieBasher.ErrorHandler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ErrorHandler/BookieBasher.ErrorHandler.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookieBasher.ErrorHandler.dll"]