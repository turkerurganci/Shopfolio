-- First-install step 1 of 4 (09 10.8 items 1 and 2): the four database users. Run by the founding (super) user, connected to the
-- maintenance database. The statements are psql statements with variables; the integration tests run the same file (09 19.1 item 4).
--
-- Variables:  migrator, app, reader, backup_status   the user names (identifiers)
--             migrator_password, app_password, reader_password   the passwords (literals; the setup script reads them from the
--                                                               db_setup_* secret files, 05 6.x.6)
--
-- A password never lands in a log (05 6.x.16): a statement that fails is written to the server log together with its text, so
-- the three statements below that carry a password run with statement logging off for this session only.
SET log_statement = 'none';
SET log_min_duration_statement = -1;
SET log_min_error_statement = panic;

-- None of the four carries SUPERUSER, CREATEDB, CREATEROLE, REPLICATION or BYPASSRLS (09 10.8 item 1).
CREATE ROLE :"migrator" LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS PASSWORD :'migrator_password';
CREATE ROLE :"app" LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS PASSWORD :'app_password';
CREATE ROLE :"reader" LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS PASSWORD :'reader_password';
-- No password: the backup status user connects only through the local socket (pg_hba.conf, 09 10.8 item 1).
CREATE ROLE :"backup_status" LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS;

RESET log_min_error_statement;
RESET log_min_duration_statement;
RESET log_statement;

-- The reader's session is read-only. This is not a security boundary (the reader can switch it off with SET); the boundary is
-- the missing privilege (09 10.8 item 2).
ALTER ROLE :"reader" SET default_transaction_read_only = on;
