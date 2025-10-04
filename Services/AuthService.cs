using AuthApi.Data;
using AuthApi.Entities;
using AuthApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AuthApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthService(AuthDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<TokenResponseDto?> LoginAsync(UserDto request)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == request.UserName);
            if (user.UserName != request.UserName)
            {
                return null;
            }
            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                return null; 
            }
            var response = new TokenResponseDto { RefreshToken = await GenerateAndSaveRefreshToken(user), AccessToken = CreateToken(user) };

            return response;
        }

        public async Task<User?> RegisterAsync(UserDto request)
        {
            var userFromDb =  _context.Users.FirstOrDefault(u => u.UserName == request.UserName);
            if (userFromDb is not null)
            {
                return null;
            }
            var user = new User();
            var hashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);
            user.UserName = request.UserName;
            user.PasswordHash = hashedPassword;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;

        }
        public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var user =await ValidateRefreshToken(request.UserId, request.RefreshToken);
            if (user is null)
            {
                return null;
            }
           return await CreateTokenResponse(user);

        }

        private async Task<TokenResponseDto> CreateTokenResponse(User response)
        {
            var tokenResponse = new TokenResponseDto
            {
                AccessToken = CreateToken(response),
                RefreshToken = GenerateAndSaveRefreshToken(response).Result
            };
            return  tokenResponse;
        }

        private async Task<User?> ValidateRefreshToken(int UserId ,string refreshToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == UserId);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryDate <= DateTime.UtcNow)
            {
                return null;
            }
            return user;
        }       
        private string GenerateRefreshToken()               
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        private async Task<string> GenerateAndSaveRefreshToken(User user)
        {
            var refreshToken = GenerateRefreshToken();
           
            
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(7);
          
            await _context.SaveChangesAsync();
            return refreshToken;
        }
        private  string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Role,user.Role)
                
            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetValue<string>("AppSettings:Token")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new JwtSecurityToken
            (
                issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
                audience: _configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);



        }

      
    }
}
