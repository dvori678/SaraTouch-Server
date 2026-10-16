using Microsoft.EntityFrameworkCore;
using SaraTouch.Data;
using SaraTouch.Core.Interfaces.Repositories;
using SaraTouch.Data.Repositories;
using SaraTouch.Core.Interfaces.Infrastructure;
using SaraTouch.Data.Seed;
using SaraTouch.Service.Infrastructure;
using SaraTouch.Service.Mapping;
using SaraTouch.Core.Interfaces.Services;
using SaraTouch.Service.Services;
using SaraTouch.Core.Settings;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SaraTouch.API.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using SaraTouch.API.Models;
using NLog.Web;
using SaraTouch.API.Middleware;
var builder = WebApplication.CreateBuilder(args);
// NLog מתחבר מתחת ל-ILogger הרגיל. הקוד בשכבות לא משתנה בכלל
builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
// Repositories: Scoped, כמו ה-DbContext, כלומר מופע אחד לכל בקשה
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITreatmentRepository, TreatmentRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
// Infrastructure
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddSingleton<IClock, SystemClock>();   // אין לו state, אז Singleton בטוח
builder.Services.AddScoped<DbInitializer>();
// AutoMapper: סורק את ה-Profile ונרשם ל-DI
builder.Services.AddAutoMapper(cfg =>
{
    cfg.LicenseKey = builder.Configuration["AutoMapper:LicenseKey"];
}, typeof(MappingProfile));

// Services
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ITreatmentService, TreatmentService>();
builder.Services.AddScoped<ITimeSlotService, TimeSlotService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
// JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
          ?? throw new InvalidOperationException("Missing Jwt configuration");
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters. Set it in User Secrets.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

// Singleton: לא מחזיק שום דבר Scoped, רק הגדרות ושעון
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
// הגדרות עסקיות מ-appsettings.json דרך IOptions
builder.Services.Configure<BookingSettings>(
    builder.Configuration.GetSection(BookingSettings.SectionName));

// enums יוצאים ב-JSON כטקסט ("Admin") ולא כמספר (1)
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// ModelState לא תקין → 400 בפורמט ErrorResponse האחיד
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value!.Errors.Count > 0)
            .ToDictionary(
                e => e.Key,
                e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());

        return new BadRequestObjectResult(new ErrorResponse
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Message = "הבקשה מכילה נתונים לא תקינים",
            CorrelationId = context.HttpContext.TraceIdentifier,
            Errors = errors
        });
    };
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// Swagger עם כפתור Authorize ל-JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SaraTouch API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "הדביקי כאן רק את ה-token, בלי המילה Bearer"
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

// migrations ו-seed אוטומטיים בעליית השרת, לפני שהשרת מתחיל לקבל בקשות
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await initializer.InitializeAsync();
}

// סדר ה-pipeline חשוב:
app.UseMiddleware<ExceptionHandlingMiddleware>();  // 1. ראשון: עוטף את כל השאר ותופס כל שגיאה
app.UseMiddleware<CorrelationIdMiddleware>();      // 2. מייצר מזהה לבקשה
app.UseMiddleware<RequestLoggingMiddleware>();     // 3. רושם כל בקשה, כבר עם המזהה

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();   // authentication לפני authorization
app.UseAuthorization();
app.MapControllers();

app.Run();