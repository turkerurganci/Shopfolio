# Backup tool image (05 8.2.2, 09 17): a small base image, the restic binary pinned by image digest, and the scheduler of the
# nightly file backup. Build context is the repository root (09 4.3 item 1):  docker build -f deploy/images/backup/Dockerfile .
# Base image is pinned by tag and digest (05 8.2.3, MK-10); the monthly maintenance release renews both (05 8.8.2).

# ---- restic: the binary comes from the official restic image, pinned by tag and index digest ----
# No download from GitHub release assets at build time: a GitHub outage (HTTP 504 on release downloads) failed every build.
# The registry verifies the pinned digest, so the binary is as fixed as the former SHA-256 check made it.
# restic 0.19.1 (released 2026-07-05, BSD-2-Clause). A new version changes the tag and the digest together (09 3.3 item 7);
# Dependabot does not watch this file.
FROM restic/restic:0.19.1@sha256:136600b6ff6843d61d355f7f71f460a166429f35de6fd11b568fece3c9a4d510 AS restic

# ---- final ----
FROM alpine:3.24.3@sha256:be2656061b536c1883c4034790e21d0a09c5aaaf51968b89368a3c0371ea2c9a
# ca-certificates: encrypted connection to the object storage. tzdata: BACKUP_TIME_ZONE for the scheduler.
RUN apk add --no-cache ca-certificates tzdata \
    && addgroup -g 10001 -S backup \
    && adduser -u 10001 -S -G backup -h /home/backup backup
COPY --from=restic --chown=0:0 --chmod=0555 /usr/bin/restic /usr/local/bin/restic
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
