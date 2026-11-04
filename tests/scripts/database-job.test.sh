#!/bin/sh
# Script test of the nightly database backup job (11 T07 KK3 for the pgBackRest side; 05 7.6.1; K-963): every backup names its
# repository, the off-server repository is taken, a failing command stops the round, retention stays in pgbackrest.conf. The result
# of the off-server backup is written into the status record as the backup status user through the local socket (D-39 (a); T118):
# success -> last_succeeded_at, failure -> last_failed_at; a failed status write never stops the local backup but fails the job.

. "$(dirname "$0")/lib.sh"

new_workdir
install_stub pgbackrest
install_stub psql
JOB="${REPO_ROOT}/deploy/images/database/database-job.sh"
SUCCEEDED="psql UPDATE public.backup_status_records SET last_succeeded_at = now()"
FAILED="psql UPDATE public.backup_status_records SET last_failed_at = now()"

# run_job <name> <fail_on>: one round with the pgbackrest and psql stubs (both record into one calls file, in order); the exit code
# is left in JOB_CODE, psql's arguments in ARGS.
run_job() {
  CALLS="${WORKDIR}/calls-$1.txt"
  ARGS="${WORKDIR}/args-$1.txt"
  : >"$CALLS"
  : >"$ARGS"
  mkdir -p "${WORKDIR}/log"
  JOB_CODE=0
  env PATH="${WORKDIR}/bin:${PATH}" STUB_CALLS="$CALLS" STUB_ARGS="$ARGS" STUB_STATE="${WORKDIR}/state-$1" \
    STUB_FAIL_ON="${2:-}" STUB_FAIL_CODE=9 POSTGRES_DB=shopfolio \
    DATABASE_JOB_LOG_DIR="${WORKDIR}/log" sh "$JOB" >/dev/null 2>&1 || JOB_CODE=$?
}

run_job ok
assert_eq "0" "$JOB_CODE" "success: exit code"
assert_eq "4" "$(wc -l <"$CALLS" | tr -d ' ')" "success: stanza-create, the off-server backup, the status write, the local backup"
assert_eq "--stanza=shopfolio stanza-create" "$(sed -n '1p' "$CALLS")" "the stanza is created first (idempotent)"
assert_eq "--stanza=shopfolio --repo=2 --type=full backup" "$(sed -n '2p' "$CALLS")" "the off-server repository is backed up, named explicitly"
assert_eq "$SUCCEEDED" "$(sed -n '3p' "$CALLS")" "success: the off-server result is written before the local copy"
assert_eq "--stanza=shopfolio --repo=1 --type=full backup" "$(sed -n '4p' "$CALLS")" "the local copy is backed up, named explicitly"
assert_not_contains "$CALLS" "last_failed_at" "success: no failure is written"
# The status user through the local socket, without a password, the application database, stopping at the first error.
assert_contains "$ARGS" "-X -q -v ON_ERROR_STOP=1 --no-password --host=/var/run/postgresql --username=shopfolio_backup_status --dbname=shopfolio -c" \
  "the status write connects as the backup status user through the local socket"
assert_contains "${WORKDIR}/log/database-backup.log" "round ends" "success: the round ends"
# No backup command without --repo (a command without it would go to repository 1 only).
if grep -E ' backup$' "$CALLS" | grep -vq -- '--repo='; then
  fail "a backup command without --repo"
else
  pass
fi

# The off-server backup failing stops the round before the local one, writes the failure and exits with pgBackRest's code.
run_job offserver "--repo=2"
assert_eq "9" "$JOB_CODE" "off-server backup fails: the job exits with the code"
assert_eq "$FAILED" "$(sed -n '3p' "$CALLS")" "off-server backup fails: the failure is written"
assert_not_contains "$CALLS" "--repo=1" "off-server backup fails: the local backup is not run"
assert_not_contains "$CALLS" "last_succeeded_at" "off-server backup fails: no success is written"
assert_contains "${WORKDIR}/log/database-backup.log" "round starts" "the job writes its own log file"

# The stanza step failing stops everything; the off-server backup was not taken, so it is a failure of the round.
run_job stanza "stanza-create"
assert_eq "9" "$JOB_CODE" "stanza-create fails: exit code"
assert_eq "2" "$(wc -l <"$CALLS" | tr -d ' ')" "stanza-create fails: no backup runs, only the failure is written"
assert_eq "$FAILED" "$(sed -n '2p' "$CALLS")" "stanza-create fails: the failure is written"

# The success write failing (the database is down, the row's privilege is missing) does not stop the local backup; the job exits
# with psql's code so the scheduler logs a failed round.
run_job statusdown "last_succeeded_at"
assert_eq "9" "$JOB_CODE" "the success write fails: the job exits with psql's code"
assert_eq "--stanza=shopfolio --repo=1 --type=full backup" "$(sed -n '4p' "$CALLS")" "the success write fails: the local copy is still taken"
assert_contains "${WORKDIR}/log/database-backup.log" "the success could not be written" "the success write fails: logged"

finish
