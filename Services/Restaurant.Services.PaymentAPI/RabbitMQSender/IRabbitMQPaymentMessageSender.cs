using Restaurant.MessageBus;

namespace Restaurant.Services.PaymentAPI.RabbitMQSender
{
    public interface IRabbitMQPaymentMessageSender
    {
        void SendMessage(BaseMessage baseMessage, string queueName);
    }
}
