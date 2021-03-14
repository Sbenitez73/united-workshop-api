using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WorkshopAPI.Models;
using WorkshopAPI.Services;
using WorkshopAPI.Contexts;
using Microsoft.EntityFrameworkCore;
using WorkshopAPI.Entities;

namespace WorkshopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext context;
        private readonly HashService _hashService;

        public AccountController(UserManager<ApplicationUser> userManager,
                                  SignInManager<ApplicationUser> signInManager,
                                  IConfiguration configuration,
                                  ApplicationDbContext context, 
                                  HashService hasService)
        {
            this.context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _hashService = hasService;

        }


        [HttpPost("Create")]
        public async Task<ActionResult<UserToken>> CreateUser([FromBody] UserInfo model)
        {
            var user = new ApplicationUser { UserName = model.User, Email = model.User };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                return BuildToken(model);
            }
            else
            {
                return BadRequest("Username or password invalid");
            }
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserToken>> Login([FromBody] UserInfo userInfo)
        {
            Users user = await context.Users.FirstOrDefaultAsync(x => x.UserName == userInfo.User);

            if (user != null)
            {
                if ( HashService.CheckHash(userInfo.Password, user.Password, user.SaltPassword) )
                {
                    return BuildToken(userInfo);
                }
                else
                {
                    ApiError errorResponse = new ApiError();
                    errorResponse.Success = false;
                    errorResponse.ErrorMessage = "El usuario o la contraseña es invalida";
                    return new JsonResult(errorResponse);
                }
            }
            else
            {
                ApiError errorResponse = new ApiError();
                errorResponse.Success = false;
                errorResponse.ErrorMessage = "El usuario o la contraseña es invalida";
                return new JsonResult(errorResponse);
            }
        }

        private UserToken BuildToken(UserInfo userInfo)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.User),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddHours(4);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new UserToken()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }
    }
}
