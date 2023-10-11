using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using Restaurant.Services.Email.Messages;

namespace Restaurant.Services.Email.Messaging
{
    public class RabbitMQEmailConsumer : BackgroundService
    {
        private readonly ILogger<RabbitMQEmailConsumer> _logger;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQEmailConsumer(ILogger<RabbitMQEmailConsumer> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "rmuser",
                Password = "rmpassword"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "paymentexchange", type: ExchangeType.Fanout);

            _channel.QueueDeclare(queue: "emailqueue", false, false, false, arguments: null);

            _channel.QueueBind(queue: "emailqueue",
                  exchange: "paymentexchange",
                  routingKey: string.Empty);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                var updatePaymentResultMessage = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(content);
                await HandleMessage(updatePaymentResultMessage);

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume("emailqueue", false, consumer);

            return Task.CompletedTask;
        }

        private async Task HandleMessage(UpdatePaymentResultMessage updatePaymentResultMessage)
        {
            //Тут должна быть логика отправления сообщения по почте, но условимся логированием операции

            if (updatePaymentResultMessage is not null)
            {
                _logger.LogInformation("Отправление сообщение на {0}", updatePaymentResultMessage.Email);
            }
        }
    }
}
