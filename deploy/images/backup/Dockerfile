# Backup tool image (05 8.2.2, 09 17): a small base image, the restic binary pinned by SHA-256, and the scheduler of the nightly
# file backup. Build context is the repository root (09 4.3 item 1):  docker build -f deploy/images/backup/Dockerfile .
# Base image is pinned by tag and digest (05 8.2.3, MK-10); the monthly maintenance release renews both (05 8.8.2).

# ---- fetch: download restic and refuse any binary whose SHA-256 differs from the one pinned here ----
FROM alpine:3.24.3@sha256:be2656061b536c1883c4034790e21d0a09c5aaaf51968b89368a3c0371ea2c9a AS fetch
# restic 0.19.1 (released 2026-07-05, BSD-2-Clause). The digests are those of the release's SHA256SUMS file for the .bz2 assets.
# A new version changes all three lines together (09 3.3 item 7); Dependabot does not watch this file.
ARG RESTIC_VERSION=0.19.1
ARG RESTIC_SHA256_AMD64=f415415624dcc452f2a02b8c33641791a8c6d6d3b65bbb3543fcf9a25151585c
ARG RESTIC_SHA256_ARM64=a5f64aaab53d51e311fa3829124c5b703f2d14cf187d8640b6be3b2b49376465
ARG TARGETARCH
RUN set -eu; \
    case "${TARGETARCH:-amd64}" in \
      amd64) expected="${RESTIC_SHA256_AMD64}" ;; \
      arm64) expected="${RESTIC_SHA256_ARM64}" ;; \
      *) echo "unsupported architecture: ${TARGETARCH}" >&2; exit 1 ;; \
    esac; \
    wget -q -O /tmp/restic.bz2 "https://github.com/restic/restic/releases/download/v${RESTIC_VERSION}/restic_${RESTIC_VERSION}_linux_${TARGETARCH:-amd64}.bz2"; \
    echo "${expected}  /tmp/restic.bz2" | sha256sum -c -; \
    bunzip2 /tmp/restic.bz2; \
    chmod 0555 /tmp/restic

# ---- final ----
FROM alpine:3.24.3@sha256:be2656061b536c1883c4034790e21d0a09c5aaaf51968b89368a3c0371ea2c9a
# ca-certificates: encrypted connection to the object storage. tzdata: BACKUP_TIME_ZONE for the scheduler.
RUN apk add --no-cache ca-certificates tzdata \
    && addgroup -g 10001 -S backup \
    && adduser -u 10001 -S -G backup -h /home/backup backup
COPY --from=fetch /tmp/restic /usr/local/bin/restic
COPY --chmod=0555 deploy/images/shared/scheduler.sh /usr/local/bin/scheduler.sh

# No secret enters the image: the repository password and the object storage key arrive as mounted files at run time
# (09 17 item 2). The file-backup job and the status writer arrive with the backup task.
USER backup
ENTRYPOINT ["/usr/local/bin/scheduler.sh"]
