using System.Text.Json;
using System.Text.Json.Serialization;
using RgAi.Backend.Models;
using RgAi.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("Llm", client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddSingleton(BackendSettings.Load());
builder.Services.AddSingleton(new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true
});

builder.Services.AddSingleton<SessionStorageService>();
builder.Services.AddSingleton<ModelSelectionService>(provider =>
{
    var settings = provider.GetRequiredService<BackendSettings>();
    return new ModelSelectionService(settings.DefaultLlmModel);
});
builder.Services.AddScoped<LlmService>();
builder.Services.AddScoped<MemoryService>();
builder.Services.AddScoped<InferenceService>();
builder.Services.AddScoped<CodeIndexingService>();

var app = builder.Build();

app.UseCors();
app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
