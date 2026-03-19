using Microsoft.AspNetCore.Mvc.Testing;

namespace UnlimitedRPG.Tests;

/// <summary>
/// Spins up the full API in-process for integration tests.
/// No service overrides needed — the API already uses stubs and an in-memory database.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
}
