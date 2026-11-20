using System.Net;

HttpClient httpClient = new HttpClient();
Dictionary<DateTime, string> cache = new Dictionary<DateTime, string>();
object lockObject = new object();

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5050");
var app = builder.Build();

app.MapGet("/archive/{year}/{month}/{day}/daily_json.js", async (string year, string month, string day) =>
{
    DateTime date = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));

    lock (lockObject)
    {
        if (cache.ContainsKey(date))
        {
            Console.WriteLine("Из кеша: " + date.ToString("dd.MM.yyyy"));
            return Results.Content(cache[date], "application/json");
        }
    }

    HttpResponseMessage response = await httpClient.GetAsync(
        "https://www.cbr-xml-daily.ru/archive/" + year + "/" + month + "/" + day + "/daily_json.js");

    if (response.StatusCode == HttpStatusCode.NotFound)
    {
        Console.WriteLine("404 от cbr: " + date.ToString("dd.MM.yyyy"));
        return Results.NotFound();
    }

    string json = await response.Content.ReadAsStringAsync();

    lock (lockObject)
    {
        cache[date] = json;
    }

    Console.WriteLine("Скачал с cbr: " + date.ToString("dd.MM.yyyy"));
    return Results.Content(json, "application/json");
});

app.Run();