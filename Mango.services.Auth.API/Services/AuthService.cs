using Google.Apis.Auth;
using Mailjet.Client.Resources;
using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.DTO;
using Mango.Services.AuthAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace Mango.Services.AuthAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDBContext _db;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly HttpClient _facebookHttpClient;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;

        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,  AppDBContext db, IJwtTokenGenerator jwtTokenGenerator, SignInManager<ApplicationUser> signInManager, EmailService emailService,
            IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _jwtTokenGenerator = jwtTokenGenerator;
            _signInManager = signInManager;
            _emailService = emailService;
            _config = config;
            _facebookHttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com")
            };
        }

        public async Task<bool> AssignRole(string email, string roleName)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
            if (user != null)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    //create role if it does not exist
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
                await _userManager.AddToRoleAsync(user, roleName);
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDto.UserName.ToLower());
            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);
            if (user == null || isValid == false)
            {
                return new LoginResponseDto() { User = null, Token = "" };
            }
            

            // kiểm tra confirm email chưa?

            // tạo token
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            UserDto userDTO = new()
            {
                Email = user.Email,
                ID = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber
            };
            return new LoginResponseDto()
            {
                User = userDTO,
                Token = token
            };
        }

        public async Task<string> Register(RegistrationRequestDto registrationRequestDto)
        {
            if (await CheckEmailExistsAsync(registrationRequestDto.Email))
            {
                return $"An existing account is using {registrationRequestDto.Email}, email addres. Please try with another email address";
            }

            ApplicationUser user = new()
            {
                UserName = registrationRequestDto.Email.ToLower(),
                Email = registrationRequestDto.Email.ToLower(),
                NormalizedUserName = registrationRequestDto.Name.ToLower(),
                Name = registrationRequestDto.Name.ToLower(),
                PhoneNumber = registrationRequestDto.PhoneNumber.ToLower(),
            };
            try
            {
                var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
                if (result.Succeeded)
                {
                    /*var userToReturn = _db.ApplicationUsers.First(u => u.UserName == registrationRequestDto.Email);
                    await this.AssignRole(userToReturn.Email,  !string.IsNullOrEmpty(registrationRequestDto.Role) ? registrationRequestDto.Role :  SD.UserRole);*/
                    await _userManager.AddToRoleAsync(user, !string.IsNullOrEmpty(registrationRequestDto.Role) ? registrationRequestDto.Role : SD.UserRole);
                    try
                    {
                        if (await SendConfirmEMailAsync(user))
                        {
                            return "";
                        }

                        return "Failed to send email. Please contact admin";
                    }
                    catch (Exception)
                    {
                        return "Failed to send email. Please contact admin";
                    }
                    return "";
                }
                else
                {
                    return result.Errors.FirstOrDefault().Description;
                }

          
            }
            catch (Exception ex)
            {
                return ex.Message;
                throw;
            }
        }

        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _db.ApplicationUsers.AnyAsync(x => x.Email == email.ToLower());
        }
       
        public async Task<bool> SendConfirmEMailAsync(ApplicationUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{_config["ApiSettings:JwtOptions:ClientUrl"]}/{_config["Email:ConfirmEmailPath"]}?token={token}&email={user.Email}";

            var body = $"<p>Hello: {user.Name}</p>" +
                "<p>Please confirm your email address by clicking on the following link.</p>" +
                $"<p><a href=\"{url}\">Click here</a></p>" +
                "<p>Thank you,</p>" +
                $"<br>{_config["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto(user.Email, "Confirm your email", body);

            return await _emailService.SendEmailAsync(emailSend);
        }

        public async Task<bool> SendForgotUsernameOrPasswordEmail(ApplicationUser user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{_config["ApiSettings:JwtOptions:ClientUrl"]}/{_config["Email:ResetPasswordPath"]}?token={token}&email={user.Email}";

            var body = $"<p>Hello: {user.Name}</p>" +
               $"<p>Username: {user.UserName}.</p>" +
               "<p>In order to reset your password, please click on the following link.</p>" +
               $"<p><a href=\"{url}\">Click here</a></p>" +
               "<p>Thank you,</p>" +
               $"<br>{_config["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto(user.Email, "Forgot username or password", body);

            return await _emailService.SendEmailAsync(emailSend);
        }

        public async Task<bool> GoogleValidatedAsync(string accessToken, string userId)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(accessToken);

            if (!payload.Audience.Equals(_config["Google:ClientId"]))
            {
                return false;
            }

            if (!payload.Issuer.Equals("accounts.google.com") && !payload.Issuer.Equals("https://accounts.google.com"))
            {
                return false;
            }

            if (payload.ExpirationTimeSeconds == null)
            {
                return false;
            }

            DateTime now = DateTime.Now.ToUniversalTime();
            DateTime expiration = DateTimeOffset.FromUnixTimeSeconds((long)payload.ExpirationTimeSeconds).DateTime;
            if (now > expiration)
            {
                return false;
            }

            if (!payload.Subject.Equals(userId))
            {
                return false;
            }

            return true;
        }

        /*{
          "email": "ngotrungkien@gmail.com",
          "name": "Ngô Kiên",
          "accessToken": "EAAEmC5nZCtK4BO6AGUgTbsfBGNT0AA6GlhFZCo4n71IKV7291oQHGpX6QP9LhPXCID7GR6jltBNLE8Q2dCw8Dd0tWZCZBwDYlEFnYIvHzepXJGlZCcjexqPtIJGO4YUZCXre071y328uiXn73pFpRnR1pZBKNuDSzhUZAvcS32y6bZBRcvcs4EqjkxcIt4R0cZCZAHuV3EwXyWRQ00JHsmvZAiqXrOPtFqoZD",
          "userId": "1027020745093989",
          "provider": "Facebook"
        }*/
        public async Task<bool> FacebookValidatedAsync(string accessToken, string userId)
            {
                var facebookKeys = _config["Facebook:AppId"] + "|" + _config["Facebook:AppSecret"];
                var fbResult = await _facebookHttpClient.GetFromJsonAsync<FacebookResultDto>($"debug_token?input_token={accessToken}&access_token={facebookKeys}");

                if (fbResult == null || fbResult.Data.Is_Valid == false || !fbResult.Data.User_Id.Equals(userId))
                {
                    return false;
                }

                return true;
            }
        }
}
