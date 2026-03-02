using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using FeedbackAPI.DTOs;
using FeedbackAPI.Hubs;
using FeedbackAPI.Models;
using FeedbackAPI.Repositories;

namespace FeedbackAPI.Services.Implements
{
    public class FeedbackReplyService : IFeedbackReplyService
    {
        private readonly IFeedbackReplyRepository _repo;
        private readonly IMapper _mapper;
        private readonly IHubContext<FeedbackHub> _hub;

        public FeedbackReplyService(
            IFeedbackReplyRepository repo,
            IMapper mapper,
            IHubContext<FeedbackHub> hub)
        {
            _repo = repo;
            _mapper = mapper;
            _hub = hub;
        }

        public async Task<IEnumerable<FeedbackReplyReadDto>> GetByFeedbackIdAsync(int feedbackId)
        {
            var replies = await _repo.GetByFeedbackIdAsync(feedbackId);
            return _mapper.Map<IEnumerable<FeedbackReplyReadDto>>(replies);
        }

        public async Task<FeedbackReplyReadDto?> GetByIdAsync(int replyId)
        {
            var reply = await _repo.GetByIdAsync(replyId);
            return reply == null ? null : _mapper.Map<FeedbackReplyReadDto>(reply);
        }

        public async Task<FeedbackReplyReadDto> CreateAsync(int feedbackId, FeedbackReplyCreateDto dto)
        {
            var entity = _mapper.Map<FeedbackReply>(dto);
            var created = await _repo.CreateAsync(feedbackId, entity);
            var result = _mapper.Map<FeedbackReplyReadDto>(created);

            // Notify clients watching this feedback thread
            await _hub.Clients
                .Group($"feedback_{feedbackId}")
                .SendAsync("ReplyCreated", result);

            return result;
        }

        public async Task<FeedbackReplyReadDto?> UpdateAsync(int replyId, FeedbackReplyUpdateDto dto)
        {
            var updatedData = _mapper.Map<FeedbackReply>(dto);
            var entity = await _repo.UpdateAsync(replyId, updatedData);
            if (entity == null) return null;

            var result = _mapper.Map<FeedbackReplyReadDto>(entity);

            await _hub.Clients
                .Group($"feedback_{entity.FeedbackID}")
                .SendAsync("ReplyUpdated", result);

            return result;
        }

        public async Task<bool> DeleteAsync(int replyId)
        {
            // Fetch once — reuse for both SignalR broadcast and the actual delete
            var existing = await _repo.GetByIdAsync(replyId);
            if (existing == null) return false;

            await _repo.DeleteEntityAsync(existing);

            await _hub.Clients
                .Group($"feedback_{existing.FeedbackID}")
                .SendAsync("ReplyDeleted", replyId);

            return true;
        }
    }
}
