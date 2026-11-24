# MEMORY.md

Memoria de trabajo de ServiceFlow: se lee al empezar y se actualiza al terminar cada tarea. Máximo ~50 líneas; sin datos sensibles.

## Estado actual

- Solución `ServiceFlowS.slnx` (.NET 10): 4 proyectos en `/src` y 3 en `/tests`.
- Arquitectura limpia ya definida: `Customer` en `Domain`, EF Core + Identity<int> en `Infrastructure`, migración `InitialCreate` generada.
- API: controllers + OpenAPI/Swagger (SF-004). `Swashbuckle.AspNetCore` 11.0.1; Swagger expuesto en **todos** los entornos (`/swagger`, `/swagger/v1/swagger.json`).
- Esquema `Bearer` documentado (JWT) con requisito global. **No hay validación JWT real**: falta `AddAuthentication`/`AddJwtBearer` y `[Authorize]` (otro issue).
- Endpoint de ejemplo versionado: `WeatherForecastController` usa `[Route("api/v1/[controller]")]` (plantilla VS reutilizada; sustituir por endpoints de dominio cuando lleguen).
- Tests: 7 pruebas de integración nuevas en `ServiceFlow.IntegrationTests/OpenApi/SwaggerTests.cs` (todas pasan). Stubs de plantilla siguen en los otros proyectos.
- `GlobalExceptionHandler` existe pero **no está registrado** en DI; 8 tests preexistentes de `ProblemDetailsTests` fallan por eso y por validación en `record ModelDto` de .NET 10. No eliminar ni ocultar.

## Decisiones (y porqué)

- Textos y comentarios en español: coincide con los commits, los comentarios `RN` y `AGENTS.md`.
- Proyectos en la raíz agrupados en `/src` y `/tests` en el `.slnx`: es la estructura existente.
- Sin dependencias, frameworks ni pasos de build nuevos sin preguntar: límite explícito del usuario. Swashbuckle se añadió con aprobación explícita para SF-004.
- Cambiar el formato de los datos guardados requiere confirmación previa: límite explícito del usuario.
- Nunca guardar secretos aquí ni en `appsettings` (`.env` está en `.gitignore`).
- Se quitó `Microsoft.AspNetCore.OpenApi`: exige `Microsoft.OpenApi < 3.0` y Swashbuckle 11 necesita `>= 3.11.0` (conflicto NU1107). Un solo generador de documentación evita dos JSON distintos.
- Swagger en todos los entornos porque RNF-008 no lo limita a Development (decisión del usuario).

## Errores a evitar

- Invertir las dependencias entre capas (`Domain` no debe referenciar capas externas).
- Editar migraciones o el *snapshot* a mano: generar nuevas con `dotnet ef`.
- Commitear `.env`, contraseñas o datos personales.
- Añadir paquetes o modificar el pipeline de build sin permiso.
- Reintroducir `Microsoft.AspNetCore.OpenApi` junto a Swashbuckle 11: conflicto de versiones.
- Con Swashbuckle 11 los tipos van en `Microsoft.OpenApi` (no `.Models`); el requisito de seguridad usa `OpenApiSecuritySchemeReference("Bearer", document)`, no `OpenApiReference`.
- La configuración de Swagger UI 11 está en `/swagger/index.js`, no inline en el HTML.
- Confundir "documentar Bearer" con "validar el token": son cosas distintas.
- Dejar la memoria desactualizada al terminar una tarea.

## Pendientes / a confirmar

- Arreglar `ProblemDetailsTests`: registrar `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()` y mover la validación de `ModelDto` al parámetro del constructor del `record`.
- Sustituir los tests plantilla por pruebas reales (Unit/Architecture).
- Definir cuándo se configura autenticación JWT real en la API.
