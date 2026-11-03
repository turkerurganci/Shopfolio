#!/bin/sh
# Nightly full backup of the database (05 7.6.1; 09 17). The scheduler (scheduler.sh) starts it at BACKUP_RUN_TIME inside the
# database container. pgBackRest takes the backup of the stanza `shopfolio` twice, each time into one named repository:
#
#   --repo=1  the local copy in the server's own volume (hurried restore while the server is up; 05 7.6.1)
#   --repo=2  the object storage outside the server (the real backup; its key and cipher key arrive in the environment that
#             database-entrypoint.sh prepared)
#
# The repository is ALWAYS named: a backup without --repo goes to the first repository, which would leave the off-server copy
# untaken (05 7.6.1; K-963). The off-server repository goes first, so a day with a slow local disk never costs the real backup.
# A failing command stops the round and the job exits with its code; the scheduler logs the code. Writing the result into the
# database's status record (05 7.6.1, 09 10.8) arrives with the task that creates that table (DEFERRED_BACKLOG D-37).
# Retention is set by time in pgbackrest.conf (30 days off-server) and applied by pgBackRest's own expire after each backup.
# Shell tracing stays off; no secret is written to the log.

set -eu

STANZA="shopfolio"
OFF_SERVER_REPOSITORY=2
LOCAL_REPOSITORY=1

# pgbackrest_run <arguments...>: the one place that calls pgBackRest (the script test replaces the binary on PATH).
pgbackrest_run() {
  pgbackrest --stanza="$STANZA" "$@"
}

main() {
  # The job's own lines go to a file in the database log volume, not to the container output (05 9.1.4, 9.2.2).
  exec >>"${DATABASE_JOB_LOG_DIR:-/var/log/postgresql}/database-backup.log" 2>&1
  echo "database-backup: round starts"
  # Idempotent: an existing stanza is left as it is.
  pgbackrest_run stanza-create
  pgbackrest_run --repo="$OFF_SERVER_REPOSITORY" --type=full backup
  pgbackrest_run --repo="$LOCAL_REPOSITORY" --type=full backup
  echo "database-backup: round ends"
}

if [ "${DATABASE_JOB_SOURCE_ONLY:-0}" != "1" ]; then
  main "$@"
fi
