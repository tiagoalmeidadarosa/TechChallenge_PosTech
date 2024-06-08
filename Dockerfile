FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TechChallenge_Fase01/TechChallenge_Fase01.API.csproj", "TechChallenge_Fase01/"]
COPY ["TechChallenge_Fase01.Infrastructure/TechChallenge_Fase01.Infrastructure.csproj", "TechChallenge_Fase01.Infrastructure/"]
COPY ["TechChallenge_Fase01.Core/TechChallenge_Fase01.Core.csproj", "TechChallenge_Fase01.Core/"]
RUN dotnet restore "./TechChallenge_Fase01/TechChallenge_Fase01.API.csproj"
COPY . .
WORKDIR "/src/TechChallenge_Fase01"
RUN dotnet build "./TechChallenge_Fase01.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TechChallenge_Fase01.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TechChallenge_Fase01.API.dll"]