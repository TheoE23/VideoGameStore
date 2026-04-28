namespace VideoGameStore.Services.Payments
{
    public class PayPalService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public PayPalService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        private async Task<string> GetAccessToken()
        {
            var clientId = _config["PayPal:ClientId"];
            var secret = _config["PayPal:Secret"];

            var authToken = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"{clientId}:{secret}")
            );

            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{_config["PayPal:BaseUrl"]}/v1/oauth2/token");

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

            request.Content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            return data.access_token;
        }

        public async Task<string> CreateOrderAsync(decimal amount, string returnUrl, string cancelUrl)
        {
            var token = await GetAccessToken();

            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{_config["PayPal:BaseUrl"]}/v2/checkout/orders");

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            request.Content = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    intent = "CAPTURE",
                    purchase_units = new[]
                    {
                new {
                    amount = new {
                        currency_code = "USD",
                        value = amount.ToString("0.00")
                    }
                }
                    },
                    application_context = new
                    {
                        return_url = returnUrl,
                        cancel_url = cancelUrl
                    }
                }),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            foreach (var link in data.links)
            {
                if (link.rel == "approve")
                {
                    return link.href;
                }
            }

            throw new Exception("No approval link found");
        }

        public async Task<bool> CaptureOrderAsync(string orderId)
        {
            var token = await GetAccessToken();

            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{_config["PayPal:BaseUrl"]}/v2/checkout/orders/{orderId}/capture");

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            request.Content = new StringContent(
                "{}",
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            if (json.Contains("ORDER_ALREADY_CAPTURED"))
                return true;

            return response.IsSuccessStatusCode;
        }
    }
}
