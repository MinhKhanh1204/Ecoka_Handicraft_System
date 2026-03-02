using AutoMapper;
using FeedbackAPI.DTOs;
using FeedbackAPI.Models;
using FeedbackAPI.Repositories;

namespace FeedbackAPI.Services.Implements
{
    public class FeedbackReplyService : IFeedbackReplyService
    {
        private readonly IFeedbackReplyRepository _repo;
        private readonly IMapper _mapper;

        public FeedbackReplyService(IFeedbackReplyRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
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
            return _mapper.Map<FeedbackReplyReadDto>(created);
        }

        public async Task<FeedbackReplyReadDto?> UpdateAsync(int replyId, FeedbackReplyUpdateDto dto)
        {
            var updatedData = _mapper.Map<FeedbackReply>(dto);
            var result = await _repo.UpdateAsync(replyId, updatedData);
            return result == null ? null : _mapper.Map<FeedbackReplyReadDto>(result);
        }

        public Task<bool> DeleteAsync(int replyId)
            => _repo.DeleteAsync(replyId);
    }
}
