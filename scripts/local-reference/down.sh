#!/usr/bin/env bash
# Local reference installation: take it down (T09; DEPLOY_RUNBOOK K.10 cleanup). Removes by name only; the other projects'
# containers, networks and volumes are never touched (no prune, no wildcard).
#
#   bash scripts/local-reference/down.sh installation   the installation inside the server (containers, networks, volumes), the
#                                                       DOCKER-USER rule and the secrets; the server and the storage stay
#   bash scripts/local-reference/down.sh all            everything named t09-* (server, storage, volumes, network) and the state dir

set -euo pipefail
export MSYS_NO_PATHCONV=1

HOST="t09-host"
STATE_DIR="${T09_STATE_DIR:-/tmp/t09-state}"
FILES="--env-file /etc/shopfolio/release.local.env --env-file /etc/shopfolio/settings.env -f deploy/compose.yaml -f deploy/compose.local-reference.yaml"

installation() {
  if docker inspect "$HOST" >/dev/null 2>&1 && [ "$(docker inspect --format '{{.State.Running}}' "$HOST")" = "true" ]; then
    docker exec "$HOST" sh -c "cd /opt/shopfolio 2>/dev/null && docker compose $FILES --profile tools down -v --remove-orphans 2>/dev/null || true"
    docker exec "$HOST" sh -c "cd /opt/shopfolio 2>/dev/null && sh deploy/firewall/docker-user-egress.sh remove 2>/dev/null || true"
    docker exec "$HOST" sh -c "rm -rf /etc/shopfolio/secrets/* /etc/shopfolio/settings.env /etc/shopfolio/release.local.env /etc/shopfolio/caddy-root.crt"
  fi
}

case "${1:-}" in
  installation) installation ;;
  all)
    installation
    for name in t09-client t09-capture t09-minio "$HOST"; do docker rm -f -v "$name" >/dev/null 2>&1 || true; done
    for volume in t09-work t09-minio-certs t09-minio-data t09-docker-data t09-host-etc; do docker volume rm -f "$volume" >/dev/null 2>&1 || true; done
    docker network rm t09-net >/dev/null 2>&1 || true
    rm -rf "$STATE_DIR"
    ;;
  *)
    echo "usage: $0 installation | all" >&2
    exit 2
    ;;
esac
echo "down: $1 done"
