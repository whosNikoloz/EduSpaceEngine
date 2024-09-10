using EduSpaceEngine.Dto;
using EduSpaceEngine.Dto.User;
using EduSpaceEngine.Dto.User.LoginRequest;
using EduSpaceEngine.Dto.User.Password;
using EduSpaceEngine.Model;
using Microsoft.AspNetCore.Mvc;

namespace EduSpaceEngine.Services.User
{
    public interface IUserService
    {
        string Test();
        Task<IEnumerable<UserDto>> GetUsersAsync();
        Task<ResponseUser> GetUserAsync(int userId);
        Task<IActionResult> DeleteUserAsync(int userId);
        Task<bool> CheckEmailLoginAsync(CheckEmailRequest request);
        Task<bool> CheckEmailRegistrationAsync(CheckEmailRequest request);
        Task<bool> CheckUserNameAsync(string username);
        Task<ResponseUser> RegisterUserAsync(UserRegisterRequest request);
        Task<IActionResult> RegisterOAuthUserAsync(OAuthUserRegisterRequest request);
        Task<IActionResult> RemoveUserAsync(int userId);
        Task<ResponseUser> LoginOAuth2Async(OAuth2LoginRequest request);
        Task<bool> CheckOAuth2ExistAsync(CheckOauth2ExistsReqeust request);
        Task<ResponseUser> LoginWithEmailAsync(UserLoginEmailRequest request);
        Task<ResponseUser> LoginWithUserNameAsync(UserLoginUserNameRequest request);
        Task<ResponseUser> LoginWithPhoneNumberAsync(UserLoginPhoneRequest request);
        Task<IActionResult> ChangeGeneralInfoAsync(ChangeGeneralRequest request, int userId, string JWTRole);
        Task<IActionResult> ChangePasswordAsync(ChangePasswordRequest request, int userId, string JWTRole);
        Task<IActionResult> ChangeUsernameOrPhoneNumberAsync(UserModel requestUser, int userId, string JWTRole);
        Task<IActionResult> UploadUserProfileImageAsync(UploadImageRequest imageRequest, int userId, string JWTRole);
        Task<bool> VerifyEmailAsync(string token);
        Task<IActionResult> ForgotPasswordRequestAsync(string email);
        Task<IActionResult> ResetPasswordAsync(ResetPasswordRequest request);
        Task<IActionResult> ChangeEmailRequestAsync(string email, int userid);
        Task<IActionResult> ChangeEmailAsync(string email, int userid);
        Task<IActionResult> ReLoginAsync(string password, int userId);
    }
}
