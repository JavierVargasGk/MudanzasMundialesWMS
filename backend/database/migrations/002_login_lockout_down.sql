-- Rollback for 002_login_lockout.sql. Run manually:
--   psql ... -f backend/database/rollback/002_login_lockout_down.sql
-- Not placed under migrations/ on purpose: Postgres's docker-entrypoint-initdb.d
-- runs every .sql file it finds there, and a down-script must never run
-- automatically alongside the up-scripts.

DROP TRIGGER IF EXISTS trg_guard_login_stage_update ON usuarios;
DROP FUNCTION IF EXISTS fn_guard_login_stage_update();
DROP POLICY IF EXISTS usuarios_update_login ON usuarios;

ALTER TABLE usuarios
    DROP COLUMN IF EXISTS intentos_fallidos,
    DROP COLUMN IF EXISTS bloqueado_hasta;