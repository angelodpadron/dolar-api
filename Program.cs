using System.Text.Json;
using Microsoft.OpenApi.Models;
using DolarApi.Utils;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "8081";

builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0.0",
        Title = "Dolar API",
        Description = "Minimal Web API hecha con ASP.NET Core para consultar valores económicos en Argentina (Dolar, Reservas, Riesgo País, etc).  \n Los datos son obtenidos de [DolarSi](https://www.dolarsi.com/)",
        Contact = new OpenApiContact
        {
            Name = "Angelo Padron",
            Url = new Uri("mailto:padron891@gmail.com?subject=Dolar API")
        },
        License = new OpenApiLicense
        {
            Name = "GNU GPLv3",
            Url = new Uri("https://www.gnu.org/licenses/gpl-3.0.en.html")
        }

    }
    );
}
);

var app = builder.Build();

app.UseSwagger();

var cotizacionesUrl = builder.Configuration.GetValue<string>("CotizacionesUrl");
var endpoints = await GetEndpointsFor("Common/Endpoints.json");

// dolar

var dolar = app.MapGroup("/dolar");

// generales

dolar.MapGet("/principales", (HttpClient http) => CotizacionDolar.GetValoresPrincipales(http, cotizacionesUrl));

// por tipo

foreach (var tipo in endpoints["TipoDolar"])
{
    dolar.MapGet($"/{tipo.Value}", (HttpClient http) => CotizacionDolar.GetValorDolar(http, cotizacionesUrl, tipo.Key));
}

// oficial por banco

foreach (var banco in endpoints["Bancos"])
{
    dolar.MapGet($"/oficial/{banco.Value}", (HttpClient http) => CotizacionDolar.GetValorOficialBanco(http, cotizacionesUrl, banco.Key));
}

// reservas y circulante

var reservas = app.MapGroup("/reservas");
var circulante = app.MapGroup("/circulante");

reservas.MapGet("/", (HttpClient http) => ReservasYCirculantes.GetRyC(http, cotizacionesUrl, "Reservas"));
circulante.MapGet("/", (HttpClient http) => ReservasYCirculantes.GetRyC(http, cotizacionesUrl, "Circulante"));

// riesgo pais

var riesgoPais = app.MapGroup("/riesgo_pais");

foreach (var pais in endpoints["RiesgoPais"])
{
    riesgoPais.MapGet($"/{pais.Value}", (HttpClient http) => RiesgoPais.GetRiesgoPais(http, cotizacionesUrl, pais.Key));
}

// agro

var agro = app.MapGroup("/agro");

foreach (var producto in endpoints["Agro"])
{
    agro.MapGet($"/{producto.Value}", (HttpClient http) => Agro.GetAgro(http, cotizacionesUrl, producto.Key));
}

// energia

var energia = app.MapGroup("/energia");

foreach (var e in endpoints["Energia"])
{
    energia.MapGet($"/{e.Value}", (HttpClient http) => Energia.GetEnergia(http, cotizacionesUrl, e.Key));
}


// metales

var metales = app.MapGroup("/metales");

foreach (var m in endpoints["Metales"])
{
    metales.MapGet($"/{m.Value}", (HttpClient http) => Metales.GetMetales(http, cotizacionesUrl, m.Key));
}

static async Task<Dictionary<string, Dictionary<string, string>>> GetEndpointsFor(string path)
{
    string json = await File.ReadAllTextAsync(path);
    return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
}


app.UseSwaggerUI();

app.Run();
