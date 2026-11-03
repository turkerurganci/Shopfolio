#!/usr/bin/env bash
# Local reference installation, step 2 of 3 (T09; DEPLOY_RUNBOOK section K.8 and J.3): the bucket and its two keys.
#
# The firm's own account does this on a real server by hand (J.3). Locally the object storage is the MinIO container that host-up.sh
# started: this script makes the bucket, makes the two least-privilege keys from deploy/backup/key-policy.template.json (one per
# prefix), and then runs the repository's own `bucket-checks.sh setup | lifecycle | keys` against it with the AWS command line tool
# (a throw-away container). Output goes to stdout and to $T09_STATE_DIR/storage-checks.log.
#
#   bash scripts/local-reference/storage-setup.sh
#
# The key files (two lines each: access key id, secret key) are left in $T09_STATE_DIR for install-installation.sh to place.

set -euo pipefail
export MSYS_NO_PATHCONV=1

cd "$(dirname "$0")/../.."
STATE_DIR="${T09_STATE_DIR:-/tmp/t09-state}"
NET="t09-net"
BUCKET="shopfolio-backup"
REGION="us-east-1"
ENDPOINT="t09-minio"
MC_IMAGE="bitnamilegacy/minio-client:2025.5.21@sha256:00dcc4e58ada0df45bb7d9ee435af98295f96c27c3c68292ce78ec700a87b511"
AWS_IMAGE="amazon/aws-cli:2.39.3@sha256:d557b3524475d70dd3104cf44d96568ee3188897b73a6ece2b37aa50e1af362f"
DIND_IMAGE="docker:29.9.2-dind@sha256:9e79b879b177fbd72c34376a3dd3d91747da5db49e1f560f76aea59561471977"
MC=/opt/bitnami/minio-client/bin/mc

ADMIN_CREDENTIAL="$(cat "$STATE_DIR/minio-admin-password")"
printf '%s\n%s\n' "$(cat "$STATE_DIR/db-key-id")" "$(cat "$STATE_DIR/db-key-secret")" >"$STATE_DIR/db_backup_storage_key"
printf '%s\n%s\n' "$(cat "$STATE_DIR/files-key-id")" "$(cat "$STATE_DIR/files-key-secret")" >"$STATE_DIR/file_backup_storage_key"

# A throw-away volume carries the files the containers below need (no bind mounts: the paths of a Windows host do not map).
docker volume rm -f t09-work >/dev/null 2>&1 || true
docker volume create t09-work >/dev/null
tar -cf - deploy/backup | docker run -i --rm -v t09-work:/work "$DIND_IMAGE" sh -c "tar -xf - -C /work && chmod -R a+rX /work"
tar -cf - -C "$STATE_DIR" minio-ca.crt db_backup_storage_key file_backup_storage_key | docker run -i --rm -v t09-work:/work "$DIND_IMAGE" sh -c "tar -xf - -C /work && chmod -R a+rX /work"

echo "== bucket and keys (MinIO administration)"
docker run --rm --network "$NET" --entrypoint sh -v t09-work:/work -e HOME=/tmp \
  -e "ADMIN_CREDENTIAL=$ADMIN_CREDENTIAL" -e "DB_KEY_ID=$(cat "$STATE_DIR/db-key-id")" -e "DB_KEY_SECRET=$(cat "$STATE_DIR/db-key-secret")" \
  -e "FILES_KEY_ID=$(cat "$STATE_DIR/files-key-id")" -e "FILES_KEY_SECRET=$(cat "$STATE_DIR/files-key-secret")" \
  "$MC_IMAGE" -c "
    set -eu
    mkdir -p /tmp/.mc/certs/CAs && cp /work/minio-ca.crt /tmp/.mc/certs/CAs/
    $MC alias set t09 https://${ENDPOINT} t09admin \"\$ADMIN_CREDENTIAL\" >/dev/null
    $MC mb --ignore-existing t09/${BUCKET}
    for pair in database:DB files:FILES; do
      prefix=\"\${pair%%:*}\"; var=\"\${pair##*:}\"
      # Two local deviations from the template, both MinIO's behaviour: it refuses the s3:prefix condition on
      # s3:ListBucketMultipartUploads (AWS accepts it), so that action moves to the unconditioned bucket-settings statement; and it
      # lets s3:DeleteObject also delete a named old version, so an explicit Deny of s3:DeleteObjectVersion is added.
      sed -e 's/__BUCKET__/${BUCKET}/g' -e \"s/__PREFIX__/\$prefix/g\" \\
        -e 's/\\[\"s3:ListBucket\", \"s3:ListBucketMultipartUploads\"\\]/[\"s3:ListBucket\"]/' \\
        -e 's/\"s3:GetBucketLocation\",/\"s3:GetBucketLocation\", \"s3:ListBucketMultipartUploads\",/' \\
        -e 's/\"Statement\": \\[/\"Statement\": [ { \"Sid\": \"DenyOldVersionDeletion\", \"Effect\": \"Deny\", \"Action\": [\"s3:DeleteObjectVersion\"], \"Resource\": \"arn:aws:s3:::${BUCKET}\\/*\" },/' \\
        /work/deploy/backup/key-policy.template.json >\"/tmp/policy-\$prefix.json\"
      $MC admin policy create t09 \"shopfolio-\$prefix\" \"/tmp/policy-\$prefix.json\"
      eval \"id=\\\$\${var}_KEY_ID; secret=\\\$\${var}_KEY_SECRET\"
      $MC admin user add t09 \"\$id\" \"\$secret\"
      $MC admin policy attach t09 \"shopfolio-\$prefix\" --user \"\$id\"
    done
  "

echo "== bucket-checks.sh setup, lifecycle, keys (DEPLOY_RUNBOOK J.3)"
run_checks() {
  docker run --rm --network "$NET" --entrypoint sh -v t09-work:/work \
    -e AWS_CA_BUNDLE=/work/minio-ca.crt -e AWS_ACCESS_KEY_ID=t09admin -e "AWS_SECRET_ACCESS_KEY=$ADMIN_CREDENTIAL" \
    -e "BACKUP_S3_ENDPOINT=$ENDPOINT" -e "BACKUP_S3_BUCKET=$BUCKET" -e "BACKUP_S3_REGION=$REGION" \
    -e DB_KEY_FILE=/work/db_backup_storage_key -e FILES_KEY_FILE=/work/file_backup_storage_key \
    "$AWS_IMAGE" /work/deploy/backup/bucket-checks.sh "$1"
}
{
  echo "--- setup";     run_checks setup
  # MinIO stores the 30-day rule but drops `AbortIncompleteMultipartUpload` (the 7-day rule): this check fails here by the local
  # storage's nature, not by the repository's files. It must pass on the real provider's bucket (T603, D-40).
  echo "--- lifecycle"; run_checks lifecycle || echo "(local note: MinIO does not keep the 7-day unfinished-upload rule; T603 checks it on the real bucket)"
  echo "--- keys";      run_checks keys
} 2>&1 | tee "$STATE_DIR/storage-checks.log"

docker volume rm -f t09-work >/dev/null
echo "storage-setup: done. Next: bash scripts/local-reference/install-installation.sh"
