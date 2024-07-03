using EduSpaceEngine.Model.User.LoginRequest;
using EduSpaceEngine.Model.User.Password;
using EduSpaceEngine.Model.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using EduSpaceEngine.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics.Metrics;
using System;
using AutoMapper;
using Asp.Versioning;

namespace EduSpaceEngine.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/")]
    public class UserController : ControllerBase
    {
        private readonly DataDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public UserController(DataDbContext context, IConfiguration configuration,IMapper mapper)
        {
            _configuration = configuration;
            _context = context;
            _mapper = mapper;
        }


        [HttpGet("Test")]
        public IActionResult Test()
        {
            return Ok("v1");
        }

        // მოიძიეთ ყველა მომხმარებლის სია.
        // მოითხოვს ადმინისტრატორის პრივილეგიებს.
        // GET api/Users
        [HttpGet("Users"), Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        // მიიღეთ კონკრეტული მომხმარებლის პროფილი მომხმარებლის სახელით.
        // საჭიროებს ავთენტიფიკაციას.
        // GET api/User/{username}
        [HttpGet("User/{userid}"), Authorize]
        public async Task<IActionResult> GetUser(int userid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var user = await _context.Users
                .Include(u => u.Enrollments)
                .Include(u => u.Posts)
                .Include(u => u.Comments)
                .Include(u => u.Progresses)
                .FirstOrDefaultAsync(u => u.UserId == userid);

            if (user == null)
            {
                return BadRequest("No User");
            }
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            if(user == null)
            {
                return NotFound("UserNotFound");
            }

            if(user.OAuthProvider != null)
            {
                user.Email = $"{user.Email} ({user.OAuthProvider})";
            }
            

            string jwttoken = CreateToken(user);

            var response = new
            {
                User = new
                {
                    userId = user.UserId,
                    userName = user.UserName,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber,
                    picture = user.Picture,
                    joinedAt = user.VerifiedAt
                },
                Token = jwttoken
            };

            
            return Ok(response);
        }




        [HttpPost("Auth/Login/check-email")]
        public async Task<IActionResult> CheckEmailLogin(CheckEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool emailExists = await _context.Users.AnyAsync(u => (u.Email == request.Email) && u.OAuthProvider == null);

            if (!emailExists)
            {
                return Ok(new
                {
                    successful = false,
                    error = "ასეთი მეილი არარსებობს"
                });
            }

            return Ok(new
            {
                Successful = true
            });
        }

        [HttpPost("Auth/Register/check-email")]
        public async Task<IActionResult> CheckEmailReg(CheckEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool emailExists = await _context.Users.AnyAsync(u => (u.Email == request.Email) && u.OAuthProvider == null);


            if (emailExists)
            {
                return Ok(new
                {
                    successful = false,
                    error = "ასეთი მეილი უკვე არსებობს"
                });
            }

            return Ok(new
            {
                Successful = true
            });
        }

        [HttpGet("Auth/Register/check-username/{username}")]
        public async Task<IActionResult> CheckUserName(string username)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool usernameExists = await _context.Users.AnyAsync(u => u.UserName == username);

            if (usernameExists)
            {
                return Ok(new
                {
                    successful = false,
                    error = "სახელი დაკავებულია"
                });
            }

            return Ok(new
            {
                Successful = true
            });
        }

        // ახალი მომხმარებლის რეგისტრაცია.
        // POST api/Auth/რეგისტრაცია
        [HttpPost("Auth/Register")]
        public async Task<IActionResult> RegisterUser(UserRegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Users.Any(u => (u.Email == request.Email) && u.OAuthProvider == null) || _context.Users.Any(u => u.UserName == request.UserName))
            {
                return BadRequest("User (Email or Username) already exists.");
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            UserModel user = _mapper.Map<UserRegisterRequest, UserModel>(request);
            user.OAuthProvider = null;
            user.OAuthProviderId = null;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.VerificationToken = CreateRandomToken();

            if (!_context.Users.Any())
            {
                user.Role = "admin"; // Assign "admin" role
            }
            else 
            {
                user.Role = "user"; // Assign "user" role
            }

            try { 

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
                return StatusCode(500, "An error occurred while saving changes.");
            }

            string host = "localhost:44310";

            string verificationLink = Url.ActionLink("VerifyEmail", "User", new { token = user.VerificationToken }, Request.Scheme, host);


            await SendVerificationEmail(user.Email, user.UserName, verificationLink);

            return Ok("User successfully created. Verification email sent.");
        }


        [HttpPost("Auth/RegisterOAuth2")]
        public async Task<IActionResult> RegisterOAuthUser(OAuthUserRegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if a user with the same OAuthProvider and OAuthProviderId already exists
            if (_context.Users.Any(u => u.OAuthProvider == request.oAuthProvider && u.OAuthProviderId == request.oAuthProviderId))
            {
                return BadRequest("OAuth2 User already exists.");
            }

            var (firstName, lastName) = ExtractNamesFromUsername(request.username);

            // Generate a unique username based on firstName and lastName
            var uniqueUsername = GenerateUniqueUsername(firstName, lastName);


            UserModel user = _mapper.Map<OAuthUserRegisterRequest, UserModel>(request);
            user.VerificationToken = CreateRandomToken();
            user.UserName = uniqueUsername;
            user.FirstName = firstName;
            user.LastName = lastName;


            if (!_context.Users.Any())
            {
                user.Role = "admin"; // Assign "admin" role
                
            }
            else
            {
                user.Role = "user"; // Assign "user" role
            }

            user.VerifiedAt = DateTime.Now;

            // Save the new user to the database

            try
            {

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
                return StatusCode(500, "An error occurred while saving changes.");
            }

            return Ok("OAuth2 User successfully registered.");
        }

        // Function to generate a unique username based on firstName and lastName
        private string GenerateUniqueUsername(string firstName, string lastName)
        {
            var baseUsername = (firstName.Length > 0 ? firstName[0].ToString() : "") + (lastName.Length > 0 ? lastName : "");
            var username = baseUsername;
            var count = 1;

            // Check if the username is already in use, and if so, append a number to make it unique
            while (_context.Users.Any(u => u.UserName == username))
            {
                username = $"{baseUsername}{count}";
                count++;
            }

            return username;
        }

        private (string, string) ExtractNamesFromUsername(string username)
        {
            // Split the username into parts based on spaces
            var nameParts = username.Split(' ');

            // Take the first part as the first name
            var firstName = nameParts.Length > 0 ? nameParts[0] : "";

            // Take the rest as the last name
            var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

            return (firstName, lastName);
        }

        // ამოიღეთ მომხმარებელი ID-ით.
        // მოითხოვს ადმინისტრატორის პრივილეგიებს.
        // DELETE api/Auth/Remove/{userid}
        [HttpDelete("Auth/Remove/{userid}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> RemoveUser(int userid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userid);
            if(user == null)
            {
                return BadRequest("use not Found");
            }

            try { 

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
                return StatusCode(500, "An error occurred while saving changes.");
            }


            return Ok("user Rmeoved");
        }


        [HttpPost("Auth/OAuthEmail")]
        public async Task<IActionResult> LoginOAuth2(OAuth2LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users
                .Include(u => u.Enrollments)
                .Include(u => u.Posts)
                .Include(u => u.Comments)
                .Include(u => u.Progresses)
                .FirstOrDefaultAsync(u => u.OAuthProvider == request.OAuthProvider && u.OAuthProviderId == request.OAuthProviderId); // Fix the comparison here

            if (user == null)
            {
                return BadRequest("User not found.");
            }
            user.Email = user.Email + "(" + user.OAuthProvider + ")";

            // Normally, OAuth2 authentication would have already occurred, and you'd have an access token
            // and user information from the OAuth2 provider.

            // Instead of checking a password, you can assume that if the user exists and reached this point,
            // they have successfully authenticated through OAuth2.

            // You can generate a JWT token or another type of authentication token for the user at this point.
            string jwttoken = CreateToken(user);

            var response = new
            {
                Token = jwttoken
            };

            return Ok(new
            {
                successful = true,
                response
            });
        }



        // შეამოწმებს თუ Oauth2 მომხმარებელი არსებობს.
        // POST api/Auth/OAuth2Exist
        [HttpPost("Auth/OAuth2Exist")]
        public async Task<IActionResult> CheckeOatuh2Exist(CheckOauth2ExistsReqeust reqeust)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.OAuthProvider == reqeust.OAuthProvider && u.OAuthProviderId == reqeust.OAuthProviderId);

            if (user == null)
            {
                return BadRequest(false);
            }
            return Ok(true);
        }

        // შედით ელექტრონული ფოსტით და პაროლით.
        // POST api/Auth/Email
        [HttpPost("Auth/Email")]
        public async Task<IActionResult> LoginWithEmail(UserLoginEmailRequest request)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.OAuthProvider == null);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Ok(new
                {
                    successful = false,
                    error = "Wrong Passwrod"
                });
            }

            if (user.VerifiedAt == DateTime.MinValue)
            {
                return Ok(new
                {
                    successful = false,
                    error = "User Not Verified"
                });
            }

            string jwttoken = CreateToken(user);

            var response = new
            {
                Token = jwttoken
            };

            return Ok(new
            {
                successful = true,
                response
            });

        }

        // შედით მომხმარებლის სახელით და პაროლით.
        // POST api/Auth/Username
        [HttpPost("Auth/Username")]
        public async Task<IActionResult> LoginWithUserName(UserLoginUserNameRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var user = await _context.Users
                .Include(u => u.Enrollments)
                .Include(u => u.Notifications)
                .Include(u => u.Posts)
                .Include(u => u.Comments)
                .Include(u => u.Progresses)
                .FirstOrDefaultAsync(u => u.UserName == request.UserName);

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }

            if (user.VerifiedAt == null)
            {
                return BadRequest("User not verified.");
            }

            string token = CreateToken(user);

            return Ok(user);
        }

        // შედით ტელეფონის ნომრით და პაროლით.
        // POST api/Auth/Phone
        [HttpPost("Auth/Phone")]
        public async Task<IActionResult> LoginWithPhoneNumber(UserLoginPhoneRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var user = await _context.Users
                .Include(u => u.Enrollments)
                .Include(u => u.Notifications)
                .Include(u => u.Posts)
                .Include(u => u.Comments)
                .Include(u => u.Progresses)
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }

            if (user.VerifiedAt == null)
            {
                return BadRequest("User not verified.");
            }

            string token = CreateToken(user);

            return Ok(user);
        }


        // შეცვალეთ მომხმარებლის პაროლი.
        // საჭიროებს ავთენტიფიკაციას.
        // POST api/User/ChangeGeneral
        [HttpPut("User/ChangeGeneral"), Authorize]
        public async Task<IActionResult> ChangeGeneral(ChangeGeneralRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == request.UserId);
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            if (user == null)
            {
                return NotFound("user not found.");
            }
            if (userId != user.UserId.ToString())
            {
                if (JWTRole != "admin")
                {
                    return BadRequest("Authorize invalid");
                }
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
            if(existingUser != user && existingUser != null)
            {
                return BadRequest("Username already exists in the database.");
            }
            

            user.UserName = request.UserName;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
            }


            return Ok("Successfully Changed");
        }



        // შეცვალეთ მომხმარებლის პაროლი.
        // საჭიროებს ავთენტიფიკაციას.
        // POST api/User/ChangePassword
        [HttpPut("User/ChangePassword"), Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == request.UserId);
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            if (user == null)
            {
                return BadRequest("user not found.");
            }
            if (userId != user.UserId.ToString())
            {
                if (JWTRole != "admin")
                {
                    return BadRequest("Authorize invalid");
                }
            }
            if (!VerifyPasswordHash(request.OldPassword, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);


            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
            }


            return Ok("Successfully Changed");
        }


        // შეცვალეთ მომხმარებლის სახელი ან ტელეფონის ნომერი.
        // საჭიროებს ავთენტიფიკაციას.
        // POST api/User/ChangeUsernameOrNumber
        [HttpPost("User/ChangeUsernameOrNumber"), Authorize]
        public async Task<IActionResult> ChangeUsernameOrNumber(UserModel requestuser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == requestuser.Email);

            if (user == null)
            {
                return BadRequest("user not found.");
            }
            if (userId != user.UserId.ToString())
            {
                if (JWTRole != "admin")
                {
                    return BadRequest("Authorize invalid");
                }
            }

            user.UserName = requestuser.UserName;
            user.PhoneNumber = requestuser.PhoneNumber;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Successfully changed Username or number");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
                return StatusCode(500, "An error occurred while saving changes.");
            }
        }


        // ატვირთეთ მომხმარებლის პროფილის სურათი.
        // საჭიროებს ავთენტიფიკაციას.
        // POST api/User/UploadImage
        [HttpPost("User/UploadImage"), Authorize]
        public async Task<IActionResult> UploadUserProfileImage(UploadImageRequest imagerequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role


            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == imagerequest.UserId);

            if (user == null)
            {
                return BadRequest("user not found.");
            }
            if (userId != user.UserId.ToString())
            {
                if (JWTRole != "admin")
                {
                    return BadRequest("Authorize invalid");
                }
            }

            user.Picture = imagerequest.PictureUrl;

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
                return StatusCode(500, "An error occurred while saving changes.");
            }
        }



        // გადაამოწმეთ მომხმარებლის ელ.ფოსტის მისამართი ნიშნის გამოყენებით.
        // GET api/Verify/Email
        [HttpGet("Verify/Email")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);

            if (user == null)
            {
                return BadRequest("Invalid token.");
            }

            user.VerifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            string verificationSuccessUrl = "http://localhost:3000/user/auth/verification-successful";

            // Redirect the user to the verification success URL
            return Redirect(verificationSuccessUrl);
        }



        // მოითხოვეთ პაროლის აღდგენა ელექტრონული ფოსტით.
        // POST api/User/ForgotPass
        [HttpPost("User/ForgotPassword")]
        public async Task<IActionResult> ForgotPasswordRequest(string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return BadRequest("User Not Found");
            }


            user.PasswordResetToken = CreateRandomToken();
            user.ResetTokenExpires = DateTime.Now.AddDays(1);

            string returnUrl = "http://localhost:3000/ka/user/reset-password";

            string verificationLink = $"{returnUrl}?token={user.PasswordResetToken}";

            await _context.SaveChangesAsync();

            await SendEmail(email, user.UserName, verificationLink);

            return Ok($"You may reset your password now.");
        }



        // გადააყენეთ მომხმარებლის პაროლი გადატვირთვის ნიშნის გამოყენებით.
        // POST api/User/ResetPassword
        [HttpPut("User/ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);

            if (user == null || user.ResetTokenExpires < DateTime.Now)
            {
                return BadRequest("Invalid Token");
            }


            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return Ok($"Password Succesfully resets.");
        }



        [HttpPost("User/ChangeEmailRequest/{email}"), Authorize]
        public async Task<IActionResult> ChangeEmailRequest(string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role


            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user == null)
            {
                return BadRequest("User Not Found");

            }
            //პირველად გაიგზავნოს ძველ მაილზე გაფრთხილება

            await SendWarningEmail(user.Email, user.UserName);


            if (_context.Users.Any(u => u.Email == email && u.OAuthProvider == null ))
            {
                return BadRequest("ასეთი ანგარიში უკვე რეგისტრირებულია");
            }


            Random random = new Random();

            int verificationCode = random.Next(1000, 10000);


            await SendChangeEmailCode(email, user.UserName, verificationCode);


            await _context.SaveChangesAsync();

            return Ok(verificationCode);
        }



        [HttpPost("User/ChangeEmail/{email}"), Authorize]
        public async Task<IActionResult> ChangeEmail(string email)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role


            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user == null)
            {
                return BadRequest("User Not Found");
            }


            user.Email = email;

            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpGet("User/ReLogin/{password}"), Authorize]
        public async Task<IActionResult> ReLogin(string password)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role


            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user == null)
            {
                return BadRequest("User Not Found");
            }


            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }


            return Ok();
        }




        private async Task SendVerificationEmail(string email, string user, string confirmationLink)
        {

            string messageBody = $@"
            <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
            <html xmlns=""http://www.w3.org/1999/xhtml"">

            <head>
              <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
              <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
              <title>Verify your account</title>

              <style>
                .button {{
                        display: inline-block;
                        background-color: #007bff;
                        color: white !important;
                        border: none;
                        border-radius: 20px;
                        padding: 10px 20px;
                        text-decoration: none;
                        cursor: pointer;
                    }}
              </style>
            </head>


            <body style=""font-family: Helvetica, Arial, sans-serif; margin: 0px; padding: 0px; background-color: #ffffff;"">
              <table role=""presentation""
                style=""width: 100%; border-collapse: collapse; border: 0px; border-spacing: 0px; font-family: Arial, Helvetica, sans-serif; background-color: rgb(239, 239, 239);"">
                <tbody>
                  <tr>
                    <td align=""center"" style=""padding: 1rem 2rem; vertical-align: top; width: 100%;"">
                      <table role=""presentation"" style=""max-width: 600px; border-collapse: collapse; border: 0px; border-spacing: 0px; text-align: left;"">
                        <tbody>
                          <tr>
                            <td style=""padding: 40px 0px 0px;"">
                              <div style=""text-align: left;"">
                                <div style=""padding-bottom: 20px;""><img src=""https://firebasestorage.googleapis.com/v0/b/eduspace-a81b5.appspot.com/o/EduSpaceLogo.png?alt=media&token=7b7dc8a5-05d8-4348-9b4c-c19913949c67"" alt=""Company"" style=""width: 56px;""></div>
                              </div>
                              <div style=""padding: 20px; background-color: rgb(255, 255, 255); border-radius: 20px;"">
                                <div style=""color: rgb(0, 0, 0); text-align: center;"">
                                  <h1 style=""margin: 1rem 0"">👋</h1>
                                  <h1 style=""margin: 1rem 0"">მოგესალმებით, {user} !</h1>
                                  <p style=""padding-bottom: 16px"">გმადლობთ, რომ დარეგისტრირდით EduSpace-ზე თქვენი ანგარიშის გასააქტიურებლად, გთხოვთ,დააჭიროთ ქვემოთ მოცემულ ღილაკს</p>
                                  <a href={confirmationLink} class='button'>გააქტიურება</a>
                                  <p style=""padding-bottom: 16px"">თუ ამ მისამართის დადასტურება არ მოგითხოვიათ, შეგიძლიათ იგნორირება გაუკეთოთ ამ ელფოსტას.</p>
                                  <p style=""padding-bottom: 16px"">Thank you, EduSpace Team</p>
                                </div>
                              </div>
                              <div style=""padding-top: 20px; color: rgb(153, 153, 153); text-align: center;"">
                                <p style=""padding-bottom: 16px"">© 2023 Nikoloza. ყველა უფლება დაცულია</p>
                              </div>
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </td>
                  </tr>
                </tbody>
              </table>
            </body>

            </html>";

            using (MailMessage message = new MailMessage("noreplynika@gmail.com", email))
            {
                message.Subject = "EduSpace.ge მომხმარებლის აქტივაცია";
                message.Body = messageBody;
                message.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.Credentials = new NetworkCredential("noreplynika@gmail.com", "cdqwvhmdwljietwq");
                    smtpClient.EnableSsl = true;

                    try
                    {
                        await smtpClient.SendMailAsync(message);
                    }
                    catch (Exception)
                    {
                        // Handle any exception that occurs during the email sending process
                        // You can log the error or perform other error handling actions
                    }
                }
            }
        }
        private async Task SendEmail(string email, string user, string confirmationLink)
        {
            string messageBody = $@"
            <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
            <html xmlns=""http://www.w3.org/1999/xhtml"">

            <head>
              <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
              <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
              <title>Verify your account</title>

              <style>
                .button {{
                        display: inline-block;
                        background-color: #007bff;
                        color: white !important;
                        border: none;
                        border-radius: 20px;
                        padding: 10px 20px;
                        text-decoration: none;
                        cursor: pointer;
                    }}
              </style>
            </head>


            <body style=""font-family: Helvetica, Arial, sans-serif; margin: 0px; padding: 0px; background-color: #ffffff;"">
              <table role=""presentation""
                style=""width: 100%; border-collapse: collapse; border: 0px; border-spacing: 0px; font-family: Arial, Helvetica, sans-serif; background-color: rgb(239, 239, 239);"">
                <tbody>
                  <tr>
                    <td align=""center"" style=""padding: 1rem 2rem; vertical-align: top; width: 100%;"">
                      <table role=""presentation"" style=""max-width: 600px; border-collapse: collapse; border: 0px; border-spacing: 0px; text-align: left;"">
                        <tbody>
                          <tr>
                            <td style=""padding: 40px 0px 0px;"">
                              <div style=""text-align: left;"">
                                <div style=""padding-bottom: 20px;""><img src=""https://firebasestorage.googleapis.com/v0/b/eduspace-a81b5.appspot.com/o/EduSpaceLogo.png?alt=media&token=7b7dc8a5-05d8-4348-9b4c-c19913949c67"" alt=""Company"" style=""width: 56px;""></div>
                              </div>
                              <div style=""padding: 20px; background-color: rgb(255, 255, 255); border-radius: 20px;"">
                                <div style=""color: rgb(0, 0, 0); text-align: center;"">
                                  <h1 style=""margin: 1rem 0"">🔒</h1>
                                  <h1 style=""margin: 1rem 0"">მოგესალმებით, {user}</h1>
                                  <p style=""padding-bottom: 16px"">თქვენი EduSpace-ს ანგარიშიდან მოთხოვნილია პაროლის აღდგენა. ახალი პაროლის დასაყენებლად გთხოვთ დააჭიროთ პაროლის აღდგენის ღილაკს.</p>
                                  <a href={confirmationLink} class='button'>პაროლის აღდგენა</a>
                                  <p style=""padding-bottom: 16px"">თუ პაროლის გადაყენება არ მოგითხოვიათ, შეგიძლიათ უგულებელყოთ ეს ელფოსტა.</p>
                                  <p style=""padding-bottom: 16px"">Thank you, EduSpace Team</p>
                                </div>
                              </div>
                              <div style=""padding-top: 20px; color: rgb(153, 153, 153); text-align: center;"">
                                <p style=""padding-bottom: 16px"">© 2023 Nikoloza. ყველა უფლება დაცულია</p>
                              </div>
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </td>
                  </tr>
                </tbody>
              </table>
            </body>

            </html>";

            using (MailMessage message = new MailMessage("noreplynika@gmail.com", email))
            {
                message.Subject = "EduSpace.ge ანგარიშის აღდგენა";
                message.Body = messageBody;
                message.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.Credentials = new NetworkCredential("noreplynika@gmail.com", "cdqwvhmdwljietwq");
                    smtpClient.EnableSsl = true;

                    try
                    {
                        await smtpClient.SendMailAsync(message);
                    }
                    catch (Exception)
                    {
                        // Handle any exception that occurs during the email sending process
                        // You can log the error or perform other error handling actions
                    }
                }
            }
        }

        private async Task SendWarningEmail(string email, string user)

        {
            
            string messageBody = $@"

            <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
            <html xmlns=""http://www.w3.org/1999/xhtml"">

            <head>
              <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
              <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
              <title>Verify your account</title>

              <style>

                .button {{
                        display: inline-block;
                        background-color: #007bff;
                        color: white !important;
                        border: none;
                        border-radius: 20px;
                        padding: 10px 20px;
                        text-decoration: none;
                        cursor: pointer;

                    }}
              </style>
            </head>

            <body style=""font-family: Helvetica, Arial, sans-serif; margin: 0px; padding: 0px; background-color: #ffffff;"">
              <table role=""presentation""
                style=""width: 100%; border-collapse: collapse; border: 0px; border-spacing: 0px; font-family: Arial, Helvetica, sans-serif; background-color: rgb(239, 239, 239);"">
                <tbody>
                  <tr>
                    <td align=""center"" style=""padding: 1rem 2rem; vertical-align: top; width: 100%;"">
                      <table role=""presentation"" style=""max-width: 600px; border-collapse: collapse; border: 0px; border-spacing: 0px; text-align: left;"">
                        <tbody>
                          <tr>
                            <td style=""padding: 40px 0px 0px;"">
                              <div style=""text-align: left;"">
                                <div style=""padding-bottom: 20px;""><img src=""https://firebasestorage.googleapis.com/v0/b/eduspace-a81b5.appspot.com/o/EduSpaceLogo.png?alt=media&token=7b7dc8a5-05d8-4348-9b4c-c19913949c67"" alt=""Company"" style=""width: 56px;""></div>
                              </div>
                             <div style=""padding: 20px; background-color: rgb(255, 255, 255); border-radius: 20px;"">
                              <div style=""color: rgb(0, 0, 0); text-align: center;"">
                                <h1 style=""margin: 1rem 0"">⚠️</h1>
                                <h1 style=""margin: 1rem 0"">მოგესალმებით, {user} !</h1>
                                <p style=""padding-bottom: 16px"">ჩვენ შევამჩნიეთ, რომ თქვენ მოითხოვეთ ელფოსტის მისამართის შეცვლა, რომელიც დაკავშირებულია თქვენს EduSpace ანგარიშთან.</p>
                                <p style=""padding-bottom: 16px"">თუ თქვენ არ მოითხოვეთ ეს ცვლილება, შეგიძლიათ უგულებელყოთ ეს ელფოსტა.</p>
                                <p style=""padding-bottom: 16px"">Thank you, EduSpace Team</p>
                              </div>
                            </div>
                            <div style=""padding-top: 20px; color: rgb(153, 153, 153); text-align: center;"">
                              <p style=""padding-bottom: 16px"">© 2023 Nikoloza. All rights reserved.</p>
                            </div>
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </td>
                  </tr>
                </tbody>
              </table>
            </body>
            </html>";


            using (MailMessage message = new MailMessage("noreplynika@gmail.com", email))

            {
                message.Subject = "EduSpace.ge მომხმარებლის აქტივაცია";
                message.Body = messageBody;
                message.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))

                {

                    smtpClient.Credentials = new NetworkCredential("noreplynika@gmail.com", "cdqwvhmdwljietwq");
                    smtpClient.EnableSsl = true;

                    try
                    {
                        await smtpClient.SendMailAsync(message);
                    }
                    catch (Exception)
                    {
                        // Handle any exception that occurs during the email sending process
                        // You can log the error or perform other error handling actions
                    }
                }
            }
        }






        private async Task SendChangeEmailCode(string email, string user, int randomNumber)
        {
            string messageBody = $@"

            <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
            <html xmlns=""http://www.w3.org/1999/xhtml"">

            <head>
              <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
              <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
              <title>Verify your account</title>

              <style>
                .button {{
                        display: inline-block;
                        background-color: #007bff;
                        color: white !important;
                        border: none;
                        border-radius: 20px;
                        padding: 10px 20px;
                        text-decoration: none;
                        cursor: pointer;
                    }}
              </style>
            </head>


            <body style=""font-family: Helvetica, Arial, sans-serif; margin: 0px; padding: 0px; background-color: #ffffff;"">
              <table role=""presentation""
                style=""width: 100%; border-collapse: collapse; border: 0px; border-spacing: 0px; font-family: Arial, Helvetica, sans-serif; background-color: rgb(239, 239, 239);"">
                <tbody>
                  <tr>
                    <td align=""center"" style=""padding: 1rem 2rem; vertical-align: top; width: 100%;"">
                      <table role=""presentation"" style=""max-width: 600px; border-collapse: collapse; border: 0px; border-spacing: 0px; text-align: left;"">
                        <tbody>
                          <tr>
                            <td style=""padding: 40px 0px 0px;"">
                              <div style=""text-align: left;"">
                                <div style=""padding-bottom: 20px;""><img src=""https://firebasestorage.googleapis.com/v0/b/eduspace-a81b5.appspot.com/o/EduSpaceLogo.png?alt=media&token=7b7dc8a5-05d8-4348-9b4c-c19913949c67"" alt=""Company"" style=""width: 56px;""></div>
                              </div>

                              <div style=""padding: 20px; background-color: rgb(255, 255, 255); border-radius: 20px;"">
                              <div style=""color: rgb(0, 0, 0); text-align: center;"">
                                <h1 style=""margin: 1rem 0"">👌</h1>
                                <h1 style=""margin: 1rem 0"">მოგესალმებით, {user} !</h1>
                                <p style=""padding-bottom: 16px"">გმადლობთ EduSpace-ზე თქვენი ელ.ფოსტის მისამართის განახლებისთვის. თქვენი ახალი ელფოსტის დასადასტურებლად, გთხოვთ, შეიყვანოთ შემდეგი კოდი:</p>
                                <div  class='button'>{randomNumber}</div>
                                <p style=""padding-bottom: 16px"">თუ თქვენ არ მოითხოვეთ ეს ცვლილება, შეგიძლიათ უგულებელყოთ ეს ელფოსტა</p>
                                <p style=""padding-bottom: 16px"">Thank you, EduSpace Team</p>
                              </div>
                            </div>
                            <div style=""padding-top: 20px; color: rgb(153, 153, 153); text-align: center;"">
                              <p style=""padding-bottom: 16px"">© 2023 Nikoloza. All rights reserved.</p>
                            </div>

                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </td>
                  </tr>
                </tbody>
              </table>
            </body>

            </html>";

            using (MailMessage message = new MailMessage("noreplynika@gmail.com", email))

            {

                message.Subject = "EduSpace.ge ანგარიშის აღდგენა";

                message.Body = messageBody;

                message.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))

                {

                    smtpClient.Credentials = new NetworkCredential("noreplynika@gmail.com", "cdqwvhmdwljietwq");
                    smtpClient.EnableSsl = true;

                    try

                    {
                        await smtpClient.SendMailAsync(message);
                    }

                    catch (Exception)

                    {
                        // Handle any exception that occurs during the email sending process
                        // You can log the error or perform other error handling actions
                    }

                }

            }

        }






        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }


        private string CreateToken(UserModel user)
        {
            List<Claim> claims;
            try
            {
                claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, (user.Email != null ? user.Email : "")),
                new Claim(ClaimTypes.Name, (user.FirstName != null ? user.FirstName : "")),
                new Claim(ClaimTypes.Surname, (user.LastName != null ? user.LastName : "")),
                new Claim(ClaimTypes.NameIdentifier, (user.UserName != null ? user.UserName : "")),
                new Claim(ClaimTypes.MobilePhone, (user.PhoneNumber != null ? user.PhoneNumber : "") ),
                new Claim("ProfilePicture", (user.Picture != null ? user.Picture : "")),
                new Claim("joinedAt", user.VerifiedAt.ToString()),
                new Claim("Oauth", (user.OAuthProvider == null ? "" : user.OAuthProvider)),
                new Claim(ClaimTypes.Role, (user.Role != null ? user.Role : "")),
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

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
