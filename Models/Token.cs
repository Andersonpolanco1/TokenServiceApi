using System.ComponentModel.DataAnnotations.Schema;

namespace TokenServiceApi.Models
{
    [Table("Tokens")]
    public class Token
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}
