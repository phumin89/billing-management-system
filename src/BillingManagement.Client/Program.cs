using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BillingManagement.Client;
using BillingManagement.Client.OwnerCompanyProfiles;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5170/") });
builder.Services.AddScoped<OwnerCompanyProfileClient>();

await builder.Build().RunAsync();
