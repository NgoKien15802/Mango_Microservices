using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.AuthAPI.RabbitMQSender
{
    public class RabbitMQAuthMessageSender : IRabbitMQAuthMessageSender
    {
        private readonly string _hostName;
        private readonly string _username;
        private readonly string _password;
        private IConnection _connection;



        public RabbitMQAuthMessageSender()
        {
            _hostName = "localhost";
            _password = "guest";
            _username = "guest";
        }


        public void SendMessage(object message, string queueName)
        {
            if (ConnectionExists())
            {
                // tạo kênh
                using var channel = _connection.CreateModel();

                // có kênh -> tạo queue
                channel.QueueDeclare(queueName,false,false,false,null);

                // lấy ra đc từ bên kia gửi
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                // có kênh rồi thì công bố gửi body đến queue có tên là queueName
                channel.BasicPublish(exchange: "", routingKey: queueName, null,body: body);
            }

        }


        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    Password = _password,
                    UserName = _username
                };
                // lưu lại connection
                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }


        private bool ConnectionExists()
        {
            if (_connection != null)
            {
                return true;
            }
            CreateConnection();
            return true;
        }
    }
}
