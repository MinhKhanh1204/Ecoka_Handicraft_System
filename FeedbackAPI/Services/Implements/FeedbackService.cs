using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using FeedbackAPI.DTOs;
using FeedbackAPI.Hubs;
using FeedbackAPI.Models;
using FeedbackAPI.Repositories;

namespace FeedbackAPI.Services.Implements
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repo;
        private readonly IMapper _mapper;
        private readonly IHubContext<FeedbackHub> _hub;
        private readonly IAccountService _accountService;

        public FeedbackService(
            IFeedbackRepository repo,
            IMapper mapper,
            IHubContext<FeedbackHub> hub,
            IAccountService accountService)
        {
            _repo = repo;
            _mapper = mapper;
            _hub = hub;
            _accountService = accountService;
        }

        // ================= READ =================

        public async Task<IEnumerable<FeedbackReadDto>> GetAllAsync()
        {
            var feedbacks = await _repo.GetAllAsync();
            var dtos = _mapper.Map<List<FeedbackReadDto>>(feedbacks);

            foreach (var dto in dtos)
            {
                dto.Username = await _accountService.GetUsernameAsync(dto.CustomerID!);
            }

            return dtos;
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
                filter.MinRating,
                filter.MaxRating,
                filter.Status,
                filter.From,
                filter.To);

            var result = _mapper.Map<List<FeedbackReadDto>>(feedbacks);

            foreach (var fb in result)
            {
                fb.Username = await _accountService.GetUsernameAsync(fb.CustomerID!);
            }

            return result;
        }

        // ================= CREATE =================

        public async Task<FeedbackReadDto> CreateAsync(FeedbackCreateDto dto)
        {
            var existed = await _repo.ExistsAsync(dto.CustomerID, dto.ProductID);

            if (existed)
            {
                throw new Exception("You have already reviewed this product.");
            }

            var entity = _mapper.Map<Feedback>(dto);
            var created = await _repo.CreateAsync(entity);
            var result = _mapper.Map<FeedbackReadDto>(created);

            // SignalR notify
            await _hub.Clients
                .Group($"product_{result.ProductID}")
                .SendAsync("FeedbackCreated", result);

            return result;
        }

        // ================= UPDATE =================

        public async Task<FeedbackReadDto?> UpdateAsync(int feedbackId, FeedbackUpdateDto dto)
        {
            var updatedData = _mapper.Map<Feedback>(dto);
            var entity = await _repo.UpdateAsync(feedbackId, updatedData);
            if (entity == null) return null;

            var result = _mapper.Map<FeedbackReadDto>(entity);

            await _hub.Clients
                .Group($"product_{result.ProductID}")
                .SendAsync("FeedbackUpdated", result);

            return result;
        }

        // ================= DELETE =================

        public async Task<bool> DeleteAsync(int feedbackId)
        {
            // Fetch once — reuse for both SignalR broadcast and the actual delete
            var existing = await _repo.GetByIdAsync(feedbackId);
            if (existing == null) return false;

            await _repo.DeleteEntityAsync(existing);

            await _hub.Clients
                .Group($"product_{existing.ProductID}")
                .SendAsync("FeedbackDeleted", feedbackId);

            return true;
        }
    }
}
