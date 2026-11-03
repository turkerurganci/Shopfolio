-- Deliberate violation sample of the setup SQL scan (09 19.2 item 3): a password written into the file.
-- This file is not part of the deployment; the scan test reads it as text. The violation is on line 4.
CREATE ROLE :"app" LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS PASSWORD :'app_password';
CREATE ROLE :"reader" LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS PASSWORD 'made-up';
