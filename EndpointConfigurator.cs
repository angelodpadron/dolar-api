using System.Text.Json;
using DolarApi.Services;

namespace DolarApi.Helpers;

public static class EndpointConfigurator
{
    private static async Task<Dictionary<string, List<Dictionary<string, string>>>> GetEndpointsFor(
        string path
    )
    {
        string json = await File.ReadAllTextAsync(path);
        var endpoints =
            JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, string>>>>(json)
            ?? throw new Exception("No se pudo obtener los endpoints");
        return endpoints;
    }

    public static async Task ConfigureEndpointsAsync(
        WebApplication app,
        string endpointsConfigPath,
        string cotizacionesUrl
    )
    {
        var endpoints = await GetEndpointsFor(endpointsConfigPath);
        var serviceProvider = app.Services;
        var urlBase = app.MapGroup("/api");

        // dolar
        ConfigureDolarEndpoints(urlBase, serviceProvider, cotizacionesUrl, endpoints);

        // reservas y circulante
        ConfigureReservasYCirculanteEndpoints(urlBase, serviceProvider, cotizacionesUrl);

        // riesgo pais
        ConfigureRiesgoPaisEndpoints(urlBase, serviceProvider, cotizacionesUrl, endpoints);

        // agro
        ConfigureAgroEndpoints(urlBase, serviceProvider, cotizacionesUrl, endpoints);

        // energia
        ConfigureEnergiaEndpoints(urlBase, serviceProvider, cotizacionesUrl, endpoints);

        // metales
        ConfigureMetalesEndpoints(urlBase, serviceProvider, cotizacionesUrl, endpoints);
    }

    private static void ConfigureDolarEndpoints(
        RouteGroupBuilder app,
        IServiceProvider serviceProvider,
        string cotizacionesUrl,
        Dictionary<string, List<Dictionary<string, string>>> endpoints
    )
    {
        var dolar = app.MapGroup("/dolar");
        var cotizacionDolarService = serviceProvider.GetService<ICotizacionDolarService>();

        // valores principales
        dolar.MapGet(
            "/principales",
            () => cotizacionDolarService.GetValoresPrincipales(cotizacionesUrl)
        );

        // por tipo de cambio
        foreach (var tipo in endpoints["TipoDolar"])
        {
            dolar.MapGet(
                $"/{tipo["endpoint"]}",
                () => cotizacionDolarService.GetCotizacionParaTipo(cotizacionesUrl, tipo["nombre"])
            );
        }

        // oficial por banco
        foreach (var banco in endpoints["Bancos"])
        {
            dolar.MapGet(
                $"/oficial/{banco["endpoint"]}",
                () =>
                    cotizacionDolarService.GetCotizacionOficialBanco(
                        cotizacionesUrl,
                        banco["nombre"]
                    )
            );
        }
    }

    private static void ConfigureReservasYCirculanteEndpoints(
        RouteGroupBuilder app,
        IServiceProvider serviceProvider,
        string cotizacionesUrl
    )
    {
        var reservas = app.MapGroup("/reservas");
        var circulante = app.MapGroup("/circulante");

        var reservasYCirculantesService =
            serviceProvider.GetService<IReservasYCirculantesService>();

        reservas.MapGet("/", () => reservasYCirculantesService.GetReservas(cotizacionesUrl));
        circulante.MapGet("/", () => reservasYCirculantesService.GetCirculantes(cotizacionesUrl));
    }

    private static void ConfigureRiesgoPaisEndpoints(
        RouteGroupBuilder app,
        IServiceProvider serviceProvider,
        string cotizacionesUrl,
        Dictionary<string, List<Dictionary<string, string>>> endpoints
    )
    {
        var riesgoPais = app.MapGroup("/riesgo_pais");
        var riesgoPaisService = serviceProvider.GetService<IRiesgoPaisService>();

        foreach (var pais in endpoints["RiesgoPais"])
        {
            riesgoPais.MapGet(
                $"/{pais["endpoint"]}",
                () => riesgoPaisService.GetRiesgoPais(cotizacionesUrl, pais["nombre"])
            );
        }
    }

    private static void ConfigureAgroEndpoints(
        RouteGroupBuilder app,
        IServiceProvider serviceProvider,
        string cotizacionesUrl,
        Dictionary<string, List<Dictionary<string, string>>> endpoints
    )
    {
        var agro = app.MapGroup("/agro");
        var agroService = serviceProvider.GetService<IAgroService>();

        foreach (var producto in endpoints["Agro"])
        {
            agro.MapGet(
                $"/{producto["endpoint"]}",
                () => agroService.GetAgro(cotizacionesUrl, producto["nombre"])
            );
        }
    }

    private static void ConfigureEnergiaEndpoints(
        RouteGroupBuilder app,
        IServiceProvider serviceProvider,
        string cotizacionesUrl,
        Dictionary<string, List<Dictionary<string, string>>> endpoints
    )
    {
        var energia = app.MapGroup("/energia");
        var energiaService = serviceProvider.GetService<IEnergiaService>();

        foreach (var producto in endpoints["Energia"])
        {
            energia.MapGet(
                $"/{producto["endpoint"]}",
                () => energiaService.GetEnergia(cotizacionesUrl, producto["nombre"])
            );
        }
    }

    private static void ConfigureMetalesEndpoints(
        RouteGroupBuilder app,
        IServiceProvider serviceProvider,
        string cotizacionesUrl,
        Dictionary<string, List<Dictionary<string, string>>> endpoints
    )
    {
        var metales = app.MapGroup("/metales");
        var metalesService = serviceProvider.GetService<IMetalesService>();

        foreach (var producto in endpoints["Metales"])
        {
            metales.MapGet(
                $"/{producto["endpoint"]}",
                () => metalesService.GetMetales(cotizacionesUrl, producto["nombre"])
            );
        }
    }
}
