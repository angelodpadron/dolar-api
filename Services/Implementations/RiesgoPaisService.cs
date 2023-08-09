namespace DolarApi.Services;

using DolarApi.Utils;

public class RiesgoPaisService : IRiesgoPaisService
{
    private readonly HttpClient _httpClient;

    const string NODO_RIESGO_PAIS = "Riesgo_pais";
    const string NODO_ACTUALIZACION = "ultima";

    public RiesgoPaisService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IResult> GetRiesgoPais(string cotizacionesUrl, string pais)
    {
        var doc = await Utils.GetXmlDoc(_httpClient, cotizacionesUrl);
        var riesgoPais = Utils.GetXElementFromNode(doc, NODO_RIESGO_PAIS, pais);

        if (riesgoPais is null)
            return TypedResults.NotFound();

        var actualizacion = Utils.GetXElementFromNode(doc, NODO_ACTUALIZACION, NODO_RIESGO_PAIS);

        return TypedResults.Ok(
            new
            {
                Nombre = riesgoPais.Element("nombre")?.Value,
                Valor = riesgoPais.Element("compra")?.Value,
                Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
            }
        );
    }
}
