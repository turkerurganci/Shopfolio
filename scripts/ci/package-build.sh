#!/usr/bin/env bash
# CI step "7. Paketleme dogrulamasi" (05 8.3.10, 09 21.4 table, 09 17).
#
#   1. Source scan of the build files: no `latest`, every base image pinned by tag and digest, no secret value.
#   2. The three images build with `docker build -f deploy/images/<name>/Dockerfile .` (build context: repository root).
#   3. A restic archive whose SHA-256 differs from the pinned one makes the backup image build fail.
#   4. No image runs as root.
#   5. The application image carries the migration bundle; the container starts against the PostgreSQL service and the
#      liveness endpoint answers 200.
#   6. The database image records transaction commit times, carries pgBackRest and the scheduler.
#   7. The backup image carries the pinned restic and the scheduler.
#
# Images are built, never pushed (publish.yml pushes). Needs: docker, curl, and for step 5 a reachable PostgreSQL 18:
# SHOPFOLIO_TEST_DATABASE (the connection string of the CI service; no password, the service trusts the runner).
#
# Local run: PACKAGE_BUILD_NETWORK=<docker network that reaches the database> and a SHOPFOLIO_TEST_DATABASE with that host name.

set -euo pipefail

cd "$(dirname "$0")/../.."

TAG="ci"
NETWORK="${PACKAGE_BUILD_NETWORK:-host}"
HOST_PORT="${PACKAGE_BUILD_PORT:-8080}"
LIVENESS_PATH="/sistem/canlilik"
LIVENESS_DEADLINE_SECONDS=90
: "${SHOPFOLIO_TEST_DATABASE:?SHOPFOLIO_TEST_DATABASE is required (connection string of the PostgreSQL service)}"

CONTAINERS=()
cleanup() {
  for container in "${CONTAINERS[@]:-}"; do
    [ -n "$container" ] && docker rm -f "$container" >/dev/null 2>&1 || true
  done
}
trap cleanup EXIT

fail() {
  echo "::error title=Package build::$1"
  exit 1
}

# ---- 1. source scan ---------------------------------------------------------------------------------------------------
echo "== 1/7 Source scan of the build files"
mapfile -t DOCKERFILES < <(find deploy/images -name Dockerfile | sort)
[ "${#DOCKERFILES[@]}" -eq 3 ] || fail "expected three Dockerfiles under deploy/images, found ${#DOCKERFILES[@]}"

if grep -rniw -- "latest" deploy; then
  fail "'latest' appears in the build files above; pin the image by version tag and digest (05 MK-10)"
fi

for dockerfile in "${DOCKERFILES[@]}"; do
  while IFS= read -r line; do
    image="$(printf '%s' "$line" | awk '{print $2}')"
    case "$image" in
      *:*@sha256:[0-9a-f]*) ;;
      *) fail "$dockerfile: base image '$image' is not pinned by tag and digest (image:tag@sha256:...)" ;;
    esac
  done < <(grep -E '^FROM ' "$dockerfile")
done

# A secret value in a build file: a *PASSWORD/SECRET/TOKEN/KEY name given a literal value, a private key block, a cloud key id.
# A name ending in _FILE carries a path, not a value.
SENSITIVE_NAME='[A-Za-z0-9_]*(PASSWORD|PASSWD|SECRET|TOKEN|PRIVATE_KEY|ACCESS_KEY|API_KEY)[A-Za-z0-9_]*'
if grep -rnE -- "(^|[^A-Za-z0-9_])${SENSITIVE_NAME}[[:space:]]*[=:][[:space:]]*[\"']?[^[:space:]\"'\$]" deploy/images | grep -vE '_FILE[[:space:]]*[=:]'; then
  fail "a secret value appears in a build file above; secrets arrive as mounted files (09 17 item 2)"
fi
if grep -rnE -- 'BEGIN [A-Z ]*PRIVATE KEY|AKIA[0-9A-Z]{16}' deploy; then
  fail "a key block or access key id appears in a build file above"
fi
echo "ok: no 'latest', every base image pinned, no secret value"

# ---- 2. build ---------------------------------------------------------------------------------------------------------
echo "== 2/7 Build the three images"
for name in app database backup; do
  docker build -f "deploy/images/$name/Dockerfile" -t "shopfolio-$name:$TAG" .
done

# ---- 3. wrong restic digest ---------------------------------------------------------------------------------------------
echo "== 3/7 A restic archive with a wrong SHA-256 fails the backup image build"
WRONG_DIGEST_LOG="$(mktemp)"
if docker build -f deploy/images/backup/Dockerfile \
  --build-arg RESTIC_SHA256_AMD64=0000000000000000000000000000000000000000000000000000000000000000 \
  --build-arg RESTIC_SHA256_ARM64=0000000000000000000000000000000000000000000000000000000000000000 \
  --no-cache-filter fetch --progress=plain -t "shopfolio-backup:wrong-digest" . >"$WRONG_DIGEST_LOG" 2>&1; then
  rm -f "$WRONG_DIGEST_LOG"
  fail "the backup image built with a wrong restic SHA-256; the digest check does not gate the build"
fi
# The build must fail for the right reason: the digest check said so, not a network or download error.
if ! grep -q 'did NOT match' "$WRONG_DIGEST_LOG"; then
  tail -n 30 "$WRONG_DIGEST_LOG"
  rm -f "$WRONG_DIGEST_LOG"
  fail "the backup image build failed, but not with a SHA-256 mismatch ('did NOT match' is missing from its output)"
fi
rm -f "$WRONG_DIGEST_LOG"
echo "ok: the build failed with a SHA-256 mismatch, as required"

# ---- 4. users ---------------------------------------------------------------------------------------------------------
echo "== 4/7 No image runs as root"
for name in app database backup; do
  user="$(docker image inspect --format '{{.Config.User}}' "shopfolio-$name:$TAG")"
  case "$user" in
    "" | root | 0 | 0:* | root:*) fail "shopfolio-$name runs as '${user:-root}'" ;;
  esac
  uid="$(docker run --rm --entrypoint id "shopfolio-$name:$TAG" -u)"
  [ "$uid" != "0" ] || fail "shopfolio-$name process runs as UID 0"
  echo "ok: shopfolio-$name user '$user' (UID $uid)"
done

# ---- 5. application: migration bundle and liveness ------------------------------------------------------------------------
echo "== 5/7 Application image: migration bundle and liveness"
docker run --rm --entrypoint test "shopfolio-app:$TAG" -x /app/efbundle || fail "the migration bundle /app/efbundle is not in the application image"

APP_CONTAINER="shopfolio-ci-app-$$"
CONTAINERS+=("$APP_CONTAINER")
PUBLISH_ARGS=()
URL="http://localhost:8080"
if [ "$NETWORK" != "host" ]; then
  PUBLISH_ARGS=(-p "127.0.0.1:${HOST_PORT}:8080")
  URL="http://localhost:${HOST_PORT}"
fi
# The value is the CI service's connection string: no password, an ephemeral database that exists only in this job.
docker run -d --name "$APP_CONTAINER" --network "$NETWORK" "${PUBLISH_ARGS[@]}" \
  -e "Database__ConnectionString=$SHOPFOLIO_TEST_DATABASE" "shopfolio-app:$TAG" >/dev/null

deadline=$((SECONDS + LIVENESS_DEADLINE_SECONDS))
status="000"
while [ "$SECONDS" -lt "$deadline" ]; do
  status="$(curl -s -o /dev/null -m 3 -w '%{http_code}' "${URL}${LIVENESS_PATH}" || true)"
  [ "$status" = "200" ] && break
  sleep 2
done
if [ "$status" != "200" ]; then
  docker logs "$APP_CONTAINER" 2>&1 | tail -n 40
  fail "the application container did not answer 200 on ${LIVENESS_PATH} within ${LIVENESS_DEADLINE_SECONDS}s (last status $status)"
fi
echo "ok: ${LIVENESS_PATH} answered 200"

# ---- 6. database image ------------------------------------------------------------------------------------------------
echo "== 6/7 Database image: commit timestamps, pgBackRest, scheduler"
DB_CONTAINER="shopfolio-ci-db-$$"
CONTAINERS+=("$DB_CONTAINER")
docker run -d --name "$DB_CONTAINER" --network none -e POSTGRES_HOST_AUTH_METHOD=trust "shopfolio-database:$TAG" >/dev/null
commit_timestamps=""
for _ in $(seq 1 45); do
  # The first start runs the official image's initialization on a temporary server that does not read the image's
  # configuration (it reports 'off'); only the final server reports the image's setting. Wait for 'on'.
  commit_timestamps="$(docker exec "$DB_CONTAINER" psql -U postgres -Atc 'show track_commit_timestamp' 2>/dev/null || true)"
  [ "$commit_timestamps" = "on" ] && break
  sleep 2
done
if [ "$commit_timestamps" != "on" ]; then
  docker logs "$DB_CONTAINER" 2>&1 | tail -n 30
  fail "track_commit_timestamp is '${commit_timestamps:-unreachable}', expected 'on' (05 8.5.11)"
fi
docker exec "$DB_CONTAINER" pgbackrest version
docker exec "$DB_CONTAINER" test -x /usr/local/bin/scheduler.sh || fail "the scheduler is not in the database image"
echo "ok: track_commit_timestamp on, pgBackRest and the scheduler present"

# ---- 7. backup image --------------------------------------------------------------------------------------------------
echo "== 7/7 Backup tool image: restic and scheduler"
docker run --rm --entrypoint restic "shopfolio-backup:$TAG" version
docker run --rm --entrypoint test "shopfolio-backup:$TAG" -x /usr/local/bin/scheduler.sh || fail "the scheduler is not in the backup image"
echo "ok: restic and the scheduler present"

echo "Package build: all checks passed."
