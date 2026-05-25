using AquaSolution.Data.Connection;
using AquaSolution.Server;
using AquaSolution.Server.Services.Common.Hangfire;
using AquaSolution.Server.Services.Hangfire;
using AquaSolution.Server.Services.ScrapManagetment.MaterialServices;
using AquaSolution.Server.SignalR;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===================== MVC + API =====================
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// ===================== DATABASE =====================
builder.Services.AddDbContext<AquaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<ePADContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ePAD")));

// ===================== JWT AUTH =====================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("VerySecretKey12345"))
        };
    });
builder.Services.AddAuthorization();

// ===================== APP SERVICES =====================
builder.Services.AddAppServices();

// ===================== SWAGGER =====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ITSM API",
        Version = "v1"
    });
});

// ===================== SIGNALR =====================
builder.Services.AddSignalR();

// ===================== HANGFIRE =====================
builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(
              builder.Configuration.GetConnectionString("DefaultConnection"),
              new SqlServerStorageOptions
              {
                  CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                  SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                  QueuePollInterval = TimeSpan.FromSeconds(15),
                  UseRecommendedIsolationLevel = true,
                  DisableGlobalLocks = true
              });
});
builder.Services.AddHangfireServer();

// ===================== CORS =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ===================== BUILD =====================
var app = builder.Build();

// ===================== HANGFIRE DASHBOARD =====================
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAllowAllFilter() }
});

// ===================== HANGFIRE JOBS =====================
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStarted.Register(() =>
{
    // Job 9h sáng
    RecurringJob.AddOrUpdate<DailyJobService>(
        "daily-job-9am",
        job => job.RunDailyAsync(),
        Cron.Daily(9, 0),
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
        }
    );

    // Job 00:00 kích hoạt weight đã lên lịch
    RecurringJob.AddOrUpdate<MaterialService>(
        "activate-scheduled-weights-midnight",
        job => job.ActivateScheduledWeightsAsync(),
        Cron.Daily(0, 0),
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
        }
    );
});

// ===================== SWAGGER UI =====================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "swagger";
    c.SwaggerEndpoint("v1/swagger.json", "ITSM API v1");
});

// ===================== ENVIRONMENT =====================
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// ===================== MIDDLEWARE =====================
app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// ===================== ENDPOINTS =====================
app.MapRazorPages();
app.MapControllers();
app.MapHub<SignalrHub>("/signalrhub");
app.MapFallbackToFile("index.html"); // Blazor fallback (phải cuối cùng)

app.Run();