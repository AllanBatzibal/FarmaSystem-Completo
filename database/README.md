# Base de datos FarmaSystemDB

## Restauración

1. Ejecute el script principal en SQL Server Management Studio:

   `schema/FarmaSystemDB.sql`

2. (Opcional) Cargue datos de usuarios de prueba:

   `seed/AuthUsuarios.sql`

## Conexión

La cadena de conexión se configura en `backend/FarmaSystem.API/appsettings.json`:

```
Server=SU_SERVIDOR;Database=FarmaSystemDB;Trusted_Connection=True;TrustServerCertificate=True;
```
