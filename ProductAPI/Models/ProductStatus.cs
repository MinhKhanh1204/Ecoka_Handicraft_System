namespace ProductAPI.Models
{
    public static class ProductStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Active"; // In the DB it seems "Active" is the final state based on previous code
        public const string Rejected = "Rejected";
        public const string Inactive = "Inactive";
    }
}
