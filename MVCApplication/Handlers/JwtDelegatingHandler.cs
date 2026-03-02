using System.Net.Http.Headers;

namespace MVCApplication.Handlers
{
    //Auto attach Jwt to header
    public class JwtDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public JwtDelegatingHandler(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = _contextAccessor.HttpContext?.Request.Cookies["AccessToken"];

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
