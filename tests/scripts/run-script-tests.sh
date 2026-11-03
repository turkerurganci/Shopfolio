#!/bin/sh
# Runs every script test under tests/scripts (11 T07 KK3, KK4; 09 17 item 7). Part of the CI step "3. Unit" (UNIT_TEST_CMD in
# .github/ci-commands.env). Each *.test.sh runs in its own shell and uses a temporary directory of its own; none touches the
# network, a database or the repository. SCRIPT_TEST_SHELL picks the shell (default sh); the tests are POSIX.

set -u

cd "$(dirname "$0")"
SHELL_UNDER_TEST="${SCRIPT_TEST_SHELL:-sh}"
failed=0
count=0
for test_file in ./*.test.sh; do
  count=$((count + 1))
  if ! "$SHELL_UNDER_TEST" "$test_file"; then
    failed=$((failed + 1))
  fi
done

if [ "$count" -eq 0 ]; then
  echo "run-script-tests: no *.test.sh file found" >&2
  exit 1
fi
if [ "$failed" -ne 0 ]; then
  echo "run-script-tests: ${failed} of ${count} script test file(s) failed" >&2
  exit 1
fi
echo "run-script-tests: ${count} script test file(s) passed"
