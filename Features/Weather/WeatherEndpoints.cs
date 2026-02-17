namespace CodeDemo.Features.Weather;

public static class WeatherEndpoints
{
    public static void MapWeatherEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/weather");

        group.MapGet("/forecast", (Store store) =>
        {
            var forecast = Enumerable.Range(1, 7).Select(index =>
                new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    store.WeatherSummaries[Random.Shared.Next(store.WeatherSummaries.Length)]
                )).ToArray();
            return Results.Ok(forecast);
        });

        group.MapGet("/forecast/{days:int}", (int days, Store store) =>
        {
            if (days < 1 || days > 30)
                return Results.BadRequest("Days must be between 1 and 30.");

            var forecast = Enumerable.Range(1, days).Select(index =>
                new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    store.WeatherSummaries[Random.Shared.Next(store.WeatherSummaries.Length)]
                )).ToArray();
            return Results.Ok(forecast);
        });

        group.MapGet("/current", (Store store) =>
        {
            var current = new
            {
                Temperature = Random.Shared.Next(-10, 40),
                Summary = store.WeatherSummaries[Random.Shared.Next(store.WeatherSummaries.Length)],
                Humidity = Random.Shared.Next(20, 100),
                WindSpeed = Random.Shared.Next(0, 50),
                Timestamp = DateTime.UtcNow,
            };
            return Results.Ok(current);
        });
    }
}
