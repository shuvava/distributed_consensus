#!/usr/bin/env bash
##################################
# set common variavles ###########
set -a
[ -f .env ] && . .env
set +a
##################################
#set root directory ##############
SOURCE="${BASH_SOURCE[0]}"
while [ -h "$SOURCE" ]; do # resolve $SOURCE until the file is no longer a symlink
  BASE_DIR="$( cd -P "$( dirname "$SOURCE" )" >/dev/null 2>&1 && pwd )"
  SOURCE="$(readlink "$SOURCE")"
  [[ $SOURCE != /* ]] && SOURCE="$BASE_DIR/$SOURCE" # if $SOURCE was a relative symlink, we need to resolve it relative to the path where the symlink file was located
done
BASE_DIR="$( cd -P "$( dirname "$SOURCE" )/../.." >/dev/null 2>&1 && pwd )"
cd "${BASE_DIR}" || exit 1
##################################
# set variables ##################
: "${DC_CMD:="config"}"
if [ -n "$1" ]; then
    DC_CMD=$1
    shift 1
fi

if [ -n "$1" ]; then
    SRV=$1
    shift 1
fi

EXE_DIR="${BASE_DIR}/scripts/examples/${SRV}"
if [ -z "${DC_CMD}" ] || [ -z "${SRV}" ] || [ ! -d "${EXE_DIR}" ]; then
    if [ ! -d "${EXE_DIR}" ]; then
        cd "${BASE_DIR}/scripts/examples" || exit 1
        echo "service ${SRV} did not found available services:"
        for i in $(ls -d */); do echo "     ${i%%/}"; done
    fi
    echo "Usage: $(basename $0) up|down|build|config service_name [docker-compose options]"
    exit 1
fi

info() {
  echo -e "\n\e[1m[INFO] $@\e[0m\n"
}

cat ./examples/dotnet/Svv.Application.UdpBroadcast.Api/Dockerfile
info "docker-compose ${DC_CMD} ${EXE_DIR}/docker-compose.yml"
docker-compose -f "${EXE_DIR}/docker-compose.yml"  ${DC_CMD}
