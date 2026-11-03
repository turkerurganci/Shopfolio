#!/bin/sh
# Nightly file backup job (09 17 items 3-5; 05 7.6.1, 7.6.6). The scheduler (scheduler.sh) starts it at BACKUP_RUN_TIME.
#
# Four steps, in this order:  restic backup  ->  restic forget --keep-within 30d  ->  restic prune --max-unused 0  ->  restic check
# A step that fails stops the round: the later steps are not run, the status file says which step failed (`failedStep`), and the
# job exits with that step's exit code. Pruning is two commands on purpose (no `forget --prune`): the failing step stays
# distinguishable, and `--max-unused 0` takes the unused-data share (5 % by default) to zero. The data-reading integrity check
# (`restic check --read-data-subset`) is NOT run nightly: it downloads data from the repository; the recovery drill does it
# (05 7.6.8; DEPLOY_RUNBOOK section J).
#
# Settings (installation settings, DEPLOY_RUNBOOK section J): BACKUP_S3_ENDPOINT, BACKUP_S3_BUCKET, BACKUP_S3_REGION,
# BACKUP_S3_URI_STYLE. Secrets arrive as mounted files (09 17 item 2): /run/secrets/file_backup_storage_key (line 1: access
# key id, line 2: secret key) and /run/secrets/file_backup_cipher_key (the repository password). The password file is handed to
# restic by PATH (RESTIC_PASSWORD_FILE); the object storage key is read into the environment of restic's own process only.
# Shell tracing stays off and no secret is written to the log.
#
# Test seams (the script test replaces restic by a stub): FILE_BACKUP_PATHS, FILE_BACKUP_STATUS_DIR, FILE_BACKUP_LOG_DIR,
# FILE_BACKUP_SECRETS_DIR, FILE_BACKUP_REPOSITORY.

set -eu

# The paths the backup reads: the read-only mounts of the `backup` service in deploy/compose.yaml plus the tool's own log
# directory /var/log/backup (05 9.1.4: the backup tool's log is one of the server's logs). The consistency test keeps the list
# equal to those mounts. Settings files and secret files are not in this list and are not mounted into the container.
DEFAULT_BACKUP_PATHS="/data/images /data/digital /data/access-logs /data/app-logs /data/db-logs /data/os-journal /var/log/backup"

SCRIPT_DIR="$(dirname "$0")"
# shellcheck disable=SC1091 # sourced at run time; the image keeps both files in /usr/local/bin
. "${SCRIPT_DIR}/write-status.sh"

now_utc() {
  date -u +'%Y-%m-%dT%H:%M:%SZ'
}

# repository_url: the restic repository of the file backup, its own prefix `files` in the shared bucket (05 7.6.4).
repository_url() {
  if [ -n "${FILE_BACKUP_REPOSITORY:-}" ]; then
    printf '%s' "$FILE_BACKUP_REPOSITORY"
  else
    printf 's3:https://%s/%s/files' "$BACKUP_S3_ENDPOINT" "$BACKUP_S3_BUCKET"
  fi
}

# export_pair <name> <value>: exports one variable; the name is an argument, so no secret-looking assignment sits in this file.
export_pair() {
  export "$1=$2"
}

# load_environment: the object storage key and the repository password reach restic's process only.
load_environment() {
  _secrets="${FILE_BACKUP_SECRETS_DIR-}"
  [ -n "$_secrets" ] || _secrets=/run/secrets
  if [ ! -r "${_secrets}/file_backup_cipher_key" ]; then
    echo "file-backup: the repository password file is missing or unreadable" >&2
    return 1
  fi
  RESTIC_REPOSITORY="$(repository_url)"
  export RESTIC_REPOSITORY
  export_pair RESTIC_PASSWORD_FILE "${_secrets}/file_backup_cipher_key"
  if [ -z "${FILE_BACKUP_REPOSITORY:-}" ]; then
    if [ ! -r "${_secrets}/file_backup_storage_key" ]; then
      echo "file-backup: the object storage key file is missing or unreadable" >&2
      return 1
    fi
    export_pair AWS_ACCESS_KEY_ID "$(sed -n '1p' "${_secrets}/file_backup_storage_key")"
    export_pair AWS_SECRET_ACCESS_KEY "$(sed -n '2p' "${_secrets}/file_backup_storage_key")"
    export_pair AWS_DEFAULT_REGION "$BACKUP_S3_REGION"
  fi
}

# bucket_lookup: pgBackRest's `host` style is restic's `dns`; `path` is `path`.
bucket_lookup() {
  case "${BACKUP_S3_URI_STYLE:-host}" in
    path) printf 'path' ;;
    *) printf 'dns' ;;
  esac
}

# ensure_repository: the first run creates the repository (counted as part of the `backup` step).
ensure_repository() {
  if ! restic -o "s3.bucket-lookup=$(bucket_lookup)" cat config >/dev/null 2>&1; then
    # `set -e` is suspended while a step runs under run_step's `||`, so the failure is returned by hand.
    restic -o "s3.bucket-lookup=$(bucket_lookup)" init || return $?
  fi
}

# run_step <name> <command...>: runs one step; on failure records the step and the exit code and returns non-zero.
STEP_FAILED=""
STEP_CODE=0
run_step() {
  _name="$1"
  shift
  echo "file-backup: step ${_name} starts"
  _code=0
  "$@" || _code=$?
  echo "file-backup: step ${_name} exit code ${_code}"
  if [ "$_code" -ne 0 ]; then
    STEP_FAILED="$_name"
    STEP_CODE="$_code"
    return 1
  fi
  return 0
}

step_backup() {
  ensure_repository || return $?
  # shellcheck disable=SC2086 # the path list is split on purpose
  restic -o "s3.bucket-lookup=$(bucket_lookup)" backup --host shopfolio --exclude-caches $BACKUP_PATHS
}

step_forget() {
  restic -o "s3.bucket-lookup=$(bucket_lookup)" forget --host shopfolio --keep-within 30d
}

step_prune() {
  restic -o "s3.bucket-lookup=$(bucket_lookup)" prune --max-unused 0
}

step_check() {
  restic -o "s3.bucket-lookup=$(bucket_lookup)" check
}

main() {
  BACKUP_PATHS="${FILE_BACKUP_PATHS:-$DEFAULT_BACKUP_PATHS}"
  _log_dir="${FILE_BACKUP_LOG_DIR:-/var/log/backup}"
  # The tool's own log goes to its own file, not to the container output (05 9.1.4).
  exec >>"${_log_dir}/file-backup.log" 2>&1

  _started="$(now_utc)"
  echo "file-backup: round starts at ${_started}"

  _result="success"
  if load_environment; then
    run_step backup step_backup && run_step forget step_forget && run_step prune step_prune && run_step check step_check || _result="failure"
  else
    # The key or the password file is missing or unreadable: nothing can run, the first step is the one that failed.
    STEP_FAILED="backup"
    STEP_CODE=1
    _result="failure"
  fi

  _finished="$(now_utc)"
  write_status "$_started" "$_finished" "$_result" "$STEP_FAILED" "$STEP_CODE"
  echo "file-backup: round ${_result} at ${_finished}"
  exit "$STEP_CODE"
}

# The script test sources this file with FILE_BACKUP_SOURCE_ONLY=1 to reach the functions without running a round.
if [ "${FILE_BACKUP_SOURCE_ONLY:-0}" != "1" ]; then
  main "$@"
fi
