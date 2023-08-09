namespace DolarApi.Services;

public interface IRiesgoPaisService
{
    Task<IResult> GetRiesgoPais(string cotizacionesUrl, string pais);
}
