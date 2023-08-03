using System.Xml.Linq;
namespace DolarApi.Utils;

public static class Utils
{
    public static async Task<XDocument> GetXmlDoc(HttpClient httpClient, string cotizacionesUrl)
    {
        HttpResponseMessage responseMessage = await httpClient.GetAsync(cotizacionesUrl);
        responseMessage.EnsureSuccessStatusCode();
        return XDocument.Parse(await responseMessage.Content.ReadAsStringAsync());
    }

    public static XElement GetXElementFromNode(XDocument doc, string nodo, string nombre)
    {
        return doc.Descendants(nodo).Elements().FirstOrDefault(c => c.Element("nombre")?.Value.Contains(nombre) ?? false);
    }
}

public static class CotizacionDolar
{

    public static async Task<IResult> GetValoresPrincipales(HttpClient httpClient, string cotizacionesUrl)
    {
        var doc = await Utils.GetXmlDoc(httpClient, cotizacionesUrl);

        var cotizacionesDolares = doc.Descendants("Dolar").Elements()
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
    public static async Task<IResult> GetValorDolar(HttpClient httpClient, string cotizacionesUrl, string nombreCotizacion)
    {
        var doc = await Utils.GetXmlDoc(httpClient, cotizacionesUrl);
        var dolar = Utils.GetXElementFromNode(doc, "valores_principales", nombreCotizacion);

        if (dolar is null) return TypedResults.NotFound();

        var actualizacion = Utils.GetXElementFromNode(doc, "ultima", "valores_principales");

        return TypedResults.Ok(new
        {
            Nombre = dolar.Element("nombre")?.Value,
            Compra = dolar.Element("compra")?.Value,
            Venta = dolar.Element("venta")?.Value,
            Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
        });

    }
    public static async Task<IResult> GetValorOficialBanco(HttpClient http, string cotizacionesUrl, string banco)
    {


        var doc = await Utils.GetXmlDoc(http, cotizacionesUrl);
        var dolar = Utils.GetXElementFromNode(doc, "Capital_Federal", banco);

        if (dolar is null) return TypedResults.NotFound();

        var actualizacion = Utils.GetXElementFromNode(doc, "ultima", "Capital_Federal");

        return TypedResults.Ok(new
        {
            Nombre = dolar.Element("nombre")?.Value,
            Compra = dolar.Element("compra")?.Value,
            Venta = dolar.Element("venta")?.Value,
            Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
        });
    }
    
}

public static class ReservasYCirculantes
{
    public static async Task<IResult> GetRyC(HttpClient http, string cotizacionesUrl, string nombreCotizacion)
    {

        var doc = await Utils.GetXmlDoc(http, cotizacionesUrl);
        var reservas = Utils.GetXElementFromNode(doc, "Reservas_y_circulante", nombreCotizacion);

        if (reservas is null) return TypedResults.NotFound();

        var actualizacion = Utils.GetXElementFromNode(doc, "ultima", "RESERVAS_Y_CIRCULANTE");

        return TypedResults.Ok(new
        {
            Nombre = reservas.Element("nombre")?.Value,
            Valor = reservas.Element("compra")?.Value,
            Obervaciones = reservas.Element("observaciones")?.Value,
            Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
        });

    }
}

public static class RiesgoPais
{
    public static async Task<IResult> GetRiesgoPais(HttpClient http, string cotizacionesUrl, string pais)
    {

        var doc = await Utils.GetXmlDoc(http, cotizacionesUrl);
        var riesgoPais = Utils.GetXElementFromNode(doc, "Riesgo_pais", pais);

        if (riesgoPais is null) return TypedResults.NotFound();

        var actualizacion = Utils.GetXElementFromNode(doc, "ultima", "RIESGO_PAIS");

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

}

public static class Agro
{
    public static async Task<IResult> GetAgro(HttpClient http, string cotizacionesUrl, string nombreCotizacion)
    {

        var doc = await Utils.GetXmlDoc(http, cotizacionesUrl);
        var agro = Utils.GetXElementFromNode(doc, "Agro", nombreCotizacion);

        if (agro is null) return TypedResults.NotFound();

        var actualizacion = Utils.GetXElementFromNode(doc, "ultima", "AGRO");

        return TypedResults.Ok(new
        {
            Nombre = agro.Element("nombre")?.Value,
            Compra = agro.Element("compra")?.Value,
            Venta = agro.Element("venta")?.Value,
            Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
        });

    }
}

public static class Energia
{
    public static async Task<IResult> GetEnergia(HttpClient http, string cotizacionesUrl, string nombreCotizacion)
    {

        var doc = await Utils.GetXmlDoc(http, cotizacionesUrl);
        var energia = Utils.GetXElementFromNode(doc, "Energia", nombreCotizacion);

        if (energia is null) return TypedResults.NotFound();

        var actualizacion = Utils.GetXElementFromNode(doc, "ultima", "ENERGIA");

        return TypedResults.Ok(new
        {
            Nombre = energia.Element("nombre")?.Value,
            Compra = energia.Element("compra")?.Value,
            Venta = energia.Element("venta")?.Value,
            Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
        });

    }
}

public static class Metales
{
    public static async Task<IResult> GetMetales(HttpClient http, string cotizacionesUrl, string nombreCotizacion)
    {

        var doc = await Utils.GetXmlDoc(http, cotizacionesUrl);
        var metales = Utils.GetXElementFromNode(doc, "Metales", nombreCotizacion);

        if (metales is null) return TypedResults.NotFound();

        var actualizacion = Utils.GetXElementFromNode(doc, "ultima", "METALES");

        return TypedResults.Ok(new
        {
            Nombre = metales.Element("nombre")?.Value,
            Compra = metales.Element("compra")?.Value,
            Venta = metales.Element("venta")?.Value,
            Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
        });

    }
}



