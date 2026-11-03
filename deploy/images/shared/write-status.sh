#!/bin/sh
# Status file writer of the nightly file backup (09 17 item 5, 05 9.4.10). Sourced by file-backup-job.sh; it defines functions
# only and runs nothing. POSIX shell only: no `jq`, no GNU option.
#
#   write_status <startedAt> <finishedAt> <result> <failedStep> <exitCode>
#
# The file is `backup-status.json` in $FILE_BACKUP_STATUS_DIR (default /var/lib/backup/status, the Compose volume the
# application reads). It is written to a temporary file in the SAME directory first and then renamed: a rename inside one
# file system is atomic, so a reader sees the old file or the new file, never half of one. A successful round stores its own
# finish time as `lastSuccessAt`; a failed round carries the previous file's `lastSuccessAt` over. With no previous file (the
# first run) or no such field the value is the empty text. No error text, no path and no secret is ever written to the file.

STATUS_FILE_NAME="backup-status.json"

# status_dir: the directory of the status file.
status_dir() {
  printf '%s' "${FILE_BACKUP_STATUS_DIR:-/var/lib/backup/status}"
}

# previous_last_success: the `lastSuccessAt` of the existing status file, or the empty text.
previous_last_success() {
  _file="$(status_dir)/${STATUS_FILE_NAME}"
  if [ -f "$_file" ]; then
    _value="$(sed -n 's/.*"lastSuccessAt"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' "$_file" | head -n 1)"
    # Only an ISO-8601 UTC instant is carried over; anything else (a damaged file) counts as "never".
    case "$_value" in
      [0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]T[0-9][0-9]:[0-9][0-9]:[0-9][0-9]Z) printf '%s' "$_value" ;;
      *) printf '' ;;
    esac
  else
    printf ''
  fi
}

# write_status <startedAt> <finishedAt> <result: success|failure> <failedStep: backup|forget|prune|check|""> <exitCode>
write_status() {
  _started="$1"
  _finished="$2"
  _result="$3"
  _failed_step="$4"
  _exit_code="$5"

  case "$_result" in
    success) _last_success="$_finished" ;;
    failure) _last_success="$(previous_last_success)" ;;
    *)
      echo "write_status: result must be success or failure; got '${_result}'" >&2
      return 64
      ;;
  esac
  case "$_failed_step" in
    "" | backup | forget | prune | check) ;;
    *)
      echo "write_status: unknown failed step '${_failed_step}'" >&2
      return 64
      ;;
  esac

  _dir="$(status_dir)"
  _temp="${_dir}/.${STATUS_FILE_NAME}.$$"
  (
    umask 022
    printf '{\n'
    printf '  "schemaVersion": 1,\n'
    printf '  "tool": "restic",\n'
    printf '  "startedAt": "%s",\n' "$_started"
    printf '  "finishedAt": "%s",\n' "$_finished"
    printf '  "result": "%s",\n' "$_result"
    printf '  "failedStep": "%s",\n' "$_failed_step"
    printf '  "exitCode": %s,\n' "$_exit_code"
    printf '  "lastSuccessAt": "%s"\n' "$_last_success"
    printf '}\n'
  ) >"$_temp" || {
    rm -f "$_temp"
    return 1
  }
  mv -f "$_temp" "${_dir}/${STATUS_FILE_NAME}"
}
