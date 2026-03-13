var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapGet("/health", () => "ok").WithName("Health");

app.Run();