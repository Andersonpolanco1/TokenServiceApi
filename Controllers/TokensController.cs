using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TokenServiceApi.Data;
using TokenServiceApi.Models;
using TokenServiceApi.Models.DTO;
using TokenServiceApi.Repositories.Abstract;

namespace TokenServiceApi.Controllers
{
    [Route("api/[controller]/{action}")]
    [ApiController]
    public class TokensController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public TokensController(ApplicationDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost]
        public IActionResult Refresh(RefreshTokenRequestModel refreshToken)
        {
            if (refreshToken == null)
                return BadRequest("Invalid client request");

            ClaimsPrincipal claimsPrincipal = _tokenService.GetPrincipalFromExpiredToken(refreshToken.AccessToken);
            var username = claimsPrincipal.Identity.Name;

            var user = _context.Tokens.SingleOrDefault(x => x.Username == username);

            if(user == null) { }
                return BadRequest("Invalid client request");

            TokenResponse newAccesToken = _tokenService.GetToken(claimsPrincipal.Claims);
            string newRefreshToken = _tokenService.GetRefreshToken();

            user.RefreshToken = newRefreshToken;
            _context.SaveChanges();

            return Ok(new RefreshTokenRequestModel {
                AccessToken = newAccesToken.TokenString,
                RefreshToken = newRefreshToken,
            });
        }

        [Authorize]
        [HttpPost]
        public IActionResult Revoke()
        {
            var user = _context.Tokens.FirstOrDefault(u=>u.Username == User.Identity.Name);
            if (user is null)
                return BadRequest();

            user.RefreshToken = null;
            _context.SaveChanges();
            return Ok(true);
        }
    }
}
