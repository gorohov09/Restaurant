using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Restaurant.Services.OrderAPI.Messages;
using Restaurant.Services.OrderAPI.Models;
using Restaurant.Services.OrderAPI.RabbitMQSender;
using Restaurant.Services.OrderAPI.Repository;
using System.Text;
using Newtonsoft.Json;

namespace Restaurant.Services.OrderAPI.Messaging
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private readonly IOrderRepository _orderRepository;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQPaymentConsumer(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "rmuser",
                Password = "rmpassword"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "paymentqueue", false, false, false, arguments: null);
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

            _channel.BasicConsume("paymentqueue", false, consumer);

            return Task.CompletedTask;
        }

        private async Task HandleMessage(UpdatePaymentResultMessage updatePaymentResultMessage)
        {
            if (updatePaymentResultMessage is not null)
            {
                try
                {
                    await _orderRepository.UpdateOrderPaymentStatus(
                        updatePaymentResultMessage.OrderId,
                        updatePaymentResultMessage.Status);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
