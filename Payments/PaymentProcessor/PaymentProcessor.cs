namespace PaymentProcessor
{
    public class PaymentProcessor : IPaymentProcessor
    {
        public bool PaymentProccesor()
        {
            //Можно реализовать логику оплаты, но по умолчанию true
            return true;
        }
    }
}
