#!/usr/bin/env bash
# Self-test for the pre-commit secret guard (scripts/git-hooks/pre-commit).
# Builds a throwaway git repo, stages one sample at a time, and checks the
# hook's exit code. Writes nothing outside the temp directory.
#
# Usage: bash scripts/git-hooks/test-pre-commit.sh

set -uo pipefail

HOOK="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/pre-commit"
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

cd "$TMP" || exit 2
git init -q .
git config core.autocrlf false
git config core.safecrlf false
git config user.email t@example.com
git config user.name t

PASS=0
FAIL=0

# check <expected-exit> <label> <path> <content>
check() {
  local expect="$1" label="$2" path="$3" content="$4" code
  mkdir -p "$(dirname "$path")"
  printf '%s\n' "$content" > "$path"
  git add -f -- "$path"
  bash "$HOOK" >/dev/null 2>&1
  code=$?
  git rm -q --cached -f -- "$path"
  rm -f "$path"
  if [ "$code" = "$expect" ]; then
    PASS=$((PASS + 1))
    echo "ok    [$expect] $label"
  else
    FAIL=$((FAIL + 1))
    echo "FAIL  expected $expect got $code: $label"
  fi
}

echo "--- must be caught (exit 1) ---"
check 1 "config.env value"           "app/config.env"      'API_KEY=sk_live_9f3a1b2c3d4e'
check 1 "yaml password"              "app/settings.yaml"   'password: hunter2hunter2'
check 1 "json password"              "app/appsettings.json" '  "password": "abc12345xyz",'
check 1 "cs string literal"          "src/Db.cs"           'var Password = "abc12345xyz";'
check 1 "cs connection string"       "src/Db2.cs"          'ConnectionString = "Server=db;Password=abc12345;";'
check 1 "ts string literal"          "src/a.ts"            "const apiKey = 'abcd1234efgh5678';"
check 1 "py string literal"          "src/a.py"            'API_KEY = "abcd1234efgh5678"'
check 1 "private key block (txt)"    "notes/k.txt"         "-----BEGIN RSA PRIV""ATE KEY-----"
check 1 "private key in lock file"   "packages.lock.json"  "-----BEGIN PRIV""ATE KEY-----"
check 1 "private key in cs"          "src/k.cs"            "// -----BEGIN PRIV""ATE KEY-----"
check 1 ".env path"                  ".env"                'A=1'

echo "--- must pass (exit 0) ---"
check 0 "cs type CancellationToken"  "src/B.cs"            'public Task Run(CancellationToken cancellationToken = default) {}'
check 0 "cs local CancellationToken" "src/B2.cs"           'CancellationToken ct = default;'
check 0 "cs token expression"        "src/B3.cs"           'Token = ctx.CancellationToken;'
check 0 "cs ConnectionString expr"   "src/C.cs"            'options.ConnectionString = builder.ConnectionString;'
check 0 "cs UserSecrets"             "src/D.cs"            'builder.Configuration.AddUserSecrets<Program>(); // UserSecrets'
check 0 "lock file UserSecrets"      "src/Api/packages.lock.json" '"Microsoft.Extensions.Configuration.UserSecrets": "10.0.11",'
check 0 "lock file token key"        "package-lock.json"   '"token": "abcdefgh12345678"'
check 0 "yarn.lock"                  "yarn.lock"           'password: abcdefgh12345678'
check 0 "Cargo.lock"                 "Cargo.lock"          'api_key = "abcdefgh12345678"'
check 0 "placeholder env value"      "app/config.env"      'API_KEY=<YOUR_KEY_HERE>'
check 0 "markdown prose"             "docs/a.md"           'Sorun: ek sir (token/link) gerektigi yazilmiyor.'
check 0 ".env.example"               ".env.example"        'API_KEY=sk_live_9f3a1b2c3d4e'

echo "--- $PASS passed, $FAIL failed ---"
[ "$FAIL" -eq 0 ]
