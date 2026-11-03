#!/bin/sh
# Recovery drill of the database backup (05 7.6.8; 11 T07 criterion 7; DEPLOY_RUNBOOK section J.5). It runs in a separate, one-off
# container of the database image, never in the running database:
#
#   docker compose ... run --rm --no-deps database /usr/local/bin/database-restore-drill.sh
#
# What it does, in order:
#   1. restores the latest backup and its log files from the OFF-SERVER repository (--repo=2, named explicitly: without it
#      pgBackRest restores from the local copy, which does not test the object storage's key, the cipher key or the remote
#      full backup; K-963) into a scratch directory;
#   2. checks the repository's integrity by reading its data (`pgbackrest verify --repo=2`);
#   3. starts the restored copy on a private socket, lets it recover to the end of the archived log, reads the time of the last
#      committed transaction (track_commit_timestamp is on, 05 8.5.11) and stops it;
#   4. prints the drill record: the repository restored from, the duration of the restore (download included) and the last
#      moment restored. The record is what the runbook table keeps (J.5).
#
# The scratch directory is removed at the end. No secret is printed. Shell tracing stays off. POSIX shell only.

set -eu

STANZA="shopfolio"
REPOSITORY=2
SCRATCH="${DRILL_SCRATCH_DIR:-/tmp/restore-drill}"
SOCKET_DIR="${SCRATCH}-socket"

cleanup() {
  if [ -f "${SCRATCH}/postmaster.pid" ]; then
    pg_ctl -D "$SCRATCH" -m immediate stop >/dev/null 2>&1 || true
  fi
  rm -rf "$SCRATCH" "$SOCKET_DIR"
}
trap cleanup EXIT

rm -rf "$SCRATCH" "$SOCKET_DIR"
mkdir -p "$SCRATCH" "$SOCKET_DIR"
chmod 0700 "$SCRATCH" "$SOCKET_DIR"

started="$(date +%s)"
echo "drill: restoring from repository ${REPOSITORY} (the object storage)"
pgbackrest --stanza="$STANZA" --repo="$REPOSITORY" --pg1-path="$SCRATCH" restore
finished="$(date +%s)"
duration=$((finished - started))

echo "drill: verifying the repository by reading its data"
pgbackrest --stanza="$STANZA" --repo="$REPOSITORY" verify

echo "drill: starting the restored copy to read its last committed moment"
# No archiving from the copy, no network: a private socket directory only.
pg_ctl -D "$SCRATCH" -w -t 600 start -o "-c config_file=/etc/postgresql/postgresql.conf -c archive_mode=off -c listen_addresses= -c unix_socket_directories=${SOCKET_DIR} -c port=5499"
last_moment="$(psql -h "$SOCKET_DIR" -p 5499 -U postgres -d postgres -Atc "select coalesce(timestamp::text, '') from pg_last_committed_xact()")"
pg_ctl -D "$SCRATCH" -m fast stop

echo "drill-record: repository=object-storage (repo${REPOSITORY})"
echo "drill-record: restore-duration-seconds=${duration}"
echo "drill-record: last-moment=${last_moment}"
