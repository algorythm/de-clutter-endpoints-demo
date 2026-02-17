using CodeDemo;
using CodeDemo.Features.Orders;
using CodeDemo.Features.Products;
using CodeDemo.Features.Todos;
using CodeDemo.Features.Users;
using CodeDemo.Features.Weather;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Store>();

var app = builder.Build();

app.MapWeatherEndpoints();
app.MapUserEndpoints();
app.MapProductEndpoints();
app.MapOrderEndpoints();
app.MapTodoEndpoints();

app.Run();
