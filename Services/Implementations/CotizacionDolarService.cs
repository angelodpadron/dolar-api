namespace DolarApi.Services;

using System.Xml.Linq;
using DolarApi.Utils;

public class CotizacionDolarService : ICotizacionDolarService
{
    private readonly HttpClient _httpClient;

    const string NODO_DOLAR_VALORES_PRINCIPALES = "valores_principales";
    const string NODO_ACTUALIZACION = "ultima";
    const string NODO_DOLAR_OFICIAL = "Capital_Federal";
    public CotizacionDolarService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IResult> GetValoresPrincipales(string cotizacionesUrl)
    {
        var doc = await Utils.GetXmlDoc(_httpClient, cotizacionesUrl);
        var cotizaciones = doc.Descendants("Dolar")
            .Elements()
            .Select(
                c =>
                    new
                    {
                        Nombre = c.Element("nombre")?.Value,
                        Compra = c.Element("compra")?.Value,
                        Venta = c.Element("venta")?.Value,
                        Decimales = c.Element("decimales")?.Value
                    }
            )
            .ToList();

        var actualizacion =
            Utils.GetXElementFromNode(doc, "ultima", "dolar")
            ?? new XElement("fecha", "hora", "Sin datos");

        return TypedResults.Ok(
            new
            {
                Cotizaciones = cotizaciones,
                Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
            }
        );
    }

    public async Task<IResult> GetCotizacionParaTipo(string cotizacionesUrl, string tipo)
    {
        var doc = await Utils.GetXmlDoc(_httpClient, cotizacionesUrl);
        var dolar = Utils.GetXElementFromNode(doc, NODO_DOLAR_VALORES_PRINCIPALES, tipo);

        if (dolar is null)
            return TypedResults.NotFound();

        var actualizacion =
            Utils.GetXElementFromNode(doc, NODO_ACTUALIZACION, NODO_DOLAR_VALORES_PRINCIPALES);

        return TypedResults.Ok(
            new
            {
                Nombre = dolar.Element("nombre")?.Value,
                Compra = dolar.Element("compra")?.Value,
                Venta = dolar.Element("venta")?.Value,
                Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
            }
        );
    }

    public async Task<IResult> GetCotizacionOficialBanco(string cotizacionesUrl, string banco)
    {
        var doc = await Utils.GetXmlDoc(_httpClient, cotizacionesUrl);
        var dolar = Utils.GetXElementFromNode(doc, NODO_DOLAR_OFICIAL, banco);

        if (dolar is null)
            return TypedResults.NotFound();

        var actualizacion =
            Utils.GetXElementFromNode(doc, NODO_ACTUALIZACION, NODO_DOLAR_OFICIAL);

        return TypedResults.Ok(
            new
            {
                Nombre = dolar.Element("nombre")?.Value,
                Compra = dolar.Element("compra")?.Value,
                Venta = dolar.Element("venta")?.Value,
                Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
            }
        );
    }
}
