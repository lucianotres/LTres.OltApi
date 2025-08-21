FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG VERSION

WORKDIR /App
COPY ./ltres.olt.api.common ./ltres.olt.api.common
COPY ./ltres.olt.api.communication ./ltres.olt.api.communication
COPY ./ltres.olt.api.core ./ltres.olt.api.core
COPY ./ltres.olt.api.mongo ./ltres.olt.api.mongo
COPY ./ltres.olt.api.webapi ./ltres.olt.api.webapi

RUN dotnet restore ltres.olt.api.common
RUN dotnet restore ltres.olt.api.core
RUN dotnet restore ltres.olt.api.mongo
RUN dotnet restore ltres.olt.api.webapi

RUN dotnet publish -c Release -p Version=$VERSION -o out ltres.olt.api.webapi

#-------------- runtime

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .

ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000
ENTRYPOINT ["dotnet", "ltres.olt.api.webapi.dll"]