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
check 1 "cs @ prefix"                "src/v1.cs"           'var Password = @"abc12345xyz";'
check 1 "cs $ prefix"                "src/v2.cs"           'var Password = $"abc12345xyz";'
check 1 "cs $@ prefix"               "src/v3.cs"           'var Password = $@"abc12345xyz";'
check 1 "cs @$ prefix"               "src/v4.cs"           'var Password = @$"abc12345xyz";'
check 1 "cs raw literal"             "src/v5.cs"           'var Password = """abc12345xyz""";'
check 1 "cs $$ raw literal"          "src/v6.cs"           'var Password = $$"""abc12345xyz""";'
check 1 "cs u8 literal"              "src/v7.cs"           'var ApiKey = "abc12345xyz"u8;'
check 1 "cs const string"            "src/v8.cs"           'const string ApiKey = "abc12345xyz";'
check 1 "cs ?? literal"              "src/v10.cs"          'var Password = cfg["pw"] ?? "abc12345xyz";'
check 1 "ts typed const"             "src/v18.ts"          'const apiKey: string = "abcd1234efgh5678";'
check 1 "ts typed field"             "src/v19.ts"          'private readonly token: string = "abcd1234efgh5678";'
check 1 "ts object property"         "src/v20.ts"          'apiKey: "abcd1234efgh5678",'
check 1 "ts backtick"                "src/v17.ts"          'const apiKey = `abcd1234efgh5678`;'
check 1 "ts || literal"              "src/v21.ts"          'const token = process.env.TOKEN || "abcd1234efgh5678";'
check 1 "js String.raw"              "src/v22.js"          'const token = String.raw`abcd1234efgh5678`;'
check 1 "py f-string"                "src/v24.py"          'API_KEY = f"abcd1234efgh5678"'
check 1 "py r-string"                "src/v25.py"          'API_KEY = r"abcd1234efgh5678"'
check 1 "py b-string"                "src/v26.py"          'API_KEY = b"abcd1234efgh5678"'
check 1 "py typed"                   "src/v27.py"          'API_KEY: str = "abcd1234efgh5678"'
check 1 "py getenv default"          "src/v28.py"          'API_KEY = os.getenv("API_KEY", "abcd1234efgh5678")'
check 1 "py dict"                    "src/v29.py"          '"api_key": "abcd1234efgh5678",'
check 1 "go :="                      "src/v30.go"          'apiKey := "abcd1234efgh5678"'
check 1 "go var typed"               "src/v31.go"          'var apiKey string = "abcd1234efgh5678"'
check 1 "go raw string"              "src/v32.go"          'var apiKey = `abcd1234efgh5678`'
check 1 "kotlin typed"               "src/v33.kt"          'val apiKey: String = "abcd1234efgh5678"'
check 1 "rust const typed"           "src/v34.rs"          'const API_KEY: &str = "abcd1234efgh5678";'
check 1 "rust raw string"            "src/v35.rs"          'let api_key = r#"abcd1234efgh5678"#;'
check 1 "swift typed"                "src/v36.swift"       'let apiKey: String = "abcd1234efgh5678"'
check 1 "php =>"                     "src/v37.php"         "'api_key' => 'abcd1234efgh5678',"
check 1 "ruby =>"                    "src/v38.rb"          '"api_key" => "abcd1234efgh5678",'
check 1 "ruby symbol key"            "src/v39.rb"          'api_key: "abcd1234efgh5678",'
check 1 "java string"                "src/v40.java"        'String apiKey = "abcd1234efgh5678";'
check 1 "private key block (txt)"   "notes/k.txt"         "-----BEGIN RSA PRIV""ATE KEY-----"
check 1 "private key in lock file"   "packages.lock.json"  "-----BEGIN PRIV""ATE KEY-----"
check 1 "private key in cs"          "src/k.cs"            "// -----BEGIN PRIV""ATE KEY-----"
check 1 ".env path"                  ".env"                'A=1'

echo "--- must pass (exit 0) ---"
check 0 "cs type CancellationToken"  "src/B.cs"            'public Task Run(CancellationToken cancellationToken = default) {}'
check 0 "cs local CancellationToken" "src/B2.cs"           'CancellationToken ct = default;'
check 0 "cs token expression"        "src/B3.cs"           'Token = ctx.CancellationToken;'
check 0 "cs ConnectionString expr"   "src/C.cs"            'options.ConnectionString = builder.ConnectionString;'
check 0 "cs UserSecrets"             "src/D.cs"            'builder.Configuration.AddUserSecrets<Program>(); // UserSecrets'
check 0 "cs char literal"            "src/E1.cs"           "var TokenSep = 'x';"
check 0 "cs comment"                 "src/E2.cs"           '// Password = "abc12345xyz"'
check 0 "cs nameof"                  "src/E3.cs"           'var ApiKeyName = nameof(ApiKey);'
check 0 "cs string.Empty"            "src/E4.cs"           'Password = string.Empty;'
check 0 "cs placeholder literal"     "src/E5.cs"           'var Password = "<YOUR_PASSWORD>";'
check 0 "ts process.env"             "src/E6.ts"           'const token = process.env.TOKEN;'
check 0 "ts short literal"           "src/E7.ts"           'const token = "abc";'
check 0 "lock file UserSecrets"     "src/Api/packages.lock.json" '"Microsoft.Extensions.Configuration.UserSecrets": "10.0.11",'
check 0 "lock file token key"        "package-lock.json"   '"token": "abcdefgh12345678"'
check 0 "yarn.lock"                  "yarn.lock"           'password: abcdefgh12345678'
check 0 "Cargo.lock"                 "Cargo.lock"          'api_key = "abcdefgh12345678"'
check 0 "placeholder env value"      "app/config.env"      'API_KEY=<YOUR_KEY_HERE>'
check 0 "markdown prose"             "docs/a.md"           'Sorun: ek sir (token/link) gerektigi yazilmiyor.'
check 0 ".env.example"               ".env.example"        'API_KEY=sk_live_9f3a1b2c3d4e'

echo "--- $PASS passed, $FAIL failed ---"
[ "$FAIL" -eq 0 ]
