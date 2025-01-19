using EduSpaceEngine.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EduSpaceEngine.Services.Static
{
    public class StaticFuncs : IStatiFuncs
    {

        private readonly IConfiguration _configuration;
        public StaticFuncs(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        public string GenerateOtp()
        {
            var otp = RandomNumberGenerator.GetInt32(1000, 9999).ToString();
            return otp;
        }

        public string VerifyOtp(string otp, string salt)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(salt)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(otp));
                return Convert.ToBase64String(hash);
            }
        }
        public string GenerateOTPSalt()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        }

        public string CreateToken(UserModel user)
        {
            List<Claim> claims;
            try
            {
                claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email != null ? user.Email : ""),
                    new Claim(ClaimTypes.Name, user.FirstName != null ? user.FirstName : ""),
                    new Claim(ClaimTypes.Surname, user.LastName != null ? user.LastName : ""),
                    new Claim(ClaimTypes.NameIdentifier, user.UserName != null ? user.UserName : ""),
                    new Claim(ClaimTypes.MobilePhone, user.PhoneNumber != null ? user.PhoneNumber : "" ),
                    new Claim("ProfilePicture", user.Picture != null ? user.Picture : ""),
                    new Claim("joinedAt", user.VerifiedAt.ToString()),
                    new Claim("Oauth", user.OAuthProvider == null ? "" : user.OAuthProvider),
                    new Claim(ClaimTypes.Role, user.Role.ToString() != null ? user.Role.ToString() : "guest"),
                    new Claim("Plan", user.Plan != null ? user.Plan : ""),
                };
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred while creating claims: {ex.Message}");
                // You can choose to throw the exception further if it's not recoverable
                throw;
            }

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            if(key == null)
            {
                throw new Exception("Key is null");
            }
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        public (string, string) ExtractNamesFromUsername(string username)
        {
            // Split the username into parts based on spaces
            var nameParts = username.Split(' ');

            // Take the first part as the first name
            var firstName = nameParts.Length > 0 ? nameParts[0] : "";

            // Take the rest as the last name
            var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

            return (firstName, lastName);
        }



    }
}
