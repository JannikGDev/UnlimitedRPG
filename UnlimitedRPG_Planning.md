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
    Task EnqueueNarrationAsync(Guid sessionId, int round, CancellationToken ct = default);
}

public interface IContentStore
{
    Task SaveAsync<T>(string id, T entity, CancellationToken ct = default);
    Task<T?> GetAsync<T>(string id, CancellationToken ct = default);
}
```

---

## Stub Implementations (UnlimitedRPG.Stubs)

| Stub | Behavior |
|---|---|
| `StubLlmAdapter` | Returns fixed text after a 200ms fake delay |
| `StubContentOrchestrator` | Calls stub LLM, saves to store, pushes via SignalR |
| `InMemoryContentStore` | ConcurrentDictionary, JSON-serialized values |

---

## Combat Flow

1. Client calls `POST /api/rpg/sessions/{id}/attack`
2. Engine rolls dice deterministically (hit = d20 + AttackBonus vs ArmorClass, damage = 1d6 + DamageBonus)
3. `Enemy.ApplyDamage(damage)` updates HP and status
4. `CombatLog` row created with `Provider = "pending"`
5. `IContentOrchestrator.EnqueueNarrationAsync(...)` called — returns immediately
6. Response returned to client with combat result
7. Orchestrator generates narration async → updates log → pushes `NarrationReady` via SignalR

---

## SignalR Hub

- Route: `/hubs/content`
- Message pushed to client: `NarrationReady(Guid sessionId, int round, string narration)`

---

## DI Registration (Program.cs pattern)

```csharp
builder.Services
    .AddSingleton<IContentStore,        InMemoryContentStore>()
    .AddSingleton<ILlmAdapter,          StubLlmAdapter>()
    .AddSingleton<IContentOrchestrator, StubContentOrchestrator>()
    .AddSignalR();
```

---

## Status

**Done:**
- All domain models
- EF Core DbContext + entity configurations (in-memory)
- Basic worlds CRUD endpoint

**Next:**
- Core interfaces
- Stub implementations
- SignalR hub
- API endpoints: users, characters, enemy templates, sessions, attack
- DI wiring
