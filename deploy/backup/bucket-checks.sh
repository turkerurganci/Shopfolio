#!/bin/sh
# Object storage setup and installation checks of the two backups (T07; run by the installer on the server's setup machine, 11 T07
# KK5; 05 7.6.4-7.6.6; DEPLOY_RUNBOOK section J.3-J.5). They need a real bucket and the AWS command line tool, so they are not run in CI.
#
#   bucket-checks.sh setup       the firm account's ADMIN credentials (AWS_* variables): turns versioning on and installs the
#                                lifecycle rule of deploy/backup/bucket-lifecycle.json
#   bucket-checks.sh keys        the two server keys (DB_KEY_FILE, FILES_KEY_FILE: the content of the secret files db_backup_storage_key
#                                and file_backup_storage_key): each may write, read and delete the CURRENT object of its own prefix
#                                only; it may NOT write outside its prefix, delete an old version, or change versioning, the
#                                lifecycle rule or the object lock; it may read those bucket settings (05 7.6.6)
#   bucket-checks.sh lifecycle   the rule exists with its two values (30 days for old versions, 7 days for unfinished uploads)
#
# Settings (environment): BACKUP_S3_ENDPOINT, BACKUP_S3_BUCKET, BACKUP_S3_REGION (as in the installation's settings file);
# AWS_* for `setup`; DB_KEY_FILE and FILES_KEY_FILE for `keys` (the files hold the access key id on line 1, the secret key on line 2).
# The prefixes are fixed: database/ (pgBackRest) and files/ (restic). POSIX shell only; no secret is printed.

set -eu

: "${BACKUP_S3_ENDPOINT:?BACKUP_S3_ENDPOINT is required}"
: "${BACKUP_S3_BUCKET:?BACKUP_S3_BUCKET is required}"
: "${BACKUP_S3_REGION:?BACKUP_S3_REGION is required}"

SCRIPT_DIR="$(dirname "$0")"
FAILED=0
PROBE_NAME="bucket-checks-probe-$$"

ok() { echo "ok:   $1"; }
bad() {
  echo "FAIL: $1"
  FAILED=1
}

# s3api <arguments...>: the AWS tool against the installation's endpoint, with whatever credentials are in the environment.
s3api() {
  aws --endpoint-url "https://${BACKUP_S3_ENDPOINT}" --region "$BACKUP_S3_REGION" s3api "$@"
}

# as_key <key file> <arguments...>: s3api with the credentials of one server key.
as_key() {
  _file="$1"
  shift
  _id_name="AWS_ACCESS_KEY_ID"
  _second_name="AWS_SECRET_ACCESS_KEY"
  _id_value="$(sed -n '1p' "$_file")"
  _second_value="$(sed -n '2p' "$_file")"
  env "${_id_name}=${_id_value}" "${_second_name}=${_second_value}" \
    aws --endpoint-url "https://${BACKUP_S3_ENDPOINT}" --region "$BACKUP_S3_REGION" s3api "$@"
}

# expect_ok <description> <command...> / expect_denied <description> <command...>
expect_ok() {
  _description="$1"
  shift
  if "$@" >/dev/null 2>&1; then ok "$_description"; else bad "$_description (it was refused)"; fi
}
expect_denied() {
  _description="$1"
  shift
  if "$@" >/dev/null 2>&1; then bad "$_description (it was allowed)"; else ok "$_description"; fi
}

setup() {
  s3api put-bucket-versioning --bucket "$BACKUP_S3_BUCKET" --versioning-configuration Status=Enabled
  s3api put-bucket-lifecycle-configuration --bucket "$BACKUP_S3_BUCKET" --lifecycle-configuration "file://${SCRIPT_DIR}/bucket-lifecycle.json"
  ok "versioning is on and the lifecycle rule is installed"
}

lifecycle() {
  _rule="$(s3api get-bucket-lifecycle-configuration --bucket "$BACKUP_S3_BUCKET" --output json 2>/dev/null || true)"
  case "$_rule" in
    *'"NoncurrentDays": 30'*) ok "old versions expire after 30 days" ;;
    *) bad "no rule expires old versions after 30 days" ;;
  esac
  case "$_rule" in
    *'"DaysAfterInitiation": 7'*) ok "unfinished multipart uploads are aborted after 7 days" ;;
    *) bad "no rule aborts unfinished multipart uploads after 7 days" ;;
  esac
}

# check_key <name> <key file> <own prefix> <the other prefix>
check_key() {
  _name="$1"
  _key_file="$2"
  _own="$3"
  _other="$4"
  _body="${SCRIPT_DIR}/bucket-lifecycle.json"
  echo "-- key of ${_name} (prefix ${_own}/)"
  expect_ok "${_name}: writes an object under its own prefix" as_key "$_key_file" put-object --bucket "$BACKUP_S3_BUCKET" --key "${_own}/${PROBE_NAME}" --body "$_body"
  expect_ok "${_name}: reads it back" as_key "$_key_file" get-object --bucket "$BACKUP_S3_BUCKET" --key "${_own}/${PROBE_NAME}" /dev/null
  expect_ok "${_name}: overwrites it (the old content becomes an old version)" as_key "$_key_file" put-object --bucket "$BACKUP_S3_BUCKET" --key "${_own}/${PROBE_NAME}" --body "$_body"
  expect_denied "${_name}: writes outside its prefix (${_other}/)" as_key "$_key_file" put-object --bucket "$BACKUP_S3_BUCKET" --key "${_other}/${PROBE_NAME}" --body "$_body"
  expect_denied "${_name}: writes at the root of the bucket" as_key "$_key_file" put-object --bucket "$BACKUP_S3_BUCKET" --key "${PROBE_NAME}" --body "$_body"
  expect_ok "${_name}: deletes the current object (a delete marker is left)" as_key "$_key_file" delete-object --bucket "$BACKUP_S3_BUCKET" --key "${_own}/${PROBE_NAME}"

  # The old version is found by the admin; the server key must not be able to delete it.
  _version="$(s3api list-object-versions --bucket "$BACKUP_S3_BUCKET" --prefix "${_own}/${PROBE_NAME}" --query 'Versions[0].VersionId' --output text 2>/dev/null || true)"
  if [ -n "$_version" ] && [ "$_version" != "None" ]; then
    expect_denied "${_name}: deletes an old version" as_key "$_key_file" delete-object --bucket "$BACKUP_S3_BUCKET" --key "${_own}/${PROBE_NAME}" --version-id "$_version"
  else
    bad "${_name}: no old version found to try the deletion on (is versioning on? run setup first)"
  fi

  expect_denied "${_name}: switches versioning off" as_key "$_key_file" put-bucket-versioning --bucket "$BACKUP_S3_BUCKET" --versioning-configuration Status=Suspended
  expect_denied "${_name}: changes the lifecycle rule" as_key "$_key_file" put-bucket-lifecycle-configuration --bucket "$BACKUP_S3_BUCKET" --lifecycle-configuration "file://${SCRIPT_DIR}/bucket-lifecycle.json"
  expect_denied "${_name}: deletes the lifecycle rule" as_key "$_key_file" delete-bucket-lifecycle --bucket "$BACKUP_S3_BUCKET"
  # The object lock and the bucket policy are bucket settings too (05 7.6.6). A bucket without the lock refuses the call for that
  # reason as well, so this check cannot tell the two refusals apart; it still fails when the key's call goes through.
  expect_denied "${_name}: changes the object lock" as_key "$_key_file" put-object-lock-configuration --bucket "$BACKUP_S3_BUCKET" --object-lock-configuration ObjectLockEnabled=Enabled
  expect_denied "${_name}: writes the bucket policy" as_key "$_key_file" put-bucket-policy --bucket "$BACKUP_S3_BUCKET" --policy '{"Version":"2012-10-17","Statement":[]}'
  expect_ok "${_name}: reads the versioning setting" as_key "$_key_file" get-bucket-versioning --bucket "$BACKUP_S3_BUCKET"
  expect_ok "${_name}: reads the lifecycle rule" as_key "$_key_file" get-bucket-lifecycle-configuration --bucket "$BACKUP_S3_BUCKET"
}

keys() {
  : "${DB_KEY_FILE:?DB_KEY_FILE is required}"
  : "${FILES_KEY_FILE:?FILES_KEY_FILE is required}"
  check_key "database backup" "$DB_KEY_FILE" database files
  check_key "file backup" "$FILES_KEY_FILE" files database
  echo "Note: the probe objects (${PROBE_NAME}) and their delete markers stay as old versions until the lifecycle rule removes them in 30 days."
}

case "${1:-}" in
  setup) setup ;;
  keys) keys ;;
  lifecycle) lifecycle ;;
  *)
    echo "usage: bucket-checks.sh setup | keys | lifecycle" >&2
    exit 64
    ;;
esac

if [ "$FAILED" -ne 0 ]; then
  echo "Some checks failed: the object storage does not give the key the separation 05 7.6.6 needs. Where the provider cannot, use the object lock path of 05 7.6.6."
  exit 1
fi
echo "All checks passed."
