using Microsoft.EntityFrameworkCore;
using UnlimitedRPG.Api.Hubs;
using UnlimitedRPG.Core.Interfaces;
using UnlimitedRPG.Database;
using UnlimitedRPG.TextGenerator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<RPGContext>(
    options => options.UseInMemoryDatabase("RPGDatabase"));

builder.Services.AddHttpClient<ITextGenerator, LLMTextGenerator>();

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => "ok").WithName("Health");
app.MapControllers();
app.MapHub<ContentHub>("/hubs/content");

app.Run();

public partial class Program { }
