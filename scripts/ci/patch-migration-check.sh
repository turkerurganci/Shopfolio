#!/usr/bin/env bash
# publish.yml: "a patch release carries no migration" (09 21.4 madde 2, 10.6 madde 7; 05 8.3.12, 8.7.3, 8.7.4).
#
# Usage: bash scripts/ci/patch-migration-check.sh <tag>
#
# Runs before the images are pushed. For a release tag vMAJOR.MINOR.PATCH with PATCH > 0 it compares the tag with the nearest
# earlier release tag: a file added under the migrations folder fails the check.
#   - first release tag (no earlier tag)         -> the check does not run
#   - PATCH == 0                                 -> the check does not apply
#   - annotated tag whose message has a line     -> exception (05 8.7.4: the fix of a data-corrupting migration)
#     "migration-fix: K-<number>"
# A lightweight tag has no message, so it cannot carry the exception: its message would be the commit's.
# Needs the full history and the tags (fetch-depth: 0).

set -euo pipefail

TAG="${1:?usage: patch-migration-check.sh <tag>}"
MIGRATIONS_PATH="${MIGRATIONS_PATH:-src/Shopfolio.Web/Infrastructure/Migrations}"
VERSION_PATTERN='^v([0-9]+)\.([0-9]+)\.([0-9]+)$'

if [[ ! "$TAG" =~ $VERSION_PATTERN ]]; then
  echo "::error title=Patch migration check::'$TAG' is not a release tag of the form vMAJOR.MINOR.PATCH."
  exit 1
fi
PATCH="${BASH_REMATCH[3]}"

if [ "$PATCH" -eq 0 ]; then
  echo "::notice title=Patch migration check::$TAG is not a patch release (PATCH is 0): the check does not apply."
  exit 0
fi

# Nearest earlier release tag: the one just before $TAG in version order.
PREVIOUS="$(git tag --list 'v[0-9]*.[0-9]*.[0-9]*' | grep -E '^v[0-9]+\.[0-9]+\.[0-9]+$' | sort -V | grep -x -B1 -- "$TAG" | head -n 1 || true)"
if [ -z "$PREVIOUS" ] || [ "$PREVIOUS" = "$TAG" ]; then
  echo "::notice title=Patch migration check::$TAG is the first release tag: the check does not run."
  exit 0
fi

ADDED="$(git diff --name-only --diff-filter=A "$PREVIOUS" "$TAG" -- "$MIGRATIONS_PATH")"
if [ -z "$ADDED" ]; then
  echo "OK: no migration added between $PREVIOUS and $TAG."
  exit 0
fi

echo "Migration files added between $PREVIOUS and $TAG:"
echo "$ADDED"

OBJECT_TYPE="$(git cat-file -t "refs/tags/$TAG")"
if [ "$OBJECT_TYPE" = "tag" ]; then
  if git for-each-ref --format='%(contents)' "refs/tags/$TAG" | grep -Eq '^migration-fix: K-[0-9]+[[:space:]]*$'; then
    echo "::warning title=Patch migration check::$TAG adds a migration and carries the migration-fix exception (05 8.7.4)."
    exit 0
  fi
else
  echo "Note: $TAG is a lightweight tag; only an annotated tag (git tag -a) can carry the migration-fix exception."
fi

echo "::error title=Patch migration check::Patch release $TAG adds a migration since $PREVIOUS (09 10.6 madde 7). Release it as a minor version, or, for the fix of a data-corrupting migration, tag with an annotated message line 'migration-fix: K-<number>' (05 8.7.4)."
exit 1
