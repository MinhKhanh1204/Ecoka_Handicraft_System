using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ChatbotAPI.DTOs;
using ChatbotAPI.Services;
using ChatbotAPI.CustomFormatter;

namespace ChatbotAPI.Services.Implements
{
    public class ContextBuilderService : IContextBuilderService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ContextBuilderService> _logger;

        private const string SecureClient = "SecureClient";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        // Cache keys and TTL
        private const string ProductsCacheKey = "ctx_products";
        private const string VouchersCacheKey = "ctx_vouchers";
        private static readonly TimeSpan ProductsCacheDuration = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan VouchersCacheDuration = TimeSpan.FromSeconds(60);

        public ContextBuilderService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache cache,
            ILogger<ContextBuilderService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ConversationContextDto> BuildContextAsync(string? customerId)
        {
            var context = new ConversationContextDto();

            // Products & vouchers: use cache (TTL 60s) — shared across all customers.
            // Cart & orders: fetched per-customer, no caching (data changes frequently).
            var productsTask = GetProductsWithCacheAsync();
            var vouchersTask = GetVouchersWithCacheAsync();

            Task<List<CartItemInfo>> cartTask;
            Task<List<OrderInfo>> ordersTask;

            if (string.IsNullOrEmpty(customerId))
            {
                _logger.LogInformation("[ContextBuilder] No customerId — skipping cart/orders; loading products + vouchers only.");
                cartTask = Task.FromResult(new List<CartItemInfo>());
                ordersTask = Task.FromResult(new List<OrderInfo>());
            }
            else
            {
                _logger.LogInformation("[ContextBuilder] Building context for customerId={CustomerId}", customerId);
                cartTask = GetCartItemsAsync(customerId);
                ordersTask = GetRecentOrdersAsync();
            }

            // Wrap each in ContinueWith so a single API failure doesn't propagate to all.
            var wrappedProducts = productsTask.ContinueWith(t =>
            {
                if (t.IsFaulted) _logger.LogWarning(t.Exception, "[ContextBuilder] Products fetch failed — continuing without products.");
                return t.IsCompletedSuccessfully ? t.Result : new List<ProductInfo>();
            }, TaskScheduler.Default);

            var wrappedVouchers = vouchersTask.ContinueWith(t =>
            {
                if (t.IsFaulted) _logger.LogWarning(t.Exception, "[ContextBuilder] Vouchers fetch failed — continuing without vouchers.");
                return t.IsCompletedSuccessfully ? t.Result : new List<VoucherInfo>();
            }, TaskScheduler.Default);

            await Task.WhenAll(cartTask, ordersTask, wrappedProducts, wrappedVouchers);

            context.CartItems = await cartTask;
            context.RecentOrders = await ordersTask;
            context.RecommendedProducts = await wrappedProducts;
            context.AvailableVouchers = await wrappedVouchers;

            // Enrich cart items with product details (name + price) using all loaded products.
            EnrichCartItemsFromProducts(context);

            _logger.LogInformation(
                "[ContextBuilder] Done. Cart={CartCount}, Orders={OrderCount}, Products={ProductCount}, Vouchers={VoucherCount}",
                context.CartItems.Count,
                context.RecentOrders.Count,
                context.RecommendedProducts.Count,
                context.AvailableVouchers.Count);

            return context;
        }

        /// <summary>
        /// Products are cached for 60 seconds. Cart items are enriched from this full list
        /// (not just the top-8 display limit) so items outside the display window still get names/prices.
        /// </summary>
        private async Task<List<ProductInfo>> GetProductsWithCacheAsync()
        {
            if (_cache.TryGetValue(ProductsCacheKey, out List<ProductInfo>? cached) && cached != null)
            {
                _logger.LogInformation("[ContextBuilder] Products served from cache. Count={Count}", cached.Count);
                return cached;
            }

            var products = await FetchProductsFromApiAsync();
            _cache.Set(ProductsCacheKey, products, ProductsCacheDuration);
            return products;
        }

        private async Task<List<ProductInfo>> FetchProductsFromApiAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient(SecureClient);
                var productApiUrl = _configuration["ApiEndpoints:ProductAPI"];
                var url = $"{productApiUrl}/api/products";

                _logger.LogInformation("[ContextBuilder] Fetching products from {Url}", url);
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[ContextBuilder] ProductAPI returned {Status}", (int)response.StatusCode);
                    return new List<ProductInfo>();
                }

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductInfo>>>(JsonOptions);
                var products = result?.Data ?? new List<ProductInfo>();

                _logger.LogInformation("[ContextBuilder] Products fetched count={Count}", products.Count);
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ContextBuilder] Failed to fetch products");
                return new List<ProductInfo>();
            }
        }

        /// <summary>Vouchers are cached for 60 seconds (stable data).</summary>
        private async Task<List<VoucherInfo>> GetVouchersWithCacheAsync()
        {
            if (_cache.TryGetValue(VouchersCacheKey, out List<VoucherInfo>? cached) && cached != null)
            {
                _logger.LogInformation("[ContextBuilder] Vouchers served from cache. Count={Count}", cached.Count);
                return cached;
            }

            var vouchers = await FetchVouchersFromApiAsync();
            _cache.Set(VouchersCacheKey, vouchers, VouchersCacheDuration);
            return vouchers;
        }

        private async Task<List<VoucherInfo>> FetchVouchersFromApiAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient(SecureClient);
                var voucherApiUrl = _configuration["ApiEndpoints:VoucherAPI"];
                var url = $"{voucherApiUrl}/api/vouchers";

                _logger.LogInformation("[ContextBuilder] Fetching vouchers from {Url}", url);
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[ContextBuilder] VoucherAPI returned {Status}", (int)response.StatusCode);
                    return new List<VoucherInfo>();
                }

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<VoucherInfo>>>(JsonOptions);
                var raw = result?.Data ?? new List<VoucherInfo>();

                var active = raw
                    .Where(v => v.IsActive != false)
                    .Where(v => v.ExpiryDate == null || v.ExpiryDate >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
                    .Take(10)
                    .ToList();

                _logger.LogInformation("[ContextBuilder] Vouchers fetched count={Count}", active.Count);
                return active;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ContextBuilder] Failed to fetch vouchers");
                return new List<VoucherInfo>();
            }
        }

        /// <summary>
        /// CartAPI OData only returns productId + quantity.
        /// Enrich with name + price from the full products list (not just top-8).
        /// </summary>
        private void EnrichCartItemsFromProducts(ConversationContextDto context)
        {
            if (context.CartItems.Count == 0 || context.RecommendedProducts.Count == 0)
                return;

            var byId = context.RecommendedProducts
                .Where(p => !string.IsNullOrWhiteSpace(p.ProductId))
                .ToDictionary(p => p.ProductId, StringComparer.OrdinalIgnoreCase);

            foreach (var item in context.CartItems)
            {
                if (!byId.TryGetValue(item.ProductId, out var p))
                    continue;

                if (string.IsNullOrWhiteSpace(item.ProductName))
                    item.ProductName = p.ProductName ?? "";
                if (item.Price <= 0)
                    item.Price = p.DisplayPrice;
            }
        }

        /// <summary>Forwards the browser's Authorization header to internal APIs (Cart, Order).</summary>
        private void AttachIncomingAuthorization(HttpRequestMessage request)
        {
            var auth = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(auth))
            {
                if (AuthenticationHeaderValue.TryParse(auth, out var parsed))
                    request.Headers.Authorization = parsed;
                else
                    request.Headers.TryAddWithoutValidation("Authorization", auth);
            }
        }

        private async Task<List<CartItemInfo>> GetCartItemsAsync(string customerId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(SecureClient);
                var cartApiUrl = _configuration["ApiEndpoints:CartAPI"];
                var url = $"{cartApiUrl}/odata/carts?$filter=CustomerId%20eq%20'{customerId}'&$expand=CartItems&$top=1";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                AttachIncomingAuthorization(request);

                _logger.LogInformation("[ContextBuilder] Fetching cart from {Url}", url);
                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[ContextBuilder] CartAPI returned {Status} on OData endpoint", (int)response.StatusCode);
                    return new List<CartItemInfo>();
                }

                var body = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);

                var items = new List<CartItemInfo>();
                if (doc.RootElement.TryGetProperty("value", out var valueArray) &&
                    valueArray.GetArrayLength() > 0)
                {
                    var cart = valueArray[0];
                    if (cart.TryGetProperty("cartItems", out var cartItemsArray))
                    {
                        foreach (var item in cartItemsArray.EnumerateArray())
                        {
                            items.Add(new CartItemInfo
                            {
                                ProductId = item.TryGetProperty("productId", out var pid) ? pid.GetString() ?? "" : "",
                                ProductName = item.TryGetProperty("productName", out var pn) ? pn.GetString() ?? "" : "",
                                Quantity = item.TryGetProperty("quantity", out var q) ? q.GetInt32() : 0,
                                Price = item.TryGetProperty("price", out var pr) && pr.ValueKind == JsonValueKind.Number
                                    ? pr.GetDecimal()
                                    : 0
                            });
                        }
                    }
                }

                _logger.LogInformation("[ContextBuilder] Cart items count={Count}", items.Count);
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ContextBuilder] Failed to get cart items for customerId={CustomerId}", customerId);
                return new List<CartItemInfo>();
            }
        }

        private async Task<List<OrderInfo>> GetRecentOrdersAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient(SecureClient);
                var orderApiUrl = _configuration["ApiEndpoints:OrderAPI"];
                var url = $"{orderApiUrl}/api/customer/orders";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                AttachIncomingAuthorization(request);

                _logger.LogInformation("[ContextBuilder] Fetching orders from {Url}", url);
                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[ContextBuilder] OrderAPI returned {Status}", (int)response.StatusCode);
                    return new List<OrderInfo>();
                }

                var orders = await response.Content.ReadFromJsonAsync<List<OrderInfo>>(JsonOptions);
                var list = orders ?? new List<OrderInfo>();
                var recent = list
                    .OrderByDescending(o => o.OrderDate ?? DateTime.MinValue)
                    .Take(5)
                    .ToList();

                _logger.LogInformation("[ContextBuilder] Orders count={Count}", recent.Count);
                return recent;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ContextBuilder] Failed to get orders");
                return new List<OrderInfo>();
            }
        }
    }
}
