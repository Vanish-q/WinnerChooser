#!/usr/bin/env bash
# Печатает секцию CHANGELOG.md для указанной версии без строки-заголовка.
# Используется в CI, чтобы описание релиза было написано руками, а не сгенерировано.
#
#   scripts/changelog-section.sh v1.0.0 [CHANGELOG.md]
#
# Код возврата 1 — секции нет или она пустая.

set -euo pipefail

if [ "$#" -lt 1 ]; then
    echo "usage: $0 <version|vX.Y.Z> [CHANGELOG.md]" >&2
    exit 2
fi

version="${1#v}"
file="${2:-CHANGELOG.md}"

if [ ! -f "$file" ]; then
    echo "Не найден файл $file" >&2
    exit 2
fi

section="$(
    awk -v ver="$version" '
        /^##[[:space:]]*\[/ {
            if (capture) {
                exit
            }
            line = $0
            sub(/^##[[:space:]]*\[/, "", line)
            sub(/\].*$/, "", line)
            if (line == ver) {
                capture = 1
                next
            }
        }

        capture {
            if (!started && $0 ~ /[^[:space:]]/) {
                started = 1
            }
            if (started) {
                lines[++count] = $0
                if ($0 ~ /[^[:space:]]/) {
                    last = count
                }
            }
        }

        END {
            for (i = 1; i <= last; i++) {
                print lines[i]
            }
        }
    ' "$file"
)"

if [ -z "${section//[[:space:]]/}" ]; then
    echo "В $file нет непустой секции ## [$version]" >&2
    exit 1
fi

printf '%s\n' "$section"
