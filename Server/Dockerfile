ARG BASE_IMAGE=buster-slim
ARG SDK_IMAGE=buster

FROM mcr.microsoft.com/dotnet/aspnet:5.0-$BASE_IMAGE AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-$SDK_IMAGE AS build
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