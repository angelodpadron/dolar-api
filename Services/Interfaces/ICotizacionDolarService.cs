namespace DolarApi.Services;

public interface ICotizacionDolarService
{
    Task<IResult> GetValoresPrincipales(string cotizacionesUrl);
    Task<IResult> GetCotizacionParaTipo(string cotizacionesUrl, string tipo);
    Task<IResult> GetCotizacionOficialBanco(string cotizacionesUrl, string banco);
}
