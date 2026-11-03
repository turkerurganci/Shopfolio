#!/bin/sh
# Entrypoint of the database image (09 17 item 2; 05 7.6.1). It does three things around the official image's own entrypoint:
#
#   1. reads the two backup secret files (/run/secrets/db_backup_storage_key, db_backup_cipher_key) and hands them to pgBackRest
#      as environment variables of this container's process tree only (pgBackRest's archive command runs inside PostgreSQL's
#      process, so the variables must exist before PostgreSQL starts); no secret is written anywhere else;
#   2. starts the nightly full backup's scheduler and the one-time stanza creation next to the server;
#   3. runs the official entrypoint (`docker-entrypoint.sh`) with the original arguments, unchanged, and passes the server's
#      exit code on. The server keeps running as the database user: this script never switches users.
#
# Settings (installation settings, DEPLOY_RUNBOOK section J): BACKUP_S3_ENDPOINT, BACKUP_S3_BUCKET, BACKUP_S3_REGION,
# BACKUP_S3_URI_STYLE, BACKUP_RUN_TIME, BACKUP_TIME_ZONE. The storage key file has the access key id on line 1 and the secret
# key on line 2. POSIX shell only. Shell tracing stays off.

set -eu

DEFAULT_MOUNT_DIR=/run/secrets
SECRETS_DIR="${DATABASE_SECRETS_DIR-}"
[ -n "$SECRETS_DIR" ] || SECRETS_DIR="$DEFAULT_MOUNT_DIR"

require_setting() {
  eval "_value=\${$1:-}"
  if [ -z "$_value" ]; then
    echo "database-entrypoint: the setting $1 is required" >&2
    exit 64
  fi
}

# export_pair <name> <value>: exports one variable; the name is an argument, so no secret-looking assignment sits in this file.
export_pair() {
  export "$1=$2"
}

require_file() {
  if [ ! -r "$1" ]; then
    echo "database-entrypoint: the secret file $1 is missing or unreadable" >&2
    exit 64
  fi
}

for name in BACKUP_S3_ENDPOINT BACKUP_S3_BUCKET BACKUP_S3_REGION BACKUP_RUN_TIME BACKUP_TIME_ZONE; do
  require_setting "$name"
done

# A malformed run time stops the container at start, loudly; a scheduler that quietly exits would leave the nightly backup unrun.
# shellcheck disable=SC1091 # the scheduler is in the image; sourcing it only defines functions
SCHEDULER_SOURCE_ONLY=1
. /usr/local/bin/scheduler.sh
unset SCHEDULER_SOURCE_ONLY
if ! valid_run_time "$BACKUP_RUN_TIME"; then
  echo "database-entrypoint: BACKUP_RUN_TIME must be HH:MM (24-hour); got '${BACKUP_RUN_TIME}'" >&2
  exit 64
fi

require_file "${SECRETS_DIR}/db_backup_storage_key"
require_file "${SECRETS_DIR}/db_backup_cipher_key"

# Repository 2 is the object storage (repository 1 is the local copy, see pgbackrest.conf).
PGBACKREST_REPO2_S3_ENDPOINT="$BACKUP_S3_ENDPOINT"
PGBACKREST_REPO2_S3_BUCKET="$BACKUP_S3_BUCKET"
PGBACKREST_REPO2_S3_REGION="$BACKUP_S3_REGION"
PGBACKREST_REPO2_S3_URI_STYLE="${BACKUP_S3_URI_STYLE:-host}"
export PGBACKREST_REPO2_S3_ENDPOINT PGBACKREST_REPO2_S3_BUCKET PGBACKREST_REPO2_S3_REGION PGBACKREST_REPO2_S3_URI_STYLE
export_pair PGBACKREST_REPO2_S3_KEY "$(sed -n '1p' "${SECRETS_DIR}/db_backup_storage_key")"
export_pair PGBACKREST_REPO2_S3_KEY_SECRET "$(sed -n '2p' "${SECRETS_DIR}/db_backup_storage_key")"
export_pair PGBACKREST_REPO2_CIPHER_PASS "$(cat "${SECRETS_DIR}/db_backup_cipher_key")"

# A one-off command with the backup environment starts no server and no scheduler: the recovery drill in its own container
# (`docker compose run --rm --no-deps database /usr/local/bin/database-restore-drill.sh`, DEPLOY_RUNBOOK section J.5), and the
# manual first backup or the archive check in the running container (`docker compose exec database
# /usr/local/bin/database-entrypoint.sh /usr/local/bin/database-job.sh`; a plain `exec` would not carry the variables above).
case "${1:-}" in
  /usr/local/bin/database-restore-drill.sh | /usr/local/bin/database-job.sh | pgbackrest) exec "$@" ;;
esac

# The official entrypoint ends in `exec postgres`, so the process started here becomes the server. The scheduler and the stanza
# creation are siblings of it; they stop with the container.
/usr/local/bin/docker-entrypoint.sh "$@" &
server_pid=$!

/usr/local/bin/scheduler.sh /usr/local/bin/database-job.sh &
scheduler_pid=$!

/usr/local/bin/stanza-bootstrap.sh &
bootstrap_pid=$!

# A stop signal goes to the server only; the container ends when the server ends.
trap 'kill -TERM "$server_pid" 2>/dev/null || true' TERM INT

server_code=0
wait "$server_pid" || server_code=$?
# `wait` returns early when a trapped signal arrives; wait until the server is really gone.
while kill -0 "$server_pid" 2>/dev/null; do
  server_code=0
  wait "$server_pid" || server_code=$?
done

kill "$scheduler_pid" "$bootstrap_pid" 2>/dev/null || true
exit "$server_code"
