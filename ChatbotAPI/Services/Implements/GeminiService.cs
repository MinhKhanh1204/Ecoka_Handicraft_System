using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using ChatbotAPI.DTOs;
using ChatbotAPI.Exceptions;
using ChatbotAPI.Models;
using ChatbotAPI.Services;

namespace ChatbotAPI.Services.Implements
{
    public class GeminiService : IGeminiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GeminiSettings _settings;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(
            IHttpClientFactory httpClientFactory,
            IOptions<GeminiSettings> settings,
            ILogger<GeminiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            _logger = logger;
        }

    public async Task<string> GenerateResponseAsync(string userMessage, string systemPrompt)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            _logger.LogError("[GeminiService] ApiKey is EMPTY. Check appsettings.json GeminiSettings:ApiKey or User Secrets.");
            throw new InvalidOperationException("Gemini API key is not configured. See GeminiService logs.");
        }

        _logger.LogInformation("[GeminiService] Calling Gemini. Model={Model}, ApiKey prefix={Prefix}, MsgLen={MsgLen}",
            _settings.Model, _settings.ApiKey[..Math.Min(8, _settings.ApiKey.Length)], userMessage.Length);

        var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = $"System: {systemPrompt}\n\nUser: {userMessage}" }
                    }
                }
            },
            generationConfig = new
            {
                temperature = _settings.Temperature,
                maxOutputTokens = _settings.MaxTokens,
                topP = 0.95,
                topK = 40
            }
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        // Use SecureClient — in Development it bypasses SSL validation (self-signed certs).
        // GeminiService is scoped, so CreateClient("SecureClient") is appropriate here.
        var client = _httpClientFactory.CreateClient("SecureClient");
        var response = await client.PostAsync(apiUrl, httpContent);
            var statusCode = (int)response.StatusCode;
            var rawBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[GeminiService] Gemini API HTTP {Status}. Body: {Body}", statusCode, rawBody);

                if (IsQuotaOrRateLimitError(statusCode, rawBody))
                {
                    _logger.LogWarning(
                        "[GeminiService] Quota/rate limit. Cùng một dự án Google Cloud dù bao nhiêu API key vẫn chung quota — cần project mới, bật billing, hoặc đợi reset. Model={Model}",
                        _settings.Model);
                    throw new GeminiRateLimitException(
                        "Gemini báo hết quota / giới hạn (HTTP " + statusCode + "). " +
                        "Đổi API key trong cùng dự án Google KHÔNG tăng quota. " +
                        "Cách xử lý: tạo dự án Google Cloud mới + API key mới, bật Generative Language API, " +
                        "hoặc bật billing / chờ reset quota theo ngày. " +
                        "Chi tiết: https://ai.google.dev/gemini-api/docs/rate-limits");
                }

                if (TryGetAccessDeniedMessage(statusCode, rawBody, out var deniedMsg))
                    throw new GeminiAccessDeniedException(deniedMsg);

                if (statusCode == 404)
                    throw new InvalidOperationException(
                        "Model Gemini không tồn tại hoặc đã ngừng hỗ trợ (HTTP 404). Đổi GeminiSettings:Model — dùng model ổn định: gemini-2.5-flash-lite hoặc gemini-2.5-flash (gemini-2.0-flash đã deprecated).");

                throw new InvalidOperationException(
                    $"Gemini trả lỗi HTTP {statusCode}. Kiểm tra API key và bật Generative Language API trên Google Cloud.");
            }

            _logger.LogInformation("[GeminiService] Gemini returned HTTP {Status}, body len={Len}", statusCode, rawBody.Length);

            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    var text = parts[0].GetProperty("text").GetString() ?? "";
                    text = NormalizeAssistantPlainText(text);
                    _logger.LogInformation("[GeminiService] Gemini text response length={Len}", text.Length);
                    return text;
                }
            }

            return "No response generated";
        }

        public async Task<GeminiConnectionTestDto> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            var dto = new GeminiConnectionTestDto { Model = _settings.Model };
            try
            {
                var text = await GenerateResponseAsync("ping", "Trả lời đúng một từ: OK");
                dto.Ok = true;
                dto.Category = "ok";
                dto.ReplyPreview = text.Length > 160 ? text[..160] + "…" : text;
                dto.NextStepVi = "Gemini phản hồi bình thường. Nếu /message vẫn fallback, kiểm tra log khi gọi prompt dài (context).";
                return dto;
            }
            catch (GeminiRateLimitException ex)
            {
                dto.Ok = false;
                dto.Category = "quota";
                dto.HttpStatus = 429;
                dto.GeminiMessage = ex.Message;
                dto.NextStepVi =
                    "Quota/rate limit: bật billing cho Google Cloud, đợi reset quota free, hoặc đổi model (vd. gemini-2.5-flash-lite). Chi tiết: docs/GEMINI_AI_SETUP_PLAN.md";
                return dto;
            }
            catch (GeminiAccessDeniedException ex)
            {
                dto.Ok = false;
                dto.Category = "suspended_or_forbidden";
                dto.HttpStatus = 403;
                dto.GeminiMessage = ex.Message;
                dto.NextStepVi =
                    "Key/dự án bị Google khóa (CONSUMER_SUSPENDED) hoặc không đủ quyền. Tạo project + key mới tại https://aistudio.google.com/apikey — không sửa được bằng code.";
                return dto;
            }
            catch (InvalidOperationException ex)
            {
                dto.Ok = false;
                dto.Category = "config";
                dto.GeminiMessage = ex.Message;
                dto.NextStepVi = "Điền GeminiSettings:ApiKey (appsettings hoặc dotnet user-secrets).";
                return dto;
            }
            catch (Exception ex)
            {
                dto.Ok = false;
                FillDtoFromGeminiHttpFailure(dto, ex.Message);
                if (string.IsNullOrWhiteSpace(dto.GeminiMessage))
                    dto.GeminiMessage = ex.Message.Length > 500 ? ex.Message[..500] + "…" : ex.Message;
                return dto;
            }
        }

        private static void FillDtoFromGeminiHttpFailure(GeminiConnectionTestDto dto, string message)
        {
            var retIdx = message.IndexOf("returned ", StringComparison.Ordinal);
            if (retIdx >= 0)
            {
                var after = message[(retIdx + "returned ".Length)..];
                var dot = after.IndexOf('.');
                if (dot > 0 && int.TryParse(after[..dot], out var httpCode))
                    dto.HttpStatus = httpCode;
            }

            const string bodyMarker = "Body: ";
            var idx = message.IndexOf(bodyMarker, StringComparison.Ordinal);
            if (idx >= 0)
            {
                var body = message[(idx + bodyMarker.Length)..].Trim();
                if (body.Length > 900) body = body[..900] + "…";
                dto.RawErrorSnippet = body;
                TryParseGeminiJsonErrorBody(body, dto);
            }

            var gm = dto.GeminiMessage ?? "";
            var status = dto.HttpStatus;
            if (status == 404) dto.Category = "model_not_found";
            else if (status == 403 && (gm.Contains("API key", StringComparison.OrdinalIgnoreCase) ||
                                     gm.Contains("invalid", StringComparison.OrdinalIgnoreCase)))
                dto.Category = "invalid_key";
            else if (status == 403) dto.Category = "permission";
            else if (status == 400) dto.Category = "bad_request";
            else dto.Category = "unknown";

            dto.NextStepVi = HintVi(dto.Category, status);
        }

        private static void TryParseGeminiJsonErrorBody(string body, GeminiConnectionTestDto dto)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (!doc.RootElement.TryGetProperty("error", out var err)) return;
                if (err.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                    dto.GeminiMessage = m.GetString();
            }
            catch
            {
                /* ignore */
            }
        }

        private static string HintVi(string category, int? httpStatus) => category switch
        {
            "model_not_found" =>
                "Tên model sai hoặc model cũ đã tắt. Đổi GeminiSettings:Model (vd. gemini-2.5-flash-lite, gemini-2.5-flash). Xem danh sách tại https://ai.google.dev/gemini-api/docs/models/gemini",
            "invalid_key" =>
                "Key không hợp lệ. Tạo key mới tại https://aistudio.google.com/apikey và dán vào cấu hình.",
            "permission" =>
                "Bật API Generative Language (Google Cloud) cho project. Tạm thời đặt Application restrictions = None cho key để test từ localhost.",
            "bad_request" =>
                "Request không hợp lệ — thường do tên model hoặc tham số. Đổi model trong appsettings.",
            _ => httpStatus is int s
                ? $"HTTP {s}. Đọc trường rawErrorSnippet và file docs/GEMINI_AI_SETUP_PLAN.md."
                : "Đọc log ChatbotAPI và docs/GEMINI_AI_SETUP_PLAN.md."
        };

        /// <summary>401/403 — key bị khóa, CONSUMER_SUSPENDED, API không bật, v.v.</summary>
        private static bool TryGetAccessDeniedMessage(int statusCode, string rawBody, out string message)
        {
            message = "";
            if (statusCode != 401 && statusCode != 403) return false;

            var body = rawBody ?? "";
            if (body.Contains("CONSUMER_SUSPENDED", StringComparison.OrdinalIgnoreCase) ||
                body.Contains("has been suspended", StringComparison.OrdinalIgnoreCase))
            {
                message =
                    "Google đã tạm khóa API key hoặc dự án (CONSUMER_SUSPENDED). " +
                    "Đây không phải lỗi ứng dụng — cần tạo Google Cloud project mới + API key mới tại https://aistudio.google.com/apikey " +
                    "hoặc kiểm tra email/Google Cloud (thanh toán, vi phạm điều khoản).";
                return true;
            }

            if (body.Contains("API_KEY_INVALID", StringComparison.OrdinalIgnoreCase) ||
                body.Contains("API key not valid", StringComparison.OrdinalIgnoreCase))
            {
                message =
                    "API key Gemini không hợp lệ hoặc đã bị xóa. Tạo key mới tại https://aistudio.google.com/apikey và cập nhật GeminiSettings:ApiKey.";
                return true;
            }

            if (body.Contains("PERMISSION_DENIED", StringComparison.OrdinalIgnoreCase) &&
                !body.Contains("RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase))
            {
                message =
                    "Google từ chối quyền (PERMISSION_DENIED). Bật Generative Language API cho project; tạm thời đặt Application restrictions = None cho key khi test localhost.";
                return true;
            }

            message =
                "Gemini từ chối truy cập (HTTP " + statusCode + "). Kiểm tra API key, bật Generative Language API trên Google Cloud Console.";
            return true;
        }

        /// <summary>
        /// Google có thể trả 429, hoặc 403/503 kèm JSON error.status = RESOURCE_EXHAUSTED.
        /// </summary>
        private static bool IsQuotaOrRateLimitError(int statusCode, string rawBody)
        {
            if (statusCode == 429) return true;
            if (string.IsNullOrWhiteSpace(rawBody)) return false;
            try
            {
                using var doc = JsonDocument.Parse(rawBody);
                if (!doc.RootElement.TryGetProperty("error", out var err)) return false;
                if (err.TryGetProperty("status", out var st))
                {
                    var status = st.GetString();
                    if (string.Equals(status, "RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                if (err.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.Number && code.GetInt32() == 429)
                    return true;
                if (err.TryGetProperty("message", out var msg))
                {
                    var m = msg.GetString() ?? "";
                    if (m.Contains("quota", StringComparison.OrdinalIgnoreCase) ||
                        m.Contains("Quota exceeded", StringComparison.OrdinalIgnoreCase) ||
                        m.Contains("rate limit", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            catch
            {
                /* ignore parse errors */
            }
            return false;
        }

        /// <summary>
        /// Removes common Markdown markers and normalizes line breaks so chat UIs show clear paragraphs and lists.
        /// </summary>
        private static string NormalizeAssistantPlainText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            var s = text.Replace("\r\n", "\n", StringComparison.Ordinal);
            s = s.Replace("\\n", "\n", StringComparison.Ordinal);

            s = s.Replace("**", "", StringComparison.Ordinal);
            s = s.Replace("__", "", StringComparison.Ordinal);

            // List items run together: "intro * *Label:** rest" → break before bullet
            s = Regex.Replace(s, @"(\S)\s*\*\s+(?=\S)", m => m.Groups[1].Value + "\n• ");

            s = Regex.Replace(s, @"^\*\s+", "• ", RegexOptions.Multiline);

            s = Regex.Replace(s, @"\n{3,}", "\n\n");
            return s.Trim();
        }
    }
}
