-- First-install step 3 of 4 (09 10.8 items 2 and 5): what the database itself grants, and the three role settings that hand the user
-- names to the migrations. Run by the founding user, connected to the maintenance database. The integration tests run it again for
-- every database they copy from the template: a database copy takes neither the database privileges nor the role settings along.
--
-- Variables:  db, migrator, app, reader, backup_status   as in the other files
--
-- PUBLIC loses every privilege on the database; each user gets its connect privilege separately.
REVOKE ALL ON DATABASE :"db" FROM PUBLIC;
GRANT CONNECT ON DATABASE :"db" TO :"migrator", :"app", :"reader", :"backup_status";

-- The names of the three users the migrations grant to are read by the migrations from these settings of the schema owner, never
-- written in a migration (09 10.8 items 3 and 5; 10.6 item 9). A missing setting makes current_setting fail and the migration roll back.
ALTER ROLE :"migrator" IN DATABASE :"db" SET shopfolio.application_user = :'app';
ALTER ROLE :"migrator" IN DATABASE :"db" SET shopfolio.reader_user = :'reader';
ALTER ROLE :"migrator" IN DATABASE :"db" SET shopfolio.backup_status_user = :'backup_status';
