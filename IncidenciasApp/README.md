# Plataforma de Incidencias - Bicicletas Compartidas

## Despliegue
- URL publica: (pega aqui la URL de tu servicio en Render)
- Commit desplegado: b20be2f

## Tecnologias
- ASP.NET Core MVC + Identity + EF Core + SQLite
- Algolia: busqueda por estacion/descripcion, solo incidencias abiertas
- Redis: cache 60s del listado general, invalidada al cerrar
- PieSocket (WebSocket): evento IncidenciaActualizada tras persistir en BD

## Variables de entorno (configuradas en Render, no en el repo)
- ASPNETCORE_ENVIRONMENT
- Algolia__ApplicationId, Algolia__ApiKey, Algolia__IndexName
- ConnectionStrings__Redis
- PieSocket__ApiKey, PieSocket__Cluster

## Pruebas realizadas
- Busqueda Algolia: "Universidad" devuelve resultado real; "Mercado" (cerrada) no aparece
- Redis: log confirma lectura desde cache tras 60s
- PieSocket: al cerrar una incidencia se actualiza en tiempo real sin recargar

## Historial de ramas y merges
Ver: git log --graph --oneline --all
