#!/bin/sh
# Clean sample of the setup script scan (09 19.2 item 3): the password reaches psql through its own backtick command.
# This file is not part of the deployment; the scan test reads it as text.
set -eu
{
  printf '%s\n' "\\set app_password \`cat /run/secrets/db_setup_app_password\`"
  cat roles.sql
} | psql -X -v ON_ERROR_STOP=1 --file -
