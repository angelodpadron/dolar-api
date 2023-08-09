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
        return doc.Descendants(nodo)
            .Elements()
            .FirstOrDefault(
                c => c.Element("nombre")?.Value.ToLower().Contains(nombre.ToLower()) ?? false
            );
    }
}
