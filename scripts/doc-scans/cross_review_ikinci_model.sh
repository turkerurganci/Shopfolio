#!/bin/bash
# Cross-review second-model runner (K-1484).
# usage: bash cross_review_ikinci_model.sh <input dir> <output dir> name1 name2 ...
# Input files are <input dir>/<name>.txt. Order: Codex CLI (gpt-5.6-terra, high) first;
# if its usage limit is hit, the cursor-agent fallback runs only after a health call passes.
# A round is never split: partial outputs of a model that hit its limit are moved aside
# (kismi-codex/, kismi-cursor/) and the whole round is re-run with the other model.
# Claude (including API) is never a second model: it is the same family as the first model.

IN="$1"; OUT="$2"; shift 2
if [ -z "$IN" ] || [ -z "$OUT" ] || [ "$#" -eq 0 ]; then
  echo "usage: bash $0 <input dir> <output dir> name1 name2 ..." >&2
  exit 2
fi

CODEX_MODEL="gpt-5.6-terra"
CURSOR_MODEL="${CURSOR_MODEL:-gpt-5.6-sol-high}"
CA="C:/Users/teknodeveloper/AppData/Local/cursor-agent/cursor-agent.cmd"
LIMIT_RE='usage limit|hit your usage limit|rate limit|quota'

if [ "$CURSOR_MODEL" = "auto" ]; then
  echo "ERROR: CURSOR_MODEL=auto is forbidden; pass an explicit model" >&2
  exit 3
fi

mkdir -p "$OUT"
STATUS="$OUT/status.txt"
: > "$STATUS"
RETRY_AT=""

# Prints the "try again at ..." hint found in the given log files, if any.
find_retry() {
  grep -h -i -o 'try again at[^.]*' "$@" 2>/dev/null | head -1
}

# Returns 0 when any part hit a usage limit or produced an empty output.
round_limited() {
  local dir="$1"; shift
  local n
  for n in "$@"; do
    if [ ! -s "$dir/$n.out" ]; then return 0; fi
    if grep -q -i -E "$LIMIT_RE" "$dir/$n.log" 2>/dev/null; then return 0; fi
  done
  return 1
}

run_codex_round() {
  local dir="$OUT"
  local n
  for n in "$@"; do
    local empty="$dir/empty_$n"; mkdir -p "$empty"
    (
      start=$(date +%s)
      timeout 560 codex exec -s read-only -C "$empty" --skip-git-repo-check --ephemeral \
        --ignore-user-config --color never -m "$CODEX_MODEL" \
        -c 'model_reasoning_effort="high"' -o "$dir/$n.out" - < "$IN/$n.txt" > "$dir/$n.log" 2>&1
      rc=$?
      echo "codex $n rc=$rc secs=$(( $(date +%s) - start )) bytes=$(wc -c < "$dir/$n.out" 2>/dev/null || echo 0)" >> "$STATUS"
    ) &
  done
  wait
}

run_cursor_round() {
  local dir="$OUT"
  local n
  for n in "$@"; do
    local ws="$dir/ws_$n"; rm -rf "$ws"; mkdir -p "$ws"
    cp "$IN/$n.txt" "$ws/TALIMAT.md"
    (
      start=$(date +%s)
      timeout 560 "$CA" -p --mode ask --trust --workspace "$ws" --model "$CURSOR_MODEL" \
        "TALIMAT.md'yi oku ve içindeki talimata göre cevap ver" > "$dir/$n.out" 2> "$dir/$n.log"
      rc=$?
      echo "cursor $n rc=$rc secs=$(( $(date +%s) - start )) bytes=$(wc -c < "$dir/$n.out" 2>/dev/null || echo 0)" >> "$STATUS"
    ) &
  done
  wait
}

move_partial() {
  local tag="$1"; shift
  local d="$OUT/$tag"; mkdir -p "$d"
  local n
  for n in "$@"; do
    mv "$OUT/$n.out" "$OUT/$n.log" "$d/" 2>/dev/null
  done
}

cursor_health() {
  local hw="$OUT/saglik_ws"; rm -rf "$hw"; mkdir -p "$hw"
  local probe="BELIRTEC-$RANDOM$RANDOM-$(date +%s)"
  echo "$probe" > "$hw/belirtec.txt"
  timeout 560 "$CA" -p --mode ask --trust --workspace "$hw" --model "$CURSOR_MODEL" \
    "belirtec.txt dosyasını oku ve içindeki belirteci aynen yaz" > "$OUT/saglik.out" 2> "$OUT/saglik.log"
  local rc=$?
  echo "cursor saglik rc=$rc model=$CURSOR_MODEL" >> "$STATUS"
  if grep -q -i -E 'unavailable|usage limit' "$OUT/saglik.out" "$OUT/saglik.log" 2>/dev/null; then return 1; fi
  if ! grep -q -F "$probe" "$OUT/saglik.out" 2>/dev/null; then return 1; fi
  return 0
}

# 1) Codex round
run_codex_round "$@"
if ! round_limited "$OUT" "$@"; then
  echo "model=codex $CODEX_MODEL high" >> "$STATUS"
  echo "used: codex $CODEX_MODEL"
  cat "$STATUS"
  exit 0
fi
RETRY_AT="$(find_retry "$OUT"/*.log)"
move_partial kismi-codex "$@"

# 2) cursor health call, then the whole round again
if cursor_health; then
  run_cursor_round "$@"
  if ! round_limited "$OUT" "$@"; then
    echo "model=cursor $CURSOR_MODEL" >> "$STATUS"
    echo "used: cursor $CURSOR_MODEL"
    cat "$STATUS"
    exit 0
  fi
  RETRY_AT="${RETRY_AT:-$(find_retry "$OUT"/*.log)}"
  move_partial kismi-cursor "$@"
fi

# 3) neither model usable
echo "model=none (codex limited, cursor unavailable)" >> "$STATUS"
if [ -n "$RETRY_AT" ]; then
  echo "retry: $RETRY_AT" >> "$STATUS"
  echo "retry: $RETRY_AT"
fi
echo "ERROR: no second model usable; wait for the first one to reopen" >&2
cat "$STATUS"
exit 1
