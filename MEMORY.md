# MEMORY.md

Memoria de trabajo de ServiceFlow: se lee al empezar y se actualiza al terminar cada tarea. Máximo ~50 líneas; sin datos sensibles.

## Estado actual

- Solución `ServiceFlowS.slnx` (.NET 10): 4 proyectos en `/src` y 3 en `/tests`.
- Arquitectura limpia ya definida: `Customer` en `Domain`, EF Core + Identity<int> en `Infrastructure`, migración `InitialCreate` generada.
- API con controllers y OpenAPI solo en `Development`; autenticación aún sin configurar.
- Tests: solo stubs de plantilla (`IntegrationTest1.cs`), sin casos reales.
- Documentación: `AGENTS.md` con stack, dependencias, comandos, convenciones, límites e instrucciones de memoria; `MEMORY.md` creado como memoria de trabajo del proyecto.

## Decisiones (y porqué)

- Textos y comentarios en español: coincide con los commits, los comentarios `RN` y `AGENTS.md`.
- Proyectos en la raíz agrupados en `/src` y `/tests` en el `.slnx`: es la estructura existente.
- Sin dependencias, frameworks ni pasos de build nuevos sin preguntar: límite explícito del usuario.
- Cambiar el formato de los datos guardados requiere confirmación previa: límite explícito del usuario.
- Nunca guardar secretos aquí ni en `appsettings` (`.env` está en `.gitignore`).

## Errores a evitar

- Invertir las dependencias entre capas (`Domain` no debe referenciar capas externas).
- Editar migraciones o el *snapshot* a mano: generar nuevas con `dotnet ef`.
- Commitear `.env`, contraseñas o datos personales.
- Añadir paquetes o modificar el pipeline de build sin permiso.
- Dejar la memoria desactualizada al terminar una tarea.

## Pendientes / a confirmar

- Sustituir los tests plantilla por pruebas reales (Unit/Integration/Architecture).
- Definir cuándo se configurará autenticación en el pipeline de la API.
