#!/bin/sh
# Recovery drill of the file backup (05 7.6.8; 11 T07 criterion 7; DEPLOY_RUNBOOK section J.5). It runs in a separate, one-off
# container of the backup image (the scheduler is bypassed by --entrypoint), never inside the running backup service:
#
#   docker compose ... run --rm --no-deps --entrypoint /usr/local/bin/file-restore-drill.sh backup
#
# What it does:
#   1. restores a small sample of the most recent snapshot (the log directory of the application and, when present, one more
#      directory) into a scratch directory and counts the restored files;
#   2. checks the repository's integrity by reading a subset of its data (`restic check --read-data-subset=5%`; the nightly
#      round never reads data, 09 17 item 4);
#   3. prints the drill record: the repository restored from, the duration (download included) and the moment of the snapshot
#      restored from (the last moment restored).
#
# The key, the repository and the password file come from the same settings and secret files as the nightly job (it sources
# the job's functions). The scratch directory is removed at the end. No secret is printed. POSIX shell only.

set -eu

SCRIPT_DIR="$(dirname "$0")"
FILE_BACKUP_SOURCE_ONLY=1
# shellcheck disable=SC1091 # both files are in /usr/local/bin in the image
. "${SCRIPT_DIR}/file-backup-job.sh"
unset FILE_BACKUP_SOURCE_ONLY

SCRATCH="${DRILL_SCRATCH_DIR:-/tmp/file-restore-drill}"
trap 'rm -rf "$SCRATCH"' EXIT

load_environment || {
  echo "drill: the key or the password file is missing" >&2
  exit 1
}

rm -rf "$SCRATCH"
mkdir -p "$SCRATCH"
lookup="s3.bucket-lookup=$(bucket_lookup)"

# The most recent snapshot of this host: restic lists them oldest first, so the last short id is the newest.
snapshot_id="$(restic -o "$lookup" snapshots --host shopfolio --json | grep -o '"short_id": *"[^"]*"' | tail -n 1 | sed 's/.*"\([^"]*\)"$/\1/')"
[ -n "$snapshot_id" ] || {
  echo "drill: no snapshot of this host was found" >&2
  exit 1
}

started="$(date +%s)"
restic -o "$lookup" restore "$snapshot_id" --target "$SCRATCH" --include /data/app-logs --include /data/images
finished="$(date +%s)"
duration=$((finished - started))

restored_files="$(find "$SCRATCH" -type f | wc -l | tr -d ' ')"
echo "drill: ${restored_files} file(s) restored"

echo "drill: checking the repository by reading a subset of its data"
restic -o "$lookup" check --read-data-subset=5%

snapshot_time="$(restic -o "$lookup" snapshots "$snapshot_id" --json | sed -n 's/.*"time": *"\([^"]*\)".*/\1/p' | head -n 1)"

echo "drill-record: repository=object-storage (files prefix)"
echo "drill-record: restore-duration-seconds=${duration}"
echo "drill-record: last-moment=${snapshot_time}"
echo "drill-record: restored-files=${restored_files}"
