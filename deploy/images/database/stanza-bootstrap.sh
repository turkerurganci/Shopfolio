#!/bin/sh
# Creates pgBackRest's stanza once the server accepts connections (05 7.6.1). The archive command of the server fails until the
# stanza exists, and the nightly job would only create it at its first run; this makes the archive transfer start with the
# first start of the server. `stanza-create` is idempotent. The loop gives up after the first success; a server that is not
# ready yet, or an object storage that cannot be reached, is retried every 30 seconds (the failing archive command meanwhile
# keeps the log files on the local disk, 05 7.6.9).
#
# The output goes to a file in the database log volume. Shell tracing stays off; no secret is written.

set -eu

LOG_DIR="${DATABASE_JOB_LOG_DIR:-/var/log/postgresql}"
exec >>"${LOG_DIR}/stanza-bootstrap.log" 2>&1

until pg_isready -q -h /var/run/postgresql; do
  sleep 5
done

until pgbackrest --stanza=shopfolio stanza-create; do
  echo "stanza-bootstrap: stanza-create failed; retrying in 30 seconds"
  sleep 30
done
echo "stanza-bootstrap: stanza shopfolio exists"
