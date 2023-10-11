using Restaurant.MessageBus;

namespace Restaurant.Services.PaymentAPI.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdatePaymentResultMessage : BaseMessage
    {
        public int OrderId { get; set; }
        public bool Status { get; set; }

        public string Email { get; set; }
    }

    //Данное сообщение должно попадать в сервис - OrderAPI, для перевода статуса заказа в - ОПЛАЧЕНО
    //А также в сервис - Email, для рассылки уведомлений пользователю, что его заказ оплачен
    //Выходит ситуация, когда 1 consumer и 2
}
