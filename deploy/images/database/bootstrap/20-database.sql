-- First-install step 2 of 4 (05 7.1.3, 06 2.8.3; 09 10.8 item 2): the application database, UTF-8 with ICU's deterministic Turkish
-- collation, owned by the schema owner. Run by the founding user, connected to the maintenance database.
--
-- Variables:  db         the database name (identifier)
--             migrator   the schema owner's name (identifier)
--
-- The official image has already created an empty database of this name with the cluster's own locale (POSTGRES_DB); it is
-- replaced here, because the collation of a database cannot be changed afterwards. The setup script refuses to run when the name
-- is the maintenance database.
DROP DATABASE IF EXISTS :"db" WITH (FORCE);
CREATE DATABASE :"db" TEMPLATE template0 OWNER :"migrator" ENCODING 'UTF8' LOCALE_PROVIDER icu ICU_LOCALE 'tr-TR';
