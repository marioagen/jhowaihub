var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:7047");

var app = builder.Build();

app.MapGet("/user/CheckAccessByHub", (string email) => Results.Json(new
{
    hasAccess = true,
    tenants = new[]
    {
        new { name = "local", isDatabaseCreated = true },
    },
}));

app.MapGet("/api/Tenant/{tenantName}", (string tenantName) => Results.Json(new
{
    name = tenantName,
    email = "admin@local.dev",
    databaseName = "WoopiAiHub",
    plan = "standard",
    wtcsIncluded = 1000,
}));

app.MapGet("/api/Tenant/{tenantName}/llm-models", (string tenantName) => Results.Json(new[]
{
    new { id = "gpt-4o", label = "GPT-4o" },
    new { id = "gpt-4.1", label = "GPT-4.1" },
    new { id = "gemini-2.5-pro", label = "Gemini 2.5 Pro" },
    new { id = "gemini-flash-latest", label = "Gemini Flash" },
    new { id = "deepseek-r1", label = "DeepSeek R1" },
    new { id = "claude-sonnet", label = "Claude Sonnet" },
}));

app.MapPost("/user/AssignByHub", () => Results.Json(Guid.NewGuid()));
app.MapGet("/api/Tenant/all/{module}", () => Results.Json(new[]
{
    new { name = "local", isDatabaseCreated = true },
}));

app.Run();
