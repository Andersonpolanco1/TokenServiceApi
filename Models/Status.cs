namespace TokenServiceApi.Models
{
    public class Status
    {
        public const int Success = 1;
        public const int Failure = 0;

        public int StatusCode { get; set; }
        public string? Message { get; set; }
    }
}
