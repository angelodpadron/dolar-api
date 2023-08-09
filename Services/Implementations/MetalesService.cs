namespace DolarApi.Services;

using DolarApi.Utils;

public class MetalesService : IMetalesService
{
    private readonly HttpClient _httpClient;

    const string NODO_METALES = "Metales";
    const string NODO_ACTUALIZACION = "ultima";

    public MetalesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IResult> GetMetales(string cotizacionesUrl, string nombreCotizacion)
    {
        var doc = await Utils.GetXmlDoc(_httpClient, cotizacionesUrl);
        var metales = Utils.GetXElementFromNode(doc, NODO_METALES, nombreCotizacion);

        if (metales is null)
            return TypedResults.NotFound();

        var actualizacion = Utils.GetXElementFromNode(doc, NODO_ACTUALIZACION, NODO_METALES);

        return TypedResults.Ok(
            new
            {
                Nombre = metales.Element("nombre")?.Value,
                Valor = metales.Element("compra")?.Value,
                Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
            }
        );
    }
}
