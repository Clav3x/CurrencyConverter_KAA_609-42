using System.Collections.Concurrent;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5050");

builder.Services.AddSingleton(sp => new HttpClient());

var app = builder.Build();

ConcurrentDictionary<DateTime, string?> cache = new();
const int MaxCacheEntries = 200;

app.MapGet("/archive/{year}/{month}/{day}/daily_json.js",
    async (string year, string month, string day, HttpClient httpClient) =>
    {
        DateTime date;
        try { date = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day)); }
        catch (Exception) { return Results.BadRequest("Некорректная дата"); }

        if (cache.TryGetValue(date, out string? json))
        {
            if (json is null)
            {
                Console.WriteLine("Из кеша (данных нет): " + date.ToString("dd.MM.yyyy"));
                return Results.NotFound();
            }
            Console.WriteLine("Из кеша: " + date.ToString("dd.MM.yyyy"));
            return Results.Content(json, "application/json");
        }

        using HttpResponseMessage response = await httpClient.GetAsync(
            "https://www.cbr-xml-daily.ru/archive/" + year + "/" + month + "/" + day + "/daily_json.js");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            cache[date] = null;
            Console.WriteLine("404 от cbr: " + date.ToString("dd.MM.yyyy"));
            return Results.NotFound();
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500)
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);

        if (!response.IsSuccessStatusCode)
            return Results.StatusCode(StatusCodes.Status502BadGateway);

        string body = await response.Content.ReadAsStringAsync();

        if (!body.StartsWith("{") || !body.Contains("\"Valute\""))
            return Results.StatusCode(StatusCodes.Status502BadGateway);

        if (cache.Count >= MaxCacheEntries)
            cache.Clear();
        cache[date] = body;

        Console.WriteLine("Скачал с cbr: " + date.ToString("dd.MM.yyyy"));
        return Results.Content(body, "application/json");
    });

app.Run();