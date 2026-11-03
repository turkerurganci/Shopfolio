#!/bin/sh
# Installation checks of the Compose definition and its network (T05; run on the server by T09, 11 T05 KK2, KK4, KK8, KK9).
# The checks need a real server with Docker, the running installation and, for `egress`, the firewall rules applied
# (docker-user-egress.sh). They are not run in CI.
#
#   installation-checks.sh isolation       KK2: the proxy and the backup tool cannot reach the database
#   installation-checks.sh egress          KK4: database and backup subnets are refused except the object storage
#   installation-checks.sh redirect-hsts   KK8: HTTP redirects, HSTS is one year, no sub-domain, no preload
#   installation-checks.sh client-address  KK9: an IPv4 and an IPv6 request show the same client address in two places
#
# Settings (environment):
#   COMPOSE_FILES   Compose command's file and env arguments; default: the installation's own
#   SHOPFOLIO_DOMAIN                    redirect-hsts, client-address
#   OBJECT_STORAGE_HOST                 egress: a host name the object storage answers on (must pass)
#   FOREIGN_ADDRESS                     egress: an address that must be refused; default 1.1.1.1
#   CURL_OPTS                           extra curl options (for example -k against a local certificate)
#   EXPECT_IPV4, EXPECT_IPV6            client-address: this machine's public addresses, to compare with the proxy's log
# POSIX shell only.

set -eu

COMPOSE_FILES="${COMPOSE_FILES:---env-file deploy/release.env --env-file /etc/shopfolio/settings.env -f deploy/compose.yaml}"
FOREIGN_ADDRESS="${FOREIGN_ADDRESS:-1.1.1.1}"
CURL_OPTS="${CURL_OPTS:-}"
FAILED=0

compose() {
  # shellcheck disable=SC2086
  docker compose $COMPOSE_FILES "$@"
}

ok() { echo "ok:   $1"; }
bad() { echo "FAIL: $1"; FAILED=1; }

# connect <network> <host> <port>: a throw-away container of the backup image (busybox nc) tries a TCP connection.
connect() {
  image="$(compose config --images | grep shopfolio-backup | head -n 1)"
  docker run --rm --network "$1" --entrypoint sh "$image" -c "nc -w 5 $2 $3 </dev/null" >/dev/null 2>&1
}

check_isolation() {
  db_ip="$(docker inspect -f '{{ (index .NetworkSettings.Networks "shopfolio-app-data").IPAddress }}' "$(compose ps -q database)")"
  [ -n "$db_ip" ] || { bad "database address on shopfolio-app-data not found"; return; }

  # Control: a container on the database's own network reaches it (otherwise the negative results below prove nothing).
  if connect shopfolio-app-data database 5432; then ok "control: a container on app-data reaches database:5432"; else bad "control: database:5432 not reachable on app-data"; fi

  for service in proxy backup; do
    if compose exec -T "$service" sh -c "nc -w 3 database 5432 </dev/null" >/dev/null 2>&1; then
      bad "$service reaches database:5432 by name"
    else
      ok "$service cannot reach database:5432 by name"
    fi
    if compose exec -T "$service" sh -c "nc -w 3 $db_ip 5432 </dev/null" >/dev/null 2>&1; then
      bad "$service reaches $db_ip:5432 by address"
    else
      ok "$service cannot reach $db_ip:5432 by address"
    fi
  done
}

check_egress() {
  : "${OBJECT_STORAGE_HOST:?OBJECT_STORAGE_HOST is required}"
  for network in shopfolio-db-egress shopfolio-backup-egress; do
    if connect "$network" "$OBJECT_STORAGE_HOST" 443; then ok "$network reaches the object storage"; else bad "$network cannot reach the object storage"; fi
    if connect "$network" "$FOREIGN_ADDRESS" 443; then bad "$network reaches $FOREIGN_ADDRESS:443 (must be refused)"; else ok "$network is refused at $FOREIGN_ADDRESS:443"; fi
    if connect "$network" "$FOREIGN_ADDRESS" 80; then bad "$network reaches $FOREIGN_ADDRESS:80 (must be refused)"; else ok "$network is refused at $FOREIGN_ADDRESS:80"; fi
  done
}

check_redirect_hsts() {
  : "${SHOPFOLIO_DOMAIN:?SHOPFOLIO_DOMAIN is required}"
  # shellcheck disable=SC2086
  http="$(curl -sS -I $CURL_OPTS "http://$SHOPFOLIO_DOMAIN/" | tr -d '\r')"
  echo "$http" | head -n 1 | grep -Eq ' (301|302|307|308) ' && ok "HTTP answers with a redirect" || bad "HTTP does not redirect"
  echo "$http" | grep -iq "^location: https://$SHOPFOLIO_DOMAIN" && ok "redirect points to HTTPS" || bad "redirect does not point to https://$SHOPFOLIO_DOMAIN"
  # shellcheck disable=SC2086
  https="$(curl -sS -I $CURL_OPTS "https://$SHOPFOLIO_DOMAIN/" | tr -d '\r')"
  hsts="$(echo "$https" | grep -i '^strict-transport-security:' | sed 's/^[^:]*: *//')"
  [ "$hsts" = "max-age=31536000" ] && ok "HSTS is exactly: $hsts" || bad "HSTS header is '$hsts' (expected max-age=31536000, no includeSubDomains, no preload)"
}

check_client_address() {
  : "${SHOPFOLIO_DOMAIN:?SHOPFOLIO_DOMAIN is required}"
  # shellcheck disable=SC2086
  curl -4 -sS -o /dev/null $CURL_OPTS "https://$SHOPFOLIO_DOMAIN/client-address-check-v4" || true
  # shellcheck disable=SC2086
  curl -6 -sS -o /dev/null $CURL_OPTS "https://$SHOPFOLIO_DOMAIN/client-address-check-v6" || true
  for family in v4 v6; do
    line="$(compose exec -T proxy sh -c "grep client-address-check-$family /var/log/caddy/access.log | tail -n 1" || true)"
    seen="$(echo "$line" | sed -n 's/.*"remote_ip":"\([^"]*\)".*/\1/p')"
    echo "info: proxy access log, $family request: remote_ip=$seen"
    expected=""
    [ "$family" = "v4" ] && expected="${EXPECT_IPV4:-}"
    [ "$family" = "v6" ] && expected="${EXPECT_IPV6:-}"
    if [ -n "$expected" ]; then
      [ "$seen" = "$expected" ] && ok "$family: the proxy sees $expected" || bad "$family: the proxy sees '$seen', expected $expected (a gateway address means the user-space proxy is in the way)"
    fi
  done
  echo "manual: the application's own log must show the same address for the same two requests (05 6.3.3);"
  echo "        if it differs, the application does not trust the proxy and counts every client as one address."
}

case "${1:-}" in
  isolation) check_isolation ;;
  egress) check_egress ;;
  redirect-hsts) check_redirect_hsts ;;
  client-address) check_client_address ;;
  *) echo "usage: $0 isolation|egress|redirect-hsts|client-address" >&2; exit 2 ;;
esac

exit "$FAILED"
