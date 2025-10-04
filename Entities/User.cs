using System.ComponentModel.DataAnnotations;

namespace AuthApi.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryDate { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
