FROM mcr.microsoft.com/dotnet/sdk:9.0 AS test

WORKDIR /src
COPY . .

WORKDIR /src/app/server/API.UnitTests

ENTRYPOINT ["dotnet", "test"]
