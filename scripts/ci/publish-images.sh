#!/usr/bin/env bash
# publish.yml "Publish artifacts": builds the three images and pushes them to GitHub Container Registry
# (05 8.2.5, 8.3.12; MK-11).
#
#   push to main  -> tag = the full commit id
#   version tag   -> tags = the full commit id and the version number (tag vMAJOR.MINOR.PATCH -> MAJOR.MINOR.PATCH)
#
# Image names: ghcr.io/<owner>/shopfolio-app, shopfolio-database, shopfolio-backup. The registry login uses the workflow's
# workflow token, passed as REGISTRY_CREDENTIAL (permission `packages: write`); no other credential exists. Published version images are never deleted
# (05 8.2.5). There is no deployment: installations receive a release with their update round (05 8.3.13).
#
# Needs: docker; REGISTRY_CREDENTIAL, GITHUB_ACTOR, GITHUB_REPOSITORY, GITHUB_REPOSITORY_OWNER, GITHUB_SHA, GITHUB_REF_TYPE and
# GITHUB_REF_NAME (all set by Actions). PUBLISH_DRY_RUN=1 builds and tags but neither logs in nor pushes.

set -euo pipefail

cd "$(dirname "$0")/../.."

REGISTRY="ghcr.io"
: "${GITHUB_REPOSITORY:?GITHUB_REPOSITORY is required}"
: "${GITHUB_REPOSITORY_OWNER:?GITHUB_REPOSITORY_OWNER is required}"
: "${GITHUB_SHA:?GITHUB_SHA is required}"
DRY_RUN="${PUBLISH_DRY_RUN:-0}"
OWNER="${GITHUB_REPOSITORY_OWNER,,}"

TAGS=("$GITHUB_SHA")
if [ "${GITHUB_REF_TYPE:-}" = "tag" ]; then
  case "${GITHUB_REF_NAME:-}" in
    v[0-9]*.[0-9]*.[0-9]*) TAGS+=("${GITHUB_REF_NAME#v}") ;;
    *) echo "::error title=Publish::The tag '${GITHUB_REF_NAME:-}' is not vMAJOR.MINOR.PATCH."; exit 1 ;;
  esac
fi

if [ "$DRY_RUN" != "1" ]; then
  : "${REGISTRY_CREDENTIAL:?REGISTRY_CREDENTIAL (the workflow token) is required to push (workflow permission packages: write)}"
  : "${GITHUB_ACTOR:?GITHUB_ACTOR is required}"
  printf '%s' "$REGISTRY_CREDENTIAL" | docker login "$REGISTRY" -u "$GITHUB_ACTOR" --password-stdin
fi

for name in app database backup; do
  repository="$REGISTRY/$OWNER/shopfolio-$name"
  tag_args=()
  for tag in "${TAGS[@]}"; do
    tag_args+=(-t "$repository:$tag")
  done
  echo "== $repository (${TAGS[*]})"
  # source: links the package to this repository, so the package inherits the repository's access settings.
  docker build -f "deploy/images/$name/Dockerfile" \
    --label "org.opencontainers.image.source=https://github.com/$GITHUB_REPOSITORY" \
    --label "org.opencontainers.image.revision=$GITHUB_SHA" \
    "${tag_args[@]}" .
  if [ "$DRY_RUN" = "1" ]; then
    echo "dry run: not pushed"
    continue
  fi
  for tag in "${TAGS[@]}"; do
    docker push "$repository:$tag"
  done
  digest="$(docker image inspect --format '{{index .RepoDigests 0}}' "$repository:${TAGS[0]}")"
  echo "published: $digest"
  if [ -n "${GITHUB_STEP_SUMMARY:-}" ]; then
    echo "- \`$digest\` (${TAGS[*]})" >>"$GITHUB_STEP_SUMMARY"
  fi
done
