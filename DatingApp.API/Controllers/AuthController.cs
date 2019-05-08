using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class AuthController : ControllerBase
  {
    private readonly IAuthRepository _repo;
    private readonly IConfiguration _config;

    public AuthController(IAuthRepository repo, IConfiguration config)
    {
      _config = config;
      _repo = repo;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserToRegisterDTO userToRegisterDto)
    {
      userToRegisterDto.Username = userToRegisterDto.Username.ToLower();

      if (await _repo.UserExists(userToRegisterDto.Username))
        return BadRequest("Username Exists");

      var userToCreate = new User
      {
        Username = userToRegisterDto.Username
      };
      var CreatedUser = await _repo.Register(userToCreate, userToRegisterDto.Password);

      return StatusCode(201);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserToLoginDto userToLoginDto)
    {
      var userFromLogin = await _repo.Login(userToLoginDto.Username.ToLower(), userToLoginDto.Password);

      if (userFromLogin == null)
      {
        return Unauthorized();
      }

      var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, userFromLogin.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromLogin.Username)
            };

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

      var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

      var tokenDescriptor = new SecurityTokenDescriptor{
          Subject = new ClaimsIdentity(claims),
          Expires = DateTime.Now.AddDays(1),
          SigningCredentials = cred
      };
      
      var tokenHandler = new JwtSecurityTokenHandler();
      var token = tokenHandler.CreateToken(tokenDescriptor);

      return Ok(new{
          token = tokenHandler.WriteToken(token)
      });
    }
  }
}