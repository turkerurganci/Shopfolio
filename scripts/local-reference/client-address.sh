#!/usr/bin/env bash
# Local reference installation: the client address trial (T09 criterion 7 / KK7; DEPLOY_RUNBOOK K.7; 05 6.3.3; 09 15.1 item 8).
#
# One IPv4 and one IPv6 request come from a separate client container (t09-client) on the outer network, so the source address is
# the client's own and not the server's loopback. The server's own Docker publishes ports 80 and 443 of the proxy. Two places
# are compared with the client's real addresses:
#   1. the proxy's access log (installation-checks.sh client-address reads remote_ip);
#   2. what reaches the application: a packet capture in the application container's network namespace shows the X-Forwarded-For
#      header the proxy sends (the application's own client address field is T215 criterion 3; the empty image has none yet).
#
#   bash scripts/local-reference/client-address.sh

set -euo pipefail
export MSYS_NO_PATHCONV=1

HOST="t09-host"
CLIENT="t09-client"
CAPTURE="t09-capture"
CLIENT_IMAGE="curlimages/curl:8.21.0"
FILES="--env-file /etc/shopfolio/release.local.env --env-file /etc/shopfolio/settings.env -f deploy/compose.yaml -f deploy/compose.local-reference.yaml"

net_value() { docker inspect --format "{{(index .NetworkSettings.Networks \"t09-net\").$2}}" "$1"; }

docker rm -f "$CLIENT" >/dev/null 2>&1 || true
docker run -d --name "$CLIENT" --network t09-net "$CLIENT_IMAGE" sleep 600 >/dev/null
SERVER_V4="$(net_value "$HOST" IPAddress)"
SERVER_V6="$(net_value "$HOST" GlobalIPv6Address)"
CLIENT_V4="$(net_value "$CLIENT" IPAddress)"
CLIENT_V6="$(net_value "$CLIENT" GlobalIPv6Address)"
echo "server ${SERVER_V4} / ${SERVER_V6}; client ${CLIENT_V4} / ${CLIENT_V6}"

echo "== capture in the application container's network namespace (inside the server)"
docker exec "$HOST" docker rm -f "$CAPTURE" >/dev/null 2>&1 || true
docker exec "$HOST" docker run -d --name "$CAPTURE" --network container:shopfolio-app-1 --cap-add NET_RAW --cap-add NET_ADMIN alpine \
  sh -c "apk add -q tcpdump && exec tcpdump -i any -A -nn -l -U 'tcp dst port 8080' >/tmp/capture.txt 2>/dev/null" >/dev/null
for _ in $(seq 1 30); do
  docker exec "$HOST" docker exec "$CAPTURE" pgrep tcpdump >/dev/null 2>&1 && break
  sleep 1
done
sleep 2

echo "== requests from the client: one over IPv4, one over IPv6"
docker exec "$CLIENT" curl -4 -sS -k -o /dev/null -w 'IPv4 request -> %{http_code}\n' \
  --resolve "shopfolio.internal:443:${SERVER_V4}" https://shopfolio.internal/client-address-check-v4 || true
docker exec "$CLIENT" curl -6 -sS -k -o /dev/null -w 'IPv6 request -> %{http_code}\n' \
  --resolve "shopfolio.internal:443:[${SERVER_V6}]" https://shopfolio.internal/client-address-check-v6 || true
sleep 2

echo "== place 1: the proxy's access log, through the repository's own trial script"
docker exec "$HOST" sh -c "cd /opt/shopfolio && SKIP_REQUESTS=1 COMPOSE_FILES='$FILES' SHOPFOLIO_DOMAIN=shopfolio.internal EXPECT_IPV4='${CLIENT_V4}' EXPECT_IPV6='${CLIENT_V6}' sh deploy/firewall/installation-checks.sh client-address" || FAILED=1

echo "== place 2: the X-Forwarded-For header that reaches the application (packet capture)"
docker exec "$HOST" docker exec "$CAPTURE" grep -a -i -B1 -A0 'x-forwarded-for' /tmp/capture.txt | grep -a -i 'x-forwarded-for\|client-address-check' || true
v4_count="$(docker exec "$HOST" docker exec "$CAPTURE" grep -a -i -c "x-forwarded-for: ${CLIENT_V4}" /tmp/capture.txt || true)"
v6_count="$(docker exec "$HOST" docker exec "$CAPTURE" grep -a -i -c "x-forwarded-for: ${CLIENT_V6}" /tmp/capture.txt || true)"
echo "headers carrying the IPv4 client address ${CLIENT_V4}: ${v4_count}; carrying the IPv6 client address ${CLIENT_V6}: ${v6_count}"
if [ "${v4_count:-0}" -ge 1 ] && [ "${v6_count:-0}" -ge 1 ]; then
  echo "ok:   the application receives the client's own address for both requests"
else
  echo "FAIL: the header does not carry both client addresses"
  FAILED=1
fi

docker exec "$HOST" docker rm -f "$CAPTURE" >/dev/null 2>&1 || true
docker rm -f "$CLIENT" >/dev/null 2>&1 || true
exit "${FAILED:-0}"
