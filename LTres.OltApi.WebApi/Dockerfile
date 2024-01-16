FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG VERSION

WORKDIR /App
COPY ./LTres.OltApi.Common ./LTres.OltApi.Common
COPY ./LTres.OltApi.Communication ./LTres.OltApi.Communication
COPY ./LTres.OltApi.Core ./LTres.OltApi.Core
COPY ./LTres.OltApi.Mongo ./LTres.OltApi.Mongo
COPY ./LTres.OltApi.WebApi ./LTres.OltApi.WebApi

RUN dotnet restore LTres.OltApi.Common
RUN dotnet restore LTres.OltApi.Core
RUN dotnet restore LTres.OltApi.Mongo
RUN dotnet restore LTres.OltApi.WebApi

RUN dotnet publish -c Release -p Version=$VERSION -o out LTres.OltApi.WebApi

#-------------- runtime

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App
COPY --from=build-env /App/out .

ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000
ENTRYPOINT ["dotnet", "LTres.OltApi.WebApi.dll"]