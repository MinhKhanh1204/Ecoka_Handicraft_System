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
        private readonly IAccountService _accountService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IOrderService _orderService;

        public FeedbackService(
            IFeedbackRepository repo,
            IMapper mapper,
            IAccountService accountService,
            ICloudinaryService cloudinaryService,
            IOrderService orderService)
        {
            _repo = repo;
            _mapper = mapper;
            _accountService = accountService;
            _cloudinaryService = cloudinaryService;
            _orderService = orderService;
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

        public async Task<IEnumerable<FeedbackReadDto>> FilterAsync(FeedbackFilterDto filter
        )
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

        public async Task
        <
            FeedbackReadDto
        > CreateAsync(FeedbackCreateDto dto)
        {
            var existed = await _repo.ExistsAsync(dto.CustomerID, dto.ProductID);

            if (existed)
            {
                throw new Exception("You have already reviewed this product.");
            }

            // Check if purchased
            var hasPurchased = await _orderService.HasPurchasedAsync(dto.ProductID, dto.CustomerID);
            if (!hasPurchased)
            {
                throw new Exception("You must purchase this product before leaving a review.");
            }

            var entity = _mapper.Map<Feedback>(dto);

            // Handle Image Uploads
            if (dto.Images != null && dto.Images.Any())
            {
                foreach (var file in dto.Images)
                {
                    var url = await _cloudinaryService.UploadImageAsync(file);
                    if (!string.IsNullOrEmpty(url))
                    {
                        entity.FeedbackImages.Add(new FeedbackImage
                        {
                            ImageURL = url
                        });
                    }
                }
            }

            var created = await _repo.CreateAsync(entity);
            var result = _mapper.Map<FeedbackReadDto>(created);

            return result;
        }

        // ================= UPDATE =================

        public async Task<FeedbackReadDto?> UpdateAsync(int feedbackId, FeedbackUpdateDto dto)
        {
            var updatedData = _mapper.Map<Feedback>(dto);

            // Handle Image Uploads for Update (Append new images)
            if (dto.Images != null && dto.Images.Any())
            {
                foreach (var file in dto.Images)
                {
                    var url = await _cloudinaryService.UploadImageAsync(file);
                    if (!string.IsNullOrEmpty(url))
                    {
                        updatedData.FeedbackImages.Add(new FeedbackImage
                        {
                            ImageURL = url
                        });
                    }
                }
            }

            var entity = await _repo.UpdateAsync(feedbackId, updatedData);
            if (entity == null) return null;

            // If we added new images, we need to save them. 
            // The repo's UpdateAsync currently only updates base properties.
            // Let's modify Repo.UpdateAsync to handle the images collection.

            var result = _mapper.Map<FeedbackReadDto>(entity);
            return result;
        }

        // ================= DELETE =================

        public async Task<bool> DeleteAsync(int feedbackId)
        {
            // Fetch once — reuse for both SignalR broadcast and the actual delete
            var existing = await _repo.GetByIdAsync(feedbackId);
            if (existing == null) return false;

            await _repo.DeleteEntityAsync(existing);

            return true;
        }
    }
}
