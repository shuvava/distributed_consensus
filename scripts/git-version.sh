#!/bin/bash

branch=$(git rev-parse --abbrev-ref HEAD)
latest=$(git tag -l --merged master --sort='-*authordate' | head -n1)

semver_parts=(${latest//./ })
major=${semver_parts[0]:=1}
major=${major//[vV]/}
minor=${semver_parts[1]:=0}
patch=${semver_parts[2]:=0}
#echo $branch $latest

if [ -z "$latest"]
then
    patch=$(git rev-list HEAD --count)
else
    patch=$(git rev-list HEAD ^${latest} --ancestry-path ${latest} --count)
fi
version="${major}.${minor}.0.${patch}"


echo ${version}
exit 0
