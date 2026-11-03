-- First-install step 4 of 4 (09 10.8 item 2; 05 7.1.3): the trigram extension, the ownership of the schema and the default privileges
-- every later table gets. Run by the founding user, connected to the application database; it lives in that database's catalog, so a
-- copy of the database made from a template carries it.
--
-- Variables:  migrator, app, reader, backup_status   the user names (identifiers)
--
-- The trigram extension is created by the founding user: the schema owner gets no new privilege for it (05 7.1.3; K-1124).
CREATE EXTENSION IF NOT EXISTS pg_trgm;

ALTER SCHEMA public OWNER TO :"migrator";
REVOKE ALL ON SCHEMA public FROM PUBLIC;
GRANT USAGE ON SCHEMA public TO :"app", :"reader", :"backup_status";

-- Applied to every table the schema owner creates later, so no migration writes a GRANT for a new table. The reader gets SELECT only.
-- The backup status user gets nothing by default (its column privilege is written by the migration of the one table, 09 10.8 item 3).
ALTER DEFAULT PRIVILEGES FOR ROLE :"migrator" IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO :"app";
ALTER DEFAULT PRIVILEGES FOR ROLE :"migrator" IN SCHEMA public GRANT SELECT ON TABLES TO :"reader";

-- The status command lists the pending log files with it (05 9.4.10); the whole monitoring role (pg_monitor) is not granted.
GRANT EXECUTE ON FUNCTION pg_catalog.pg_ls_archive_statusdir() TO :"app";
