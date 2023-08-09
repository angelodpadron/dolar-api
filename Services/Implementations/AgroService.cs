namespace DolarApi.Services;

using DolarApi.Utils;

public class AgroService : IAgroService
{
    private readonly HttpClient _httpClient;

    const string NODO_AGRO = "Agro";
    const string NODO_ACTUALIZACION = "ultima";

    public AgroService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IResult> GetAgro(string cotizacionesUrl, string nombreCotizacion)
    {
        var doc = await Utils.GetXmlDoc(_httpClient, cotizacionesUrl);
        var agro = Utils.GetXElementFromNode(doc, NODO_AGRO, nombreCotizacion);

        if (agro is null)
            return TypedResults.NotFound();

        var actualizacion = Utils.GetXElementFromNode(doc, NODO_ACTUALIZACION, NODO_AGRO);

        return TypedResults.Ok(
            new
            {
                Nombre = agro.Element("nombre")?.Value,
                Valor = agro.Element("compra")?.Value,
                Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
            }
        );
    }
}
