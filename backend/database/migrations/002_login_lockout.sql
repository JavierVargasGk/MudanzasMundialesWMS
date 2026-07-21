

ALTER TABLE usuarios
    ADD COLUMN IF NOT EXISTS intentos_fallidos INTEGER NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS bloqueado_hasta TIMESTAMPTZ;

COMMENT ON COLUMN usuarios.intentos_fallidos IS
    'Consecutive failed login attempts since the last success or unlock. Reset to 0 on successful login.';
COMMENT ON COLUMN usuarios.bloqueado_hasta IS
    'If set and in the future, login is refused regardless of password correctness (temporary lockout).';

CREATE POLICY usuarios_update_login ON usuarios FOR UPDATE
    USING (current_setting('app.auth_stage', true) = 'login')
    WITH CHECK (current_setting('app.auth_stage', true) = 'login');

CREATE OR REPLACE FUNCTION fn_guard_login_stage_update() RETURNS TRIGGER AS $$
BEGIN
    IF current_setting('app.auth_stage', true) = 'login' THEN
        IF NEW.contrasena_hash IS DISTINCT FROM OLD.contrasena_hash
           OR NEW.rol IS DISTINCT FROM OLD.rol
           OR NEW.correo_institucional IS DISTINCT FROM OLD.correo_institucional
           OR NEW.nombre_completo IS DISTINCT FROM OLD.nombre_completo
           OR NEW.activo IS DISTINCT FROM OLD.activo
        THEN
            RAISE EXCEPTION
                'Login-stage updates may only change intentos_fallidos, bloqueado_hasta, or ultimo_acceso';
        END IF;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_guard_login_stage_update
    BEFORE UPDATE ON usuarios
    FOR EACH ROW EXECUTE FUNCTION fn_guard_login_stage_update();