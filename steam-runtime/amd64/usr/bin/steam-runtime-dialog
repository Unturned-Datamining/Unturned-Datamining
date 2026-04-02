#!/bin/sh
# Copyright 2024 Collabora Ltd.
# SPDX-License-Identifier: MIT

set -eu

me=steam-runtime-dialog

log () {
    echo "${me}[$$]: $*" >&2 || :
}

debug () {
    if [ -n "${STEAM_RUNTIME_VERBOSE-}" ]; then
        log "$*"
    fi
}

usage () {
    code="$1"
    shift

    if [ "$code" -ne 0 ]; then
        exec >&2
    fi

    cat <<EOF
Usage:
$me MODE [OPTIONS...]
$me MODE --check-features='message progress'

Modes:
--error --text=TEXT
--warning --text=TEXT
--info --text=TEXT
--progress [--text=TEXT] [--percentage=0] [--pulsate]

Options:
--auto-close
--height=480
--no-cancel
--no-wrap
--percentage=0
--pulsate
--title=TEXT
--text=TEXT
--verbose|-v (repeatable)
--width=640
EOF
    exit "${code}"
}

check_features () {
    IFS="$(printf ' \t\n')"

    for feature in $1; do
        case "$feature" in
            (message | progress)
                debug "have feature: $feature"
                ;;

            (*)
                debug "missing feature: $feature"
                return 1
                ;;
        esac
    done
}

exec_program () {
    mode="$1"
    shift

    if [ "$mode" = --check-features ]; then
        # We've checked that the program is available (possibly by running it
        # with --check-features), no need to repeat that process
        exit 0
    fi

    exec "$@"
    # not reached
    exit 255
}

xterm_fallback () {
    mode="$1"
    title="$2"
    text="$3"

    case "$mode" in
        (--error)
            [ -n "$title" ] || title="Error"
            ;;

        (--warning)
            [ -n "$title" ] || title="Warning"
            ;;

        (*)
            [ -n "$title" ] || title="Notice"
            ;;
    esac

    tempfile="$(mktemp)"
    printf '%s\n' "$text" > "$tempfile"
    ret=0

    # Intentionally not expanding $1 in the sh -c script at this level:
    # shellcheck disable=SC2016
    xterm -T "$title" -e \
    sh -c 'cat "$1"; printf "\\nPress Enter to continue:"; read INPUT' sh "$tempfile" \
    || ret=$?

    rm -f "$tempfile"
    return "$ret"
}

detect_scout_ldlp () {
    for dir in "${STEAM_RUNTIME_SCOUT-}" "${1-}" ~/.steam/root/ubuntu12_32/steam-runtime; do
        if [ -n "$dir" ] && [ -x "$dir/amd64/usr/bin/zenity" ]; then
            echo "$dir"
            return 0
        fi
    done

    return 1
}

escape_ldlp_runtime () {
    case "${STEAM_RUNTIME-}" in
        (/*)
            echo "$STEAM_RUNTIME"
            ;;

        (*)
            return 0
            ;;
    esac

    unset LD_AUDIT
    unset LD_LIBRARY_PATH
    unset LD_PRELOAD
    export PATH="/usr/local/sbin:/usr/sbin:/sbin:/usr/local/bin:/usr/bin:/bin:/usr/local/games:/usr/games"
    unset STEAM_RUNTIME

    if [ -n "${SYSTEM_LD_LIBRARY_PATH+set}" ]; then
        export LD_LIBRARY_PATH="$SYSTEM_LD_LIBRARY_PATH"
    fi

    if [ -n "${SYSTEM_LD_PRELOAD+set}" ]; then
        export LD_PRELOAD="$SYSTEM_LD_PRELOAD"
    fi

    if [ -n "${SYSTEM_PATH+set}" ]; then
        export PATH="$SYSTEM_PATH"
    fi
}

main () {
    # Command-line options
    auto_close=
    cancel=yes
    height=
    mode=
    percentage=
    pulsate=
    text=
    title=
    verbose=
    width=
    wrap=yes

    # Set to empty if we are asked for a feature that yad/zenity cannot do
    can_use_yad=yes
    can_use_zenity=yes

    # Append space-separated features if we are asked for a feature that
    # is not part of the basic set
    features=message

    ldlp_runtime="$(escape_ldlp_runtime)"

    getopt_temp=help
    getopt_temp="${getopt_temp},auto-close"
    getopt_temp="${getopt_temp},check-features:"
    getopt_temp="${getopt_temp},error"
    getopt_temp="${getopt_temp},height:"
    getopt_temp="${getopt_temp},info"
    getopt_temp="${getopt_temp},no-cancel"
    getopt_temp="${getopt_temp},no-wrap"
    getopt_temp="${getopt_temp},percentage:"
    getopt_temp="${getopt_temp},progress"
    getopt_temp="${getopt_temp},pulsate"
    getopt_temp="${getopt_temp},text:"
    getopt_temp="${getopt_temp},title:"
    getopt_temp="${getopt_temp},verbose"
    getopt_temp="${getopt_temp},warning"
    getopt_temp="${getopt_temp},width:"

    getopt_temp="$(getopt -o 'v' --long "$getopt_temp" -n "$me" -- "$@")"
    eval "set -- $getopt_temp"
    unset getopt_temp

    while [ "$#" -gt 0 ]; do
        case "$1" in
            (--help)
                usage 0
                # not reached
                ;;

            (--auto-close)
                auto_close=yes
                shift
                ;;

            (--check-features)
                if ! check_features "$2"; then
                    return 255
                fi
                mode="$1"
                shift 2
                ;;

            (--error|--info|--warning)
                mode="$1"
                shift
                ;;

            (--height)
                height="$2"
                shift 2
                ;;

            (--no-cancel)
                cancel=
                shift
                ;;

            (--no-wrap)
                wrap=
                shift
                ;;

            (--percentage)
                percentage="$2"
                shift 2
                ;;

            (--pulsate)
                pulsate=yes
                shift
                ;;

            (--progress)
                mode="$1"
                features="$features progress"
                shift
                ;;

            (--text)
                text="$2"
                shift 2
                ;;

            (--title)
                title="$2"
                shift 2
                ;;

            (--verbose | -v)
                verbose="${verbose:+"$verbose "}-v"
                STEAM_RUNTIME_VERBOSE=1
                shift
                ;;

            (--width)
                width="$2"
                shift 2
                ;;

            (--)
                shift
                break
                ;;

            (-*)
                log "Unknown option: $1"
                usage 255
                # not reached
                ;;

            (*)
                break
                ;;
        esac
    done

    if [ "$#" -gt 0 ]; then
        log "Positional parameters are not supported"
        usage 255
    fi

    if [ -z "$mode" ]; then
        log "Must specify a mode"
        usage 255
    fi

    for arg in \
        "--auto-close:$auto_close" \
        "--pulsate:$pulsate" \
        "--percentage:$percentage" \
    ; do
        if [ -n "${arg#*:}" ] && [ "$mode" != --progress ]; then
            log "${arg%%:*} can only be used with --progress"
            return 255
        fi
    done

    if [ -z "$wrap" ]; then
        no_wrap=--no-wrap
    else
        no_wrap=
    fi

    if [ -z "$cancel" ]; then
        no_cancel=--no-cancel
    else
        no_cancel=
    fi

    set -- \
        "$mode" \
        ${auto_close:+--auto-close} \
        ${height:+--height="$height"} \
        ${no_cancel:+--no-cancel} \
        ${no_wrap:+--no-wrap} \
        ${percentage:+--percentage="$percentage"} \
        ${pulsate:+--pulsate} \
        ${text:+--text="$text"} \
        ${title:+--title="$title"} \
        ${width:+--width="$width"} \
        ${NULL+}

    # For testing only.
    if [ -n "${STEAM_RUNTIME_DIALOG__FORCE_XTERM-}" ]; then
        debug "Falling back to xterm"
        xterm_fallback "$mode" "$title" "$text"
        return 0
    fi

    if [ -n "${STEAM_RUNTIME_DIALOG_UI-}" ] \
        && "$STEAM_RUNTIME_DIALOG_UI" --check-features="$features" \
    ; then
        debug "Using \$STEAM_RUNTIME_DIALOG_UI: $STEAM_RUNTIME_DIALOG_UI"
        # Intentionally word-splitting $verbose:
        # shellcheck disable=SC2086
        exec_program "$mode" "$STEAM_RUNTIME_DIALOG_UI" "$@" $verbose
    fi

    # Container runtime may contain an implementation, targeting the
    # container runtime
    if grep -F 'ID=steamrt' /usr/lib/os-release >/dev/null 2>/dev/null; then
        debug "Container runtime detected"
        is_slr=true
        # We assume that if Steam ships its own UI, it limits itself to
        # the intersection of what's in scout and what's in later runtimes
        # (notably avoiding libcurl), therefore will be runnable in any
        # later runtime
        for program in \
            /usr/bin/steam-dialog-ui \
            ~/.steam/root/ubuntu12_64/steam-dialog-ui \
            ~/.steam/root/ubuntu12_32/steam-dialog-ui \
        ; do
            if [ -x "$program" ] \
                && "$program" --check-features="$features" \
            ; then
                debug "Using $program from container runtime or Steam"
                # shellcheck disable=SC2086
                exec_program "$mode" "$program" "$@" $verbose
            fi
        done
    else
        is_slr=
    fi

    # Steam or the LD_LIBRARY_PATH scout runtime may contain an implementation,
    # targeting the LD_LIBRARY_PATH scout runtime (therefore we must use
    # run.sh to run it)
    if [ -z "$is_slr" ] \
        && ldlp_runtime=$(detect_scout_ldlp "$ldlp_runtime") \
    ; then
        debug "LDLP runtime detected at $ldlp_runtime"

        for program in \
            ~/.steam/root/ubuntu12_64/steam-dialog-ui \
            ~/.steam/root/ubuntu12_32/steam-dialog-ui \
            "$ldlp_runtime/usr/bin/steam-dialog-ui" \
            "$ldlp_runtime/amd64/usr/bin/steam-dialog-ui" \
            "$ldlp_runtime/i386/usr/bin/steam-dialog-ui" \
        ; do
            if [ -x "$program" ] \
                && "$ldlp_runtime/run.sh" "$program" --check-features="$features" \
            ; then
                debug "Using $program from Steam or LDLP runtime"
                # shellcheck disable=SC2086
                exec_program "$mode" "$ldlp_runtime/run.sh" "$program" "$@" $verbose
            fi
        done
    fi

    # Host OS environment may contain an implementation, targeting the
    # host OS environment
    if [ -z "$is_slr" ] \
        && command -v steam-dialog-ui >/dev/null \
        && steam-dialog-ui --check-features="$features" \
    ; then
        debug "Using steam-dialog-ui from host OS"
        # shellcheck disable=SC2086
        exec_program "$mode" steam-dialog-ui "$@" $verbose
    fi

    # If we're running under gamescope, don't try running zenity or yad;
    # implement --progress as "discard all progress" and everything else
    # as "just log a message"
    if [ "${XDG_CURRENT_DESKTOP-}" = gamescope ] \
        || [ -z "${STEAM_ZENITY-unset}" ] \
    ; then
        case "$mode" in
            (--progress)
                debug "Falling back to discarding progress notifications"
                cat > /dev/null
                return 0
                ;;

            (*)
                debug "Falling back to logging message"
                log "$mode: $title: $text"
                return 0
                ;;
        esac
    fi

    # Host OS environment may contain zenity or yad
    if command -v "${STEAM_ZENITY-zenity}" >/dev/null && [ -n "$can_use_zenity" ]; then
        debug "Falling back to zenity from host"
        exec_program "$mode" "${STEAM_ZENITY-zenity}" --no-markup "$@"
    fi
    if command -v yad >/dev/null && [ -n "$can_use_yad" ]; then
        debug "Falling back to yad from host"
        exec_program "$mode" yad "$@"
    fi

    # LD_LIBRARY_PATH runtime may contain an old version of zenity
    if [ -z "$is_slr" ] \
        && [ -n "${ldlp_runtime-}" ] \
        && [ -n "$can_use_zenity" ] \
    ; then
        debug "Falling back to zenity from LDLP runtime"
        exec_program "$mode" "$ldlp_runtime/run.sh" --no-markup zenity "$@"
    fi

    # A fallback implementation of --error etc. is to pop up an xterm
    if command -v xterm >/dev/null && [ "$features" = message ]; then
        debug "Falling back to xterm"
        xterm_fallback "$mode" "$title" "$text"
        return 0
    fi

    # A fallback implementation of --progress is to read stdout until EOF,
    # and then exit
    case "$1" in
        (--progress)
            debug "Falling back to discarding progress notifications"
            cat > /dev/null
            return 0
            ;;
    esac

    debug "No suitable implementation found"
    return 255
}

main "$@"

# vim:set sw=4 sts=4 et:
