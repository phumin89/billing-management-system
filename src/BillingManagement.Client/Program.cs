using BillingManagement.Client;
using BillingManagement.Client.OwnerCompanyProfiles;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var clientBaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
var apiBaseAddress = clientBaseAddress.Port == 5080
    ? new Uri("http://localhost:5081/")
    : new Uri("http://localhost:5170/");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = apiBaseAddress });
builder.Services.AddScoped<OwnerCompanyProfileClient>();

await builder.Build().RunAsync();
