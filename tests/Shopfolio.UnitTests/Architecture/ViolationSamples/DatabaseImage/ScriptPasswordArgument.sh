#!/bin/sh
# Deliberate violation sample of the setup script scan (09 19.2 item 3): the password is a psql option, so a process list shows it.
# This file is not part of the deployment; the scan test reads it as text. The violation is on line 6.
set -eu
psql -X -v ON_ERROR_STOP=1 \
  -v "app_password=$(cat /run/secrets/db_setup_app_password)" --file -
