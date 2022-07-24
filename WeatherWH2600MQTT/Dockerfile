#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_ENVIRONMENT "Production"

VOLUME /app/config /app/logs

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WeatherWH2600MQTT/WeatherWH2600MQTT.csproj", "WeatherWH2600MQTT/"]
RUN dotnet restore "WeatherWH2600MQTT/WeatherWH2600MQTT.csproj"
COPY . .
WORKDIR "/src/WeatherWH2600MQTT"
RUN dotnet build "WeatherWH2600MQTT.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WeatherWH2600MQTT.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WeatherWH2600MQTT.dll"]