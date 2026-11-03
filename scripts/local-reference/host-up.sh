#!/usr/bin/env bash
# Local reference installation, step 1 of 3 (T09; DEPLOY_RUNBOOK section K): the "server".
#
# The local server is a privileged Linux container that runs its own Docker daemon (docker:dind) with iptables, IPv4 and IPv6.
# No WSL distribution is installed. The object storage (MinIO, with a self-signed certificate) is a second container next to it
# on the same outer network; the installation inside the server reaches it by name, exactly as it would reach a bucket.
# Everything this script makes is named `t09-*`; `down.sh` removes it by name. Nothing else on the machine is touched (the other
# projects' containers keep running).
#
#   bash scripts/local-reference/host-up.sh
#
# Needs: docker (outer), the three images built from the Dockerfiles (`docker build -f deploy/images/<name>/Dockerfile -t
# shopfolio-<name>:$IMAGE_TAG .`, the packaging script's second step), internet access (Docker Hub for docker:dind and Caddy).
# Settings (environment): IMAGE_TAG (default t09), T09_STATE_DIR (default /tmp/t09-state; throw-away values of this run only).

set -euo pipefail
export MSYS_NO_PATHCONV=1

cd "$(dirname "$0")/../.."
IMAGE_TAG="${IMAGE_TAG:-t09}"
STATE_DIR="${T09_STATE_DIR:-/tmp/t09-state}"
NET="t09-net"
DIND_IMAGE="docker:29.9.2-dind@sha256:9e79b879b177fbd72c34376a3dd3d91747da5db49e1f560f76aea59561471977"
# MinIO's own image and binary downloads were withdrawn (2026); the last Bitnami mirror is used for the local object storage only.
MINIO_IMAGE="bitnamilegacy/minio:2025.5.24@sha256:451fe6858cb770cc9d0e77ba811ce287420f781c7c1b806a386f6896471a349c"
HOST="t09-host"
MINIO="t09-minio"

mkdir -p "$STATE_DIR"

for name in app database backup; do
  docker image inspect "shopfolio-$name:$IMAGE_TAG" >/dev/null 2>&1 || {
    echo "error: image shopfolio-$name:$IMAGE_TAG is missing; build it with docker build -f deploy/images/$name/Dockerfile -t shopfolio-$name:$IMAGE_TAG ." >&2
    exit 1
  }
done

echo "== network ${NET} (IPv4 and IPv6)"
docker network inspect "$NET" >/dev/null 2>&1 || docker network create --ipv6 --subnet 172.31.90.0/24 --subnet fd09:0:0:90::/64 "$NET" >/dev/null

echo "== throw-away values of this run in ${STATE_DIR}"
[ -s "$STATE_DIR/minio-admin-password" ] || openssl rand -hex 16 >"$STATE_DIR/minio-admin-password"
[ -s "$STATE_DIR/db-key-id" ] || printf 't09dbkey%s' "$(openssl rand -hex 4)" >"$STATE_DIR/db-key-id"
[ -s "$STATE_DIR/db-key-secret" ] || openssl rand -hex 20 >"$STATE_DIR/db-key-secret"
[ -s "$STATE_DIR/files-key-id" ] || printf 't09filekey%s' "$(openssl rand -hex 4)" >"$STATE_DIR/files-key-id"
[ -s "$STATE_DIR/files-key-secret" ] || openssl rand -hex 20 >"$STATE_DIR/files-key-secret"

echo "== object storage ${MINIO} (TLS on 443, certificate for the name ${MINIO})"
docker volume create t09-minio-certs >/dev/null
docker volume create t09-minio-data >/dev/null
docker run --rm -v t09-minio-certs:/certs "$DIND_IMAGE" sh -c \
  "[ -s /certs/public.crt ] || openssl req -x509 -newkey rsa:2048 -nodes -days 30 -subj /CN=${MINIO} -addext subjectAltName=DNS:${MINIO} -keyout /certs/private.key -out /certs/public.crt 2>/dev/null; chmod 644 /certs/public.crt /certs/private.key"
if ! docker inspect "$MINIO" >/dev/null 2>&1; then
  docker run -d --name "$MINIO" --network "$NET" --user root \
    -e "MINIO_ROOT_USER=t09admin" -e "MINIO_ROOT_PASSWORD=$(cat "$STATE_DIR/minio-admin-password")" \
    -v t09-minio-certs:/certs:ro -v t09-minio-data:/data \
    --entrypoint /opt/bitnami/minio/bin/minio "$MINIO_IMAGE" server /data --address ":443" --console-address ":9001" --certs-dir /certs >/dev/null
fi
docker run --rm -v t09-minio-certs:/certs:ro "$DIND_IMAGE" cat /certs/public.crt >"$STATE_DIR/minio-ca.crt"

echo "== server ${HOST} (privileged, own Docker daemon, IPv6 and ip6tables on: DEPLOY_RUNBOOK K.1)"
docker volume create t09-docker-data >/dev/null
if ! docker inspect "$HOST" >/dev/null 2>&1; then
  mkdir -p "$STATE_DIR/etc/docker"
  printf '%s\n' '{"ipv6": true, "ip6tables": true, "fixed-cidr-v6": "fd00:db8:1::/64", "experimental": true}' >"$STATE_DIR/etc/docker/daemon.json"
  docker create --name "$HOST" --privileged --network "$NET" -e DOCKER_TLS_CERTDIR= \
    --sysctl net.ipv6.conf.all.disable_ipv6=0 --sysctl net.ipv6.conf.all.forwarding=1 \
    -v t09-docker-data:/var/lib/docker "$DIND_IMAGE" >/dev/null
  tar -cf - -C "$STATE_DIR" etc/docker/daemon.json | docker cp - "$HOST:/"
  docker start "$HOST" >/dev/null
fi
for _ in $(seq 1 40); do
  docker exec "$HOST" docker info >/dev/null 2>&1 && break
  sleep 2
done
docker exec "$HOST" docker info --format 'inner Docker {{.ServerVersion}}, storage driver {{.Driver}}, memory {{.MemTotal}}'

echo "== tools inside the server (iptables is part of the image)"
docker exec "$HOST" apk add --no-cache -q acl curl bind-tools >/dev/null
docker exec "$HOST" sh -c "iptables -V; getent hosts ${MINIO}"

echo "== images: save here, load into the inner Docker (image IDs are kept)"
docker save "shopfolio-app:$IMAGE_TAG" "shopfolio-database:$IMAGE_TAG" "shopfolio-backup:$IMAGE_TAG" | docker exec -i "$HOST" docker load
docker exec "$HOST" docker pull -q caddy:2.11.7-alpine@sha256:e70b164da81ef698b3ad19af799ea6086ce6d9e48ed4079eb9fe26f6fbf14ee5

echo "== files of the installation: deploy/ and the local helpers into /opt/shopfolio"
docker exec "$HOST" sh -c "rm -rf /opt/shopfolio && mkdir -p /opt/shopfolio /etc/shopfolio/secrets && chmod 0700 /etc/shopfolio/secrets"
tar -cf - deploy scripts/local-reference | docker exec -i "$HOST" tar -xf - -C /opt/shopfolio
docker exec -i "$HOST" sh -c "cat > /etc/shopfolio/minio-ca.crt" <"$STATE_DIR/minio-ca.crt"

echo "host-up: done. Next: bash scripts/local-reference/storage-setup.sh"
