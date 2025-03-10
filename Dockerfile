FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

COPY ./src/ ./

RUN dotnet restore KeepTrack.sln
RUN dotnet publish -c Release -o out KeepTrack.Web/KeepTrack.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .
RUN mkdir /App/Files
RUN mkdir /App/Sheet
RUN mkdir /App/Keys
RUN mkdir /App/Data

ENTRYPOINT ["dotnet", "KeepTrack.dll"]