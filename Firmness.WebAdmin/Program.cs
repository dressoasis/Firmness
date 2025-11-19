using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Firmness.Domain.Entities;
using Firmness.Infrastructure.Services.Identity;
using Firmness.Infrastructure.Data;
using Firmness.Infrastructure.Data.Seed;
using Firmness.Infrastructure.Repositories;
using Firmness.Application.Interfaces;
using Firmness.Application.Services;
using Firmness.Domain.Interfaces;
using Firmness.Application.Mappings;
using Firmness.WebAdmin.ApiClients;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
Console.WriteLine(builder.Configuration.GetConnectionString("DefaultConnection"));


// --------------------------
// 2. IDENTITY CONFIG
// --------------------------
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IAuthService, AuthService>();


// --------------------------
// 3. AUTOMAPPER
// --------------------------
builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());


// --------------------------
// 4. REPOSITORIES
// --------------------------
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));


// --------------------------
// 5. SERVICES
// --------------------------
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();


// --------------------------
// 6. MVC CONTROLLERS
// --------------------------
builder.Services.AddControllersWithViews();


// --------------------------
// 7. API CLIENTS
// --------------------------
var apiBase = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5264/api/";

builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBase);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    return handler;
});

builder.Services.AddHttpClient<ICategoryApiClient, CategoryApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBase);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    return handler;
});

// --------------------------
// BUILD
// --------------------------
var app = builder.Build();


// --------------------------
// HTTP PIPELINE
// --------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// --------------------------
// SEEDING + CONNECTION TEST
// --------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        db.Database.OpenConnection();
        Console.WriteLine("✅ Successful connection to PostgreSQL Railway");
        db.Database.CloseConnection();
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error connecting to PostgreSQL:");
        Console.WriteLine(ex.Message);
    }

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

    await IdentitySeeder.SeedAsync(roleManager, userManager);
    await AdminSeed.SeedAdminsAsync(userManager, roleManager);
}

app.Run();
