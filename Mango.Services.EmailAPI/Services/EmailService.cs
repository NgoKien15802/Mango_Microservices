using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Mango.Services.EmailAPI.Services
{
    public class EmailService : IEmailService
    {
        // để tương tác với csdl
        private DbContextOptions<AppDBContext> _dbOptions;

        
        public EmailService(DbContextOptions<AppDBContext> dbOptions)
        {
            this._dbOptions = dbOptions;
        }

        //Tạo ra một chuỗi message chứa thông tin về giỏ hàng (cartDto) dưới dạng HTML.
        public async Task EmailCartAndLog(CartDto cartDto)
        {
            StringBuilder message = new StringBuilder();

            message.AppendLine("<br/>Cart Email Requested ");
            message.AppendLine("<br/>Total " + cartDto.CartHeader.CartTotal);
            message.Append("<br/>");
            message.Append("<ul>");
            foreach (var item in cartDto.CartDetails)
            {
                message.Append("<li>");
                message.Append(item.Product.Name + " x " + item.Count);
                message.Append("</li>");
            }
            message.Append("</ul>");

            // để ghi log và gửi email.
            await LogAndEmail(message.ToString(), cartDto.CartHeader.Email);
        }

       /* public async Task LogOrderPlaced(RewardsMessage rewardsDto)
        {
            string message = "New Order Placed. <br/> Order ID : " + rewardsDto.OrderId;
            await LogAndEmail(message, "dotnetmastery@gmail.com");
        }
*/
        public async Task RegisterUserEmailAndLog(string email)
        {
            //Tạo ra một thông điệp thông báo về việc đăng ký người dùng thành công.
            string message = "User Registeration Successful. <br/> Email : " + email;
            await LogAndEmail(message, email);
        }

        private async Task<bool> LogAndEmail(string message, string email)
        {
            try
            {
                EmailLogger emailLog = new()
                {
                    Email = email,
                    EmailSent = DateTime.Now,
                    Message = message
                };
                //Sử dụng đối tượng _db để thực hiện các thao tác với cơ sở dữ liệu. AppDBContext được khởi tạo bằng cách sử dụng _dbOptions đã được cấu hình trước đó.
                await using var _db = new AppDBContext(_dbOptions);
                await _db.EmailLoggers.AddAsync(emailLog);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
