using BillingManagement.Client;
using BillingManagement.Client.BillingDocuments;
using BillingManagement.Client.Customers;
using BillingManagement.Client.OwnerCompanyProfiles;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseAddress = builder.HostEnvironment.IsDevelopment()
    ? new Uri("http://localhost:5170/")
    : new Uri(builder.HostEnvironment.BaseAddress);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = apiBaseAddress });
builder.Services.AddScoped<CustomerClient>();
builder.Services.AddScoped<BillingDocumentClient>();
builder.Services.AddScoped<CustomerSessionState>();
builder.Services.AddScoped<OwnerCompanyProfileClient>();

await builder.Build().RunAsync();
