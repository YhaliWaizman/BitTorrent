using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Text;
public class Web
{
    private static readonly HttpClient client = new HttpClient();

    public static async Task<string?> GetAsync(string baseUrl, Dictionary<string, string> queryParams)
    {
        try
        {
            var uriBuilder = new UriBuilder(baseUrl);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            foreach (var param in queryParams)
            {
                query[param.Key] = param.Value;
            }

            uriBuilder.Query = query.ToString();
            string finalUrl = uriBuilder.ToString();
            HttpResponseMessage response = await client.GetAsync(finalUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
            return null;
        }
    }
    public static string UrlEncodeInfoHash(byte[] infoHash)
    {
        StringBuilder sb = new();
        foreach (byte b in infoHash)
        {
            sb.Append($"%{b:X2}");
        }
        return sb.ToString();
    }
}
