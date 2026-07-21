-- =============================================================================
-- Mudanzas WMS — 003_mock_data.sql
-- Seed script for environment testing.
-- Bypasses RLS locks using 'Administrador' session variables contextually.
-- =============================================================================

BEGIN;

-- 1. Elevate session privileges to bypass initial bootstrap constraints
SET LOCAL app.current_user_role = 'Administrador';
SET LOCAL app.current_user_id = '1';
SET LOCAL app.auth_stage = 'seeding';

-- 2. Clear pre-existing data safely
TRUNCATE TABLE 
    auditoria, reportes_discrepancia, ajustes_inventario, ordenes_despacho, 
    entradas, stock_actual, refresh_tokens, skus, ubicaciones, usuarios, clientes 
    RESTART IDENTITY CASCADE;

-- =============================================================================
-- SEEDING: USUARIOS
-- =============================================================================
INSERT INTO usuarios (nombre_completo, correo_institucional, contrasena_hash, rol, activo, intentos_fallidos, bloqueado_hasta)
VALUES 
(
    'Carlos Mendoza Brenes', 
    'carlos.mendoza@mudanzasmundiales.cr', 
    '$2b$12$03A4GdQu9oiETBcY906EEufVtVijsacCNNnl5EOOTh5Idmig4Dt0K', 
    'Administrador', 
    TRUE, 
    0, 
    NULL
),
(
    'Fabiola Arce Rojas', 
    'fabiola.arce@mudanzasmundiales.cr', 
    '$2b$12$K7e1BvXyZw9LqR2P4mNoUu1hGjK3lM4nO5pQ6rS7tU8vW9xY0z1a2', 
    'Analista', 
    TRUE, 
    0, 
    NULL
),
(
    'Esteban Granados Mora', 
    'esteban.granados@mudanzasmundiales.cr', 
    '$2b$12$L8f2CwYzAx0Mr3Q5nOpVv2iHkL4mM5nO6pQ7rS8tU9vW0xY1z2a3b', 
    'Operario', 
    TRUE, 
    0, 
    NULL
),
(
    'Usuario Sospechoso Bloqueado', 
    'brute.force@mudanzasmundiales.cr', 
    '$2b$12$M9g3DxZaBy1Ns4R6pOqWw3jIlM5nN6pO7qR8sS9tU0vW1xY2z3a4c', 
    'Operario', 
    TRUE, 
    5, 
    NOW() + INTERVAL '30 minutes'
);

-- =============================================================================
-- SEEDING: REFRESH TOKENS
-- =============================================================================
INSERT INTO refresh_tokens (id_usuario, token_hash, fecha_expiracion, revocado)
VALUES 
(1, 'd3b07384d113edec49eaa6238ad5ff00', NOW() + INTERVAL '7 days', FALSE),
(2, '65c9281a8b34f71a7d6567213451cef2', NOW() + INTERVAL '7 days', FALSE),
(3, '12a8374f6e32d4b1a23456789abcdef0', NOW() - INTERVAL '1 day', TRUE);

-- =============================================================================
-- SEEDING: CLIENTES
-- =============================================================================
INSERT INTO clientes (nombre_empresa, identificacion_juridica, contacto_nombre, contacto_telefono, contacto_correo, activo)
VALUES 
('Tech Components International', '3-101-554321', 'Lorena Mora Fonseca', '+506 2201-4000', 'logistics@techcomp.com', TRUE),
('Distribuidora Alimentaria del Sur', '3-102-987654', 'Juan Carlos Mora', '+506 2552-1100', 'inventarios@dialsul.cr', TRUE),
('Corporación Textil del Este', '3-104-123456', 'Andrés Vargas Vega', '+506 2290-7788', 'avargas@textileste.com', FALSE);

-- =============================================================================
-- SEEDING: UBICACIONES (Warehouse Layout Matrix)
-- =============================================================================
INSERT INTO ubicaciones (codigo, pasillo, estante, capacidad_maxima, activo)
VALUES 
('A-01-01', 'Pasillo A', 'Estante 1', 150, TRUE),
('A-01-02', 'Pasillo A', 'Estante 1', 150, TRUE),
('A-02-01', 'Pasillo A', 'Estante 2', 150, TRUE),
('A-02-02', 'Pasillo A', 'Estante 2', 150, TRUE),
('B-01-01', 'Pasillo B', 'Estante 1', 200, TRUE),
('B-01-02', 'Pasillo B', 'Estante 1', 200, TRUE),
('B-02-01', 'Pasillo B', 'Estante 2', 200, TRUE),
('B-02-02', 'Pasillo B', 'Estante 2', 200, TRUE),
('C-01-01', 'Pasillo C', 'Estante 1', 100, TRUE),
('C-01-02', 'Pasillo C', 'Estante 1', 100, TRUE),
('C-02-01', 'Pasillo C', 'Estante 2', 100, TRUE),
('C-02-02', 'Pasillo C', 'Estante 2', 100, TRUE),
('C-03-01', 'Pasillo C', 'Estante 3', 100, TRUE),
('D-01-01', 'Pasillo D', 'Estante 1', 300, TRUE),
('D-01-02', 'Pasillo D', 'Estante 1', 300, TRUE),
('Z-99-99', 'Pasillo Z', 'Estante 9', 10, FALSE);

-- =============================================================================
-- SEEDING: SKUS
-- =============================================================================
SET LOCAL app.current_cliente_id = '1'; 
INSERT INTO skus (id_cliente, codigo_sku, nombre, descripcion, unidad_medida, fecha_limite_almacenamiento, activo)
VALUES 
(1, 'SKU-RAM-001', 'Memoria RAM DDR4 16GB', 'Módulos de memoria de alta densidad para servidores', 'Unidades', NULL, TRUE),
(1, 'SKU-SSD-002', 'Disco Duro SSD 1TB NVMe', 'Unidades de estado sólido empresariales M.2', 'Unidades', NULL, TRUE);

-- =============================================================================
-- SEEDING: 15 LOTES DE STOCK ACTUAL (CON NUMERO_LOTE) Y ENTRADAS ASOCIADAS
-- =============================================================================
SET LOCAL app.current_user_id = '3';
SET LOCAL app.current_cliente_id = '1';

-- SKU 1 Lot Breakdown (8 Lotes)
INSERT INTO stock_actual (id_sku, id_cliente, id_ubicacion, numero_lote, cantidad_actual) VALUES
(1, 1, 1, 'LOT-RAM-2026-001', 25),
(1, 1, 2, 'LOT-RAM-2026-002', 40),
(1, 1, 3, 'LOT-RAM-2026-003', 15),
(1, 1, 4, 'LOT-RAM-2026-004', 30),
(1, 1, 5, 'LOT-RAM-2026-005', 50),
(1, 1, 6, 'LOT-RAM-2026-006', 20),
(1, 1, 7, 'LOT-RAM-2026-007', 10),
(1, 1, 8, 'LOT-RAM-2026-008', 35);

INSERT INTO entradas (id_sku, id_cliente, id_ubicacion, id_usuario, cantidad, fecha_ingreso) VALUES
(1, 1, 1, 3, 25, '2026-07-01'),
(1, 1, 2, 3, 40, '2026-07-02'),
(1, 1, 3, 3, 15, '2026-07-03'),
(1, 1, 4, 3, 30, '2026-07-04'),
(1, 1, 5, 3, 50, '2026-07-05'),
(1, 1, 6, 3, 20, '2026-07-06'),
(1, 1, 7, 3, 10, '2026-07-07'),
(1, 1, 8, 3, 35, '2026-07-08');

-- SKU 2 Lot Breakdown (7 Lotes)
INSERT INTO stock_actual (id_sku, id_cliente, id_ubicacion, numero_lote, cantidad_actual) VALUES
(2, 1, 9,  'LOT-SSD-2026-001', 60),
(2, 1, 10, 'LOT-SSD-2026-002', 45),
(2, 1, 11, 'LOT-SSD-2026-003', 80),
(2, 1, 12, 'LOT-SSD-2026-004', 15),
(2, 1, 13, 'LOT-SSD-2026-005', 25),
(2, 1, 14, 'LOT-SSD-2026-006', 100),
(2, 1, 15, 'LOT-SSD-2026-007', 75);

INSERT INTO entradas (id_sku, id_cliente, id_ubicacion, id_usuario, cantidad, fecha_ingreso) VALUES
(2, 1, 9,  3, 60,  '2026-07-09'),
(2, 1, 10, 3, 45,  '2026-07-10'),
(2, 1, 11, 3, 80,  '2026-07-11'),
(2, 1, 12, 3, 15,  '2026-07-12'),
(2, 1, 13, 3, 25,  '2026-07-13'),
(2, 1, 14, 3, 100, '2026-07-14'),
(2, 1, 15, 3, 75,  '2026-07-15');

-- =============================================================================
-- SEEDING: ORDENES DE DESPACHO
-- =============================================================================
SET LOCAL app.current_cliente_id = '1';

INSERT INTO ordenes_despacho (id_sku, id_cliente, cantidad_solicitada, fecha_salida, estado, id_usuario_creacion)
VALUES (1, 1, 10, '2026-07-20', 'pendiente', 2);

INSERT INTO ordenes_despacho (id_sku, id_cliente, cantidad_solicitada, fecha_salida, estado, id_usuario_creacion, id_usuario_confirmacion, fecha_confirmacion)
VALUES (2, 1, 5, '2026-07-17', 'confirmado', 2, 1, NOW() - INTERVAL '1 day');

-- =============================================================================
-- SEEDING: AJUSTES & DISCREPANCIAS
-- =============================================================================
SET LOCAL app.current_cliente_id = '1';
SET LOCAL app.current_user_id = '2';

INSERT INTO ajustes_inventario (id_sku, id_cliente, id_ubicacion, id_usuario_solicitante, tipo_ajuste, cantidad_ajuste, razon)

VALUES (1, 1, 1, 2, 'dano', -2, 'Dos módulos RAM rotos al colapsar caja contenedora exterior');

INSERT INTO ajustes_inventario (id_sku, id_cliente, id_ubicacion, id_usuario_solicitante, tipo_ajuste, cantidad_ajuste, razon)
VALUES (2, 1, 9, 2, 'conteo', 1, 'Ajuste compensatorio por item sobrante recuperado');

INSERT INTO reportes_discrepancia (id_sku, id_cliente, id_ubicacion, id_usuario_reporta, tipo_discrepancia, observacion, estado, id_ajuste_resultante, fecha_resolucion)
VALUES (2, 1, 9, 3, 'sobrante', 'Se halló una unidad SSD no registrada en el estante superior.', 'resuelto', 2, NOW());

-- =============================================================================
-- TESTING METADATA SANITY CHECKS
-- =============================================================================
SET LOCAL app.current_user_role = '';
SET LOCAL app.current_user_id = '';
SET LOCAL app.current_cliente_id = '';
SET LOCAL app.auth_stage = '';

COMMIT;