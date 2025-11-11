FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src
COPY app/server/ .

RUN dotnet restore
RUN dotnet build --no-restore --warnaserror
