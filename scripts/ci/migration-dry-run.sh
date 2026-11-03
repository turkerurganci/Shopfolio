#!/usr/bin/env bash
# CI step "6. Migration dry-run" (09 21.4 madde 3, 10.6; 05 8.3.9).
#
#   1. The model has no change that is not in a migration.
#   2. The migration bundle applies to an empty database.
#   3. The bundle applies to the schema of the last published release (the newest vMAJOR.MINOR.PATCH tag).
#      No release yet -> only step 2 runs and the skip is written to the CI output.
#   4. Rollback test (previous release image against the new schema + its integration tests): the images arrive with T06;
#      until then the step says so with a warning, it does not pass silently.
#
# Needs: dotnet SDK (global.json), psql, and PGHOST / PGUSER (+ PGPORT, PGPASSWORD) of a PostgreSQL 18 server.

set -euo pipefail

cd "$(dirname "$0")/../.."
ROOT="$(pwd)"
PROJECT="src/Shopfolio.Web/Shopfolio.Web.csproj"
: "${PGHOST:?PGHOST is required (PostgreSQL service host)}"
export PGUSER="${PGUSER:-postgres}"
export PGPORT="${PGPORT:-5432}"

WORK="$(mktemp -d)"
trap 'git worktree remove --force "$WORK/previous" >/dev/null 2>&1 || true; rm -rf "$WORK"' EXIT

EXT=""
case "$(uname -s)" in MINGW* | MSYS* | CYGWIN*) EXT=".exe" ;; esac

dsn() {
  local dsn="Host=$PGHOST;Port=$PGPORT;Username=$PGUSER;Database=$1;Pooling=false"
  if [ -n "${PGPASSWORD:-}" ]; then dsn="$dsn;Password=$PGPASSWORD"; fi
  printf '%s' "$dsn"
}

fresh_database() {
  psql -d postgres -v ON_ERROR_STOP=1 -q -c "DROP DATABASE IF EXISTS $1 WITH (FORCE)" -c "CREATE DATABASE $1"
}

# build_bundle <source root> <output name>: a throwaway lock file keeps the tracked packages.lock.json files untouched
# (the bundle publishes with a runtime identifier, which would add a target to them).
build_bundle() {
  local source_root="$1" output="$2"
  (
    cd "$source_root"
    dotnet tool restore >/dev/null
    dotnet restore "$PROJECT" --locked-mode >/dev/null
    NuGetLockFilePath="$WORK/throwaway-$output.lock.json" \
      dotnet ef migrations bundle --project "$PROJECT" --output "$WORK/$output$EXT" --force
  )
}

echo "== 1/4 Model changes without a migration"
dotnet tool restore >/dev/null
dotnet restore "$PROJECT" --locked-mode >/dev/null
if ! dotnet ef migrations has-pending-model-changes --project "$PROJECT"; then
  echo "::error title=Migration dry-run::The model has changes that no migration carries; add the migration (dotnet ef migrations add)."
  exit 1
fi

echo "== 2/4 Bundle on an empty database"
build_bundle "$ROOT" new
fresh_database shopfolio_dry_empty
"$WORK/new$EXT" --connection "$(dsn shopfolio_dry_empty)"

echo "== 3/4 Bundle on the schema of the last published release"
PREVIOUS_TAG="$(git tag --list 'v[0-9]*.[0-9]*.[0-9]*' | grep -E '^v[0-9]+\.[0-9]+\.[0-9]+$' | sort -V | tail -n 1 || true)"
if [ -z "$PREVIOUS_TAG" ]; then
  echo "::notice title=Migration dry-run::No published release yet (no vMAJOR.MINOR.PATCH tag): the bundle was applied to the empty database only; applying it to the last release schema and the rollback test are skipped."
  echo "SKIPPED: bundle on the last release schema (no published release)"
  echo "SKIPPED: rollback test (no published release)"
  exit 0
fi

echo "Last published release: $PREVIOUS_TAG"
git worktree add --detach "$WORK/previous" "$PREVIOUS_TAG" >/dev/null
build_bundle "$WORK/previous" previous
fresh_database shopfolio_dry_previous
"$WORK/previous$EXT" --connection "$(dsn shopfolio_dry_previous)"
"$WORK/new$EXT" --connection "$(dsn shopfolio_dry_previous)"

echo "== 4/4 Rollback test"
echo "::warning title=Migration dry-run::Rollback test (image of $PREVIOUS_TAG against the new schema, then its integration tests) is not wired yet: images arrive with T06."
