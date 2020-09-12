ARG BASE_IMAGE=buster-slim
ARG SDK_IMAGE=buster

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-$BASE_IMAGE AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-$SDK_IMAGE AS build
WORKDIR /src

COPY Client ./Client
COPY Server ./Server
COPY Core ./Core
COPY config.json .

RUN dotnet restore "Server/BookieBasher.Server.csproj"
RUN dotnet build "Server/BookieBasher.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Server/BookieBasher.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookieBasher.Server.dll"]