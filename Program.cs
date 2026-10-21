using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProductApi.Data;
using ProductApi.Middleware;
using ProductApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Services register කරනවා (Spring: @Service auto-scan — මෙතන manual) ──────────

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        // Enums → JSON එකේ string විදිහට ("Open", "High") — number වෙනුවට
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

// EF Core — MySQL (Pomelo provider). Connection string → appsettings.json.
var connectionString = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

// අපේ services — interface → implementation (Spring: @Service beans වගේ)
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// ── 2. JWT Authentication (Spring Security වගේ) ───────────────────────────────────

var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// ── CORS — Angular frontend (port 4200) එකට requests දාන්න දෙනවා ──────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── 3. Swagger (JWT testing support එක්ක) ─────────────────────────────────────────

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HelpDesk API", Version = "v1" });

    // Swagger UI එකේ "Authorize" button එක (JWT token දාන්න)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token එක දාන්න (Bearer prefix එක ඕන නෑ)."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── 4. Middleware pipeline (පිළිවෙළ වැදගත්!) ──────────────────────────────────────

app.UseMiddleware<ErrorHandlingMiddleware>();   // errors එක තැනකින් → ලස්සන JSON

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");   // CORS — Angular එකට කලින් (Spring: CORS filter)

app.UseAuthentication();   // මුලින්: කවුද? (Spring: authentication)
app.UseAuthorization();    // ඊට පස්සේ: මොනවද පුළුවන්? (Spring: authorization)

app.MapControllers();

// seed data (sample products + users) — app එක start වෙද්දී
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();    // helpDesk database + tables හදනවා (නැත්නම්)
    DbSeeder.Seed(db);              // roles + (හිස් නම්) sample data දානවා
}

app.Run();
