using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace QLNhaSach1.Service
{
    public class PaypalService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public PaypalService(IHttpClientFactory factory, IConfiguration config)
        {
            _httpClientFactory = factory;
            _config = config;
        }

        public async Task<string> CreateOrder(decimal total, string returnUrl, string cancelUrl)
        {
            var client = _httpClientFactory.CreateClient();
            
            // Get access token
            var byteArray = Encoding.UTF8.GetBytes($"{_config["Paypal:ClientId"]}:{_config["Paypal:Secret"]}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            
            var tokenRes = await client.PostAsync("https://api-m.sandbox.paypal.com/v1/oauth2/token", new FormUrlEncodedContent(new Dictionary<string, string> {
                {"grant_type", "client_credentials"}
            }));
            var tokenJson = await tokenRes.Content.ReadAsStringAsync();
            var token = JsonDocument.Parse(tokenJson).RootElement.GetProperty("access_token").GetString();

            // Create order
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new
            {
                intent = "CAPTURE",
                purchase_units = new[] {
                    new {
                        amount = new {
                            currency_code = "USD",
                            value = total.ToString("F2")
                        }
                    }
                },
                application_context = new {
                    return_url = returnUrl,
                    cancel_url = cancelUrl
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var res = await client.PostAsync("https://api-m.sandbox.paypal.com/v2/checkout/orders", content);
            var resBody = await res.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(resBody);

            var approveLink = json.RootElement.GetProperty("links").EnumerateArray()
                .FirstOrDefault(l => l.GetProperty("rel").GetString() == "approve")
                .GetProperty("href").GetString();

            return approveLink;
        }
    }
}
