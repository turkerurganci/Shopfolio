#!/bin/sh
# Script test of the database recovery drill (11 T07 criterion 7; 05 7.6.8; MK-27): the drill WAITS until the restored copy has
# finished recovery (the archived log applied) before it reads the last moment and counts the duration; it gives up within an
# upper time limit; it names the off-server repository.

. "$(dirname "$0")/lib.sh"

new_workdir
install_stub pgbackrest
install_stub pg_ctl
install_stub psql
DRILL="${REPO_ROOT}/deploy/images/database/database-restore-drill.sh"

# run_drill <name> <recovery polls that answer "still in recovery"> [timeout]: the output is left in $OUT, the code in DRILL_CODE.
run_drill() {
  CALLS="${WORKDIR}/calls-$1.txt"
  OUT="${WORKDIR}/out-$1.txt"
  : >"$CALLS"
  rm -f "${WORKDIR}/state-$1"
  DRILL_CODE=0
  env PATH="${WORKDIR}/bin:${PATH}" STUB_CALLS="$CALLS" STUB_STATE="${WORKDIR}/state-$1" STUB_RECOVERY_POLLS="$2" \
    DRILL_SCRATCH_DIR="${WORKDIR}/scratch-$1" DRILL_POLL_SECONDS=0 DRILL_RECOVERY_TIMEOUT="${3:-600}" \
    sh "$DRILL" >"$OUT" 2>&1 || DRILL_CODE=$?
}

# The copy is still replaying for three polls: the drill keeps polling, then reads the ARCHIVE's last moment.
run_drill late 3
assert_eq "0" "$DRILL_CODE" "recovery ends after some polls: exit code"
assert_eq "4" "$(grep -c 'pg_is_in_recovery' "$CALLS")" "the drill polls until pg_is_in_recovery() is false (3 true, then false)"
assert_contains "$OUT" "drill-record: last-moment=2026-11-03 15:46:29+00" "the last moment is the archive's last transaction, read after recovery"
assert_not_contains "$OUT" "15:45:00" "the base backup's moment is not the recorded last moment"
assert_contains "$OUT" "drill-record: repository=object-storage (repo2)" "the record names the off-server repository"
assert_contains "$OUT" "drill-record: restore-duration-seconds=" "the record carries the duration"
# The order: restore, verify, start, poll, read the moment, stop.
assert_eq "--stanza=shopfolio --repo=2 --pg1-path=${WORKDIR}/scratch-late restore" "$(sed -n '1p' "$CALLS")" "the restore names repo 2 and runs first"
assert_eq "1" "$(grep -n 'pg_last_committed_xact' "$CALLS" | cut -d: -f1 | awk -v p="$(grep -n 'pg_is_in_recovery' "$CALLS" | tail -n 1 | cut -d: -f1)" '{ print ($1 > p) ? 1 : 0 }')" "the moment is read after the last poll"

# Recovery already finished at the first poll.
run_drill instant 0
assert_eq "0" "$DRILL_CODE" "recovery already ended: exit code"
assert_contains "$OUT" "drill-record: last-moment=2026-11-03 15:46:29+00" "recovery already ended: the archive's last moment"

# Recovery never ends: the drill fails within the limit and never prints a record.
run_drill never 999999 1
if [ "$DRILL_CODE" -ne 0 ]; then pass; else fail "recovery never ends: the drill must fail"; fi
assert_contains "$OUT" "did not finish recovery" "recovery never ends: the reason is stated"
assert_not_contains "$OUT" "drill-record: last-moment=" "recovery never ends: no record is printed"

finish
