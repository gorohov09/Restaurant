using Newtonsoft.Json;
using RabbitMQ.Client;
using Restaurant.MessageBus;
using System.Text;

namespace Restaurant.Services.PaymentAPI.RabbitMQSender
{
    public class RabbitMQPaymentMessageSender : IRabbitMQPaymentMessageSender
    {
        private readonly string _hostname;
        private readonly string _password;
        private readonly string _username;
        private IConnection _connection;

        public RabbitMQPaymentMessageSender()
        {
            _hostname = "localhost";
            _password = "rmpassword";
            _username = "rmuser";
        }

        public void SendMessage(BaseMessage message, string exchangeName)
        {
            if (ConnectionExists())
            {
                using var channel = _connection.CreateModel();

                #region У нас Consumer-ы будут деклрировать все сущности
                //////Объявление Exchange
                //channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout);

                //////Объявление очереди
                //channel.QueueDeclare(queue: "orderstatusqueue", false, false, false, arguments: null);
                //channel.QueueDeclare(queue: "emailqueue", false, false, false, arguments: null);
                #endregion

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                channel.BasicPublish(exchange: exchangeName, routingKey: string.Empty, basicProperties: null, body: body);
            }
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password
                };
                _connection = factory.CreateConnection();
            }
            catch (Exception)
            {
                //log exception
            }
        }

        private bool ConnectionExists()
        {
            if (_connection != null)
            {
                return true;
            }
            CreateConnection();
            return _connection != null;
        }
    }
}
