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
var builder = WebApplication.CreateBuilder(args);
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
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// migrations ו-seed אוטומטיים בעליית השרת
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await initializer.InitializeAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
