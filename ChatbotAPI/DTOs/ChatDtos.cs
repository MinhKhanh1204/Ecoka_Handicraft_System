using System.Text.Json.Serialization;

namespace ChatbotAPI.DTOs;

// ─── Request / Response ────────────────────────────────────────────────────

public class ChatRequestDto
{
    public string Message { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string? CustomerId { get; set; }
    public string? CustomerName { get; set; }
}

public class ChatResponseDto
{
    public string Response { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public List<string>? SuggestedActions { get; set; }

    /// <summary>True khi không gọi được Gemini (429) và đã dùng câu trả lời dự phòng từ DB/API.</summary>
    public bool FallbackUsed { get; set; }
}

// ─── Diagnostics ────────────────────────────────────────────────────────────

/// <summary>Kết quả kiểm tra kết nối Gemini (GET /api/chat/diagnostics/gemini — chỉ Development).</summary>
public class GeminiConnectionTestDto
{
    public bool Ok { get; set; }
    public string Model { get; set; } = "";
    public string? ReplyPreview { get; set; }
    /// <summary>ok | quota | config | model_not_found | invalid_key | permission | unknown</summary>
    public string Category { get; set; } = "";
    public int? HttpStatus { get; set; }
    public string? GeminiMessage { get; set; }
    public string? RawErrorSnippet { get; set; }
    public string NextStepVi { get; set; } = "";
}

// ─── Context passed from ContextBuilderService to PromptBuilderService ───

public class ConversationContextDto
{
    public List<CartItemInfo> CartItems { get; set; } = new();
    public List<OrderInfo> RecentOrders { get; set; } = new();
    public List<ProductInfo> RecommendedProducts { get; set; } = new();
    public List<VoucherInfo> AvailableVouchers { get; set; } = new();
}

// ─── Mini DTOs used when fetching internal APIs ─────────────────────────────

public class CartItemInfo
{
    public string ProductId { get; set; } = "";
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class OrderInfo
{
    /// <summary>Khớp JSON OrderAPI: orderID</summary>
    [JsonPropertyName("orderID")]
    public string OrderID { get; set; } = "";

    public DateTime? OrderDate { get; set; }
    public decimal? TotalAmount { get; set; }

    [JsonPropertyName("paymentStatus")]
    public string? PaymentStatus { get; set; }

    [JsonPropertyName("shippingStatus")]
    public string? ShippingStatus { get; set; }

    /// <summary>Không có trong JSON — suy ra từ Payment/Shipping.</summary>
    [JsonIgnore]
    public string DisplayStatus =>
        !string.IsNullOrWhiteSpace(ShippingStatus) ? ShippingStatus! :
        !string.IsNullOrWhiteSpace(PaymentStatus) ? PaymentStatus! : "—";
}

/// <summary>Khớp ProductAPI ProductDto (camelCase qua ASP.NET).</summary>
public class ProductInfo
{
    [JsonPropertyName("productID")]
    public string ProductId { get; set; } = "";

    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = "";

    [JsonPropertyName("categoryName")]
    public string? CategoryName { get; set; }

    [JsonPropertyName("originalPrice")]
    public decimal OriginalPrice { get; set; }

    [JsonPropertyName("finalPrice")]
    public decimal FinalPrice { get; set; }

    /// <summary>Không có trong JSON — luôn tính từ FinalPrice/OriginalPrice (tránh 0 VNĐ do deserialize).</summary>
    [JsonIgnore]
    public decimal DisplayPrice => FinalPrice > 0 ? FinalPrice : OriginalPrice;
}

public class VoucherInfo
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";

    [JsonPropertyName("voucherName")]
    public string VoucherName { get; set; } = "";

    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }

    [JsonPropertyName("expiryDate")]
    public DateOnly? ExpiryDate { get; set; }

    [JsonPropertyName("discountPercentage")]
    public decimal? DiscountPercentage { get; set; }

    [JsonPropertyName("maxReducing")]
    public decimal? MaxReducing { get; set; }
}
