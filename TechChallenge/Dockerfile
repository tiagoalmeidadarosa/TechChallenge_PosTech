FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /build
COPY .. .
RUN dotnet restore

RUN dotnet publish "./TechChallenge/TechChallenge.API.csproj" -c Release -o publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /build/publish .

EXPOSE 5001

ENTRYPOINT [ "dotnet", "TechChallenge.API.dll" ]
