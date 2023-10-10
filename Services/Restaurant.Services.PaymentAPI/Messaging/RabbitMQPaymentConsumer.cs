using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json;
using PaymentProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Restaurant.Services.PaymentAPI.Messages;
using Restaurant.Services.PaymentAPI.RabbitMQSender;
using System.Text;

namespace Restaurant.Services.PaymentAPI.Messaging
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IRabbitMQPaymentMessageSender _rabbitMQPaymentMessageSender;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQPaymentConsumer(IPaymentProcessor paymentProcessor, IRabbitMQPaymentMessageSender rabbitMQPaymentMessageSender)
        {
            _paymentProcessor = paymentProcessor;
            _rabbitMQPaymentMessageSender = rabbitMQPaymentMessageSender;

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "rmuser",
                Password = "rmpassword"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "orderpaymentprocesstopic", false, false, false, arguments: null);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                var paymentRequestMessage = JsonConvert.DeserializeObject<PaymentRequestMessage>(content);
                await HandleMessage(paymentRequestMessage);

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume("orderpaymentprocesstopic", false, consumer);

            return Task.CompletedTask;
        }

        private async Task HandleMessage(PaymentRequestMessage paymentRequestMessage)
        {
            var result = _paymentProcessor.PaymentProccesor();

            var updatePaymentResultMessage = new UpdatePaymentResultMessage
            {
                OrderId = paymentRequestMessage.OrderId,
                Status = result
            };

            try
            {
                _rabbitMQPaymentMessageSender.SendMessage(updatePaymentResultMessage, "paymentqueue");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
