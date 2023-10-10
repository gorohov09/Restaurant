using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Restaurant.Services.OrderAPI.Models;
using Restaurant.Services.OrderAPI.Repository;
using System.Text;
using Newtonsoft.Json;
using Restaurant.Services.OrderAPI.Messages;
using Restaurant.Services.OrderAPI.RabbitMQSender;

namespace Restaurant.Services.OrderAPI.Messaging
{
    public class RabbitMQCheckoutConsumer : BackgroundService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IRabbitMQOrderMessageSender _rabbitMQOrderMessageSender;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQCheckoutConsumer(IOrderRepository orderRepository, IRabbitMQOrderMessageSender rabbitMQOrderMessageSender)
        {
            _orderRepository = orderRepository;
            _rabbitMQOrderMessageSender = rabbitMQOrderMessageSender;

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "rmuser",
                Password = "rmpassword"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "checkoutqueue", false, false, false, arguments: null);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                var checkoutHeaderDto = JsonConvert.DeserializeObject<CheckoutHeaderDto>(content);
                await HandleMessage(checkoutHeaderDto);

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume("checkoutqueue", false, consumer);

            return Task.CompletedTask;
        }

        private async Task HandleMessage(CheckoutHeaderDto checkoutHeaderDto)
        {
            OrderHeader orderHeader = new()
            {
                UserId = checkoutHeaderDto.UserId,
                FirstName = checkoutHeaderDto.FirstName,
                LastName = checkoutHeaderDto.LastName,
                OrderDetails = new List<OrderDetails>(),
                CardNumber = checkoutHeaderDto.CardNumber,
                CouponCode = checkoutHeaderDto.CouponCode,
                CVV = checkoutHeaderDto.CVV,
                DiscountTotal = checkoutHeaderDto.DiscountTotal,
                Email = checkoutHeaderDto.Email,
                ExpiryMonthYear = checkoutHeaderDto.ExpiryMonthYear,
                OrderTime = DateTime.Now,
                OrderTotal = checkoutHeaderDto.OrderTotal,
                PaymentStatus = false,
                Phone = checkoutHeaderDto.Phone,
                PickupDateTime = checkoutHeaderDto.PickupDateTime
            };

            foreach (var detailList in checkoutHeaderDto.CartDetails)
            {
                OrderDetails orderDetails = new()
                {
                    ProductId = detailList.ProductId,
                    ProductName = detailList.Product.Name,
                    Price = detailList.Product.Price,
                    Count = detailList.Count
                };

                orderHeader.CartTotalItems += detailList.Count;
                orderHeader.OrderDetails.Add(orderDetails);
            }

            var result = await _orderRepository.AddOrder(orderHeader);
            if (result)
            {
                var paymentRequestMessage = new PaymentRequestMessage()
                {
                    Name = orderHeader.FirstName + " " + orderHeader.LastName,
                    CardNumber = orderHeader.CardNumber,
                    CVV = orderHeader.CVV,
                    ExpiryMonthYear = orderHeader.ExpiryMonthYear,
                    OrderId = orderHeader.OrderHeaderId,
                    OrderTotal = orderHeader.OrderTotal
                };

                try
                {
                    _rabbitMQOrderMessageSender.SendMessage(paymentRequestMessage, "orderpaymentprocesstopic");
                }
                catch (Exception e)
                {
                    //Регистрация исключения, записывание в логи и т.д.
                    throw e;
                }
            }
        }
    }
}
