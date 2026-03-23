using ChatbotAPI.DTOs;
using ChatbotAPI.Services;

namespace ChatbotAPI.Services.Implements
{
    public class PromptBuilderService : IPromptBuilderService
    {
        private const string BASE_PROMPT = @"Bạn là nhân viên chăm sóc khách hàng của cửa hàng thủ công mỹ nghệ Ecoka Handicraft. Bạn cần trả lời các câu hỏi của khách hàng một cách lịch sự, chuyên nghiệp và hữu ích.

# VỀ CỬA HÀNG:
- Ecoka Handicraft là cửa hàng chuyên bán các sản phẩm thủ công mỹ nghệ handmade
- Các sản phẩm bao gồm: đồ gốm, đồ thêu, tranh treo tường, đồ trang trí, quà tặng handmade, v.v.
- Cửa hàng có chính sách đổi trả hợp lý
- Hỗ trợ thanh toán qua VNPay, MoMo, thẻ ATM/Visa

# NGUYÊN TẮC TRẢ LỜI:
1. LUÔN trả lời bằng tiếng Việt
2. Thân thiện, nhiệt tình nhưng chuyên nghiệp
3. ĐỊNH DẠNG VĂN BẢN (BẮT BUỘC):
   - KHÔNG dùng Markdown: không dùng **, __, #, `, không dùng ký tự in đậm/in nghiêng kiểu Markdown
   - Khi liệt kê: mỗi mục một dòng, bắt đầu bằng dấu gạch ngang và khoảng trắng "" - "" (ví dụ: ""- Đồ gốm: mô tả..."") hoặc dấu ""• "" rồi nội dung
   - Xuống dòng giữa các đoạn và giữa các mục danh sách để dễ đọc
   - Viết như tin nhắn chat thuần văn bản, không dùng cú pháp định dạng của Markdown
4. Nếu khách hỏi về sản phẩm: Cung cấp thông tin chi tiết về giá, mô tả, chất liệu, tình trạng còn hàng
5. Nếu khách hỏi về đơn hàng: Kiểm tra thông tin đơn hàng và trả lời trạng thái
6. Nếu khách hỏi về giỏ hàng: Thông báo số lượng và tổng tiền
7. Nếu khách hỏi về voucher/khuyến mãi: Cung cấp thông tin về các mã giảm giá hiện có
8. Nếu câu hỏi KHÔNG liên quan đến cửa hàng, sản phẩm, đơn hàng, voucher hoặc dịch vụ của Ecoka:
   - Trả lời lịch sự: ""Xin lỗi, tôi chỉ có thể hỗ trợ bạn các câu hỏi liên quan đến sản phẩm, đơn hàng, giỏ hàng và voucher của Ecoka Handicraft. Bạn có câu hỏi nào khác về cửa hàng không?""
   - KHÔNG cung cấp thông tin về các chủ đề khác
   - KHÔNG giải thích hay giúp đỡ các công việc không liên quan

# THÔNG TIN HIỆN TẠI:";

        private const string CART_CONTEXT = @"
## THÔNG TIN GIỎ HÀNG:
{0}
";

        private const string ORDER_CONTEXT = @"
## ĐƠN HÀNG GẦN ĐÂY:
{0}
";

        private const string PRODUCTS_CONTEXT = @"
## SẢN PHẨM GỢI Ý:
{0}
";

        private const string VOUCHER_CONTEXT = @"
## VOUCHER/KHUYẾN MÃI HIỆN CÓ:
{0}
";

        private const string GREETING_SUGGESTIONS = @"## GỢI Ý CÂU HỎI:
- ""Sản phẩm nào đang được khuyến mãi?""
- ""Tôi muốn tìm sản phẩm thủ công mỹ nghệ""
- ""Kiểm tra đơn hàng của tôi""
- ""Xem giỏ hàng của tôi""
- ""Các voucher hiện có""";

        public string BuildSystemPrompt(ConversationContextDto context, string? customerName)
        {
            var prompt = BASE_PROMPT;

            if (!string.IsNullOrEmpty(customerName))
            {
                prompt += $"\n\nXin chào {customerName}! Tôi có thể giúp gì cho bạn hôm nay?";
            }

            if (context.CartItems.Any())
            {
                var cartInfo = string.Join("\n", context.CartItems.Select(item =>
                {
                    var label = string.IsNullOrWhiteSpace(item.ProductName)
                        ? $"Mã SP {item.ProductId}"
                        : item.ProductName;
                    var pricePart = item.Price > 0 ? $" — {item.Price:N0} VNĐ/đơn vị" : "";
                    return $"- {label}: SL {item.Quantity}{pricePart}";
                }));
                prompt += string.Format(CART_CONTEXT, cartInfo);
            }

            if (context.RecentOrders.Any())
            {
                var orderInfo = string.Join("\n", context.RecentOrders.Select(order =>
                {
                    var dateStr = order.OrderDate.HasValue ? order.OrderDate.Value.ToString("dd/MM/yyyy") : "—";
                    var total = order.TotalAmount.HasValue ? $"{order.TotalAmount:N0} VNĐ" : "—";
                    return $"- Đơn hàng #{order.OrderID}: {order.DisplayStatus} - {total} - Ngày {dateStr}";
                }));
                prompt += string.Format(ORDER_CONTEXT, orderInfo);
            }

            if (context.RecommendedProducts.Any())
            {
                var productInfo = string.Join("\n", context.RecommendedProducts.Select(p =>
                {
                    var price = p.DisplayPrice;
                    var orig = p.OriginalPrice > 0 && p.FinalPrice > 0 && p.OriginalPrice != p.FinalPrice
                        ? $" (giá gốc {p.OriginalPrice:N0} VNĐ)"
                        : "";
                    var cat = string.IsNullOrWhiteSpace(p.CategoryName) ? "—" : p.CategoryName;
                    var name = string.IsNullOrWhiteSpace(p.ProductName) ? "(Chưa có tên)" : p.ProductName;
                    return $"- {name}: {price:N0} VNĐ{orig} — {cat}";
                }));
                prompt += string.Format(PRODUCTS_CONTEXT, productInfo);
            }

            if (context.AvailableVouchers.Any())
            {
                var voucherInfo = string.Join("\n", context.AvailableVouchers.Select(v =>
                {
                    var exp = v.ExpiryDate?.ToString("dd/MM/yyyy") ?? "—";
                    var pct = v.DiscountPercentage.HasValue ? $"{v.DiscountPercentage.Value:0.##}%" : "—";
                    var max = v.MaxReducing.HasValue ? $"{v.MaxReducing.Value:N0} VNĐ" : "—";
                    var vname = string.IsNullOrWhiteSpace(v.VoucherName) ? v.Code : v.VoucherName;
                    return $"- {v.Code}: {vname} — Giảm {pct} (tối đa {max}) — Hết hạn {exp}";
                }));
                prompt += string.Format(VOUCHER_CONTEXT, voucherInfo);
            }

            prompt += "\n\n" + GREETING_SUGGESTIONS;

            return prompt;
        }
    }
}