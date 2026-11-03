#!/bin/sh
# Runs INSIDE the local server: for each running service prints "service reference manifest-digest config-digest".
#
# An image has two identities. The image ID that the outer (classic) Docker shows is the digest of the image's configuration; the
# inner Docker stores images by manifest, so its `.Image` is the manifest digest, which changes when an image is saved and loaded. The
# configuration digest does not change: it is read from the image's own manifest.json (`docker save`). Equal configuration digests
# outside and inside mean the same image (T09 criterion 4; DEPLOY_RUNBOOK K.9).
#
# Usage: host-image-ids.sh "<compose arguments for ps -q>"   (the compose file arguments, as in install-installation.sh)

set -eu

FILES="$1"
cd /opt/shopfolio
for service in app database backup; do
  container="$(docker compose $FILES ps -q "$service")"
  reference="$(docker inspect --format '{{.Config.Image}}' "$container")"
  manifest="$(docker inspect --format '{{.Image}}' "$container")"
  config="$(docker save "$reference" | tar -xO manifest.json | sed -n 's/.*"Config":"blobs\/sha256\/\([0-9a-f]*\)".*/sha256:\1/p')"
  echo "$service $reference $manifest $config"
done
