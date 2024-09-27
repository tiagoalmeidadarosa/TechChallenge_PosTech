FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /build
COPY .. .
RUN dotnet restore

RUN dotnet publish "./TechChallenge.API.Register/TechChallenge.API.Register.csproj" -c Release -o publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /build/publish .

EXPOSE 5004

ENTRYPOINT [ "dotnet", "TechChallenge.API.Register.dll" ]