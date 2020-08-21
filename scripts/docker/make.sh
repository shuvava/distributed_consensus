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
cd "${BASE_DIR}" || exit
##################################
# set variables ##################
: "${DOMAIN:="mrshuvava"}"
: "${APP_NAME:=$(echo ${PROJECT_NAME//\./-} | awk '{print tolower($0)}')}"
: "${ACTION:="build"}"
: "${DOCKER_REPO:="mrshuvava"}"
: "${BUILD_NUMBER:="0"}"
BUILD_NUMBER=${BUILD_NUMBER##*.}
: "${BUILD_VERSION:="1.0.0.${BUILD_NUMBER}"}"
if [ "${BUILD_VERSION}" == "1.0.0.0" ]; then
  BUILD_VERSION="$(${BASE_DIR}/scripts/git-version.sh)"
  BUILD_NUMBER=${BUILD_VERSION##*.}
fi
: "${BUILD_DATE=$(date --rfc-3339=seconds | sed 's/ /T/')}"
: "${IMAGE_NAME_BASE:="${APP_NAME}"}"
: "${IMAGE_NAME:="${IMAGE_NAME_BASE}:v${BUILD_NUMBER}"}"
actions=("build" "test" "run" "pull" "push" "clean")
CIDFILE_DIR=$(mktemp --suffix=test_cidfiles -d)
EXEC_PROJECTS=("Svv.Application.UdpBroadcast.Api")

info() {
  echo -e "\n\e[1m[INFO] $@\e[0m\n"
}
# clears containers run during the test
function cleanup() {
  CONTAINER=$(cat $cid_file)
  info "Stopping and removing container $CONTAINER..."
  docker stop $CONTAINER
  exit_status=$(docker inspect -f '{{.State.ExitCode}}' $CONTAINER)
  if [ "$exit_status" != "0" ]; then
    info "Dumping logs for $CONTAINER"
    docker logs $CONTAINER
  fi
  docker rm $CONTAINER
  rm $cid_file
  info "Done."
}

# returns IP of specified named container
function get_container_ip() {
  local id="$1" ; shift
  docker inspect --format='{{.NetworkSettings.IPAddress}}' $(cat $cid_file)
}

# start a new container
function create_container() {
  # create container with a cidfile in a directory for cleanup
  docker run --cidfile $cid_file -d $IMAGE_NAME
  echo "Created container $(cat $cid_file)"
}

check_result() {
  local result="$1"
  if [[ "$result" != "0" ]]; then
    info "TEST FAILED (${result})"
    cleanup
    exit $result
  else
    info "TEST SUCCEEDED (${result})"
  fi
}
test_connection() {
  info "Testing the HTTP connection (http://$(get_container_ip)) ${CONTAINER_ARGS} ..."
  local max_attempts=3
  local sleep_time=1
  local attempt=1
  local result=1
  while [ $attempt -le $max_attempts ]; do
    response_code=$(curl -s -w %{http_code} -o /dev/null http://$(get_container_ip)/health/liveness)
    status=$?
    if [ $status -eq 0 ]; then
      if [ $response_code -eq 200 ]; then
        result=0
      fi
      break
    fi
    attempt=$(( $attempt + 1 ))
    sleep $sleep_time
  done
  return $result
}

if [ ! -z "$1" ]; then
    ACTION=$1
    shift 1
fi
if [ ! -z "$1" ]; then
    OPTS=$@
    shift $#
fi
if [[ ! " ${actions[@]} " =~ " ${ACTION} " ]]; then
    echo "Usage: $(basename $0) build|test|run|pull|push [docker options]"
    exit 1
fi


if [ "$ACTION" = "build" ]; then
  info "${ACTION} ${IMAGE_NAME}"
  case ${APP_NAME} in
    *) DOCKER_FILE=./examples/dotnet/${PROJECT_NAME}/Dockerfile
  esac

  export BUILD_VERSION \
        BUILD_DATE
  docker build \
    -t ${IMAGE_NAME_BASE}:latest \
    -t ${IMAGE_NAME} \
    -t ${DOCKER_REPO}/${IMAGE_NAME_BASE}:latest \
    -t ${DOCKER_REPO}/${IMAGE_NAME} \
    --build-arg BUILD_VERSION \
    --build-arg BUILD_DATE \
    -f ${DOCKER_FILE} \
    $OPTS .
    info "completed"
fi

if [ "$ACTION" = "test" ]; then
  info "run tests"
  cid_file=$(mktemp -u --suffix=.cid)
  trap cleanup EXIT
  create_container ${IMAGE_NAME_BASE}
  test_connection
  check_result $?
  cleanup
fi

if [ "$ACTION" = "run" ]; then
  info "exec container"
  winpty docker run -it --rm -p ${PORT}:80 $OPTS ${IMAGE_NAME}
fi

if [ "$ACTION" = "push" ]; then
  info "push container to remote"
  #docker login ${DOCKER_REPO} --username ${DOCKER_USERNAME} --password ${DOCKER_PASSWORD}
  docker push ${DOCKER_REPO}/${IMAGE_NAME}
  docker push ${DOCKER_REPO}/${IMAGE_NAME_BASE}:latest
fi

if [ "$ACTION" = "pull" ]; then
  #docker login ${DOCKER_REPO} --username ${DOCKER_USERNAME} --password ${DOCKER_PASSWORD}
  tag=${OPTS:-latest}
  info "pull container '${IMAGE_NAME_BASE}:${tag}' from remote ${DOCKER_REPO}"
  docker pull ${DOCKER_REPO}/${IMAGE_NAME_BASE}:${tag}
  docker tag ${DOCKER_REPO}/${IMAGE_NAME_BASE}:${tag} ${IMAGE_NAME_BASE}:${tag}
fi

if [ "$ACTION" = "clean" ]; then
  ./scripts/docker/image_cleanup.sh
fi
