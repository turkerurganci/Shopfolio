#!/bin/sh
# Helpers of the script tests (09 17 item 7: shell tests of the backup scripts). POSIX shell only. Sourced by the *.test.sh files.
#
# A script test runs the real script of deploy/images against a stub of the external tool (restic, pgbackrest) that lives in
# tests/scripts/stubs/ and writes a temporary directory of its own. The tests never touch a network, a database or the repository.

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
STUBS_DIR="${REPO_ROOT}/tests/scripts/stubs"
FAILURES=0
ASSERTIONS=0
TEST_NAME="$(basename "$0")"

fail() {
  FAILURES=$((FAILURES + 1))
  echo "FAIL [${TEST_NAME}] $1" >&2
}

pass() {
  ASSERTIONS=$((ASSERTIONS + 1))
}

# assert_eq <expected> <actual> <description>
assert_eq() {
  if [ "$1" = "$2" ]; then
    pass
  else
    fail "$3: expected '$1', got '$2'"
  fi
}

# assert_contains <file> <fixed text> <description>
assert_contains() {
  if grep -qF -- "$2" "$1"; then
    pass
  else
    fail "$3: '$2' not found in $1"
  fi
}

# assert_not_contains <file> <fixed text> <description>
assert_not_contains() {
  if grep -qF -- "$2" "$1"; then
    fail "$3: '$2' found in $1"
  else
    pass
  fi
}

# new_workdir: a fresh temporary directory, removed when the test ends.
new_workdir() {
  WORKDIR="$(mktemp -d)"
  trap 'rm -rf "$WORKDIR"' EXIT
}

# install_stub <name>: copies tests/scripts/stubs/<name> to $WORKDIR/bin and makes it executable (git keeps no mode on Windows).
install_stub() {
  mkdir -p "${WORKDIR}/bin"
  cp "${STUBS_DIR}/$1" "${WORKDIR}/bin/$1"
  chmod 0755 "${WORKDIR}/bin/$1"
}

finish() {
  if [ "$FAILURES" -ne 0 ]; then
    echo "${TEST_NAME}: ${FAILURES} failure(s) in $((ASSERTIONS + FAILURES)) assertion(s)" >&2
    exit 1
  fi
  echo "ok ${TEST_NAME} (${ASSERTIONS} assertions)"
}
