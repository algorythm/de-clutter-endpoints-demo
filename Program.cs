using CodeDemo;
using CodeDemo.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Store>();

var app = builder.Build();

app.MapWeatherEndpoints();
app.MapUserEndpoints();
app.MapProductEndpoints();
app.MapOrderEndpoints();
app.MapTodoEndpoints();

app.Run();
