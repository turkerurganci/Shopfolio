-- Deliberate violation sample of the setup SQL scan (09 19.2 item 3): a privilege given to PUBLIC.
-- This file is not part of the deployment; the scan test reads it as text. The violation is on line 4.
REVOKE ALL ON SCHEMA public FROM PUBLIC;
GRANT USAGE ON SCHEMA public TO PUBLIC;
