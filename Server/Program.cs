//using AquaSolution.Data.Connection;
//using AquaSolution.Data.Data;
//using AquaSolution.Server;
//using AquaSolution.Server.SignalR;
//using AquaSolution.Shared.Permissions;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Identity;
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

//        // Optional: để thấy lỗi nếu token invalid
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
//builder.Services.AddSwaggerGen();
//builder.Services.AddSignalR();
////--------------------------------------------------------------------
//var app = builder.Build();
//app.UsePathBase("/AquaSolution");
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
//    db.Database.Migrate();
//    //DbSeeder.SeedData(db);
//}
//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseWebAssemblyDebugging();
//}
//else
//{
//    app.UseExceptionHandler("/Error");
//    app.UseHsts();
//}
//app.UseSwagger();
//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("v1/swagger.json", "AquaSolution API v1");
//    c.RoutePrefix = "swagger";
//});

//app.UseAuthentication(); 
//app.UseAuthorization();  
//app.UseHttpsRedirection();

//app.UseBlazorFrameworkFiles();
//app.UseStaticFiles();

//app.UseRouting();

//app.MapRazorPages();
//app.MapControllers();
//app.MapHub<SignalrHub>("/signalrhub");
//app.MapFallbackToFile("index.html");

//app.Run();
////---------------------------------------------------------------
//using AquaSolution.Data.Connection;
//using AquaSolution.Data.Data;
//using AquaSolution.Server;
//using AquaSolution.Server.SignalR;
//using AquaSolution.Shared.Permissions;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Identity;
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

//        // Optional: để thấy lỗi nếu token invalid
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
//builder.Services.AddSwaggerGen();
//builder.Services.AddSignalR();
////--------------------------------------------------------------------
//var app = builder.Build();

//app.UsePathBase("/AquaSolution"); // giữ nguyên path base
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
//    db.Database.Migrate();
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

//app.UseStaticFiles();
//app.UseBlazorFrameworkFiles("/AquaSolution");

//app.UseRouting();

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapRazorPages();
//app.MapControllers();
//app.MapHub<SignalrHub>("/signalrhub");
//app.MapFallbackToFile("index.html");

//app.Run();
//---------------------------------------------------------------
using AquaSolution.Data.Connection;
using AquaSolution.Data.Data;
using AquaSolution.Server;
using AquaSolution.Server.SignalR;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
//-----------------------CustomConfig---------------------------------
builder.Services.AddDbContext<AquaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

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

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAppServices();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
//--------------------------------------------------------------------

var app = builder.Build();

app.UsePathBase("/AquaSolution"); // giữ base path

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
    db.Database.Migrate();
    //DbSeeder.SeedData(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapHub<SignalrHub>("/signalrhub");

// Map fallback với base path
app.MapFallbackToFile("index.html");

app.Run();

