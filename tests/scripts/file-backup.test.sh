#!/bin/sh
# Script test of the nightly file backup (11 T07 KK3, KK4; 09 17 items 4, 5 and 7).
#
#   KK3  the four steps run in the order backup, forget, prune, check; a failing step stops the later ones; pruning is two commands
#        and `prune` runs with `--max-unused 0`; the secret values never reach the log.
#   KK4  the status file fits its schema after every outcome; a failed round carries the previous `lastSuccessAt`, the first run
#        has an empty one; the file is written atomically (temporary file in the same directory, then renamed).

. "$(dirname "$0")/lib.sh"

new_workdir
install_stub restic

JOB_DIR="${WORKDIR}/job"
mkdir -p "$JOB_DIR"
cp "${REPO_ROOT}/deploy/images/backup/file-backup-job.sh" "$JOB_DIR/"
cp "${REPO_ROOT}/deploy/images/shared/write-status.sh" "$JOB_DIR/"

KEY_ID_VALUE="test-only-key-id"
SECOND_LINE_VALUE="test-only-key-secret"
CIPHER_VALUE="test-only-cipher"

# prepare: a clean round directory with the secret files; the status directory exists and is empty.
prepare() {
  ROUND="${WORKDIR}/round-$1"
  mkdir -p "${ROUND}/status" "${ROUND}/log" "${ROUND}/secrets"
  printf '%s\n%s\n' "$KEY_ID_VALUE" "$SECOND_LINE_VALUE" >"${ROUND}/secrets/file_backup_storage_key"
  printf '%s' "$CIPHER_VALUE" >"${ROUND}/secrets/file_backup_cipher_key"
  CALLS="${ROUND}/calls.txt"
  STUBENV="${ROUND}/stub-env.txt"
  : >"$CALLS"
}

# run_job <fail_at> <fail_code>: one round with the restic stub; the exit code is left in JOB_CODE.
run_job() {
  JOB_CODE=0
  env PATH="${WORKDIR}/bin:${PATH}" \
    STUB_CALLS="$CALLS" STUB_ENV="$STUBENV" STUB_FAIL_AT="${1:-}" STUB_FAIL_CODE="${2:-3}" \
    FILE_BACKUP_PATHS="/data/images /data/digital" \
    FILE_BACKUP_STATUS_DIR="${ROUND}/status" FILE_BACKUP_LOG_DIR="${ROUND}/log" FILE_BACKUP_SECRETS_DIR="${ROUND}/secrets" \
    BACKUP_S3_ENDPOINT="s3.test.invalid" BACKUP_S3_BUCKET="test-bucket" BACKUP_S3_REGION="test-region" BACKUP_S3_URI_STYLE="path" \
    sh "${JOB_DIR}/file-backup-job.sh" >/dev/null 2>&1 || JOB_CODE=$?
}

# json_field <name>: the value of a field of the status file (strings without quotes, numbers as they are).
json_field() {
  sed -n "s/^ *\"$1\": *\"\{0,1\}\([^\",]*\)\"\{0,1\},\{0,1\}\$/\1/p" "${ROUND}/status/backup-status.json"
}

# assert_schema <expected result> <expected failedStep> <expected exitCode>
assert_schema() {
  _file="${ROUND}/status/backup-status.json"
  assert_eq "1" "$(json_field schemaVersion)" "schemaVersion"
  assert_eq "restic" "$(json_field tool)" "tool"
  assert_eq "$1" "$(json_field result)" "result"
  assert_eq "$2" "$(json_field failedStep)" "failedStep"
  assert_eq "$3" "$(json_field exitCode)" "exitCode"
  case "$(json_field startedAt)" in
    [0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]T[0-9][0-9]:[0-9][0-9]:[0-9][0-9]Z) pass ;;
    *) fail "startedAt is not an ISO-8601 UTC instant: '$(json_field startedAt)'" ;;
  esac
  case "$(json_field finishedAt)" in
    [0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]T[0-9][0-9]:[0-9][0-9]:[0-9][0-9]Z) pass ;;
    *) fail "finishedAt is not an ISO-8601 UTC instant: '$(json_field finishedAt)'" ;;
  esac
  # Exactly the nine schema lines: braces, eight fields; no free text, no path, no secret.
  assert_eq "10" "$(wc -l <"$_file" | tr -d ' ')" "line count of the status file"
  assert_not_contains "$_file" "$SECOND_LINE_VALUE" "status file carries no secret"
  # No temporary file is left behind (the write was a rename).
  assert_eq "1" "$(ls -A "${ROUND}/status" | wc -l | tr -d ' ')" "only the status file is in the status directory"
}

# steps_called: the restic sub-commands of the round in call order, one line, repository creation and probing left out.
steps_called() {
  awk '{ if ($1 != "cat" && $1 != "init") printf "%s ", $1 }' "$CALLS" | sed 's/ $//'
}

# ---- 1. a round that succeeds ----------------------------------------------------------------------------------------------
prepare success
run_job
assert_eq "0" "$JOB_CODE" "success: exit code"
assert_eq "backup forget prune check" "$(steps_called)" "success: the four steps run in the order of 09 17 item 4"
assert_contains "$CALLS" "forget --host shopfolio --keep-within 30d" "forget keeps 30 days by time"
assert_contains "$CALLS" "prune --max-unused 0" "prune runs with --max-unused 0"
assert_not_contains "$CALLS" "--prune" "pruning is never the combined forget --prune"
assert_not_contains "$CALLS" "--read-data" "the nightly check does not read data"
assert_contains "$CALLS" "backup --host shopfolio --exclude-caches /data/images /data/digital" "backup reads exactly the given paths"
assert_schema "success" "" "0"
assert_eq "$(json_field finishedAt)" "$(json_field lastSuccessAt)" "success: lastSuccessAt is finishedAt"
# The password goes to restic as a FILE PATH; the key reaches only restic's process; neither value is in the log.
assert_contains "$STUBENV" "RESTIC_PASSWORD_FILE=${ROUND}/secrets/file_backup_cipher_key" "the password is handed over as a path"
assert_contains "$STUBENV" "RESTIC_REPOSITORY=s3:https://s3.test.invalid/test-bucket/files" "the repository is the file backup's own prefix"
assert_contains "$STUBENV" "KEY_ID_SET=yes" "the access key id reaches restic"
assert_contains "$STUBENV" "SECOND_LINE_SET=yes" "the second key line reaches restic"
for value in "$KEY_ID_VALUE" "$SECOND_LINE_VALUE" "$CIPHER_VALUE"; do
  assert_not_contains "${ROUND}/log/file-backup.log" "$value" "the log carries no secret value"
done
FIRST_SUCCESS="$(json_field lastSuccessAt)"

# ---- 2. each step failing: later steps do not run, the file names the step, lastSuccessAt is carried over -------------------
for failing in backup forget prune check; do
  prepare "fail-$failing"
  # A previous successful round is in the status directory.
  cp "${WORKDIR}/round-success/status/backup-status.json" "${ROUND}/status/backup-status.json"
  run_job "$failing" 7
  assert_eq "7" "$JOB_CODE" "fail at ${failing}: the job exits with the step's code"
  case "$failing" in
    backup) expected="backup" ;;
    forget) expected="backup forget" ;;
    prune) expected="backup forget prune" ;;
    check) expected="backup forget prune check" ;;
  esac
  assert_eq "$expected" "$(steps_called)" "fail at ${failing}: no step after the failed one runs"
  assert_schema "failure" "$failing" "7"
  assert_eq "$FIRST_SUCCESS" "$(json_field lastSuccessAt)" "fail at ${failing}: lastSuccessAt carries the previous value"
done

# ---- 3. the first run fails: no previous file, lastSuccessAt is the empty text ---------------------------------------------
prepare first-run-fails
run_job backup 4
assert_eq "4" "$JOB_CODE" "first run failing: exit code"
assert_schema "failure" "backup" "4"
assert_eq "" "$(json_field lastSuccessAt)" "first run failing: lastSuccessAt is empty"
assert_contains "${ROUND}/status/backup-status.json" '"lastSuccessAt": ""' "first run failing: the field is present and empty"

# ---- 4. a damaged previous file counts as "never" --------------------------------------------------------------------------
prepare damaged
printf 'not json at all\n"lastSuccessAt": "tomorrow"\n' >"${ROUND}/status/backup-status.json"
run_job check 2
assert_schema "failure" "check" "2"
assert_eq "" "$(json_field lastSuccessAt)" "a damaged previous file is carried over as empty"

# ---- 5. a missing secret file fails the first step without calling restic --------------------------------------------------
prepare no-secret
rm -f "${ROUND}/secrets/file_backup_cipher_key"
run_job
assert_eq "1" "$JOB_CODE" "missing password file: exit code"
assert_eq "" "$(steps_called)" "missing password file: restic is not called"
assert_schema "failure" "backup" "1"

# ---- 6. the status writer stages its file in the SAME directory and renames it ---------------------------------------------
assert_contains "${REPO_ROOT}/deploy/images/shared/write-status.sh" '_temp="${_dir}/.${STATUS_FILE_NAME}.$$"' "the temporary file is in the status directory"
assert_contains "${REPO_ROOT}/deploy/images/shared/write-status.sh" 'mv -f "$_temp" "${_dir}/${STATUS_FILE_NAME}"' "the temporary file is renamed over the status file"

# ---- 7. the writer refuses a value outside the schema ----------------------------------------------------------------------
prepare bad-values
WRITE_CODE=0
env FILE_BACKUP_STATUS_DIR="${ROUND}/status" sh -c '. "$1"; write_status 2026-01-01T00:00:00Z 2026-01-01T00:01:00Z success unknown 0' sh "${JOB_DIR}/write-status.sh" >/dev/null 2>&1 || WRITE_CODE=$?
assert_eq "64" "$WRITE_CODE" "an unknown failedStep is refused"
assert_eq "0" "$(ls -A "${ROUND}/status" | wc -l | tr -d ' ')" "a refused write leaves no file"

finish
