namespace DolarApi.Services;

using DolarApi.Utils;

public class ReservasYCirculantesService : IReservasYCirculantesService
{
    private readonly HttpClient _httpClient;

    const string NODO_RESERVAS_Y_CIRCULANTE = "Reservas_y_circulante";
    const string NODO_ACTUALIZACION = "ultima";

    public ReservasYCirculantesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private async Task<IResult> GetReservasYCirculantes(
        string cotizacionesUrl,
        string nombreCotizacion
    )
    {
        var doc = await Utils.GetXmlDoc(_httpClient, cotizacionesUrl);
        var reservas = Utils.GetXElementFromNode(doc, NODO_RESERVAS_Y_CIRCULANTE, nombreCotizacion);

        if (reservas is null)
            return TypedResults.NotFound();

        var actualizacion = Utils.GetXElementFromNode(
            doc,
            NODO_ACTUALIZACION,
            NODO_RESERVAS_Y_CIRCULANTE
        );

        return TypedResults.Ok(
            new
            {
                Nombre = reservas.Element("nombre")?.Value,
                Valor = reservas.Element("compra")?.Value,
                Obervaciones = reservas.Element("observaciones")?.Value,
                Actualizado = $"{actualizacion?.Element("fecha")?.Value} {actualizacion?.Element("hora")?.Value}"
            }
        );
    }

    public async Task<IResult> GetCirculantes(string cotizacionesUrl)
    {
        return await GetReservasYCirculantes(cotizacionesUrl, "Circulante");
    }

    public async Task<IResult> GetReservas(string cotizacionesUrl)
    {
        return await GetReservasYCirculantes(cotizacionesUrl, "Reservas");
    }
}
