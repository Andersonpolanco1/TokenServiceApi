using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace TokenServiceApi.Models
{
    public class Role
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}
