# Plataforma de Incidencias — Bicicletas Compartidas

Proyecto base: ASP.NET Core MVC + Identity + EF Core (SQLite).

## Cómo correrlo localmente

```bash
cd IncidenciasApp
dotnet restore
dotnet tool install --global dotnet-ef   # solo la primera vez
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

Abrir `https://localhost:5001` (o el puerto que indique la consola).

Usuario de prueba: `admin@incidencias.com` / `Admin123!`

## Configurar claves reales (NO se suben a git)

En vez de editar `appsettings.json` con las claves reales, usa *user-secrets* localmente:

```bash
dotnet user-secrets init
dotnet user-secrets set "Algolia:ApplicationId" "TU_APP_ID"
dotnet user-secrets set "Algolia:ApiKey" "TU_API_KEY"
dotnet user-secrets set "ConnectionStrings:Redis" "TU_CONNECTION_STRING_REDIS"
dotnet user-secrets set "PieHost:WebSocketUrl" "wss://..."
```

En Render, estas mismas claves se configuran como **Environment Variables** del Web Service (ver sección de despliegue).

## Estado del repositorio

- `main`: commit inicial del proyecto base (login, EF Core, SQLite, listado y cierre de incidencias).
- Cada pregunta del examen se implementa en su propia rama (`feature/...`) y se fusiona a `main` mediante Pull Request.

## Commit desplegado en Render

_(completar al final con el hash del commit de main que quedó publicado)_

## Pruebas realizadas

_(completar: capturas o pasos de cómo se probó Algolia, Redis y PieHost en producción)_
