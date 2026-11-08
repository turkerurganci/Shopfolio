# Backup tool image (05 8.2.2, 09 17): a small base image, the restic binary pinned by SHA-256, and the scheduler of the nightly
# file backup. Build context is the repository root (09 4.3 item 1):  docker build -f deploy/images/backup/Dockerfile .
# Base image is pinned by tag and digest (05 8.2.3, MK-10); the monthly maintenance release renews both (05 8.8.2).

# ---- fetch: download restic and refuse any binary whose SHA-256 differs from the one pinned here ----
# The download is retried (5 attempts, 10 s apart) so a short GitHub outage does not fail the build; busybox wget has no retry
# options, hence the shell loop. The SHA-256 check below still gates every attempt's result.
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
    downloaded=no; \
    for attempt in 1 2 3 4 5; do \
      if wget -q -O /tmp/restic.bz2 "https://github.com/restic/restic/releases/download/v${RESTIC_VERSION}/restic_${RESTIC_VERSION}_linux_${TARGETARCH:-amd64}.bz2"; then downloaded=yes; break; fi; \
      echo "restic download failed (attempt ${attempt} of 5)" >&2; \
      if [ "${attempt}" -lt 5 ]; then sleep 10; fi; \
    done; \
    [ "${downloaded}" = yes ] || { echo "restic download failed after 5 attempts" >&2; exit 1; }; \
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
# The nightly job and the status writer it sources (09 17 items 4 and 5). Compose gives the scheduler the job as its command.
COPY --chmod=0555 deploy/images/backup/file-backup-job.sh /usr/local/bin/file-backup-job.sh
COPY --chmod=0444 deploy/images/shared/write-status.sh /usr/local/bin/write-status.sh
# The recovery drill (DEPLOY_RUNBOOK section J.5): run by hand in a one-off container, never by the scheduler.
COPY --chmod=0555 deploy/images/backup/file-restore-drill.sh /usr/local/bin/file-restore-drill.sh

# Mount points of the Compose volumes (deploy/compose.yaml): the tool's own log directory and the status file directory. The
# status directory is shared with the application (another user reads it), so it is open to both and holds only the status file.
RUN mkdir -p /var/log/backup /var/lib/backup/status \
    && chown backup:backup /var/log/backup \
    && chmod 1777 /var/lib/backup/status

# No secret enters the image: the repository password and the object storage key arrive as mounted files at run time
# (09 17 item 2).
USER backup
ENTRYPOINT ["/usr/local/bin/scheduler.sh"]
