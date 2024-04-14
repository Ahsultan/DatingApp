using System.Security.Cryptography;
using System.Text;
using Api.Entities;
using Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api;

// [ServiceFilter(typeof(LogUserActivity))]
[ApiController]
[Route("/api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly ITokenService _tokenService;
    public AppDataContext _context;

    public AccountController(AppDataContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterDto registerDto) {

        if(await UserExists(registerDto.UserName)) return BadRequest("Username is alredy exists");

        using var hmac = new HMACSHA512();

        var user = new AppUser 
        {
            UserName = registerDto.UserName.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginDto loginDto) {
        var user = await _context.Users.Include(p => p.Photos)
        .SingleOrDefaultAsync(x => x.UserName == loginDto.UserName);

        if(user == null) return Unauthorized("Username is not valid");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for(int i = 0; i < computeHash.Length; i++) {
            if(computeHash[i] != user.PasswordHash[i]) return Unauthorized("Password is not valid");
        }

        var userdto = new UserDto {
            UserName = user.UserName,
            Token = _tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };

        return Ok(userdto);
    }

    //Reset Password with email verfication

    private async Task<bool> UserExists(string userName) {
        return await _context.Users.AnyAsync(user => user.UserName.Equals(userName.ToLower()));
    }
}
