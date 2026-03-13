# UnlimitedRPG — Architecture Overview

This document summarises the architecture decisions made during initial planning. 
It is intended to be read by Claude Code to provide context when working on this codebase.

---

## Project Goal

A modular roleplaying framework that uses LLMs and image generation to produce dynamic content (items, NPCs, enemies, world, adventures) at runtime. 
The game engine (rules, dice, combat, stats) is fully deterministic and programmed — no LLM involvement in game logic.

---

## Guiding Principles

- **Game engine is the source of truth.** The engine fires events; the AI layer reacts to them asynchronously. The engine never waits for an LLM response.
- **Every module has a clear interface.** All modules are swappable via dependency injection. No module depends on a concrete implementation of another.
- **Every module has a stub implementation.** Stubs technically fulfil the interface but return the simplest possible fixed output. They exist to validate end-to-end plumbing before any real AI integration is built.
- **Interfaces are defined before implementations.** The contract comes first; both stub and real implementations follow from it.
- **Content generation is always async.** Results are pushed to the frontend via SignalR, never polled. Even stub implementations must resolve asynchronously (with a fake delay) to force correct async handling in the rest of the system.

---

## Solution Structure

```
UnlimitedRPG.sln
├── UnlimitedRPG.Core          # Interfaces and domain models only. Zero external dependencies.
├── UnlimitedRPG.Stubs         # Stub implementations of all Core interfaces.
├── UnlimitedRPG.Api           # ASP.NET Core 9 host. Wires modules via DI. REST + SignalR.
├── UnlimitedRPG.Tests         # xUnit tests. References Core and Stubs.
└── UnlimitedRPG.Frontend      # SvelteKit + TypeScript frontend.
```

### Dependency rules

- `Core` has no references to other projects and no external NuGet dependencies.
- `Stubs` references `Core` only.
- `Api` references `Core` and `Stubs`. Real adapter implementations live here (or in future separate projects).
- `Tests` references `Core` and `Stubs`.
- `Frontend` communicates with `Api` only via HTTP and SignalR.

---

## Backend Stack

| Concern | Choice | Notes |
|---|---|---|
| Runtime | .NET 9 | Target framework `net9.0` |
| API style | Minimal REST + SignalR | REST for CRUD-like operations; SignalR for async content push |
| OpenAPI | `Microsoft.AspNetCore.OpenApi` 9.0.5 | Do NOT use Swashbuckle — incompatible with .NET 9 |
| Real-time | ASP.NET Core SignalR | Included in framework, no separate package needed |
| DI | Built-in `Microsoft.Extensions.DependencyInjection` | Stub/real swap happens in `Program.cs` |

---

## Frontend Stack

| Concern | Choice | Notes |
|---|---|---|
| Framework | SvelteKit (SPA mode) | Svelte 5 with runes (`$state`, `$derived`, `$effect`) |
| Language | TypeScript (strict mode) | Always strict |
| Real-time | `@microsoft/signalr` | Wrapped in a typed Svelte runes store — components never touch the connection directly |
| Styling | Tailwind CSS v4 | Vite plugin, no config file, `@import "tailwindcss"` in app.css |
| Type sharing | `openapi-typescript` | Generates TS types from the .NET OpenAPI spec automatically. Run on build. |
| HTTP | Native fetch or `ky` | No axios |

---

## Architecture Layers

### 1. Frontend (SvelteKit)
Renders world, inventory, dialogue, combat. Reads from local Svelte state only. Receives content updates via SignalR. Never calls the game engine directly.

### 2. Backend — Game Engine
Deterministic rules, dice, stats, combat resolution, turn management, inventory. Written in C#. **No LLM calls.** Fires domain events onto the event bus when content generation is needed (e.g. `ChestOpened`, `RoomEntered`, `NpcEncountered`).

### 3. Backend — Event Bus
Decouples the game engine from the AI layer. The engine publishes events; the content orchestrator subscribes. This means the engine has zero knowledge of AI providers.

### 4. Backend — Content API (REST)
Handles synchronous requests: fetch a cached item, fetch a known NPC, query world state. Thin layer over the content store.

### 5. Backend — Content Orchestrator
The most complex piece. Subscribes to engine events, decides what to generate and when, builds prompts via the prompt registry, dispatches jobs to the LLM and image adapters, validates results, persists to the content store, and pushes completion notifications via SignalR. Manages async job lifecycle.

### 6. Backend — AI Adapters
Thin, swappable wrappers around external providers. One interface per concern.

### 7. Storage
- **World State DB** — entity positions, flags, game progress.
- **Content Store** — generated items, NPCs, lore. Starts as in-memory; upgrades to Postgres/SQLite later.
- **Asset Cache** — generated images, keyed by a hash of the generation parameters to avoid regeneration.

---

## Core Interfaces

All in `UnlimitedRPG.Core/Interfaces/`.

```csharp
// LLM text and structured output
public interface ILlmAdapter
{
    Task<string> GenerateTextAsync(string prompt, CancellationToken ct = default);
    Task<T>      GenerateStructuredAsync<T>(string prompt, CancellationToken ct = default);
}

// Image generation
public interface IImageAdapter
{
    Task<ImageResult> GenerateAsync(ImageRequest request, CancellationToken ct = default);
}

// Content generation pipeline entry point
public interface IContentOrchestrator
{
    Task<ContentJob> RequestAsync(ContentTrigger trigger, CancellationToken ct = default);
}

// Persistent content storage
public interface IContentStore
{
    Task    SaveAsync<T>(string id, T entity, CancellationToken ct = default);
    Task<T?> GetAsync<T>(string id, CancellationToken ct = default);
}

// Game world state (feeds into prompts)
public interface IWorldState
{
    WorldContext GetContext(string scope);
}

// Prompt template management
public interface IPromptRegistry
{
    string GetTemplate(string entityType);
    string Render(string template, WorldContext context);
}
```

---

## Core Domain Models

All in `UnlimitedRPG.Core/Models/`. All are `record` types — immutable, structural equality.

```csharp
// Minimal world context — intentionally simple for v1
public record WorldContext(
    string Biome,        // "forest" | "dungeon" | "city"
    int    PlayerLevel,
    string Tone          // "dark" | "whimsical" | "gritty"
);

public record ContentTrigger(
    string       EntityType,  // "item" | "npc" | "enemy"
    string       TriggerId,   // e.g. "chest_opened"
    WorldContext Context
);

public enum JobStatus { Queued, Processing, Complete, Failed }

public record ContentJob(
    Guid      JobId,
    string    EntityType,
    JobStatus Status
);

public record ImageResult(string Url, string Prompt, string Provider);
```

---

## Stub Implementations

All in `UnlimitedRPG.Stubs/`. Purpose: prove end-to-end plumbing works before any real AI is integrated.

| Stub | Behaviour |
|---|---|
| `StubLlmAdapter` | `GenerateTextAsync` returns `"Some text."`. `GenerateStructuredAsync<T>` returns `Activator.CreateInstance<T>()` (all defaults). |
| `StubImageAdapter` | Returns a fixed placeholder URL (`https://placehold.co/512x512`). |
| `StubContentOrchestrator` | Returns a `ContentJob` instantly, then after a **500ms fake delay** pushes a completion via SignalR. The delay is intentional — it forces the frontend to handle the loading state correctly. |
| `InMemoryContentStore` | `ConcurrentDictionary<string, object>`. Resets on restart. This is a legitimate v1 implementation, not just a stub. |
| `StubWorldState` | Always returns `new WorldContext("forest", 1, "whimsical")`. |
| `StubPromptRegistry` | Returns `"Generate a {EntityType} for a level {PlayerLevel} player in a {Biome} setting with a {Tone} tone."` for every entity type. |

---

## DI Registration Pattern

All module wiring happens in `UnlimitedRPG.Api/Program.cs`. This is the single seam where stubs are swapped for real implementations, one module at a time.

```csharp
var useStubs = builder.Configuration["UseStubs"] == "true";

builder.Services
    .AddSingleton<IContentStore,  InMemoryContentStore>()   // real enough for v1
    .AddSingleton<IWorldState,    StubWorldState>()
    .AddSingleton<IPromptRegistry, StubPromptRegistry>()
    .AddSingleton<ILlmAdapter,    useStubs ? typeof(StubLlmAdapter)    : typeof(ClaudeAdapter))
    .AddSingleton<IImageAdapter,  useStubs ? typeof(StubImageAdapter)  : typeof(StabilityAdapter))
    .AddSingleton<IContentOrchestrator, StubContentOrchestrator>();
```

Set `"UseStubs": "true"` in `appsettings.Development.json` to run fully on stubs.

---

## SignalR Content Push

The frontend subscribes to `/hubs/content` on startup. When a content job completes, the backend pushes a `ContentReady` message with the job ID and generated entity. The frontend resolves the pending job and updates state.

Frontend pattern (Svelte 5 runes):

```typescript
// contentConnection.svelte.ts
import * as signalR from '@microsoft/signalr'
import type { ContentJob, GeneratedEntity } from './api.gen'  // from openapi-typescript

function createContentStore() {
  let pendingJobs = $state<Record<string, ContentJob>>({})
  let connected   = $state(false)

  const hub = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/content')
    .withAutomaticReconnect()
    .build()

  hub.on('ContentReady', (jobId: string, entity: GeneratedEntity) => {
    delete pendingJobs[jobId]
    onContentReady?.(entity)
  })

  let onContentReady: ((e: GeneratedEntity) => void) | undefined

  return {
    get connected()   { return connected },
    get pendingJobs() { return pendingJobs },
    onContentReady(cb: (e: GeneratedEntity) => void) { onContentReady = cb },
    connect: () => hub.start().then(() => connected = true)
  }
}

export const contentStore = createContentStore()
```

---

## Type Sharing (Backend → Frontend)

1. .NET OpenAPI spec is generated automatically at `/openapi/v1.json` when running in development.
2. `openapi-typescript` reads the spec and generates `src/lib/api.gen.ts`.
3. All frontend code imports types from `api.gen.ts` — no manual type duplication.

Add to `package.json`:
``json
"scripts": {
  "generate:types": "openapi-typescript http://localhost:5001/openapi/v1.json -o src/lib/api.gen.ts"
}
``

Run `npm run generate:types` after any backend model changes.

---

## What Has Been Built So Far

- [x] Solution and project structure created
- [x] `UnlimitedRPG.Core` — domain models (`WorldContext`, `ContentTrigger`, `ContentJob`) and interfaces (`ILlmAdapter`, `IContentOrchestrator`, `IWorldState`)
- [x] `UnlimitedRPG.Api` — compiles cleanly on .NET 9 with `Microsoft.AspNetCore.OpenApi` 9.0.5
- [ ] Remaining Core interfaces (`IImageAdapter`, `IContentStore`, `IPromptRegistry`)
- [ ] Stub implementations in `UnlimitedRPG.Stubs`
- [ ] SignalR hub wired in `Api`
- [ ] DI registration in `Program.cs`
- [ ] Frontend scaffolded and connected to SignalR
- [ ] End-to-end smoke test: trigger → orchestrator → SignalR → frontend

---

## Decisions Still Open

- Specific LLM provider for the first real `ILlmAdapter` implementation (Claude API is the likely first target)
- Image generation provider for the first real `IImageAdapter` implementation
- Whether `WorldContext` needs expansion beyond the v1 three fields before real content looks acceptable
- Persistent storage choice when `InMemoryContentStore` is replaced (SQLite for simplicity, Postgres for production)