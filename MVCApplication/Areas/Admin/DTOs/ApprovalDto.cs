using MVCApplication.Areas.Admin.DTOs;
using MVCApplication.CustomFormatter;
using MVCApplication.Models.DTOs;

namespace MVCApplication.Areas.Admin.DTOs
{
    public class ApprovalDto
    {
        public PagedResult<ReadProductDto> PendingProducts { get; set; } = new();
        public List<ReadCategoryDto> PendingCategories { get; set; } = new();
        public List<VoucherDto> PendingVouchers { get; set; } = new();
    }
}