get_unity_version() {
    echo $(cat $1/ProjectSettings/ProjectVersion.txt | grep m_EditorVersion: | sed 's/m_EditorVersion: //g')
}

get_unity_path() {
    local unity_version=$1
    if ${docker:-false}; then
        # When in docker mode, the version number is all we need
        echo $unity_version
    else
        local unity_path=$(find /Applications/Unity/Hub/Editor -type d -name "$unity_version*" | sort -r | head -n 1)
        echo $unity_path
    fi
}

get_build_type() {
    case "$(uname -s)" in
        Linux*)     echo "Linux" ;;
        Darwin*)    echo "macOS" ;;
        *)          echo "Unknown OS" ; exit 1 ;;
    esac

}

unity_docker() {
    local unity_version="$1"
    shift
    local executable="$1"
    shift

    docker_image="unityci/editor:ubuntu-${unity_version}-linux-il2cpp-3"
    echo "Using docker Unity $docker_image ..."
    docker run \
        --rm \
        -v "${repo_root}:/project" \
        -w /project \
        --platform linux/amd64 \
        --env unity_path="/opt/Unity/Editor" \
        --env UNITY_DOCKER=1 \
        --env UNITY_LICENSE \
        --env UNITY_SERIAL \
        --env UNITY_EMAIL \
        --env UNITY_PASSWORD \
        "$docker_image" \
        "$executable" \
        "$@"
}

unity() {
    local unity_path="$1"
    shift

    if [ "${UNITY_DOCKER:-}" = "1" ]; then
        # Activate Unity First
        activation_args=()
        [ -n "${UNITY_SERIAL:-}" ] && activation_args+=(-serial "${UNITY_SERIAL}")
        [ -n "${UNITY_EMAIL:-}" ] && activation_args+=(-username "${UNITY_EMAIL}")
        [ -n "${UNITY_PASSWORD:-}" ] && activation_args+=(-password "${UNITY_PASSWORD}")

        echo "*** ACTIVATING UNITY LICENSE ***"
        /usr/bin/xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' \
            /usr/bin/unity-editor \
            -logFile /dev/stdout \
            -batchmode \
            -nographics \
            "${activation_args[@]}"
    fi


    echo "Using Unity at $unity_path ..."
    if [ "$(uname)" == "Darwin" ]; then
        "$unity_path/Unity.app/Contents/MacOS/Unity" "$@"
    else
        "$unity_path/Unity" "$@"
    fi
}