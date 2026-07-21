-- =============================================================
-- Mudanzas WMS — 001_init.sql — Initial schema
-- PostgreSQL 15. Row-Level Security is central to this schema:
-- every cliente-scoped table enforces isolation via session
-- variables set per-request by the API (see UnitOfWork.cs).
-- =============================================================

-- =============================================================
-- GUARDRAILS
-- =============================================================
DROP TABLE IF EXISTS
    reportes_discrepancia, auditoria, ajustes_inventario, ordenes_despacho,
    entradas, stock_actual, refresh_tokens, skus, ubicaciones, usuarios, clientes
    CASCADE;

DROP TYPE IF EXISTS
    rol_usuario, estado_despacho, tipo_ajuste, tipo_discrepancia, estado_discrepancia, estado_lote
    CASCADE;

-- =============================================================
-- ENUMS
-- =============================================================
CREATE TYPE rol_usuario AS ENUM ('Administrador', 'Analista', 'Operario');
CREATE TYPE estado_despacho AS ENUM ('pendiente', 'confirmado');
CREATE TYPE tipo_ajuste AS ENUM ('merma', 'dano', 'conteo');
CREATE TYPE tipo_discrepancia AS ENUM ('faltante', 'sobrante', 'dano', 'otro');
CREATE TYPE estado_discrepancia AS ENUM ('abierto', 'resuelto', 'descartado');
CREATE TYPE estado_lote AS ENUM ('Vencido', 'Disponible', 'Descartado');

-- =============================================================
-- TABLES
-- =============================================================

CREATE TABLE usuarios (
    id_usuario              SERIAL          PRIMARY KEY,
    nombre_completo         VARCHAR(150)    NOT NULL,
    correo_institucional    VARCHAR(150)    NOT NULL UNIQUE,
    contrasena_hash         VARCHAR(255)    NOT NULL,
    rol                     rol_usuario     NOT NULL,
    activo                  BOOLEAN         NOT NULL DEFAULT TRUE,
    fecha_creacion          TIMESTAMPTZ     NOT NULL DEFAULT now(),
    ultimo_acceso           TIMESTAMPTZ
);

CREATE TABLE refresh_tokens (
    id_token                UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    id_usuario              INTEGER         NOT NULL REFERENCES usuarios(id_usuario),
    token_hash              VARCHAR(255)    NOT NULL,
    fecha_expiracion        TIMESTAMPTZ     NOT NULL,
    revocado                BOOLEAN         NOT NULL DEFAULT FALSE,
    fecha_creacion          TIMESTAMPTZ     NOT NULL DEFAULT now()
);

CREATE TABLE clientes (
    id_cliente              SERIAL          PRIMARY KEY,
    nombre_empresa          VARCHAR(150)    NOT NULL,
    identificacion_juridica VARCHAR(50),
    contacto_nombre         VARCHAR(150),
    contacto_telefono       VARCHAR(20),
    contacto_correo         VARCHAR(150),
    activo                  BOOLEAN         NOT NULL DEFAULT TRUE,
    fecha_registro          TIMESTAMPTZ     NOT NULL DEFAULT now()
);

CREATE TABLE ubicaciones (
    id_ubicacion            SERIAL          PRIMARY KEY,
    codigo                  VARCHAR(20)     NOT NULL UNIQUE,
    pasillo                 VARCHAR(10),
    estante                 VARCHAR(10),
    capacidad_maxima        INTEGER,
    activo                  BOOLEAN         NOT NULL DEFAULT TRUE
);

CREATE TABLE skus (
    id_sku                      SERIAL          PRIMARY KEY,
    id_cliente                  INTEGER         NOT NULL REFERENCES clientes(id_cliente),
    codigo_sku                  VARCHAR(50)     NOT NULL,
    nombre                      VARCHAR(150)    NOT NULL,
    descripcion                 TEXT,
    unidad_medida                VARCHAR(20)     NOT NULL,
    fecha_limite_almacenamiento DATE,
    activo                       BOOLEAN         NOT NULL DEFAULT TRUE,
    fecha_creacion               TIMESTAMPTZ     NOT NULL DEFAULT now(),
    UNIQUE (id_cliente, codigo_sku)
);

CREATE TABLE stock_actual (
    id_stock                INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_sku                  INTEGER         NOT NULL REFERENCES skus(id_sku),
    id_cliente               INTEGER         NOT NULL REFERENCES clientes(id_cliente),
    id_ubicacion             INTEGER         NOT NULL REFERENCES ubicaciones(id_ubicacion),
    cantidad_actual          INTEGER         NOT NULL DEFAULT 0 CHECK (cantidad_actual >= 0),
    status                  estado_lote     NOT NULL DEFAULT 'Disponible',
    numero_lote             VARCHAR(50),
    fecha_actualizacion      TIMESTAMPTZ     NOT NULL DEFAULT now(),
    UNIQUE (id_sku, id_ubicacion)
);

CREATE TABLE entradas (
    id_entrada               SERIAL          PRIMARY KEY,
    id_sku                   INTEGER         NOT NULL REFERENCES skus(id_sku),
    id_cliente                INTEGER         NOT NULL REFERENCES clientes(id_cliente),
    id_ubicacion              INTEGER         NOT NULL REFERENCES ubicaciones(id_ubicacion),
    id_usuario                 INTEGER         NOT NULL REFERENCES usuarios(id_usuario),
    cantidad                  INTEGER         NOT NULL CHECK (cantidad > 0),
    fecha_ingreso              DATE            NOT NULL,
    fecha_registro             TIMESTAMPTZ     NOT NULL DEFAULT now()
);

CREATE TABLE ordenes_despacho (
    id_despacho                 SERIAL              PRIMARY KEY,
    id_sku                      INTEGER             NOT NULL REFERENCES skus(id_sku),
    id_cliente                   INTEGER             NOT NULL REFERENCES clientes(id_cliente),
    cantidad_solicitada           INTEGER             NOT NULL CHECK (cantidad_solicitada > 0),
    fecha_salida                  DATE                NOT NULL,
    estado                        estado_despacho     NOT NULL DEFAULT 'pendiente',
    id_usuario_creacion            INTEGER             NOT NULL REFERENCES usuarios(id_usuario),
    id_usuario_confirmacion        INTEGER             REFERENCES usuarios(id_usuario),
    fecha_creacion                 TIMESTAMPTZ         NOT NULL DEFAULT now(),
    fecha_confirmacion             TIMESTAMPTZ
);

CREATE TABLE ajustes_inventario (
    id_ajuste                    SERIAL          PRIMARY KEY,
    id_sku                       INTEGER         NOT NULL REFERENCES skus(id_sku),
    id_cliente                    INTEGER         NOT NULL REFERENCES clientes(id_cliente),
    id_ubicacion                   INTEGER         NOT NULL REFERENCES ubicaciones(id_ubicacion),
    id_usuario_solicitante          INTEGER         NOT NULL REFERENCES usuarios(id_usuario),
    tipo_ajuste                    tipo_ajuste     NOT NULL,
    cantidad_ajuste                 INTEGER         NOT NULL,
    razon                           TEXT            NOT NULL,
    fecha_registro                  TIMESTAMPTZ     NOT NULL DEFAULT now()
);

CREATE TABLE reportes_discrepancia (
    id_reporte                    SERIAL                  PRIMARY KEY,
    id_sku                        INTEGER                 NOT NULL REFERENCES skus(id_sku),
    id_cliente                     INTEGER                 NOT NULL REFERENCES clientes(id_cliente),
    id_ubicacion                    INTEGER                 NOT NULL REFERENCES ubicaciones(id_ubicacion),
    id_usuario_reporta               INTEGER                 NOT NULL REFERENCES usuarios(id_usuario),
    tipo_discrepancia                tipo_discrepancia       NOT NULL,
    observacion                      TEXT                    NOT NULL,
    estado                           estado_discrepancia     NOT NULL DEFAULT 'abierto',
    id_ajuste_resultante              INTEGER                 REFERENCES ajustes_inventario(id_ajuste),
    fecha_reporte                     TIMESTAMPTZ             NOT NULL DEFAULT now(),
    fecha_resolucion                  TIMESTAMPTZ
);

CREATE TABLE auditoria (
    id_auditoria             BIGSERIAL       PRIMARY KEY,
    id_usuario               INTEGER         REFERENCES usuarios(id_usuario),
    tabla_afectada             VARCHAR(50)     NOT NULL,
    id_entidad_afectada         INTEGER         NOT NULL,
    accion                      VARCHAR(30)     NOT NULL,
    valores_anteriores           JSONB,
    valores_nuevos               JSONB,
    fecha                        TIMESTAMPTZ     NOT NULL DEFAULT now()
);

-- =============================================================
-- AUDIT TRIGGER (RNF08 — every inventory-affecting write gets an
-- immutable audit row; id_usuario comes from the session variable
-- the API sets per request, not from the row itself)
-- =============================================================
CREATE OR REPLACE FUNCTION fn_registrar_auditoria() RETURNS TRIGGER AS $$
DECLARE
    v_usuario_id INTEGER;
    v_entidad_id INTEGER;
BEGIN
    v_usuario_id := NULLIF(current_setting('app.current_user_id', true), '')::INTEGER;

    IF TG_OP = 'INSERT' THEN
        v_entidad_id := (to_jsonb(NEW) ->> TG_ARGV[0])::INTEGER;
        INSERT INTO auditoria (id_usuario, tabla_afectada, id_entidad_afectada, accion, valores_anteriores, valores_nuevos)
        VALUES (v_usuario_id, TG_TABLE_NAME, v_entidad_id, TG_OP, NULL, to_jsonb(NEW));
        RETURN NEW;
    ELSIF TG_OP = 'UPDATE' THEN
        v_entidad_id := (to_jsonb(NEW) ->> TG_ARGV[0])::INTEGER;
        INSERT INTO auditoria (id_usuario, tabla_afectada, id_entidad_afectada, accion, valores_anteriores, valores_nuevos)
        VALUES (v_usuario_id, TG_TABLE_NAME, v_entidad_id, TG_OP, to_jsonb(OLD), to_jsonb(NEW));
        RETURN NEW;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_auditoria_entradas
    AFTER INSERT ON entradas
    FOR EACH ROW EXECUTE FUNCTION fn_registrar_auditoria('id_entrada');

CREATE TRIGGER trg_auditoria_stock_actual
    AFTER UPDATE ON stock_actual
    FOR EACH ROW EXECUTE FUNCTION fn_registrar_auditoria('id_stock');

CREATE TRIGGER trg_auditoria_ordenes_despacho_insert
    AFTER INSERT ON ordenes_despacho
    FOR EACH ROW EXECUTE FUNCTION fn_registrar_auditoria('id_despacho');

CREATE TRIGGER trg_auditoria_ordenes_despacho_update
    AFTER UPDATE ON ordenes_despacho
    FOR EACH ROW EXECUTE FUNCTION fn_registrar_auditoria('id_despacho');

CREATE TRIGGER trg_auditoria_ajustes
    AFTER INSERT ON ajustes_inventario
    FOR EACH ROW EXECUTE FUNCTION fn_registrar_auditoria('id_ajuste');


ALTER TABLE clientes ENABLE ROW LEVEL SECURITY;
ALTER TABLE clientes FORCE ROW LEVEL SECURITY;
CREATE POLICY clientes_select ON clientes FOR SELECT USING (true);
CREATE POLICY clientes_write  ON clientes FOR INSERT WITH CHECK (current_setting('app.current_user_role', true) = 'Administrador');
CREATE POLICY clientes_update ON clientes FOR UPDATE
    USING (current_setting('app.current_user_role', true) = 'Administrador')
    WITH CHECK (current_setting('app.current_user_role', true) = 'Administrador');

ALTER TABLE ubicaciones ENABLE ROW LEVEL SECURITY;
ALTER TABLE ubicaciones FORCE ROW LEVEL SECURITY;
CREATE POLICY ubicaciones_select ON ubicaciones FOR SELECT USING (true);
CREATE POLICY ubicaciones_write  ON ubicaciones FOR INSERT WITH CHECK (current_setting('app.current_user_role', true) = 'Administrador');
CREATE POLICY ubicaciones_update ON ubicaciones FOR UPDATE
    USING (current_setting('app.current_user_role', true) = 'Administrador')
    WITH CHECK (current_setting('app.current_user_role', true) = 'Administrador');


ALTER TABLE usuarios ENABLE ROW LEVEL SECURITY;
ALTER TABLE usuarios FORCE ROW LEVEL SECURITY;
CREATE POLICY usuarios_select ON usuarios FOR SELECT
    USING (
        current_setting('app.current_user_role', true) = 'Administrador'
        OR id_usuario = NULLIF(current_setting('app.current_user_id', true), '')::INTEGER
    );

CREATE POLICY usuarios_select_login ON usuarios FOR SELECT
    USING (current_setting('app.auth_stage', true) = 'login');
CREATE POLICY usuarios_insert ON usuarios FOR INSERT
    WITH CHECK (current_setting('app.current_user_role', true) = 'Administrador');
CREATE POLICY usuarios_update ON usuarios FOR UPDATE
    USING (
        current_setting('app.current_user_role', true) = 'Administrador'
        OR id_usuario = NULLIF(current_setting('app.current_user_id', true), '')::INTEGER
    )
    WITH CHECK (
        current_setting('app.current_user_role', true) = 'Administrador'
        OR id_usuario = NULLIF(current_setting('app.current_user_id', true), '')::INTEGER
    );

ALTER TABLE refresh_tokens ENABLE ROW LEVEL SECURITY;
ALTER TABLE refresh_tokens FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS refresh_tokens_insert ON refresh_tokens;
CREATE POLICY refresh_tokens_select ON refresh_tokens FOR SELECT
    USING (
        current_setting('app.current_user_role', true) = 'Administrador'
        OR id_usuario = NULLIF(current_setting('app.current_user_id', true), '')::INTEGER
    );
CREATE POLICY refresh_tokens_insert ON refresh_tokens FOR INSERT
    WITH CHECK (
        current_setting('app.auth_stage', true) = 'login'
        OR id_usuario = NULLIF(current_setting('app.current_user_id', true), '')::INTEGER
    );
CREATE POLICY refresh_tokens_update ON refresh_tokens FOR UPDATE
    USING (id_usuario = NULLIF(current_setting('app.current_user_id', true), '')::INTEGER)
    WITH CHECK (id_usuario = NULLIF(current_setting('app.current_user_id', true), '')::INTEGER);
CREATE POLICY refresh_tokens_select_by_hash ON refresh_tokens FOR SELECT
    USING (current_setting('app.auth_stage', true) = 'login');

CREATE OR REPLACE FUNCTION fn_apply_cliente_rls(p_table regclass) RETURNS void AS $$
BEGIN
    EXECUTE format('ALTER TABLE %s ENABLE ROW LEVEL SECURITY', p_table);
    EXECUTE format('ALTER TABLE %s FORCE ROW LEVEL SECURITY', p_table);
    EXECUTE format($f$
        CREATE POLICY %1$I_select ON %2$s FOR SELECT USING (
            current_setting('app.current_user_role', true) = 'Administrador'
            OR id_cliente = NULLIF(current_setting('app.current_cliente_id', true), '')::INTEGER
        )$f$, p_table::text, p_table);
    EXECUTE format($f$
        CREATE POLICY %1$I_insert ON %2$s FOR INSERT WITH CHECK (
            current_setting('app.current_user_role', true) = 'Administrador'
            AND id_cliente = NULLIF(current_setting('app.current_cliente_id', true), '')::INTEGER
        )$f$, p_table::text, p_table);
    EXECUTE format($f$
        CREATE POLICY %1$I_update ON %2$s FOR UPDATE
        USING (
            current_setting('app.current_user_role', true) = 'Administrador'
            AND id_cliente = NULLIF(current_setting('app.current_cliente_id', true), '')::INTEGER
        )
        WITH CHECK (
            current_setting('app.current_user_role', true) = 'Administrador'
            AND id_cliente = NULLIF(current_setting('app.current_cliente_id', true), '')::INTEGER
        )$f$, p_table::text, p_table);
END;
$$ LANGUAGE plpgsql;

SELECT fn_apply_cliente_rls('skus');
SELECT fn_apply_cliente_rls('stock_actual');
SELECT fn_apply_cliente_rls('entradas');
SELECT fn_apply_cliente_rls('ordenes_despacho');
SELECT fn_apply_cliente_rls('ajustes_inventario');
SELECT fn_apply_cliente_rls('reportes_discrepancia');


ALTER TABLE auditoria ENABLE ROW LEVEL SECURITY;
ALTER TABLE auditoria FORCE ROW LEVEL SECURITY;
CREATE POLICY auditoria_select ON auditoria FOR SELECT
    USING (current_setting('app.current_user_role', true) = 'Administrador');
CREATE POLICY auditoria_insert ON auditoria FOR INSERT
    WITH CHECK (current_setting('app.current_user_role', true) IS NOT NULL);


-- TODO:
---------------------- DELETE LATER
INSERT INTO usuarios (nombre_completo, correo_institucional, contrasena_hash, rol)
VALUES (
    'Administrador',
    'admin@mudanzasmundiales.cr',
    '$2b$12$03A4GdQu9oiETBcY906EEufVtVijsacCNNnl5EOOTh5Idmig4Dt0K',
    'Administrador'
);