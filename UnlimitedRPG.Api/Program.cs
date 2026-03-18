using Microsoft.EntityFrameworkCore;
using UnlimitedRPG.Database;

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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => "ok").WithName("Health");
app.MapControllers();

app.Run();
