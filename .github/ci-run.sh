#!/usr/bin/env bash
# Runs one CI step whose command lives in .github/ci-commands.env.
#
# Usage: bash .github/ci-run.sh <VAR_NAME> ["<human step label>"]
#
# Behaviour:
#   value set                         -> run it, propagate exit code
#   value == NOT_APPLICABLE           -> skip with an explicit note
#   value empty, SETUP_COMPLETE=false -> warn and skip   (setup period)
#   value empty, SETUP_COMPLETE=true  -> FAIL            (gate does not bend)
#
# The last rule is deliberate: a green pipeline that runs nothing is the
# "misleading green CI" defect this playbook exists to prevent.
#
# The env file is PARSED, not sourced. Sourcing would execute an unquoted
# multi-word value (LINT_CMD=npm run lint) as a command at load time and
# fail with a confusing error pointing at the config file.

set -uo pipefail

VAR_NAME="${1:?usage: ci-run.sh <VAR_NAME> [label]}"
LABEL="${2:-$VAR_NAME}"
ENV_FILE="$(dirname "$0")/ci-commands.env"

if [ ! -f "$ENV_FILE" ]; then
  echo "::error title=CI config missing::$ENV_FILE bulunamadi."
  exit 1
fi

# Reads KEY=VALUE from the env file. Last assignment wins. Strips a trailing
# CR (CRLF checkouts) and one layer of surrounding single/double quotes.
read_key() {
  local key="$1" line val
  line="$(grep -E "^[[:space:]]*${key}=" "$ENV_FILE" | tail -n 1 || true)"
  [ -n "$line" ] || return 0
  val="${line#*=}"
  val="${val%$'\r'}"
  case "$val" in
    \"*\") val="${val#\"}"; val="${val%\"}" ;;
    \'*\') val="${val#\'}"; val="${val%\'}" ;;
  esac
  printf '%s' "$val"
}

SETUP_COMPLETE="$(read_key SETUP_COMPLETE)"
SETUP_COMPLETE="${SETUP_COMPLETE:-false}"
CMD="$(read_key "$VAR_NAME")"

if [ "$CMD" = "NOT_APPLICABLE" ]; then
  echo "::notice title=$LABEL::Bu projede uygulanmiyor (NOT_APPLICABLE)."
  exit 0
fi

if [ -z "$CMD" ]; then
  if [ "$SETUP_COMPLETE" = "true" ]; then
    echo "::error title=$LABEL::SETUP_COMPLETE=true ama $VAR_NAME bos."
    echo "Kurulum tamamlanmis sayiliyor; bos birakilan adim sessizce gecemez."
    echo "Cozum: .github/ci-commands.env icinde $VAR_NAME'i doldurun veya"
    echo "       bu katman projede yoksa NOT_APPLICABLE yazin."
    exit 1
  fi
  echo "::warning title=$LABEL::$VAR_NAME henuz bos — adim atlandi."
  echo "SETUP.md §4'u tamamlayin. SETUP_COMPLETE=true yapildiginda bu adim FAIL edecek."
  exit 0
fi

echo "▶ $LABEL"
echo "  $CMD"
echo "---------------------------------------------------------------"
bash -c "$CMD"
