# UnlimitedRPG — Architecture Overview

## Project Goal

A modular RPG framework that uses LLMs to generate dynamic narration at runtime.
The game engine (combat, dice, stats) is fully deterministic — no LLM involvement in game logic.
After the engine resolves an outcome, the AI layer generates narration asynchronously and pushes it to the frontend via SignalR.

---

## Guiding Principles

- **Engine is source of truth.** The engine resolves events; the AI layer narrates them. The engine never waits for an LLM.
- **All modules are swappable via DI.** Stubs exist for every interface so the full loop can be tested without a real LLM.
- **Content generation is always async.** Results are pushed via SignalR, never polled.

---

## Stack

| Concern | Choice |
|---|---|
| Runtime | .NET 10 |
| API | ASP.NET Core — REST controllers + SignalR |
| OpenAPI | Microsoft.AspNetCore.OpenApi + Swashbuckle.AspNetCore |
| ORM | EF Core 10, in-memory database |
| Frontend | SvelteKit, TypeScript (strict), Tailwind CSS v4, @microsoft/signalr |
| Type sharing | openapi-typescript (generates src/lib/api.gen.ts from /openapi/v1.json) |

---

## Solution Structure

```
UnlimitedRPG.sln
├── UnlimitedRPG.Core      # Domain models and interfaces. Zero external dependencies.
├── UnlimitedRPG.Database  # EF Core DbContext and entity configurations.
├── UnlimitedRPG.Stubs     # Stub implementations of all Core interfaces.
├── UnlimitedRPG.Api       # ASP.NET Core host. DI wiring. REST + SignalR.
├── UnlimitedRPG.Tests     # xUnit tests.
└── UnlimitedRPG.Frontend  # SvelteKit frontend.
```

**Dependency rules:**
- `Core` → no references
- `Database` → `Core`
- `Stubs` → `Core`
- `Api` → `Core`, `Database`, `Stubs`
- `Tests` → `Core`, `Stubs`

---

## Domain Model (UnlimitedRPG.Core/Model)

| Entity | Key Fields |
|---|---|
| `User` | Id, Username, Email |
| `PlayerCharacter` | Id, Name, Hp, AttackBonus, DamageBonus, ArmorClass, UserId |
| `World` | Id, Name |
| `EnemyTemplate` | Id, Name, BaseHp, AttackBonus, DamageBonus, ArmorClass, WorldId |
| `Enemy` | Id, CurrentHp, Status (Alive/Staggered/Dead), TemplateId |
| `Session` | Id, StartedAt, Status (Active/Completed/Abandoned), WorldId, UserId, PlayerCharacterId |
| `CombatLog` | Id, Round, Hit, Damage, Narration, Provider, Timestamp, SessionId |

**Key methods:**
- `EnemyTemplate.Instantiate()` → creates an `Enemy` with `CurrentHp = BaseHp`
- `Enemy.ApplyDamage(int damage)` → reduces HP, sets Status thresholds (≤0 Dead, ≤3 Staggered)
- `Session.Complete()` / `Session.Abandon()` → transition status

---

## Core Interfaces (UnlimitedRPG.Core/Interfaces)

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

---

## Implementations

### Stubs (UnlimitedRPG.Stubs)

| Stub | Behavior |
|---|---|
| `StubLlmAdapter` | Returns fixed text after a 200ms fake delay |
| `StubContentOrchestrator` | Fire-and-forgets: calls stub LLM, pushes via INotificationService |
| `InMemoryContentStore` | ConcurrentDictionary, JSON-serialized values |

### Api (UnlimitedRPG.Api)

| Class | Purpose |
|---|---|
| `GameEngine` | Implements `IGameEngine` — d20 hit check, 1d6 damage, enemy status transitions |
| `ContentHub` | SignalR hub at `/hubs/content` — exposes `JoinSession(Guid)` |
| `SignalRNotificationService` | Implements `INotificationService` — pushes via `IHubContext<ContentHub>` |

---

## Combat Flow

1. Client calls `POST /api/sessions/{id}/actions`
2. `IGameEngine.Process(state, PlayerAttackInput)` → `ProcessResult(newState, CombatEvent)`
3. `CombatLog` entry created with `Provider = "pending"`, empty narration
4. `IContentOrchestrator.EnqueueNarrationAsync(sessionId, combatEvent)` — fire-and-forget
5. Response returned to client with updated state
6. Orchestrator: LLM generates narration → `INotificationService.SendNarrationAsync` → SignalR pushes `NarrationReady`
7. Frontend receives `NarrationReady(sessionId, round, narration)` → updates log entry in place

---

## SignalR Hub

- Route: `/hubs/content`
- Client invokes: `JoinSession(Guid sessionId)` → joins group
- Server pushes: `NarrationReady(Guid sessionId, int round, string narration)`

---

## DI Registration (Program.cs)

```csharp
builder.Services
    .AddSingleton<IContentStore,        InMemoryContentStore>()
    .AddSingleton<ILlmAdapter,          StubLlmAdapter>()
    .AddSingleton<IContentOrchestrator, StubContentOrchestrator>()
    .AddScoped<INotificationService,    SignalRNotificationService>()
    .AddScoped<IGameEngine,             GameEngine>();

app.MapHub<ContentHub>("/hubs/content");
```

---

## Status

**Done:**
- All domain models + EF Core DbContext (in-memory)
- All Core interfaces
- Stub implementations (StubLlmAdapter, StubContentOrchestrator, InMemoryContentStore)
- GameEngine (deterministic combat resolution)
- SignalR hub (ContentHub) + SignalRNotificationService
- Full DI wiring
- Sessions API (create, get, execute action)
- Worlds API (list worlds)
- Frontend: world selection, session combat UI, SignalR NarrationReady handler

**Next:**
- Replace hardcoded world/enemy data with database-seeded EF Core data
- Enemy counter-attacks (enemy attacks player each round)
- Real LLM adapter (Claude API)
- User + character creation endpoints and frontend
