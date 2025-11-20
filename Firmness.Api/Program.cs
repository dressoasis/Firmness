using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DotNetEnv;

using System.Net;
using System.Security.Claims;
using Firmness.Domain.Entities;
using Firmness.Domain.Interfaces;
using Firmness.Infrastructure.Data;
using Firmness.Infrastructure.Repositories;
using Firmness.Application.Interfaces;
using Firmness.Application.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================
// CARGAR VARIABLES .ENV
// ==========================
Env.Load();

var host = Environment.GetEnvironmentVariable("DB_HOST");
var port = Environment.GetEnvironmentVariable("DB_PORT");
var user = Environment.GetEnvironmentVariable("DB_USER");
var pass = Environment.GetEnvironmentVariable("DB_PASS");
var name = Environment.GetEnvironmentVariable("DB_NAME");

var connectionString = $"Host={host};Port={port};Database={name};Username={user};Password={pass};";

// ==========================
// CONFIGURAR POSTGRES
// ==========================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// ==========================
// CONFIGURAR IDENTITY
// ==========================
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ==========================
// CONFIGURAR CORS
// ==========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ==========================
// INYECCIÓN DE DEPENDENCIAS
// ==========================
builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// ==========================
// CONFIGURAR CONTROLLERS
// ==========================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// ==========================
// CONFIGURAR JWT (HS256)
// ==========================
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role
    };

});

// ==========================
// SWAGGER + JWT
// ==========================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Firmness API",
        Version = "v1"
    });

    // 🔐 JWT EN SWAGGER
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer. Ej: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

// ==========================
// BUILD
// ==========================
var app = builder.Build();

// ==========================
// MIDDLEWARE
// ==========================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Firmness API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ==========================
// TEST DE CONEXIÓN
// ==========================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.OpenConnection();
        Console.WriteLine("✅ API conectada a PostgreSQL");
        db.Database.CloseConnection();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
    }
}

app.Run();
