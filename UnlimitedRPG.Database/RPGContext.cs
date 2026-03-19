using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UnlimitedRPG.Core.Model;

namespace UnlimitedRPG.Database;

public class RPGContext(DbContextOptions<RPGContext> options) : DbContext(options)
{
    public DbSet<Session> Sessions { get; set; }
    public DbSet<PlayerCharacter> PlayerCharacters { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<World> Worlds { get; set; }
    public DbSet<CombatLog> CombatLogs { get; set; }
    public DbSet<Enemy> Enemy { get; set; }
    public DbSet<EnemyTemplate> EnemyTemplates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assemblyWithConfigurations = GetType().Assembly;
        modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfigurations);
    }
}