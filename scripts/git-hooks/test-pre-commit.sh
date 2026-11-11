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
check 1 "cs placeholder concat"      "src/r1.cs"           'Password = "<" + "Zx9kLmQ2rT7vWp" + ">";'
check 1 "cs env-style concat"        "src/r2.cs"           'Password = "${" + "Zx9kLmQ2rT7vWp" + "}";'
check 1 "ts angle concat"            "src/r3.ts"           "const token = '<' + 'Zx9kLmQ2rT7vWp' + '>';"
check 1 "java angle concat"          "src/r4.java"         'String password = "<" + "Zx9kLmQ2rT7vWp" + ">";'
check 1 "cs angle concat comma"      "src/r5.cs"           'Password = "<" + "Zx9kLmQ2rT7vWp" + ">",'
check 1 "cs angle list"              "src/r6.cs"           'Password = "<", Other = "Zx9kLmQ2rT7vWp>";'
check 1 "cs angle concat variable"   "src/r7.cs"           'Password = "<" + realSecretValue + ">";'
check 1 "cs your- concat"            "src/r8.cs"           'Password = "your-" + "Zx9kLmQ2rT7vWp";'
check 1 "py angle concat no ;"       "src/r9.py"           'PASSWORD = "<" + "Zx9kLmQ2rT7vWp" + ">"'
check 1 "ts angle concat no ;"       "src/r10.ts"          "const token = '<' + 'Zx9kLmQ2rT7vWp' + '>'"
check 1 "config.env value"           "app/config.env"      'API_KEY=sk_live_9f3a1b2c3d4e'
check 1 "yaml password"              "app/settings.yaml"   'password: hunter2hunter2'
check 1 "json password"              "app/appsettings.json" '  "password": "abc12345xyz",'
check 1 "cs string literal"          "src/Db.cs"           'var Password = "abc12345xyz";'
check 1 "cs connection string"       "src/Db2.cs"          'ConnectionString = "Server=db;Password=abc12345;";'
check 1 "ts string literal"          "src/a.ts"            "const apiKey = 'abcd1234efgh5678';"
check 1 "py string literal"          "src/a.py"            'API_KEY = "abcd1234efgh5678"'
check 1 "cs @ prefix"                "src/v1.cs"           'var Password = @"abc12345xyz";'
check 1 "cs $ prefix"                "src/v2.cs"           'var Password = $"abc12345xyz";'
# Shell environment option forms (D-80): the -e / --env prefix does not hide the key.
check 1 "sh -e quoted"               "scripts/e1.sh"       '  -e "LinkKey__DerivationSecret=ZGV2LW9ubHktbWFkZS11cC1saW5r" \'
check 1 "sh -e bare"                 "scripts/e2.sh"       '  -e API_KEY=sk_live_9f3a1b2c3d4e \'
check 1 "sh --env quoted"            "scripts/e3.sh"       '  --env "DB_PASSWORD=abc12345xyz" \'
check 1 "sh --env= form"             "scripts/e4.sh"       '  --env=DB_PASSWORD=abc12345xyz \'
check 0 "sh -e placeholder"          "scripts/e5.sh"       '  -e "API_KEY=<YOUR_KEY_HERE>"'
check 0 "sh -e _FILE short path"     "scripts/e6.sh"       '  -e POSTGRES_DB=shopfolio'
# Every -e / --env option on the line is its own candidate (D-1036): a second option on the same line and an option inside a
# one-line command are caught; the same forms with a placeholder or on a comment line pass.
check 1 "sh second -e on one line"   "scripts/e7.sh"       '  -e POSTGRES_DB=shopfolio -e API_KEY=sk_live_9f3a1b2c3d4e \'
check 1 "sh docker run -e one line"  "scripts/e8.sh"       'docker run --rm -e "API_KEY=sk_live_9f3a1b2c3d4e" img'
check 1 "sh second --env= one line"  "scripts/e9.sh"       'docker run --env=POSTGRES_DB=shopfolio --env=DB_PASSWORD=abc12345xyz img'
check 1 "sh -e bare with quoted part" "scripts/e10.sh"     'docker run -e DB_PASSWORD="abc12345 xyz" img'
check 0 "sh docker run -e placeholder" "scripts/e11.sh"    'docker run --rm -e "API_KEY=<YOUR_KEY_HERE>" img'
check 0 "sh second -e placeholder"   "scripts/e12.sh"      '  -e POSTGRES_DB=shopfolio -e API_KEY=changeme \'
check 0 "sh -e on a comment line"    "scripts/e13.sh"      '# docker run --rm -e "API_KEY=sk_live_9f3a1b2c3d4e" img'
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
check 1 "cs 7-char literal"          "src/r1.cs"           'var Password = "abc1234";'
check 1 "cs 6-char literal"          "src/r2.cs"           'var Password = "abc123";'
check 1 "cs 6-char no var"           "src/r3.cs"           'Password = "abc123"'
check 1 "ts 7-char literal"          "src/r4.ts"           "const token = 'abc1234';"
check 1 "py 7-char literal"          "src/r5.py"           'PASSWORD = "abc1234"'
check 1 "cs escaped quote"           "src/r6.cs"           'var Password = "Pa\"ss12345";'
check 1 "cs verbatim doubled quote"  "src/r7.cs"           'var Password = @"Pa""ss12345";'
check 1 "cs plus concatenation"      "src/r8.cs"           'var Password = "abcd" + "efgh";'
check 1 "py implicit concatenation"  "src/r9.py"           'API_KEY = ("abcd" "efgh")'
check 1 "cs ef column + concat"      "src/m1.cs"           '    password_hash = table.Column<string>(type: "text") + "abcd1234efgh";'
check 1 "cs ef column + literal"     "src/m2.cs"           '    password_hash = "abcd1234efgh" + table.Column<string>(type: "text");'
check 1 "cs short literal + ;"       "src/s1.cs"          'var Password = "admin";'
check 1 "cs 5-digit literal + ;"     "src/s2.cs"           'var Password = "12345";'
check 1 "cs short literal + ,"       "src/s3.cs"           '    Password = "admin",'
check 1 "cs short literal + comment" "src/s4.cs"           'var Password = "root"; // dev only'
check 1 "ts short literal + ;"       "src/s5.ts"           "const token = 'admin';"
check 1 "py short literal + comment" "src/s6.py"           'PASSWORD = "admin"  # dev'
check 1 "cs short literal CRLF"      "src/s7.cs"           $'var Password = "admin";\r'
check 1 "cs escaped backslash"       "src/s8.cs"           'var Password = "P\\w0rd";'
check 1 "cs escaped tabs"            "src/s9.cs"           'var Password = "a\tb\tc\td";'
check 1 "cs verbatim trailing bs"    "src/s10.cs"          'var Password = @"abcd1234\";'
check 1 "cs verbatim path bs"        "src/s11.cs"          'var ApiKey = @"C:\k\abc12345\";'
check 1 "go raw trailing bs"         "src/s12.go"          'apiKey := `abcdefgh\`'
check 1 "cs byte array key"          "src/s13.cs"          'var secretKey = new byte[] { 0x2b, 0x7e, 0x15, 0x16, 0x28, 0xae };'
check 1 "go byte slice key"          "src/s14.go"          'var secretKey = []byte{0x01, 0x02, 0x03, 0x04}'
check 1 "ts Buffer.from array"       "src/s15.ts"          'const secretKey = Buffer.from([1, 2, 3, 4, 5, 6, 7, 8]);'
check 1 "py bytes array"             "src/s16.py"          'SECRET_KEY = bytes([1, 2, 3, 4, 5, 6, 7, 8])'
check 1 "cs numeric key"             "src/s17.cs"          'const long ApiKey = 1234567890123;'
check 1 "ts numeric key"             "src/s18.ts"          'const API_KEY = 12345678901234;'
check 1 "ruby %q key"                "src/s19.rb"          'api_key = %q(abcd1234efgh)'
check 1 "cs hex ulong key"           "src/h1.cs"           'const ulong HmacSecret = 0xDEADBEEFCAFEBABE;'
check 1 "cs hex key mixed digits"    "src/h2.cs"           'const ulong ApiKey = 0x2b7e151628aed2a6;'
check 1 "py hex int key"             "src/h3.py"           'SECRET_KEY = 0x2b7e151628aed2a6abf7158809cf4f3c'
check 1 "go hex const"               "src/h4.go"           'const apiKey = 0x2b7e151628aed2a6'
check 1 "cs 2 ulong words key"       "src/h5.cs"           'var SecretKey = new ulong[] { 0x2b7e151628aed2a6, 0xabf7158809cf4f3c };'
check 1 "cs 3 uint words key"        "src/h6.cs"           'var SecretKey = new uint[] { 0x2b7e1516, 0x28aed2a6, 0xabf71588 };'
check 1 "cs hex separators"          "src/h17.cs"          'const ulong ApiKey = 0xDEAD_BEEF_CAFE_BABE;'
check 1 "cs hex CRLF"                "src/j9.cs"           $'const ulong ApiKey = 0xDEADBEEFCAFEBABE;\r'
check 1 "cs uppercase 0X array"      "src/h10.cs"          'var SecretKey = new byte[] { 0X2B, 0X7E, 0X15, 0X16, 0X28 };'
check 1 "rust suffixed hex array"    "src/h11.rs"          'const SECRET_KEY: [u8; 4] = [0x2bu8, 0x7eu8, 0x15u8, 0x16u8];'
check 1 "java (byte) cast array"     "src/j1.java"         'var secretKey = new byte[]{(byte)0xab, (byte)0xcd, (byte)0xef, (byte)0x12};'
check 1 "java negative byte array"   "src/j2.java"         'var secretKey = new byte[]{-85, 12, -100, 7, 99};'
check 1 "kotlin negative byteArrayOf" "src/j3.kt"          'val secretKey = byteArrayOf(-85, 12, -100, 7, 99)'
check 1 "kotlin .toByte() array"     "src/j4.kt"           'val secretKey = byteArrayOf(0x2B.toByte(), 0x7E.toByte(), 0x15.toByte(), 0x16.toByte())'
check 1 "kotlin hex byteArrayOf"     "src/j5.kt"           'val secretKey = byteArrayOf(0x2b, 0x7e, 0x15, 0x16)'
check 1 "java 3 hex (byte) array"    "src/j6.java"         'var secretKey = new byte[]{(byte)0xab, (byte)0xcd, (byte)0xef};'
check 1 "cs 3 hex byte array"        "src/j7.cs"           'var secretKey = new byte[] { 0x2b, 0x7e, 0x15 };'
check 1 "rust 3 suffixed hex array"  "src/j8.rs"           'let secret_key = [0x2bu8, 0x7eu8, 0x15u8];'
check 1 "cs digit separators"       "src/h12.cs"          'const long ApiKey = 1_234_567_890_123;'
check 1 "py digit separators"        "src/h13.py"          'API_KEY = 1_234_567_890_123'
check 1 "ruby %q/ delimiter"         "src/r3.rb"           'API_KEY = %q/sk_live_9f3a1b2c3d4e/'
check 1 "ruby %W[ list"              "src/r4.rb"           'API_TOKENS = %W[sk_live_9f3a1b2c3d4e sk_test_1]'
check 1 "ruby %i list"               "src/r4b.rb"          'API_TOKENS = %i[sk_live_9f3a1b2c3d4e]'
check 1 "ruby heredoc ~"             "src/r5.rb"           $'API_TOKEN = <<~SECRET_TOKEN\n  sk_live_9f3a1b2c3d4e\nSECRET_TOKEN'
check 1 "php heredoc"                "src/r5c.php"         $'\'api_key\' => <<<EOT\nsk_live_9f3a1b2c3d4e\nEOT'
check 1 "cs placeholder + number"    "src/q3.cs"           'var Password = "" + 12345678;'
check 1 "cs placeholder + hex"       "src/q2.cs"           'var Password = "" + 0xDEADBEEFCAFE;'
check 1 "py placeholder + bytes"     "src/q4.py"           "SECRET_KEY = b'' + bytes([1, 2, 3, 4, 5, 6, 7, 8])"
check 1 "json 6-char"               "app/b.json"          '"Password": "abc123"'
check 1 "private key block (txt)"  "notes/k.txt"         "-----BEGIN RSA PRIV""ATE KEY-----"
check 1 "private key in lock file"   "packages.lock.json"  "-----BEGIN PRIV""ATE KEY-----"
check 1 "private key in cs"          "src/k.cs"            "// -----BEGIN PRIV""ATE KEY-----"
check 1 ".env path"                  ".env"                'A=1'

check 1 "r5 cs raw 4 quotes"         "src/t1.cs"           'var Password = """"abc12345xyz"""";'
check 1 "r5 cs raw 6 quotes"         "src/t2.cs"           'var Password = """"""abc12345xyz"""""";'
check 1 "r5 java spaced byte cast"   "src/t3.java"         'secretKey = new byte[] {(byte) 0x8a, (byte) 0x1b, (byte) 0x2c, (byte) 0x3d};'
check 1 "r5 java spaced 3 items"     "src/t4.java"         'secretKey = new byte[] {(byte) 0x8a, (byte) 0x1b, (byte) 0x2c};'
check 1 "r5 cs spaced negative"      "src/t5.cs"           'var secretKey = new byte[] { (byte) 200, (byte) 13, (byte) 44, (byte) 61 };'
check 1 "r5 go byte() calls"         "src/t6.go"           'secretKey := []byte{byte(0x8a), byte(0x1b), byte(0x2c)}'
check 1 "r5 swift UInt8() calls"     "src/t7.swift"        'let secretKey = [UInt8(0x8a), UInt8(0x1b), UInt8(0x2c)]'
check 1 "r5 rust as u8"              "src/t8.rs"           'let secret_key = [0x8a as u8, 0x1b as u8, 0x2c as u8];'
check 1 "r5 cs unchecked"            "src/t9.cs"           'var secretKey = new[] { unchecked((byte)0x8a), unchecked((byte)0x1b) };'
check 1 "r5 java hex 6 digits"       "src/t10.java"        'apiKey = 0xDEADBE;'
check 1 "r5 js regex literal"        "src/t11.js"          'const token = /abc12345xyz/;'
check 1 "r5 py chr concat"           "src/t12.py"          'PASSWORD = chr(104)+chr(117)+chr(110)+chr(116)'
check 1 "r5 ruby symbol value"       "src/t13.rb"          'api_key: :sk_live_abcdef123'
check 1 "r5 short decimal array"     "src/t14.cs"          'var secretKey = new byte[] { 1, 2, 3 };'
check 1 "r5 yarn.lock.bak not a lock" "web/app/yarn.lock.bak" 'password: abcdefgh12345678'
check 1 "not exception: hex number"  "src/t15.cs"          'Token = 0x8a8b8c8d8e8f9091;'
check 1 "not exception: concat"      "src/t16.cs"          'ApiKey = Foo.Bar + "abcdefgh12";'
check 1 "not exception: FromBase64"  "src/t17.cs"          'Secret = Convert.FromBase64String("abcdefgh12");'
check 1 "not exception: call"        "src/t18.py"          'password = some_func(x)'
check 1 "not exception: TimeSpan"    "src/t19.cs"          'var TokenLifetime = TimeSpan.FromMinutes(15);'
check 1 "not exception: new Token"   "src/t20.cs"          'var Token = new Token(a, b, c, d);'
check 1 "not exception: ts non-null" "src/t21.ts"          'const apiKey = process.env.API_KEY!;'
check 1 "not exception: index"       "src/t22.cs"          'Token = settings["abcdefgh"];'
check 1 "not exception: digit start" "src/t23.cs"          'Token = 1abcdefgh;'
check 1 "not exception: nameof+str"  "src/t24.cs"          'Token = nameof(A) + "abcdefgh12";'
check 1 "not exception: default(x)+" "src/t25.cs"          'Token = default(Foo) ?? "abcdefgh12";'
check 1 "yaml identifier right side" "app/t26.yaml"        'token: CancellationToken'
check 1 "ini identifier right side"  "app/t27.ini"         'Token = builder.ConnectionString'
check 1 "json identifier right side" "app/t28.json"        '"token": CancellationToken,'
check 1 "md identifier right side"   "docs/t29.md"         'Token = options.Value.ApiKey'
check 1 "extensionless identifier"   "app/t30"             'Token = options.Value.ApiKey'
check 1 "config identifier"          "app/t31.config"      'ConnectionString = builder.ConnectionString'
check 1 "yml dotted chain"           "app/t32.yml"         'password: some.dotted.chain.value'

check 1 "ef column defaultValue"     "src/m5.cs"           '    password_hash = table.Column<string>(type: "text", nullable: false, defaultValue: "Sup3rS3cretValue!"),'
check 1 "ef column comment"          "src/m6.cs"           '    api_key = table.Column<string>(type: "text", comment: "sk_live_9f3a1b2c3d4e"),'
check 1 "ef column positional"       "src/m7.cs"           '    Password = table.Column<string>("abc12345xyz");'
check 1 "ef column defaultValueSql"  "src/m8.cs"           '    token = table.Column<string>(type: "text", defaultValueSql: "'"'"'abc12345xyz'"'"'"),'
check 1 "ef column open + literal"   "src/m9.cs"           '    password_hash = table.Column<string>(type: "text", defaultValue: "Sup3rS3cretValue!",'
check 1 "ef column ctor open"        "src/m4.cs"           '    password_hash = table.Column<string>('
check 1 "ef column open multi-line"  "src/m12.cs"          $'    password_hash = table.Column<string>(\n        type: "text",\n        defaultValue: "Sup3rS3cretValue!"),'
check 1 "ef column type secret"      "src/m14.cs"          '    password_hash = table.Column<string>(type: "sk_live_9f3a1b2c3d4e", nullable: true),'
check 1 "ef column type mixed case"  "src/m15.cs"          '    password_hash = table.Column<string>(type: "Sup3rS3cret", nullable: true),'
check 1 "ef column open 2-line"     "src/m13.cs"          $'    password_hash = table.Column<string>(type: "text",\n        defaultValue: "Sup3rS3cretValue!"),'

echo "--- must pass (exit 0) ---"
check 0 "exc: dotted chain"          "src/u1.cs"           'ApiKey = options.Value.ApiKey;'
check 0 "exc: ef column ctor"        "src/m3.cs"           '    password_hash = table.Column<string>(type: "text", nullable: true),'
check 0 "exc: ef column timestamp"   "src/m10.cs"          '    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),'
check 0 "exc: ef column maxLength"   "src/m11.cs"          '    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),'
check 0 "exc: ef type tz form"       "src/m16.cs"          '    password_hash = table.Column<string>(type: "timestamp with time zone", nullable: true),'
check 0 "exc: ef type numeric"       "src/m17.cs"          '    api_key = table.Column<decimal>(type: "numeric(12,2)", nullable: true),'
check 0 "exc: nameof chain"       "src/u2.cs"           'Token = nameof(Foo.Bar);'
check 0 "exc: default(Type)"         "src/u3.cs"           'Token = default(CancellationToken);'
check 0 "exc: default(Ns.Type)"      "src/u4.cs"           'Token = default(System.Threading.CancellationToken);'
check 0 "exc: null"                  "src/u5.cs"           'Token = null;'
check 0 "exc: default"               "src/u6.cs"           'Token = default;'
check 0 "exc: trailing )"            "src/u7.cs"           'Foo(ConnectionString = builder.ConnectionString)'
check 0 "exc: trailing ,"            "src/u8.ts"           'token: ctx.cancellationToken,'
check 0 "exc: python attribute"      "src/u9.py"           'API_KEY = settings.API_KEY'
check 0 "exc: razor"                 "src/u10.razor"       'Token = Model.CancellationToken;'
check 0 "exc: cshtml"                "src/u11.cshtml"      'ApiKey = Options.ApiKey;'
check 0 "exc: go chain"              "src/u12.go"          'apiKey = cfg.APIKey'
check 0 "exc: placeholder literal ;" "src/u13.java"        'String password = "changeme";'
check 0 "exc: empty literal"         "src/u14.kt"          'val password = ""'
check 0 "known limit: bare identifier" "src/u15.cs"        'Token = AKIAEXAMPLEKEY123;'
check 0 "lock file keeps skip"       "poetry.lock"         'password = "abcdefgh12345678"'
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
check 0 "cs changeme literal"        "src/p1.cs"           'var Password = "changeme";'
check 0 "cs env placeholder literal" "src/p2.cs"           'var Password = "${DB_PASSWORD}";'
check 0 "cs placeholder CRLF"        "src/p3.cs"           $'var Password = "<YOUR_PASSWORD>";\r'
check 0 "cs placeholder comma"       "src/p5.cs"           'Password = "<YOUR_TOKEN>",'
check 0 "cs empty literal"           "src/p6.cs"           'Password = "";'
check 0 "cs changeme ;"             "src/p4.cs"           'var Password = "changeme";'
check 0 "cs CRLF type name"          "src/p10.cs"          $'CancellationToken ct = default;\r'
check 0 "cs CRLF expression"         "src/p11.cs"          $'options.ConnectionString = builder.ConnectionString;\r'
check 0 "cs nameof ApiKey"           "src/n9.cs"           'var ApiKeyName = nameof(ApiKey);'
check 0 "cs new Version(1,0,0)"      "src/n23.cs"          'var ApiVersion = new Version(1, 0, 0);'
check 0 "cs shift not heredoc"       "src/n16.cs"          'var TokenShift = x << 2;'
check 0 "ts GetSection name"         "src/n19.ts"          'token: CancellationTokenSource;'
check 0 "cs empty literal CRLF"      "src/n20.cs"          $'var Password = "";\r'
check 0 "ts process.env"           "src/E6.ts"           'const token = process.env.TOKEN;'
check 0 "ts short literal"           "src/E7.ts"           'const token = "abc";'
check 0 "lock file UserSecrets"     "src/Api/packages.lock.json" '"Microsoft.Extensions.Configuration.UserSecrets": "10.0.11",'
check 0 "lock file token key"        "package-lock.json"   '"token": "abcdefgh12345678"'
check 0 "yarn.lock"                  "yarn.lock"           'password: abcdefgh12345678'
check 0 "Cargo.lock"                 "Cargo.lock"          'api_key = "abcdefgh12345678"'
check 0 "placeholder env value"      "app/config.env"      'API_KEY=<YOUR_KEY_HERE>'
check 0 "markdown prose"             "docs/a.md"           'Sorun: ek sir (token/link) gerektigi yazilmiyor.'
check 0 ".env.example"               ".env.example"        'API_KEY=sk_live_9f3a1b2c3d4e'

echo "--- large file (pre-filter) ---"
# 3200 harmless lines; the pre-filter must still reach a secret at the very end.
BIG="$(for i in $(seq 1 3200); do printf '    public int Field%s { get; set; }\n' "$i"; done)"
check 1 "large cs, secret at end"    "src/Big1.cs"         "$BIG"$'\n''var Password = "abc12345xyz";'
check 0 "large cs, no secret"        "src/Big2.cs"         "$BIG"

echo "--- $PASS passed, $FAIL failed ---"
[ "$FAIL" -eq 0 ]
