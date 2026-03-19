using Microsoft.EntityFrameworkCore;
using UnlimitedRPG.Api.Engine;
using UnlimitedRPG.Api.Hubs;
using UnlimitedRPG.Api.Services;
using UnlimitedRPG.Core.Interfaces;
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
    .AddScoped<INotificationService,    SignalRNotificationService>()
    .AddScoped<IGameEngine,             GameEngine>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => "ok").WithName("Health");
app.MapControllers();
app.MapHub<ContentHub>("/hubs/content");

app.Run();
