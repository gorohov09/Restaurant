using Restaurant.MessageBus;

namespace Restaurant.Services.ShoppingCartAPI.RabbitMQSender
{
    public interface IRabbitMQCartMessageSender
    {
        void SendMessage(BaseMessage baseMessage, string queueName);
    }
}
