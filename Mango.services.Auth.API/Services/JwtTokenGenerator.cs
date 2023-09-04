using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Services.IServices;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Mango.Services.AuthAPI.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtOption _jwtOptions;

        //IOptions: giúp mã nguồn của JwtTokenGenerator trở nên độc lập với cài đặt cụ thể của JWT.
        public JwtTokenGenerator(IOptions<JwtOption> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }
        public string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // ký bằng khóa bí mật
            var key = Encoding.ASCII.GetBytes(_jwtOptions.Secret);

            // để xác thực, ủy quyền theo chuẩn cấu trúc của jwt 
            var claimList = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email,applicationUser?.Email),
                new Claim(JwtRegisteredClaimNames.Sub,applicationUser?.Id),
                new Claim(JwtRegisteredClaimNames.Name,applicationUser?.UserName)
            };

            // add roles
            claimList.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role))); 

            //được sử dụng để cấu hình thông tin cho JWT.
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = _jwtOptions.Audience,
                Issuer = _jwtOptions.Issuer,
                Subject = new ClaimsIdentity(claimList),
                Expires = DateTime.UtcNow.AddDays(1),
                //Thông tin về cách JWT sẽ được ký. Ở đây, nó sử dụng HmacSha256Signature và sử dụng khóa bí mật
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            // trả về chuỗi jwt
            return tokenHandler.WriteToken(token);
        }
    }
}
