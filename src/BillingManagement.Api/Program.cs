using BillingManagement.Application;
using BillingManagement.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddBillingManagementApplication();
builder.Services.AddBillingManagementInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy =>
        policy
            .WithOrigins("http://localhost:5156", "https://localhost:7004")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("Client");

app.UseAuthorization();

app.MapControllers();

app.Run();
