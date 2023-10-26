FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

WORKDIR /App
COPY . .

RUN dotnet restore LTres.OltApi.CLI

RUN dotnet publish -c Release -o out LTres.OltApi.CLI

#-------------- runtime

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .

ENV RABBITMQ_HOST=rabbit

ENTRYPOINT ["dotnet", "LTres.OltApi.CLI.dll"]