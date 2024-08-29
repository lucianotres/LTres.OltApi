FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG VERSION

WORKDIR /App
COPY ./LTres.OltApi.Common ./LTres.OltApi.Common
COPY ./LTres.OltApi.Core ./LTres.OltApi.Core
COPY ./LTres.OltApi.RabbitMQ ./LTres.OltApi.RabbitMQ
COPY ./LTres.OltApi.Snmp ./LTres.OltApi.Snmp
COPY ./LTres.OltApi.Worker ./LTres.OltApi.Worker

RUN dotnet restore LTres.OltApi.Common
RUN dotnet restore LTres.OltApi.Core
RUN dotnet restore LTres.OltApi.RabbitMQ
RUN dotnet restore LTres.OltApi.Snmp
RUN dotnet restore LTres.OltApi.Worker

RUN dotnet publish -c Release -p Version=$VERSION -o out LTres.OltApi.Worker

#-------------- runtime

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .

ENV RABBITMQ_HOST=rabbit

ENTRYPOINT ["dotnet", "LTres.OltApi.Worker.dll"]