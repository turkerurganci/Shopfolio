-- Deliberate violation sample of the setup SQL scan (09 19.2 item 3): a role that does not say NOSUPERUSER and the other four.
-- This file is not part of the deployment; the scan test reads it as text. The violation is on line 4.
CREATE ROLE :"app" LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS;
CREATE ROLE :"reader" LOGIN NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS;
