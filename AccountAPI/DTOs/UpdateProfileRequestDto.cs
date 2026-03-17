namespace AccountAPI.DTOs
{
	public class UpdateProfileRequestDto
	{
		// Account

		public IFormFile? Avatar { get; set; }

		// Customer
		public string FullName { get; set; } = null!;

		public DateTime? DateOfBirth { get; set; }

		public string? Gender { get; set; }

		public string? Phone { get; set; }

		public string? Address { get; set; }
	}
}
