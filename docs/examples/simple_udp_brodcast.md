# UDP Broadcast Example

demonstration of UDP broadcast functionality of `Svv.Broadcaster` project

this example using `Svv.Application.UdpBroadcast.Api` example project

## Prerequisites

* Docker
* docker-compose

## Build locally
in repository root folder run

```shell script
dotnet restore
dotnet build
```

## build docker

```shell script
PROJECT_NAME=Svv.Application.UdpBroadcast.Api ./make.sh -d build
```

## Run locally

```sh
cd ./build/Svv.Application.UdpBroadcast.Api
dotnet Svv.Application.UdpBroadcast.Api.dll --console
```


## Run in docker
in repository root folder open bash

```shell script
./make.sh -e up udp_broadcas
```

