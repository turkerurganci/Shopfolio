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
#
# The result of the off-server backup is written into the database's status record (06 4.67; 05 7.6.1, 9.4; 09 10.8 item 3;
# D-39 (a)) through the local socket as the backup status user, which has no password (peer authentication, pg_hba.conf) and may
# only update the two moments: a success sets last_succeeded_at, a failure of stanza-create or of the off-server backup sets
# last_failed_at. The local copy's backup does not count. The statement has no WHERE: the table has one row (06 2.3.2), which the
# initial data creates (T119); before it exists the update changes no row.
#
# A failing pgBackRest command stops the round and the job exits with its code; the scheduler logs the code. A status write that
# fails is logged and makes the job exit with psql's code, but it never stops the local copy's backup.
# Retention is set by time in pgbackrest.conf (30 days off-server) and applied by pgBackRest's own expire after each backup.
# Shell tracing stays off; no secret is written to the log.

set -eu

STANZA="shopfolio"
OFF_SERVER_REPOSITORY=2
LOCAL_REPOSITORY=1
# The user and the socket are fixed like in the setup script (initdb/10-bootstrap.sh) and the client authentication file.
BACKUP_STATUS_USER=shopfolio_backup_status
SOCKET_DIR=/var/run/postgresql

# pgbackrest_run <arguments...>: the one place that calls pgBackRest (the script test replaces the binary on PATH).
pgbackrest_run() {
  pgbackrest --stanza="$STANZA" "$@"
}

# status_write <statement>: runs one statement as the backup status user (the script test replaces psql on PATH).
status_write() {
  psql -X -q -v ON_ERROR_STOP=1 --no-password --host="$SOCKET_DIR" --username="$BACKUP_STATUS_USER" \
    --dbname="${POSTGRES_DB:-shopfolio}" -c "$1"
}

main() {
  # The job's own lines go to a file in the database log volume, not to the container output (05 9.1.4, 9.2.2).
  exec >>"${DATABASE_JOB_LOG_DIR:-/var/log/postgresql}/database-backup.log" 2>&1
  echo "database-backup: round starts"

  # Idempotent: an existing stanza is left as it is.
  backup_code=0
  { pgbackrest_run stanza-create && pgbackrest_run --repo="$OFF_SERVER_REPOSITORY" --type=full backup; } || backup_code=$?
  if [ "$backup_code" -ne 0 ]; then
    echo "database-backup: the off-server backup failed (code $backup_code)"
    status_write "UPDATE public.backup_status_records SET last_failed_at = now()" \
      || echo "database-backup: the failure could not be written to the status record"
    exit "$backup_code"
  fi

  status_code=0
  status_write "UPDATE public.backup_status_records SET last_succeeded_at = now()" || status_code=$?
  if [ "$status_code" -ne 0 ]; then
    echo "database-backup: the success could not be written to the status record (code $status_code)"
  fi

  pgbackrest_run --repo="$LOCAL_REPOSITORY" --type=full backup
  if [ "$status_code" -ne 0 ]; then
    exit "$status_code"
  fi
  echo "database-backup: round ends"
}

if [ "${DATABASE_JOB_SOURCE_ONLY:-0}" != "1" ]; then
  main "$@"
fi
