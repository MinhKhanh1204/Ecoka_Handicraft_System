using Microsoft.AspNetCore.Mvc;
using PaymentAPI.DTOs;
using PaymentAPI.Utils;
using System.Net.Http.Json;

namespace PaymentAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public PaymentController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("create-vnpay-url")]
        public IActionResult CreateVnPayUrl([FromBody] PaymentRequestDto request)
        {
            var vnpay = new VnPayLibrary();
            var config = _configuration.GetSection("VNPay");

            string ipAddr = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            if (ipAddr == "::1" || ipAddr.StartsWith("::ffff:")) ipAddr = "127.0.0.1";

            var vnpTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vnpTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnpTimeZone);

            vnpay.AddRequestData("vnp_Version", config["Version"] ?? "2.1.0");
            vnpay.AddRequestData("vnp_Command", config["Command"] ?? "pay");
            vnpay.AddRequestData("vnp_TmnCode", config["TmnCode"]!);
            vnpay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", vnpTime.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddr);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", request.OrderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", config["CallbackUrl"] ?? request.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", request.OrderId);

            string paymentUrl = vnpay.CreateRequestUrl(config["BaseUrl"]!, config["HashSecret"]!);

            return Ok(new PaymentResponseDto { Success = true, PaymentUrl = paymentUrl });
        }

        [HttpPost("create-momo-url")]
        public async Task<IActionResult> CreateMomoUrl([FromBody] PaymentRequestDto request)
        {
            var momo = new MomoLibrary();
            var config = _configuration.GetSection("Momo");

            string endpoint = config["ApiUrl"]!;
            string partnerCode = config["PartnerCode"]!;
            string accessKey = config["AccessKey"]!;
            string secretKey = config["SecretKey"]!;
            string orderInfo = request.OrderInfo;
            string redirectUrl = !string.IsNullOrEmpty(request.ReturnUrl) ? request.ReturnUrl : config["RedirectUrl"]!;
            string ipnUrl = config["IpnUrl"]!;
            string requestType = "captureWallet";

            string amount = ((long)request.Amount).ToString();
            string orderId = request.OrderId + "_" + DateTime.Now.Ticks; 
            string requestId = Guid.NewGuid().ToString();
            string extraData = "";

            // accessKey=$accessKey&amount=$amount&extraData=$extraData&ipnUrl=$ipnUrl&orderId=$orderId&orderInfo=$orderInfo&partnerCode=$partnerCode&redirectUrl=$redirectUrl&requestId=$requestId&requestType=$requestType
            string rawHash = "accessKey=" + accessKey +
                "&amount=" + amount +
                "&extraData=" + extraData +
                "&ipnUrl=" + ipnUrl +
                "&orderId=" + orderId +
                "&orderInfo=" + orderInfo +
                "&partnerCode=" + partnerCode +
                "&redirectUrl=" + redirectUrl +
                "&requestId=" + requestId +
                "&requestType=" + requestType;

            string signature = momo.CreateSignature(secretKey, rawHash);

            var message = new
            {
                partnerCode,
                partnerName = "Ecoka Handicraft",
                storeId = "EcokaStore",
                requestId,
                amount = (long)request.Amount,
                orderId,
                orderInfo,
                redirectUrl,
                ipnUrl,
                lang = "vi",
                extraData,
                requestType,
                signature
            };

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(endpoint, message);
            var responseData = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();

            if (response.IsSuccessStatusCode && responseData.ValueKind != System.Text.Json.JsonValueKind.Undefined)
            {
                if (responseData.TryGetProperty("payUrl", out var payUrlElement))
                {
                    string? payUrl = payUrlElement.GetString();
                    if (!string.IsNullOrEmpty(payUrl))
                    {
                        return Ok(new PaymentResponseDto { Success = true, PaymentUrl = payUrl });
                    }
                }
            }

            return BadRequest(new PaymentResponseDto { Success = false, Message = "Failed to create MoMo payment URL" });
        }

        [HttpPost("momo-ipn")]
        public async Task<IActionResult> MomoIpn([FromBody] System.Text.Json.JsonElement data)
        {
            var config = _configuration.GetSection("Momo");
            string secretKey = config["SecretKey"]!;
            string accessKey = config["AccessKey"]!;

            // Verify signature
            string partnerCode = data.GetProperty("partnerCode").GetString()!;
            string orderIdRaw = data.GetProperty("orderId").GetString()!;
            string requestId = data.GetProperty("requestId").GetString()!;
            string amount = data.GetProperty("amount").GetRawText()!;
            string orderInfo = data.GetProperty("orderInfo").GetString()!;
            string orderType = data.GetProperty("orderType").GetString()!;
            string transId = data.GetProperty("transId").GetRawText()!;
            string resultCode = data.GetProperty("resultCode").GetRawText()!;
            string message = data.GetProperty("message").GetString()!;
            string payType = data.GetProperty("payType").GetString()!;
            string responseTime = data.GetProperty("responseTime").GetRawText()!;
            string extraData = data.GetProperty("extraData").GetString()!;
            string signatureReceived = data.GetProperty("signature").GetString()!;

            // rawHash for IPN: accessKey=$accessKey&amount=$amount&extraData=$extraData&message=$message&orderId=$orderId&orderInfo=$orderInfo&partnerCode=$partnerCode&requestId=$requestId&responseTime=$responseTime&resultCode=$resultCode&transId=$transId
            string rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&message={message}&orderId={orderIdRaw}&orderInfo={orderInfo}&partnerCode={partnerCode}&requestId={requestId}&responseTime={responseTime}&resultCode={resultCode}&transId={transId}";

            var momo = new MomoLibrary();
            string signatureComputed = momo.CreateSignature(secretKey, rawHash);

            if (signatureComputed.Equals(signatureReceived, StringComparison.InvariantCultureIgnoreCase))
            {
                if (resultCode == "0")
                {
                    string orderId = orderIdRaw.Split('_')[0];
                    await UpdateOrderStatus(orderId, "Paid", "MoMo");
                }
                return NoContent();
            }

            return BadRequest("Invalid signature");
        }

        [HttpGet("vnpay-callback")]
        public async Task<IActionResult> VnPayCallback()
        {
            var config = _configuration.GetSection("VNPay");
            var vnpay = new VnPayLibrary();

            foreach (var key in Request.Query.Keys)
            {
                var value = Request.Query[key].ToString();
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value);
                }
            }

            string orderId = vnpay.GetResponseData("vnp_TxnRef");
            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
            string vnp_SecureHash = Request.Query["vnp_SecureHash"]!;

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, config["HashSecret"]!);
            if (checkSignature)
            {
                if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                {
                    await UpdateOrderStatus(orderId, "Paid", "VNPay");
                }
            }

            // Redirect back to MVC frontend
            string redirectUrl = _configuration["Momo:RedirectUrl"] ?? "https://localhost:7010/Orders/PaymentCallback";
            return Redirect(redirectUrl + Request.QueryString);
        }

        private async Task UpdateOrderStatus(string orderId, string status, string paymentMethod)
        {
            try 
            {
                var client = _httpClientFactory.CreateClient();
                string url = _configuration["OrderApiUrl"] + orderId + "/payment-status";
                await client.PutAsJsonAsync(url, new { Status = status, PaymentMethod = paymentMethod });
            }
            catch (Exception)
            {
                // Log error (should use ILogger in production)
            }
        }
    }
}
