using Microsoft.EntityFrameworkCore;
using UnlimitedRPG.Api.Engine;
using UnlimitedRPG.Api.Hubs;
using UnlimitedRPG.Api.Services;
using UnlimitedRPG.Core.Interfaces;
using UnlimitedRPG.Core.Model;
using UnlimitedRPG.Database;
using UnlimitedRPG.Stubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "UnlimitedRPG API", Version = "v1" });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});
builder.Services.AddControllers();
builder.Services.AddDbContextFactory<RPGContext>(
    options => options
        .UseInMemoryDatabase("MemoryDatabase")
);

builder.Services
    .AddSingleton<IContentStore,        InMemoryContentStore>()
    .AddSingleton<ILlmAdapter,          StubLlmAdapter>()
    .AddSingleton<IContentOrchestrator, StubContentOrchestrator>()
    .AddSingleton<INotificationService, SignalRNotificationService>()
    .AddScoped<IGameEngine,             GameEngine>();

var app = builder.Build();

// Seed worlds and their enemy templates if not already present
await using (var ctx = await app.Services
    .GetRequiredService<IDbContextFactory<RPGContext>>()
    .CreateDbContextAsync())
{
    if (!ctx.Worlds.Any())
    {
        var darklands  = new World { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "The Darklands" };
        var sunkenVale = new World { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "Sunken Vale"   };

        ctx.Worlds.AddRange(darklands, sunkenVale);
        ctx.EnemyTemplates.AddRange(
            new EnemyTemplate { Name = "Shadow Wraith",  BaseHp = 10, AttackBonus = 2, DamageBonus = 1, ArmorClass = 13, WorldId = darklands.Id,  World = darklands  },
            new EnemyTemplate { Name = "Drowned Knight", BaseHp = 14, AttackBonus = 3, DamageBonus = 2, ArmorClass = 15, WorldId = sunkenVale.Id, World = sunkenVale }
        );
        await ctx.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => "ok").WithName("Health");
app.MapControllers();
app.MapHub<ContentHub>("/hubs/content");

app.Run();

public partial class Program { }
