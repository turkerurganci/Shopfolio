#!/usr/bin/env bash
# Self-test for the pre-push CI guard (scripts/git-hooks/pre-push, Layer 2).
# Builds a throwaway git repo and a fake `gh` that reports the branch's last
# CI run, then feeds the hook one push line at a time and checks its exit code.
# Writes nothing outside the temp directory.
#
# Usage: bash scripts/git-hooks/test-pre-push.sh

set -uo pipefail

HOOK="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/pre-push"
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

mkdir -p "$TMP/bin" "$TMP/repo"
cd "$TMP/repo" || exit 2
git init -q .
git config user.email t@example.com
git config user.name t
git commit -q --allow-empty -m "a"; A="$(git rev-parse HEAD)"
git commit -q --allow-empty -m "b"; B="$(git rev-parse HEAD)"
git checkout -q --orphan other
git commit -q --allow-empty -m "c"; C="$(git rev-parse HEAD)"

# Fake gh: auth ok; `run list` answers from $FAKE_RUN.
cat > "$TMP/bin/gh" <<'SH'
#!/usr/bin/env bash
[ "$1" = "auth" ] && exit 0
[ "$1" = "run" ] && { printf '%s\n' "$FAKE_RUN"; exit 0; }
exit 1
SH
chmod +x "$TMP/bin/gh"
export PATH="$TMP/bin:$PATH"

PASS=0
FAIL=0

# check <expected-exit> <label> <run-json> <pushed-sha>
check() {
  local expect="$1" label="$2" code
  FAKE_RUN="$3" bash "$HOOK" origin x >/dev/null 2>&1 <<< "refs/heads/x $4 refs/heads/chore/x 0000000000000000000000000000000000000000"
  code=$?
  if [ "$code" = "$expect" ]; then
    PASS=$((PASS + 1)); echo "ok    [$expect] $label"
  else
    FAIL=$((FAIL + 1)); echo "FAIL  expected $expect got $code: $label"
  fi
}

red() { printf '[{"conclusion":"failure","status":"completed","headSha":"%s"}]' "$1"; }

check 0 "green branch"                                 '[{"conclusion":"success","status":"completed","headSha":"x"}]' "$B"
check 0 "no run yet"                                   '[]' "$B"
check 0 "red branch, fix commit on top of failed one"  "$(red "$A")" "$B"
check 1 "red branch, same commit pushed again"         "$(red "$B")" "$B"
check 1 "red branch, unrelated history"                "$(red "$A")" "$C"
check 1 "red branch, run without headSha"              '[{"conclusion":"failure","status":"completed"}]' "$B"

echo "pre-push test: $PASS ok, $FAIL failed"
[ "$FAIL" = 0 ]
