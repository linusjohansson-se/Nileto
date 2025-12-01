using Scalar.AspNetCore;
using WebApi;
using WebApi.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies();

var app = builder.Build();

app.MapEndpoints(app.MapGroup("/api"));

app.UseOpenApi();

app.UseHttpsRedirection();

app.Run();
