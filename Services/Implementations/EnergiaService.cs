namespace DolarApi.Services;

using DolarApi.Utils;

public class EnergiaService : IEnergiaService
{
    private readonly HttpClient _httpClient;

    const string NODO_ENERGIA = "Energia";
    const string NODO_ACTUALIZACION = "ultima";

    public EnergiaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IResult> GetEnergia(string cotizacionesUrl, string nombreCotizacion)
    {
        var doc = await Utils.GetXmlDoc(_httpClient, cotizacionesUrl);
        var energia = Utils.GetXElementFromNode(doc, NODO_ENERGIA, nombreCotizacion);

        if (energia is null)
            return TypedResults.NotFound();

        var actualizacion = Utils.GetXElementFromNode(doc, NODO_ACTUALIZACION, NODO_ENERGIA);

        return TypedResults.Ok(
            new
            {
                Nombre = energia.Element("nombre")?.Value,
                Valor = energia.Element("compra")?.Value,
                Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
            }
        );
    }
}
