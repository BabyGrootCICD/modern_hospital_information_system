using AutoMapper;
using SHIS.MOHD.WebAPI.Dtos;
using SHIS.MOHD.WebAPI.Models;
using SHIS.MOHD.WebAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SHIS.MOHD.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private MOHDContext _context;

        public AuthController(IConfiguration configuration, MOHDContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new ResultDTO() { isSuccess = true , Message = "Request successful." });
        }

        [HttpPost("JWTLogin")]
        public IActionResult JWTLongin(LoginPost value)
        {
            ResultDTO result = new ResultDTO() { isSuccess = false };

            if (value == null 
                || string.IsNullOrWhiteSpace(value.Account) 
                || string.IsNullOrWhiteSpace(value.PassWord)) 
            {
                result.isSuccess = false;
                result.returnValue = "";
                result.Message = "Invalid username or password.";
                return Unauthorized(result);
            }

            var userInfo = _context.MohdUsers.Where(c => c.UserIdno == value.Account && c.UserPassword == value.PassWord).FirstOrDefault();

            if (userInfo == null)
            {
                result.isSuccess = false;
                result.returnValue = "";
                result.Message = "Invalid username or password.";
                return Unauthorized(result);
            }
            else
            {

                var claims = new List<Claim>
            {
                new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Iss , userInfo.UserIdno),
                new Claim("FullName" , $"{userInfo.UserNameFirstname} {userInfo.UserNameMidname} {userInfo.UserNameLastname}"),
                new Claim(JwtRegisteredClaimNames.NameId, userInfo.UserNameMidname)
            };

                //日後可擴充使用者權限
                //claims.Add(new Claim(ClaimTypes.Role, "Upload"));

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:KEY"]));

                var jwt = new JwtSecurityToken
                (
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
                );

                var token = new JwtSecurityTokenHandler().WriteToken(jwt);
                result.isSuccess = true;
                result.returnValue = token;
                result.Message = "Get token";
                return Ok(result);
            }
        }
    }
}
