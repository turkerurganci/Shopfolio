-- First-install step 1 of 4 (09 10.8 items 1 and 2): the four database users. Run by the founding (super) user, connected to the
-- maintenance database. The statements are psql statements with variables; the integration tests run the same file (09 19.1 item 4).
--
-- Variables:  migrator, app, reader, backup_status   the user names (identifiers)
--
-- No password is in this file and none passes through it (05 6.x.16, 09 10.8 item 1): the users are made without one. The setup
-- script then gives the three network users their passwords with psql's own \password, which turns the password into a SCRAM-SHA-256
-- verifier in the client, so the server never receives or logs the clear text. (The tests set theirs with ALTER ROLE.)
--
-- None of the four carries SUPERUSER, CREATEDB, CREATEROLE, REPLICATION or BYPASSRLS (09 10.8 item 1).
CREATE ROLE :"migrator" LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS;
CREATE ROLE :"app" LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS;
CREATE ROLE :"reader" LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS;
-- No password: the backup status user connects only through the local socket (pg_hba.conf, 09 10.8 item 1).
CREATE ROLE :"backup_status" LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS;

-- The reader's session is read-only. This is not a security boundary (the reader can switch it off with SET); the boundary is
-- the missing privilege (09 10.8 item 2).
ALTER ROLE :"reader" SET default_transaction_read_only = on;
