using System.Collections.Generic;

namespace MVCApplication.Areas.Admin.DTOs
{
    public class DashboardStats
    {
        public IEnumerable<dynamic> RevenueData { get; set; }

        public IEnumerable<dynamic> OrdersByMonth { get; set; }

        public int SelectedRevenueYear { get; set; }

        public int TotalOrders { get; set; }

        public long TotalItemsSold { get; set; }

        public int PositiveFeedbackCount { get; set; }

        public int NegativeFeedbackCount { get; set; }

        public int ProductPendingCount { get; set; }

        public int VoucherApprovalCount { get; set; }

        public int CategoryApprovalCount { get; set; }


        public DashboardStats(
            IEnumerable<dynamic> RevenueData,
            IEnumerable<dynamic> OrdersByMonth,
            int SelectedRevenueYear,
            int TotalOrders,
            long TotalItemsSold,
            int PositiveFeedbackCount,
            int NegativeFeedbackCount,
            int ProductPendingCount,
            int VoucherApprovalCount,
            int CategoryApprovalCount
        )
        {
            this.RevenueData = RevenueData;
            this.OrdersByMonth = OrdersByMonth;
            this.SelectedRevenueYear = SelectedRevenueYear;
            this.TotalOrders = TotalOrders;
            this.TotalItemsSold = TotalItemsSold;
            this.PositiveFeedbackCount = PositiveFeedbackCount;
            this.NegativeFeedbackCount = NegativeFeedbackCount;
            this.ProductPendingCount = ProductPendingCount;
            this.VoucherApprovalCount = VoucherApprovalCount;
            this.CategoryApprovalCount = CategoryApprovalCount;
        }
    }
}