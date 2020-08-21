#!/usr/bin/env bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

cd $DIR/../..
# set common variavles ###########
set -a
[ -f .env ] && . .env
set +a

echo build docker image
./make.sh -d build
