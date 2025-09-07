FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG VERSION

WORKDIR /App
COPY ./ltres.olt.api.common ./ltres.olt.api.common
COPY ./ltres.olt.api.core ./ltres.olt.api.core
COPY ./ltres.olt.api.rabbitmq ./ltres.olt.api.rabbitmq
COPY ./ltres.olt.api.snmp ./ltres.olt.api.snmp
COPY ./ltres.olt.api.worker ./ltres.olt.api.worker

RUN dotnet restore ltres.olt.api.common
RUN dotnet restore ltres.olt.api.core
RUN dotnet restore ltres.olt.api.rabbitmq
RUN dotnet restore ltres.olt.api.snmp
RUN dotnet restore ltres.olt.api.worker

RUN dotnet publish -c Release -p Version=$VERSION -o out ltres.olt.api.worker

#-------------- runtime

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .

ENV RABBITMQ_HOST=rabbit

ENTRYPOINT ["dotnet", "ltres.olt.api.worker.dll"]