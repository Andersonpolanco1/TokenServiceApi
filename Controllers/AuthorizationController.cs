using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TokenServiceApi.Data;
using TokenServiceApi.Models;
using TokenServiceApi.Models.DTO;
using TokenServiceApi.Repositories.Abstract;

namespace TokenServiceApi.Controllers
{
    [Route("api/[controller]/{action}")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;

        public AuthorizationController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ITokenService tokenService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody]LoginModel loginRequest)
        {
            var user = await _userManager.FindByNameAsync(loginRequest.Username);

            if(user != null && await _userManager.CheckPasswordAsync(user, loginRequest.Password)){
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var role in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }

                var tokenResponse = _tokenService.GetToken(authClaims);
                var refreshToken = _tokenService.GetRefreshToken();
                var token = _context.Tokens.FirstOrDefault(t => t.Username == user.UserName);

                if(token is null)
                {
                    var newToken = new Token()
                    {
                        Username = user.UserName,
                        RefreshToken = refreshToken,
                        RefreshTokenExpiry = DateTime.Now.AddMinutes(1)
                    };

                    _context.Add(newToken);
                }
                else
                {
                    token.RefreshToken = refreshToken;
                    token.RefreshTokenExpiry = DateTime.Now.AddMinutes(1);
                }

                _context.SaveChanges();
                return Ok(new LoginResponseModel()
                {
                    Name = user.Name,
                    Username = user.UserName,
                    Token = tokenResponse.TokenString,
                    RefreshToken = refreshToken,
                    Expiration = tokenResponse.ValidTo,
                    StatusCode=1,
                    Message="Logged in"

                });
            }
            return Ok(new { 
                        StatusCode=0, 
                        Message="Failed loggin attemp"
                    });

        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody]RegisterModel registerModel)
        {
            var status = new Status();

            if (!ModelState.IsValid)
            {
                status.StatusCode = Status.Failure;
                status.Message = "Please complete all properties.";
                return Ok(status);
            }

            var userExists = await _userManager.FindByNameAsync(registerModel.UserName);

            if (userExists != null)
            {
                status.StatusCode = Status.Failure;
                status.Message = "Username alredy exists.";
                return Ok(status);
            }

            var newUser = new User()
            {
                UserName = registerModel.UserName,
                SecurityStamp = Guid.NewGuid().ToString(),
                Email = registerModel.Email,
                Name = registerModel.Name
            };

            var result = await _userManager.CreateAsync(newUser, registerModel.Password);
            if (!result.Succeeded)
            {
                status.StatusCode=Status.Failure;
                status.Message = $"User creation failed. {result.Errors.First().Description}";
                return Ok(status);
            }

            if (!await _roleManager.RoleExistsAsync(Role.User))
                await _roleManager.CreateAsync(new IdentityRole(Role.User));

            result = await _userManager.AddToRoleAsync(newUser, Role.User);

            if (!result.Succeeded)
            {
                status.StatusCode = Status.Failure;
                status.Message = $"{result.Errors.First().Description}";
                return Ok(status);
            }

            status.StatusCode = Status.Success;
            status.Message = $"Successfully registered.";
            return Ok(status);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordModel changePasswordModel)
        {
            Status status = new Status();

            if (!ModelState.IsValid)
            {
                status.StatusCode = Status.Failure;
                status.Message = "Request failed, verify fields.";
                return Ok(status);
            };
            var userToChangePass = await _userManager.FindByNameAsync(changePasswordModel.Username);

            if (userToChangePass is null)
            {
                status.StatusCode = Status.Failure;
                status.Message = "User not found.";
                return Ok(status);
            }

            if (! await _userManager.CheckPasswordAsync(userToChangePass,changePasswordModel.CurrentPassword))
            {
                status.StatusCode = Status.Failure;
                status.Message = "Current password not found.";
                return Ok(status);
            }

            if (!changePasswordModel.NewPassword.Equals(changePasswordModel.ConfirmNewPassword))
            {
                status.StatusCode = Status.Failure;
                status.Message = "new passwords not math.";
                return Ok(status);
            }

            var result = await _userManager.ChangePasswordAsync(userToChangePass, changePasswordModel.CurrentPassword, changePasswordModel.NewPassword);

            if (!result.Succeeded)
            {
                status.StatusCode = Status.Failure;
                status.Message = "Failed to change password..";
                return Ok(status);
            }

            status.StatusCode = Status.Success;
            status.Message = "Successfully changed password.";
            return Ok(status);
        }
    }
}
