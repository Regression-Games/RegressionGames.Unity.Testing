get_unity_version() {
    if [ ! -f "$1/ProjectSettings/ProjectVersion.txt" ]; then
        echo "error: $1/ProjectSettings/ProjectVersion.txt not found" >&2
        exit 1
    fi
    echo $(cat "$1/ProjectSettings/ProjectVersion.txt" | grep m_EditorVersion: | sed 's/m_EditorVersion: //g')
}

get_unity_path() {
    local unity_version=$1
    local unity_path=$(find /Applications/Unity/Hub/Editor -type d -name "$unity_version*" | sort -r | head -n 1)
    echo $unity_path
}

get_build_type() {
    case "$(uname -s)" in
        Linux*)     echo "Linux" ;;
        Darwin*)    echo "macOS" ;;
        *)          echo "Unknown OS" ; exit 1 ;;
    esac

}

unity() {
    local unity_path="$1"
    shift
    "$unity_path/Unity.app/Contents/MacOS/Unity" "$@"
}