#!/bin/sh
# Installs the command line part of the macOS thumbnail provider: the
# NativeAOT sprview-thumbnailer binary that the QuickLook extension invokes.
#
#   ./install.sh [binary]           install to /usr/local/bin (needs sudo)
#   ./install.sh --user [binary]    install to ~/.local/bin and adapt the
#                                   QuickLook extension constant
set -eu

mode=system
if [ "${1:-}" = "--user" ]; then
    mode=user
    shift
fi

binary="${1:-$(dirname "$0")/sprview-thumbnailer}"
if [ ! -x "$binary" ]; then
    echo "error: sprview-thumbnailer binary not found at: $binary" >&2
    echo "build it with: dotnet publish src/SPRView.Net.Thumbnailer -c Release -r osx-arm64" >&2
    exit 1
fi

if [ "$mode" = user ]; then
    target="$HOME/.local/bin/sprview-thumbnailer"
    mkdir -p "$HOME/.local/bin"
    install -m 0755 "$binary" "$target"
    echo "Installed $target"
    echo "NOTE: update ThumbnailProvider.swift thumbnailerPath to \"$target\""
    echo "and rebuild the QuickLook app extension in Xcode."
else
    install -m 0755 "$binary" /usr/local/bin/sprview-thumbnailer
    echo "Installed /usr/local/bin/sprview-thumbnailer"
fi
