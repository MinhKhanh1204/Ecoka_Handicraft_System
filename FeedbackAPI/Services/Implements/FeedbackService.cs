using AutoMapper;
using FeedbackAPI.DTOs;
using FeedbackAPI.Models;
using FeedbackAPI.Repositories;

namespace FeedbackAPI.Services.Implements
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repo;
        private readonly IMapper _mapper;

        public FeedbackService(IFeedbackRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // ================= READ =================

        public async Task<IEnumerable<FeedbackReadDto>> GetAllAsync()
        {
            var feedbacks = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<FeedbackReadDto>>(feedbacks);
        }

        public async Task<FeedbackReadDto?> GetByIdAsync(int feedbackId)
        {
            var feedback = await _repo.GetByIdAsync(feedbackId);
            return feedback == null ? null : _mapper.Map<FeedbackReadDto>(feedback);
        }

        // ================= FILTER =================

        public async Task<IEnumerable<FeedbackReadDto>> FilterAsync(FeedbackFilterDto filter)
        {
            var feedbacks = await _repo.FilterAsync(
                filter.CustomerID,
                filter.ProductID,
                filter.OrderID,
                filter.MinRating,
                filter.MaxRating,
                filter.Status,
                filter.From,
                filter.To);

            return _mapper.Map<IEnumerable<FeedbackReadDto>>(feedbacks);
        }

        // ================= CREATE =================

        public async Task<FeedbackReadDto> CreateAsync(FeedbackCreateDto dto)
        {
            var entity = _mapper.Map<Feedback>(dto);
            var created = await _repo.CreateAsync(entity);
            return _mapper.Map<FeedbackReadDto>(created);
        }

        // ================= UPDATE =================

        public async Task<FeedbackReadDto?> UpdateAsync(int feedbackId, FeedbackUpdateDto dto)
        {
            var updatedData = _mapper.Map<Feedback>(dto);
            var result = await _repo.UpdateAsync(feedbackId, updatedData);
            return result == null ? null : _mapper.Map<FeedbackReadDto>(result);
        }

        // ================= DELETE =================

        public Task<bool> DeleteAsync(int feedbackId)
            => _repo.DeleteAsync(feedbackId);
    }
}
