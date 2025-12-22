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
using AquaSolution.Server.SignalR;

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

// JWT Auth (không ảnh hưởng Swagger)
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

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AquaSolution API",
        Version = "v1"
    });

    // KHÔNG cấu hình Security => Swagger không cần đăng nhập
});

// SignalR
builder.Services.AddSignalR();

// ===================== BUILD =====================
var app = builder.Build();

// ===================== MIDDLEWARE =====================

// BasePath (GIỮ NGUYÊN)
app.UsePathBase("/AquaSolution");

// Swagger PHẢI nằm NGOÀI IsDevelopment
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "swagger"; // => /AquaSolution/swagger
    c.SwaggerEndpoint(
        "/AquaSolution/swagger/v1/swagger.json",
        "AquaSolution API v1"
    );
});

// Dev tools
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

// Auth
app.UseAuthentication();
app.UseAuthorization();

// ===================== MAP =====================

app.MapRazorPages();
app.MapControllers();
app.MapHub<SignalrHub>("/signalrhub");

// Blazor fallback (PHẢI CUỐI CÙNG)
app.MapFallbackToFile("index.html");

app.Run();

