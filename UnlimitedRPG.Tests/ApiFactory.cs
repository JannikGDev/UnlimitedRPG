using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UnlimitedRPG.Database;

namespace UnlimitedRPG.Tests;

/// <summary>
/// Spins up the full API in-process for integration tests.
/// Each factory instance gets its own isolated in-memory database so test classes
/// don't interfere with each other's seeded data.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var uniqueDbName = $"TestDb_{Guid.NewGuid()}";

        builder.ConfigureServices(services =>
        {
            // Remove the shared "MemoryDatabase" registration and replace with an isolated one
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<RPGContext>)
                         || d.ServiceType == typeof(IDbContextFactory<RPGContext>))
                .ToList();
            foreach (var d in toRemove) services.Remove(d);

            services.AddDbContextFactory<RPGContext>(options =>
                options.UseInMemoryDatabase(uniqueDbName));
        });
    }
}
