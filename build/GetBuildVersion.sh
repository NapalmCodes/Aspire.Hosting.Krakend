#!/usr/bin/env bash

VersionString=$1

# Use regex to match the version components
if [[ $VersionString =~ ^([0-9]+)\.([0-9]+)\.([0-9]+)? ]]; then
    Major="${BASH_REMATCH[1]}"
    Minor="${BASH_REMATCH[2]}"
    Patch="${BASH_REMATCH[3]}"
    tags=(${VersionString//-/ })
    PreReleaseTag=${tags[1]}

    Version="${Major}.${Minor}.${Patch}"

    if [[ -n "$PreReleaseTag" ]]; then
        Version="${Version}-${PreReleaseTag}"
    fi

    echo "$Version"
else
    echo "Error: Version string '$VersionString' does not match the required format." >&2
    exit 1
fi
