namespace TokenServiceApi.Models.DTO
{
    public class LoginResponseModel:Status
    {
        public string Name { get; set; }
        public string  Username { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? Expiration { get; set; }
    }
}
