# Database image (05 8.2.2, 09 17): the official PostgreSQL 18 image plus pgBackRest and the scheduler of the nightly full backup.
# pgBackRest's archive command must run inside PostgreSQL's own container and the tool has no official container image
# (checked 2026-11-03, see Docs/TASK_REPORTS/T06_REPORT.md), so this image is built in the project.
# Build context is the repository root (09 4.3 item 1):  docker build -f deploy/images/database/Dockerfile .
# Base image is pinned by tag and digest (05 8.2.3, MK-10); the monthly maintenance release renews both (05 8.8.2).

FROM postgres:18.6-trixie@sha256:3a4264520e121176619f54ff867d12774bc81e95e7b84db58d63bccdb6144b81

# pgBackRest comes from the PostgreSQL project's own apt repository, which the official image already trusts and configures.
# The version is not pinned: apt-get update reads the repository index at build time, and the PGDG repository keeps only the
# current pgBackRest release, so a pinned version would break the build later. The installed version is printed into the build
# log; the base image digest pins the base only, so two builds of this file on different days can carry different pgBackRest
# releases. This is the one exception to "the tool version is pinned" (09 17 item 6); the version is reviewed in the monthly
# maintenance release (05 8.8.2).
RUN apt-get update \
    && apt-get install --no-install-recommends -y pgbackrest \
    && rm -rf /var/lib/apt/lists/* \
    && pgbackrest version

# The scheduler is the same file in this image and in the backup image (09 17 item 3). It starts the nightly job at
# BACKUP_RUN_TIME; the job and the one-time stanza creation are the backup task's scripts.
COPY --chmod=0555 deploy/images/shared/scheduler.sh /usr/local/bin/scheduler.sh
COPY --chmod=0555 deploy/images/database/database-job.sh /usr/local/bin/database-job.sh
COPY --chmod=0555 deploy/images/database/stanza-bootstrap.sh /usr/local/bin/stanza-bootstrap.sh
# The recovery drill (DEPLOY_RUNBOOK section J.5): run by hand in a one-off container, never by the scheduler.
COPY --chmod=0555 deploy/images/database/database-restore-drill.sh /usr/local/bin/database-restore-drill.sh
COPY --chmod=0555 deploy/images/database/database-entrypoint.sh /usr/local/bin/database-entrypoint.sh

# Configuration is a main file that includes a directory: later tasks add their settings (archive, backup) as files in the
# directory without touching the main file. track_commit_timestamp makes the database record when each transaction committed,
# which the restore guard counts rows by (05 8.5.11). The archive settings (05 7.6.1) are the second file of the directory.
COPY deploy/images/database/postgresql.conf /etc/postgresql/postgresql.conf
COPY deploy/images/database/postgresql.conf.d/ /etc/postgresql/postgresql.conf.d/

# pgBackRest's configuration (two repositories, retention by time; no secret in it).
COPY deploy/images/database/pgbackrest.conf /etc/pgbackrest/pgbackrest.conf

# pgBackRest's own directories are writable by the database user: the image runs as that user, not as root.
RUN mkdir -p /etc/pgbackrest /var/spool/pgbackrest /var/lib/pgbackrest \
    && chown -R postgres:postgres /etc/pgbackrest /var/spool/pgbackrest /var/lib/pgbackrest

# The local copy of the last full backup lives in the database's own volume, next to the data directory (05 7.6.1). The directory
# is created here so that the volume takes it (and its owner) from the image when it is first mounted.
RUN mkdir -p /var/lib/postgresql/local-backup && chown postgres:postgres /var/lib/postgresql/local-backup

# Mount point of the database log volume (deploy/compose.yaml); the backup tool reads it read-only (05 9.1.4). pgBackRest and the
# backup scripts write their logs here, in their own files, not to the container output (05 9.1.4; D-32).
RUN mkdir -p /var/log/postgresql/pgbackrest && chown -R postgres:postgres /var/log/postgresql

# No secret enters the image: passwords and backup keys arrive as mounted files at run time (09 17 item 2). The entrypoint
# wraps the official one: it turns the two backup key files into pgBackRest's environment, starts the scheduler and runs the
# official entrypoint unchanged (the server still runs as the database user).
USER postgres
ENTRYPOINT ["/usr/local/bin/database-entrypoint.sh"]
CMD ["postgres", "-c", "config_file=/etc/postgresql/postgresql.conf"]
