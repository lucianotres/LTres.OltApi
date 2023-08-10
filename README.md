# Introduction 
LTres OLT API it's a monitoring tool to provide access of OLT informations over REST API.

# Getting Started
All modules run at docker containers, so you just need a docker instance to build and run.

## Run the stack

At root you'll find a docker-compose file, you can just run **`docker-compose up -d`**

## Module by module

1. WebApi - Module to give the REST API access
    - to build use **`docker build -t ltres.oltapi.webapi LTres.OltApi.WebApi/.`**
    - run it for test purpose use **`docker run -d -p 5000:5000 -e ASPNETCORE_ENVIRONMENT=Development ltres.oltapi.webapi`**
    - open local url **http://localhost:5000/swagger**
