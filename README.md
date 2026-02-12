# CreditSystem - Backend Financiero con Procesos Asincrónicos

Sistema de alta de solicitudes de crédito con procesamiento asincronico, comunicacion basada en eventos y manejo de fallos parciales

## Stack Tecnológico

- .NET 8 (LTS)
- ASP.NET Core Web API
- Entity Framework Core (InMemory)
- MediatR (CQRS y eventos de dominio)
- System.Threading.Channels (cola in-memory)
- xUnit, FluentAssertions, NSubstitute

## Como ejecutar el proyecto

### Prerrequisitos

- .NET 8 SDK

### Ejecutar la API

```bash
cd CreditSystem.Api
dotnet run
```

Swagger UI disponible en `/swagger`

### Ejecutar los tests

```bash
dotnet test
```

Resultado esperado: 26 tests pasando (21 unitarios + 5 de integracion)

---

## Como se modela el proceso

La solicitud de credito atraviesa un pipeline secuencial de 4 pasos, cada uno implementado como un componente independiente (`IProcessStepExecutor`):

```
POST /api/creditapplications → Registra solicitud → Encola para procesamiento
                                                            ↓
                                              [BackgroundWorker consume]
                                                            ↓
                                              1. Validar elegibilidad
                                                            ↓
                                              2. Evaluar riesgo crediticio
                                                            ↓
                                              3. Calcular condiciones
                                                            ↓
                                              4. Emitir decisión final
```

La API responde inmediatamente con `202 Accepted` y el procesamiento ocurre en background. El cliente puede consultar el estado en cualquier momento con `GET /api/creditapplications/{id}`

### Estados de la solicitud

| Estado | Descripcion |
|---|---|
| `Created` | Solicitud registrada, pendiente de procesamiento |
| `InProgress` | Pipeline en ejecucion |
| `EligibilityFailed` | Rechazada por no cumplir requisitos mnimos de elegibilidad |
| `RiskRejected` | Rechazada por analisis de riesgo crediticio |
| `Rejected` | Decisión final negativa |
| `Approved` | Credito aprobado |
| `Faulted` | Error tecnico durante el procesamiento |
| `RetryPending` | Re ejecucion programada tras un fallo |

Se agregaron `EligibilityFailed` y `RiskRejected` como estados especificos para dar visibilidad sobre el motivo exacto del rechazo, facilitando el analisis de datos y la comunicación con el cliente

---

## Como se coordinan los pasos

### Orquestador (Process Manager)

El `CreditApplicationOrchestrator` coordina la ejecucion secuencial. Para cada paso:

1. Crea un registro `ProcessStep` para auditoriia
2. Ejecuta el paso correspondiente
3. Persiste el resultado (exito o fallo) de forma independiente
4. Publica un evento de dominio (`StepCompletedEvent` o `StepFailedEvent`)
5. Si falla, detiene el pipeline y asigna el estado correspondiente

### Comunicacion basada en eventos

Los eventos se publican como `INotification` de MediatR, lo que permite agregar comportamiento reactivo (logging, notificaciones) sin modificar el orquestador

### Procesamiento en background

El `CreditApplicationWorker` es un `BackgroundService` que consume solicitudes de un `Channel<Guid>`. Cada solicitud se procesa en un scope de DI independiente, asegurando aislamiento entre operacioness

### Desacoplamiento

- Cada paso implementa `IProcessStepExecutor` y se puede agregar, quitar o reordenar sin tocar el orquestador
- Los Controllers solo dependen de `IMediator`, no de servicios concretos
- La persistencia esta abstraida detrás de `ICreditApplicationRepository`

---

## Como se manejan errores y estados

### El proceso no es transaccional de punta a punta

Cada paso persiste su resultado de forma independiente. Si el paso 2 falla, el paso 1 ya quedo guardado con su resultado. Esto permite ver el progreso parcial y reintentar desde donde fallo

### Tres niveles de manejo de fallos

**Error de negocio:** Un paso retorna `StepResult(false, "mensaje")`. El orquestador registra el fallo, asigna un estado especifico segun el paso (`EligibilityFailed`, `RiskRejected`) y detiene el pipeline

**Excepción técnica:** Si un paso lanza una excepcion, el orquestador la captura, registra el error en el `ProcessStep`, asigna estado `Faulted` y loguea el detalle

**Error del worker:** Si el worker falla al procesar una solicitud, el error se captura sin detener el servicio, permitiendo continuar con las siguientes solicitudes

### Recuperacion ante errores

El endpoint `POST /api/creditapplications/{id}/retry` permite re procesar solicitudes en estado de fallo. Valida que la solicitud este en un estado "retriable" (`Faulted`, `EligibilityFailed`, `RiskRejected`, `Rejected`), cambia el estado a `RetryPending` y re encola para procesamiento

### Auditoria

Cada paso ejecutado genera un registro `ProcessStep` con: tipo de paso, resultado (exito/fallo), mensaje de error, cantidad de reintentos y timestamps de inicio y fin

---

## Que mejoras se implementarian en un entorno productivo

1. **Persistencia real:** Reemplazar InMemory por SQL Server o PostgreSQL. El cambio es transparente gracias a la abstraccion del repositorio

2. **Message Broker:** Reemplazar `Channel<Guid>` por RabbitMQ o Azure Service Bus para garantizar durabilidad de mensajes y procesamiento distribuido

3. **Retry con Polly:** Implementar politicas de reintento configurables por paso con exponential backoff para fallos transitorios

4. **Idempotencia:** Agregar mecanismo de idempotencia para evitar procesamiento duplicado ante reintentos o mensajes repetidos

5. **Outbox Pattern:** Garantizar consistencia entre la persistencia y la publicacion de eventos

6. **Observabilidad:** Integrar OpenTelemetry para tracing distribuido, logs estructurados

7. **Health Checks:** Endpoints para monitorear el estado del worker y la conectividad con servicios externos

8. **Rate Limiting y Autenticación:** Proteger la API con limites de tasa y JWT/API Keys

---

## API

### Crear solicitud

```http
POST /api/creditapplications
{
    "customerName": "Juan PErez",
    "customerId": "CUST-001",
    "requestedAmount": 50000
}
```
→ `202 Accepted` con `{ "id": "guid", "status": "Created" }`

### Consultar estado

```http
GET /api/creditapplications/{id}
```
→ `200 OK` con solicitud, estado actual y detalle de cada paso ejecutado

### Listar solicitudes

```http
GET /api/creditapplications
```
→ `200 OK` con listado de todas las solicitudes

### Reintentar solicitud

```http
POST /api/creditapplications/{id}/retry
```
→ `202 Accepted` si la solicitud esta en estado de fallo
→ `409 Conflict` si la solicitud no es retriable

## Arquitectura

```
CreditSystem.Domain          → Entidades, enums, interfaces (sin dependencias externas)
CreditSystem.Application     → Casos de uso, orquestador, steps, eventos
CreditSystem.Infrastructure  → Persistencia (EF Core), Background Worker
CreditSystem.Api             → Controllers, configuración, punto de entrada
```
