FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG VERSION

WORKDIR /App
COPY ./LTres.Olt.Api.Common ./LTres.Olt.Api.Common
COPY ./LTres.Olt.Api.Core ./LTres.Olt.Api.Core
COPY ./LTres.Olt.Api.Mongo ./LTres.Olt.Api.Mongo
COPY ./LTres.Olt.Api.RabbitMQ ./LTres.Olt.Api.RabbitMQ
COPY ./LTres.Olt.Api.Controller ./LTres.Olt.Api.Controller

RUN dotnet restore LTres.Olt.Api.Common
RUN dotnet restore LTres.Olt.Api.Core
RUN dotnet restore LTres.Olt.Api.Mongo
RUN dotnet restore LTres.Olt.Api.RabbitMQ
RUN dotnet restore LTres.Olt.Api.Controller

RUN dotnet publish -c Release -p Version=$VERSION -o out LTres.Olt.Api.Controller

#-------------- runtime

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .

ENV RABBITMQ_HOST=rabbit

ENTRYPOINT ["dotnet", "LTres.Olt.Api.Controller.dll"]