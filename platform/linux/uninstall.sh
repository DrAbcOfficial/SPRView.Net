#!/bin/sh
# Removes the XDG thumbnail provider installed by install.sh.
#
#   sudo ./uninstall.sh       system wide
#   ./uninstall.sh --user     current user
set -eu

mode=system
if [ "${1:-}" = "--user" ]; then
    mode=user
fi

if [ "$mode" = user ]; then
    prefix="$HOME/.local"
else
    prefix=/usr/local
fi

rm -f "$prefix/bin/sprview-thumbnailer"
rm -f "$prefix/share/thumbnailers/sprview.thumbnailer"
rm -f "$prefix/share/mime/packages/sprview-spr.xml"

if command -v update-mime-database >/dev/null 2>&1; then
    update-mime-database "$prefix/share/mime"
fi

echo "XDG thumbnailer for .spr files removed. Restart your file manager to refresh."
