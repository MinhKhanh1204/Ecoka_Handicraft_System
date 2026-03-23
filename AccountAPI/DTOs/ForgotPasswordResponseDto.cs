namespace AccountAPI.DTOs
{
    public class ForgotPasswordResponseDto
    {
        /// <summary>
        /// True when an account was found and a reset email was sent.
        /// </summary>
        public bool EmailExists { get; set; }

        /// <summary>
        /// Human-readable message to show to the user in the UI.
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// Deep link to the registration page — only populated when EmailExists is false.
        /// </summary>
        public string? RegisterUrl { get; set; }
    }
}
