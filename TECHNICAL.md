# UnlimitedRPG — Technical Overview

The project is made out of 2 parts, the backend and frontend.
Backend:  implemented as a modern .Net Web Application that exposes an API
Frontend: implemented in Svelte with Typescript, that uses the API

The backend contains all the business logic, handles persistency (database) and makes call to external service.
The frontend is the user-facing UI running in the browser communicating with the backend

# General Architecture Approach
The project should be modular with cleanly defined interfaces between each module.
The backend is deployed as a monolith, but the internal structure should be highly modular.

# Guiding Principles
- The game engine resolves all outcomes deterministically. The LLM layer only narrates — it never influences game logic, and the engine never waits for it.
- All modules are swappable via DI. Stubs exist for every interface so the full loop can be tested without a real LLM.
- Content generation is always async. Results are pushed via SignalR; never polled.

# Tech Stack

| Concern | Choice |
|---|---|
| Runtime | .NET 10 |
| API | ASP.NET Core — REST controllers + SignalR |
| OpenAPI | Microsoft.AspNetCore.OpenApi + Swashbuckle.AspNetCore |
| ORM | EF Core 10, in-memory database |
| Frontend | SvelteKit, TypeScript (strict), Tailwind CSS v4, @microsoft/signalr |
| Type sharing | openapi-typescript (generates src/lib/api.gen.ts from /openapi/v1.json) |

# Backend Components

- **Core**: Contains the domain model, module interfaces, and DTO objects for inter-module communication. No dependencies on any other component.
- **Database**: Handles all persistency (EF Core DbContext and entity configurations).
- **Api**: Startup project. Holds controllers, DI wiring, SignalR hubs.
- **Stubs**: Stub implementations of all Core interfaces. Allows testing without a real LLM or database.
- **Tests**: xUnit test project.

**Dependency rules:**
- `Core` → no references
- `Database` → `Core`
- `Stubs` → `Core`
- `Api` → `Core`, `Database`, `Stubs`
- `Tests` → `Core`, `Stubs`

# Frontend Component

- SvelteKit application in `UnlimitedRPG.Frontend/`
- Communicates with the backend via REST and SignalR
- TypeScript types are generated from the OpenAPI spec: run openapi-typescript against `/openapi/v1.json` to regenerate `src/lib/api.gen.ts`. Do not hand-edit generated types.

# Domain Model (UnlimitedRPG.Core/Model)

| Entity | Key Fields |
|---|---|
| `User` | Id, Username, Email |
| `PlayerCharacter` | Id, Name, Hp, AttackBonus, DamageBonus, ArmorClass, UserId |
| `World` | Id, Name |
| `EnemyTemplate` | Id, Name, BaseHp, AttackBonus, DamageBonus, ArmorClass, WorldId |
| `Enemy` | Id, CurrentHp, Status (Alive/Staggered/Dead), TemplateId |
| `Session` | Id, StartedAt, Status (Active/Completed/Abandoned), WorldId, UserId, PlayerCharacterId |
| `CombatLog` | Id, Round, Hit, Damage, Narration, Provider, Timestamp, SessionId |

Key methods:
- `EnemyTemplate.Instantiate()` → creates an `Enemy` with `CurrentHp = BaseHp`
- `Enemy.ApplyDamage(int damage)` → reduces HP; thresholds: ≤3 = Staggered, ≤0 = Dead
- `Session.Complete()` / `Session.Abandon()` → transition session status

# Core Interfaces (UnlimitedRPG.Core/Interfaces)

```csharp
public interface ILlmAdapter
{
    Task<string> GenerateTextAsync(string prompt, CancellationToken ct = default);
}

public interface IContentOrchestrator
{
    Task EnqueueNarrationAsync(Guid sessionId, CombatEvent combatEvent, CancellationToken ct = default);
}

public interface IContentStore
{
    Task SaveAsync<T>(string id, T entity, CancellationToken ct = default);
    Task<T?> GetAsync<T>(string id, CancellationToken ct = default);
}

public interface INotificationService
{
    Task SendNarrationAsync(Guid sessionId, int round, string narration, CancellationToken ct = default);
}

public interface IGameEngine
{
    ProcessResult Process(GameState state, IInput input);
}
```

# Implementations

## Stubs (UnlimitedRPG.Stubs)

| Class | Behavior |
|---|---|
| `StubLlmAdapter` | Returns fixed narration text after a 200ms fake delay |
| `StubContentOrchestrator` | Fire-and-forgets: calls LLM adapter, pushes result via INotificationService |
| `InMemoryContentStore` | ConcurrentDictionary with JSON-serialized values |

## Api (UnlimitedRPG.Api)

| Class | Purpose |
|---|---|
| `GameEngine` | Implements `IGameEngine` — d20 hit check, 1d6 damage, enemy status transitions |
| `ContentHub` | SignalR hub at `/hubs/content` — exposes `JoinSession(Guid)` |
| `SignalRNotificationService` | Implements `INotificationService` — pushes via `IHubContext<ContentHub>` |

# Combat Flow

1. Client calls `POST /api/sessions/{id}/actions`
2. `IGameEngine.Process(state, PlayerAttackInput)` → `ProcessResult(newState, CombatEvent)`
3. `CombatLog` entry created with `Provider = "pending"`, empty narration
4. `IContentOrchestrator.EnqueueNarrationAsync(sessionId, combatEvent)` — fire-and-forget
5. Response returned to client with updated state
6. Orchestrator: LLM generates narration → `INotificationService.SendNarrationAsync` → SignalR pushes `NarrationReady`
7. Frontend receives `NarrationReady(sessionId, round, narration)` → updates log entry in place

# SignalR Hub

- Route: `/hubs/content`
- Client invokes: `JoinSession(Guid sessionId)` → joins group
- Server pushes: `NarrationReady(Guid sessionId, int round, string narration)`

# DI Registration (Program.cs)

```csharp
builder.Services
    .AddSingleton<IContentStore,        InMemoryContentStore>()
    .AddSingleton<ILlmAdapter,          StubLlmAdapter>()
    .AddSingleton<IContentOrchestrator, StubContentOrchestrator>()
    .AddScoped<INotificationService,    SignalRNotificationService>()
    .AddScoped<IGameEngine,             GameEngine>();

app.MapHub<ContentHub>("/hubs/content");
```
