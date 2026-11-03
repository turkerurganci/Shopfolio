#!/bin/sh
# Setup script of the database (09 10.8, 17 item 1; 05 7.1.3, 6.x.6). The official image runs every file of
# /docker-entrypoint-initdb.d once, on the first start with an empty data directory, as the database's founding (super) user,
# against a temporary server that listens on the local socket only. With an existing data directory it does not run.
#
# It creates the four users (schema owner, application, read-only, backup status), the application database (UTF-8, ICU Turkish
# collation), the privileges and the pg_trgm extension, writes the three role settings the migrations read the user names from,
# and replaces the client authentication files. The SQL is in /usr/local/share/shopfolio/bootstrap (the integration tests run
# the same files); this script only supplies the values.
#
# The passwords come from the setup copies of the secret files (compose.install.yaml; DEPLOY_RUNBOOK section B). They reach psql
# through its own backtick command, never on a command line, in an environment variable, or in a shell variable. No shell trace
# (`set -x`) is switched on, and nothing here prints a password (09 17 item 2).
set -eu

SQL_DIR=/usr/local/share/shopfolio/bootstrap
MOUNT_DIR=/run/secrets

DATABASE="${POSTGRES_DB:-shopfolio}"
FOUNDER="${POSTGRES_USER:-postgres}"
MIGRATOR=shopfolio_migrator
APPLICATION=shopfolio_app
READER=shopfolio_reader
BACKUP_STATUS=shopfolio_backup_status

fail() {
  echo "shopfolio setup: $1" >&2
  exit 1
}

# The database name is spliced into two files below: only the characters of a plain identifier are accepted.
case "$DATABASE" in
  "" | *[!a-z0-9_]*) fail "the database name must consist of the characters a-z, 0-9 and _" ;;
  postgres) fail "the application database must not be the maintenance database 'postgres'" ;;
esac
case "$FOUNDER" in
  "" | *[!A-Za-z0-9_]*) fail "the founding user's name must consist of the characters A-Z, a-z, 0-9 and _" ;;
esac

for name in db_setup_migrator_password db_setup_app_password db_setup_reader_password; do
  [ -s "$MOUNT_DIR/$name" ] || fail "the secret file $MOUNT_DIR/$name is missing or empty (compose.install.yaml)"
done

# Runs one SQL file as the founding user against one database. $1 = file, $2 = database. Standard input carries the `\set` lines
# that read the passwords, then the file.
run_sql() {
  {
    printf '%s\n' "\\set migrator_password \`cat $MOUNT_DIR/db_setup_migrator_password\`"
    printf '%s\n' "\\set app_password \`cat $MOUNT_DIR/db_setup_app_password\`"
    printf '%s\n' "\\set reader_password \`cat $MOUNT_DIR/db_setup_reader_password\`"
    cat "$SQL_DIR/$1"
  } | psql -X -q -v ON_ERROR_STOP=1 \
    -v "db=$DATABASE" -v "migrator=$MIGRATOR" -v "app=$APPLICATION" -v "reader=$READER" -v "backup_status=$BACKUP_STATUS" \
    --username "$FOUNDER" --no-password --dbname "$2" --file -
}

run_sql 10-roles.sql postgres
run_sql 20-database.sql postgres
run_sql 30-database-access.sql postgres
run_sql 40-schema.sql "$DATABASE"

# Client authentication last, so the connections of the steps above (and of the scripts that run after this one against the same
# temporary server, which has read the image's own files already) are not affected. The new files are read when the real server
# starts. The names go into the templates with sed; both are plain identifiers (checked above).
substitute() {
  sed -e "s/@FOUNDER@/$FOUNDER/g" -e "s/@DATABASE@/$DATABASE/g" "$SQL_DIR/$1" >"$PGDATA/$1.new"
  mv "$PGDATA/$1.new" "$PGDATA/$1"
}
substitute pg_hba.conf
substitute pg_ident.conf

echo "shopfolio setup: users, database '$DATABASE', privileges, pg_trgm and client authentication are in place"
