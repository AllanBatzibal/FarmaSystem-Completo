# FarmaSystem

Sistema de gestión farmacéutica con arquitectura en capas (MVC).

```
[Vue.js Frontend] → HTTP/REST → [API Controllers] → [Services] → [DbContext / SQL Server]
     Vista              Controlador              Modelo           Base de Datos
```

## Estructura

| Carpeta | Descripción |
|---------|-------------|
| `backend/` | Solución .NET 8: Core, Infrastructure, API, Tests |
| `frontend/` | Vue 3 + Pinia + Vite |
| `database/` | Scripts SQL (schema y seed) |

## Ejecución

### Backend

```powershell
cd backend/FarmaSystem.API
dotnet run
```

API: `http://localhost:5280` · Swagger: `/swagger`

### Frontend

```powershell
cd frontend
copy .env.example .env
npm install
npm run dev
```

App: `http://localhost:5173`

## Compilar

```powershell
cd backend
dotnet build FarmaSystem.sln
```

## Capas del backend

- **FarmaSystem.Core** — Modelos, DTOs, interfaces de servicios
- **FarmaSystem.Infrastructure** — EF Core, implementaciones de servicios, AutoMapper
- **FarmaSystem.API** — Controladores REST, JWT, Swagger
