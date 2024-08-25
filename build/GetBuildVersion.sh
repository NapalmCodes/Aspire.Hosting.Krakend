function GetBuildVersion() {
    VersionString="$1"

    # Use regex to match the version components
    if [[ "$VersionString" =~ ^([0-9]+)(\.([0-9]+))?(\.([0-9]+))?(\-([0-9A-Za-z\-\.]+))?(\+([0-9]+))?$ ]]; then
        Major="${BASH_REMATCH[1]}"
        Minor="${BASH_REMATCH[3]}"
        Patch="${BASH_REMATCH[5]}"
        PreReleaseTag="${BASH_REMATCH[7]}"
        BuildRevision="${BASH_REMATCH[9]}"

        # Handle empty values
        [[ -z "$Minor" ]] && Minor="0"
        [[ -z "$Patch" ]] && Patch="0"
        [[ -z "$BuildRevision" ]] && BuildRevision="0"

        Version="${Major}.${Minor}.${Patch}"
        
        if [[ -n "$PreReleaseTag" ]]; then
            Version="${Version}-${PreReleaseTag}"
        fi

        if [[ "$BuildRevision" -ne 0 ]]; then
            Version="${Version}.${BuildRevision}"
        fi

        echo "$Version"
    else
        echo "1.0.0-build"
    fi
}
