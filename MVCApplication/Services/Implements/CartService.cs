using MVCApplication.Models;
using Newtonsoft.Json.Linq;

namespace MVCApplication.Services.Implements
{
    public class CartService : ICartService
    {
        private readonly HttpClient _http;

        public CartService(HttpClient http)
        {
            _http = http;
        }

        // GET /odata/carts?$expand=CartItems — OData, trả về cart của user hiện tại (JWT trong header)
        public async Task<CartViewModel?> GetCartAsync()
        {
            var response = await _http.GetAsync("odata/carts?$expand=CartItems");

            string json = await response.Content.ReadAsStringAsync();

            // Debug: in ra raw response để kiểm tra
            Console.WriteLine($"[CartService] Status: {(int)response.StatusCode}");
            Console.WriteLine($"[CartService] Raw JSON: {json}");

            if (!response.IsSuccessStatusCode) return null;

            try
            {
                var value = JObject.Parse(json)["value"];
                if (value == null || !value.HasValues) return null;

                var cartObj = value.First!;
                return new CartViewModel
                {
                    CartId     = (int)cartObj["CartId"]!,
                    CustomerId = (string)cartObj["CustomerId"]!,
                    CartItems  = ((JArray)cartObj["CartItems"]!)
                        .Select(i => new CartItemViewModel
                        {
                            CartItemId = (int)i["CartItemID"]!,       // C# prop: CartItemID (uppercase ID)
                            ProductId  = (string?)i["ProductID"],      // C# prop: ProductID (uppercase ID)
                            Quantity   = (int)i["Quantity"]!
                        }).ToList()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CartService] Parse error: {ex.Message}");
                Console.WriteLine($"[CartService] JSON was: {json}");
                return null;  // trả null thay vì crash → View hiển thị giỏ trống
            }
        }

        // POST /cart/items
        public async Task<bool> AddToCartAsync(string productId, int quantity)
        {
            var body = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(new { ProductId = productId, Quantity = quantity }),
                System.Text.Encoding.UTF8,
                "application/json");
            var response = await _http.PostAsync("cart/items", body);
            return response.IsSuccessStatusCode;
        }

        // PUT /cart/items/{cartItemId}
        public async Task<bool> UpdateCartItemAsync(int cartItemId, int quantity)
        {
            var body = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(new { Quantity = quantity }),
                System.Text.Encoding.UTF8,
                "application/json");
            var response = await _http.PutAsync($"cart/items/{cartItemId}", body);
            return response.IsSuccessStatusCode;
        }

        // DELETE /cart/items/{cartItemId}
        public async Task<bool> DeleteCartItemAsync(int cartItemId)
        {
            var response = await _http.DeleteAsync($"cart/items/{cartItemId}");
            return response.IsSuccessStatusCode;
        }
    }
}
