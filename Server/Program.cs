////---------------------------------------------------------------
//using AquaSolution.Data.Connection;
//using AquaSolution.Data.Data;
//using AquaSolution.Server;
//using AquaSolution.Server.SignalR;

//using Microsoft.AspNetCore.Authentication.JwtBearer;

//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();
//builder.Services.AddRazorPages();
////-----------------------CustomConfig---------------------------------
//builder.Services.AddDbContext<AquaDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

////-------------------------------------------------------------------------------------------

//builder.Services.AddDbContext<ePADContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("ePAD")));

////-------------------------------------------------------------------------------------------
//builder.Services.AddControllers();

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = false,
//            ValidateAudience = false,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes("VerySecretKey12345"))
//        };

//        options.Events = new JwtBearerEvents
//        {
//            OnAuthenticationFailed = context =>
//            {
//                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
//                return Task.CompletedTask;
//            }
//        };
//    });

//builder.Services.AddAppServices();
//builder.Services.AddAuthorization();
//builder.Services.AddEndpointsApiExplorer();
////builder.Services.AddSwaggerGen();

//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
//    {
//        Title = "AquaSolution API",
//        Version = "v1"
//    });
//});

//builder.Services.AddSignalR();
////--------------------------------------------------------------------

//var app = builder.Build();

//app.UsePathBase("/AquaSolution"); // giữ base path

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
//    //db.Database.Migrate();
//    //DbSeeder.SeedData(db);
//}

//if (app.Environment.IsDevelopment())
//{
//    app.UseWebAssemblyDebugging();
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
//else
//{
//    app.UseExceptionHandler("/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();

//app.UseBlazorFrameworkFiles(); 
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthentication();
//app.UseAuthorization();
////app.UseSwagger();

////app.UseSwaggerUI(c =>
////{
////    c.SwaggerEndpoint("v1/swagger.json", "AquaSolution API v1");
////    c.RoutePrefix = "swagger";
////});

//app.MapRazorPages();
//app.MapControllers();
//app.MapHub<SignalrHub>("/signalrhub");

//// Map fallback với base path
//app.MapFallbackToFile("index.html");

//app.Run();

//----------------------------------------------------------------------------------------------------------------------------------------------
using AquaSolution.Data.Connection;
using AquaSolution.Data.Data;
using AquaSolution.Server;
using AquaSolution.Server.Services.Common.Hangfire;
using AquaSolution.Server.Services.Hangfire;
using AquaSolution.Server.SignalR;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===================== SERVICES =====================

// MVC + API
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// DbContext
builder.Services.AddDbContext<AquaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<ePADContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ePAD")));

// JWT Auth (GIỮ NHƯNG SWAGGER KHÔNG DÙNG)
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
builder.Services.AddAppServices();

// ===================== SWAGGER =====================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ITSM API",
        Version = "v1"
    });

    // ❌ KHÔNG add security → Swagger không cần login
});

// SignalR
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


// ===================== BUILD =====================
var app = builder.Build();
//app.UsePathBase("/AquaSolution");
//app.UsePathBase("/ITSM");

// 👉 HANGFIRE DASHBOARD
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[]
    {
        new HangfireAllowAllFilter()
    }
});

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStarted.Register(() =>
{
    RecurringJob.AddOrUpdate<DailyJobService>(
        "daily-job-9am",
        job => job.RunDailyAsync(),
        Cron.Daily(9, 0),
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
        }
    );
});

// ===================== MIDDLEWARE =====================

// ===================== SWAGGER =====================
// ⚠️ QUAN TRỌNG: SwaggerEndpoint PHẢI LÀ RELATIVE PATH
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "swagger";         
    c.SwaggerEndpoint(
        "v1/swagger.json",
        "ITSM API v1"
    );
});

// ===================== ENV =====================
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

// Auth (API vẫn dùng, Swagger thì không)
app.UseAuthentication();
app.UseAuthorization();

// ===================== MAP =====================

app.MapRazorPages();
app.MapControllers();
app.MapHub<SignalrHub>("/signalrhub");

// Blazor fallback (PHẢI CUỐI CÙNG)
app.MapFallbackToFile("index.html");

app.Run();

