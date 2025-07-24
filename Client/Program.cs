using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AquaSolution.Client;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using AquaService.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
//-------------------------CustomConfig-------------------------------

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthStateProvider>();

//builder.Services.AddHttpClient("ServerAPI", client =>
//{
//    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
//});
var baseUri = builder.HostEnvironment.BaseAddress;
#if DEBUG
// Chạy local, không thêm gì
builder.Services.AddHttpClient("ServerAPI", client =>
{
    client.BaseAddress = new Uri(baseUri);
});
#else
// Chạy khi publish, thêm thư mục host
builder.Services.AddHttpClient("ServerAPI", client =>
{
    client.BaseAddress = new Uri($"{baseUri}AquaSolution/");
});
#endif

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerAPI"));
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddAntDesign();

//--------------------------------------------------------------------
await builder.Build().RunAsync();
