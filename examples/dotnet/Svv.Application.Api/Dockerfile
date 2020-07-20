ARG DOTNET_VERSION=3.1
ARG DOTNET_VERSION_MINOR=201
ARG DOTNET_RUNTIME=linux-musl-x64
ARG PROJECT_NAME=Svv.Application.Api
ARG BUILD_CONFIGURATION=Release
ARG BUILD_VERSION=1.0.0.0

FROM mcr.microsoft.com/dotnet/core/sdk:${DOTNET_VERSION}.${DOTNET_VERSION_MINOR} AS base
ARG DOTNET_RUNTIME
WORKDIR /app
COPY ./*.sln ./*.config ./*.props ./
COPY ./src/dotnet/**/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p src/dotnet/${file%.*}/ && mv $file src/dotnet/${file%.*}/; done
COPY ./tests/dotnet/**/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p tests/dotnet/${file%.*}/ && mv $file tests/dotnet/${file%.*}/; done
COPY ./examples/dotnet/**/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p examples/dotnet/${file%.*}/ && mv $file examples/dotnet/${file%.*}/; done
RUN dotnet restore --configfile /app/nuget.config -r ${DOTNET_RUNTIME}

FROM mcr.microsoft.com/dotnet/core/sdk:${DOTNET_VERSION}.${DOTNET_VERSION_MINOR} AS build
ARG DOTNET_RUNTIME
ARG PROJECT_NAME
ARG BUILD_CONFIGURATION
ARG BUILD_VERSION
WORKDIR /app
COPY ./*.sln ./*.props ./
COPY ./src/dotnet ./src/dotnet/
COPY ./examples/dotnet ./examples/dotnet/
COPY --from=base app/packages ./packages/
COPY --from=base app/*.config ./
COPY --from=base app/build ./build/
RUN dotnet publish examples/dotnet/${PROJECT_NAME}/${PROJECT_NAME}.csproj \
    -c ${BUILD_CONFIGURATION} --no-restore \
    -o out -r ${DOTNET_RUNTIME} \
    -p:Version=${BUILD_VERSION} \
    --self-contained true

FROM mcr.microsoft.com/dotnet/core/runtime:3.1.4-alpine
ARG PROJECT_NAME
ARG BUILD_VERSION
ARG BUILD_DATE
LABEL name=${PROJECT_NAME} \
    version=${BUILD_VERSION} \
    build-date=${BUILD_DATE} \
    vcs-type=git \
    vcs-url="https://github.com/shuvava/distributed_consensus.git" \
    maintainer="mrshuvava"
WORKDIR /app
COPY --from=build app/out .
ENV TZ=UTC
EXPOSE 80
ENV ASPNETCORE_ENVIRONMENT=Fargate
ENV URLS="http://*:80"
ENTRYPOINT ["dotnet", "./Svv.Application.Api.dll"]
