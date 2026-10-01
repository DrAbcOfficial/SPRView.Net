#!/bin/sh
# Installs the XDG thumbnail provider for .spr files.
#
#   sudo ./install.sh [binary]              system wide (/usr/local)
#   ./install.sh --user [binary]            current user (~/.local)
#
# [binary] defaults to a NativeAOT sprview-thumbnailer next to this script.
set -eu

mode=system
if [ "${1:-}" = "--user" ]; then
    mode=user
    shift
fi

binary="${1:-$(dirname "$0")/sprview-thumbnailer}"
if [ ! -x "$binary" ]; then
    echo "error: sprview-thumbnailer binary not found at: $binary" >&2
    echo "build it with: dotnet publish src/SPRView.Net.Thumbnailer -c Release -r linux-x64" >&2
    exit 1
fi

here="$(cd "$(dirname "$0")" && pwd)"

if [ "$mode" = user ]; then
    prefix="$HOME/.local"
else
    prefix=/usr/local
fi

bindir="$prefix/bin"
datadir="$prefix/share"

install -D -m 0755 "$binary" "$bindir/sprview-thumbnailer"
install -D -m 0644 "$here/sprview.thumbnailer" "$datadir/thumbnailers/sprview.thumbnailer"
install -D -m 0644 "$here/spr-mime.xml" "$datadir/mime/packages/sprview-spr.xml"

if command -v update-mime-database >/dev/null 2>&1; then
    update-mime-database "$datadir/mime"
fi

echo "Installed XDG thumbnailer for .spr files:"
echo "  binary:      $bindir/sprview-thumbnailer"
echo "  thumbnailer: $datadir/thumbnailers/sprview.thumbnailer"
echo "  mime type:   application/x-spr"
echo "Restart your file manager (e.g. 'nautilus -q') to pick the new thumbnailer up."
