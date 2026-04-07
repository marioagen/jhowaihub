using WoopiAiHub.Api.Attributes;
using WoopiAiHub.Application.DependencyInjection;
using WoopiAiHub.Domain.DependencyInjection;
using WoopiAiHub.Repository.DependencyInjection;
using WoopiAiHub.Repository.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using WoopiAiHub.Api.Exceptions;
using System.Text.Json.Serialization;
using WoopiAiHub.Infrastructure.DependencyInjection;
using WoopiAiHub.Api.Hubs;
using WoopiAiHub.Application.Services.Hubs;
using WoopiAiHub.Domain.Interfaces.Hubs;
using WoopiAiHub.Application.Utils;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

builder.Services.AddExternalApi(config);
builder.Services.AddRepository(config);
builder.Services.AddValidation();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
    c.OperationFilter<SwaggerCustomHeader>();
});

var allowedOrigin = config["CORS"]?.Trim();
if (!string.IsNullOrWhiteSpace(allowedOrigin))
{
    builder.Services.AddCors(p => p.AddPolicy("manager", policy =>
    {
        policy
            .WithOrigins(allowedOrigin) 
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); 
    }));
}
else {
    throw new InvalidOperationException("CORS origin não está configurado. Verifique a chave 'CORS' no appsettings ou variável de ambiente.");
}

builder.Services.AddInfrastructure(config);
builder.Services.AddApplication();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IHubNotifier, HubNotifier>();
builder.Services.AddSingleton<IConnectionMappingService, ConnectionMappingService>();
builder.Services.Configure<ChatCompletionSettings>(builder.Configuration.GetSection("ChatCompletionSettings"));
builder.Services.Configure<ResponseOpenAiSettings>(builder.Configuration.GetSection("ResponseOpenAiSettings"));
builder.Services.AddSignalR();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});
builder.Services.AddHealthChecks();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "appsettings.Development.json");
}

app.UseCors("manager");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseMultiTenantExtension();
app.MapControllers();
app.UseExceptionHandler();

app.MapHealthChecks("/healthz");

app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
