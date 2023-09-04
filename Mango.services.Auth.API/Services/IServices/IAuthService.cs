using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.AuthAPI.Services.IServices
{
    public interface IAuthService
    {
        Task<string> Register(RegistrationRequestDto registrationRequestDto);

        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);

        Task<bool> AssignRole(string email, string roleName);

        Task<bool> SendConfirmEMailAsync(ApplicationUser user);

        Task<bool> SendForgotUsernameOrPasswordEmail(ApplicationUser user);

        Task<bool> GoogleValidatedAsync(string accessToken, string userId);

        Task<bool> FacebookValidatedAsync(string accessToken, string userId);

        
    }
}
