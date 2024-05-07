namespace Talabat.APIs.Errors
{
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }

        public ApiResponse(int statuscode, string? message = null)
        {
            StatusCode = statuscode;
            Message = message ?? GetDefaultMessageForStatusCode(statuscode);
        }

        private string? GetDefaultMessageForStatusCode(int statuscode)
        {
            return statuscode switch
            {
                400 => "Bad Request",
                401 => "Unauthorized",
                404 => "Resource was not found",
                500 => "Errors are the path to dark side. Errors lead to anger. Anger leads to hate. Hate leads to career change",
                _ => null
            };
        }
    }
}
