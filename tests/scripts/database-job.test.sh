#!/bin/sh
# Script test of the nightly database backup job (11 T07 KK3 for the pgBackRest side; 05 7.6.1; K-963): every backup names its
# repository, the off-server repository is taken, a failing command stops the round, retention stays in pgbackrest.conf.

. "$(dirname "$0")/lib.sh"

new_workdir
install_stub pgbackrest
JOB="${REPO_ROOT}/deploy/images/database/database-job.sh"

# run_job <fail_on>: one round with the pgbackrest stub; the exit code is left in JOB_CODE.
run_job() {
  CALLS="${WORKDIR}/calls-$1.txt"
  : >"$CALLS"
  mkdir -p "${WORKDIR}/log"
  JOB_CODE=0
  env PATH="${WORKDIR}/bin:${PATH}" STUB_CALLS="$CALLS" STUB_FAIL_ON="${2:-}" STUB_FAIL_CODE=9 \
    DATABASE_JOB_LOG_DIR="${WORKDIR}/log" sh "$JOB" >/dev/null 2>&1 || JOB_CODE=$?
}

run_job ok
assert_eq "0" "$JOB_CODE" "success: exit code"
assert_eq "3" "$(wc -l <"$CALLS" | tr -d ' ')" "success: stanza-create and two backups"
assert_eq "--stanza=shopfolio stanza-create" "$(sed -n '1p' "$CALLS")" "the stanza is created first (idempotent)"
assert_eq "--stanza=shopfolio --repo=2 --type=full backup" "$(sed -n '2p' "$CALLS")" "the off-server repository is backed up, named explicitly"
assert_eq "--stanza=shopfolio --repo=1 --type=full backup" "$(sed -n '3p' "$CALLS")" "the local copy is backed up, named explicitly"
# No backup command without --repo (a command without it would go to repository 1 only).
if grep -E ' backup$' "$CALLS" | grep -vq -- '--repo='; then
  fail "a backup command without --repo"
else
  pass
fi

# The off-server backup failing stops the round before the local one and the job exits with pgBackRest's code.
run_job offserver "--repo=2"
assert_eq "9" "$JOB_CODE" "off-server backup fails: the job exits with the code"
assert_not_contains "$CALLS" "--repo=1" "off-server backup fails: the local backup is not run"
assert_contains "${WORKDIR}/log/database-backup.log" "round starts" "the job writes its own log file"

# The stanza step failing stops everything.
run_job stanza "stanza-create"
assert_eq "9" "$JOB_CODE" "stanza-create fails: exit code"
assert_eq "1" "$(wc -l <"$CALLS" | tr -d ' ')" "stanza-create fails: no backup runs"

finish
