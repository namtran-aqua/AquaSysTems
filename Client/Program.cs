//using AquaService.Client.Services;
//using AquaSolution.Client;
//using AquaSolution.Client.Common;
//using Blazored.SessionStorage;
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.AspNetCore.Components.Web;
//using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

//var builder = WebAssemblyHostBuilder.CreateDefault(args);
//builder.RootComponents.Add<App>("#app");
//builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
////-------------------------CustomConfig-------------------------------
//builder.Services.AddTransient<AuthMessageHandler>();
////builder.Services.AddBlazored_sessionStorage();
//builder.Services.AddBlazoredSessionStorage();
//builder.Services.AddAuthorizationCore();
//builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthStateProvider>();
//builder.Services.AddScoped(sp =>
//    new HttpClient
//    {
//        BaseAddress = new Uri($"{builder.HostEnvironment.BaseAddress}ITSM/")
//    });
//builder.Services.AddHttpClient("ServerAPI", client =>
//{
//    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
//}).AddHttpMessageHandler<AuthMessageHandler>();

////builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerAPI"));
//builder.Services.AddAuthorizationCore();
//builder.Services.AddScoped<CustomAuthenticationStateProvider>();
//builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthenticationStateProvider>());
//builder.Services.AddAntDesign();
//builder.Services.AddScoped<PermissionService>();
////--------------------------------------------------------------------
//await builder.Build().RunAsync();
using AquaSolution.Client;
using AquaSolution.Client.Common;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ================= HTTP CLIENT =================
builder.Services.AddTransient<AuthMessageHandler>();

builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();

    return new HttpClient
    {
        // Ví dụ: http://server14/ITSM/
        BaseAddress = new Uri($"{navigationManager.BaseUri}")
    };
});

// ================= AUTH =================
builder.Services.AddBlazoredSessionStorage();

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthenticationStateProvider>());

// ================= UI + SERVICE =================
builder.Services.AddAntDesign();
builder.Services.AddScoped<PermissionService>();

await builder.Build().RunAsync();
