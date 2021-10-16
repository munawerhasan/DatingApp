using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DatingApp.Controllers
{
  
    public class AccountController : BaseAPIController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDTOs>> Register(RegisterDTOs registerDTOs)
        {
            if (await UserExist(registerDTOs.Username)) return BadRequest("Username Is Taken");
            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                UserName = registerDTOs.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTOs.Password)),
                PasswordSalt = hmac.Key
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new UserDTOs
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        private async Task<bool> UserExist(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTOs>> Login(LoginDTOs loginDTOs)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == loginDTOs.Username);
            if (user == null) return Unauthorized("Invalid username");
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTOs.Password));
            for(int i = 0; i < computeHash.Length; i++)
            {
                if (computeHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }
            return new UserDTOs
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }
    }
}
