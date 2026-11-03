#!/usr/bin/env bash
# Local reference installation: the trial runs of T09 (DEPLOY_RUNBOOK section K.6, K.7, K.9). Runs on the developer machine; every
# command runs inside the local server t09-host. One sub-command per trial; the output is the evidence the runbook records.
#
#   bash scripts/local-reference/checks.sh liveness | secrets | bootstrap | image-ids | isolation | egress | redirect-hsts | client-address
#
# Settings (environment): IMAGE_TAG (default t09), T09_STATE_DIR (default /tmp/t09-state).

set -euo pipefail
export MSYS_NO_PATHCONV=1

STATE_DIR="${T09_STATE_DIR:-/tmp/t09-state}"
IMAGE_TAG="${IMAGE_TAG:-t09}"
HOST="t09-host"
FILES="--env-file /etc/shopfolio/release.local.env --env-file /etc/shopfolio/settings.env -f deploy/compose.yaml -f deploy/compose.local-reference.yaml"
CURL_CA="--cacert /etc/shopfolio/caddy-root.crt"

cd "$(dirname "$0")/../.."
# The trial scripts are the repository's current ones: copy them in before each run.
tar -cf - deploy/firewall deploy/backup scripts/local-reference | docker exec -i "$HOST" tar -xf - -C /opt/shopfolio

on_host() { docker exec "$HOST" sh -c "cd /opt/shopfolio && $1"; }
stamp() { date -u +%Y-%m-%dT%H:%M:%SZ; }

# The proxy's own local certificate authority signs the site certificate; the server trusts exactly that root for the trials.
prepare_trust() {
  on_host "docker cp shopfolio-proxy-1:/data/caddy/pki/authorities/local/root.crt /etc/shopfolio/caddy-root.crt"
  on_host "grep -q shopfolio.internal /etc/hosts || printf '127.0.0.1 shopfolio.internal\n::1 shopfolio.internal\n' >>/etc/hosts"
}

case "${1:-}" in
  liveness)
    prepare_trust
    echo "$(stamp) services:"
    on_host "docker compose $FILES ps --format 'table {{.Service}}\t{{.Status}}'"
    echo "$(stamp) inside the application container:"
    on_host "docker compose $FILES exec -T app bash -c \"exec 3<>/dev/tcp/127.0.0.1/8080 && printf 'GET /sistem/canlilik HTTP/1.0\\r\\n\\r\\n' >&3 && head -n 1 <&3\""
    echo "$(stamp) from the server, by domain name, through the proxy (the proxy's own local certificate):"
    on_host "curl -sS $CURL_CA -o /dev/null -w 'https://shopfolio.internal/sistem/canlilik -> %{http_code}\n' https://shopfolio.internal/sistem/canlilik"
    ;;
  secrets)
    for service in app database backup; do
      echo "-- $service: ls -ln /run/secrets"
      on_host "docker compose $FILES exec -T $service ls -ln /run/secrets | awk 'NR>1 {print \$1, \$3\":\"\$4, \$9}'"
    done
    ;;
  bootstrap)
    echo "-- \\du"
    on_host "docker compose $FILES exec -T database psql -U postgres -d shopfolio -c '\\du'"
    echo "-- \\drds"
    on_host "docker compose $FILES exec -T database psql -U postgres -d shopfolio -c '\\drds'"
    echo "-- founding user over the network must be refused"
    on_host "docker compose $FILES exec -T database psql -h 127.0.0.1 -U postgres -d shopfolio -Atc 'select 1' 2>&1 | head -n 3 || true"
    echo "-- trust lines in pg_hba.conf"
    on_host "docker compose $FILES exec -T database psql -U postgres -d shopfolio -Atc \"select count(*) from pg_hba_file_rules where auth_method = 'trust'\""
    echo "-- passwords in the database container's log (a verifier is expected, a plain value is not)"
    on_host "docker compose $FILES logs database 2>&1 | grep -ci \"password '\" || true"
    ;;
  image-ids)
    echo "-- inside the server: running service, reference, manifest digest, configuration digest"
    inner="$(docker exec "$HOST" sh /opt/shopfolio/scripts/local-reference/host-image-ids.sh "$FILES")"
    echo "$inner"
    echo "-- outside the server: the image ID of the build (the configuration digest) and the comparison"
    status=0
    for name in app database backup; do
      outer="$(docker image inspect --format '{{.Id}}' "shopfolio-$name:$IMAGE_TAG")"
      config="$(echo "$inner" | awk -v s="$name" '$1 == s {print $4}')"
      if [ "$outer" = "$config" ]; then
        echo "ok:   $name  $outer  (outside) = $config  (inside, running)"
      else
        echo "FAIL: $name  $outer  (outside) != $config  (inside, running)"
        status=1
      fi
    done
    exit "$status"
    ;;
  isolation)
    on_host "COMPOSE_FILES='$FILES' sh deploy/firewall/installation-checks.sh isolation"
    ;;
  egress)
    # The name of the object storage is not resolvable by the throw-away containers of the trial (see compose.local-reference.yaml);
    # its address is given instead. The firewall rule itself was applied by name (OBJECT_STORAGE_HOSTS=t09-minio).
    minio_ip="$(docker inspect --format '{{(index .NetworkSettings.Networks "t09-net").IPAddress}}' t09-minio)"
    on_host "COMPOSE_FILES='$FILES' OBJECT_STORAGE_HOST=$minio_ip sh deploy/firewall/installation-checks.sh egress"
    echo "-- DNS from a container on a filtered network while the rule is applied (D-31 (b)): a public name must resolve"
    on_host "docker run --rm --network shopfolio-db-egress alpine:3.24.3@sha256:be2656061b536c1883c4034790e21d0a09c5aaaf51968b89368a3c0371ea2c9a nslookup example.com 2>&1 | grep -c '^Address: [0-9]' | sed 's/^/addresses returned for example.com: /'"
    ;;
  redirect-hsts)
    prepare_trust
    on_host "COMPOSE_FILES='$FILES' SHOPFOLIO_DOMAIN=shopfolio.internal CURL_OPTS='$CURL_CA' sh deploy/firewall/installation-checks.sh redirect-hsts"
    ;;
  client-address)
    "$(dirname "$0")/client-address.sh"
    ;;
  *)
    echo "usage: $0 liveness | secrets | bootstrap | image-ids | isolation | egress | redirect-hsts | client-address" >&2
    exit 2
    ;;
esac
