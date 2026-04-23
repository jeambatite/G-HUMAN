using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GHumanAPI.Data;
using GHumanAPI.Services;
using Resend;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURACIÓN DE BASE DE DATOS ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("❌ FATAL: La cadena de conexión 'DefaultConnection' está vacía o no fue encontrada.");
    Console.WriteLine("   Asegúrate de que la variable de entorno 'ConnectionStrings__DefaultConnection' esté configurada en Railway.");
}
else
{
    Console.WriteLine("✅ Cadena de conexión cargada correctamente.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// --- 2. CONFIGURACIÓN DE JWT ---
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// --- 3. CONFIGURACIÓN DE CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowRailwayFront", policy =>
    {
        policy.WithOrigins(
                "https://beautiful-adaptation-production-7e38.up.railway.app",
                "http://localhost:4200"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// --- 4. INYECCIÓN DE DEPENDENCIAS ---
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IPermisoService, PermisoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<INominaService, NominaService>();
builder.Services.AddHostedService<GHumanAPI.BackgroundServices.NominaBackgroundService>();
builder.Services.AddScoped<IReclutamientoService, ReclutamientoService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IResend, ResendClient>();
builder.Services.Configure<ResendClientOptions>(options =>
{
    options.ApiToken = Environment.GetEnvironmentVariable("Resend__ApiKey")
        ?? builder.Configuration["Resend:ApiKey"]!;
});

var app = builder.Build();

// --- 5. AUTO-MIGRACIÓN ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        Console.WriteLine("✅ Base de datos sincronizada.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error al sincronizar: {ex.Message}");
    }
}

// --- 6. MIDDLEWARE PIPELINE ---

// --- 6. MIDDLEWARE PIPELINE ---

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();

// 1. CORS antes de Routing para interceptar preflight OPTIONS correctamente
app.UseCors("AllowRailwayFront");

// 2. Enrutamiento después de CORS
app.UseRouting();

// 3. Redirección (Opcional, a veces causa problemas con CORS en Railway, prueba comentarlo si sigue fallando)
// app.UseHttpsRedirection(); 

app.UseStaticFiles();

// 4. Seguridad
app.UseAuthentication();
app.UseAuthorization();

// 5. Mapeo
app.MapControllers();

app.Run();