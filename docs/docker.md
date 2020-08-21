# Docker usage

## build image

from repo root run:

```sh
docker build -t svv-app-api -f ./examples/dotnet/Svv.Application.Api/Dockerfile .
```

## Run docker container

```sh
docker run -it --rm -p 8080:8080 --name svv-app-api-instance svv-app-api
```

## Run with custom entrypoint

```sh
docker run -it --rm -p 8080:8080 --name svv-app-api-instance --entrypoint 'bash' svv-app-api
```

## Clean up

```sh
docker container ls -al --filter name=svv-app-api-instance
docker container rm
```
