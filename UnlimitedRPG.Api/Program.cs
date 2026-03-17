using Microsoft.EntityFrameworkCore;
using RpgFramework.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("UnlimitedRPG"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => "ok").WithName("Health");

// GeneratedItems CRUD
app.MapGet("/items", async (AppDbContext db) =>
    await db.GeneratedItems.ToListAsync());

app.MapGet("/items/{id:guid}", async (Guid id, AppDbContext db) =>
    await db.GeneratedItems.FindAsync(id) is { } item
        ? Results.Ok(item)
        : Results.NotFound());

app.MapPost("/items", async (GeneratedItem item, AppDbContext db) =>
{
    db.GeneratedItems.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

app.MapDelete("/items/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var item = await db.GeneratedItems.FindAsync(id);
    if (item is null) return Results.NotFound();
    db.GeneratedItems.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
