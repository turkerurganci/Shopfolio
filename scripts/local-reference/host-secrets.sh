#!/bin/sh
# Runs INSIDE the local server (t09-host), as root: writes the installation's settings file and its 15 secret files
# (DEPLOY_RUNBOOK B.1, K.2). Called by install-installation.sh; the two backup key files arrive in /tmp/t09-in.
#
# Every value is made up for this run (cryptographic generator, or a made-up provider value). The provider values (12, 13, 14) are
# not real: the empty application image's liveness endpoint does not read them (T09 criterion 1). Nothing is printed.
# POSIX shell only (busybox).

set -eu

VAULT=/etc/shopfolio/secrets
IN=/tmp/t09-in
mkdir -p /etc/shopfolio "$VAULT"
chmod 0755 /etc/shopfolio
chmod 0700 "$VAULT"
rm -f "$VAULT"/*

# put <name> <uid>: the value is on standard input; the file belongs to the one container user that reads it, mode 0400.
put() {
  install -o "$2" -g "$2" -m 0400 /dev/stdin "$VAULT/$1"
}
gen() { openssl rand -hex 24; }

cat >/etc/shopfolio/settings.env <<'SETTINGS'
SHOPFOLIO_DOMAIN=shopfolio.internal
Email__SenderAddress=info@shopfolio.internal
Email__Region=eu-central-1
BACKUP_S3_ENDPOINT=t09-minio
BACKUP_S3_REGION=us-east-1
BACKUP_S3_BUCKET=shopfolio-backup
BACKUP_S3_URI_STYLE=path
BACKUP_RUN_TIME=01:30
BACKUP_TIME_ZONE=Europe/Istanbul
SETTINGS
chmod 0644 /etc/shopfolio/settings.env

# The image references of this run (the local images; the repository's release.env keeps its placeholders: no tag, no release).
cat >/etc/shopfolio/release.local.env <<RELEASE
SHOPFOLIO_APP_IMAGE=shopfolio-app:${IMAGE_TAG:-t09}
SHOPFOLIO_DATABASE_IMAGE=shopfolio-database:${IMAGE_TAG:-t09}
SHOPFOLIO_BACKUP_IMAGE=shopfolio-backup:${IMAGE_TAG:-t09}
T09_MINIO_IP=${MINIO_IP:?MINIO_IP is required}
RELEASE
chmod 0644 /etc/shopfolio/release.local.env

# Database: founder, then one password per user, written twice (setup copy 999, connection copy 1654).
gen | put db_superuser_password 999
migrator="$(gen)"
printf '%s' "$migrator" | put db_migrator_password 1654
printf '%s' "$migrator" | put db_setup_migrator_password 999
app="$(gen)"
printf 'Host=database;Database=shopfolio;Username=shopfolio_app;Password=%s' "$app" | put db_app_connection 1654
printf '%s' "$app" | put db_setup_app_password 999
reader="$(gen)"
printf 'Host=database;Database=shopfolio;Username=shopfolio_reader;Password=%s' "$reader" | put db_reader_connection 1654
printf '%s' "$reader" | put db_setup_reader_password 999
unset migrator app reader

# Backup: the two keys come from the object storage setup; the two cipher keys are generated (two different values, 48 characters).
put db_backup_storage_key 999 <"$IN/db_backup_storage_key"
gen | put db_backup_cipher_key 999
put file_backup_storage_key 10001 <"$IN/file_backup_storage_key"
gen | put file_backup_cipher_key 10001

# Application: made-up provider values (12, 13, 14), the derivation secret is generated (15).
printf 't09-made-up-payment-merchant:%s' "$(gen)" | put payment_provider_keys 1654
printf 't09-made-up-mail-key-id\n%s' "$(gen)" | put email_credential 1654
printf 't09-made-up-google-value:%s' "$(gen)" | put google_client_secret 1654
openssl rand -base64 32 | put link_derivation_secret 1654

rm -rf "$IN"
ls -ln "$VAULT" | awk 'NR>1 {print $1, $3":"$4, $9}'
