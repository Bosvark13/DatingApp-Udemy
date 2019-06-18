using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.api.Data;
using DatingApp.api.DTO;
using DatingApp.api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository authRepository;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;

        public AuthController(IAuthRepository authRepository, IConfiguration configuration, IMapper mapper )
        {
            this.authRepository = authRepository;
            this.configuration = configuration;
            this.mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if (await authRepository.UserExists(userForRegisterDto.Username)) return BadRequest("Username already exists");

            var user = new User()
            {
                Username = userForRegisterDto.Username
            };

            var newUser = await authRepository.Register(user, userForRegisterDto.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var user = await authRepository.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (user == null) return Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:Token").Value));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
              Subject = new ClaimsIdentity(claims),
              Expires = DateTime.Now.AddDays(1),
              SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var currentUser = mapper.Map<UserForListDto>(user);

            return Ok(new {
                token = tokenHandler.WriteToken(token),
                currentUser
            });
        }
    }
}