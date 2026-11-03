#!/usr/bin/env bash
# CI step "6. Migration dry-run" (09 21.4 madde 3, 10.6; 05 8.3.9).
#
#   1. The model has no change that is not in a migration.
#   2. The migration bundle applies to an empty database.
#   3. The bundle applies to the schema of the last published release (the newest vMAJOR.MINOR.PATCH tag).
#      No release yet -> only step 2 runs and the skip is written to the CI output.
#   4. Rollback test, once a release exists: (A) the application image of that release is pulled from the registry and started
#      against the new schema; liveness must answer 200. (B) its integration tests against the same schema: NOT wired (D-19), so
#      with a release present the step FAILS (it never passes on a warning). No release yet -> step 3 already exited.
#
# Needs: dotnet SDK (global.json), psql, and PGHOST / PGUSER (+ PGPORT, PGPASSWORD) of a PostgreSQL 18 server; for step 4 also
# docker, curl and (Actions) GITHUB_REPOSITORY_OWNER, REGISTRY_CREDENTIAL, GITHUB_ACTOR. Local overrides: ROLLBACK_PREVIOUS_APP_IMAGE,
# ROLLBACK_NETWORK, ROLLBACK_LIVENESS_URL.

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
# Part A: the application image of the last release is pulled from the registry and started against the schema the new bundle
# produced (database shopfolio_dry_previous, already migrated by step 3); its liveness endpoint must answer 200 (05 8.3.9, 8.7.3).
OWNER="${GITHUB_REPOSITORY_OWNER:-}"
PREVIOUS_IMAGE="${ROLLBACK_PREVIOUS_APP_IMAGE:-ghcr.io/${OWNER,,}/shopfolio-app:${PREVIOUS_TAG#v}}"
if [ -n "${REGISTRY_CREDENTIAL:-}" ] && [ -n "${GITHUB_ACTOR:-}" ]; then
  printf '%s' "$REGISTRY_CREDENTIAL" | docker login ghcr.io -u "$GITHUB_ACTOR" --password-stdin >/dev/null
fi
if ! docker pull "$PREVIOUS_IMAGE" >/dev/null; then
  echo "::error title=Migration dry-run::Release $PREVIOUS_TAG exists but its application image '$PREVIOUS_IMAGE' cannot be pulled from the registry; the rollback test cannot run (the package must be public after the first publish, 05 8.2.5)."
  exit 1
fi

ROLLBACK_CONTAINER="shopfolio-rollback-$$"
trap 'docker rm -f "$ROLLBACK_CONTAINER" >/dev/null 2>&1 || true; git worktree remove --force "$WORK/previous" >/dev/null 2>&1 || true; rm -rf "$WORK"' EXIT
docker run -d --name "$ROLLBACK_CONTAINER" --network "${ROLLBACK_NETWORK:-host}" \
  -e "Database__ConnectionString=$(dsn shopfolio_dry_previous)" "$PREVIOUS_IMAGE" >/dev/null
ROLLBACK_URL="${ROLLBACK_LIVENESS_URL:-http://localhost:8080/sistem/canlilik}"
deadline=$((SECONDS + 90))
status="000"
while [ "$SECONDS" -lt "$deadline" ]; do
  status="$(curl -s -o /dev/null -m 3 -w '%{http_code}' "$ROLLBACK_URL" || true)"
  [ "$status" = "200" ] && break
  sleep 2
done
if [ "$status" != "200" ]; then
  docker logs "$ROLLBACK_CONTAINER" 2>&1 | tail -n 40
  echo "::error title=Migration dry-run::The image of $PREVIOUS_TAG did not answer 200 on its liveness endpoint against the new schema (last status $status): the new migrations break the previous release (05 8.7.1, 8.7.3)."
  exit 1
fi
echo "Part A passed: the image of $PREVIOUS_TAG runs against the new schema (liveness 200)."

# Part B: the previous release's integration tests against the same schema (09 21.4 item 3) are NOT wired (D-19). The release's tests
# build their own template database from their own bundle and have no mode that reuses an already migrated database, and the
# registry carries no test artifact. Once a release exists this step therefore fails instead of passing on a warning, so the
# gap cannot hide behind a green run (CI_CD_SETUP.md section 2, "misleading green").
echo "::error title=Migration dry-run::Part B of the rollback test (integration tests of $PREVIOUS_TAG against the new schema) is not wired yet (D-19, DEFERRED_BACKLOG.md); a published release makes this step fail until it is."
exit 1
