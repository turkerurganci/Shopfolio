#!/bin/sh
# Script test of the scheduler (09 17 item 7): the next-run computation (past midnight, time already passed, exactly now, the
# octal trap of "08"/"09"), the run-time check and the required zone (D-30 (b)).

. "$(dirname "$0")/lib.sh"

SCHEDULER="${REPO_ROOT}/deploy/images/shared/scheduler.sh"

# The functions are reached by sourcing the file with SCHEDULER_SOURCE_ONLY=1 (no loop starts).
SCHEDULER_SOURCE_ONLY=1
. "$SCHEDULER"
unset SCHEDULER_SOURCE_ONLY
set +eu

# seconds_until_next <HH:MM> <HH> <MM> <SS>
assert_eq "3600" "$(seconds_until_next 01:30 00 30 00)" "before the run time, later the same day"
assert_eq "84600" "$(seconds_until_next 01:30 02 00 00)" "after the run time: tomorrow"
assert_eq "86400" "$(seconds_until_next 01:30 01 30 00)" "exactly the run time counts as passed: a day later"
assert_eq "1" "$(seconds_until_next 01:30 01 29 59)" "one second before"
assert_eq "86399" "$(seconds_until_next 01:30 01 30 01)" "one second after"
assert_eq "5400" "$(seconds_until_next 01:30 00 00 00)" "from midnight"
assert_eq "5401" "$(seconds_until_next 01:30 23 59 59)" "from one second before midnight"
# A run time past midnight reached from the evening: 23:00 -> 00:30 is 5400 seconds.
assert_eq "5400" "$(seconds_until_next 00:30 23 00 00)" "the run time is after midnight, now is the evening"
# "08" and "09" are not octal numbers.
assert_eq "3601" "$(seconds_until_next 09:09 08 08 59)" "08 and 09 are decimal"
assert_eq "60" "$(seconds_until_next 08:09 08 08 00)" "08:09 is decimal"

# valid_run_time
for good in 00:00 01:30 09:59 19:59 23:59; do
  if valid_run_time "$good"; then pass; else fail "valid_run_time refused '$good'"; fi
done
for bad in 24:00 1:30 01:60 0130 "" 01:5 ab:cd 01:30:00; do
  if valid_run_time "$bad"; then fail "valid_run_time accepted '$bad'"; else pass; fi
done

# A start without a job command, with a bad run time, or without BACKUP_TIME_ZONE ends with exit code 64 (no silent default).
run_scheduler() {
  CODE=0
  env -u BACKUP_RUN_TIME -u BACKUP_TIME_ZONE "$@" sh "$SCHEDULER" "$JOB" >/dev/null 2>&1 || CODE=$?
}
JOB="true"
run_scheduler BACKUP_RUN_TIME=01:30
assert_eq "64" "$CODE" "a missing BACKUP_TIME_ZONE stops the scheduler"
run_scheduler BACKUP_RUN_TIME=25:00 BACKUP_TIME_ZONE=Europe/Istanbul
assert_eq "64" "$CODE" "a malformed BACKUP_RUN_TIME stops the scheduler"
run_scheduler BACKUP_TIME_ZONE=Europe/Istanbul
assert_eq "64" "$CODE" "a missing BACKUP_RUN_TIME stops the scheduler"
CODE=0
env -u BACKUP_RUN_TIME -u BACKUP_TIME_ZONE BACKUP_RUN_TIME=01:30 BACKUP_TIME_ZONE=Europe/Istanbul sh "$SCHEDULER" >/dev/null 2>&1 || CODE=$?
assert_eq "64" "$CODE" "a missing job command stops the scheduler"

finish
