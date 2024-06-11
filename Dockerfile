FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish "./TechChallenge_Fase01.sln" -c Release -o publish 

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TechChallenge_Fase01.API.dll"]