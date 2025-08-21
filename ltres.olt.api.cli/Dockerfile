FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG VERSION

WORKDIR /App
COPY . .

RUN dotnet restore ltres.olt.api.cli

RUN dotnet publish -c Release -p Version=$VERSION -o out ltres.olt.api.cli

#-------------- runtime

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .

ENTRYPOINT ["dotnet", "ltres.olt.api.cli.dll"]