ARG BASE_IMAGE=buster-slim
FROM mcr.microsoft.com/dotnet/core/runtime:3.1-$BASE_IMAGE AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-$BASE_IMAGE AS build

COPY MatchProcessor ./MatchProcessor
COPY Core ./Core
COPY config.json .

RUN dotnet restore "MatchProcessor/BookieBasher.MatchProcessor.csproj"
RUN dotnet build "MatchProcessor/BookieBasher.MatchProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MatchProcessor/BookieBasher.MatchProcessor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookieBasher.MatchProcessor.dll"]