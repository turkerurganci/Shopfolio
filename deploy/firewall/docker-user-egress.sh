#!/bin/sh
# Egress limit of the database and the backup tool (05 3.2.5, MK-14; D-01 (c), K-987).
#
#   docker-user-egress.sh apply     install the rules (idempotent; run as root, after Docker is up)
#   docker-user-egress.sh remove    take the rules out
#   docker-user-egress.sh status    print the chain
#
# Compose cannot limit a network's outbound traffic by destination: a network is either closed or open to every address
# (05 3.2.5). So the destination is limited in the server's firewall. Container traffic is forwarded traffic: it does not pass
# the host's INPUT/OUTPUT rules, and the place for user rules that Docker evaluates before its own is the DOCKER-USER chain of
# the FORWARD path (Docker documentation, "Docker with iptables", accessed 2026-11-03: "A placeholder for user-defined rules
# that will be processed before rules in the DOCKER-FORWARD and DOCKER chains").
#
# Two subnets are limited, the ones deploy/compose.yaml fixes for `db-egress` and `backup-egress`. From each, the only
# connection that passes is TCP 443 to the object storage's addresses; everything else is rejected. Both networks are IPv4-only,
# so there is no IPv6 way out to limit.
#
# Settings (environment):
#   OBJECT_STORAGE_HOSTS   required; space-separated host names of the object storage endpoint(s). They are resolved to IPv4
#                          addresses when `apply` runs. A provider can change the addresses behind a name: run `apply` again
#                          on a timer (the chain is rebuilt each time). The install steps are in DEPLOY_RUNBOOK (T09).
#
# The rules are lost at reboot: run `apply` once Docker is up at each boot (T09 installs the unit; D-31).
# POSIX shell only.

set -eu

CHAIN="SHOPFOLIO-EGRESS"
DB_SUBNET="172.30.2.0/24"
BACKUP_SUBNET="172.30.3.0/24"
OBJECT_STORAGE_PORT="443"

fail() {
  echo "error: $1" >&2
  exit 1
}

require_root() {
  [ "$(id -u)" -eq 0 ] || fail "run as root"
  command -v iptables >/dev/null 2>&1 || fail "iptables not found"
}

resolve_storage() {
  [ -n "${OBJECT_STORAGE_HOSTS:-}" ] || fail "OBJECT_STORAGE_HOSTS is required"
  for host in $OBJECT_STORAGE_HOSTS; do
    getent ahostsv4 "$host" | awk '{print $1}' | sort -u
  done
}

remove_rules() {
  for subnet in "$DB_SUBNET" "$BACKUP_SUBNET"; do
    while iptables -C DOCKER-USER -s "$subnet" -j "$CHAIN" 2>/dev/null; do
      iptables -D DOCKER-USER -s "$subnet" -j "$CHAIN"
    done
  done
  if iptables -nL "$CHAIN" >/dev/null 2>&1; then
    iptables -F "$CHAIN"
    iptables -X "$CHAIN"
  fi
}

apply_rules() {
  addresses="$(resolve_storage)"
  [ -n "$addresses" ] || fail "no IPv4 address found for: $OBJECT_STORAGE_HOSTS"
  iptables -nL DOCKER-USER >/dev/null 2>&1 || fail "chain DOCKER-USER not found (is Docker running?)"

  remove_rules
  iptables -N "$CHAIN"
  for address in $addresses; do
    # RETURN hands the packet back to DOCKER-USER and on to Docker's own rules, which allow the outbound connection.
    iptables -A "$CHAIN" -d "$address" -p tcp --dport "$OBJECT_STORAGE_PORT" -j RETURN
  done
  iptables -A "$CHAIN" -j REJECT
  # At the top of DOCKER-USER, ahead of anything else, so that nothing earlier accepts the traffic.
  iptables -I DOCKER-USER 1 -s "$BACKUP_SUBNET" -j "$CHAIN"
  iptables -I DOCKER-USER 1 -s "$DB_SUBNET" -j "$CHAIN"
  echo "ok: $DB_SUBNET and $BACKUP_SUBNET may reach only tcp/$OBJECT_STORAGE_PORT of: $(echo "$addresses" | tr '\n' ' ')"
}

case "${1:-}" in
  apply)  require_root; apply_rules ;;
  remove) require_root; remove_rules; echo "ok: rules removed" ;;
  status) require_root; iptables -nvL DOCKER-USER; iptables -nvL "$CHAIN" ;;
  *) fail "usage: $0 apply|remove|status" ;;
esac
