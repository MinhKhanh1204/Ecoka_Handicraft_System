using Microsoft.EntityFrameworkCore;
using VoucherAPI.Admin.DTOs;
using VoucherAPI.Admin.Repositories;
using VoucherAPI.CustomFormatter;
using VoucherAPI.Models;

namespace VoucherAPI.Admin.Services.Implements
{
    public class VoucherAdminService : IVoucherAdminService
    {
        private readonly IVoucherAdminRepository _repository;

        public VoucherAdminService(IVoucherAdminRepository repository)
        {
            _repository = repository;
        }

        public Task<PagedResult<VoucherListDto>> GetPagedAsync(string? keyword, string? status, string? sortBy, int pageNumber, int pageSize)
        {
            var query = _repository.GetQueryable();

            // Filter by keyword
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim().ToLower();
                query = query.Where(v =>
                    (v.VoucherName != null && EF.Functions.Like(v.VoucherName, $"%{k}%")) ||
                    (v.Code != null && EF.Functions.Like(v.Code, $"%{k}%")));
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var isActive = status.Equals("Active", StringComparison.OrdinalIgnoreCase);
                query = query.Where(v => v.IsActive == isActive);
            }

            // Sort
            query = sortBy?.ToLower() switch
            {
                "name_asc" => query.OrderBy(v => v.VoucherName),
                "name_desc" => query.OrderByDescending(v => v.VoucherName),
                "code_asc" => query.OrderBy(v => v.Code),
                "code_desc" => query.OrderByDescending(v => v.Code),
                "expiry_asc" => query.OrderBy(v => v.ExpiryDate),
                "expiry_desc" => query.OrderByDescending(v => v.ExpiryDate),
                "discount_desc" => query.OrderByDescending(v => v.DiscountPercentage),
                "discount_asc" => query.OrderBy(v => v.DiscountPercentage),
                "id_desc" => query.OrderByDescending(v => v.VoucherId),
                _ => query.OrderBy(v => v.VoucherId)
            };

            var totalCount = query.Count();
            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new VoucherListDto
                {
                    VoucherId = v.VoucherId,
                    VoucherName = v.VoucherName,
                    Code = v.Code,
                    DiscountPercentage = v.DiscountPercentage,
                    MaxReducing = v.MaxReducing,
                    Quantity = v.Quantity,
                    ExpiryDate = v.ExpiryDate,
                    IsActive = v.IsActive
                })
                .ToList();

            return Task.FromResult(PagedResult<VoucherListDto>.Create(items, totalCount, pageNumber, pageSize));
        }

        public async Task<VoucherDetailDto?> GetByIdAsync(int id)
        {
            var v = await _repository.GetByIdAsync(id);
            if (v == null) return null;

            return new VoucherDetailDto
            {
                VoucherId = v.VoucherId,
                VoucherName = v.VoucherName,
                Code = v.Code,
                Description = v.Description,
                DiscountPercentage = v.DiscountPercentage,
                MaxReducing = v.MaxReducing,
                Quantity = v.Quantity,
                UsageCount = v.UsageCount,
                ExpiryDate = v.ExpiryDate,
                IsActive = v.IsActive,
                MinOrderValue = v.MinOrderValue,
                MaxUsagePerUser = v.MaxUsagePerUser
            };
        }

        public async Task<int> CreateAsync(CreateVoucherDto dto, bool createdByAdmin)
        {
            var code = dto.Code?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(code))
                throw new InvalidOperationException("Voucher code is required.");

            var existing = await _repository.GetByCodeAsync(code);
            if (existing != null)
                throw new InvalidOperationException($"Voucher code '{code}' already exists.");

            var voucher = new Voucher
            {
                VoucherName = dto.VoucherName?.Trim(),
                Code = code,
                Description = dto.Description,
                DiscountPercentage = dto.DiscountPercentage,
                MaxReducing = dto.MaxReducing,
                Quantity = dto.Quantity,
                UsageCount = 0,
                ExpiryDate = dto.ExpiryDate,
                IsActive = createdByAdmin ? dto.IsActive : false,
                MinOrderValue = dto.MinOrderValue,
                MaxUsagePerUser = dto.MaxUsagePerUser
            };

            var created = await _repository.AddAsync(voucher);
            return created.VoucherId;
        }

        public async Task<bool> ApproveAsync(int id)
        {
            var voucher = await _repository.GetByIdAsync(id);
            if (voucher == null) return false;

            voucher.IsActive = true;
            await _repository.UpdateAsync(voucher);
            return true;
        }

        public async Task<bool> UpdateAsync(int id, UpdateVoucherDto dto)
        {
            var voucher = await _repository.GetByIdAsync(id);
            if (voucher == null) return false;

            if (voucher.UsageCount.HasValue && dto.Quantity < voucher.UsageCount.Value)
            {
                throw new InvalidOperationException(
                    $"Quantity cannot be less than the number of vouchers already used ({voucher.UsageCount.Value}). Please set quantity to at least {voucher.UsageCount.Value}.");
            }

            voucher.VoucherName = dto.VoucherName;
            voucher.Description = dto.Description;
            voucher.DiscountPercentage = dto.DiscountPercentage;
            voucher.MaxReducing = dto.MaxReducing;
            voucher.Quantity = dto.Quantity;
            voucher.ExpiryDate = dto.ExpiryDate;
            voucher.IsActive = dto.IsActive;
            voucher.MinOrderValue = dto.MinOrderValue;
            voucher.MaxUsagePerUser = dto.MaxUsagePerUser;

            await _repository.UpdateAsync(voucher);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var voucher = await _repository.GetByIdAsync(id);
            if (voucher == null) return false;

            await _repository.DeleteAsync(voucher);
            return true;
        }
    }
}
