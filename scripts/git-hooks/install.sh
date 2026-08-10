#!/usr/bin/env bash
# Project Playbook git hook installer.
#
# Points git's core.hooksPath at this folder instead of copying files into
# .git/hooks. Benefits: hooks stay version-controlled, edits take effect
# immediately, and there is no stale-copy drift.
#
# Usage (once per clone):
#   bash scripts/git-hooks/install.sh

set -e

if ! git rev-parse --git-dir >/dev/null 2>&1; then
  echo "ERROR: not inside a git repository" >&2
  exit 1
fi

REPO_ROOT="$(git rev-parse --show-toplevel)"
HOOKS_PATH="scripts/git-hooks"

git config core.hooksPath "$HOOKS_PATH"
echo "Set core.hooksPath = $HOOKS_PATH"

for hook in "$REPO_ROOT/$HOOKS_PATH"/*; do
  hook_name="$(basename "$hook")"
  case "$hook_name" in
    install.sh|README*|*.md|*.config) continue ;;
  esac
  if [ -f "$hook" ]; then
    chmod +x "$hook"
    echo "Ensured executable: $hook_name"
  fi
done

cat <<'EOF'

Hooks installed via core.hooksPath.

Verify:
  git config core.hooksPath          # should print: scripts/git-hooks

Test:
  git push origin main                                   # blocked (Layer 1)
  PB_ALLOW_DIRECT_PUSH=1 git push origin main            # bypass + logged

Disable (revert to default .git/hooks/):
  git config --unset core.hooksPath

Optional overrides: create scripts/git-hooks/hooks.config, e.g.
  PB_PROTECTED_BRANCHES="main develop"
  PB_TASK_BRANCH_PREFIX="task/"
EOF
