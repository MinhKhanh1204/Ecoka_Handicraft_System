using MVCApplication.Areas.Admin.DTOs;

namespace MVCApplication.Areas.Admin.ViewModels
{
    /// <summary>
    /// Dữ liệu hiển thị khối hồ sơ trên sidebar admin (từ API + fallback từ JWT).
    /// </summary>
    public class AdminProfileSidebarViewModel
    {
        public StaffDetailViewModel? Staff { get; set; }
        public string FallbackUsername { get; set; } = "";
        public IReadOnlyList<string> FallbackRoles { get; set; } = Array.Empty<string>();
        public string? AccountId { get; set; }
    }
}
