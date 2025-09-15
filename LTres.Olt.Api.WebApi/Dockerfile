FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG VERSION

WORKDIR /App
COPY ./LTres.Olt.Api.Common ./LTres.Olt.Api.Common
COPY ./LTres.Olt.Api.Communication ./LTres.Olt.Api.Communication
COPY ./LTres.Olt.Api.Core ./LTres.Olt.Api.Core
COPY ./LTres.Olt.Api.Mongo ./LTres.Olt.Api.Mongo
COPY ./LTres.Olt.Api.WebApi ./LTres.Olt.Api.WebApi

RUN dotnet restore LTres.Olt.Api.Common
RUN dotnet restore LTres.Olt.Api.Core
RUN dotnet restore LTres.Olt.Api.Mongo
RUN dotnet restore LTres.Olt.Api.WebApi

RUN dotnet publish -c Release -p Version=$VERSION -o out LTres.Olt.Api.Mongo
RUN dotnet publish -c Release -p Version=$VERSION -o out LTres.Olt.Api.WebApi

#-------------- runtime

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .

ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000
ENTRYPOINT ["dotnet", "LTres.Olt.Api.WebApi.dll"]