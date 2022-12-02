using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace TokenServiceApi.Models
{
    [Table("AspNetUsers")]
    public class User:IdentityUser
    {
        public string Name { get; set; }
    }
}
