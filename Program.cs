using System.Xml.Linq;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
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

const string CotizacionesUrl = "https://www.dolarsi.com/api/dolarSiInfo.xml";

// dolar

var dolar = app.MapGroup("/dolar");

// generales

dolar.MapGet("/principales", GetValoresPrincipales);
dolar.MapGet("/oficial", (HttpClient httpClient) => GetValorDolar(httpClient, CotizacionesUrl, "Dolar Oficial"));
dolar.MapGet("/blue", (HttpClient httpClient) => GetValorDolar(httpClient, CotizacionesUrl, "Dolar Blue"));
dolar.MapGet("/bolsa", (HttpClient httpClient) => GetValorDolar(httpClient, CotizacionesUrl, "Dolar Bolsa"));
dolar.MapGet("/liqui", (HttpClient httpClient) => GetValorDolar(httpClient, CotizacionesUrl, "Dolar Contado con Liqui"));
dolar.MapGet("/turista", (HttpClient httpClient) => GetValorDolar(httpClient, CotizacionesUrl, "Dolar Turista"));
dolar.MapGet("/soja", (HttpClient httpClient) => GetValorDolar(httpClient, CotizacionesUrl, "Dolar Soja"));

// oficial por banco

dolar.MapGet("/oficial/banco_nacion", (HttpClient httpClient) => GetValorDolarOficialBanco(httpClient, CotizacionesUrl, "Banco Nación"));
dolar.MapGet("/oficial/banco_hipotecario", (HttpClient httpClient) => GetValorDolarOficialBanco(httpClient, CotizacionesUrl, "Banco Hipotecario"));
dolar.MapGet("/oficial/banco_bbva", (HttpClient httpClient) => GetValorDolarOficialBanco(httpClient, CotizacionesUrl, "Banco BBVA"));
dolar.MapGet("/oficial/banco_galicia", (HttpClient httpClient) => GetValorDolarOficialBanco(httpClient, CotizacionesUrl, "Banco Galicia"));
dolar.MapGet("/oficial/banco_santander", (HttpClient httpClient) => GetValorDolarOficialBanco(httpClient, CotizacionesUrl, "Banco Santander"));
dolar.MapGet("/oficial/banco_supervielle", (HttpClient httpClient) => GetValorDolarOficialBanco(httpClient, CotizacionesUrl, "Banco Supervielle"));
dolar.MapGet("/oficial/banco_patagonia", (HttpClient httpClient) => GetValorDolarOficialBanco(httpClient, CotizacionesUrl, "Banco Patagonia"));
dolar.MapGet("/oficial/banco_itau", (HttpClient httpClient) => GetValorDolarOficialBanco(httpClient, CotizacionesUrl, "Banco Itaú Argentina S.A."));
dolar.MapGet("/oficial/banco_icbc", (HttpClient httpClient) => GetValorDolarOficialBanco(httpClient, CotizacionesUrl, "Industrial and Commercial Bank of China "));

static async Task<IResult> GetValoresPrincipales(HttpClient httpClient)
{
    try
    {
        HttpResponseMessage responseMessage = await httpClient.GetAsync(CotizacionesUrl);
        responseMessage.EnsureSuccessStatusCode();
        string xmlString = await responseMessage.Content.ReadAsStringAsync();

        var xml = XDocument.Parse(xmlString);

        var cotizacionesDolares = xml.Descendants("Dolar").Elements()
            .Select(c => new
            {
                Nombre = c.Element("nombre")?.Value,
                Compra = c.Element("compra")?.Value,
                Venta = c.Element("venta")?.Value,
                Decimales = c.Element("decimales")?.Value
            }
            )
            .ToList();

        return TypedResults.Ok(cotizacionesDolares);


    }
    catch (HttpRequestException)
    {
        return TypedResults.StatusCode(503);
    }

}

static async Task<IResult> GetValorDolar(HttpClient httpClient, string cotizacionesUrl, string nombreCotizacion)
{
    try
    {
        HttpResponseMessage responseMessage = await httpClient.GetAsync(CotizacionesUrl);
        responseMessage.EnsureSuccessStatusCode();
        string xmlString = await responseMessage.Content.ReadAsStringAsync();

        var xml = XDocument.Parse(xmlString);

        var dolar = xml
            .Descendants("valores_principales")
            .Elements()
            .FirstOrDefault(c => c.Element("nombre")?.Value == nombreCotizacion);

        if (dolar is null) return TypedResults.NotFound();

        var actualizacion = xml.Descendants("ultima").Elements().FirstOrDefault(c => c.Element("nombre")?.Value == "valores_principales");

        return TypedResults.Ok(new
        {
            Nombre = dolar.Element("nombre")?.Value,
            Compra = dolar.Element("compra")?.Value,
            Venta = dolar.Element("venta")?.Value,
            Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
        });

    }
    catch (HttpRequestException)
    {
        return TypedResults.StatusCode(503);
    }
}

static async Task<IResult> GetValorDolarOficialBanco(HttpClient http, string cotizacionesUrl, string banco)
{
    try
    {
        HttpResponseMessage responseMessage = await http.GetAsync(CotizacionesUrl);
        responseMessage.EnsureSuccessStatusCode();
        string xmlDoc = await responseMessage.Content.ReadAsStringAsync();

        var xml = XDocument.Parse(xmlDoc);

        var dolar = xml
            .Descendants("Capital_Federal")
            .Elements()
            .FirstOrDefault(c => c.Element("nombre")?.Value == banco); // con acento en la "o"

        if (dolar is null) return TypedResults.NotFound();

        var actualizacion = xml.Descendants("ultima").Elements().FirstOrDefault(c => c.Element("nombre")?.Value == "Capital_Federal");

        return TypedResults.Ok(new
        {
            Nombre = dolar.Element("nombre")?.Value,
            Compra = dolar.Element("compra")?.Value,
            Venta = dolar.Element("venta")?.Value,
            Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
        });
    }
    catch (HttpRequestException)
    {
        return TypedResults.StatusCode(503);
    }
}

// reservas y circulante

var reservas = app.MapGroup("/reservas");
var circulante = app.MapGroup("/circulante");

reservas.MapGet("/", (HttpClient http) => GetRyC(http, CotizacionesUrl, "Reservas"));
circulante.MapGet("/", (HttpClient http) => GetRyC(http, CotizacionesUrl, "Circulante"));

static async Task<IResult> GetRyC(HttpClient http, string cotizacionesUrl, string nombreCotizacion)
{
    try
    {
        HttpResponseMessage responseMessage = await http.GetAsync(CotizacionesUrl);
        responseMessage.EnsureSuccessStatusCode();
        string xmlDoc = await responseMessage.Content.ReadAsStringAsync();

        var xml = XDocument.Parse(xmlDoc);

        var reservas = xml.
            Descendants("Reservas_y_circulante")
            .Elements()
            .FirstOrDefault(q => q.Element("nombre")?.Value.Contains(nombreCotizacion) ?? false);

        if (reservas is null) return TypedResults.NotFound();


        return TypedResults.Ok(new
        {
            Nombre = reservas.Element("nombre")?.Value,
            Valor = reservas.Element("compra")?.Value,
            Obervaciones = reservas.Element("observaciones")?.Value
        });

    }
    catch (HttpRequestException)
    {
        return TypedResults.StatusCode(503);
    }
}

// riesgo pais

var riesgoPais = app.MapGroup("/riesgo_pais");

riesgoPais.MapGet("/argentina", (HttpClient http) => GetRiesgoPais(http, CotizacionesUrl, "Argentina"));
riesgoPais.MapGet("/brasil", (HttpClient http) => GetRiesgoPais(http, CotizacionesUrl, "Brasil"));
riesgoPais.MapGet("/uruguay", (HttpClient http) => GetRiesgoPais(http, CotizacionesUrl, "Uruguay"));
riesgoPais.MapGet("/mexico", (HttpClient http) => GetRiesgoPais(http, CotizacionesUrl, "México"));

static async Task<IResult> GetRiesgoPais(HttpClient http, string cotizacionesUrl, string pais)
{
    try
    {
        HttpResponseMessage responseMessage = await http.GetAsync(CotizacionesUrl);
        responseMessage.EnsureSuccessStatusCode();
        string xmlDoc = await responseMessage.Content.ReadAsStringAsync();

        var xml = XDocument.Parse(xmlDoc);

        var riesgoPais = xml.
            Descendants("Riesgo_pais")
            .Elements()
            .FirstOrDefault(q => q.Element("nombre")?.Value.Contains(pais) ?? false);

        if (riesgoPais is null) return TypedResults.NotFound();

        var actualizacion = xml.Descendants("ultima").Elements().FirstOrDefault(c => c.Element("nombre")?.Value == "RIESGO_PAIS");

        return TypedResults.Ok(new
        {
            Nombre = riesgoPais.Element("nombre")?.Value,
            Puntos = riesgoPais.Element("compra")?.Value,
            Variacion = riesgoPais.Element("venta")?.Value,
            MejorCompra = riesgoPais.Element("mejor_compra")?.Value,
            MejorVenta = riesgoPais.Element("mejor_venta")?.Value,
            Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
        });

    }
    catch (HttpRequestException)
    {
        return TypedResults.StatusCode(503);
    }

}

// agro

var agro = app.MapGroup("/agro");

agro.MapGet("/trigo", (HttpClient httpClient) => GetAgro(httpClient, CotizacionesUrl, "Trigo"));
agro.MapGet("/maiz", (HttpClient httpClient) => GetAgro(httpClient, CotizacionesUrl, "Maiz"));
agro.MapGet("/soja_rosario", (HttpClient httpClient) => GetAgro(httpClient, CotizacionesUrl, "Soja Rosario"));
agro.MapGet("/soja_chicago", (HttpClient httpClient) => GetAgro(httpClient, CotizacionesUrl, "Soja Chicago"));

static async Task<IResult> GetAgro(HttpClient http, string cotizacionesUrl, string nombreCotizacion)
{
    try
    {
        HttpResponseMessage responseMessage = await http.GetAsync(CotizacionesUrl);
        responseMessage.EnsureSuccessStatusCode();
        string xmlDoc = await responseMessage.Content.ReadAsStringAsync();

        var xml = XDocument.Parse(xmlDoc);

        var agro = xml.
            Descendants("Agro")
            .Elements()
            .FirstOrDefault(q => q.Element("nombre")?.Value.Contains(nombreCotizacion) ?? false);

        if (agro is null) return TypedResults.NotFound();

        var actualizacion = xml.Descendants("ultima").Elements().FirstOrDefault(c => c.Element("nombre")?.Value == "AGRO");

        return TypedResults.Ok(new
        {
            Nombre = agro.Element("nombre")?.Value,
            Compra = agro.Element("compra")?.Value,
            Venta = agro.Element("venta")?.Value,
            Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
        });

    }
    catch (HttpRequestException)
    {
        return TypedResults.StatusCode(503);
    }
}

// energia

var energia = app.MapGroup("/energia");

energia.MapGet("/petroleo/wti", (HttpClient httpClient) => GetEnergia(httpClient, CotizacionesUrl, "WTI"));
energia.MapGet("/petroleo/brent", (HttpClient httpClient) => GetEnergia(httpClient, CotizacionesUrl, "BRENT"));
energia.MapGet("/gas", (HttpClient httpClient) => GetEnergia(httpClient, CotizacionesUrl, "Gas"));

static async Task<IResult> GetEnergia(HttpClient http, string cotizacionesUrl, string nombreCotizacion)
{
    try
    {
        HttpResponseMessage responseMessage = await http.GetAsync(CotizacionesUrl);
        responseMessage.EnsureSuccessStatusCode();
        string xmlDoc = await responseMessage.Content.ReadAsStringAsync();

        var xml = XDocument.Parse(xmlDoc);

        var energia = xml.
            Descendants("Energia")
            .Elements()
            .FirstOrDefault(q => q.Element("nombre")?.Value.Contains(nombreCotizacion) ?? false);

        if (energia is null) return TypedResults.NotFound();

        var actualizacion = xml.Descendants("ultima").Elements().FirstOrDefault(c => c.Element("nombre")?.Value == "ENERGIA");

        return TypedResults.Ok(new
        {
            Nombre = energia.Element("nombre")?.Value,
            Compra = energia.Element("compra")?.Value,
            Venta = energia.Element("venta")?.Value,
            Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
        });

    }
    catch (HttpRequestException)
    {
        return TypedResults.StatusCode(503);
    }
}

// metales

var metales = app.MapGroup("/metales");

metales.MapGet("/oro", (HttpClient httpClient) => GetMetales(httpClient, CotizacionesUrl, "Oro"));
metales.MapGet("/plata", (HttpClient httpClient) => GetMetales(httpClient, CotizacionesUrl, "Plata"));
metales.MapGet("/cobre", (HttpClient httpClient) => GetMetales(httpClient, CotizacionesUrl, "Cobre"));

static async Task<IResult> GetMetales(HttpClient http, string cotizacionesUrl, string nombreCotizacion)
{
    try
    {
        HttpResponseMessage responseMessage = await http.GetAsync(CotizacionesUrl);
        responseMessage.EnsureSuccessStatusCode();
        string xmlDoc = await responseMessage.Content.ReadAsStringAsync();

        var xml = XDocument.Parse(xmlDoc);

        var metales = xml.
            Descendants("Metales")
            .Elements()
            .FirstOrDefault(q => q.Element("nombre")?.Value.Contains(nombreCotizacion) ?? false);

        if (metales is null) return TypedResults.NotFound();

        var actualizacion = xml.Descendants("ultima").Elements().FirstOrDefault(c => c.Element("nombre")?.Value == "METALES");

        return TypedResults.Ok(new
        {
            Nombre = metales.Element("nombre")?.Value,
            Compra = metales.Element("compra")?.Value,
            Venta = metales.Element("venta")?.Value,
            Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
        });

    }
    catch (HttpRequestException)
    {
        return TypedResults.StatusCode(503);
    }
}

app.UseSwaggerUI();

app.Run();
