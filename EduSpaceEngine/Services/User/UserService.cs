using EduSpaceEngine.Model;
using Microsoft.AspNetCore.Mvc;
using EduSpaceEngine.Dto;
using EduSpaceEngine.Dto.User;
using EduSpaceEngine.Dto.User.Password;
using EduSpaceEngine.Dto.User.LoginRequest;
using EduSpaceEngine.Data;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using EduSpaceEngine.Services.Email;
using EduSpaceEngine.Services.Static;

namespace EduSpaceEngine.Services.User
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly DataDbContext _db;
        private readonly IStatiFuncs _staticFuncs;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public UserService(IConfiguration configuration, DataDbContext db, IStatiFuncs staticFuncs, IEmailService emailService, IMapper mapper)
        {
            _configuration = configuration;
            _db = db;
            _staticFuncs = staticFuncs;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<IActionResult> ChangeEmailAsync(string email, int userid)
        {
            // Retrieve the user from the database
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userid);

            // Check if the user exists
            if (user == null)
            {
                return new NotFoundObjectResult("User not found");
            }

            // Send a warning email to the current email address
            var resWarEmail = await _emailService.SendWarningEmailAsync(user.Email, user.UserName);
            if(resWarEmail != string.Empty)
            {
                return new BadRequestObjectResult("An error occurred while sending the email");
            }

            // Check if the new email is already registered
            if (_db.Users.Any(u => u.Email == email && u.OAuthProvider == null))
            {
                return new ConflictObjectResult("An account with this email already exists");
            }

            // Generate a verification code
            Random random = new Random();
            int verificationCode = random.Next(1000, 10000);

            // Send the verification code to the new email address
            var resEmail = await _emailService.SendChangeEmailCodeAsync(email, user.UserName, verificationCode);

            if (resEmail != string.Empty)
            {
                return new BadRequestObjectResult(resEmail);
            }

            try
            {
                await _db.SaveChangesAsync();
                // Return success response with the verification code
                return new OkObjectResult(new { VerificationCode = verificationCode });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<IActionResult> ChangeEmailRequestAsync(string email, int userid)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userid);

            if (user == null)
            {
                return new NotFoundObjectResult("User Not Found");

            }
            //პირველად გაიგზავნოს ძველ მაილზე გაფრთხილება

            var resWarEmail = await _emailService.SendWarningEmailAsync(user.Email, user.UserName);
            if (resWarEmail != string.Empty)
            {
                return new BadRequestObjectResult(resWarEmail);
            }

            if (_db.Users.Any(u => u.Email == email && u.OAuthProvider == null))
            {
                return new ConflictObjectResult("ასეთი ანგარიში უკვე რეგისტრირებულია");
            }


            Random random = new Random();

            int verificationCode = random.Next(1000, 10000);

            var resVerEmail =  await _emailService.SendChangeEmailCodeAsync(email, user.UserName, verificationCode);
            if(resVerEmail != string.Empty)
            {
                return new BadRequestObjectResult(resVerEmail);
            }
           /* try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }*/

            return new OkObjectResult(new { VerificationCode = verificationCode });
        }

        public async Task<IActionResult> ChangeGeneralInfoAsync(ChangeGeneralRequest request, int userId, string JWTRole)
        {
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return new NotFoundObjectResult("User not found.");
            }

            if (existingUser != user && existingUser != null)
            {
                return new ConflictObjectResult("Username already exists in the database.");
            }


            if (userId != user.UserId)
            {
                if (JWTRole != "admin")
                {
                    return new UnauthorizedObjectResult("Authorize invalid");
                }
            }

            user = _mapper.Map<ChangeGeneralRequest, UserModel>(request, user);

            try
            {
                // Save changes to the database
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                // _logger.LogError(ex, "An error occurred while saving the changes.");

                // Return a bad request response
                return new BadRequestObjectResult("An error occurred while saving the changes.");
            }

            // Return success response
            return new OkObjectResult("User updated successfully.");
        }

        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordRequest request, int userId, string JWTRole)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == request.UserId);

            if (user == null)
            {
                return new NotFoundObjectResult("user not found.");
            }
            if (userId != user.UserId)
            {
                if (JWTRole != "admin")
                {
                    return new UnauthorizedObjectResult("Authorize invalid");
                }
            }
            if (!_staticFuncs.VerifyPasswordHash(request.OldPassword, user.PasswordHash, user.PasswordSalt))
            {
                return new BadRequestObjectResult("Wrong password.");
            }

            _staticFuncs.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);


            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return new OkObjectResult("Successfully Changed");
        }

        public async Task<IActionResult> ChangeUsernameOrPhoneNumberAsync(UserModel requestUser, int userId, string JWTRole)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == requestUser.Email);

            if (user == null)
            {
                return new NotFoundObjectResult("user not found.");
            }

            if (userId != user.UserId && JWTRole != "admin")
            {
                return new UnauthorizedObjectResult("Unauthorized: Invalid role.");
            }

            // Update user properties
            user.UserName = requestUser.UserName;
            user.PhoneNumber = requestUser.PhoneNumber;

            try
            {
                // Save changes to the database
                await _db.SaveChangesAsync();
                return new OkObjectResult("Successfully changed username or phone number.");
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                // _logger.LogError(ex, "An error occurred while saving changes.");

                // Return an internal server error status with a message
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<bool> CheckEmailLoginAsync(CheckEmailRequest request)
        {
            bool emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email && u.OAuthProvider == null);

            if (!emailExists)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> CheckEmailRegistrationAsync(CheckEmailRequest request)
        {
            bool emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email && u.OAuthProvider == null && u.VerifiedAt != DateTime.MinValue);
            
            return emailExists;
        }

        public async Task<bool> CheckOAuth2ExistAsync(CheckOauth2ExistsReqeust request)
        {
            var user = await _db.Users
            .FirstOrDefaultAsync(u => u.OAuthProvider == request.OAuthProvider && u.OAuthProviderId == request.OAuthProviderId);

            if (user == null)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> CheckUserNameAsync(string username)
        {
            bool usernameExists = await _db.Users.AnyAsync(u => u.UserName == username);

            return usernameExists;
        }

        public async Task<IActionResult> DeleteUserAsync(int userId)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return new NotFoundObjectResult("User not Found");
            }

            try
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            

            return new OkObjectResult("User and related entities deleted");
        }

        public async Task<IActionResult> ForgotPasswordRequestAsync(string email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return new NotFoundObjectResult("User Not Found");
            }


            user.PasswordResetToken = _staticFuncs.CreateRandomToken();
            user.ResetTokenExpires = DateTime.Now.AddDays(1);

            string returnUrl = "https://edu-space.vercel.app/en/user/reset-password";

            string verificationLink = $"{returnUrl}?token={user.PasswordResetToken}";
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            await _emailService.SendEmailAsync(email, user.UserName, verificationLink);

            var resEmail = await _emailService.SendEmailAsync(email, user.UserName, verificationLink);
            if(resEmail != string.Empty)
            {
                return new BadRequestObjectResult(resEmail);
            }

            return new OkObjectResult($"You may reset your password now.");
        }

        public async Task<ResponseUser> GetUserAsync(int userId)
        {

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return new ResponseUser
                {
                    User = null,
                    Token = null,
                    Error = "User Not Found"
                };
            }

            if (user.OAuthProvider != null)
            {
                user.Email = $"{user.Email} ({user.OAuthProvider})";
            }



            string jwttoken = _staticFuncs.CreateToken(user);

            UserDto userDto = _mapper.Map<UserModel, UserDto>(user);

            return new ResponseUser
            {
                User = userDto,
                Token = jwttoken,
                Error = null
            };
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            var DtoUsers = await _db.Users.Select(u => _mapper.Map<UserModel, UserDto>(u)).ToListAsync();
            return DtoUsers;
        }

        public async Task<ResponseUser> LoginOAuth2Async(OAuth2LoginRequest request)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.OAuthProvider == request.OAuthProvider && u.OAuthProviderId == request.OAuthProviderId); // Fix the comparison here

            if (user == null)
            {
                return new ResponseUser
                {
                    User = null,
                    Token = null,
                    Error = "User Not Found"
                };
            }
            user.Email = user.Email + "(" + user.OAuthProvider + ")";

            // Normally, OAuth2 authentication would have already occurred, and you'd have an access token
            // and user information from the OAuth2 provider.

            // Instead of checking a password, you can assume that if the user exists and reached this point,
            // they have successfully authenticated through OAuth2.

            // You can generate a JWT token or another type of authentication token for the user at this point.

            UserDto userDto = _mapper.Map<UserModel, UserDto>(user);

            return new ResponseUser
            {
                User = userDto,
                Token = _staticFuncs.CreateToken(user),
                Error = null
            };
        }

        public async Task<ResponseUser> LoginWithEmailAsync(UserLoginEmailRequest request)
        {
            var user = await _db.Users
               .FirstOrDefaultAsync(u => u.Email == request.Email && u.OAuthProvider == null);

            if (user == null)
            {
                return new ResponseUser
                {
                    User = null,
                    Token = null,
                    Error = "User Not Found"
                };
            }

            if (!_staticFuncs.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return new ResponseUser
                {
                    User = null,
                    Token = null,
                    Error = "Wrong Password"
                };
            }

            /*if (user.VerifiedAt == DateTime.MinValue)
            {
                return new ResponseUser
                {
                    User = null,
                    Token = null,
                    Error = "User Not Verified"
                };
            }*/

            string jwttoken = _staticFuncs.CreateToken(user);

            UserDto userDto = _mapper.Map<UserModel, UserDto>(user);

            return new ResponseUser
            {
                User = userDto,
                Token = jwttoken,
                Error = null
            };
        }

        public async Task<ResponseUser> LoginWithPhoneNumberAsync(UserLoginPhoneRequest request)
        {
            var user = await _db.Users
               .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

            if (user == null)
            {
                return new ResponseUser
                {
                    User = null,
                    Token = null,
                    Error = "User Not Found"
                };
            }

            if (!_staticFuncs.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return new ResponseUser
                {
                    User = null,
                    Token = null,
                    Error = "Wrong Password"
                };
            }

            if (user.VerifiedAt == null)
            {
                return new ResponseUser
                {
                    User = null,
                    Token = null,
                    Error = "User Not Verified"
                };
            }

            string token = _staticFuncs.CreateToken(user);

            UserDto userDto = _mapper.Map<UserModel, UserDto>(user);

            return new ResponseUser
            {
                User = userDto,
                Token = token,
                Error = null
            };
        }

        public async Task<ResponseUser> LoginWithUserNameAsync(UserLoginUserNameRequest request)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.UserName == request.UserName);

            if (user == null)
            {
                return new ResponseUser
                {
                    User = null,
                    Token = null,
                    Error = "User Not Found"
                };
            }

            if (!_staticFuncs.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return new ResponseUser
                {
                    User = null,
                    Token = null,
                    Error = "Wrong Password"
                };
            }

            if (user.VerifiedAt == null)
            {
                return new ResponseUser
                {
                    User = null,
                    Token = null,
                    Error = "User Not Verified"
                };
            }

            string token = _staticFuncs.CreateToken(user);

            UserDto userDto = _mapper.Map<UserModel, UserDto>(user);

            return new ResponseUser
            {
                User = userDto,
                Token = token,
                Error = null
            };
        }

        public async Task<IActionResult> RegisterOAuthUserAsync(OAuthUserRegisterRequest request)
        {
            // Check if a user with the same OAuthProvider and OAuthProviderId already exists
            if (_db.Users.Any(u => u.OAuthProvider == request.oAuthProvider && u.OAuthProviderId == request.oAuthProviderId))
            {
                return new BadRequestObjectResult("OAuth2 User already exists.");
            }

            var (firstName, lastName) = _staticFuncs.ExtractNamesFromUsername(request.username);

            // Generate a unique username based on firstName and lastName
            var uniqueUsername = GenerateUniqueUsername(firstName, lastName);

            UserModel user = new UserModel();
            user.OAuthProvider = request.oAuthProvider;
            user.OAuthProviderId = request.oAuthProviderId;
            user.Picture = request.picture;
            user.Email = request.email;
            user.UserName = uniqueUsername;
            user.FirstName = firstName;
            user.LastName = lastName;



            if (!_db.Users.Any())
            {
                user.Role = UserRole.Admin; 

            }
            else
            {
                user.Role = UserRole.User; 
            }

            user.VerifiedAt = DateTime.Now;

            // Save the new user to the database

            try
            {

                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
                return new BadRequestObjectResult("An error occurred while saving changes.");
            }

            return new OkObjectResult("OAuth2 User successfully registered.");
        }

        private string GenerateUniqueUsername(string firstName, string lastName)
        {
            var baseUsername = (firstName.Length > 0 ? firstName[0].ToString() : "") + (lastName.Length > 0 ? lastName : "");
            var username = baseUsername;
            var count = 1;

            // Check if the username is already in use, and if so, append a number to make it unique
            while (_db.Users.Any(u => u.UserName == username))
            {
                username = $"{baseUsername}{count}";
                count++;
            }

            return username;
        }

        public async Task<ResponseUser> RegisterUserAsync(UserRegisterRequest request)
        {
            var existingUser = _db.Users
                .Include(u => u.Enrollments)
                .Include(u => u.Notifications)
                .Include(u => u.Posts)
                .Include(u => u.Comments)
                .Include(u => u.Progresses)
                .FirstOrDefault(u => u.Email == request.Email && u.OAuthProvider == null);

            if (existingUser != null)
            {
                _db.CourseEnroll.RemoveRange(existingUser.Enrollments);
                _db.Notifications.RemoveRange(existingUser.Notifications);
                _db.Posts.RemoveRange(existingUser.Posts);
                _db.Comments.RemoveRange(existingUser.Comments);
                _db.Progress.RemoveRange(existingUser.Progresses);

                _db.Users.Remove(existingUser);

                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception occurred during deletion: " + ex.Message);
                    return new ResponseUser
                    {
                        User = null,
                        Token = null,
                        Error = "An error occurred while deleting the existing user."
                    };
                }
            }

            // Check if username is already taken
            if (_db.Users.Any(u => u.UserName == request.UserName))
            {
                return new ResponseUser
                {
                    User = null,
                    Token = null,
                    Error = "Username is already taken."
                };
            }

            // Create a new user
            _staticFuncs.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            UserModel user = new UserModel
            {
                Email = request.Email,
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                OAuthProvider = null,
                OAuthProviderId = null,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                LastActivity = DateTime.UtcNow 
            };

            if (!_db.Users.Any())
            {
                user.Role = UserRole.Admin;
            }
            else
            {
                user.Role = UserRole.Guest;
            }

            try
            {
                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
                return new ResponseUser
                {
                    User = null,
                    Token = null,
                    Error = "An error occurred while saving the new user."
                };
            }

            UserDto userDto = _mapper.Map<UserModel, UserDto>(user);

            return new ResponseUser
            {
                User = userDto,
                Token = null,
                Error = null
            };
        }


        public async Task<IActionResult> ReLoginAsync(string password, int userId)
        {
            /*var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role*/


            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return new NotFoundObjectResult("User Not Found");
            }


            if (!_staticFuncs.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return new BadRequestObjectResult("Wrong password.");
            }


            return new OkObjectResult("Success");
        }

        public async Task<IActionResult> RemoveUserAsync(int userId)
        {
            // Retrieve the user from the database
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return new NotFoundObjectResult("User not found.");
            }

            try
            {
                // Remove the user
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();

                // Return success response
                return new OkObjectResult("User removed successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework instead of Console.WriteLine)
                Console.WriteLine($"Exception occurred during SaveChangesAsync: {ex.Message}");

                // Return a 500 Internal Server Error status with a message
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordRequest request)
        {

            var user = await _db.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);

            if (user == null || user.ResetTokenExpires < DateTime.Now)
            {
                return new UnauthorizedObjectResult("Invalid Token");
            }


            _staticFuncs.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


            return new OkObjectResult($"Password Succesfully resets.");
        }

        public string Test()
        {
            return "Test";
        }

        public async Task<IActionResult> UploadUserProfileImageAsync(UploadImageRequest imageRequest, int userId, string JWTRole)
        {

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == imageRequest.UserId);

            if (user == null)
            {
                return new NotFoundObjectResult("user not found.");
            }
            if (userId != user.UserId)
            {
                if (JWTRole != "admin")
                {
                    return new UnauthorizedObjectResult("Authorize invalid");
                }
            }

            user.Picture = imageRequest.PictureUrl;

            try
            {
                await _db.SaveChangesAsync();
                return new OkObjectResult("Updated Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }


        public async Task<string> VerifyEmail(int userId, string otp)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return "User Not Found";
            }

            if (DateTime.UtcNow > user.OTPExpirationTime)
            {
                return "OTP Expired";
            }

            var hashedEnteredOtp = _staticFuncs.VerifyOtp(otp, user.OTPSalt);

            if (hashedEnteredOtp == user.HashedOTP)
            {
                user.Role = user.Role != UserRole.Admin ? UserRole.User : UserRole.Admin;
                user.VerifiedAt = DateTime.UtcNow; 
                await _db.SaveChangesAsync();
                return String.Empty;
            }

            return "Wrong OTP";
        }

        public async Task<string> RequestVerify(int userId)
        {
            var user = await
                _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return "User Not Found";
            }
            var otp = _staticFuncs.GenerateOtp();
            
            var salt = _staticFuncs.GenerateOTPSalt();

            string hashedOtp = _staticFuncs.VerifyOtp(otp, salt);

            user.OTPSalt = salt;
            user.HashedOTP = hashedOtp;
            user.OTPExpirationTime = DateTime.UtcNow.AddMinutes(5);

            var respEmail = await _emailService.SendVerificationEmailAsync(user.Email, user.UserName, otp);

            if(respEmail != string.Empty)
            {
                return respEmail;
            }
            await _db.SaveChangesAsync();
            //string verificationSuccessUrl = "https://edu-space.vercel.app/en/user/auth/verification-successful";

            // Redirect the user to the verification success URL
            return string.Empty;
        }
    }
}
