using Application;
using Infrastructure;
using WebApi.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

app.MapEndpoints(app.MapGroup("/api"));

app.UseOpenApi();

app.ApplyMigrations();

app.UseHttpsRedirection();

app.Run();
