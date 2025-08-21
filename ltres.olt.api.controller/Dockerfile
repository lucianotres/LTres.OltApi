FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG VERSION

WORKDIR /App
COPY ./ltres.olt.api.common ./ltres.olt.api.common
COPY ./ltres.olt.api.core ./ltres.olt.api.core
COPY ./ltres.olt.api.mongo ./ltres.olt.api.mongo
COPY ./ltres.olt.api.rabbitmq ./ltres.olt.api.rabbitmq
COPY ./ltres.olt.api.controller ./ltres.olt.api.controller

RUN dotnet restore ltres.olt.api.common
RUN dotnet restore ltres.olt.api.core
RUN dotnet restore ltres.olt.api.mongo
RUN dotnet restore ltres.olt.api.rabbitmq
RUN dotnet restore ltres.olt.api.controller

RUN dotnet publish -c Release -p Version=$VERSION -o out ltres.olt.api.controller

#-------------- runtime

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .

ENV RABBITMQ_HOST=rabbit

ENTRYPOINT ["dotnet", "ltres.olt.api.controller.dll"]