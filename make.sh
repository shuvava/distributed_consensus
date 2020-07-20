#!/usr/bin/env bash

# set common variavles
set -a
[ -f .env ] && . .env
set +a
#set root directory
SOURCE="${BASH_SOURCE[0]}"
while [ -h "$SOURCE" ]; do # resolve $SOURCE until the file is no longer a symlink
  BASE_DIR="$( cd -P "$( dirname "$SOURCE" )" >/dev/null 2>&1 && pwd )"
  SOURCE="$(readlink "$SOURCE")"
  [[ $SOURCE != /* ]] && SOURCE="$BASE_DIR/$SOURCE" # if $SOURCE was a relative symlink, we need to resolve it relative to the path where the symlink file was located
done
BASE_DIR="$( cd -P "$( dirname "$SOURCE" )/" >/dev/null 2>&1 && pwd )"


#########################
# common helpers
usage()
{
  echo "Usage: $(basename $0) -d docker_action [optional aruments] Run docker script"
  echo "Usage: $(basename $0) -h                                  Display this help message."
}

set_variable()
{
  local varname=$1
  shift
  if [ -z "${!varname}" ]; then
    eval "$varname=\"$@\""
  else
    echo "Error: $varname already set"
    usage
    exit 1
  fi
}

run_docker()
{
    . "${BASE_DIR}/scripts/docker/make.sh"
}

#########################
# Main script starts here
unset ACTION SRV

while getopts 'dh?' opt
do
  case ${opt} in
    d)
        set_variable ACTION DOCKER
        ;;
    h)
      usage
      exit 0
      ;;
  esac
done

[ -z "$ACTION" ] && usage && exit 2
shift $((OPTIND-1))

[ "$ACTION" == "DOCKER" ] && run_docker $@
