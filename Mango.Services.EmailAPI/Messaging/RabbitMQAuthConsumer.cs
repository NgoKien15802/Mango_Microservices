using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQAuthConsumer : BackgroundService
    {

        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQAuthConsumer(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Password = "guest",
                UserName = "guest",
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            //định nghĩa một hàng đợi trên kênh _channel sử dụng tên hàng đợi lấy từ cấu hình.
            // để lấy message từ hàng đợi có tên đó
            _channel.QueueDeclare(_configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue")
                , false, false, false, null);

        }

        //Đây là phương thức chính của Hosted Service. Nó được triển khai từ BackgroundService và được gọi khi dịch vụ được bắt đầu.
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // dừng nếu yêu cầu hủy
            stoppingToken.ThrowIfCancellationRequested();

            //Tạo một consumer để lắng nghe các tin nhắn từ hàng đợi. có tên kênh ở trên từ appsetting
            var consumer = new EventingBasicConsumer(_channel);

            //Khi nhận được một tin nhắn, nó sẽ gọi phương thức HandleMessage.
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                String email = JsonConvert.DeserializeObject<string>(content);
                HandleMessage(email).GetAwaiter().GetResult();

                _channel.BasicAck(ea.DeliveryTag, false);
            };

                //để bắt đầu tiêu thụ tin nhắn trên hàng đợi 
            _channel.BasicConsume(_configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue"), false, consumer);

            return Task.CompletedTask;
        }

        //Nhận một địa chỉ email từ tin nhắn đến từ RabbitMQ.
        private async Task HandleMessage(string email)
        {
            // xử lý việc đăng ký người dùng. Khi đó, dịch vụ sẽ ghi log và gửi email thông báo đăng ký bên emailService
            _emailService.RegisterUserEmailAndLog(email).GetAwaiter().GetResult();
        }
    }
}
