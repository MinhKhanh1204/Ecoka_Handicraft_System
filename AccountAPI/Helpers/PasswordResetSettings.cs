namespace AccountAPI.Helpers
{
    public class PasswordResetSettings
    {
        /// <summary>
        /// Base URL of the MVC app (e.g. https://localhost:7200) for reset link in email.
        /// </summary>
        public string ClientBaseUrl { get; set; } = null!;
        /// <summary>
        /// Token validity in minutes.
        /// </summary>
        public int TokenExpiryMinutes { get; set; } = 60;
    }
}
