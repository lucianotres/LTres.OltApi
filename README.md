# Introduction 
LTres OLT API it's a monitoring tool to provide access of OLT informations over REST API.

THIS PROJECT IT'S UNDER CONSTRUCTION! A lot of things are missing, and some of them are not working as expected.

Source code can be found at https://github.com/lucianotres/LTres.OltApi  
and at https://dev.azure.com/ltres/OLT%20API/ where you can find work items and tasks.

# Getting Started
All modules run at docker containers, so you just need a docker instance to build and run.

## Run the stack

At root you'll find a docker-compose file, you can just run **`docker-compose up -d`**

## Module by module

1. WebApi - Module to give the REST API access
    - to build use **`docker build -t ltres.oltapi.webapi LTres.OltApi.WebApi/.`**
    - run it for test purpose use **`docker run -d -p 5000:5000 -e ASPNETCORE_ENVIRONMENT=Development ltres.oltapi.webapi`**
    - open local url **http://localhost:5000/swagger**
