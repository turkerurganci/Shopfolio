#!/bin/sh
# Nightly-job scheduler shared by the database image and the backup image (09 17 item 3). The same file is in both.
#
#   scheduler.sh <job command> [arguments...]
#
# Settings (installation settings, 05 8.4): BACKUP_RUN_TIME ("HH:MM", 24-hour) and BACKUP_TIME_ZONE (a tz database name).
# The loop sleeps until the next run moment, runs the job, and computes the next moment only after the job returns. A job that
# runs past the next run moment therefore skips the missed round, and a single loop never runs two rounds at once. There is
# no cron package. POSIX shell only: no `date -d` and no other GNU option.
#
# The job writes its own result; the scheduler logs only the job's exit code (no secret is ever written to the log, and
# shell tracing stays off).

set -eu

# strip_zeros <digits>: "08" -> "8", "00" -> "0". Arithmetic treats a leading zero as octal, which "08" and "09" are not.
strip_zeros() {
  _digits="${1#0}"
  printf '%s' "${_digits:-0}"
}

# seconds_until_next <HH:MM> <HH> <MM> <SS>: whole seconds from the given current time to the next run moment.
# The run moment itself, reached exactly, counts as already past (the next one is a day later): a job that finishes within
# the same second therefore cannot start a second round.
seconds_until_next() {
  _target="$1"
  _target_h="$(strip_zeros "${_target%%:*}")"
  _target_m="$(strip_zeros "${_target##*:}")"
  _now_h="$(strip_zeros "$2")"
  _now_m="$(strip_zeros "$3")"
  _now_s="$(strip_zeros "$4")"
  _target_total=$((_target_h * 3600 + _target_m * 60))
  _now_total=$((_now_h * 3600 + _now_m * 60 + _now_s))
  _delta=$((_target_total - _now_total))
  if [ "$_delta" -le 0 ]; then
    _delta=$((_delta + 86400))
  fi
  printf '%s' "$_delta"
}

# valid_run_time <value>: succeeds for "HH:MM" with HH 00-23 and MM 00-59.
valid_run_time() {
  case "$1" in
    [0-1][0-9]:[0-5][0-9] | 2[0-3]:[0-5][0-9]) return 0 ;;
    *) return 1 ;;
  esac
}

run_loop() {
  while true; do
    # shellcheck disable=SC2046 # the three fields are split on purpose
    _wait="$(seconds_until_next "$BACKUP_RUN_TIME" $(date +'%H %M %S'))"
    echo "scheduler: next run in ${_wait}s (${BACKUP_RUN_TIME} ${TZ})"
    sleep "$_wait"
    _code=0
    "$@" || _code=$?
    echo "scheduler: job exited with code ${_code}"
  done
}

main() {
  if [ "$#" -lt 1 ]; then
    echo "scheduler: usage: scheduler.sh <job command> [arguments...]" >&2
    exit 64
  fi
  BACKUP_RUN_TIME="${BACKUP_RUN_TIME:-}"
  if ! valid_run_time "$BACKUP_RUN_TIME"; then
    echo "scheduler: BACKUP_RUN_TIME must be HH:MM (24-hour); got '${BACKUP_RUN_TIME}'" >&2
    exit 64
  fi
  # No silent default: a missing zone would run the backup in a zone nobody chose, possibly after 03:00 Turkey time (05 5.3.2).
  # Compose gives the installation's default (deploy/compose.yaml, DEPLOY_RUNBOOK section J); a direct start must set it.
  if [ -z "${BACKUP_TIME_ZONE:-}" ]; then
    echo "scheduler: BACKUP_TIME_ZONE is required (a tz database name, for example Europe/Istanbul)" >&2
    exit 64
  fi
  TZ="$BACKUP_TIME_ZONE"
  export TZ
  run_loop "$@"
}

# Tests source this file with SCHEDULER_SOURCE_ONLY=1 to reach the functions without starting the loop.
if [ "${SCHEDULER_SOURCE_ONLY:-0}" != "1" ]; then
  main "$@"
fi
