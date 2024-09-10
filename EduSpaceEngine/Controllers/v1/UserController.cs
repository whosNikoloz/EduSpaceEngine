using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Asp.Versioning;
using EduSpaceEngine.Model;
using EduSpaceEngine.Dto.User;
using EduSpaceEngine.Services.Email;
using EduSpaceEngine.Services.User;
using EduSpaceEngine.Dto.User.LoginRequest;
using EduSpaceEngine.Dto.User.Password;
using GreenDonut;
using Azure;
using EduSpaceEngine.Dto;

namespace EduSpaceEngine.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public UserController(IEmailService emailService, IUserService userService)
        {
            _userService = userService;
            _emailService = emailService;
        }

        [HttpGet("Test")]
        public IActionResult Test()
        {
            ResponseModel res = new ResponseModel();
            res.status = true;
            res.result = _userService.Test();
            return Ok(res);
        }

        // მოიძიეთ ყველა მომხმარებლის სია.
        // მოითხოვს ადმინისტრატორის პრივილეგიებს.
        // GET api/Users
        [HttpGet("Users"), Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetUsersAsync();
            ResponseModel res = new ResponseModel();
            if (users == null)
            {
                res.status = false;
                res.result = "მომხმარებელები არ მოიძიეთ";
                return Ok(res);
            }

            res.status = true;
            res.result = users;
            return Ok(res);
        }

        // მიიღეთ კონკრეტული მომხმარებლის პროფილი მომხმარებლის სახელით.
        // საჭიროებს ავთენტიფიკაციას.
        // GET api/User/{username}
        [HttpGet("User/{userid}")]
        public async Task<IActionResult> GetUser(int userid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Call the service method to get the user
            var response = await _userService.GetUserAsync(userid);

            ResponseModel res = new ResponseModel();

            // Check if there was an error
            if (response.Error != null)
            {
                res.status = false;
                res.result = response.Error;
                return Ok(res);
            }

            // Return the user data and token if everything is fine
            var responseUser = new
            {
                User = response.User,
                Token = response.Token
            };

            res.status = true;
            res.result = responseUser;
            return Ok(res);
        }


        [HttpDelete("User/{userid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUser(int userid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.DeleteUserAsync(userid);

            var res = new ResponseModel();

            switch (result)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }



        [HttpPost("Auth/Login/check-email")]
        public async Task<IActionResult> CheckEmailLogin(CheckEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var response = await _userService.CheckEmailLoginAsync(request);

            ResponseModel res = new ResponseModel();

            if (!response)
            {
                res.status = false;
                res.result = "ასეთი მეილი არ არსებობს";
                return Ok(res);
            }

            res.status = true;
            res.result = "ასეთი მეილი არსებობს";
            return Ok(res);
        }


        [HttpPost("Auth/Register/check-email")]
        public async Task<IActionResult> CheckEmailReg(CheckEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _userService.CheckEmailRegistrationAsync(request);

            ResponseModel res = new ResponseModel();

            if (response)
            {
                res.status = false;
                res.result = "ასეთი მეილი არსებობ";
                return Ok(res);
            }

            res.status = true;
            res.result = "ასეთი მეილი არ არსებობს";
            return Ok(res);


        }

        [HttpGet("Auth/Register/check-username/{username}")]
        public async Task<IActionResult> CheckUserName(string username)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var response = await _userService.CheckUserNameAsync(username);


            ResponseModel res = new ResponseModel();

            if (response)
            {
                res.status = false;
                res.result = "სახელი დაკავებულია";
                return Ok(res);
            }

            res.status = true;
            res.result = "სახელი დაკავებული არ არის";
            return Ok(res);
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

            ResponseUser response = await _userService.RegisterUserAsync(request);

            ResponseModel res = new ResponseModel();

            if (response.Error != null)
            {
                res.status = false;
                res.result = response.Error;
                return Ok(res);
            }


            string host = "localhost:7213";

            string verificationLink = Url.ActionLink("VerifyEmail", "User", new { token = response.User.VerificationToken }, Request.Scheme, host);


            await _emailService.SendVerificationEmailAsync(response.User.Email, response.User.UserName, verificationLink);


            res.status = true;
            res.result = "მომხმარებელი წარმატებით შეიქმნა. გასადასახელებლად გამოიგზავნეთ ელ. ფოსტა.";
            return Ok(res);
        }


        [HttpPost("Auth/RegisterOAuth2")]
        public async Task<IActionResult> RegisterOAuthUser(OAuthUserRegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _userService.RegisterOAuthUserAsync(request);

            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }

        // ამოიღეთ მომხმარებელი ID-ით.
        // მოითხოვს ადმინისტრატორის პრივილეგიებს.
        // DELETE api/Auth/Remove/{userid}
        [HttpDelete("Auth/Remove/{userid}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> RemoveUser(int userid)
        {
            var response = await _userService.RemoveUserAsync(userid);

            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }


        [HttpPost("Auth/OAuthEmail")]
        public async Task<IActionResult> LoginOAuth2(OAuth2LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ResponseUser response = await _userService.LoginOAuth2Async(request);

            if(response.Error != null)
            {
                return Ok(new
                {
                    status = false,
                    result = response.Error
                });
            }

            return Ok(new
            {
                status = true,
                result = new
                {
                    User = response.User,
                    Token = response.Token
                }
            });
        }



        // შეამოწმებს თუ Oauth2 მომხმარებელი არსებობს.
        // POST api/Auth/OAuth2Exist
        [HttpPost("Auth/OAuth2Exist")]
        public async Task<IActionResult> CheckOAuth2Exist(CheckOauth2ExistsReqeust reqeust)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _userService.CheckOAuth2ExistAsync(reqeust);



            ResponseModel res = new ResponseModel();

            if (response)
            {
                res.status = true;
                res.result = "ასეთი მომხმარებელი არსებობს";
                return Ok(res);
            }

            res.status = false;
            res.result = "ასეთი მომხმარებელი არ არსებობს";
            return Ok(res);
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

            ResponseUser response = await _userService.LoginWithEmailAsync(request);

            ResponseModel res = new ResponseModel();
            if (response.Error != null)
            {

                res.status = false;
                res.result = response.Error;
                return Ok(res);
            }
        
            res.status = true;
            res.result = new
            {
                User = response.User,
                Token = response.Token
            };

            return Ok(res);
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

            ResponseUser response = await _userService.LoginWithUserNameAsync(request);
            ResponseModel res = new ResponseModel();
            if (response.Error != null)
            {

                res.status = false;
                res.result = response.Error;
                return Ok(res);
            }

            res.status = true;
            res.result = new
            {
                User = response.User,
                Token = response.Token
            };
            return Ok(res);

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

            ResponseUser response = await _userService.LoginWithPhoneNumberAsync(request);
            ResponseModel res = new ResponseModel();
            if (response.Error != null)
            {

                res.status = false;
                res.result = response.Error;
                return Ok(res);
            }

            res.status = true;
            res.result = new
            {
                User = response.User,
                Token = response.Token
            };

            return Ok(res);

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
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            var response = await _userService.ChangeGeneralInfoAsync(request, Int32.Parse(userId), JWTRole);

            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case ConflictObjectResult conflict:
                    res.status = false;
                    res.result = conflict.Value?.ToString();
                    return Conflict(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
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

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            var response = await _userService.ChangePasswordAsync(request, Int32.Parse(userId), JWTRole);

            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }


        // შეცვალეთ მომხმარებლის სახელი ან ტელეფონის ნომერი.
        // საჭიროებს ავთენტიფიკაციას.
        // POST api/User/ChangeUsernameOrNumber
        [HttpPost("User/ChangeUsernameOrNumber"), Authorize]
        public async Task<IActionResult> ChangeUsernameOrPhoneNumber(UserModel requestuser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            var response = await _userService.ChangeUsernameOrPhoneNumberAsync(requestuser, Int32.Parse(userId), JWTRole);

            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
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


            var response = await _userService.UploadUserProfileImageAsync(imagerequest, Int32.Parse(userId), JWTRole);

            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
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

            var response = await _userService.VerifyEmailAsync(token);

            if (!response)
            {
                return Ok(new
                {
                    status = false,
                    result = "Verification Failed"
                });
            }

            string verificationSuccessUrl = "https://edu-space.vercel.app/en/user/auth/verification-successful";

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

            var response = await _userService.ForgotPasswordRequestAsync(email);
            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
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
            var response = await _userService.ResetPasswordAsync(request);

            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }



        [HttpPost("User/ChangeEmailRequest/{email}"), Authorize]
        public async Task<IActionResult> ChangeEmailRequest(string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var response = await _userService.ChangeEmailRequestAsync(email, Int32.Parse(userId));
            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);
                case ConflictObjectResult conflictObjectResult:
                    res.status = false;
                    res.result = conflictObjectResult.Value?.ToString();
                    return Conflict(res);
                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }



        [HttpPost("User/ChangeEmail/{email}"), Authorize]
        public async Task<IActionResult> ChangeEmail(string email)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var response = await _userService.ChangeEmailAsync(email, Int32.Parse(userId));
            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case ConflictObjectResult conflictObjectResult:
                    res.status = false;
                    res.result = conflictObjectResult.Value?.ToString();
                    return Conflict(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }


        [HttpGet("User/ReLogin/{password}"), Authorize]
        public async Task<IActionResult> ReLogin(string password)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var response = await _userService.ReLoginAsync(password, Int32.Parse(userId));
            var res = new ResponseModel();

            switch (response)
            {
                case NotFoundObjectResult notFound:
                    res.status = false;
                    res.result = notFound.Value?.ToString();
                    return NotFound(res);

                case BadRequestObjectResult badReq:
                    res.status = false;
                    res.result = badReq.Value?.ToString();
                    return BadRequest(res);

                case UnauthorizedObjectResult unResult:
                    res.status = false;
                    res.result = unResult.Value?.ToString();
                    return Unauthorized(res);

                case OkObjectResult okResult:
                    res.status = true;
                    res.result = okResult.Value;
                    return Ok(res);
                default:
                    res.status = false;
                    res.result = "Unexpected Error";
                    return BadRequest(res);
            }
        }
    }
}
