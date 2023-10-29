FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
ARG VERSION

WORKDIR /App
COPY ./LTres.OltApi.Common ./LTres.OltApi.Common
COPY ./LTres.OltApi.Core ./LTres.OltApi.Core
COPY ./LTres.OltApi.Mongo ./LTres.OltApi.Mongo
COPY ./LTres.OltApi.RabbitMQ ./LTres.OltApi.RabbitMQ
COPY ./LTres.OltApi.WorkerController ./LTres.OltApi.WorkerController

RUN dotnet restore LTres.OltApi.Common
RUN dotnet restore LTres.OltApi.Core
RUN dotnet restore LTres.OltApi.Mongo
RUN dotnet restore LTres.OltApi.RabbitMQ
RUN dotnet restore LTres.OltApi.WorkerController

RUN dotnet publish -c Release -p Version=$VERSION -o out LTres.OltApi.WorkerController

#-------------- runtime

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App
COPY --from=build-env /App/out .

ENV RABBITMQ_HOST=rabbit

ENTRYPOINT ["dotnet", "LTres.OltApi.WorkerController.dll"]