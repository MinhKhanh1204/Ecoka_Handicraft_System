namespace AccountAPI.CustomFormatter
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public T? Data { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                StatusCode = StatusCodes.Status200OK,
                Data = data
            };
        }

        public static ApiResponse<T> Fail(string message, int statusCode)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Data = default
            };
        }
    }

}
