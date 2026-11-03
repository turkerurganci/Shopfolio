#!/bin/sh
# Deliberate violation sample of the setup script scan (09 19.2 item 3): the shell trace is on and would print the password.
# This file is not part of the deployment; the scan test reads it as text. The violation is on line 5.
set -eu
set -x
psql -X --file -
