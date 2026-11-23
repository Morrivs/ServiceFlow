# AGENTS.md

## Resumen

ServiceFlow es una API REST para gestionar servicios (clientes, solicitudes de servicio, cotizaciones y órdenes de trabajo). Stack: **ASP.NET Core / .NET 10** (SDK 10.0.401), **EF Core 10** con **SQL Server 2022**, **ASP.NET Core Identity** y **xUnit**. La solución usa formato `.slnx`: `ServiceFlowS.slnx`.

## Estructura del repositorio

Los proyectos están en la raíz; el `.slnx` los agrupa en las carpetas virtuales `/src` y `/tests`.

```
ServiceFlowS.slnx              # solución (carpetas /src y /tests)
ServiceFlow.Domain/            # /src — entidades y reglas de negocio
ServiceFlow.Application/       # /src — casos de uso, interfaces, DTOs
ServiceFlow.Infrastructure/    # /src — EF Core, Identity, configuraciones, migraciones
ServiceFlow.API/               # /src — controllers, Program.cs, appsettings
ServiceFlow.UnitTests/         # /tests
ServiceFlow.IntegrationTests/  # /tests
ServiceFlow.ArchitectureTests/ # /tests
docker-compose.yml             # SQL Server 2022 (serviceflow-sql, puerto 1433)
.env                           # SA_PASSWORD (ignorado por git)
.editorconfig
```

## Regla de oro: dependencias

El sentido de las referencias es siempre hacia el centro:

`Domain` ← `Application` ← `Infrastructure` ← `API`

- `Domain`: **no** referencia ningún otro proyecto; no depende de EF Core ni de ASP.NET.
- `Application`: solo referencia `Domain`.
- `Infrastructure`: referencia `Application` y `Domain`; implementa sus interfaces.
- `API`: referencia `Application` e `Infrastructure`; es el único punto de composición (`Program.cs`).
- Los proyectos de tests referencian lo que necesiten, pero el código de producción nunca referencia tests.
- Si una capa necesita algo de una externa, define una interfaz en `Application`/`Domain` y su implementación en `Infrastructure`.

## Comandos habituales

```bash
dotnet build ServiceFlowS.slnx
dotnet test ServiceFlowS.slnx
dotnet run --project ServiceFlow.API
docker compose up -d          # SQL Server en localhost:1433

# Migraciones (requiere la herramienta dotnet-ef)
dotnet ef migrations add <Nombre> --project ServiceFlow.Infrastructure --startup-project ServiceFlow.API
dotnet ef database update --project ServiceFlow.Infrastructure --startup-project ServiceFlow.API
```

URLs de desarrollo (ver `ServiceFlow.API/Properties/launchSettings.json`): `http://localhost:5003` y `https://localhost:7242`. El archivo `ServiceFlow.API.http` usa el puerto HTTP.

## Configuración y secretos

- `AddInfrastructure` lee `ConnectionStrings:Default`, que **no está** en `appsettings.json`. Proporcionarlo con user-secrets (`dotnet user-secrets set "ConnectionStrings:Default" "..."` desde `ServiceFlow.API`) o con la variable de entorno `ConnectionStrings__Default`.
- `docker-compose.yml` toma `MSSQL_SA_PASSWORD` de `.env` (`SA_PASSWORD`). `.env` está en `.gitignore`: **nunca commitear contraseñas ni secretos**.
- `appsettings.Development.json` solo ajusta logging; no añadir secretos a ningún `appsettings*.json`.

## Convenciones de código

- C# con `Nullable` y `ImplicitUsings` habilitados; namespaces con bloque `{ }`, llaves en línea nueva (estilo Allman) y 4 espacios de indentación.
- Identificadores en inglés; comentarios y reglas de negocio en español, con formato `RN-XXX` (p. ej. `RN-015: sin borrado en cascada`).
- Entidades de dominio ricas: constructor público para crearlas, constructor privado para EF, propiedades con `private set` y métodos de comportamiento (`Deactivate()`), sin setters públicos.
- Fechas siempre UTC con sufijo `Utc` (`CreatedAtUtc`, `UpdatedAtUtc`, `DateTime.UtcNow`).
- Concurrencia optimista con `RowVersion` (`byte[]`, `.IsRowVersion()`).
- Reglas de persistencia en `ServiceFlow.Infrastructure/Persistence/Configurations`, como `internal sealed class XxxConfiguration : IEntityTypeConfiguration<T>`; se descubren con `ApplyConfigurationsFromAssembly`.
- Decimales con precisión `(18, 2)` por convención global en `ServiceFlowDbContext`; todas las FK con `DeleteBehavior.Restrict` (RN-015), sin borrado en cascada.
- Índices únicos con nombre `UX_<Tabla>_<Columna>` (p. ej. `UX_Customers_Email`).
- Identity usa claves `int`: `ApplicationUser : IdentityUser<int>`, `ApplicationRole` y tablas `Users`, `Roles`, `UserRoles`, etc. Hay secuencias `seq_ServiceRequestNumber`, `seq_QuoteNumber` y `seq_WorkOrderNumber` para la numeración.
- No editar a mano los archivos de `Migrations/` ni el *snapshot*: generar siempre nuevas migraciones con `dotnet ef`.

## Estado actual y notas para agentes

- `ServiceFlow.API/WeatherForecast.cs` y `ServiceFlow.API/Controllers/WeatherForecastController.cs` son plantilla de Visual Studio; no son parte del dominio.
- Los tres proyectos de tests contienen solo el stub `IntegrationTest1.cs` (plantilla de Aspire, sin tests activos). `ArchitectureTests` es el lugar previsto para verificar las dependencias entre capas.
- `Program.cs` registra infraestructura, controllers y OpenAPI; OpenAPI se expone solo en `Development`. El pipeline incluye `UseAuthorization()`, pero todavía no hay autenticación configurada.
- Al implementar una funcionalidad nueva: entidad en `Domain`, interfaz/caso de uso en `Application`, implementación EF en `Infrastructure`, endpoint en `API` y tests en el proyecto correspondiente.
- Después de cualquier cambio: `dotnet build ServiceFlowS.slnx` y `dotnet test ServiceFlowS.slnx`. Si cambia el modelo de datos, crear la migración correspondiente.

## limites
- Pregunta antes: crear archivos nuevos, cambiar el formato de los datos guardados.
- nunca: añadir dependencias, frameworks o un paso de build en la capa de dominio, hay que respetar la arquitectura limpia.
- Siempre: actualizar `MEMORY.md` al terminar cada tarea. 
## Memoria 
- Al empezar, lee `MEMORY.md` para conocer el estado del proyecto y las decisiones 
tomadas. 
- Al terminar una tarea, actualízalo: estado actual, decisiones importantes (con su 
porqué) y errores a evitar. 
- Mantenlo breve (máximo ~50 líneas): resume o elimina lo que ya no aporte. 
- Si algo se convierte en una regla permanente, propón moverlo a `AGENTS.md` en lugar de 
dejarlo en la memoria. 
- No guardes nunca datos sensibles (claves, tokens, datos personales). 