# Database image (05 8.2.2, 09 17): the official PostgreSQL 18 image plus pgBackRest and the scheduler of the nightly full backup.
# pgBackRest's archive command must run inside PostgreSQL's own container and the tool has no official container image
# (checked 2026-11-03, see Docs/TASK_REPORTS/T06_REPORT.md), so this image is built in the project.
# Build context is the repository root (09 4.3 item 1):  docker build -f deploy/images/database/Dockerfile .
# Base image is pinned by tag and digest (05 8.2.3, MK-10); the monthly maintenance release renews both (05 8.8.2).

FROM postgres:18.6-trixie@sha256:3a4264520e121176619f54ff867d12774bc81e95e7b84db58d63bccdb6144b81

# pgBackRest comes from the PostgreSQL project's own apt repository, which the official image already trusts and configures.
# The version is not pinned: apt-get update reads the repository index at build time, and the PGDG repository keeps only the
# current pgBackRest release, so a pinned version would break the build later. The installed version is printed into the build
# log; the base image digest pins the base only, so two builds of this file on different days can carry different pgBackRest releases.
RUN apt-get update \
    && apt-get install --no-install-recommends -y pgbackrest \
    && rm -rf /var/lib/apt/lists/* \
    && pgbackrest version

# The scheduler is the same file in this image and in the backup image (09 17 item 3). The job it runs arrives with the backup
# task; this image only carries the scheduler.
COPY --chmod=0555 deploy/images/shared/scheduler.sh /usr/local/bin/scheduler.sh

# Configuration is a main file that includes a directory: later tasks add their settings (archive, backup) as files in the
# directory without touching the main file. track_commit_timestamp makes the database record when each transaction committed,
# which the restore guard counts rows by (05 8.5.11).
COPY deploy/images/database/postgresql.conf /etc/postgresql/postgresql.conf
COPY deploy/images/database/postgresql.conf.d/ /etc/postgresql/postgresql.conf.d/

# pgBackRest's own directories are writable by the database user: the image runs as that user, not as root.
RUN mkdir -p /etc/pgbackrest /var/log/pgbackrest /var/spool/pgbackrest /var/lib/pgbackrest \
    && chown -R postgres:postgres /etc/pgbackrest /var/log/pgbackrest /var/spool/pgbackrest /var/lib/pgbackrest

# Mount point of the database log volume (deploy/compose.yaml); the backup tool reads it read-only (05 9.1.4). Where the server
# and pgBackRest write their logs is set by the task that configures them.
RUN mkdir -p /var/log/postgresql && chown postgres:postgres /var/log/postgresql

# No secret enters the image: passwords and backup keys arrive as mounted files at run time (09 17 item 2).
USER postgres
CMD ["postgres", "-c", "config_file=/etc/postgresql/postgresql.conf"]
