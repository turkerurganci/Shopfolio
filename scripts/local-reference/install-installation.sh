#!/usr/bin/env bash
# Local reference installation, step 3 of 3 (T09; DEPLOY_RUNBOOK section K.2, K.4, K.5): secrets, firewall rule, first install, start.
#
# Runs on the developer machine; every installation command runs inside the local server t09-host (host-up.sh) as root, the way
# the installer would run it on a real server. Output is the evidence of section K.10's rows 2, 7 and 8.
#
#   bash scripts/local-reference/install-installation.sh

set -euo pipefail
export MSYS_NO_PATHCONV=1

cd "$(dirname "$0")/../.."
STATE_DIR="${T09_STATE_DIR:-/tmp/t09-state}"
IMAGE_TAG="${IMAGE_TAG:-t09}"
HOST="t09-host"
COMPOSE="docker compose --env-file /etc/shopfolio/release.local.env --env-file /etc/shopfolio/settings.env -f deploy/compose.yaml -f deploy/compose.local-reference.yaml"
INSIDE="cd /opt/shopfolio"

on_host() { docker exec "$HOST" sh -c "$INSIDE && $1"; }

echo "== current deploy/ and local helpers into /opt/shopfolio"
tar -cf - deploy scripts/local-reference | docker exec -i "$HOST" tar -xf - -C /opt/shopfolio

echo "== K.2 settings and secret files"
docker exec "$HOST" mkdir -p /tmp/t09-in
docker exec -i "$HOST" sh -c "cat > /tmp/t09-in/db_backup_storage_key" <"$STATE_DIR/db_backup_storage_key"
docker exec -i "$HOST" sh -c "cat > /tmp/t09-in/file_backup_storage_key" <"$STATE_DIR/file_backup_storage_key"
MINIO_IP="$(docker inspect --format '{{(index .NetworkSettings.Networks "t09-net").IPAddress}}' t09-minio)"
docker exec -e "IMAGE_TAG=$IMAGE_TAG" -e "MINIO_IP=$MINIO_IP" "$HOST" sh /opt/shopfolio/scripts/local-reference/host-secrets.sh
mkdir -p "$STATE_DIR" && date -u +%Y-%m-%dT%H:%M:%SZ >"$STATE_DIR/install-started-at"

echo "== K.4 DOCKER-USER rule (the object storage is the only address the database and backup networks may reach)"
on_host "OBJECT_STORAGE_HOSTS=t09-minio sh deploy/firewall/docker-user-egress.sh apply"
on_host "sh deploy/firewall/docker-user-egress.sh status" | head -n 20

echo "== K.5 first install: database only, with the install overlay and empty data directories"
on_host "$COMPOSE -f deploy/compose.install.yaml up -d database"
for _ in $(seq 1 60); do
  state="$(docker exec "$HOST" sh -c "$INSIDE && $COMPOSE ps --format '{{.Health}}' database" 2>/dev/null || true)"
  [ "$state" = "healthy" ] && break
  sleep 3
done
echo "database health: ${state:-unknown}"

echo "== K.5 everything else: up -d (migrate runs first)"
on_host "$COMPOSE up -d"
on_host "$COMPOSE ps"
