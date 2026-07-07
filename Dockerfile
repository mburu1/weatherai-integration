FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY WeatherAI.slnx ./
COPY Directory.Build.props ./
COPY WeatherAI.Contracts/ WeatherAI.Contracts/
COPY WeatherAI.Client/ WeatherAI.Client/
COPY WeatherAI.Api/ WeatherAI.Api/

RUN dotnet restore WeatherAI.Api/WeatherAI.Api.csproj
RUN dotnet publish WeatherAI.Api/WeatherAI.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "WeatherAI.Api.dll"]