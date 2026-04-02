#!/bin/sh

# launch-options.sh — undo Steam Runtime environment to run launch-options.py
#
# Copyright © 2017-2022 Collabora Ltd.
#
# SPDX-License-Identifier: MIT
#
# Permission is hereby granted, free of charge, to any person obtaining
# a copy of this software and associated documentation files (the
# "Software"), to deal in the Software without restriction, including
# without limitation the rights to use, copy, modify, merge, publish,
# distribute, sublicense, and/or sell copies of the Software, and to
# permit persons to whom the Software is furnished to do so, subject to
# the following conditions:
#
# The above copyright notice and this permission notice shall be included
# in all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
# EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
# MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
# IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
# CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
# TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
# SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

set -e
set -u

log () {
    echo "${me-steam-runtime-launch-options}[$$]: $*" >&2 || :
}

# run_like_game [exec] COMMAND ARGS
# Run COMMAND ARGS, but wrap it as if it was the actual game for the
# purposes of systemd resource control, Gamescope metadata and so on.
#
# If the first argument is "exec", replace the shell with the COMMAND
# instead of running it as a subprocess.
run_like_game () {
    use_exec=

    if [ "$1" = exec ]; then
        use_exec=yes
        shift
    fi

    app_id="${STEAM_COMPAT_APP_ID-${SteamAppId-}}"

    if [ -n "$app_id" ]; then
        if [ -x ~/.steam/root/ubuntu12_32/reaper ]; then
            set -- ~/.steam/root/ubuntu12_32/reaper SteamLaunch AppId="$app_id" -- "$@"
        else
            log "warning: ~/.steam/root/ubuntu12_32/reaper not found"
        fi
    else
        log "warning: Steam app ID not found in environment"
    fi

    if [ -x ~/.steam/root/ubuntu12_32/steam-launch-wrapper ]; then
        set -- ~/.steam/root/ubuntu12_32/steam-launch-wrapper -- "$@"
    else
        log "warning: ~/.steam/root/ubuntu12_32/steam-launch-wrapper not found"
    fi

    ${use_exec:+exec} "$@"
}

main () {
    me="$(readlink -f "$0")"
    here="${me%/*}"
    me="${me##*/}"

    default_path="/usr/local/sbin:/usr/sbin:/sbin:/usr/local/bin:/usr/bin:/bin:/usr/local/games:/usr/games"
    # Intentionally not STEAM_RUNTIME_SCOUT: we want the LDLP runtime
    # that we were previously running in, if any
    steam_runtime="${STEAM_RUNTIME-}"

    # Undo any weird environment before we start running external
    # executables. We put it back before running the actual app/game.

    if [ -n "${LD_LIBRARY_PATH-}" ]; then
        set -- "--steam-runtime-env=LD_LIBRARY_PATH=$LD_LIBRARY_PATH" "$@"
    fi

    if [ -n "${LD_AUDIT-}" ]; then
        set -- "--steam-runtime-env=LD_AUDIT=$LD_AUDIT" "$@"
    fi

    if [ -n "${LD_PRELOAD-}" ]; then
        set -- "--steam-runtime-env=LD_PRELOAD=$LD_PRELOAD" "$@"
    fi

    if [ -n "${PATH-}" ]; then
        set -- "--steam-runtime-env=PATH=$PATH" "$@"
    fi

    if [ -n "${STEAM_RUNTIME-}" ]; then
        set -- "--steam-runtime-env=STEAM_RUNTIME=$STEAM_RUNTIME" "$@"
    fi

    unset LD_AUDIT
    unset LD_LIBRARY_PATH
    unset LD_PRELOAD
    export PATH="$default_path"
    unset STEAM_RUNTIME

    if [ -n "${SYSTEM_LD_LIBRARY_PATH+set}" ]; then
        set -- "--steam-runtime-env=SYSTEM_LD_LIBRARY_PATH=$SYSTEM_LD_LIBRARY_PATH" "$@"
        export LD_LIBRARY_PATH="$SYSTEM_LD_LIBRARY_PATH"
    fi

    if [ -n "${STEAM_RUNTIME_LIBRARY_PATH+set}" ]; then
        set -- "--steam-runtime-env=STEAM_RUNTIME_LIBRARY_PATH=$STEAM_RUNTIME_LIBRARY_PATH" "$@"
    fi

    if [ -n "${SYSTEM_PATH+set}" ]; then
        set -- "--steam-runtime-env=SYSTEM_PATH=$SYSTEM_PATH" "$@"
        export PATH="$SYSTEM_PATH"
    fi

    if [ -n "${PRESSURE_VESSEL_APP_LD_LIBRARY_PATH+set}" ]; then
        set -- "--steam-runtime-env=PRESSURE_VESSEL_APP_LD_LIBRARY_PATH=$PRESSURE_VESSEL_APP_LD_LIBRARY_PATH" "$@"
    fi

    if [ -x "$here/../libexec/steam-runtime-tools-0/launch-options.py" ]; then
        script="$here/../libexec/steam-runtime-tools-0/launch-options.py"
    elif [ -x "$here/${me%.sh}.py" ]; then
        script="$here/${me%.sh}.py"
    else
        # This will fail, and we'll show an error message
        script="$here/../libexec/steam-runtime-tools-0/launch-options.py"
    fi

    if ! result="$("$script" --check-gui-dependencies 2>&1)"; then
        log "error: $result"

        if [ -x ~/.steam/root/steam-dialog ]; then
            if [ -e "$script" ]; then
                result="The pressure-vessel developer/debugging options menu requires Python 3, PyGI, GTK 3, and GTK 3 GObject-Introspection data.

$result"
            fi
            run_like_game ~/.steam/root/steam-dialog --error --width=500 --text="$result"
            exit 125
        fi

        if [ -z "${STEAM_ZENITY-unset}" ]; then
            # Steam sets STEAM_ZENITY to the empty string if it wants to
            # suppress use of zenity dialogs, for example on Steam Deck.
            exit 125
        fi

        result="$(printf '%s' "$result" | sed -e 's/&/\&amp;/' -e 's/</\&lt;/' -e 's/>/\&gt;/')"
        run="env"

        case "$steam_runtime" in
            (/*)
                if [ "$STEAM_ZENITY" = 'zenity' ]; then
                    # Re-enter the Steam Runtime, because STEAM_ZENITY=zenity
                    # currently means the one from the Steam Runtime's PATH,
                    # which might not work otherwise
                    run="$steam_runtime/run.sh"
                fi
                ;;
        esac

        if [ -e "$script" ]; then
            text="The pressure-vessel developer/debugging options menu requires Python 3, PyGI, GTK 3, and GTK 3 GObject-Introspection data.

    <small>$result</small>"
        else
            text="$result"
        fi

        run_like_game "$run" "${STEAM_ZENITY:-zenity}" --error --width 500 --text "$text" || :
        exit 125
    fi

    if command_line="$(run_like_game exec "$script" --command-line-fd=1 "$@")" && [ -n "$command_line" ]; then
        log "info: exec $command_line"
        eval "exec $command_line" || exit 125
    else
        log "error: $script failed or was cancelled" >&2
    fi

    exit 125
}

main "$@"

# vim:set sw=4 sts=4 et:
