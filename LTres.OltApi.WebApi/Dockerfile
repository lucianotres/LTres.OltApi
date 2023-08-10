FROM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build-env
WORKDIR /App
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

#-------------- runtime

FROM mcr.microsoft.com/dotnet/aspnet:8.0-preview
WORKDIR /App
COPY --from=build-env /App/out .

ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000
ENTRYPOINT ["dotnet", "LTres.OltApi.WebApi.dll"]