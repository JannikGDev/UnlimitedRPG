using Microsoft.EntityFrameworkCore;
using UnlimitedRPG.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddDbContextFactory<RPGContext>(
    options => options
        .UseInMemoryDatabase("MemoryDatabase")
);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => "ok").WithName("Health");
app.MapControllers();

app.Run();
