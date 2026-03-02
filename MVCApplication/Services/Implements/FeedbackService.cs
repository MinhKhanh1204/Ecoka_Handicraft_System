using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using MVCApplication.Models;

namespace MVCApplication.Services.Implements
{
    public class FeedbackService : IFeedbackService
    {
        private readonly HttpClient _http;

        public FeedbackService(HttpClient http)
        {
            _http = http;
        }

        public async Task<IEnumerable<Feedback>> GetAllAsync()
        {
            var response = await _http.GetAsync("/feedbacks");
            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<Feedback>();

            return await response.Content.ReadFromJsonAsync<IEnumerable<Feedback>>()
                   ?? Enumerable.Empty<Feedback>();
        }

        public async Task<Feedback?> GetByIdAsync(int feedbackId)
        {
            var response = await _http.GetAsync($"/feedbacks/{feedbackId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Feedback>();
        }

        public async Task<IEnumerable<Feedback>> FilterAsync(FeedbackFilterDto filter)
        {
            if (filter == null)
                return Enumerable.Empty<Feedback>();

            var q = new Dictionary<string, string?>();

            if (!string.IsNullOrWhiteSpace(filter.CustomerID))
                q["CustomerID"] = filter.CustomerID;
            if (!string.IsNullOrWhiteSpace(filter.ProductID))
                q["ProductID"] = filter.ProductID;
            if (filter.MinRating.HasValue)
                q["MinRating"] = filter.MinRating.Value.ToString();
            if (filter.MaxRating.HasValue)
                q["MaxRating"] = filter.MaxRating.Value.ToString();
            if (!string.IsNullOrWhiteSpace(filter.Status))
                q["Status"] = filter.Status;
            if (filter.From.HasValue)
                q["From"] = filter.From.Value.ToString("o");
            if (filter.To.HasValue)
                q["To"] = filter.To.Value.ToString("o");

            var uri = QueryHelpers.AddQueryString("/feedbacks/filter", q);
            var response = await _http.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<Feedback>();

            return await response.Content.ReadFromJsonAsync<IEnumerable<Feedback>>()
                   ?? Enumerable.Empty<Feedback>();
        }

        public async Task<Feedback> CreateAsync(FeedbackCreateDto dto)
        {
            var response = await _http.PostAsJsonAsync("/feedbacks", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Feedback>()
                   ?? throw new InvalidOperationException("Feedback creation returned null response");
        }

        public async Task<Feedback?> UpdateAsync(int feedbackId, FeedbackUpdateDto dto)
        {
            var response = await _http.PutAsJsonAsync($"/feedbacks/{feedbackId}", dto);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Feedback>();
        }

        public async Task<bool> DeleteAsync(int feedbackId)
        {
            var response = await _http.DeleteAsync($"/feedbacks/{feedbackId}");
            return response.IsSuccessStatusCode;
        }
    }
}
