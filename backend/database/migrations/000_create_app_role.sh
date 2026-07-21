
set -e

: "${WMS_APP_DB_PASSWORD:?WMS_APP_DB_PASSWORD must be set (see .env)}"

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    DO \$\$
    BEGIN
        IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'wms_app') THEN
            CREATE ROLE wms_app LOGIN PASSWORD '$WMS_APP_DB_PASSWORD';
        ELSE
            ALTER ROLE wms_app WITH PASSWORD '$WMS_APP_DB_PASSWORD';
        END IF;
    END
    \$\$;

    GRANT CONNECT ON DATABASE "$POSTGRES_DB" TO wms_app;
    GRANT USAGE ON SCHEMA public TO wms_app;

    -- Applies automatically to tables/sequences/functions 001_init.sql is
    -- about to create (same schema, created afterward by POSTGRES_USER).
    ALTER DEFAULT PRIVILEGES IN SCHEMA public
        GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO wms_app;
    ALTER DEFAULT PRIVILEGES IN SCHEMA public
        GRANT USAGE, SELECT ON SEQUENCES TO wms_app;
    ALTER DEFAULT PRIVILEGES IN SCHEMA public
        GRANT EXECUTE ON FUNCTIONS TO wms_app;
EOSQL

echo "wms_app role ready."
