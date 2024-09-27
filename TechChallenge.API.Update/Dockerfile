FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /build
COPY .. .
RUN dotnet restore

RUN dotnet publish "./TechChallenge.API.Update/TechChallenge.API.Update.csproj" -c Release -o publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /build/publish .

EXPOSE 5003

ENTRYPOINT [ "dotnet", "TechChallenge.API.Update.dll" ]